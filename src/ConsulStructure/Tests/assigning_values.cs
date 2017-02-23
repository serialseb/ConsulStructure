using ConsulStructure.Tests.Examples;
using Shouldly;
using Xunit;

namespace ConsulStructure.Tests
{
    public class assigning_values : AbstractStructureTest
    {
        [Fact]
        public void unknown_key_is_ignored()
        {
            var demo = new SimpleProperties();
            Structure.Start(demo, TestOptions<SimpleProperties>($@"[ {KV("/unknown", "valuestring")} ]"));
            IgnoredKeys.ShouldContainKey("/unknown");
        }

        [Fact]
        public void known_string_key_assigned()
        {
            var demo = new SimpleProperties();
            Structure.Start(demo, TestOptions<SimpleProperties>($@"[ {KV("/keystring", "valuestring")} ]"));
            demo.KeyString.ShouldBe("valuestring");
        }

        [Fact]
        public void known_bool_key_assigned()
        {
            var demo = new SimpleProperties();
            Structure.Start(demo, TestOptions<SimpleProperties>($@"[ {KV("/keybool", "true")} ]"));
            demo.KeyBool.ShouldBe(true);
            Structure.Start(demo, TestOptions<SimpleProperties>($@"[ {KV("/keybool", "false")} ]"));
            demo.KeyBool.ShouldBe(false);
        }

        [Fact]
        public void known_int_key_assigned()
        {
            var demo = new SimpleProperties();
            Structure.Start(demo, TestOptions<SimpleProperties>($@"[ {KV("/keyint32", "1")} ]"));
            demo.KeyInt32.ShouldBe(1);
            Structure.Start(demo, TestOptions<SimpleProperties>($@"[ {KV("/keyint32", "0")} ]"));
            demo.KeyInt32.ShouldBe(0);
        }
    }
}