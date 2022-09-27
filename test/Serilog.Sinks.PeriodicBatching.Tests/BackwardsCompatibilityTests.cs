#if FEATURE_ASYNCDISPOSABLE

using System.Threading.Tasks;
using Serilog.Sinks.PeriodicBatching.Tests.Support;
using Xunit;

namespace Serilog.Sinks.PeriodicBatching.Tests;

public class BackwardsCompatibilityTests
{
    [Fact]
    public async Task LegacySinksAreDisposedWhenLoggerIsDisposedAsync()
    {
        var sink = new LegacyDisposeTrackingSink();
        var logger = new LoggerConfiguration().WriteTo.Sink(sink).CreateLogger();
        await logger.DisposeAsync();
        Assert.True(sink.IsDisposed);
    }
}

#endif
