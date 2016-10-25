using BenchmarkDotNet.Attributes;
using Serilog.Events;
using Serilog.Sinks.PeriodicBatching.PerformanceTests.Support;
using Serilog.Tests.Support;
using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace Serilog.Sinks.PeriodicBatching.PerformanceTests
{
    public class BoundedQueue_Enqueue_Benchmark
    {
        const int NON_BOUNDED = -1;

        [Params(50, 100)]
        public int Items { get; set; }

        [Params(NON_BOUNDED, 50, 100)]
        public int Limit { get; set; }

        [Params(1, -1)]
        public int ConcurrencyLevel { get; set; }

        readonly LogEvent _logEvent = Some.LogEvent();

        Func<ConcurrentQueue<LogEvent>> _concurrentQueueFactory;
        Func<BoundedConcurrentQueue<LogEvent>> _boundedConcurrentQueueFactory;
        Func<BlockingCollection<LogEvent>> _blockingCollectionFactory;
        Func<SynchronizedQueue<LogEvent>> _synchronizedQueueFactory;

        [Setup]
        public void Setup()
        {
            _concurrentQueueFactory = () => new ConcurrentQueue<LogEvent>();
            _boundedConcurrentQueueFactory = Limit != NON_BOUNDED ? new Func<BoundedConcurrentQueue<LogEvent>>(() => new BoundedConcurrentQueue<LogEvent>(Limit))
                                                                  : new Func<BoundedConcurrentQueue<LogEvent>>(() => new BoundedConcurrentQueue<LogEvent>());
            _blockingCollectionFactory = Limit != NON_BOUNDED ? new Func<BlockingCollection<LogEvent>>(() => new BlockingCollection<LogEvent>(Limit))
                                                              : new Func<BlockingCollection<LogEvent>>(() => new BlockingCollection<LogEvent>());
            _synchronizedQueueFactory = Limit != NON_BOUNDED ? new Func<SynchronizedQueue<LogEvent>>(() => new SynchronizedQueue<LogEvent>(Limit))
                                                              : new Func<SynchronizedQueue<LogEvent>>(() => new SynchronizedQueue<LogEvent>());
        }

        [Benchmark(Baseline = true)]
        public void ConcurrentQueue()
        {
            var queue = _concurrentQueueFactory();
            EnqueueItems(evt => queue.Enqueue(evt));
        }

        [Benchmark]
        public void BoundedConcurrentQueue()
        {
            var queue = _boundedConcurrentQueueFactory();
            EnqueueItems(evt => queue.TryEnqueue(evt));
        }

        [Benchmark]
        public void BlockingCollection()
        {
            var queue = _blockingCollectionFactory();
            EnqueueItems(evt => queue.TryAdd(evt));
        }

        [Benchmark]
        public void SynchronizedQueue()
        {
            var queue = _synchronizedQueueFactory();
            EnqueueItems(evt => queue.TryEnqueue(evt));
        }

        void EnqueueItems(Action<LogEvent> enqueueAction)
        {
            Parallel.For(0, Items, new ParallelOptions() { MaxDegreeOfParallelism = ConcurrencyLevel }, _ => enqueueAction(_logEvent));
        }
    }
}
