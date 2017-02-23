using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Shouldly;
using Xunit;

namespace ConsulStructure.Tests
{
    public class Values : StructureTest
    {
        [Fact]
        public void unknown_key_is_ignored()
        {
            var demo = new SimpleProperties();
            MemoryStructure(demo, $@"[ {KV("/unknown", "valuestring")} ]");
            IgnoredKeys.ShouldContainKey("/unknown");
        }

        [Fact]
        public void known_string_key_assigned()
        {
            var demo = new SimpleProperties();
            MemoryStructure(demo, $@"[ {KV("/keystring", "valuestring")} ]");
            demo.KeyString.ShouldBe("valuestring");
        }

        [Fact]
        public void known_bool_key_assigned()
        {
            var demo = new SimpleProperties();
            MemoryStructure(demo, $@"[ {KV("/keybool", "true")} ]");
            demo.KeyBool.ShouldBe(true);
            MemoryStructure(demo, $@"[ {KV("/keybool", "false")} ]");
            demo.KeyBool.ShouldBe(false);
        }
    }

    public class Keys : StructureTest
    {
        [Fact]
        public void property_discovered()
        {
            var demo = new SimpleProperties();
            MemoryStructure(demo);
            DiscoveredKeys.ShouldContainKey("/keystring");
        }

        [Fact]
        public void nested_property_discovered()
        {
            var demo = new NestedProperties();
            MemoryStructure(demo);
            DiscoveredKeys.ShouldContainKey("/nested/keystring");
        }
    }

    public class NestedProperties
    {
        public SimpleProperties Nested { get; set; } = new SimpleProperties();
    }

    public class Tests : StructureTest
    {
        [Fact]
        public void AssignString()
        {
            var demo = new SimpleProperties();
            MemoryStructure(demo, "[]");
            demo.KeyString.ShouldBeNull();
        }
    }

    public abstract class StructureTest
    {
        protected string KV(string key, string value)
        {
            return $@"{{ ""Key"": ""{key}"", ""Value"": ""{Convert.ToBase64String(Encoding.UTF8.GetBytes(value))}"" }}";
        }

        protected readonly Dictionary<string, PropertyInfo> DiscoveredKeys
            = new Dictionary<string, PropertyInfo>(StringComparer.OrdinalIgnoreCase);


        protected readonly Dictionary<string, byte[]> IgnoredKeys
            = new Dictionary<string, byte[]>(StringComparer.OrdinalIgnoreCase);

        protected void MemoryStructure<T>(T structure, string json = null)
        {
            Structure.Start(structure, new Structure.Options
            {
                Factories =
                {
                    Watcher = (changes, options) =>
                    {
                        if (json != null)
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
            });
        }
    }


    public class SimpleProperties
    {
        public string KeyString { get; set; }
        public bool KeyBool { get; set; }
        public int KeyInt { get; set; }
    }
}