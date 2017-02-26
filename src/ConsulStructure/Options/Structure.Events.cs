using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Reflection;

namespace ConsulStructure
{
    internal partial class Structure
    {
        internal class Events
        {
            internal delegate void KeyDiscoveredDelegate(string keyPath, PropertyInfo property);

            internal KeyDiscoveredDelegate KeyDiscovered { get; set; } = (key, property) => { };
            internal Action<IEnumerable<KeyValuePair<string, byte[]>>> KeyValuesesIgnored { get; set; } = (keyValues) => { };
            internal Action<IEnumerable<KeyValuePair<string, object>>> KeyValuesAssigned { get; set; } = (keyValues) => { };

            public Action<Exception> HttpError { get; set; } = (Exception) => { };
            public Action<HttpRequestMessage, HttpResponseMessage, TimeSpan> HttpSuccess { get; set; } = (request, response, duration) => { };
        }
    }
}