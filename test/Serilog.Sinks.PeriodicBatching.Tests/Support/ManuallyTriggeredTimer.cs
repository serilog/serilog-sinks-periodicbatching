namespace Serilog.Sinks.PeriodicBatching.Tests.Support;

internal class ManuallyTriggeredTimer : PortableTimer
{
    public ManuallyTriggeredTimer(Func<CancellationToken, Task> onTick) : base(onTick)
    {
    }

    /// <summary>
    /// Manually trigger the timer tick
    /// </summary>
    public void TriggerTick()
    {
        OnTick();
    }

    /// <summary>
    /// Remove real timer, use <see cref="TriggerTick"/> to have a better control on the timing in the context of a test
    /// </summary>
    /// <param name="interval"></param>
    public override void Start(TimeSpan interval) 
    {
    }
}
