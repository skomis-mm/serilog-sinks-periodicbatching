#if !WAITABLE_TIMER

using Serilog.Debugging;
using Serilog.Sinks.PeriodicBatching;
using System;
using System.Threading;
using Xunit;

namespace Serilog.Tests.Sinks.PeriodicBatching
{
    public class PortableTimerTests
    {
        [Fact]
        public void WhenItStartsItWaitsUntilHandled_OnDispose()
        {
            bool wasCalled = false;
            using (var timer = new PortableTimer(delegate { Thread.Sleep(50); wasCalled = true; }))
            { 
                timer.Start(TimeSpan.Zero);
                Thread.Sleep(10);
            }

            Assert.True(wasCalled);
        }

        [Fact]
        public void WhenWaitingShouldCancel_OnDispose()
        {
            bool wasCalled = false;
            bool writtenToSelflog = false;

            SelfLog.Enable(_ => writtenToSelflog = true);

            using (var timer = new PortableTimer(delegate { Thread.Sleep(50); wasCalled = true; }))
            {
                timer.Start(TimeSpan.FromMilliseconds(10));
            }

            Thread.Sleep(100);

            Assert.False(wasCalled, "tick handler was called");
            Assert.False(writtenToSelflog, "message was written to SelfLog");
        }

        [Fact]
        public void WhenDisposedWillThrow_OnStart()
        {
            bool wasCalled = false;
            var timer = new PortableTimer(delegate { wasCalled = true; });
            timer.Start(TimeSpan.FromMilliseconds(100));
            timer.Dispose();

            Assert.False(wasCalled);
            Assert.Throws<ObjectDisposedException>(() => timer.Start(TimeSpan.Zero));
        }
    }
}

#endif
