using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Owin;

namespace Tests.Infrastructure
{
  public class ConsulSimulator
  {
    int currentIndex = 0;
    AutoResetAwaitable onWrite = new AutoResetAwaitable();
    readonly Dictionary<int, Tuple<string, string>> kvStore = new Dictionary<int, Tuple<string, string>>();

    public async Task PutKeyWithDelay(string key, string value, TimeSpan delay)
    {
      await Task.Delay(delay);
      PutKey(key, value);
    }

    public void PutKey(string key, string value)
    {
      lock (kvStore)
      {
        kvStore[++currentIndex] = Tuple.Create(key, value);
        PruneOldValues(key);
      }
      onWrite.Signal();
    }

    void PruneOldValues(string key)
    {
      for (var i = 0; i < currentIndex; i++)
      {
        if (kvStore.ContainsKey(i) && kvStore[i].Item1 == key)
          kvStore.Remove(i);
      }
    }

    public async Task Invoke(IOwinContext env)
    {
      int currentIndexCopy;

      int requestedIndex = int.TryParse(env.Request.Query["index"], out requestedIndex) ? requestedIndex : 0;
      while ((currentIndexCopy = currentIndex) <= requestedIndex)
      {
        await onWrite.WaitOne();
      }

      Dictionary<string, string> data;
      lock (kvStore)
      {
        data = kvStore.Where(kv => kv.Key <= currentIndexCopy)
                      .ToDictionary(kv => kv.Value.Item1, kv => kv.Value.Item2);
      }

      await env.Response(200, currentIndexCopy, data.ToJson());
    }
  }
}