using ConsulStructure.Tests.Infrastructure;
using Shouldly;
using Xunit;

namespace ConsulStructure.Tests.Examples
{
    public class Tests : AbstractStructureTest
    {
        [Fact]
        public void AssignString()
        {
            var demo = new SimpleProperties();
            Structure.Start(demo, TestOptions<SimpleProperties>("[]"));
            demo.KeyString.ShouldBeNull();
        }
    }
}