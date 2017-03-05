using System.Threading.Tasks;
using ConsulStructure;
using Shouldly;
using Tests.Examples;
using Tests.Infrastructure;
using Xunit;

namespace Tests.typed
{
  public class assigning_values : AbstractStructureTest
  {
    [Fact]
    public async Task unknown_key_is_ignored()
    {
      var demo = new SimpleProperties();
      var s = Structure.Start(demo, TestOptions<SimpleProperties>($@"[ {Http.KV("/unknown", "valuestring")} ]"));
      IgnoredKeys.ShouldContainKey("/unknown");
      await s.Stop();
    }

    [Fact]
    public async Task known_string_key_assigned()
    {
      var demo = new SimpleProperties();
      var s = Structure.Start(demo, TestOptions<SimpleProperties>($@"[ {Http.KV("/keystring", "valuestring")} ]"));
      demo.KeyString.ShouldBe("valuestring");
      await s.Stop();
    }

    [Fact]
    public async Task known_bool_key_assigned()
    {
      var demo = new SimpleProperties();
      var s = Structure.Start(demo, TestOptions<SimpleProperties>($@"[ {Http.KV("/keybool", "true")} ]"));
      demo.KeyBool.ShouldBe(true);
      Structure.Start(demo, TestOptions<SimpleProperties>($@"[ {Http.KV("/keybool", "false")} ]"));
      demo.KeyBool.ShouldBe(false);
      await s.Stop();
    }

    [Fact]
    public async Task known_int_key_assigned()
    {
      var demo = new SimpleProperties();
      var s = Structure.Start(demo, TestOptions<SimpleProperties>($@"[ {Http.KV("/keyint32", "1")} ]"));
      demo.KeyInt32.ShouldBe(1);
      await s.Stop();
      s = Structure.Start(demo, TestOptions<SimpleProperties>($@"[ {Http.KV("/keyint32", "0")} ]"));
      demo.KeyInt32.ShouldBe(0);
      await s.Stop();
    }
  }
}