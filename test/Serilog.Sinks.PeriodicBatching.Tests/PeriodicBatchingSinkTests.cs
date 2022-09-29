using Serilog.Sinks.PeriodicBatching.Tests.Support;
using Xunit;
using Serilog.Tests.Support;
using System.Threading;

namespace Serilog.Sinks.PeriodicBatching.Tests;

public class PeriodicBatchingSinkTests
{
    static readonly TimeSpan TinyWait = TimeSpan.FromMilliseconds(200);
    static readonly TimeSpan MicroWait = TimeSpan.FromMilliseconds(1);

    [Fact]
    public void WhenAnEventIsEnqueuedItIsWrittenToABatchOnDispose()
    {
        var bs = new InMemoryBatchedSink(TimeSpan.Zero);
        var pbs = new PeriodicBatchingSink(bs, new() { BatchSizeLimit = 2, Period = TinyWait, EagerlyEmitFirstEvent = true });
        var evt = Some.InformationEvent();
        pbs.Emit(evt);
        pbs.Dispose();
        Assert.Equal(1, bs.Batches.Count);
        Assert.Equal(1, bs.Batches[0].Count);
        Assert.Same(evt, bs.Batches[0][0]);
        Assert.True(bs.IsDisposed);
        Assert.False(bs.WasCalledAfterDisposal);
    }

#if FEATURE_ASYNCDISPOSABLE
        [Fact]
        public async ValueTask WhenAnEventIsEnqueuedItIsWrittenToABatchOnDisposeAsync()
        {
            var bs = new InMemoryBatchedSink(TimeSpan.Zero);
            var pbs = new PeriodicBatchingSink(
                bs, new()
                {
                    BatchSizeLimit = 2, Period = TinyWait, EagerlyEmitFirstEvent = true
                });
            var evt = Some.InformationEvent();
            pbs.Emit(evt);
            await pbs.DisposeAsync();
            Assert.Equal(1, bs.Batches.Count);
            Assert.Equal(1, bs.Batches[0].Count);
            Assert.Same(evt, bs.Batches[0][0]);
            Assert.True(bs.IsDisposed);
            Assert.True(bs.IsDisposedAsync);
            Assert.False(bs.WasCalledAfterDisposal);
        }
#endif

    [Fact]
    public void WhenAnEventIsEnqueuedItIsWrittenToABatchOnTimer()
    {
        var bs = new InMemoryBatchedSink(TimeSpan.Zero);
        var pbs = new PeriodicBatchingSink(
            bs,
            new()
            {
                BatchSizeLimit = 2, Period = TinyWait, EagerlyEmitFirstEvent = true
            });
        var evt = Some.InformationEvent();
        pbs.Emit(evt);
        Thread.Sleep(TinyWait + TinyWait);
        bs.Stop();
        pbs.Dispose();
        Assert.Equal(1, bs.Batches.Count);
        Assert.True(bs.IsDisposed);
        Assert.False(bs.WasCalledAfterDisposal);
    }

    [Fact]
    public void WhenAnEventIsEnqueuedItIsWrittenToABatchOnDisposeWhileRunning()
    {
        var bs = new InMemoryBatchedSink(TinyWait + TinyWait);
        var pbs = new PeriodicBatchingSink(bs, new() { BatchSizeLimit = 2, Period = MicroWait, EagerlyEmitFirstEvent = true });
        var evt = Some.InformationEvent();
        pbs.Emit(evt);
        Thread.Sleep(TinyWait);
        pbs.Dispose();
        Assert.Equal(1, bs.Batches.Count);
        Assert.True(bs.IsDisposed);
        Assert.False(bs.WasCalledAfterDisposal);
    }

    [Fact]
    public void WhenTheQueueIsFilledQuickerThanItTakesToEmptyItShouldBeVisibleThroughTheMonitoringCallback()
    {
        var bs = new InMemoryBatchedSink(TinyWait); // Will really take TinyWait to process a batch
        var monitoring = new Monitoring();
        var batchSizeLimit = 2;
        var opts = new PeriodicBatchingSinkOptions
        {
            BatchSizeLimit = batchSizeLimit,
            EagerlyEmitFirstEvent = false,
            Period = MicroWait, // Not used here as we trigger the tick manually
            MonitoringPeriod = MicroWait, // same
            MonitoringCallbackAsync = monitoring.MonitoringCallback,
        };
        var fakeTimers = new ManuallyTriggeredTimerFactory();
        var pbs = new PeriodicBatchingSink(bs, opts, fakeTimers);

        // Start filling the queue
        FillQueueWithOneBatch(pbs, batchSizeLimit);
        fakeTimers.CollectQueueState();
        Assert.Equal(2, monitoring.NumberOfEventsInTheQueue);

        // Start emptying the queue, but it will take one TinyWait per batch
        fakeTimers.StartProcessingLogEvents();
        fakeTimers.CollectQueueState();
        Assert.Equal(0, monitoring.NumberOfEventsInTheQueue);

        // Filling the queue again, but quicker than the TinyWait => monitoring should show that it grows
        FillQueueWithOneBatch(pbs, batchSizeLimit);
        fakeTimers.CollectQueueState();
        Assert.Equal(2, monitoring.NumberOfEventsInTheQueue);
        FillQueueWithOneBatch(pbs, batchSizeLimit);
        fakeTimers.CollectQueueState();
        Assert.Equal(4, monitoring.NumberOfEventsInTheQueue);
        FillQueueWithOneBatch(pbs, batchSizeLimit);
        fakeTimers.CollectQueueState();
        Assert.Equal(6, monitoring.NumberOfEventsInTheQueue);

        // Now let's wait the 3 TinyWait and observe the queue going down
        Thread.Sleep(TinyWait);
        fakeTimers.CollectQueueState();
        Assert.Equal(4, monitoring.NumberOfEventsInTheQueue);
        Thread.Sleep(TinyWait);
        fakeTimers.CollectQueueState();
        Assert.Equal(2, monitoring.NumberOfEventsInTheQueue);
        Thread.Sleep(TinyWait);
        fakeTimers.CollectQueueState();
        Assert.Equal(0, monitoring.NumberOfEventsInTheQueue);
    }

    private void FillQueueWithOneBatch(PeriodicBatchingSink pbs, int batchSizeLimit)
    {
        var evt = Some.InformationEvent();
        for (int i = 0; i < batchSizeLimit; i++)
        {
            pbs.Emit(evt);
        }
    }
}