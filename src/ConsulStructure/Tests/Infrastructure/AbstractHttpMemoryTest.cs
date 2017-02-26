using System;
using System.Net.Http;
using Microsoft.Owin;
using Microsoft.Owin.Builder;
using Microsoft.Owin.Testing;
using Owin;
using AppFunc = System.Func<Microsoft.Owin.IOwinContext, System.Threading.Tasks.Task>;

namespace ConsulStructure.Tests.Infrastructure
{
    public abstract class AbstractHttpMemoryTest : AbstractStructureTest
    {
        static HttpClient HttpClientSimulator(Structure.Options options, ConsulSimulator simulator, Func<AppFunc, AppFunc> response)
        {
            var appFunc = new AppBuilder()
                .Use((env, next) => simulator.Invoke(env));
            if (response != null)
                appFunc = appFunc.Use(response);
            return new HttpClient(new OwinClientHandler(appFunc.Build())) {BaseAddress = options.ConsulUri};
        }

        protected readonly AutoResetAwaitable KeyAssigned = new AutoResetAwaitable();
        protected readonly AutoResetAwaitable HttpEvent = new AutoResetAwaitable();
        protected readonly ConsulSimulator ConsulSimulator = new ConsulSimulator();

        internal Structure.Options TestOptions(Func<AppFunc, AppFunc> responseMiddleware = null)
        {
            return new Structure.Options
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
                    KeyValuesAssigned = (kv) =>
                    {
                        KeyAssigned.Signal();
                    },
                    HttpError = exception =>
                    {
                        LastException = exception;
                        HttpEvent.Signal();
                    },
                    HttpSuccess = (request, response, duration) =>
                    {
                        LastRequest = request;
                        LastResponse = response;
                        LastDuration = duration;
                        HttpEvent.Signal();
                    }
                }
            };
        }

        protected TimeSpan LastDuration { get; private set; }
        protected HttpResponseMessage LastResponse { get; private set; }
        protected HttpRequestMessage LastRequest { get; private set; }

        protected Exception LastException { get; set; }
    }
}