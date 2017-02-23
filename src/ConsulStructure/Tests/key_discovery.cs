using ConsulStructure.Tests.Examples;
using Shouldly;
using Xunit;

namespace ConsulStructure.Tests
{
    public class key_discovery : AbstractStructureTest
    {
        [Fact]
        public void property_discovered()
        {
            var demo = new SimpleProperties();
            Structure.Start(demo, TestOptions<SimpleProperties>("[]"));
            DiscoveredKeys.ShouldContainKey("/keystring");
        }

        [Fact]
        public void nested_property_discovered()
        {
            var demo = new NestedProperties();
            Structure.Start(demo, TestOptions<NestedProperties>("[]"));
            DiscoveredKeys.ShouldContainKey("/nested/keystring");
        }
    }
}