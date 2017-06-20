using Serilog.Debugging;
using Serilog.Sinks.PeriodicBatching;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

#pragma warning disable 1998

namespace Serilog.Tests.Sinks.PeriodicBatching
{
    public class PortableTimerTests
    {
        [Fact]
        public void WhenItStartsItWaitsUntilHandled_OnDispose()
        {
            bool wasCalled = false;

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
            bool wasCalled = false;
            bool writtenToSelflog = false;

            SelfLog.Enable(_ => writtenToSelflog = true);

            using (var timer = new PortableTimer(async delegate { await Task.Delay(50); wasCalled = true; }))
            {
                timer.Start(TimeSpan.FromMilliseconds(20));
            }

            Thread.Sleep(100);

            Assert.False(wasCalled, "tick handler was called");
            Assert.False(writtenToSelflog, "message was written to SelfLog");
        }

        [Fact]
        public void WhenActiveShouldCancel_OnDispose()
        {
            bool wasCalled = false;
            bool writtenToSelflog = false;

            SelfLog.Enable(_ => writtenToSelflog = true);

            var barrier = new Barrier(participantCount: 2);

            using (var timer = new PortableTimer(
                                    async token =>
                                    {
                                        barrier.SignalAndWait();
                                        await Task.Delay(50);

                                        wasCalled = true;
                                        await Task.Delay(50, token);
                                    }))
            {
                timer.Start(TimeSpan.FromMilliseconds(20));
                barrier.SignalAndWait();
            }

            Assert.True(wasCalled, "tick handler wasn't called");
            Assert.True(writtenToSelflog, "message wasn't written to SelfLog");
        }

        [Fact]
        public void WhenDisposedWillThrow_OnStart()
        {
            bool wasCalled = false;
            var timer = new PortableTimer(async delegate { wasCalled = true; });
            timer.Start(TimeSpan.FromMilliseconds(100));
            timer.Dispose();

            Assert.False(wasCalled);
            Assert.Throws<ObjectDisposedException>(() => timer.Start(TimeSpan.Zero));
        }

        [Fact]
        public void WhenOverlapsShouldProcessOneAtTime_OnTick()
        {
            bool userHandlerOverlapped = false;

            PortableTimer timer = null;
            timer = new PortableTimer(
                async _ =>
                {
                    if (Monitor.TryEnter(timer))
                    {
                        try
                        {
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
            PortableTimer timer = null;
            timer = new PortableTimer(async _ => timer.Start(TimeSpan.FromMilliseconds(1)));

            timer.Start(TimeSpan.Zero);
            Thread.Sleep(50);

            Parallel.For(0, Environment.ProcessorCount * 2, _ => timer.Dispose());
        }        
    }
}

#pragma warning restore 1998