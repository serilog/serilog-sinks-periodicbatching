using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Serilog.Events;
using Serilog.Tests.Support;

namespace Serilog.Sinks.PeriodicBatching.Tests
{
    class InMemoryBatchedSink : IBatchedLogEventSink, IDisposable
    {
        readonly TimeSpan _batchEmitDelay;
        readonly object _stateLock = new object();
        bool _stopped;

        // Postmortem only
        public bool WasCalledAfterDisposal { get; private set; }
        public IList<IList<LogEvent>> Batches { get; }
        public bool IsDisposed { get; private set; }

        public InMemoryBatchedSink(TimeSpan batchEmitDelay)
        {
            _batchEmitDelay = batchEmitDelay;
            Batches = new List<IList<LogEvent>>();
        }

        public void Stop()
        {
            lock (_stateLock)
            {
                _stopped = true;
            }
        }

        public Task EmitBatchAsync(IEnumerable<LogEvent> events)
        {
            lock (_stateLock)
            {
                if (_stopped)
                    return Task.FromResult(0);

                if (IsDisposed)
                    WasCalledAfterDisposal = true;

                Thread.Sleep(_batchEmitDelay);
                Batches.Add(events.ToList());
            }

            return Task.FromResult(0);
        }

        public Task OnEmptyBatchAsync() => Task.FromResult(0);

        public void Dispose()
        {
            lock (_stateLock)
                IsDisposed = true;
        }
    }

    public class PeriodicBatchingSinkTests
    {
        static readonly TimeSpan TinyWait = TimeSpan.FromMilliseconds(200);
        static readonly TimeSpan MicroWait = TimeSpan.FromMilliseconds(1);

        // Some very, very approximate tests here :)

        [Fact]
        public void WhenAnEventIsEnqueuedItIsWrittenToABatch_OnFlush()
        {
            var bs = new InMemoryBatchedSink(TimeSpan.Zero);
            var pbs = new PeriodicBatchingSink(bs, new PeriodicBatchingSinkOptions
                { BatchSizeLimit = 2, Period = TinyWait, EagerlyEmitFirstEvent = true });
            var evt = Some.InformationEvent();
            pbs.Emit(evt);
            pbs.Dispose();
            Assert.Equal(1, bs.Batches.Count);
            Assert.Equal(1, bs.Batches[0].Count);
            Assert.Same(evt, bs.Batches[0][0]);
            Assert.True(bs.IsDisposed);
            Assert.False(bs.WasCalledAfterDisposal);
        }

        [Fact]
        public void WhenAnEventIsEnqueuedItIsWrittenToABatch_OnTimer()
        {
            var bs = new InMemoryBatchedSink(TimeSpan.Zero);
            var pbs = new PeriodicBatchingSink(bs, new PeriodicBatchingSinkOptions
                { BatchSizeLimit = 2, Period = TinyWait, EagerlyEmitFirstEvent = true });
            var evt = Some.InformationEvent();
            pbs.Emit(evt);
            Thread.Sleep(TinyWait + TinyWait);
            bs.Stop();
            pbs.Dispose();
            Assert.Equal(1, bs.Batches.Count);
            Assert.True(bs.IsDisposed);
            Assert.False(bs.WasCalledAfterDisposal);
        }

        [Fact]
        public void WhenAnEventIsEnqueuedItIsWrittenToABatch_FlushWhileRunning()
        {
            var bs = new InMemoryBatchedSink(TinyWait + TinyWait);
            var pbs = new PeriodicBatchingSink(bs, new PeriodicBatchingSinkOptions { BatchSizeLimit = 2, Period = MicroWait, EagerlyEmitFirstEvent = true });
            var evt = Some.InformationEvent();
            pbs.Emit(evt);
            Thread.Sleep(TinyWait);
            pbs.Dispose();
            Assert.Equal(1, bs.Batches.Count);
            Assert.True(bs.IsDisposed);
            Assert.False(bs.WasCalledAfterDisposal);
        }
    }
}
