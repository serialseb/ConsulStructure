using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ConsulStructure.Tests.Infrastructure
{
    public static class Http
    {
        public static string KV(string key, string value)
        {
            return $@"{{ ""Key"": ""{key}"", ""Value"": ""{Convert.ToBase64String(Encoding.UTF8.GetBytes(value))}"" }}";
        }

        public static string ToJson(this IDictionary<string, string> kv)
        {
            return $"[{string.Join(",", kv.Select(item => KV(item.Key, item.Value)))}]";
        }
    }
}