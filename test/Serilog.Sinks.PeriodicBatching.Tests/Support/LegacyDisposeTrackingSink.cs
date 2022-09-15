using System;

#pragma warning disable CS0618

namespace Serilog.Sinks.PeriodicBatching.Tests.Support
{
    public class LegacyDisposeTrackingSink : PeriodicBatchingSink
    {
        public bool IsDisposed { get; private set; }

        public LegacyDisposeTrackingSink()
            : base(10, TimeSpan.FromMinutes(1))
        {
        }

        protected override void Dispose(bool disposing)
        {
            IsDisposed = true;
        }
    }
}
