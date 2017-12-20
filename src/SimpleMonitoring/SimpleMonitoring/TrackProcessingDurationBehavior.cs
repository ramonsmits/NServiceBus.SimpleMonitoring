using System;
using System.Collections.Concurrent;
using NServiceBus;
using NServiceBus.Logging;
using NServiceBus.Pipeline;
using NServiceBus.Pipeline.Contexts;

class TrackProcessingDurationBehavior : IBehavior<IncomingContext>
{
    readonly ILog Log = LogManager.GetLogger(nameof(TrackProcessingDurationBehavior));
    readonly ConcurrentDictionary<TransportMessage, DateTime> Messages;
    readonly TimeSpan Threshold;

    public TrackProcessingDurationBehavior(ConcurrentDictionary<TransportMessage, DateTime> messages, TimeSpan threshold)
    {
        Messages = messages;
        Threshold = threshold;
    }

    public void Invoke(IncomingContext context, Action next)
    {
        var instance = context.PhysicalMessage;
        var start = DateTime.UtcNow;

        try
        {
            Messages.TryAdd(instance, start);
            next();
        }
        finally
        {
            Messages.TryRemove(instance, out start);
            var duration = DateTime.UtcNow - start;
            Log.DebugFormat("Message {0} processing duration: {1}", instance.Id, duration);
            if (duration > Threshold) Log.WarnFormat("Message {0} processing duration {1} larger than allowed threshold {2}.", instance.Id, duration, Threshold);
        }
    }
}