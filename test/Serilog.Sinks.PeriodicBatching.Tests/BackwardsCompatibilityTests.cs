#if FEATURE_ASYNCDISPOSABLE

using Serilog.Sinks.PeriodicBatching.Tests.Support;
using Serilog.Tests.Support;
using Xunit;

namespace Serilog.Sinks.PeriodicBatching.Tests;

public class BackwardsCompatibilityTests
{
    static readonly TimeSpan TinyWait = TimeSpan.FromMilliseconds(200);
    
    [Fact]
    public void LegacyWhenAnEventIsEnqueuedItIsWrittenToABatchOnDispose()
    {
        var bs = new LegacyDisposeTrackingSink();
        var evt = Some.InformationEvent();
        bs.Emit(evt);
        bs.Dispose();
        var batch = Assert.Single(bs.Batches);
        var batched = Assert.Single(batch);
        Assert.Same(evt, batched);
        Assert.True(bs.IsDisposed);
        Assert.False(bs.WasCalledAfterDisposal);
    }
    
    [Fact]
    public void LegacyWhenAnEventIsEnqueuedItIsWrittenToABatchOnTimer()
    {
        var bs = new LegacyDisposeTrackingSink();
        var evt = Some.InformationEvent();
        bs.Emit(evt);
        Thread.Sleep(TinyWait + TinyWait);
        bs.Stop();
        bs.Dispose();
        Assert.Single(bs.Batches);
        Assert.True(bs.IsDisposed);
        Assert.False(bs.WasCalledAfterDisposal);
    }
    
    [Fact]
    public void LegacySinksAreDisposedWhenLoggerIsDisposed()
    {
        var sink = new LegacyDisposeTrackingSink();
        var logger = new LoggerConfiguration().WriteTo.Sink(sink).CreateLogger();
        logger.Dispose();
        Assert.True(sink.IsDisposed);
    }
    
    [Fact]
    public async Task LegacySinksAreDisposedWhenLoggerIsDisposedAsync()
    {
        var sink = new LegacyDisposeTrackingSink();
        var logger = new LoggerConfiguration().WriteTo.Sink(sink).CreateLogger();
        await logger.DisposeAsync();
        Assert.True(sink.IsDisposed);
    }
}

#endif