using System;
using System.Threading.Tasks;
using ConsulStructure.Tests.Examples;
using ConsulStructure.Tests.Infrastructure;
using Shouldly;
using Xunit;

namespace ConsulStructure.Tests.http
{
    public class successful : AbstractHttpMemoryTest
    {
        [Fact]
        public async Task can_shut_down()
        {
            await Structure.Start(new SimpleProperties(), TestOptions()).Stop();
        }

        [Fact]
        public async Task key_is_assigned_repeatedly()
        {
            var config = new SimpleProperties();
            var updater = Structure.Start(config, TestOptions());

            ConsulSimulator.PutKey("/keystring", "first");
            await KeyAssigned.WaitOne();

            config.KeyString.ShouldBe("first");

            await Task.WhenAll(
                ConsulSimulator.PutKeyWithDelay("/keystring", "second", TimeSpan.FromSeconds(3)),
                KeyAssigned.WaitOne());

            config.KeyString.ShouldBe("second");
            await updater.Stop();
        }

        [Fact]
        public async Task mutliple_key_changes_get_correct_value()
        {
            var config = new SimpleProperties();
            var updater = Structure.Start(config, TestOptions());

            ConsulSimulator.PutKey("/keystring", "first");
            ConsulSimulator.PutKey("/keystring", "second");

            await KeyAssigned.WaitOne();

            config.KeyString.ShouldBe("second");
            await updater.Stop();
        }

        [Fact]
        public async Task key_is_assigned_once()
        {
            var config = new SimpleProperties();
            var updater = Structure.Start(config, TestOptions());

            ConsulSimulator.PutKey("/keystring", "http");
            await KeyAssigned.WaitOne();

            config.KeyString.ShouldBe("http");
            await updater.Stop();
        }
    }
}