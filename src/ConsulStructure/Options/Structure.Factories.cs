using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace ConsulStructure
{
    partial class Structure
    {
        internal class Factories
        {
            public delegate Func<Task> WatcherDelegate(Action<KeyValuePair<string, byte[]>> onChanges, Options options);

            public WatcherDelegate Watcher { get; set; } = (onChanges, options) => new BlockingHttpWatcher(onChanges, options).Stop;

            public Func<Options, HttpClient> HttpClient { get; set; } =
                options => new HttpClient
                {
                    BaseAddress = options.ConsulUri,
                    Timeout = options.Timeout
                };
        }
    }
}