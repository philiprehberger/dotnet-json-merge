using System.Text.Json.Nodes;

namespace Philiprehberger.JsonMerge;

/// <summary>
/// Describes the kind of operation that would be applied during a merge.
/// </summary>
public enum MergeOperationKind
{
    /// <summary>A new property is added.</summary>
    Add,

    /// <summary>An existing property value is replaced.</summary>
    Replace,

    /// <summary>A property is removed (due to null handling).</summary>
    Remove,

    /// <summary>An array is merged using the configured strategy.</summary>
    ArrayMerge
}

/// <summary>
/// Represents a single operation that would be applied during a merge.
/// </summary>
/// <param name="Path">The dot-separated JSON path of the affected property.</param>
/// <param name="Kind">The kind of merge operation.</param>
/// <param name="BaseValue">The original value from the base document, serialized as a JSON string. Null when the property does not exist in the base.</param>
/// <param name="IncomingValue">The incoming value from the override document, serialized as a JSON string. Null when the property is being removed.</param>
public record MergeOperation(
    string Path,
    MergeOperationKind Kind,
    string? BaseValue,
    string? IncomingValue);

/// <summary>
/// Contains the result of a dry-run merge preview, including the final merged document
/// and a list of all operations that would be applied.
/// </summary>
public class MergePreview
{
    /// <summary>
    /// Gets the final merged document that would result from applying the merge.
    /// </summary>
    public JsonNode? Result { get; }

    /// <summary>
    /// Gets the list of operations that would be applied during the merge.
    /// </summary>
    public IReadOnlyList<MergeOperation> Operations { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="MergePreview"/> class.
    /// </summary>
    /// <param name="result">The merged document.</param>
    /// <param name="operations">The list of merge operations.</param>
    internal MergePreview(JsonNode? result, IReadOnlyList<MergeOperation> operations)
    {
        Result = result;
        Operations = operations;
    }
}
