using System.Globalization;
using Xunit;

namespace Serilog.Sinks.PeriodicBatching.Tests;

public class FailureAwareBatchSchedulerTests
{
    readonly TimeSpan _defaultPeriod = TimeSpan.FromSeconds(2);

    [Fact]
    public void WhenNoFailuresHaveOccurredTheRegularIntervalIsUsed()
    {
        var bcs = new FailureAwareBatchScheduler(_defaultPeriod);
        Assert.Equal(_defaultPeriod, bcs.NextInterval);
    }

    [Fact]
    public void WhenOneFailureHasOccurredTheRegularIntervalIsUsed()
    {
        var bcs = new FailureAwareBatchScheduler(_defaultPeriod);
        bcs.MarkFailure();
        Assert.Equal(_defaultPeriod, bcs.NextInterval);
    }

    [Fact]
    public void WhenTwoFailuresHaveOccurredTheIntervalBacksOff()
    {
        var bcs = new FailureAwareBatchScheduler(_defaultPeriod);
        bcs.MarkFailure();
        bcs.MarkFailure();
        Assert.Equal(TimeSpan.FromSeconds(10), bcs.NextInterval);
    }

    [Fact]
    public void WhenABatchSucceedsTheStatusResets()
    {
        var bcs = new FailureAwareBatchScheduler(_defaultPeriod);
        bcs.MarkFailure();
        bcs.MarkFailure();
        bcs.MarkSuccess();
        Assert.Equal(_defaultPeriod, bcs.NextInterval);
    }

    [Fact]
    public void WhenThreeFailuresHaveOccurredTheIntervalBacksOff()
    {
        var bcs = new FailureAwareBatchScheduler(_defaultPeriod);
        bcs.MarkFailure();
        bcs.MarkFailure();
        bcs.MarkFailure();
        Assert.Equal(TimeSpan.FromSeconds(20), bcs.NextInterval);
        Assert.False(bcs.ShouldDropBatch);
    }

    [Fact]
    public void When8FailuresHaveOccurredTheIntervalBacksOffAndBatchIsDropped()
    {
        var bcs = new FailureAwareBatchScheduler(_defaultPeriod);
        for (var i = 0; i < 8; ++i)
        {
            Assert.False(bcs.ShouldDropBatch);
            bcs.MarkFailure();
        }
        Assert.Equal(TimeSpan.FromMinutes(10), bcs.NextInterval);
        Assert.True(bcs.ShouldDropBatch);
        Assert.False(bcs.ShouldDropQueue);
    }

    [Fact]
    public void When10FailuresHaveOccurredTheQueueIsDropped()
    {
        var bcs = new FailureAwareBatchScheduler(_defaultPeriod);
        for (var i = 0; i < 10; ++i)
        {
            Assert.False(bcs.ShouldDropQueue);
            bcs.MarkFailure();
        }
        Assert.True(bcs.ShouldDropQueue);
    }

    [Fact]
    public void AtTheDefaultIntervalRetriesFor10MinutesBeforeDroppingBatch()
    {
        var bcs = new FailureAwareBatchScheduler(_defaultPeriod);
        var cumulative = TimeSpan.Zero;
        do
        {
            bcs.MarkFailure();

            if (!bcs.ShouldDropBatch)
                cumulative += bcs.NextInterval;
        } while (!bcs.ShouldDropBatch);

        Assert.False(bcs.ShouldDropQueue);
        Assert.Equal(TimeSpan.Parse("00:10:32", CultureInfo.InvariantCulture), cumulative);
    }

    [Fact]
    public void AtTheDefaultIntervalRetriesFor30MinutesBeforeDroppingQueue()
    {
        var bcs = new FailureAwareBatchScheduler(_defaultPeriod);
        var cumulative = TimeSpan.Zero;
        do
        {
            bcs.MarkFailure();

            if (!bcs.ShouldDropQueue)
                cumulative += bcs.NextInterval;
        } while (!bcs.ShouldDropQueue);

        Assert.Equal(TimeSpan.Parse("00:30:32", CultureInfo.InvariantCulture), cumulative);
    }
}