using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using ConsulStructure;
using Microsoft.Owin;
using Microsoft.Owin.Builder;
using Microsoft.Owin.Testing;
using Owin;
using AppFunc = System.Func<Microsoft.Owin.IOwinContext, System.Threading.Tasks.Task>;

namespace Tests.Infrastructure
{
    public abstract class AbstractHttpMemoryTest : AbstractStructureTest
    {
        static HttpClient HttpClientSimulator(Structure.Options options, ConsulSimulator simulator, Func<AppFunc, AppFunc> interceptor)
        {
            Func<IOwinContext,Task> invoker = simulator.Invoke;
            if (interceptor != null)
                invoker = interceptor(invoker);

            var appFunc = new AppBuilder()
                .Use((env, next) => invoker(env))
                .Build();

            return new HttpClient(new OwinClientHandler(appFunc)) {BaseAddress = options.ConsulUri};
        }

        protected readonly AwaitableQueue<IEnumerable<KeyValuePair<string, object>>> KeyValuesAssigned =
            new AwaitableQueue<IEnumerable<KeyValuePair<string, object>>>();

        protected readonly AwaitableQueue<Exception> HttpErrors = new AwaitableQueue<Exception>();

        protected readonly AwaitableQueue<Tuple<HttpRequestMessage, HttpResponseMessage, TimeSpan>> HttpSuccesses =
            new AwaitableQueue<Tuple<HttpRequestMessage, HttpResponseMessage, TimeSpan>>();

        protected readonly ConsulSimulator ConsulSimulator = new ConsulSimulator();

        internal Structure.Options TestOptions(
            Func<AppFunc, AppFunc> responseMiddleware = null,
            Action<Structure.Options> more = null)
        {
            var testOptions = new Structure.Options
            {
                Factories =
                {
                    HttpClient = options => HttpClientSimulator(options, ConsulSimulator, responseMiddleware)
                },
                Events =
                {
                    KeyDiscovered = (keypath, property) => DiscoveredKeys[keypath] = property,
                    KeyValuesesIgnored = (kvs) =>
                    {
                        foreach (var kv in kvs) IgnoredKeys[kv.Key] = kv.Value;
                    },
                    KeyValuesAssigned = KeyValuesAssigned.Enqueue,
                    HttpError = HttpErrors.Enqueue,
                    HttpSuccess = (request, response, duration) =>
                    {
                        HttpSuccesses.Enqueue(Tuple.Create(request, response, duration));
                    }
                }
            };
            more?.Invoke(testOptions);
            return testOptions;
        }
    }
}