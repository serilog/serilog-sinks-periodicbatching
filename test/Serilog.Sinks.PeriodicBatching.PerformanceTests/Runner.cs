using BenchmarkDotNet.Running;
using Xunit;

namespace Serilog.Sinks.PeriodicBatching.PerformanceTests
{
    public class Runner
    {
        [Fact]
        public void BoundedQueue_Enqueue()
        {
            BenchmarkRunner.Run<BoundedQueue_Enqueue_Benchmark>();
        }

        [Fact]
        public void BoundedQueue_Enqueue_Dequeue()
        {
            BenchmarkRunner.Run<BoundedQueue_Enqueue_Dequeue_Benchmark>();
        }
    }
}
