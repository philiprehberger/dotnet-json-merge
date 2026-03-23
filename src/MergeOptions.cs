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
/// <param name="ArrayStrategy">The strategy for merging arrays.</param>
/// <param name="NullHandling">How null values should be handled.</param>
public record MergeOptions(
    ArrayStrategy ArrayStrategy = ArrayStrategy.Replace,
    NullHandling NullHandling = NullHandling.Keep);
