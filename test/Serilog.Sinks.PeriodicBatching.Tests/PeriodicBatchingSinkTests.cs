using Serilog.Sinks.PeriodicBatching.Tests.Support;
using Xunit;
using Serilog.Tests.Support;

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
}