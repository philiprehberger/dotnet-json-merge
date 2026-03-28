using System.Text.Json;

namespace Philiprehberger.JsonMerge;

/// <summary>
/// Defines how arrays should be merged.
/// </summary>
public enum ArrayStrategy
{
    /// <summary>Override array replaces base array entirely.</summary>
    Replace = 0,

    /// <summary>Override array elements are appended to base array.</summary>
    Concat,

    /// <summary>Arrays are merged with duplicate values removed.</summary>
    Union
}

/// <summary>
/// Defines how null values are handled during merge.
/// </summary>
public enum NullHandling
{
    /// <summary>Null values from override are kept in the result.</summary>
    Keep = 0,

    /// <summary>Null values from override cause the property to be removed.</summary>
    Remove
}

/// <summary>
/// Configuration options for JSON merge operations.
/// </summary>
/// <param name="ArrayStrategy">The strategy for merging arrays. Defaults to <see cref="ArrayStrategy.Replace"/>.</param>
/// <param name="NullHandling">How null values should be handled. Defaults to <see cref="NullHandling.Keep"/>.</param>
/// <param name="ArrayMatchKey">
/// When set, array elements from both documents are matched by this key's value
/// and merged together instead of positional merging. Only applies to arrays of objects.
/// </param>
/// <param name="OnConflict">
/// Optional callback invoked when the same JSON path has different scalar values in both documents.
/// Receives the path, the base element, and the override element. The returned value is used as the merged result.
/// When null, the override value wins by default.
/// </param>
/// <param name="PathFilter">
/// When set, only JSON paths matching the filter patterns are merged from the override document.
/// All other paths retain their values from the base document. Supports dot-separated paths (e.g. "db.host").
/// </param>
public record MergeOptions(
    ArrayStrategy ArrayStrategy = ArrayStrategy.Replace,
    NullHandling NullHandling = NullHandling.Keep,
    string? ArrayMatchKey = null,
    Func<string, JsonElement, JsonElement, JsonElement>? OnConflict = null,
    IReadOnlyList<string>? PathFilter = null);
