# Serilog.Sinks.PeriodicBatching [![Build status](https://ci.appveyor.com/api/projects/status/w2agqyd8rn0jur9y?svg=true)](https://ci.appveyor.com/project/serilog/serilog-sinks-periodicbatching) [![NuGet Version](http://img.shields.io/nuget/v/Serilog.Sinks.periodicbatching.svg?style=flat)](https://www.nuget.org/packages/Serilog.Sinks.periodicbatching/)

A wrapper for Serilog sinks that asynchronously emits events in batches, useful when logging to a slow and/or remote target.

> [!IMPORTANT]
> Serilog 4.x and later versions support batching natively. New projects should use Serilog's `IBatchedLogEventSink` and
> `WriteTo.Sink(IBatchedLogEventSink)`, not this package which is now only maintained for compatibility reasons.

### Updating for Serilog v4+

First, update your _Serilog_ package reference to the latest published version.

This example is from [Serilog.Sinks.Postgresql.Alternative](https://github.com/serilog-contrib/Serilog.Sinks.Postgresql.Alternative/commit/dd282cdd01c694ed586052a023e653d11dda62d1). Old code:

```csharp
var batchingOptions = new PeriodicBatchingSinkOptions()
{
    BatchSizeLimit = postgresOptions.BatchSizeLimit,
    Period = postgresOptions.Period,
    QueueLimit = postgresOptions.QueueLimit
};

var batchingSink = new PeriodicBatchingSink(new PostgreSqlSink(postgresOptions), batchingOptions);
return sinkConfiguration.Sink(batchingSink, restrictedToMinimumLevel, levelSwitch);
```

New code:

```csharp
var batchingOptions = new BatchingOptions()
{
    BatchSizeLimit = postgresOptions.BatchSizeLimit,
    BufferingTimeLimit = postgresOptions.Period,
    QueueLimit = postgresOptions.QueueLimit
};

return sinkConfiguration.Sink(
    new PostgreSqlSink(postgresOptions), batchingOptions, restrictedToMinimumLevel, levelSwitch);
```

When you're done, don't forget to remove the _Serilog.Sinks.PeriodicBatching_ package dependency.

### Getting started

Sinks that, for performance reasons, need to emit events in batches, can be implemented using `PeriodicBatchingSink`
from this package.

First, install the package into your Sink project:

```
dotnet add package Serilog.Sinks.PeriodicBatching
```

Then, instead of implementing Serilog's `ILogEventSink`, implement `IBatchedLogEventSink` in your sink class:

```csharp
class ExampleBatchedSink : IBatchedLogEventSink
{
    public async Task EmitBatchAsync(IEnumerable<LogEvent> batch)
    {
        foreach (var logEvent in batch)
            Console.WriteLine(logEvent);
    }
    
    public Task OnEmptyBatchAsync() { }
}
```

Finally, in your sink's configuration method, construct a `PeriodicBatchingSink` that wraps your batched sink:

```csharp
public static class LoggerSinkExampleConfiguration
{
    public static LoggerConfiguration Example(this LoggerSinkConfiguration loggerSinkConfiguration)
    {
        var exampleSink = new ExampleBatchedSink();
        
        var batchingOptions = new PeriodicBatchingSinkOptions
        {
            BatchSizeLimit = 100,
            Period = TimeSpan.FromSeconds(2),
            EagerlyEmitFirstEvent = true,
            QueueLimit = 10000
        };
        
        var batchingSink = new PeriodicBatchingSink(exampleSink, batchingOptions);
        
        return loggerSinkConfiguration.Sink(batchingSink);
    }
}
```
