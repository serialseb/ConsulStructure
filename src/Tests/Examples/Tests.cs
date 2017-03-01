using ConsulStructure;
using Shouldly;
using Tests.Infrastructure;
using Xunit;

namespace Tests.Examples
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