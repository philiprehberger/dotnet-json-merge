using System.Text.Json;
using System.Text.Json.Nodes;

namespace Philiprehberger.JsonMerge;

/// <summary>
/// Provides static methods for deep merging JSON documents using System.Text.Json.
/// </summary>
public static class JsonMerge
{
    private static readonly MergeOptions DefaultOptions = new();

    /// <summary>
    /// Deep merges two JSON documents. Properties from <paramref name="overrideNode"/>
    /// take precedence over <paramref name="baseNode"/> for scalar values.
    /// Objects are merged recursively and arrays are replaced by default.
    /// </summary>
    /// <param name="baseNode">The base JSON document.</param>
    /// <param name="overrideNode">The override JSON document whose values take precedence.</param>
    /// <returns>A new merged <see cref="JsonNode"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="baseNode"/> is null.</exception>
    public static JsonNode? Merge(JsonNode baseNode, JsonNode overrideNode)
    {
        ArgumentNullException.ThrowIfNull(baseNode);
        return NodeMerger.MergeNodes(baseNode, overrideNode, DefaultOptions);
    }

    /// <summary>
    /// Deep merges two JSON documents with custom merge options.
    /// </summary>
    /// <param name="baseNode">The base JSON document.</param>
    /// <param name="overrideNode">The override JSON document whose values take precedence.</param>
    /// <param name="options">The merge options controlling array and null handling.</param>
    /// <returns>A new merged <see cref="JsonNode"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="baseNode"/> or <paramref name="options"/> is null.</exception>
    public static JsonNode? Merge(JsonNode baseNode, JsonNode overrideNode, MergeOptions options)
    {
        ArgumentNullException.ThrowIfNull(baseNode);
        ArgumentNullException.ThrowIfNull(options);
        return NodeMerger.MergeNodes(baseNode, overrideNode, options);
    }

    /// <summary>
    /// Merges multiple JSON documents from left to right. Each subsequent document
    /// overrides values from previous documents.
    /// </summary>
    /// <param name="nodes">The JSON documents to merge, in order of increasing priority.</param>
    /// <returns>A new merged <see cref="JsonNode"/>.</returns>
    /// <exception cref="ArgumentException">Thrown when fewer than 2 nodes are provided.</exception>
    public static JsonNode? MergeAll(params JsonNode[] nodes)
    {
        if (nodes.Length < 2)
        {
            throw new ArgumentException("At least two nodes are required for merging.", nameof(nodes));
        }

        var result = NodeMerger.MergeNodes(nodes[0], nodes[1], DefaultOptions);

        for (var i = 2; i < nodes.Length; i++)
        {
            result = NodeMerger.MergeNodes(result, nodes[i], DefaultOptions);
        }

        return result;
    }

    /// <summary>
    /// Applies a JSON Merge Patch (RFC 7396) to a target JSON document.
    /// Null values in the patch cause the corresponding key to be removed from the target.
    /// Non-null values are set or recursively merged for objects.
    /// </summary>
    /// <param name="target">The target JSON document to patch.</param>
    /// <param name="patch">The merge patch document conforming to RFC 7396.</param>
    /// <returns>A new <see cref="JsonDocument"/> with the patch applied.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="target"/> or <paramref name="patch"/> is null.</exception>
    public static JsonDocument MergePatch(JsonDocument target, JsonDocument patch)
    {
        ArgumentNullException.ThrowIfNull(target);
        ArgumentNullException.ThrowIfNull(patch);

        var result = ApplyMergePatch(
            JsonNode.Parse(target.RootElement.GetRawText()),
            patch.RootElement);

        var json = result?.ToJsonString() ?? "null";
        return JsonDocument.Parse(json);
    }

    /// <summary>
    /// Recursively applies RFC 7396 merge patch semantics.
    /// </summary>
    private static JsonNode? ApplyMergePatch(JsonNode? target, JsonElement patch)
    {
        if (patch.ValueKind != JsonValueKind.Object)
        {
            // Non-object patch replaces the target entirely
            return JsonNode.Parse(patch.GetRawText());
        }

        // If target is not an object, start with an empty object
        JsonObject result;
        if (target is JsonObject targetObj)
        {
            result = new JsonObject();
            foreach (var kvp in targetObj)
            {
                result[kvp.Key] = NodeMerger.DeepClone(kvp.Value);
            }
        }
        else
        {
            result = new JsonObject();
        }

        foreach (var property in patch.EnumerateObject())
        {
            if (property.Value.ValueKind == JsonValueKind.Null)
            {
                result.Remove(property.Name);
            }
            else
            {
                var existingValue = result.ContainsKey(property.Name) ? result[property.Name] : null;
                result[property.Name] = ApplyMergePatch(existingValue, property.Value);
            }
        }

        return result;
    }
}
