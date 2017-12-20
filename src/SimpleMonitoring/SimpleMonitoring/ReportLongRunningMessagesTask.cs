using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using NServiceBus;
using NServiceBus.Features;
using NServiceBus.Logging;
using NServiceBus.Transport;

class ReportLongRunningMessagesTask : FeatureStartupTask
{
    readonly TimeSpan Threshold;
    readonly TimeSpan Interval;
    readonly ILog Log = LogManager.GetLogger(nameof(ReportLongRunningMessagesTask));

    CancellationTokenSource cancellationTokenSource;
    CancellationToken cancellationToken;
    Task loopTask;
    readonly ConcurrentDictionary<IncomingMessage, DateTime> Messages;

    public ReportLongRunningMessagesTask(ConcurrentDictionary<IncomingMessage, DateTime> messages, TimeSpan threshold, TimeSpan interval)
    {
        Threshold = threshold;
        Interval = interval;
        Messages = messages;
    }

    protected override Task OnStart(IMessageSession session)
    {
        cancellationTokenSource = new CancellationTokenSource();
        cancellationToken = cancellationTokenSource.Token;

        loopTask = Task.Run(Loop);
        return Task.FromResult(0);
    }

    protected override Task OnStop(IMessageSession session)
    {
        cancellationTokenSource.Cancel();
        return loopTask;
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
                    Log.WarnFormat("Took more time ({0:g}) than the interval ({1:g}). Not delaying", duration, Interval);
                    continue;
                }

                var next = Next(start);
                var delay = next - now;
                Log.DebugFormat("Delaying {0:g}", delay);
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
        var threshold = now - Threshold;
        foreach (var i in Messages)
        {
            if (i.Value < threshold)
            {
                var duration = now - i.Value;
                Log.WarnFormat("Message '{0}' is running for {1} which is longer than {2}.", i.Key, duration, Threshold);
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