using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ConsulStructure
{
    internal partial class Structure
    {
        class LambdaStructureWatcher
        {
            readonly Action<IEnumerable<KeyValuePair<string, byte[]>>> _instance;
            readonly Options _options;
            readonly Func<Task> _watcherDisposer;

            public LambdaStructureWatcher(Action<IEnumerable<KeyValuePair<string, byte[]>>> instance, Options options)
            {
                _instance = instance;
                _options = options;

                _watcherDisposer = options.Factories.Watcher(ApplyConfiguration, options);
            }

            void ApplyConfiguration(IEnumerable<KeyValuePair<string, byte[]>> keyValuePairs)
            {
                _instance(keyValuePairs);
                _options.Events.KeyValuesAssigned(
                    keyValuePairs.Select(kv=>new KeyValuePair<string,object>(kv.Key,kv.Value))
                                 .ToList());
            }

            public Task Dispose()
            {
                return _watcherDisposer();
            }
        }
    }
}