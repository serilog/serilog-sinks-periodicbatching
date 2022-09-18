using Xunit;

namespace Serilog.Sinks.PeriodicBatching.Tests;

public class BoundedConcurrentQueueTests
{
    [Fact]
    public void WhenBoundedShouldNotExceedLimit()
    {
        const int limit = 100;
        var queue = new BoundedConcurrentQueue<int>(limit);

        for (var i = 0; i < limit * 2; i++)
        {
            queue.TryEnqueue(i);
        }

        Assert.Equal(limit, queue.Count);
    }

    [Fact]
    public void WhenInvalidLimitWillThrow()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new BoundedConcurrentQueue<int>(-42));
    }
}