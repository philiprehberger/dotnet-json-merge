# Philiprehberger.JsonMerge

[![CI](https://github.com/philiprehberger/dotnet-json-merge/actions/workflows/ci.yml/badge.svg)](https://github.com/philiprehberger/dotnet-json-merge/actions/workflows/ci.yml)
[![NuGet](https://img.shields.io/nuget/v/Philiprehberger.JsonMerge.svg)](https://www.nuget.org/packages/Philiprehberger.JsonMerge)
[![GitHub release](https://img.shields.io/github/v/release/philiprehberger/dotnet-json-merge)](https://github.com/philiprehberger/dotnet-json-merge/releases)
[![Last updated](https://img.shields.io/github/last-commit/philiprehberger/dotnet-json-merge)](https://github.com/philiprehberger/dotnet-json-merge/commits/main)
[![License](https://img.shields.io/github/license/philiprehberger/dotnet-json-merge)](LICENSE)
[![Bug Reports](https://img.shields.io/github/issues/philiprehberger/dotnet-json-merge/bug)](https://github.com/philiprehberger/dotnet-json-merge/issues?q=is%3Aissue+is%3Aopen+label%3Abug)
[![Feature Requests](https://img.shields.io/github/issues/philiprehberger/dotnet-json-merge/enhancement)](https://github.com/philiprehberger/dotnet-json-merge/issues?q=is%3Aissue+is%3Aopen+label%3Aenhancement)
[![Sponsor](https://img.shields.io/badge/sponsor-GitHub%20Sponsors-ec6cb9)](https://github.com/sponsors/philiprehberger)

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
using System.Text.Json.Nodes;
using Philiprehberger.JsonMerge;

var options = new MergeOptions(ArrayStrategy.Concat, NullHandling.Remove);
var result = JsonMerge.Merge(baseDoc!, overrideDoc!, options);
```

### Merge Multiple Documents

```csharp
using System.Text.Json.Nodes;
using Philiprehberger.JsonMerge;

var result = JsonMerge.MergeAll(defaults, environment, local);
```

### Array Element Matching by Key

```csharp
using System.Text.Json.Nodes;
using Philiprehberger.JsonMerge;

var baseDoc = JsonNode.Parse("""{"users": [{"id": 1, "name": "Alice"}, {"id": 2, "name": "Bob"}]}""");
var overrideDoc = JsonNode.Parse("""{"users": [{"id": 2, "name": "Robert"}]}""");

var options = new MergeOptions(ArrayMatchKey: "id");
var result = JsonMerge.Merge(baseDoc!, overrideDoc!, options);
// Users matched by "id": Bob becomes Robert, Alice unchanged
```

### Merge Conflict Callback

```csharp
using System.Text.Json;
using System.Text.Json.Nodes;
using Philiprehberger.JsonMerge;

var options = new MergeOptions(OnConflict: (path, left, right) =>
{
    // Custom resolution: keep the base value for "name" paths
    return path.EndsWith("name") ? left : right;
});
var result = JsonMerge.Merge(baseDoc!, overrideDoc!, options);
```

### Path-Based Selective Merge

```csharp
using System.Text.Json.Nodes;
using Philiprehberger.JsonMerge;

var options = new MergeOptions(PathFilter: new[] { "db.host", "version" });
var result = JsonMerge.Merge(baseDoc!, overrideDoc!, options);
// Only "db.host" and "version" are merged from override; other paths keep base values
```

### RFC 7396 JSON Merge Patch

```csharp
using System.Text.Json;
using Philiprehberger.JsonMerge;

var target = JsonDocument.Parse("""{"a": 1, "b": 2, "c": 3}""");
var patch = JsonDocument.Parse("""{"b": null, "c": 4, "d": 5}""");

var result = JsonMerge.MergePatch(target, patch);
// {"a": 1, "c": 4, "d": 5} — "b" removed by null, "c" updated, "d" added
```

## API

### `JsonMerge`

| Method | Description |
|--------|-------------|
| `Merge(JsonNode baseNode, JsonNode overrideNode)` | Deep merges two JSON documents. Override values win for scalars. |
| `Merge(JsonNode baseNode, JsonNode overrideNode, MergeOptions options)` | Merges with custom options. |
| `MergeAll(params JsonNode[] nodes)` | Merges multiple documents left to right. |
| `MergePatch(JsonDocument target, JsonDocument patch)` | Applies an RFC 7396 JSON Merge Patch. Null values remove keys. |

### `MergeOptions`

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `ArrayStrategy` | `ArrayStrategy` | `Replace` | How to handle array merges. |
| `NullHandling` | `NullHandling` | `Keep` | How to handle null values. |
| `ArrayMatchKey` | `string?` | `null` | Match array elements by this key for merging instead of positional. |
| `OnConflict` | `Func<string, JsonElement, JsonElement, JsonElement>?` | `null` | Callback for custom conflict resolution with (path, base, override). |
| `PathFilter` | `IReadOnlyList<string>?` | `null` | Only merge paths matching these patterns. |

### `ArrayStrategy`

| Value | Description |
|-------|-------------|
| `Replace` | Replace base array with override array. |
| `Concat` | Concatenate both arrays. |
| `Union` | Union of both arrays. |

### `NullHandling`

| Value | Description |
|-------|-------------|
| `Keep` | Keep null values in the result. |
| `Remove` | Remove null values from the result. |

## Development

```bash
dotnet build src/Philiprehberger.JsonMerge.csproj --configuration Release
```

## Support

If you find this package useful, consider giving it a star on GitHub — it helps motivate continued maintenance and development.

[![LinkedIn](https://img.shields.io/badge/Philip%20Rehberger-LinkedIn-0A66C2?logo=linkedin)](https://www.linkedin.com/in/philiprehberger)
[![More packages](https://img.shields.io/badge/more-open%20source%20packages-blue)](https://philiprehberger.com/open-source-packages)

## License

[MIT](LICENSE)
