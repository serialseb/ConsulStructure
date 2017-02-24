using System;
using System.Collections.Generic;
using System.Net.Http;
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
            internal delegate void HttpErrorDelegate(Exception e);

            internal delegate void HttpSuccessDelegate(HttpRequestMessage request, HttpResponseMessage response, TimeSpan duration);

            internal KeyDiscoveredDelegate KeyDiscovered { get; set; } = (key, property) => { };
            internal KeyValuesIgnoredDelegate KeyValuesesIgnored { get; set; } = (keyValues) => { };
            internal KeyValuesAssignedDelegate KeyValuesAssigned { get; set; } = (keyValues) => { };

            public HttpErrorDelegate HttpError { get; set; } = (Exception) => { };
            public HttpSuccessDelegate HttpSuccess { get; set; } = (request, response, duration) => { };
        }
    }
}