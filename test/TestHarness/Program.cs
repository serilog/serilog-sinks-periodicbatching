using System.Diagnostics;
using Serilog;
using Serilog.Debugging;
using Serilog.Events;
using Serilog.Sinks.PeriodicBatching;
// ReSharper disable PossibleMultipleEnumeration

// ReSharper disable PossibleLossOfFraction

SelfLog.Enable(Console.Error);

const int producers = 3;
const int countPerProducer = 1000;
var producerInterval = TimeSpan.FromMilliseconds(90);

const int batchSize = 50;
var batchInterval = TimeSpan.FromMilliseconds(100);
var maximumBufferDelay = TimeSpan.FromMilliseconds(80);

var sinkFailAfterEvents = (int?)50;

Console.WriteLine($"Producers will take {countPerProducer * producerInterval} to complete");
Console.WriteLine($"Producers will generate a total of {countPerProducer * producers} events");
Console.WriteLine($"Consumer will take {countPerProducer * producers / batchSize * batchInterval} to write batches");

var options = new PeriodicBatchingSinkOptions
{
    EagerlyEmitFirstEvent = false,
    QueueLimit = null,
    BatchSizeLimit = batchSize,
    Period = maximumBufferDelay
};

var batchedSink = new EmptyBatchedSink(batchInterval, sinkFailAfterEvents);

var logger = new LoggerConfiguration()
    .WriteTo.Sink(new PeriodicBatchingSink(batchedSink, options))
    .CreateLogger();

var threads = Enumerable.Range(0, producers)
    .Select(id => new Thread(() => RunProducer(id, logger, producerInterval, countPerProducer)))
    .ToArray();

var sw = Stopwatch.StartNew();

foreach (var thread in threads)
{
    thread.Start();
}

foreach (var thread in threads)
{
    thread.Join();
}

Console.WriteLine($"All producers done in {sw.Elapsed}");

await logger.DisposeAsync();

Console.WriteLine($"All batches processed in {sw.Elapsed}");
Console.WriteLine($"Sink saw {batchedSink.EventCount} events in {batchedSink.BatchCount} batches");
Console.WriteLine($"Largest batch was {batchedSink.MaxBatchSize}; smallest was {batchedSink.MinBatchSize}");

static void RunProducer(int id, ILogger logger, TimeSpan interval, int count)
{
    for (var i = 0; i < count; ++i)
    {
        logger.Information("Hello number {N} from producer {Id}!", i, id);
        Thread.Sleep(interval);
    }
}

class EmptyBatchedSink(TimeSpan flushDelay, int? failAfterEvents): IBatchedLogEventSink
{
    bool _failureDone;
    
    public int BatchCount { get; private set; }
    public int EventCount { get; private set; }
    public int MaxBatchSize { get; private set; }
    public int MinBatchSize { get; private set; } = int.MaxValue;
    
    public async Task EmitBatchAsync(IEnumerable<LogEvent> batch)
    {
        if (failAfterEvents is { } f && EventCount >= f && !_failureDone)
        {
            _failureDone = true;
            throw new Exception("This batch failed.");
        }
        
        BatchCount++;
        EventCount += batch.Count();
        MinBatchSize = Math.Min(MinBatchSize, batch.Count());
        MaxBatchSize = Math.Max(MaxBatchSize, batch.Count());
        
        await Task.Delay(flushDelay);
    }

    public Task OnEmptyBatchAsync()
    {
        return Task.CompletedTask;
    }
}
