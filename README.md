[![Coverage Status](https://coveralls.io/repos/github/serialseb/ConsulStructure/badge.svg?branch=master)](https://coveralls.io/github/serialseb/ConsulStructure?branch=master)
[![Coverity Scan](https://img.shields.io/coverity/scan/11894.svg)]()
[![Build Status](https://ci.appveyor.com/api/projects/status/mp5v36y4lg76ppju?svg=true)](https://ci.appveyor.com/project/OpenRasta/consulstructure)
[![GitHub release](https://img.shields.io/github/release/serialseb/ConsulStructure.svg)](https://github.com/serialseb/ConsulStructure/releases/latest)
[![NuGet](https://img.shields.io/nuget/v/ConsulStructure.svg)](https://www.nuget.org/packages/ConsulStructure/)
[![GitHub pull requests](https://img.shields.io/github/issues-pr/serialseb/ConsulStructure.svg)](https://github.com/serialseb/ConsulStructure/pulls)

# ConsulStructure – Simple no-dependency configuration


[Consul](https://consul.io) is, amongst other things, a distributed key/value store,
so it can store any bit of data to a specified key, which is just a string.

Little bits of string are pretty useful for configuration data. There are manny ways
to get that data in your configuration. But sometimes you just want **very simple
configuration that doesn't suck**, without bringing in a whole set of packages for it,
so you can hit go as quickly as possible.

This is ConsulStructure.

## Quickstart

ConsulStructure ships as a source-code package, so you don't add more pressure to your
dependency graph.

You will be able to add the code through nuget, when I publish it.

```csharp
Install-Package ConsulStructure
```

To start receiving updates from Consul into your settings, it's one line, no dependencies,
no frills.

```csharp
var configuration = my ProjectConfiguration {
  MyValue = "a very good value", // key: /myvalue
  Subconfiguration = {
    IsStructureAwesome = true // key: /subconfiguration/isstructureawesome
  }
};
var updater = Structure.Start(configuration);
```

Your configuration class can have any level of nesting, it's up to you what it looks like.
Any change to consul is reflected immediately in your class.

## Supported types

Currently, Structure supports nesting, and a couple of base data types for keys, `bool`, `string`,
`int`. As consul only sees values as byte arrays, Converters are customisable to match your needs.

## Using without configuration objects

You can also use ConsulStructure to receive key changes without building objects.

```csharp
public class ConsulListening
{
    public void ListenToKeyValues(IEnumerable<KeyValuePair<string,byte[]>> keyValues)
    {
        foreach(var kv in keyValues) Console.WriteLine($"Received key={kv.Key} with value {Encoding.UTF8.GetString(kv.Value)}");
    }

    public Task Main()
    {
        Structure.Start(ListenToKeyValues);
    }
}
```

## How does it work

On starting up, Structure pre-compiles all the keys in your system, so there is no run-time
reflection. It then does a continuous polling on the agent's HTTP API, to receive values
as soon as they become available.

Whenever a key is updated, the value is assigned immediately, and fast. The data is parsed as
json using the excellent SimpleJson library.

## Things to do

 - [x] Test HTTP part of the library
 - [x] Create AppVeyor / Travis build
 - [x] Create nuget package
 - [ ] Publish to nuget
 - [ ] Add all base datatype converters
 - [ ] Add converters for go's way of writing data (dates and timespans come to mind)
 - [ ] Exponential back off strategy

## Credits

This project is inspired by the excellent functionality provided by the Go-based
[consulstructure](https://github.com/mitchellh/consulstructure/), and uses
[SimpleJson](https://github.com/facebook-csharp-sdk/simple-json) for embeded json serialisation.
