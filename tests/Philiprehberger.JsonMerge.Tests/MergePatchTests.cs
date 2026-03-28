using Xunit;
using System.Text.Json;

namespace Philiprehberger.JsonMerge.Tests;

public class MergePatchTests
{
    [Fact]
    public void MergePatch_NullValueRemovesKey()
    {
        var target = JsonDocument.Parse("""{"a": 1, "b": 2}""");
        var patch = JsonDocument.Parse("""{"b": null}""");

        var result = JsonMerge.MergePatch(target, patch);

        Assert.Equal(1, result.RootElement.GetProperty("a").GetInt32());
        Assert.False(result.RootElement.TryGetProperty("b", out _));
    }

    [Fact]
    public void MergePatch_AddsAndOverridesProperties()
    {
        var target = JsonDocument.Parse("""{"a": 1, "b": 2}""");
        var patch = JsonDocument.Parse("""{"b": 3, "c": 4}""");

        var result = JsonMerge.MergePatch(target, patch);

        Assert.Equal(1, result.RootElement.GetProperty("a").GetInt32());
        Assert.Equal(3, result.RootElement.GetProperty("b").GetInt32());
        Assert.Equal(4, result.RootElement.GetProperty("c").GetInt32());
    }

    [Fact]
    public void MergePatch_RecursivelyMergesNestedObjects()
    {
        var target = JsonDocument.Parse("""{"a": {"x": 1, "y": 2}}""");
        var patch = JsonDocument.Parse("""{"a": {"y": 3, "z": 4}}""");

        var result = JsonMerge.MergePatch(target, patch);

        var a = result.RootElement.GetProperty("a");
        Assert.Equal(1, a.GetProperty("x").GetInt32());
        Assert.Equal(3, a.GetProperty("y").GetInt32());
        Assert.Equal(4, a.GetProperty("z").GetInt32());
    }

    [Fact]
    public void MergePatch_NonObjectPatchReplacesTarget()
    {
        var target = JsonDocument.Parse("""{"a": 1}""");
        var patch = JsonDocument.Parse("""42""");

        var result = JsonMerge.MergePatch(target, patch);

        Assert.Equal(42, result.RootElement.GetInt32());
    }

    [Fact]
    public void MergePatch_NullTarget_ThrowsArgumentNullException()
    {
        var patch = JsonDocument.Parse("""{"a": 1}""");

        Assert.Throws<ArgumentNullException>(() => JsonMerge.MergePatch(null!, patch));
    }

    [Fact]
    public void MergePatch_NullPatch_ThrowsArgumentNullException()
    {
        var target = JsonDocument.Parse("""{"a": 1}""");

        Assert.Throws<ArgumentNullException>(() => JsonMerge.MergePatch(target, null!));
    }
}
