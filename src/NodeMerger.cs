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
    /// <param name="currentPath">The current JSON path for conflict callbacks and path filtering.</param>
    /// <returns>A new merged <see cref="JsonNode"/>.</returns>
    internal static JsonNode? MergeNodes(JsonNode? baseNode, JsonNode? overrideNode, MergeOptions options, string currentPath = "")
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
            return MergeObjects(baseObj, overrideObj, options, currentPath);
        }

        if (baseNode is JsonArray baseArr && overrideNode is JsonArray overrideArr)
        {
            return MergeArrays(baseArr, overrideArr, options, currentPath);
        }

        // Scalar conflict: invoke callback if provided
        if (options.OnConflict is not null)
        {
            var baseElement = JsonSerializer.Deserialize<JsonElement>(baseNode.ToJsonString());
            var overrideElement = JsonSerializer.Deserialize<JsonElement>(overrideNode.ToJsonString());
            var resolved = options.OnConflict(currentPath, baseElement, overrideElement);
            return JsonNode.Parse(resolved.GetRawText());
        }

        // Scalar override wins
        return DeepClone(overrideNode);
    }

    private static JsonObject MergeObjects(JsonObject baseObj, JsonObject overrideObj, MergeOptions options, string currentPath)
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
            var childPath = string.IsNullOrEmpty(currentPath) ? kvp.Key : $"{currentPath}.{kvp.Key}";

            // If path filter is set, skip paths not in the filter
            if (options.PathFilter is not null && !IsPathAllowed(childPath, options.PathFilter))
            {
                continue;
            }

            if (kvp.Value is null && options.NullHandling == NullHandling.Remove)
            {
                result.Remove(kvp.Key);
                continue;
            }

            if (result.ContainsKey(kvp.Key))
            {
                result[kvp.Key] = MergeNodes(result[kvp.Key], kvp.Value, options, childPath);
            }
            else
            {
                result[kvp.Key] = DeepClone(kvp.Value);
            }
        }

        return result;
    }

    /// <summary>
    /// Checks whether a given path is allowed by the path filter.
    /// A path is allowed if it matches a filter exactly, is a prefix of a filter entry,
    /// or a filter entry is a prefix of the path.
    /// </summary>
    private static bool IsPathAllowed(string path, IReadOnlyList<string> filters)
    {
        foreach (var filter in filters)
        {
            if (path == filter || path.StartsWith(filter + ".", StringComparison.Ordinal) || filter.StartsWith(path + ".", StringComparison.Ordinal))
            {
                return true;
            }
        }

        return false;
    }

    private static JsonArray MergeArrays(JsonArray baseArr, JsonArray overrideArr, MergeOptions options, string currentPath)
    {
        // If ArrayMatchKey is set and arrays contain objects, merge by key
        if (!string.IsNullOrEmpty(options.ArrayMatchKey))
        {
            return MergeArraysByKey(baseArr, overrideArr, options, currentPath);
        }

        return options.ArrayStrategy switch
        {
            ArrayStrategy.Concat => ConcatArrays(baseArr, overrideArr),
            ArrayStrategy.Union => UnionArrays(baseArr, overrideArr),
            _ => DeepCloneArray(overrideArr) // Replace
        };
    }

    private static JsonArray MergeArraysByKey(JsonArray baseArr, JsonArray overrideArr, MergeOptions options, string currentPath)
    {
        var key = options.ArrayMatchKey!;
        var result = new JsonArray();
        var overrideIndex = new Dictionary<string, JsonNode>();

        // Index override elements by key value
        foreach (var item in overrideArr)
        {
            if (item is JsonObject obj && obj.ContainsKey(key))
            {
                var keyValue = obj[key]?.ToJsonString() ?? "null";
                overrideIndex[keyValue] = item;
            }
        }

        var matched = new HashSet<string>();

        // Merge base elements with matching override elements
        foreach (var item in baseArr)
        {
            if (item is JsonObject obj && obj.ContainsKey(key))
            {
                var keyValue = obj[key]?.ToJsonString() ?? "null";

                if (overrideIndex.TryGetValue(keyValue, out var overrideItem))
                {
                    var elementPath = string.IsNullOrEmpty(currentPath) ? $"[{keyValue}]" : $"{currentPath}[{keyValue}]";
                    result.Add(MergeNodes(item, overrideItem, options, elementPath));
                    matched.Add(keyValue);
                }
                else
                {
                    result.Add(DeepClone(item));
                }
            }
            else
            {
                result.Add(DeepClone(item));
            }
        }

        // Add unmatched override elements
        foreach (var item in overrideArr)
        {
            if (item is JsonObject obj && obj.ContainsKey(key))
            {
                var keyValue = obj[key]?.ToJsonString() ?? "null";
                if (!matched.Contains(keyValue))
                {
                    result.Add(DeepClone(item));
                }
            }
            else
            {
                result.Add(DeepClone(item));
            }
        }

        return result;
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

    internal static JsonNode? DeepClone(JsonNode? node)
    {
        if (node is null)
        {
            return null;
        }

        return JsonNode.Parse(node.ToJsonString());
    }
}
