namespace Serilog.Sinks.PeriodicBatching;

/// <summary>
/// Utils class that allow replacement of real timers in the context of unit tests
/// </summary>
internal class PortableTimerFactory
{
    public virtual PortableTimer CreateMainTimer(Func<CancellationToken, Task> onTick) => new PortableTimer(onTick);

    public virtual PortableTimer CreateMonitoringTimer(Func<CancellationToken, Task> onTick) => new PortableTimer(onTick);
}
