using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using NServiceBus;
using NServiceBus.Features;
using NServiceBus.Logging;
using NServiceBus.Transport;

class ReportLongRunningMessagesTask(ConcurrentDictionary<IncomingMessage, DateTime> messages, TimeSpan threshold, TimeSpan interval) : FeatureStartupTask
{
    readonly ILog Log = LogManager.GetLogger(SimpleMonitoringFeature.LoggerName);
    readonly bool IsDebugEnabled = LogManager.GetLogger(SimpleMonitoringFeature.LoggerName).IsDebugEnabled;

    CancellationTokenSource cancellationTokenSource;
    Task loopTask;

    protected override Task OnStart(IMessageSession session, CancellationToken cancellationToken)
    {
        cancellationTokenSource = new CancellationTokenSource();
        loopTask = Task.Run(Loop);
        return Task.CompletedTask;
    }

    protected override Task OnStop(IMessageSession session, CancellationToken cancellationToken)
    {
        cancellationTokenSource?.Cancel();
        return loopTask ?? Task.CompletedTask;
    }

    async Task Loop()
    {
        var token = cancellationTokenSource!.Token;
        while (!token.IsCancellationRequested)
        {
            try
            {
                var start = DateTime.UtcNow;
                Invoke();

                var now = DateTime.UtcNow;
                var duration = now - start;

                if (duration > interval)
                {
                    Log.WarnFormat("Took more time ({0:g}) than the interval ({1:g}). Not delaying", duration, interval);
                    continue;
                }

                var next = Next(start);
                var delay = next - now;
                if (IsDebugEnabled) Log.DebugFormat("Delaying {0:g}", delay);
                await Task.Delay(delay, token).ConfigureAwait(false);
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

    void Invoke()
    {
        var now = DateTime.UtcNow;
        var cutoff = now - threshold;
        foreach (var i in messages)
        {
            if (i.Value < cutoff)
            {
                var duration = now - i.Value;
                Log.WarnFormat("Message '{0}' is already running for '{1:g}', which is larger than the threshold '{2:g}'.", i.Key.MessageId, duration, threshold);
            }
        }
    }

    DateTime Next(DateTime last)
    {
        var now = DateTime.UtcNow;
        var next = last;
        do
        {
            next += interval;
        } while (next < now);
        return next;
    }
}
