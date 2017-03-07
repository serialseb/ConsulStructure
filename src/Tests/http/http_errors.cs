using System;
using System.Diagnostics;
using System.Net;
using System.Threading.Tasks;
using ConsulStructure;
using Shouldly;
using Tests.Examples;
using Tests.Infrastructure;
using Xunit;

namespace Tests.http
{
  public class http_errors : AbstractHttpMemoryTest
  {
    [Fact]
    public async Task error500()
    {
      var listener = Structure.Start(new SimpleProperties(), TestOptions(next => env => env.Response(500)));
      var exception = await HttpErrors.Dequeue();

      exception.ShouldNotBeNull();

      exception.ShouldBeOfType<InvalidOperationException>();

      await listener.Stop();
    }

    [Fact]
    public async Task nanIndexHeader()
    {
      var listener = Structure.Start(
        new SimpleProperties(),
        TestOptions(
          next => env =>
          {
            env.Response.Headers["X-Consul-Index"] = "nope";
            return env.Response(200);
          }));
      ConsulSimulator.PutKey("kv", "nope");
      var exception = await HttpErrors.Dequeue();

      exception.ShouldBeOfType<InvalidOperationException>();

      await listener.Stop();
    }

    [Fact]
    public async Task tooManyIndexHeader()
    {
      var listener = Structure.Start(
        new SimpleProperties(),
        TestOptions(
          next => env =>
          {
            env.Response.Headers.Add("X-Consul-Index", new[] {"1", "2"});

            return env.Response(200);
          }));

      ConsulSimulator.PutKey("kv", "nope");
      var exception = await HttpErrors.Dequeue();

      Trace.WriteLine(exception.Message);
      exception.ShouldBeOfType<InvalidOperationException>();

      await listener.Stop();
    }

    [Fact]
    public async Task noIndexHeader()
    {
      var listener = Structure.Start(new SimpleProperties(), TestOptions(next => env => env.Response(200)));
      var exception = await HttpErrors.Dequeue();

      exception.ShouldBeOfType<InvalidOperationException>();

      await listener.Stop();
    }

    [Fact]
    public async Task throws()
    {
      var listener = Structure.Start(
        new SimpleProperties(),
        TestOptions(next => env => { throw new OperationCanceledException(); }));

      var exception = await HttpErrors.Dequeue();

      exception.ShouldBeOfType<OperationCanceledException>();

      await listener.Stop();
    }

    [Fact]
    public async Task retries_on_errors_until_successful()
    {
      int retries = 0;
      var listener = Structure.Start(
        new SimpleProperties(),
        TestOptions(next => env => (retries++ < 1 ? env.Response(500) : next(env))));

      ConsulSimulator.PutKey("keystring", "first");

      var success = await HttpSuccesses.Dequeue();
      success.Item2.StatusCode.ShouldBe((HttpStatusCode) 200);

      await listener.Stop();
    }
  }
}