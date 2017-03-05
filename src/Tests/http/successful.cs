using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using ConsulStructure;
using Shouldly;
using Tests.Examples;
using Tests.Infrastructure;
using Xunit;

namespace Tests.http
{
  public class successful : AbstractHttpMemoryTest
  {
    [Fact]
    public async Task can_shut_down()
    {
      await Structure.Start(new SimpleProperties(), TestOptions()).Stop();
    }

    [Fact]
    public async Task same_index_understood_as_no_change()
    {
      var config = new SimpleProperties();
      var updater = Structure.Start(config, TestOptions(next => env => env.Response(200, 0)));

      await HttpSuccesses.Dequeue();
      HttpErrors.Count.ShouldBe(0);
      KeyValuesAssigned.Count.ShouldBe(0);

      await updater.Stop();
    }

    [Fact]
    public async Task key_is_assigned_repeatedly()
    {
      var config = new SimpleProperties();
      var updater = Structure.Start(config, TestOptions());

      ConsulSimulator.PutKey("/keystring", "first");
      await KeyValuesAssigned.Dequeue();

      config.KeyString.ShouldBe("first");

      await Task.WhenAll(
        ConsulSimulator.PutKeyWithDelay("/keystring", "second", TimeSpan.FromSeconds(3)),
        KeyValuesAssigned.Dequeue());

      config.KeyString.ShouldBe("second");
      await updater.Stop();
    }

    [Fact]
    public async Task mutliple_key_changes_get_correct_value()
    {
      var config = new SimpleProperties();

      ConsulSimulator.PutKey("/keystring", "first");
      ConsulSimulator.PutKey("/keystring", "second");

      var updater = Structure.Start(config, TestOptions());

      var lastResult = await KeyValuesAssigned.Dequeue();
      lastResult.Last().Value.ShouldBe("second");

      config.KeyString.ShouldBe("second");
      await updater.Stop();
    }

    [Fact]
    public async Task key_is_assigned_once()
    {
      var config = new SimpleProperties();
      var updater = Structure.Start(config, TestOptions());

      ConsulSimulator.PutKey("/keystring", "http");
      await KeyValuesAssigned.Dequeue();

      config.KeyString.ShouldBe("http");
      await updater.Stop();
    }

    [Fact]
    public async Task http_events_published()
    {
      var config = new SimpleProperties();
      var updater = Structure.Start(config, TestOptions());

      ConsulSimulator.PutKey("/keystring", "http");
      var result = await HttpSuccesses.Dequeue();

      result.Item1.RequestUri.AbsolutePath.ShouldStartWith("/v1/kv/");

      result.Item2.StatusCode.ShouldBe(HttpStatusCode.OK);

      result.Item3.ShouldBeGreaterThan(TimeSpan.Zero);

      await updater.Stop();
    }
  }
}