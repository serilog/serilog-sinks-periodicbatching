using Serilog.Sinks.PeriodicBatching;
using System;
using Xunit;

namespace Serilog.Tests.Sinks.PeriodicBatching
{
    public class BoundedConcurrentQueueTests
    {
        [Fact]
        public void WhenBoundedShouldNotExceedLimit()
        {
            var limit = 100;
            var queue = new BoundedConcurrentQueue<int>(limit);

            for (int i = 0; i < limit * 2; i++)
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
}
