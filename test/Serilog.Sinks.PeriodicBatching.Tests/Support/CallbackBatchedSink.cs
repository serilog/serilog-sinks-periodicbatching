using Serilog.Events;

namespace Serilog.Sinks.PeriodicBatching.Tests.Support;

class CallbackBatchedSink(Func<IEnumerable<LogEvent>, Task> callback) : IBatchedLogEventSink
{
    public Task EmitBatchAsync(IEnumerable<LogEvent> batch)
    {
        return callback(batch);
    }

    public Task OnEmptyBatchAsync()
    {
        return Task.CompletedTask;
    }
}
