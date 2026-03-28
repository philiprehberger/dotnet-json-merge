using Xunit;
using System.Text.Json.Nodes;

namespace Philiprehberger.JsonMerge.Tests;

public class ArrayMatchKeyTests
{
    [Fact]
    public void Merge_ArrayMatchKey_MergesElementsById()
    {
        var baseNode = JsonNode.Parse("""{"items": [{"id": 1, "name": "alpha"}, {"id": 2, "name": "beta"}]}""")!;
        var overrideNode = JsonNode.Parse("""{"items": [{"id": 2, "name": "bravo"}]}""")!;
        var options = new MergeOptions(ArrayMatchKey: "id");

        var result = JsonMerge.Merge(baseNode, overrideNode, options);

        var items = result!["items"]!.AsArray();
        Assert.Equal(2, items.Count);
        Assert.Equal("alpha", items[0]!["name"]!.GetValue<string>());
        Assert.Equal("bravo", items[1]!["name"]!.GetValue<string>());
    }

    [Fact]
    public void Merge_ArrayMatchKey_AddsUnmatchedOverrideElements()
    {
        var baseNode = JsonNode.Parse("""{"items": [{"id": 1, "val": "a"}]}""")!;
        var overrideNode = JsonNode.Parse("""{"items": [{"id": 2, "val": "b"}]}""")!;
        var options = new MergeOptions(ArrayMatchKey: "id");

        var result = JsonMerge.Merge(baseNode, overrideNode, options);

        var items = result!["items"]!.AsArray();
        Assert.Equal(2, items.Count);
        Assert.Equal("a", items[0]!["val"]!.GetValue<string>());
        Assert.Equal("b", items[1]!["val"]!.GetValue<string>());
    }

    [Fact]
    public void Merge_ArrayMatchKey_DeepMergesMatchedObjects()
    {
        var baseNode = JsonNode.Parse("""{"items": [{"id": "x", "a": 1, "b": 2}]}""")!;
        var overrideNode = JsonNode.Parse("""{"items": [{"id": "x", "b": 3, "c": 4}]}""")!;
        var options = new MergeOptions(ArrayMatchKey: "id");

        var result = JsonMerge.Merge(baseNode, overrideNode, options);

        var item = result!["items"]![0]!;
        Assert.Equal(1, item["a"]!.GetValue<int>());
        Assert.Equal(3, item["b"]!.GetValue<int>());
        Assert.Equal(4, item["c"]!.GetValue<int>());
    }

    [Fact]
    public void Merge_ArrayMatchKey_PreservesNonObjectElements()
    {
        var baseNode = JsonNode.Parse("""{"items": [1, 2, 3]}""")!;
        var overrideNode = JsonNode.Parse("""{"items": [4, 5]}""")!;
        var options = new MergeOptions(ArrayMatchKey: "id");

        var result = JsonMerge.Merge(baseNode, overrideNode, options);

        var items = result!["items"]!.AsArray();
        // Non-object elements without the key are kept from base, then unmatched override added
        Assert.Equal(5, items.Count);
    }
}
