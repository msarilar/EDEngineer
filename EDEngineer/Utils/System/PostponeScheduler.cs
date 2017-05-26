using System;
using System.Threading;
using Timer = System.Timers.Timer;

namespace EDEngineer.Utils.System
{
    /// <summary>
    /// Used to avoid calling repeatedly some method in a short interval when a single call would have been enough.
    /// When the GUI triggers an event many times, the postpone scheduler will execute only one method call as long
    /// as a short time passed between calls.
    /// </summary>
    public class PostponeScheduler : IDisposable
    {
        private readonly Timer timer;

        public PostponeScheduler(Action action, double delayMilliseconds = 300)
        {
            timer = new Timer(delayMilliseconds)
            {
                AutoReset = false
            };

            var context = SynchronizationContext.Current;
            timer.Elapsed += (o, e) => context.Post(s => action(), null);
        }

        public void Schedule()
        {
            timer.Stop();
            timer.Start();
        }

        public void Dispose()
        {
            timer.Dispose();
        }
    }
}
