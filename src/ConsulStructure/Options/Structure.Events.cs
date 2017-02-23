using System.Reflection;

namespace ConsulStructure
{
    partial class Structure
    {
        internal class Events
        {
            internal delegate void KeyDiscoveredDelegate(string keyPath, PropertyInfo property);

            internal delegate void KeyValueIgnoredDelegate(string keyPath, byte[] value);

            internal delegate void KeyValueAssignedDelegate(string keyPath, object value);

            internal KeyDiscoveredDelegate KeyDiscovered { get; set; } = (key, property) => { };
            internal KeyValueIgnoredDelegate KeyValueIgnored { get; set; } = (path, value) => { };
            internal KeyValueAssignedDelegate KeyValueAssigned { get; set; } = (path, value) => { };
        }
    }
}