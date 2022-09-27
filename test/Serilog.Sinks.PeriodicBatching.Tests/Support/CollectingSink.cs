using Serilog.Core;
using Serilog.Events;

namespace Serilog.Tests.Support;

class CollectingSink : ILogEventSink
{
    readonly List<LogEvent> _events = new();

    public List<LogEvent> Events => _events;

    public LogEvent SingleEvent => _events.Single();

    public void Emit(LogEvent logEvent)
    {
        _events.Add(logEvent);
    }
}