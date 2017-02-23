using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Json;

namespace ConsulStructure
{
    partial class Structure
    {
        internal class Converters
        {
            public Func<byte[], string> String { get; set; } = bytes => Encoding.UTF8.GetString(bytes);
            public Func<byte[], int> Int32 { get; set; } = bytes => int.Parse(Encoding.UTF8.GetString(bytes));
            public Func<byte[], bool> Bool { get; set; } = bytes => bool.Parse(Encoding.UTF8.GetString(bytes));

            public Func<string, IEnumerable<KeyValuePair<string, byte[]>>> KeyParser { get; set; } = ParseJson;

            static IEnumerable<KeyValuePair<string, byte[]>> ParseJson(string content)
            {
                return SimpleJson.DeserializeObject<KV[]>(content)
                    .Select(kv => new KeyValuePair<string, byte[]>(kv.Key, Convert.FromBase64String(kv.Value)));
            }
        }
    }
}