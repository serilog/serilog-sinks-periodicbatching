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

using Serilog.Configuration;
using Serilog.Core;
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
    readonly ILogEventSink _targetSink;
    readonly bool _inheritanceApi;
    
    /// <summary>
    /// Constant used with legacy constructor to indicate that the internal queue shouldn't be limited.
    /// </summary>
    [Obsolete("Implement `IBatchedLogEventSink` and use the `PeriodicBatchingSinkOptions` constructor.")]
    public const int NoQueueLimit = -1;

    static ILogEventSink CreateSink(
        IBatchedLogEventSink batchedLogEventSink,
        bool disposeBatchedSink,
        PeriodicBatchingSinkOptions options)
    {
        if (options == null) throw new ArgumentNullException(nameof(options));
        if (options.BatchSizeLimit <= 0)
            throw new ArgumentOutOfRangeException(nameof(options), "The batch size limit must be greater than zero.");
        if (options.Period <= TimeSpan.Zero)
            throw new ArgumentOutOfRangeException(nameof(options), "The period must be greater than zero.");

        var adaptedOptions = new BatchingOptions
        {
            BatchSizeLimit = options.BatchSizeLimit,
            QueueLimit = options.QueueLimit,
            BufferingTimeLimit = options.Period,
            EagerlyEmitFirstEvent = options.EagerlyEmitFirstEvent
        };

        var adapter = new LegacyBatchedSinkAdapter(batchedLogEventSink, disposeBatchedSink);

        return LoggerSinkConfiguration.CreateSink(lc => lc.Sink(adapter, adaptedOptions));
    }

    /// <summary>
    /// Construct a <see cref="PeriodicBatchingSink"/>.
    /// </summary>
    /// <param name="batchedSink">A <see cref="IBatchedLogEventSink"/> to send log event batches to. Batches and empty
    /// batch notifications will not be sent concurrently. When the <see cref="PeriodicBatchingSink"/> is disposed,
    /// it will dispose this object if possible.</param>
    /// <param name="options">Options controlling behavior of the sink.</param>
    public PeriodicBatchingSink(IBatchedLogEventSink batchedSink, PeriodicBatchingSinkOptions options)
    {
        _targetSink = CreateSink(batchedSink, true, options);
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
    {
        _inheritanceApi = true;
        _targetSink = CreateSink(this, false, new PeriodicBatchingSinkOptions
        {
            BatchSizeLimit = batchSizeLimit,
            Period = period,
            EagerlyEmitFirstEvent = true,
            QueueLimit = null
        });
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
    {
        _inheritanceApi = true;
        _targetSink = CreateSink(this, false, new PeriodicBatchingSinkOptions
        {
            BatchSizeLimit = batchSizeLimit,
            Period = period,
            EagerlyEmitFirstEvent = true,
            QueueLimit = queueLimit == NoQueueLimit ? null : queueLimit
        });
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
        if (!_inheritanceApi || CanInclude(logEvent))
            _targetSink.Emit(logEvent);
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
        ((IDisposable)_targetSink).Dispose();
        GC.SuppressFinalize(this);
    }

#if FEATURE_ASYNCDISPOSABLE
        /// <inheritdoc/>
        public async ValueTask DisposeAsync()
        {
            await ((IAsyncDisposable)_targetSink).DisposeAsync();
            if (_inheritanceApi)
                Dispose(true);

            GC.SuppressFinalize(this);
        }
#endif
    
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
