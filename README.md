# Philiprehberger.JsonMerge

[![CI](https://github.com/philiprehberger/dotnet-json-merge/actions/workflows/ci.yml/badge.svg)](https://github.com/philiprehberger/dotnet-json-merge/actions/workflows/ci.yml)
[![NuGet](https://img.shields.io/nuget/v/Philiprehberger.JsonMerge.svg)](https://www.nuget.org/packages/Philiprehberger.JsonMerge)
[![License](https://img.shields.io/github/license/philiprehberger/dotnet-json-merge)](LICENSE)

Deep merge JSON documents with configurable array strategies and null handling using System.Text.Json.

## Installation

```bash
dotnet add package Philiprehberger.JsonMerge
```

## Usage

```csharp
using System.Text.Json.Nodes;
using Philiprehberger.JsonMerge;

var baseDoc = JsonNode.Parse("""{"name": "app", "db": {"host": "localhost", "port": 5432}}""");
var overrideDoc = JsonNode.Parse("""{"db": {"host": "production.db", "ssl": true}, "version": "2.0"}""");

var result = JsonMerge.Merge(baseDoc!, overrideDoc!);
// {"name": "app", "db": {"host": "production.db", "port": 5432, "ssl": true}, "version": "2.0"}
```

### Custom Options

```csharp
var options = new MergeOptions(ArrayStrategy.Concat, NullHandling.Remove);
var result = JsonMerge.Merge(baseDoc!, overrideDoc!, options);
```

### Merge Multiple Documents

```csharp
var result = JsonMerge.MergeAll(defaults, environment, local);
```

## API

### `JsonMerge`

- `Merge(JsonNode baseNode, JsonNode overrideNode)` — Deep merges two JSON documents. Override values win for scalars.
- `Merge(JsonNode baseNode, JsonNode overrideNode, MergeOptions options)` — Merges with custom options.
- `MergeAll(params JsonNode[] nodes)` — Merges multiple documents left to right.

### `MergeOptions`

Record with two properties:
- `ArrayStrategy` — How to handle array merges: `Replace` (default), `Concat`, `Union`.
- `NullHandling` — How to handle null values: `Keep` (default), `Remove`.

### `ArrayStrategy`

Enum: `Replace`, `Concat`, `Union`.

### `NullHandling`

Enum: `Keep`, `Remove`.

## Development

```bash
dotnet build src/Philiprehberger.JsonMerge.csproj --configuration Release
```

## License

[MIT](LICENSE)
