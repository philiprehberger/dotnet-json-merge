using Xunit;
using System.Text.Json.Nodes;

namespace Philiprehberger.JsonMerge.Tests;

public class MergeOptionsTests
{
    [Fact]
    public void Merge_ArrayStrategyReplace_ReplacesArray()
    {
        var baseNode = JsonNode.Parse("""{"items": [1, 2]}""")!;
        var overrideNode = JsonNode.Parse("""{"items": [3, 4]}""")!;
        var options = new MergeOptions(ArrayStrategy: ArrayStrategy.Replace);

        var result = JsonMerge.Merge(baseNode, overrideNode, options);

        var items = result!["items"]!.AsArray();
        Assert.Equal(2, items.Count);
        Assert.Equal(3, items[0]!.GetValue<int>());
        Assert.Equal(4, items[1]!.GetValue<int>());
    }

    [Fact]
    public void Merge_ArrayStrategyConcat_ConcatenatesArrays()
    {
        var baseNode = JsonNode.Parse("""{"items": [1, 2]}""")!;
        var overrideNode = JsonNode.Parse("""{"items": [3, 4]}""")!;
        var options = new MergeOptions(ArrayStrategy: ArrayStrategy.Concat);

        var result = JsonMerge.Merge(baseNode, overrideNode, options);

        var items = result!["items"]!.AsArray();
        Assert.Equal(4, items.Count);
    }

    [Fact]
    public void Merge_ArrayStrategyUnion_RemovesDuplicates()
    {
        var baseNode = JsonNode.Parse("""{"items": [1, 2, 3]}""")!;
        var overrideNode = JsonNode.Parse("""{"items": [2, 3, 4]}""")!;
        var options = new MergeOptions(ArrayStrategy: ArrayStrategy.Union);

        var result = JsonMerge.Merge(baseNode, overrideNode, options);

        var items = result!["items"]!.AsArray();
        Assert.Equal(4, items.Count);
    }

    [Fact]
    public void Merge_NullHandlingRemove_RemovesNullProperties()
    {
        var baseNode = JsonNode.Parse("""{"a": 1, "b": 2}""")!;
        var overrideNode = JsonNode.Parse("""{"b": null}""")!;
        var options = new MergeOptions(NullHandling: NullHandling.Remove);

        var result = JsonMerge.Merge(baseNode, overrideNode, options);

        Assert.Equal(1, result!["a"]!.GetValue<int>());
        Assert.Null(result["b"]);
    }

    [Fact]
    public void Merge_NullHandlingKeep_KeepsNullProperties()
    {
        var baseNode = JsonNode.Parse("""{"a": 1, "b": 2}""")!;
        var overrideNode = JsonNode.Parse("""{"b": null}""")!;
        var options = new MergeOptions(NullHandling: NullHandling.Keep);

        var result = JsonMerge.Merge(baseNode, overrideNode, options);

        Assert.Equal(1, result!["a"]!.GetValue<int>());
    }
}
