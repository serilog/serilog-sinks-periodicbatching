namespace Serilog.Sinks.PeriodicBatching.Tests.Support;

class Monitoring
{
    public int NumberOfEventsInTheQueue { get; private set; }

    public Task MonitoringCallback(int numberOfEventsInTheQueue)
    {
        NumberOfEventsInTheQueue = numberOfEventsInTheQueue;
#if NET452
        return Task.FromResult(1);
#else
        return Task.CompletedTask;
#endif
    }
}
