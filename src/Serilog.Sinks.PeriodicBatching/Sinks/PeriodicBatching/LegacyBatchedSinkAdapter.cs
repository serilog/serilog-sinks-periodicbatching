using Serilog.Events;

namespace Serilog.Sinks.PeriodicBatching;

sealed class LegacyBatchedSinkAdapter: Core.IBatchedLogEventSink, IDisposable
#if FEATURE_ASYNCDISPOSABLE
    , IAsyncDisposable
#endif
{
    readonly IBatchedLogEventSink _inner;
    readonly bool _dispose;

    public LegacyBatchedSinkAdapter(IBatchedLogEventSink inner, bool dispose)
    {
        _inner = inner;
        _dispose = dispose;
    }

    public Task EmitBatchAsync(IReadOnlyCollection<LogEvent> batch)
    {
        return _inner.EmitBatchAsync(batch);
    }

    public Task OnEmptyBatchAsync()
    {
        return _inner.OnEmptyBatchAsync();
    }

    public void Dispose()
    {
        if (!_dispose)
            return;
        
        (_inner as IDisposable)?.Dispose();
    }

#if FEATURE_ASYNCDISPOSABLE
    public async ValueTask DisposeAsync()
    {
        if (!_dispose)
            return;

        if (_inner is IAsyncDisposable asyncDisposable)
        {
            await asyncDisposable.DisposeAsync();
        }
        else
        {
            Dispose();
        }
    }
#endif
}
