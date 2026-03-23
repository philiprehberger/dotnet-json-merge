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
}
