using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;

namespace ConsulStructure.Tests.Infrastructure
{
    public abstract class AbstractStructureTest
    {
        protected readonly Dictionary<string, PropertyInfo> DiscoveredKeys
            = new Dictionary<string, PropertyInfo>(StringComparer.OrdinalIgnoreCase);


        protected readonly Dictionary<string, byte[]> IgnoredKeys
            = new Dictionary<string, byte[]>(StringComparer.OrdinalIgnoreCase);

        internal Structure.Options TestOptions<T>(string json = "[]")
        {
            return new Structure.Options
            {
                Factories =
                {
                    Watcher = (changes, options) =>
                    {
                        foreach (var kv in options.Converters.KeyParser(json))
                            changes(kv);
                        return () => Task.CompletedTask;
                    }
                },
                Events =
                {
                    KeyDiscovered = (keypath, property) => DiscoveredKeys[keypath] = property,
                    KeyValueIgnored = (keypath, value) => IgnoredKeys[keypath] = value
                }
            };
        }
    }
}