using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Json;

namespace ConsulStructure
{
    internal partial class Structure
    {
        internal static Structure Start<T>(T instance, Options options = null)
        {
            options = options ?? new Options();
            return new Structure(new StructureWatcher<T>(instance, options).Dispose);
        }
    }

    internal partial class Structure
    {
        readonly Func<Task> _stopper;

        Structure(Func<Task> stopper)
        {
            _stopper = stopper;
        }

        public Task Stop()
        {
            return _stopper();
        }

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

                _propertySetters = BuildPropertyGraph(typeof(T), options.Prefix, converters, options.Events.KeyDiscovered)
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

            static IEnumerable<KeyValuePair<string, Func<T, byte[],object>>> BuildPropertyGraph(
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

                        yield return new KeyValuePair<string, Func<T, byte[],object>>(
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

        static class HttpClientExtensions
        {
            internal static async Task<int> WaitForChanges(
                HttpClient client,
                string prefix,
                Action<KeyValuePair<string, byte[]>> result,
                CancellationToken cancel,
                TimeSpan timeout,
                int existingIndex,
                Func<string,IEnumerable<KeyValuePair<string,byte[]>>> parser)
            {
                var response = await client.GetAsync(
                    CreateKvPrefixUri(prefix, timeout, existingIndex), HttpCompletionOption.ResponseContentRead,
                    cancel);
                var indexResponseHeader = response.Headers.GetValues("X-Consul-Index").LastOrDefault();

                int newIndex;
                if (indexResponseHeader == null
                    || !int.TryParse(indexResponseHeader, out newIndex)
                    || newIndex <= existingIndex)
                    return existingIndex;

                foreach (var key in parser(await response.Content.ReadAsStringAsync()))
                    result(key);
                return newIndex;
            }

            static Uri CreateKvPrefixUri(string prefix, TimeSpan wait, int index)
            {
                var indexParam = index == 0 ? "" : $"&existingIndex={index}";
                return new Uri(
                    $"/v1/kv/{EnsureUnslashed(prefix)}?wait={wait.TotalSeconds}s{indexParam}&existingIndex=existingIndex&recurse",
                    UriKind.Relative);
            }

            static string EnsureUnslashed(string prefix)
            {
                return prefix.StartsWith("/") ? prefix.Substring(1) : prefix;
            }
        }

        class BlockingHttpWatcher
        {
            readonly Action<KeyValuePair<string, byte[]>> _configurationReceived;
            readonly Options _options;
            readonly HttpClient _client = new HttpClient();
            readonly CancellationTokenSource _dispose = new CancellationTokenSource();
            Task _loop;

            public BlockingHttpWatcher(Action<KeyValuePair<string, byte[]>> configurationReceived, Options options)
            {
                _configurationReceived = configurationReceived;
                _options = options;
                _client.BaseAddress = options.ConsulUri;

                _loop = Run();
            }

            async Task Run()
            {
                var idx = 0;
                while (_dispose.IsCancellationRequested == false)
                {
                    idx = await HttpClientExtensions.WaitForChanges(
                        _client,
                        _options.Prefix,
                        _configurationReceived,
                        _dispose.Token,
                        _options.Timeout,
                        idx,
                        _options.Converters.KeyParser);
                }
            }

            public async Task Stop()
            {
                _dispose.Cancel();
                await _loop;
                _client.Dispose();
            }
        }
    }


    public class ConfigurationObject
    {
    }

    partial class Structure
    {
        internal class Options
        {
            public Uri ConsulUri { get; set; } = new Uri("http://localhost:8500");

            public TimeSpan Timeout { get; set; } = TimeSpan.FromMinutes(10);
            public string Prefix { get; set; } = "";

            public Converters Converters { get; } = new Converters();
            public Factories Factories { get; } = new Factories();
            public Events Events { get; } = new Events();
        }

        internal class Events
        {
            internal delegate void KeyDiscoveredDelegate(string keyPath, PropertyInfo property);

            internal delegate void KeyValueIgnoredDelegate(string keyPath, byte[] value);
            internal delegate void KeyValueAssignedDelegate(string keyPath, object value);

            internal KeyDiscoveredDelegate KeyDiscovered {get;set;} = (key, property) => { };
            internal KeyValueIgnoredDelegate KeyValueIgnored { get; set; } = (path, value) => { };
            internal KeyValueAssignedDelegate KeyValueAssigned { get; set; } = (path, value) => { };
        }

        internal delegate Func<Task> Watcher(Action<KeyValuePair<string,byte[]>> onChanges, Options options);

        internal class Factories
        {
            public Watcher Watcher = (onChanges, options) => new BlockingHttpWatcher(onChanges, options).Stop;

            public Func<Options, HttpClient> HttpClient { get; set; } =
                options => new HttpClient
                {
                    BaseAddress = options.ConsulUri,
                    Timeout = options.Timeout
                };
        }

        internal class Converters
        {
            public Func<byte[], string> String { get; set; } = bytes => Encoding.UTF8.GetString(bytes);
            public Func<byte[], int> Int32 { get; set; } = bytes => int.Parse(Encoding.UTF8.GetString(bytes));
            public Func<byte[], bool> Bool { get; set; } = bytes => bool.Parse(Encoding.UTF8.GetString(bytes));

            public Func<string, IEnumerable<KeyValuePair<string, byte[]>>> KeyParser { get; set; } = ParseJson;

            static IEnumerable<KeyValuePair<string, byte[]>> ParseJson(string content)
            {
                return SimpleJson.DeserializeObject<KV[]>(content)
                    .Select(kv => new KeyValuePair<string, byte[]>(kv.Key, Convert.FromBase64String(kv.Value)));
            }
        }
    }

    internal partial class Structure
    {
        static IEnumerable<KV> Read(string json)
        {
            return SimpleJson.DeserializeObject<KV[]>(json);
        }

        internal class KV
        {
            public string Key { get; set; }
            public string Value { get; set; }
        }
    }
}