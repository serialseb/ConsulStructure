using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using ConsulStructure.Tests.Examples;
using Microsoft.Owin;
using Microsoft.Owin.Builder;
using Microsoft.Owin.Testing;
using Owin;
using Shouldly;
using Xunit;

namespace ConsulStructure.Tests
{
    public class successful_http_operation : AbstractHttpMemoryTest
    {
        [Fact]
        public async Task can_shut_down()
        {
        }

        [Fact]
        public async Task key_is_assigned_repeatedly()
        {
            var config = new SimpleProperties();
            var testOptions = TestOptions<SimpleProperties>(
                async r =>
                {
                    var idxValue = r.Request.Query["index"];
                    var idx = string.IsNullOrEmpty(idxValue) ? 0 : int.Parse(idxValue);
                    idx++;
                    await Task.Delay(TimeSpan.FromSeconds(1));
                    await r.Response(200, idx, $"[ {KV("/keystring", $"http{idx}")} ]");
                });

            Structure updater = null;
            int assignments = 0;
            testOptions.Events.KeyValueAssigned = (path, value) =>
            {
                if (++assignments >= 2)
                {
                    updater.Stop();
                    KeyAssigned.SetResult(true);

                }
            };

            updater = Structure.Start(config, testOptions);
            var result = await Task.WhenAny(KeyAssigned.Task, Task.Delay(TimeSpan.FromMinutes(1)));

            assignments.ShouldBe(2);
            result.ShouldBe(KeyAssigned.Task, "Assignment timed out");
            config.KeyString.ShouldBe("http2");
            await updater.Stop();
        }
        [Fact]
        public async Task key_is_assigned_once()
        {
            var config = new SimpleProperties();
            var updater = Structure.Start(config, TestOptions<SimpleProperties>(
                r => r.Response(200, 1, $"[ {KV("/keystring", "http")} ]")
            ));
            var result = await Task.WhenAny(KeyAssigned.Task, Task.Delay(TimeSpan.FromMinutes(1)));
            result.ShouldBe(KeyAssigned.Task, "Assignment timed out");
            config.KeyString.ShouldBe("http");
            await updater.Stop();
        }
    }

    public static class OwinExtensions
    {
        public static Task Response(this IOwinContext env, int statusCode, int consulIndex = -1, string body = "")
        {
            env.Response.StatusCode = statusCode;
            if (consulIndex >= 0)
                env.Response.Headers["X-Consul-Index"] = consulIndex.ToString();

            return body == null ? env.Response.Body.FlushAsync() : env.Response.WriteAsync(body);
        }
    }

    public abstract class AbstractHttpMemoryTest : AbstractStructureTest
    {
        static HttpClient Response(Structure.Options options, Func<IOwinContext, Task> response)
        {
            var appFunc = new AppBuilder()
                .Use((env, next) => response(env))
                .Build();
            return new HttpClient(new OwinClientHandler(appFunc)) {BaseAddress = options.ConsulUri};
        }

        protected TaskCompletionSource<bool> KeyAssigned = new TaskCompletionSource<bool>();

        internal Structure.Options TestOptions<T>(Func<IOwinContext, Task> response)
        {
            return new Structure.Options
            {
                Factories =
                {
                    HttpClient = options => Response(options, response)
                },
                Events =
                {
                    KeyDiscovered = (keypath, property) => DiscoveredKeys[keypath] = property,
                    KeyValueIgnored = (keypath, value) => IgnoredKeys[keypath] = value,
                    KeyValueAssigned = (keypath, value) => KeyAssigned.SetResult(true)
                }
            };
        }
    }

    public abstract class AbstractStructureTest
    {
        protected string KV(string key, string value)
        {
            return $@"{{ ""Key"": ""{key}"", ""Value"": ""{Convert.ToBase64String(Encoding.UTF8.GetBytes(value))}"" }}";
        }

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