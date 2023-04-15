namespace Serilog.Sinks.PeriodicBatching.Tests.Support;

internal class ManuallyTriggeredTimerFactory : PortableTimerFactory
{
    const int SmallWaitToLetTheTaskStartBeforeContinuingTheTest = 10;

    public ManuallyTriggeredTimer? MainTimer { get; set; }
    public ManuallyTriggeredTimer? MonitoringTimer { get; set; }

    public override PortableTimer CreateMainTimer(Func<CancellationToken, Task> onTick)
    {
        MainTimer = new ManuallyTriggeredTimer(onTick);
        return MainTimer;
    }

    public override PortableTimer CreateMonitoringTimer(Func<CancellationToken, Task> onTick)
    {
        MonitoringTimer = new ManuallyTriggeredTimer(onTick);
        return MonitoringTimer;
    }

    public void CollectQueueState()
    {
        TriggerTick(MonitoringTimer!);
    }

    public void StartProcessingLogEvents()
    {
        TriggerTick(MainTimer!);
    }

    private void TriggerTick(ManuallyTriggeredTimer timer)
    {
        // The tick needs to run on its own thread so that we can empty the queue, fill it and collect the queue state in parallel
        Task.Run(() => timer.TriggerTick());
        Thread.Sleep(SmallWaitToLetTheTaskStartBeforeContinuingTheTest);
    }
}
