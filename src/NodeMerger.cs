using System.Text.Json;
using System.Text.Json.Nodes;

namespace Philiprehberger.JsonMerge;

/// <summary>
/// Performs recursive deep merge of JSON nodes.
/// </summary>
internal static class NodeMerger
{
    /// <summary>
    /// Recursively merges two JSON nodes according to the specified options.
    /// </summary>
    /// <param name="baseNode">The base node.</param>
    /// <param name="overrideNode">The override node whose values take precedence.</param>
    /// <param name="options">The merge options.</param>
    /// <returns>A new merged <see cref="JsonNode"/>.</returns>
    internal static JsonNode? MergeNodes(JsonNode? baseNode, JsonNode? overrideNode, MergeOptions options)
    {
        if (overrideNode is null)
        {
            return options.NullHandling == NullHandling.Remove ? null : DeepClone(baseNode);
        }

        if (baseNode is null)
        {
            return DeepClone(overrideNode);
        }

        if (baseNode is JsonObject baseObj && overrideNode is JsonObject overrideObj)
        {
            return MergeObjects(baseObj, overrideObj, options);
        }

        if (baseNode is JsonArray baseArr && overrideNode is JsonArray overrideArr)
        {
            return MergeArrays(baseArr, overrideArr, options);
        }

        // Scalar override wins
        return DeepClone(overrideNode);
    }

    private static JsonObject MergeObjects(JsonObject baseObj, JsonObject overrideObj, MergeOptions options)
    {
        var result = new JsonObject();

        // Add all base properties
        foreach (var kvp in baseObj)
        {
            result[kvp.Key] = DeepClone(kvp.Value);
        }

        // Merge or override with override properties
        foreach (var kvp in overrideObj)
        {
            if (kvp.Value is null && options.NullHandling == NullHandling.Remove)
            {
                result.Remove(kvp.Key);
                continue;
            }

            if (result.ContainsKey(kvp.Key))
            {
                result[kvp.Key] = MergeNodes(result[kvp.Key], kvp.Value, options);
            }
            else
            {
                result[kvp.Key] = DeepClone(kvp.Value);
            }
        }

        return result;
    }

    private static JsonArray MergeArrays(JsonArray baseArr, JsonArray overrideArr, MergeOptions options)
    {
        return options.ArrayStrategy switch
        {
            ArrayStrategy.Concat => ConcatArrays(baseArr, overrideArr),
            ArrayStrategy.Union => UnionArrays(baseArr, overrideArr),
            _ => DeepCloneArray(overrideArr) // Replace
        };
    }

    private static JsonArray ConcatArrays(JsonArray baseArr, JsonArray overrideArr)
    {
        var result = new JsonArray();

        foreach (var item in baseArr)
        {
            result.Add(DeepClone(item));
        }

        foreach (var item in overrideArr)
        {
            result.Add(DeepClone(item));
        }

        return result;
    }

    private static JsonArray UnionArrays(JsonArray baseArr, JsonArray overrideArr)
    {
        var result = new JsonArray();
        var seen = new HashSet<string>();

        foreach (var item in baseArr)
        {
            var json = item?.ToJsonString() ?? "null";
            if (seen.Add(json))
            {
                result.Add(DeepClone(item));
            }
        }

        foreach (var item in overrideArr)
        {
            var json = item?.ToJsonString() ?? "null";
            if (seen.Add(json))
            {
                result.Add(DeepClone(item));
            }
        }

        return result;
    }

    private static JsonArray DeepCloneArray(JsonArray array)
    {
        var result = new JsonArray();

        foreach (var item in array)
        {
            result.Add(DeepClone(item));
        }

        return result;
    }

    private static JsonNode? DeepClone(JsonNode? node)
    {
        if (node is null)
        {
            return null;
        }

        return JsonNode.Parse(node.ToJsonString());
    }
}
