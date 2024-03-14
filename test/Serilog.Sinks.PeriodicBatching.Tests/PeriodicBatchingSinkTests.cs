using Serilog.Debugging;
using Serilog.Sinks.PeriodicBatching.Tests.Support;
using Xunit;
using Serilog.Tests.Support;
using Xunit.Abstractions;

namespace Serilog.Sinks.PeriodicBatching.Tests;

public class PeriodicBatchingSinkTests
{
    static readonly TimeSpan TinyWait = TimeSpan.FromMilliseconds(200);
    static readonly TimeSpan MicroWait = TimeSpan.FromMilliseconds(1);

    public PeriodicBatchingSinkTests(ITestOutputHelper testOutputHelper)
    {
        SelfLog.Enable(testOutputHelper.WriteLine);
    }

    [Fact]
    public void WhenAnEventIsEnqueuedItIsWrittenToABatchOnDispose()
    {
        var bs = new InMemoryBatchedSink(TimeSpan.Zero);
        var pbs = new PeriodicBatchingSink(bs, new() { BatchSizeLimit = 2, Period = TinyWait, EagerlyEmitFirstEvent = true });
        var evt = Some.InformationEvent();
        pbs.Emit(evt);
        pbs.Dispose();
        Assert.Single(bs.Batches);
        Assert.Single(bs.Batches[0]);
        Assert.Same(evt, bs.Batches[0][0]);
        Assert.True(bs.IsDisposed);
        Assert.False(bs.WasCalledAfterDisposal);
    }

#if FEATURE_ASYNCDISPOSABLE
        [Fact]
        public async Task WhenAnEventIsEnqueuedItIsWrittenToABatchOnDisposeAsync()
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
            Assert.Single(bs.Batches);
            Assert.Single(bs.Batches[0]);
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
        Assert.Single(bs.Batches);
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
        Assert.Single(bs.Batches);
        Assert.True(bs.IsDisposed);
        Assert.False(bs.WasCalledAfterDisposal);
    }

    [Fact]
    public void ExecutionContextDoesNotFlowToBatchedSink()
    {
        var local = new AsyncLocal<int>
        {
            Value = 5
        };

        var observed = 17;
        var bs = new CallbackBatchedSink(_ =>
        {
            observed = local.Value;
            return Task.CompletedTask;
        });
        
        var pbs = new PeriodicBatchingSink(bs, new());
        var evt = Some.InformationEvent();
        pbs.Emit(evt);
        pbs.Dispose();

        Assert.Equal(default, observed);
    }
    
    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task EagerlyEmitFirstEventCausesQuickInitialBatch(bool eagerlyEmit)
    {
        long batchesEmitted = 0;
        var bs = new CallbackBatchedSink(_ =>
        {
            // ReSharper disable once AccessToModifiedClosure
            Interlocked.Increment(ref batchesEmitted);
            return Task.CompletedTask;
        });

        var options = new PeriodicBatchingSinkOptions
        {
            Period = TimeSpan.FromSeconds(2),
            EagerlyEmitFirstEvent = eagerlyEmit,
            BatchSizeLimit = 10,
            QueueLimit = 1000
        };
        
        var pbs = new PeriodicBatchingSink(bs, options);
        
        var evt = Some.InformationEvent();
        pbs.Emit(evt);

        await Task.Delay(1900);
        Assert.Equal(eagerlyEmit ? 1L : 0, Interlocked.Read(ref batchesEmitted));
        
#if FEATURE_ASYNCDISPOSABLE
        await pbs.DisposeAsync();
#else
        pbs.Dispose();
#endif

        Assert.Equal(1L, Interlocked.Read(ref batchesEmitted));
    }
}