using System;
using System.Collections.Concurrent;
using System.Configuration;
using NServiceBus.Features;
using NServiceBus.Logging;
using NServiceBus.Transport;

public class SimpleMonitoringFeature : Feature
{
    public SimpleMonitoringFeature()
    {
        EnableByDefault();
    }

    protected override void Setup(FeatureConfigurationContext context)
    {
        const double thresholdDefault = 15D;
        var thresholdValue = Convert.ToDouble(ConfigurationManager.AppSettings["NServiceBus/Extensions/LongRunningMessages/WarningThresholdInSeconds"]);
        var threshold = TimeSpan.FromSeconds(thresholdValue == 0D ? thresholdDefault : thresholdValue);

        LogManager.GetLogger(nameof(TrackProcessingDurationBehavior)).InfoFormat("WarningThresholdInSeconds: {0}", threshold);

        var messages = new ConcurrentDictionary<IncomingMessage, DateTime>();
        var instance = new TrackProcessingDurationBehavior(messages, threshold);

        context.Container.RegisterSingleton(instance);
        context.Pipeline.Register(nameof(TrackProcessingDurationBehavior), typeof(TrackProcessingDurationBehavior), "Reports long running messages");
        context.RegisterStartupTask(new ReportLongRunningMessagesTask(messages, threshold, threshold));
    }
}