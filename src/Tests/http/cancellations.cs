using System;
using System.Threading;
using System.Threading.Tasks;
using ConsulStructure;
using Shouldly;
using Tests.Examples;
using Tests.Infrastructure;
using Xunit;

namespace Tests.http
{
  public class cancellations : AbstractHttpMemoryTest
  {
    [Fact]
    public async Task edge_case_cancel_after_delay_before_retry()
    {
      var waiter = new ManualResetEventSlim(false);
      var backoffAwaiter = new AutoResetAwaitable();

      var backoffCalculated = false;
      var listener = Structure.Start(
        new SimpleProperties(),
        TestOptions(
          next => env => env.Response(500),
          op =>
          {
            var existingBackoff = op.HttpBackoff;
            op.HttpBackoff = (options, existing) =>
            {
              backoffAwaiter.Signal();
              waiter.Wait();
              backoffCalculated = true;
              return existingBackoff(options, existing);
            };
          }));
      ConsulKvSimulator.PutKey("test", "1");
      await backoffAwaiter.WaitOne();

      // first request stopping
      var stopper = listener.Stop();
      //stopper.Start();
      waiter.Set();
      await stopper;
      backoffCalculated.ShouldBeTrue();
    }

    [Fact]
    public async Task during_http_call()
    {
      var listener = Structure.Start(
        new SimpleProperties(),
        TestOptions(
          next => async env =>
          {
            await Task.Delay(TimeSpan.FromMinutes(10));
            await next(env);
          }));
      await listener.Stop();
    }

    [Fact]
    public async Task during_backoff()
    {
      var listener = Structure.Start(
        new SimpleProperties(),
        TestOptions(next => env => env.Response(500)));

      // ensure we get to good exponential
      await Task.Delay(TimeSpan.FromSeconds(3));
      await listener.Stop();
    }
  }
}