using Xunit;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace Philiprehberger.JsonMerge.Tests;

public class OnConflictTests
{
    [Fact]
    public void Merge_OnConflict_InvokedOnScalarConflict()
    {
        var baseNode = JsonNode.Parse("""{"name": "base"}""")!;
        var overrideNode = JsonNode.Parse("""{"name": "override"}""")!;
        string? capturedPath = null;

        var options = new MergeOptions(OnConflict: (path, left, right) =>
        {
            capturedPath = path;
            return left; // Keep base value
        });

        var result = JsonMerge.Merge(baseNode, overrideNode, options);

        Assert.Equal("name", capturedPath);
        Assert.Equal("base", result!["name"]!.GetValue<string>());
    }

    [Fact]
    public void Merge_OnConflict_ReturnsCustomResolvedValue()
    {
        var baseNode = JsonNode.Parse("""{"count": 10}""")!;
        var overrideNode = JsonNode.Parse("""{"count": 20}""")!;

        var options = new MergeOptions(OnConflict: (path, left, right) =>
        {
            var sum = left.GetInt32() + right.GetInt32();
            return JsonSerializer.Deserialize<JsonElement>(sum.ToString());
        });

        var result = JsonMerge.Merge(baseNode, overrideNode, options);

        Assert.Equal(30, result!["count"]!.GetValue<int>());
    }

    [Fact]
    public void Merge_OnConflict_NestedPathIsCorrect()
    {
        var baseNode = JsonNode.Parse("""{"db": {"host": "localhost"}}""")!;
        var overrideNode = JsonNode.Parse("""{"db": {"host": "production"}}""")!;
        string? capturedPath = null;

        var options = new MergeOptions(OnConflict: (path, left, right) =>
        {
            capturedPath = path;
            return right;
        });

        var result = JsonMerge.Merge(baseNode, overrideNode, options);

        Assert.Equal("db.host", capturedPath);
        Assert.Equal("production", result!["db"]!["host"]!.GetValue<string>());
    }

    [Fact]
    public void Merge_OnConflict_NotInvokedForNewProperties()
    {
        var baseNode = JsonNode.Parse("""{"a": 1}""")!;
        var overrideNode = JsonNode.Parse("""{"b": 2}""")!;
        var callCount = 0;

        var options = new MergeOptions(OnConflict: (path, left, right) =>
        {
            callCount++;
            return right;
        });

        var result = JsonMerge.Merge(baseNode, overrideNode, options);

        Assert.Equal(0, callCount);
        Assert.Equal(1, result!["a"]!.GetValue<int>());
        Assert.Equal(2, result!["b"]!.GetValue<int>());
    }
}
