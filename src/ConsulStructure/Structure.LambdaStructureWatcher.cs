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
            readonly Action<KeyValuePair<string,byte[]>> _instance;
            readonly Options _options;
            readonly Func<Task> _watcherDisposer;

            public LambdaStructureWatcher(Action<KeyValuePair<string,byte[]>> instance, Options options)
            {
                _instance = instance;
                _options = options;

                _watcherDisposer = options.Factories.Watcher(ApplyConfiguration, options);
            }

            void ApplyConfiguration(KeyValuePair<string, byte[]> kv)
            {
                _instance(kv);
                _options.Events.KeyValueAssigned(kv.Key, kv.Value);
            }

            public Task Dispose()
            {
                return _watcherDisposer();
            }
        }
    }
}