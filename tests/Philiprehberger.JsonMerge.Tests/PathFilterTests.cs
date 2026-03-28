using Xunit;
using System.Text.Json.Nodes;

namespace Philiprehberger.JsonMerge.Tests;

public class PathFilterTests
{
    [Fact]
    public void Merge_PathFilter_OnlyMergesMatchingPaths()
    {
        var baseNode = JsonNode.Parse("""{"a": 1, "b": 2, "c": 3}""")!;
        var overrideNode = JsonNode.Parse("""{"a": 10, "b": 20, "c": 30}""")!;
        var options = new MergeOptions(PathFilter: new[] { "a", "c" });

        var result = JsonMerge.Merge(baseNode, overrideNode, options);

        Assert.Equal(10, result!["a"]!.GetValue<int>());
        Assert.Equal(2, result!["b"]!.GetValue<int>());
        Assert.Equal(30, result!["c"]!.GetValue<int>());
    }

    [Fact]
    public void Merge_PathFilter_WorksWithNestedPaths()
    {
        var baseNode = JsonNode.Parse("""{"db": {"host": "localhost", "port": 5432}, "app": {"name": "old"}}""")!;
        var overrideNode = JsonNode.Parse("""{"db": {"host": "production", "port": 3306}, "app": {"name": "new"}}""")!;
        var options = new MergeOptions(PathFilter: new[] { "db.host" });

        var result = JsonMerge.Merge(baseNode, overrideNode, options);

        Assert.Equal("production", result!["db"]!["host"]!.GetValue<string>());
        Assert.Equal(5432, result!["db"]!["port"]!.GetValue<int>());
        Assert.Equal("old", result!["app"]!["name"]!.GetValue<string>());
    }

    [Fact]
    public void Merge_PathFilter_ParentPathIncludesAllChildren()
    {
        var baseNode = JsonNode.Parse("""{"db": {"host": "localhost", "port": 5432}, "name": "old"}""")!;
        var overrideNode = JsonNode.Parse("""{"db": {"host": "production", "port": 3306}, "name": "new"}""")!;
        var options = new MergeOptions(PathFilter: new[] { "db" });

        var result = JsonMerge.Merge(baseNode, overrideNode, options);

        Assert.Equal("production", result!["db"]!["host"]!.GetValue<string>());
        Assert.Equal(3306, result!["db"]!["port"]!.GetValue<int>());
        Assert.Equal("old", result!["name"]!.GetValue<string>());
    }

    [Fact]
    public void Merge_PathFilter_NullFilterMergesEverything()
    {
        var baseNode = JsonNode.Parse("""{"a": 1, "b": 2}""")!;
        var overrideNode = JsonNode.Parse("""{"a": 10, "b": 20}""")!;
        var options = new MergeOptions(PathFilter: null);

        var result = JsonMerge.Merge(baseNode, overrideNode, options);

        Assert.Equal(10, result!["a"]!.GetValue<int>());
        Assert.Equal(20, result!["b"]!.GetValue<int>());
    }
}
