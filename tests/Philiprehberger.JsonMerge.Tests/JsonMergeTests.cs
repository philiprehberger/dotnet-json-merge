using Xunit;
using System.Text.Json.Nodes;

namespace Philiprehberger.JsonMerge.Tests;

public class JsonMergeTests
{
    [Fact]
    public void Merge_ScalarOverride_ReturnsOverrideValue()
    {
        var baseNode = JsonNode.Parse("""{"name": "base"}""")!;
        var overrideNode = JsonNode.Parse("""{"name": "override"}""")!;

        var result = JsonMerge.Merge(baseNode, overrideNode);

        Assert.Equal("override", result!["name"]!.GetValue<string>());
    }

    [Fact]
    public void Merge_NestedObjects_MergesRecursively()
    {
        var baseNode = JsonNode.Parse("""{"a": {"x": 1, "y": 2}}""")!;
        var overrideNode = JsonNode.Parse("""{"a": {"y": 3, "z": 4}}""")!;

        var result = JsonMerge.Merge(baseNode, overrideNode);

        Assert.Equal(1, result!["a"]!["x"]!.GetValue<int>());
        Assert.Equal(3, result!["a"]!["y"]!.GetValue<int>());
        Assert.Equal(4, result!["a"]!["z"]!.GetValue<int>());
    }

    [Fact]
    public void Merge_NullBaseNode_ThrowsArgumentNullException()
    {
        var overrideNode = JsonNode.Parse("""{"a": 1}""")!;

        Assert.Throws<ArgumentNullException>(() => JsonMerge.Merge(null!, overrideNode));
    }

    [Fact]
    public void Merge_AddsNewProperties()
    {
        var baseNode = JsonNode.Parse("""{"a": 1}""")!;
        var overrideNode = JsonNode.Parse("""{"b": 2}""")!;

        var result = JsonMerge.Merge(baseNode, overrideNode);

        Assert.Equal(1, result!["a"]!.GetValue<int>());
        Assert.Equal(2, result!["b"]!.GetValue<int>());
    }

    [Fact]
    public void MergeAll_MultipleDocuments_MergesLeftToRight()
    {
        var a = JsonNode.Parse("""{"x": 1, "y": 1}""")!;
        var b = JsonNode.Parse("""{"y": 2, "z": 2}""")!;
        var c = JsonNode.Parse("""{"z": 3}""")!;

        var result = JsonMerge.MergeAll(a, b, c);

        Assert.Equal(1, result!["x"]!.GetValue<int>());
        Assert.Equal(2, result!["y"]!.GetValue<int>());
        Assert.Equal(3, result!["z"]!.GetValue<int>());
    }

    [Fact]
    public void MergeAll_LessThanTwoNodes_ThrowsArgumentException()
    {
        var a = JsonNode.Parse("""{"x": 1}""")!;

        Assert.Throws<ArgumentException>(() => JsonMerge.MergeAll(a));
    }
}
