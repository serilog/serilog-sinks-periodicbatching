// Copyright © Serilog Contributors
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using Serilog.Core;
using Serilog.Debugging;
using Serilog.Events;

// ReSharper disable UnusedParameter.Global, ConvertIfStatementToConditionalTernaryExpression, MemberCanBePrivate.Global, UnusedMember.Global, VirtualMemberNeverOverridden.Global, ClassWithVirtualMembersNeverInherited.Global, SuspiciousTypeConversion.Global

namespace Serilog.Sinks.PeriodicBatching;

/// <summary>
/// Buffers log events into batches for background flushing.
/// </summary>
/// <remarks>
/// To avoid unbounded memory growth, events are discarded after attempting
/// to send a batch, regardless of whether the batch succeeded or not. Implementers
/// that want to change this behavior need to either implement from scratch, or
/// embed retry logic in the batch emitting functions.
/// </remarks>
public class PeriodicBatchingSink : ILogEventSink, IDisposable, IBatchedLogEventSink
#if FEATURE_ASYNCDISPOSABLE
        , IAsyncDisposable
#endif
{
    /// <summary>
    /// Constant used with legacy constructor to indicate that the internal queue shouldn't be limited.
    /// </summary>
    [Obsolete("Implement `IBatchedLogEventSink` and use the `PeriodicBatchingSinkOptions` constructor.")]
    public const int NoQueueLimit = -1;

    readonly IBatchedLogEventSink _batchedLogEventSink;
    readonly int _batchSizeLimit;
    readonly bool _eagerlyEmitFirstEvent;
    readonly BoundedConcurrentQueue<LogEvent> _queue;
    readonly BatchedConnectionStatus _status;
    readonly Queue<LogEvent> _waitingBatch = new();

    readonly object _stateLock = new();

    readonly PortableTimer _timer;

    bool _unloading;
    bool _started;

    /// <summary>
    /// Construct a <see cref="PeriodicBatchingSink"/>.
    /// </summary>
    /// <param name="batchedSink">A <see cref="IBatchedLogEventSink"/> to send log event batches to. Batches and empty
    /// batch notifications will not be sent concurrently. When the <see cref="PeriodicBatchingSink"/> is disposed,
    /// it will dispose this object if possible.</param>
    /// <param name="options">Options controlling behavior of the sink.</param>
    public PeriodicBatchingSink(IBatchedLogEventSink batchedSink, PeriodicBatchingSinkOptions options)
        : this(options, new PortableTimerFactory())
    {
        _batchedLogEventSink = batchedSink ?? throw new ArgumentNullException(nameof(batchedSink));
    }

    /// <summary>
    /// Construct a <see cref="PeriodicBatchingSink"/>. New code should avoid subclassing
    /// <see cref="PeriodicBatchingSink"/> and use
    /// <see cref="PeriodicBatchingSink(Serilog.Sinks.PeriodicBatching.IBatchedLogEventSink,Serilog.Sinks.PeriodicBatching.PeriodicBatchingSinkOptions)"/>
    /// instead.
    /// </summary>
    /// <param name="batchSizeLimit">The maximum number of events to include in a single batch.</param>
    /// <param name="period">The time to wait between checking for event batches.</param>
    [Obsolete("Implement `IBatchedLogEventSink` and use the `PeriodicBatchingSinkOptions` constructor.")]
    protected PeriodicBatchingSink(int batchSizeLimit, TimeSpan period)
        : this(new()
        {
            BatchSizeLimit = batchSizeLimit,
            Period = period,
            EagerlyEmitFirstEvent = true,
            QueueLimit = null
        }, new PortableTimerFactory())
    {
        _batchedLogEventSink = this;
    }

    /// <summary>
    /// Construct a <see cref="PeriodicBatchingSink"/>. New code should avoid subclassing
    /// <see cref="PeriodicBatchingSink"/> and use
    /// <see cref="PeriodicBatchingSink(Serilog.Sinks.PeriodicBatching.IBatchedLogEventSink,Serilog.Sinks.PeriodicBatching.PeriodicBatchingSinkOptions)"/>
    /// instead.
    /// </summary>
    /// <param name="batchSizeLimit">The maximum number of events to include in a single batch.</param>
    /// <param name="period">The time to wait between checking for event batches.</param>
    /// <param name="queueLimit">Maximum number of events in the queue - use <see cref="NoQueueLimit"/> for an unbounded queue.</param>
    [Obsolete("Implement `IBatchedLogEventSink` and use the `PeriodicBatchingSinkOptions` constructor.")]
    protected PeriodicBatchingSink(int batchSizeLimit, TimeSpan period, int queueLimit)
        : this(new()
        {
            BatchSizeLimit = batchSizeLimit,
            Period = period,
            EagerlyEmitFirstEvent = true,
            QueueLimit = queueLimit == NoQueueLimit ? null : queueLimit
        }, new PortableTimerFactory())
    {
        _batchedLogEventSink = this;
    }

    // need to access a ctor with PortableTimerFactory injected in unit tests so that we can replace it with fake timers,
    // but without exposing it to the external world => internal
    internal PeriodicBatchingSink(IBatchedLogEventSink batchedSink, PeriodicBatchingSinkOptions options, PortableTimerFactory timerFactory): this(options, timerFactory) 
    {
        _batchedLogEventSink = batchedSink;
    }

    PeriodicBatchingSink(PeriodicBatchingSinkOptions options, PortableTimerFactory timerFactory)
    {
        if (options == null) throw new ArgumentNullException(nameof(options));

        if (options.BatchSizeLimit <= 0)
            throw new ArgumentOutOfRangeException(nameof(options), "The batch size limit must be greater than zero.");
        if (options.Period <= TimeSpan.Zero)
            throw new ArgumentOutOfRangeException(nameof(options), "The period must be greater than zero.");

        _batchSizeLimit = options.BatchSizeLimit;
        _queue = new(options.QueueLimit);
        _status = new(options.Period);
        _eagerlyEmitFirstEvent = options.EagerlyEmitFirstEvent;
        _timer = timerFactory.CreateMainTimer(_ => OnTick());

        // Initialized by externally-callable constructors.
        _batchedLogEventSink = null!;
    }

    /// <summary>
    /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
    /// </summary>
    /// <filterpriority>2</filterpriority>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Free resources held by the sink.
    /// </summary>
    /// <param name="disposing">If true, called because the object is being disposed; if false,
    /// the object is being disposed from the finalizer.</param>
    protected virtual void Dispose(bool disposing)
    {
        if (!disposing) return;

        lock (_stateLock)
        {
            if (_unloading)
                return;

            _unloading = true;
        }

        _timer.Dispose();

        // This is the place where SynchronizationContext.Current is unknown and can be != null
        // so we prevent possible deadlocks here for sync-over-async downstream implementations.
        TaskUtil.ResetSyncContextAndWait(OnTick);

        (_batchedLogEventSink as IDisposable)?.Dispose();
    }

#if FEATURE_ASYNCDISPOSABLE
        /// <inheritdoc/>
        public async ValueTask DisposeAsync()
        {
            lock (_stateLock)
            {
                if (_unloading)
                    return;

                _unloading = true;
            }

            _timer.Dispose();

            await OnTick().ConfigureAwait(false);

            if (ReferenceEquals(_batchedLogEventSink, this))
            {
                // The sink is being used in the obsolete inheritance-based mode. Old sinks won't
                // override something like `DisposeAsyncCore()`; we just forward to the synchronous
                // `Dispose()` method to ensure whatever cleanup they do still occurs.
                Dispose(true);
                return;
            }

            if (_batchedLogEventSink is IAsyncDisposable asyncDisposable)
                await asyncDisposable.DisposeAsync().ConfigureAwait(false);
            else
                (_batchedLogEventSink as IDisposable)?.Dispose();

            GC.SuppressFinalize(this);
        }
#endif

    /// <summary>
    /// Emit a batch of log events, running to completion synchronously.
    /// </summary>
    /// <param name="events">The events to emit.</param>
    /// <remarks>Override either <see cref="EmitBatch"/> or <see cref="EmitBatchAsync"/>,
    /// not both.</remarks>
    protected virtual void EmitBatch(IEnumerable<LogEvent> events)
    {
    }

    /// <summary>
    /// Emit a batch of log events, running asynchronously.
    /// </summary>
    /// <param name="events">The events to emit.</param>
    /// <remarks>Override either <see cref="EmitBatchAsync"/> or <see cref="EmitBatch"/>,
    /// not both. </remarks>
#pragma warning disable 1998
    protected virtual async Task EmitBatchAsync(IEnumerable<LogEvent> events)
#pragma warning restore 1998
    {
        // ReSharper disable once MethodHasAsyncOverload
        EmitBatch(events);
    }

    async Task OnTick()
    {
        try
        {
            bool batchWasFull;
            do
            {
                while (_waitingBatch.Count < _batchSizeLimit &&
                       _queue.TryDequeue(out var next))
                {
                    _waitingBatch.Enqueue(next);
                }

                if (_waitingBatch.Count == 0)
                {
                    await _batchedLogEventSink.OnEmptyBatchAsync().ConfigureAwait(false);
                    return;
                }

                await _batchedLogEventSink.EmitBatchAsync(_waitingBatch).ConfigureAwait(false);

                batchWasFull = _waitingBatch.Count >= _batchSizeLimit;
                _waitingBatch.Clear();
                _status.MarkSuccess();
            }
            while (batchWasFull); // Otherwise, allow the period to elapse
        }
        catch (Exception ex)
        {
            SelfLog.WriteLine("Exception while emitting periodic batch from {0}: {1}", this, ex);
            _status.MarkFailure();
        }
        finally
        {
            if (_status.ShouldDropBatch)
                _waitingBatch.Clear();

            if (_status.ShouldDropQueue)
            {
                while (_queue.TryDequeue(out _)) { }
            }

            lock (_stateLock)
            {
                if (!_unloading)
                    SetTimer(_status.NextInterval);
            }
        }
    }

    void SetTimer(TimeSpan interval)
    {
        _timer.Start(interval);
    }

    /// <summary>
    /// Emit the provided log event to the sink. If the sink is being disposed or
    /// the app domain unloaded, then the event is ignored.
    /// </summary>
    /// <param name="logEvent">Log event to emit.</param>
    /// <exception cref="ArgumentNullException">The event is null.</exception>
    /// <remarks>
    /// The sink implements the contract that any events whose Emit() method has
    /// completed at the time of sink disposal will be flushed (or attempted to,
    /// depending on app domain state).
    /// </remarks>
    public void Emit(LogEvent logEvent)
    {
        if (logEvent == null) throw new ArgumentNullException(nameof(logEvent));

        if (_unloading)
            return;

        if (!_started)
        {
            lock (_stateLock)
            {
                if (_unloading) return;
                if (!_started)
                {
                    _queue.TryEnqueue(logEvent);
                    _started = true;

                    if (_eagerlyEmitFirstEvent)
                    {
                        // Special handling to try to get the first event across as quickly
                        // as possible to show we're alive!
                        SetTimer(TimeSpan.Zero);
                    }
                    else
                    {
                        SetTimer(_status.NextInterval);
                    }

                    return;
                }
            }
        }

        _queue.TryEnqueue(logEvent);
    }

    /// <summary>
    /// Determine whether a queued log event should be included in the batch. If
    /// an override returns false, the event will be dropped.
    /// </summary>
    /// <param name="logEvent">An event to test for inclusion.</param>
    /// <returns>True if the event should be included in the batch; otherwise, false.</returns>
    // ReSharper disable once UnusedParameter.Global
    protected virtual bool CanInclude(LogEvent logEvent)
    {
        return true;
    }

    /// <summary>
    /// Allows derived sinks to perform periodic work without requiring additional threads
    /// or timers (thus avoiding additional flush/shut-down complexity).
    /// </summary>
    /// <remarks>Override either <see cref="OnEmptyBatch"/> or <see cref="OnEmptyBatchAsync"/>,
    /// not both. </remarks>
    protected virtual void OnEmptyBatch()
    {
    }

    /// <summary>
    /// Allows derived sinks to perform periodic work without requiring additional threads
    /// or timers (thus avoiding additional flush/shut-down complexity).
    /// </summary>
    /// <remarks>Override either <see cref="OnEmptyBatchAsync"/> or <see cref="OnEmptyBatch"/>,
    /// not both. </remarks>
#pragma warning disable 1998
    protected virtual async Task OnEmptyBatchAsync()
#pragma warning restore 1998
    {
        // ReSharper disable once MethodHasAsyncOverload
        OnEmptyBatch();
    }

    Task IBatchedLogEventSink.EmitBatchAsync(IEnumerable<LogEvent> batch) => EmitBatchAsync(batch);
    Task IBatchedLogEventSink.OnEmptyBatchAsync() => OnEmptyBatchAsync();
}