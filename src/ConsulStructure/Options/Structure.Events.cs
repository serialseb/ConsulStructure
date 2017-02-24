using System;
using System.Collections.Generic;
using System.Reflection;

namespace ConsulStructure
{
    partial class Structure
    {
        internal class Events
        {
            internal delegate void KeyDiscoveredDelegate(string keyPath, PropertyInfo property);

            internal delegate void KeyValuesIgnoredDelegate(IEnumerable<KeyValuePair<string, byte[]>> keyValues);

            internal delegate void KeyValuesAssignedDelegate(IEnumerable<KeyValuePair<string, object>> keyValues);

            internal KeyDiscoveredDelegate KeyDiscovered { get; set; } = (key, property) => { };
            internal KeyValuesIgnoredDelegate KeyValuesesIgnored { get; set; } = (keyValues) => { };
            internal KeyValuesAssignedDelegate KeyValuesAssigned { get; set; } = (keyValues) => { };
        }
    }
}