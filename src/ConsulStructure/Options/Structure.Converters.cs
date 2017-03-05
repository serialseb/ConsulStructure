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
        var jsonObject = (JsonArray) SimpleJson.DeserializeObject(content);
        foreach (var obj in jsonObject)
        {
          var item = (JsonObject) obj;
          var key = item["Key"].ToString();
          var value = Convert.FromBase64String(item["Value"].ToString());
          yield return new KeyValuePair<string, byte[]>(key, value);
        }
      }
    }
  }
}