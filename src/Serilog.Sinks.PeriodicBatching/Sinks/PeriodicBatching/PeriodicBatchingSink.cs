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
public sealed class PeriodicBatchingSink : ILogEventSink, IDisposable
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
    readonly CancellationTokenSource _unloading = new();
    // The write side can wait on this to ensure shutdown has completed.
    readonly Task _loop;
    
    // Used only by the read side.
    readonly IBatchedLogEventSink _batchedLogEventSink;
    readonly int _batchSizeLimit;
    readonly bool _eagerlyEmitFirstEvent;
    readonly BatchedConnectionStatus _status;
    readonly Queue<LogEvent> _waitingBatch = new();

    /// <summary>
    /// Construct a <see cref="PeriodicBatchingSink"/>.
    /// </summary>
    /// <param name="batchedSink">A <see cref="IBatchedLogEventSink"/> to send log event batches to. Batches and empty
    /// batch notifications will not be sent concurrently. When the <see cref="PeriodicBatchingSink"/> is disposed,
    /// it will dispose this object if possible.</param>
    /// <param name="options">Options controlling behavior of the sink.</param>
    public PeriodicBatchingSink(IBatchedLogEventSink batchedSink, PeriodicBatchingSinkOptions options)
    {
        if (options == null) throw new ArgumentNullException(nameof(options));
        if (options.BatchSizeLimit <= 0)
            throw new ArgumentOutOfRangeException(nameof(options), "The batch size limit must be greater than zero.");
        if (options.Period <= TimeSpan.Zero)
            throw new ArgumentOutOfRangeException(nameof(options), "The period must be greater than zero.");

        _batchedLogEventSink = batchedSink ?? throw new ArgumentNullException(nameof(batchedSink));
        _batchSizeLimit = options.BatchSizeLimit;
        _queue = options.QueueLimit is { } limit
            ? Channel.CreateBounded<LogEvent>(new BoundedChannelOptions(limit) { SingleReader = true })
            : Channel.CreateUnbounded<LogEvent>(new UnboundedChannelOptions { SingleReader = true });
        _status = new BatchedConnectionStatus(options.Period);
        _eagerlyEmitFirstEvent = options.EagerlyEmitFirstEvent;

        _loop = Task.Run(LoopAsync, _unloading.Token);
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

        if (_unloading.IsCancellationRequested)
            return;

        _queue.Writer.TryWrite(logEvent);
    }
    
    /// <summary>
    /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
    /// </summary>
    /// <filterpriority>2</filterpriority>
    public void Dispose()
    {
        try
        {
            lock (_stateLock)
            {
                if (!_unloading.IsCancellationRequested)
                {
                    _queue.Writer.Complete();
                    _unloading.Cancel();
                }
            }

            _loop.Wait();
        }
        catch (Exception ex)
        {
            // E.g. the task was canceled before ever being run, or internally failed and threw
            // an unexpected exception.
            SelfLog.WriteLine($"PeriodicBatchingSink ({_batchedLogEventSink}) caught exception during disposal: {ex}");
        }

        (_batchedLogEventSink as IDisposable)?.Dispose();
    }

#if FEATURE_ASYNCDISPOSABLE
        /// <inheritdoc/>
        public async ValueTask DisposeAsync()
        {
            try
            {
                lock (_stateLock)
                {
                    if (!_unloading.IsCancellationRequested)
                    {
                        _queue.Writer.Complete();
                        _unloading.Cancel();
                    }
                }

                await _loop.ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                // E.g. the task was canceled before ever being run, or internally failed and threw
                // an unexpected exception.
                SelfLog.WriteLine($"PeriodicBatchingSink ({_batchedLogEventSink}): caught exception during async disposal: {ex}");
            }

            if (_batchedLogEventSink is IAsyncDisposable asyncDisposable)
                await asyncDisposable.DisposeAsync().ConfigureAwait(false);
            else
                (_batchedLogEventSink as IDisposable)?.Dispose();
        }
#endif

    async Task LoopAsync()
    {
        var isEagerBatch = _eagerlyEmitFirstEvent;
        do
        {
            var fillBatch = Task.Delay(_status.NextInterval);
            do
            {
                while (_waitingBatch.Count < _batchSizeLimit &&
                       !_unloading.IsCancellationRequested &&
                       _queue.Reader.TryRead(out var next))
                {
                    _waitingBatch.Enqueue(next);
                }
            } while ((_waitingBatch.Count < _batchSizeLimit || _waitingBatch.Count > 0 && isEagerBatch) &&
                     !_unloading.IsCancellationRequested &&
                     await TryWaitToReadAsync(_queue.Reader, fillBatch, _unloading.Token));

            try
            {
                if (_waitingBatch.Count == 0)
                {
                    await _batchedLogEventSink.OnEmptyBatchAsync().ConfigureAwait(false);
                }
                else
                {
                    isEagerBatch = false;
                    await _batchedLogEventSink.EmitBatchAsync(_waitingBatch).ConfigureAwait(false);
                }

                _waitingBatch.Clear();
                _status.MarkSuccess();
            }
            catch (Exception ex)
            {
                SelfLog.WriteLine($"PeriodicBatchingSink ({_batchedLogEventSink}) failed emitting a batch: {ex}");
                _status.MarkFailure();
                
                if (_status.ShouldDropBatch)
                    _waitingBatch.Clear();

                if (_status.ShouldDropQueue)
                {
                    // This is not ideal; the goal is to reduce memory pressure on the client if the server is offline
                    // for extended periods. May be worth reviewing and possibly abandoning this.
                    while (_queue.Reader.TryRead(out _) && !_unloading.IsCancellationRequested) { }
                }
            }
        }
        while (!_unloading.IsCancellationRequested);
        
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
                while (_waitingBatch.Count < _batchSizeLimit &&
                       _queue.Reader.TryRead(out var next))
                {
                    _waitingBatch.Enqueue(next);
                }

                if (_waitingBatch.Count != 0)
                {
                    await _batchedLogEventSink.EmitBatchAsync(_waitingBatch).ConfigureAwait(false);
                    _waitingBatch.Clear();
                }
            }
        }
        catch (Exception ex)
        {
            SelfLog.WriteLine($"PeriodicBatchingSink ({_batchedLogEventSink}) failed emitting a batch during shutdown; dropping remaining queued events: {ex}");
        }
    }

    // Wait until `reader` has items to read. Returns `false` if the `timeout` task completes, or if the reader is cancelled.
    async Task<bool> TryWaitToReadAsync(ChannelReader<LogEvent> reader, Task timeout, CancellationToken cancellationToken)
    {
        var completed = await Task.WhenAny(timeout, reader.WaitToReadAsync(cancellationToken).AsTask());

        // Avoid unobserved task exceptions in the cancellation and failure cases. Note that we may not end up observing
        // read task cancellation exceptions during shutdown, may be some room to improve.
        if (completed is { Exception: not null, IsCanceled: false })
        {
            SelfLog.WriteLine($"PeriodicBatchingSink ({_batchedLogEventSink}) could not read from queue: {completed.Exception}");
        }
            
        // `Task.IsCompletedSuccessfully` not available in .NET Standard 2.0/Framework.
        return completed != timeout && completed is { IsCompleted: true, IsCanceled: false, IsFaulted: false };
    }
}