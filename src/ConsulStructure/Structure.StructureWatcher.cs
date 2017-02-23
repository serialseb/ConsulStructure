using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace ConsulStructure
{
    internal partial class Structure
    {
        class StructureWatcher<T>
        {
            readonly T _instance;
            readonly Options _options;
            readonly Func<Task> _watcherDisposer;

            public StructureWatcher(T instance, Options options)
            {
                _instance = instance;
                _options = options;

                var converters = new Dictionary<Type, Expression>
                {
                    {typeof(string), Lambda(options.Converters.String)},
                    {typeof(int), Lambda(options.Converters.Int32)},
                    {typeof(bool), Lambda(options.Converters.Bool)}
                };

                _propertySetters = BuildPropertyGraph(typeof(T), options.Prefix, converters,
                        options.Events.KeyDiscovered)
                    .ToDictionary(entry => entry.Key, entry => entry.Value, StringComparer.OrdinalIgnoreCase);

                _watcherDisposer = options.Factories.Watcher(ApplyConfiguration, options);
            }

            readonly Dictionary<string, Func<T, byte[], object>> _propertySetters;

            void ApplyConfiguration(KeyValuePair<string, byte[]> kv)
            {
                Func<T, byte[], object> converter;
                if (_propertySetters.TryGetValue(kv.Key, out converter))
                {
                    var conversionResult = converter(_instance, kv.Value);
                    _options.Events.KeyValueAssigned(kv.Key, conversionResult);
                }
                else
                {
                    _options.Events.KeyValueIgnored(kv.Key, kv.Value);
                }
            }

            public Task Dispose()
            {
                return _watcherDisposer();
            }

            static Expression<Func<byte[], TReturn>> Lambda<TReturn>(Func<byte[], TReturn> lambda)
            {
                return LambdaExp(bytes => lambda(bytes));
            }

            static Expression<Func<byte[], TReturn>> LambdaExp<TReturn>(
                Expression<Func<byte[], TReturn>> lambda) => lambda;

            static IEnumerable<KeyValuePair<string, Func<T, byte[], object>>> BuildPropertyGraph(
                Type current,
                string baseKey,
                IReadOnlyDictionary<Type, Expression> converters,
                Events.KeyDiscoveredDelegate log,
                ParameterExpression structureParam = null,
                Expression currentMemberExpr = null)
            {
                structureParam = structureParam ?? Expression.Parameter(typeof(T), "structure");
                currentMemberExpr = currentMemberExpr ?? structureParam;
                foreach (var property in current.GetProperties())
                {
                    var propertyAccess = Expression.MakeMemberAccess(currentMemberExpr, property);
                    var currentKey = baseKey + "/" + property.Name;
                    Expression converterExpression;
                    if (converters.TryGetValue(property.PropertyType, out converterExpression))
                    {
                        var bytesParam = Expression.Parameter(typeof(byte[]));

                        var converterInvoker = Expression.Invoke(converterExpression, bytesParam);

                        var propertySetter = Expression.Assign(propertyAccess, converterInvoker);
                        var returnValue = Expression.Convert(propertySetter, typeof(object));

                        var finalLambda =
                            Expression.Lambda<Func<T, byte[], object>>(returnValue, structureParam, bytesParam);

                        yield return new KeyValuePair<string, Func<T, byte[], object>>(
                            currentKey,
                            finalLambda.Compile());
                        log(currentKey, property);
                    }
                    else
                    {
                        foreach (var assigner in BuildPropertyGraph(
                            property.PropertyType,
                            currentKey,
                            converters,
                            log,
                            structureParam,
                            propertyAccess))
                            yield return assigner;
                    }
                }
            }
        }
    }
}