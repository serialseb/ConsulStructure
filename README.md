# ConsulStructure – Simple no-dependency configuration

Sometimes you just want very simple configuration that doesn't suck, without bringing
in a whole set of packages for it, so you can hit go as quickly as possible.

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
  MyValue = "a very good value",
  Subconfiguration = {
    IsStructureAwesome = true
  }
};
var updater = Structure.New(configuration);
```

Your configuration class can have any level of nesting, it's up to you what it looks like.

```chsarp
public class ProjectConfiguration {
  // consul key is /myvalue
  public string MyValue { get; set; }
  public SubConfiguration Sub { get; set; }
}
```

Any change to consul is reflected immediately in your class.

## Supported types

Currently, Structure supports nesting, and a couple of base data types for keys, `bool`, `string`,
`int`. As consul only sees values as byte arrays, Converters are customisable to match your needs.

## How does it work

On starting up, Structure pre-compiles all the keys in your system, so there is no run-time
reflection. It then does a continuous polling on the agent's HTTP API, to receive values
as soon as they become available.

Whenever a key is updated, the value is assigned immediately, and fast. The data is parsed as
json using the excellent SimpleJson library.

## Things to do

 - [ ] Test HTTP part of the library
 - [ ] Create AppVeyor / Travis build
 - [ ] Create nuget package
 - [ ] Add all base datatype converters
 - [ ] Add converters for go's way of writing data (dates and timespans come to mind)

## Credits

This project is inspired by the excellent functionality provided by the Go-based
[consulstructure](https://github.com/mitchellh/consulstructure/], and uses
[SimpleJson](https://github.com/facebook-csharp-sdk/simple-json) for embeded json serialisation.
