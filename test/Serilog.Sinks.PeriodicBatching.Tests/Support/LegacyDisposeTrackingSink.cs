using Serilog.Events;

#pragma warning disable CS0618

namespace Serilog.Sinks.PeriodicBatching.Tests.Support;

public class LegacyDisposeTrackingSink()
    : PeriodicBatchingSink(10, TimeSpan.FromMinutes(1))
{
    readonly object _stateLock = new();
    bool _stopped;

    // Postmortem only
    public bool WasCalledAfterDisposal { get; private set; }
    public bool IsDisposed { get; private set; }
    public IList<IList<LogEvent>> Batches { get; } = new List<IList<LogEvent>>();

    public void Stop()
    {
        lock (_stateLock)
        {
            _stopped = true;
        }
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        
        lock (_stateLock)
        {
            IsDisposed = true;
        }
    }

    protected override void EmitBatch(IEnumerable<LogEvent> events)
    {
        lock (_stateLock)
        {
            if (_stopped)
                return;

            if (IsDisposed)
                WasCalledAfterDisposal = true;

            Batches.Add(events.ToList());
        }
    }
}