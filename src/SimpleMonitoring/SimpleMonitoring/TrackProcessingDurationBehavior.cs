using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using NServiceBus.Logging;
using NServiceBus.Pipeline;
using NServiceBus.Transport;

class TrackProcessingDurationBehavior : Behavior<ITransportReceiveContext>
{
    readonly ILog Log = LogManager.GetLogger(nameof(TrackProcessingDurationBehavior));
    readonly ConcurrentDictionary<IncomingMessage, DateTime> Messages;
    readonly TimeSpan Threshold;

    public TrackProcessingDurationBehavior(ConcurrentDictionary<IncomingMessage, DateTime> messages, TimeSpan threshold)
    {
        Messages = messages;
        Threshold = threshold;
    }

    public override async Task Invoke(ITransportReceiveContext context, Func<Task> next)
    {
        var instance = context.Message;
        var start = DateTime.UtcNow;

        try
        {
            Messages.TryAdd(instance, start);
            await next().ConfigureAwait(false);
        }
        finally
        {
            Messages.TryRemove(instance, out start);
            var duration = DateTime.UtcNow - start;
            Log.DebugFormat("Message '{0}' processing duration: {1:g}", instance.MessageId, duration);
            if (duration > Threshold) Log.WarnFormat("Message '{0}' processing duration {0:g} larger than allowed threshold {1:g}.", duration, Threshold);
        }
    }
}