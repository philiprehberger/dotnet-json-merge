using System.Text.Json.Nodes;

namespace Philiprehberger.JsonMerge;

/// <summary>
/// Represents a conflict detected during a three-way merge where both sides
/// changed the same path to different values.
/// </summary>
/// <param name="Path">The dot-separated JSON path where the conflict occurred.</param>
/// <param name="BaseValue">The original value from the base document, serialized as JSON. Null if the property did not exist.</param>
/// <param name="OursValue">The value from the "ours" document, serialized as JSON. Null if the property was removed.</param>
/// <param name="TheirsValue">The value from the "theirs" document, serialized as JSON. Null if the property was removed.</param>
public record MergeConflict(
    string Path,
    string? BaseValue,
    string? OursValue,
    string? TheirsValue);

/// <summary>
/// Contains the result of a three-way merge, including the merged document
/// and any conflicts that could not be automatically resolved.
/// </summary>
public class ThreeWayMergeResult
{
    /// <summary>
    /// Gets the merged document. When conflicts exist, the "ours" value is used as the default resolution.
    /// </summary>
    public JsonNode? Result { get; }

    /// <summary>
    /// Gets the list of conflicts where both "ours" and "theirs" changed the same path differently.
    /// </summary>
    public IReadOnlyList<MergeConflict> Conflicts { get; }

    /// <summary>
    /// Gets a value indicating whether the merge completed without conflicts.
    /// </summary>
    public bool HasConflicts => Conflicts.Count > 0;

    /// <summary>
    /// Initializes a new instance of the <see cref="ThreeWayMergeResult"/> class.
    /// </summary>
    /// <param name="result">The merged document.</param>
    /// <param name="conflicts">The list of detected conflicts.</param>
    internal ThreeWayMergeResult(JsonNode? result, IReadOnlyList<MergeConflict> conflicts)
    {
        Result = result;
        Conflicts = conflicts;
    }
}
