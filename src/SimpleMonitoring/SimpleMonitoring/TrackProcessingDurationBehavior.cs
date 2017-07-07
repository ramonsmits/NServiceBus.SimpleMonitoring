using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using NServiceBus.Logging;
using NServiceBus.Pipeline;

class TrackProcessingDurationBehavior : Behavior<ITransportReceiveContext>
{
    readonly ILog Log = LogManager.GetLogger(nameof(TrackProcessingDurationBehavior));
    readonly ConcurrentDictionary<string, DateTime> Messages;
    readonly TimeSpan Thresshold;

    public TrackProcessingDurationBehavior(ConcurrentDictionary<string, DateTime> messages, TimeSpan thresshold)
    {
        Messages = messages;
        Thresshold = thresshold;
    }

    public override async Task Invoke(ITransportReceiveContext context, Func<Task> next)
    {
        var id = context.Message.MessageId;
        var start = DateTime.UtcNow;

        try
        {
            Messages.TryAdd(id, start);
            await next().ConfigureAwait(false);
        }
        finally
        {
            Messages.TryRemove(id, out start);
            var duration = DateTime.UtcNow - start;
            Log.DebugFormat("Message {0} processing duration: {1}", id, duration);
            if (duration > Thresshold) Log.WarnFormat("Processing duration {0} larger than allowed thresshold {1}.", duration, Thresshold);
        }
    }
}