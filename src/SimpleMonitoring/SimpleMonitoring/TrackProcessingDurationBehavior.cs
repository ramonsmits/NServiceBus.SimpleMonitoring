using System;
using System.Collections.Concurrent;
using NServiceBus;
using NServiceBus.Logging;
using NServiceBus.Pipeline;
using NServiceBus.Pipeline.Contexts;

class TrackProcessingDurationBehavior : IBehavior<IncomingContext>
{
    readonly ILog Log = LogManager.GetLogger(SimpleMonitoringFeature.LoggerName);
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
            Log.DebugFormat("Message '{0}' total processing duration: '{1:g}'", instance.Id, duration);
            if (duration > Threshold) Log.WarnFormat("Message '{0}' total processing duration ({1:g}) is larger than the threshold '{2:g}'.", instance.Id, duration, Threshold);
        }
    }
}