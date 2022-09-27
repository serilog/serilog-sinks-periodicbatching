using BenchmarkDotNet.Attributes;
using Serilog.Events;
using Serilog.Sinks.PeriodicBatching.PerformanceTests.Support;
using Serilog.Tests.Support;
using System.Collections.Concurrent;

namespace Serilog.Sinks.PeriodicBatching.PerformanceTests;

public class BoundedQueue_Enqueue_Dequeue_Benchmark
{
    const int NON_BOUNDED = -1;

    [Params(50, 100)]
    public int Items { get; set; }

    [Params(NON_BOUNDED, 50, 100)]
    public int Limit { get; set; }

    readonly LogEvent _logEvent = Some.LogEvent();

    Func<ConcurrentQueue<LogEvent>> _concurrentQueueFactory = null!;
    Func<BoundedConcurrentQueue<LogEvent>> _boundedConcurrentQueueFactory = null!;
    Func<BlockingCollection<LogEvent>> _blockingCollectionFactory = null!;
    Func<SynchronizedQueue<LogEvent>> _synchronizedQueueFactory = null!;

    [Setup]
    public void Setup()
    {
        _concurrentQueueFactory = () => new();
        _boundedConcurrentQueueFactory = Limit != NON_BOUNDED ? () => new(Limit)
            : new Func<BoundedConcurrentQueue<LogEvent>>(() => new());
        _blockingCollectionFactory = Limit != NON_BOUNDED ? () => new(Limit)
            : new Func<BlockingCollection<LogEvent>>(() => new());
        _synchronizedQueueFactory = Limit != NON_BOUNDED ? () => new(Limit)
            : new Func<SynchronizedQueue<LogEvent>>(() => new());
    }

    [Benchmark(Baseline = true)]
    public void ConcurrentQueue()
    {
        var queue = _concurrentQueueFactory();
        EnqueueDequeueItems(evt => queue.Enqueue(evt), evt => queue.TryDequeue(out evt));
    }

    [Benchmark]
    public void BoundedConcurrentQueue()
    {
        var queue = _boundedConcurrentQueueFactory();
        EnqueueDequeueItems(evt => queue.TryEnqueue(evt), evt => queue.TryDequeue(out evt));
    }

    [Benchmark]
    public void BlockingCollection()
    {
        var queue = _blockingCollectionFactory();
        EnqueueDequeueItems(evt => queue.TryAdd(evt), evt => queue.TryTake(out evt));
    }

    [Benchmark]
    public void SynchronizedQueue()
    {
        var queue = _synchronizedQueueFactory();
        EnqueueDequeueItems(evt => queue.TryEnqueue(evt), evt => queue.TryDequeue(out evt));
    }

    void EnqueueDequeueItems(Action<LogEvent> enqueueAction, Action<LogEvent?> dequeueAction)
    {
        Parallel.Invoke(
            () => Parallel.For(0, Items, _ => enqueueAction(_logEvent)),
            () => { for (var i = 0; i < Items; i++) dequeueAction(_logEvent); });
    }
}