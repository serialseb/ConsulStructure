using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using ConsulStructure.Tests.Infrastructure;
using Shouldly;
using Xunit;

namespace ConsulStructure.Tests.untyped
{
    public class property_assignment : AbstractHttpMemoryTest
    {
        [Fact]
        public async Task receives_key()
        {
            string receivedKey = null;
            byte[] receivedValue = null;
            Action<IEnumerable<KeyValuePair<string, byte[]>>> keyReceiver = (kvs) =>
            {
                foreach (var kv in kvs)
                {
                    receivedKey = kv.Key;
                    receivedValue = kv.Value;
                }
            };

            var updater = Structure.Start(keyReceiver, TestOptions());

            ConsulSimulator.PutKey("/key", "/value");
            await KeyAssigned.WaitOne();

            receivedKey.ShouldBe("/key");
            receivedValue.ShouldBe(Encoding.UTF8.GetBytes("/value"));
            await updater.Stop();
        }
    }
}