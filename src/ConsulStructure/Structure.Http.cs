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
      internal delegate Task<HttpResponseMessage> Invoker(HttpRequestMessage request);

      internal static readonly HttpResponseMessage NullResponse = new HttpResponseMessage();

      internal static async Task<int> WaitForChanges(
        Invoker sender,
        string prefix,
        Action<IEnumerable<KeyValuePair<string, byte[]>>> result,
        TimeSpan timeout,
        int existingIndex,
        Func<string, IEnumerable<KeyValuePair<string, byte[]>>> parser)
      {
        var request = new HttpRequestMessage(HttpMethod.Get, CreateKvPrefixUri(prefix, timeout, existingIndex));
        var response = await sender(request);

        var newIndex = int.Parse(response.Headers.GetValues("X-Consul-Index").Single());
        if (newIndex <= existingIndex)
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