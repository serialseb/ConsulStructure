using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace ConsulStructure
{
    internal partial class Structure
    {
        static class Http
        {
            internal static async Task<int> WaitForChanges(
                Func<HttpRequestMessage,Task<HttpResponseMessage>> sender,
                string prefix,
                Action<IEnumerable<KeyValuePair<string, byte[]>>> result,
                TimeSpan timeout,
                int existingIndex,
                Func<string, IEnumerable<KeyValuePair<string, byte[]>>> parser)
            {
                var request = new HttpRequestMessage()
                {
                    Method = HttpMethod.Get,
                    RequestUri = CreateKvPrefixUri(prefix, timeout, existingIndex),

                };
                var response = await sender(request);

                var indexResponseHeader = response.Headers.GetValues("X-Consul-Index").LastOrDefault();

                int newIndex;
                if (indexResponseHeader == null
                    || !int.TryParse(indexResponseHeader, out newIndex)
                    || newIndex <= existingIndex)
                    return existingIndex;

                result(parser(await response.Content.ReadAsStringAsync()));
                return newIndex;
            }

            static Uri CreateKvPrefixUri(string prefix, TimeSpan wait, int index)
            {
                var indexParam = index == 0 ? "" : $"&index={index}";
                return new Uri(
                    $"/v1/kv/{EnsureUnslashed(prefix)}?wait={wait.TotalSeconds}s{indexParam}&recurse",
                    UriKind.Relative);
            }

            static string EnsureUnslashed(string prefix)
            {
                return prefix.StartsWith("/") ? prefix.Substring(1) : prefix;
            }
        }
    }
}