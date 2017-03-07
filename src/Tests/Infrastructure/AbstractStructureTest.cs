using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using ConsulStructure;

namespace Tests.Infrastructure
{
  public abstract class AbstractStructureTest
  {
    protected readonly Dictionary<string, PropertyInfo> DiscoveredKeys
      = new Dictionary<string, PropertyInfo>(StringComparer.OrdinalIgnoreCase);


    protected readonly Dictionary<string, byte[]> IgnoredKeys
      = new Dictionary<string, byte[]>(StringComparer.OrdinalIgnoreCase);

    protected readonly AwaitableQueue<IEnumerable<KeyValuePair<string, object>>> KeyValuesAssigned =
      new AwaitableQueue<IEnumerable<KeyValuePair<string, object>>>();

    protected readonly AwaitableQueue<IEnumerable<KeyValuePair<string, byte[]>>> KeyValuesIgnored =
      new AwaitableQueue<IEnumerable<KeyValuePair<string, byte[]>>>();

    internal Structure.Options TestOptions<T>(string json = "[]")
    {
      return new Structure.Options
      {
        Factories =
        {
          Watcher = (changes, options) =>
          {
            changes(options.Converters.KeyParser(json));
            return () => Task.CompletedTask;
          }
        },
        Events =
        {
          KeyDiscovered = (keypath, property) => DiscoveredKeys[keypath] = property,
          KeyValuesesIgnored = (kvs) =>
          {
            KeyValuesIgnored.Enqueue(kvs);
            foreach (var kv in kvs) IgnoredKeys[kv.Key] = kv.Value;
          },
          KeyValuesAssigned = KeyValuesAssigned.Enqueue
        }
      };
    }
  }
}