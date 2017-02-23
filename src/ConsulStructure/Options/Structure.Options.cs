using System;

namespace ConsulStructure
{
    partial class Structure
    {
        internal partial class Options
        {
            public Uri ConsulUri { get; set; } = new Uri("http://localhost:8500");

            public TimeSpan Timeout { get; set; } = TimeSpan.FromMinutes(10);
            public string Prefix { get; set; } = "";

            public Converters Converters { get; } = new Converters();
            public Factories Factories { get; } = new Factories();
            public Events Events { get; } = new Events();
        }
    }
}