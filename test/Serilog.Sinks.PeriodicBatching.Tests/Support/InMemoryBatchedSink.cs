using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Serilog.Events;

namespace Serilog.Sinks.PeriodicBatching.Tests.Support
{
    sealed class InMemoryBatchedSink : IBatchedLogEventSink, IDisposable
#if FEATURE_ASYNCDISPOSABLE
        , IAsyncDisposable
#endif
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

#if FEATURE_ASYNCDISPOSABLE
        public bool IsDisposedAsync { get; private set; }

        public ValueTask DisposeAsync()
        {
            lock (_stateLock)
            {
                IsDisposedAsync = true;
                Dispose();
                return default;
            }            
        }
#endif
    }
}