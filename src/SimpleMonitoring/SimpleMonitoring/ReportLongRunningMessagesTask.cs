using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using NServiceBus;
using NServiceBus.Features;
using NServiceBus.Logging;

class ReportLongRunningMessagesTask : FeatureStartupTask
{
    readonly TimeSpan Thresshold;
    readonly TimeSpan Interval;
    readonly ILog Log = LogManager.GetLogger(nameof(ReportLongRunningMessagesTask));

    CancellationTokenSource cancellationTokenSource;
    CancellationToken cancellationToken;
    Task looptask;
    readonly ConcurrentDictionary<string, DateTime> Messages;

    public ReportLongRunningMessagesTask(ConcurrentDictionary<string, DateTime> messages, TimeSpan thresshold, TimeSpan interval)
    {
        Thresshold = thresshold;
        Interval = interval;
        Messages = messages;
    }

    protected override Task OnStart(IMessageSession session)
    {
        cancellationTokenSource = new CancellationTokenSource();
        cancellationToken = cancellationTokenSource.Token;

        looptask = Task.Run(Loop);
        return Task.FromResult(0);
    }

    protected override Task OnStop(IMessageSession session)
    {
        cancellationTokenSource.Cancel();
        return looptask;
    }

    async Task Loop()
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                var start = DateTime.UtcNow;
                await Invoke().ConfigureAwait(false);

                var now = DateTime.UtcNow;
                var duration = now - start;

                if (duration > Interval)
                {
                    Log.WarnFormat("Took more time ({0}) than the interval ({1}). Not delaying", duration, Interval);
                    continue;
                }

                var next = Next(start);
                var delay = next - now;
                Log.DebugFormat("Delaying {0}", delay);
                await Task.Delay(delay, cancellationToken).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                return;
            }
            catch (Exception ex)
            {
                Log.Error("Loop", ex);
            }
        }
    }

    Task Invoke()
    {
        var now = DateTime.UtcNow;
        var thresshold = now - Thresshold;
        foreach (var i in Messages)
        {
            if (i.Value < thresshold)
            {
                var duration = now - i.Value;
                Log.WarnFormat("Message '{0}' is running for {1} which is longer than {2}.", i.Key, duration, Thresshold);
            }
        }

        return Task.FromResult(0);
    }

    DateTime Next(DateTime last)
    {
        var now = DateTime.UtcNow;
        var next = last;
        do
        {
            next += Interval;
        } while (next < now);
        return next;
    }
}