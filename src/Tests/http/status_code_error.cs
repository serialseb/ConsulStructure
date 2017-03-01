using System;
using System.Threading.Tasks;
using ConsulStructure;
using Shouldly;
using Tests.Examples;
using Tests.Infrastructure;
using Xunit;

namespace Tests.http
{
    public class status_code_error : AbstractHttpMemoryTest
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
        public async Task noIndexHeader()
        {
            var listener = Structure.Start(new SimpleProperties(), TestOptions(next => env => env.Response(200)));
            var exception = await HttpErrors.Dequeue();

            exception.ShouldBeOfType<InvalidOperationException>();

            await listener.Stop();
        }
    }
}