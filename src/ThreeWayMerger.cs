using System.Text.Json.Nodes;

namespace Philiprehberger.JsonMerge;

/// <summary>
/// Performs three-way merge logic by diffing "ours" and "theirs" against a common base,
/// merging non-conflicting changes and reporting conflicts.
/// </summary>
internal static class ThreeWayMerger
{
    /// <summary>
    /// Recursively merges "ours" and "theirs" against a common base.
    /// Non-conflicting changes from either side are accepted. When both sides
    /// change the same path to different values, a conflict is recorded and
    /// the "ours" value is used as the default resolution.
    /// </summary>
    internal static JsonNode? Merge(JsonNode? baseNode, JsonNode? ours, JsonNode? theirs, string currentPath, List<MergeConflict> conflicts)
    {
        var baseJson = Serialize(baseNode);
        var oursJson = Serialize(ours);
        var theirsJson = Serialize(theirs);

        var oursChanged = baseJson != oursJson;
        var theirsChanged = baseJson != theirsJson;

        // Neither side changed: return base
        if (!oursChanged && !theirsChanged)
        {
            return NodeMerger.DeepClone(baseNode);
        }

        // Only ours changed: take ours
        if (oursChanged && !theirsChanged)
        {
            return NodeMerger.DeepClone(ours);
        }

        // Only theirs changed: take theirs
        if (!oursChanged && theirsChanged)
        {
            return NodeMerger.DeepClone(theirs);
        }

        // Both changed to the same value: no conflict
        if (oursJson == theirsJson)
        {
            return NodeMerger.DeepClone(ours);
        }

        // Both changed differently — attempt recursive merge for objects
        if (baseNode is JsonObject baseObj && ours is JsonObject oursObj && theirs is JsonObject theirsObj)
        {
            return MergeObjects(baseObj, oursObj, theirsObj, currentPath, conflicts);
        }

        // Both changed differently on a scalar or incompatible types — conflict
        conflicts.Add(new MergeConflict(currentPath, baseJson, oursJson, theirsJson));
        return NodeMerger.DeepClone(ours);
    }

    private static JsonObject MergeObjects(JsonObject baseObj, JsonObject oursObj, JsonObject theirsObj, string currentPath, List<MergeConflict> conflicts)
    {
        var result = new JsonObject();
        var allKeys = new HashSet<string>();

        foreach (var kvp in baseObj) allKeys.Add(kvp.Key);
        foreach (var kvp in oursObj) allKeys.Add(kvp.Key);
        foreach (var kvp in theirsObj) allKeys.Add(kvp.Key);

        foreach (var key in allKeys)
        {
            var childPath = string.IsNullOrEmpty(currentPath) ? key : $"{currentPath}.{key}";

            var baseVal = baseObj.ContainsKey(key) ? baseObj[key] : null;
            var oursVal = oursObj.ContainsKey(key) ? oursObj[key] : null;
            var theirsVal = theirsObj.ContainsKey(key) ? theirsObj[key] : null;

            var baseHas = baseObj.ContainsKey(key);
            var oursHas = oursObj.ContainsKey(key);
            var theirsHas = theirsObj.ContainsKey(key);

            // Handle deletions
            var oursDeleted = baseHas && !oursHas;
            var theirsDeleted = baseHas && !theirsHas;

            if (oursDeleted && theirsDeleted)
            {
                // Both deleted — omit
                continue;
            }

            if (oursDeleted && !theirsDeleted)
            {
                if (Serialize(baseVal) == Serialize(theirsVal))
                {
                    // Ours deleted, theirs unchanged — accept deletion
                    continue;
                }

                // Ours deleted but theirs modified — conflict
                conflicts.Add(new MergeConflict(childPath, Serialize(baseVal), null, Serialize(theirsVal)));
                continue; // Ours wins (deletion)
            }

            if (!oursDeleted && theirsDeleted)
            {
                if (Serialize(baseVal) == Serialize(oursVal))
                {
                    // Theirs deleted, ours unchanged — accept deletion
                    continue;
                }

                // Theirs deleted but ours modified — conflict, ours wins (keep)
                conflicts.Add(new MergeConflict(childPath, Serialize(baseVal), Serialize(oursVal), null));
                result[key] = NodeMerger.DeepClone(oursVal);
                continue;
            }

            // Handle additions
            var oursAdded = !baseHas && oursHas;
            var theirsAdded = !baseHas && theirsHas;

            if (oursAdded && !theirsAdded)
            {
                result[key] = NodeMerger.DeepClone(oursVal);
                continue;
            }

            if (!oursAdded && theirsAdded)
            {
                result[key] = NodeMerger.DeepClone(theirsVal);
                continue;
            }

            if (oursAdded && theirsAdded)
            {
                // Both added — recurse to detect if same or conflicting
                var merged = Merge(null, oursVal, theirsVal, childPath, conflicts);
                if (merged is not null)
                {
                    result[key] = merged;
                }

                continue;
            }

            // Both exist — recurse
            var mergedValue = Merge(baseVal, oursVal, theirsVal, childPath, conflicts);
            if (mergedValue is not null)
            {
                result[key] = mergedValue;
            }
        }

        return result;
    }

    private static string? Serialize(JsonNode? node)
    {
        return node?.ToJsonString();
    }
}
