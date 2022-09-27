using Serilog.Debugging;
using Xunit;

#pragma warning disable 1998

// ReSharper disable AccessToModifiedClosure

namespace Serilog.Sinks.PeriodicBatching.Tests;

public class PortableTimerTests
{
    [Fact]
    public void WhenItStartsItWaitsUntilHandled_OnDispose()
    {
        var wasCalled = false;

        var barrier = new Barrier(participantCount: 2);

        using (var timer = new PortableTimer(
                   async delegate
                   {
                       barrier.SignalAndWait();
                       await Task.Delay(100);
                       wasCalled = true;
                   }))
        {
            timer.Start(TimeSpan.Zero);
            barrier.SignalAndWait();
        }

        Assert.True(wasCalled);
    }

    [Fact]
    public void WhenWaitingShouldCancel_OnDispose()
    {
        var wasCalled = false;
        var writtenToSelfLog = false;

        SelfLog.Enable(_ => writtenToSelfLog = true);

        using (var timer = new PortableTimer(async delegate { await Task.Delay(50); wasCalled = true; }))
        {
            timer.Start(TimeSpan.FromMilliseconds(20));
        }

        Thread.Sleep(100);

        Assert.False(wasCalled, "tick handler was called");
        Assert.False(writtenToSelfLog, "message was written to SelfLog");
    }

    [Fact]
    public void WhenActiveShouldCancel_OnDispose()
    {
        var wasCalled = false;
        var writtenToSelfLog = false;

        SelfLog.Enable(_ => writtenToSelfLog = true);

        var barrier = new Barrier(participantCount: 2);

        using (var timer = new PortableTimer(
                   async token =>
                   {
                       // ReSharper disable once MethodSupportsCancellation
                       barrier.SignalAndWait();
                       // ReSharper disable once MethodSupportsCancellation
                       await Task.Delay(20);

                       wasCalled = true;
                       Interlocked.MemoryBarrier();
                       await Task.Delay(100, token);
                   }))
        {
            timer.Start(TimeSpan.FromMilliseconds(20));
            barrier.SignalAndWait();
        }

        Thread.Sleep(100);
        Interlocked.MemoryBarrier();

        Assert.True(wasCalled, "tick handler wasn't called");
        Assert.True(writtenToSelfLog, "message wasn't written to SelfLog");
    }

    [Fact]
    public void WhenDisposedWillThrow_OnStart()
    {
        var wasCalled = false;
        var timer = new PortableTimer(async delegate { wasCalled = true; });
        timer.Start(TimeSpan.FromMilliseconds(100));
        timer.Dispose();

        Assert.False(wasCalled);
        Assert.Throws<ObjectDisposedException>(() => timer.Start(TimeSpan.Zero));
    }

    [Fact]
    public void WhenOverlapsShouldProcessOneAtTime_OnTick()
    {
        var userHandlerOverlapped = false;

        PortableTimer timer = null!;
        timer = new(
            async _ =>
            {
                if (Monitor.TryEnter(timer))
                {
                    try
                    {
                        // ReSharper disable once PossibleNullReferenceException
                        timer.Start(TimeSpan.Zero);
                        Thread.Sleep(20);
                    }
                    finally
                    {
                        Monitor.Exit(timer);
                    }
                }
                else
                {
                    userHandlerOverlapped = true;
                }
            });

        timer.Start(TimeSpan.FromMilliseconds(1));
        Thread.Sleep(50);
        timer.Dispose();

        Assert.False(userHandlerOverlapped);
    }

    [Fact]
    public void CanBeDisposedFromMultipleThreads()
    {
        PortableTimer? timer = null;
        // ReSharper disable once PossibleNullReferenceException
        timer = new(async _ => timer?.Start(TimeSpan.FromMilliseconds(1)));

        timer.Start(TimeSpan.Zero);
        Thread.Sleep(50);

        Parallel.For(0, Environment.ProcessorCount * 2, _ => timer.Dispose());
    }
}