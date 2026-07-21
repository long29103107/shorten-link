using ShortenLink.Core.Querying;
using Xunit;

namespace ShortenLink.Core.Tests;

public sealed class QueryingExtensionsTests
{
    private static readonly Sample[] Items =
    [
        new(1, "Alpha", 10, new DateTime(2025, 1, 1)),
        new(2, "Beta", 30, new DateTime(2024, 1, 1)),
        new(3, "Alpine", 20, new DateTime(2026, 1, 1))
    ];

    [Fact]
    public void ApplySort_SupportsDescendingAndThenByFields()
    {
        var result = Items.AsQueryable()
            .ApplySort("-Score,+Name", ["Score", "Name"])
            .Select(item => item.Id)
            .ToArray();

        Assert.Equal([2, 3, 1], result);
    }

    [Fact]
    public void ApplySort_RejectsPropertyOutsideAllowList()
    {
        Assert.Throws<ArgumentException>(() => Items.AsQueryable().ApplySort("CreatedAt", ["Name"]).ToArray());
    }

    [Fact]
    public void ApplyFilter_SupportsGroupsBooleanOperatorsAndStringOperations()
    {
        var result = Items.AsQueryable()
            .ApplyFilter("((Score ge `20`) & (Name startsWith `Al`)) | (Name eq `Beta`)", ["Score", "Name"])
            .Select(item => item.Id)
            .ToArray();

        Assert.Equal([2, 3], result);
    }

    [Fact]
    public void ApplyFilter_SupportsNotAndIn()
    {
        var result = Items.AsQueryable()
            .ApplyFilter("!(Id in `[1,3]`)", ["Id"])
            .Single();

        Assert.Equal(2, result.Id);
    }

    [Fact]
    public void ApplyFilter_RejectsPropertyOutsideAllowList()
    {
        Assert.Throws<ArgumentException>(() => Items.AsQueryable().ApplyFilter("(Score gt `10`)", ["Name"]).ToArray());
    }

    private sealed record Sample(int Id, string Name, int Score, DateTime CreatedAt);
}
