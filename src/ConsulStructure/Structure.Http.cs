using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ConsulStructure
{
    internal partial class Structure
    {
        static class Http
        {
            internal static async Task<int> WaitForChanges(
                HttpClient client,
                string prefix,
                Action<KeyValuePair<string, byte[]>> result,
                CancellationToken cancel,
                TimeSpan timeout,
                int existingIndex,
                Func<string, IEnumerable<KeyValuePair<string, byte[]>>> parser)
            {
                var response = await client.GetAsync(
                    CreateKvPrefixUri(prefix, timeout, existingIndex),
                    cancel);
                var indexResponseHeader = response.Headers.GetValues("X-Consul-Index").LastOrDefault();

                int newIndex;
                if (indexResponseHeader == null
                    || !Int32.TryParse(indexResponseHeader, out newIndex)
                    || newIndex <= existingIndex)
                    return existingIndex;

                foreach (var key in parser(await response.Content.ReadAsStringAsync()))
                    result(key);
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