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

using System.Threading.Channels;
using Serilog.Core;
using Serilog.Debugging;
using Serilog.Events;

// ReSharper disable UnusedParameter.Global, ConvertIfStatementToConditionalTernaryExpression, MemberCanBePrivate.Global, UnusedMember.Global, VirtualMemberNeverOverridden.Global, ClassWithVirtualMembersNeverInherited.Global, SuspiciousTypeConversion.Global

namespace Serilog.Sinks.PeriodicBatching;

/// <summary>
/// Buffers log events into batches for background flushing.
/// </summary>
public class PeriodicBatchingSink : ILogEventSink, IDisposable, IBatchedLogEventSink
#if FEATURE_ASYNCDISPOSABLE
        , IAsyncDisposable
#endif
{
    // Buffers events from the write- to the read side.
    readonly Channel<LogEvent> _queue;

    // These fields are used by the write side to signal shutdown.
    // A mutex is required because the queue writer `Complete()` call is not idempotent and will throw if
    // called multiple times, e.g. via multiple `Dispose()` calls on this sink.
    readonly object _stateLock = new();
    // Needed because the read loop needs to observe shutdown even when the target batched (remote) sink is
    // unable to accept events (preventing the queue from being drained and completion being observed).
    readonly CancellationTokenSource _shutdownSignal = new();
    // The write side can wait on this to ensure shutdown has completed.
    readonly Task _runLoop;
    
    // Used only by the read side.
    readonly IBatchedLogEventSink _targetSink = null!;
    readonly int _batchSizeLimit;
    readonly bool _eagerlyEmitFirstEvent;
    readonly FailureAwareBatchScheduler _batchScheduler;
    readonly Queue<LogEvent> _currentBatch = new();
    readonly Task _waitForShutdownSignal;
    Task<bool>? _cachedWaitToRead;
    
    /// <summary>
    /// Constant used with legacy constructor to indicate that the internal queue shouldn't be limited.
    /// </summary>
    [Obsolete("Implement `IBatchedLogEventSink` and use the `PeriodicBatchingSinkOptions` constructor.")]
    public const int NoQueueLimit = -1;

    /// <summary>
    /// Construct a <see cref="PeriodicBatchingSink"/>.
    /// </summary>
    /// <param name="batchedSink">A <see cref="IBatchedLogEventSink"/> to send log event batches to. Batches and empty
    /// batch notifications will not be sent concurrently. When the <see cref="PeriodicBatchingSink"/> is disposed,
    /// it will dispose this object if possible.</param>
    /// <param name="options">Options controlling behavior of the sink.</param>
    public PeriodicBatchingSink(IBatchedLogEventSink batchedSink, PeriodicBatchingSinkOptions options)
        : this(options)
    {
        _targetSink = batchedSink ?? throw new ArgumentNullException(nameof(batchedSink));
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
        : this(new PeriodicBatchingSinkOptions
        {
            BatchSizeLimit = batchSizeLimit,
            Period = period,
            EagerlyEmitFirstEvent = true,
            QueueLimit = null
        })
    {
        _targetSink = this;
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
        : this(new PeriodicBatchingSinkOptions
        {
            BatchSizeLimit = batchSizeLimit,
            Period = period,
            EagerlyEmitFirstEvent = true,
            QueueLimit = queueLimit == NoQueueLimit ? null : queueLimit
        })
    {
        _targetSink = this;
    }

    PeriodicBatchingSink(PeriodicBatchingSinkOptions options)
    {
        if (options == null) throw new ArgumentNullException(nameof(options));
        if (options.BatchSizeLimit <= 0)
            throw new ArgumentOutOfRangeException(nameof(options), "The batch size limit must be greater than zero.");
        if (options.Period <= TimeSpan.Zero)
            throw new ArgumentOutOfRangeException(nameof(options), "The period must be greater than zero.");

        _batchSizeLimit = options.BatchSizeLimit;
        _queue = options.QueueLimit is { } limit
            ? Channel.CreateBounded<LogEvent>(new BoundedChannelOptions(limit) { SingleReader = true })
            : Channel.CreateUnbounded<LogEvent>(new UnboundedChannelOptions { SingleReader = true });
        _batchScheduler = new FailureAwareBatchScheduler(options.Period);
        _eagerlyEmitFirstEvent = options.EagerlyEmitFirstEvent;
        _waitForShutdownSignal = Task.Delay(Timeout.InfiniteTimeSpan, _shutdownSignal.Token)
            .ContinueWith(e => e.Exception, TaskContinuationOptions.OnlyOnFaulted);

        // The conditional here is no longer required in .NET 8+ (dotnet/runtime#82912)
        using (ExecutionContext.IsFlowSuppressed() ? (IDisposable?)null : ExecutionContext.SuppressFlow())
        {
            _runLoop = Task.Run(LoopAsync);
        }
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

        if (_shutdownSignal.IsCancellationRequested)
            return;

        _queue.Writer.TryWrite(logEvent);
    }

    async Task LoopAsync()
    {
        var isEagerBatch = _eagerlyEmitFirstEvent;
        do
        {
            // Code from here through to the `try` block is expected to be infallible. It's structured this way because
            // any failure modes within it haven't been accounted for in the rest of the sink design, and would need
            // consideration in order for the sink to function robustly (i.e. to avoid hot/infinite looping).
            
            var fillBatch = Task.Delay(_batchScheduler.NextInterval);
            do
            {
                while (_currentBatch.Count < _batchSizeLimit &&
                       !_shutdownSignal.IsCancellationRequested &&
                       _queue.Reader.TryRead(out var next) &&
                       CanInclude(next))
                {
                    _currentBatch.Enqueue(next);
                }
            } while ((_currentBatch.Count < _batchSizeLimit && !isEagerBatch || _currentBatch.Count == 0) &&
                     !_shutdownSignal.IsCancellationRequested &&
                     await TryWaitToReadAsync(_queue.Reader, fillBatch, _shutdownSignal.Token).ConfigureAwait(false));

            try
            {
                if (_currentBatch.Count == 0)
                {
                    await _targetSink.OnEmptyBatchAsync().ConfigureAwait(false);
                }
                else
                {
                    isEagerBatch = false;

                    await _targetSink.EmitBatchAsync(_currentBatch).ConfigureAwait(false);

                    _currentBatch.Clear();
                    _batchScheduler.MarkSuccess();
                }
            }
            catch (Exception ex)
            {
                WriteToSelfLog("failed emitting a batch", ex);
                _batchScheduler.MarkFailure();

                if (_batchScheduler.ShouldDropBatch)
                {
                    WriteToSelfLog("dropping the current batch");
                    _currentBatch.Clear();
                }

                if (_batchScheduler.ShouldDropQueue)
                {
                    WriteToSelfLog("dropping all queued events");
                    
                    // Not ideal, uses some CPU capacity unnecessarily and doesn't complete in bounded time. The goal is
                    // to reduce memory pressure on the client if the server is offline for extended periods. May be
                    // worth reviewing and possibly abandoning this.
                    while (_queue.Reader.TryRead(out _) && !_shutdownSignal.IsCancellationRequested) { }
                }

                // Wait out the remainder of the batch fill time so that we don't overwhelm the server. With each
                // successive failure the interval will increase. Needs special handling so that we don't need to
                // make `fillBatch` cancellable (and thus fallible).
                await Task.WhenAny(fillBatch, _waitForShutdownSignal).ConfigureAwait(false);
            }
        }
        while (!_shutdownSignal.IsCancellationRequested);
        
        // At this point:
        //  - The sink is being disposed
        //  - The queue has been completed
        //  - The queue may or may not be empty
        //  - The waiting batch may or may not be empty
        //  - The target sink may or may not be accepting events
        
        // Try flushing the rest of the queue, but bail out on any failure. Shutdown time is unbounded, but it
        // doesn't make sense to pick an arbitrary limit - a future version might add a new option to control this.
        try
        {
            while (_queue.Reader.TryPeek(out _))
            {
                while (_currentBatch.Count < _batchSizeLimit &&
                       _queue.Reader.TryRead(out var next))
                {
                    _currentBatch.Enqueue(next);
                }

                if (_currentBatch.Count != 0)
                {
                    await _targetSink.EmitBatchAsync(_currentBatch).ConfigureAwait(false);
                    _currentBatch.Clear();
                }
            }
        }
        catch (Exception ex)
        {
            WriteToSelfLog("failed emitting a batch during shutdown; dropping remaining queued events", ex);
        }
    }
    
    // Wait until `reader` has items to read. Returns `false` if the `timeout` task completes, or if the reader is cancelled.
    async Task<bool> TryWaitToReadAsync(ChannelReader<LogEvent> reader, Task timeout, CancellationToken cancellationToken)
    {
        var waitToRead = _cachedWaitToRead ?? reader.WaitToReadAsync(cancellationToken).AsTask();
        _cachedWaitToRead = null;
        
        var completed = await Task.WhenAny(timeout, waitToRead).ConfigureAwait(false);

        // Avoid unobserved task exceptions in the cancellation and failure cases. Note that we may not end up observing
        // read task cancellation exceptions during shutdown, may be some room to improve.
        if (completed is { Exception: not null, IsCanceled: false })
        {
            WriteToSelfLog($"could not read from queue: {completed.Exception}");
        }

        if (completed == timeout)
        {
            // Dropping references to `waitToRead` will cause it and some supporting objects to leak; disposing it
            // will break the channel and cause future attempts to read to fail. So, we cache and reuse it next time
            // around the loop.
            
            _cachedWaitToRead = waitToRead;
            return false;
        }

        if (waitToRead.Status is not TaskStatus.RanToCompletion)
            return false;
        
        return await waitToRead;
    }
    
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
        
        SignalShutdown();
        
        try
        {
            _runLoop.Wait();
        }
        catch (Exception ex)
        {
            // E.g. the task was canceled before ever being run, or internally failed and threw
            // an unexpected exception.
            WriteToSelfLog("caught exception during disposal", ex);
        }

        if (ReferenceEquals(_targetSink, this))
        {
            // The sink is being used in the obsolete inheritance-based mode.
            return;
        }

        (_targetSink as IDisposable)?.Dispose();
    }

#if FEATURE_ASYNCDISPOSABLE
        /// <inheritdoc/>
        public async ValueTask DisposeAsync()
        {
            SignalShutdown();

            try
            {
                await _runLoop.ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                // E.g. the task was canceled before ever being run, or internally failed and threw
                // an unexpected exception.
                WriteToSelfLog("caught exception during async disposal", ex);
            }

            if (ReferenceEquals(_targetSink, this))
            {
                // The sink is being used in the obsolete inheritance-based mode. Old sinks won't
                // override something like `DisposeAsyncCore()`; we just forward to the synchronous
                // `Dispose()` method to ensure whatever cleanup they do still occurs.
                Dispose(true);
                return;
            }

            if (_targetSink is IAsyncDisposable asyncDisposable)
                await asyncDisposable.DisposeAsync().ConfigureAwait(false);
            else
                (_targetSink as IDisposable)?.Dispose();

            GC.SuppressFinalize(this);
        }
#endif

    void SignalShutdown()
    {
        lock (_stateLock)
        {
            if (!_shutdownSignal.IsCancellationRequested)
            {
                // Relies on synchronization via `_stateLock`: once the writer is completed, subsequent attempts to
                // complete it will throw.
                _queue.Writer.Complete();
                _shutdownSignal.Cancel();
            }
        }
    }

    void WriteToSelfLog(string message, Exception? exception = null)
    {
        var ex = exception != null ? $"{Environment.NewLine}{exception}" : "";
        SelfLog.WriteLine($"PeriodicBatchingSink ({_targetSink}): {message}{ex}");
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
