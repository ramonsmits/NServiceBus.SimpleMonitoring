using System;
using System.Collections.Concurrent;
using System.Configuration;
using NServiceBus.Features;
using NServiceBus.Logging;
using NServiceBus.Transport;

public class SimpleMonitoringFeature : Feature
{
    static internal string LoggerName = "NServiceBus.SimpleMonitoring";

    public SimpleMonitoringFeature()
    {
        EnableByDefault();
    }

    protected override void Setup(FeatureConfigurationContext context)
    {
        const double thresholdDefault = 15D;
        var thresholdValue = Convert.ToDouble(ConfigurationManager.AppSettings["NServiceBus/SimpleMonitoring/LongRunningMessages/WarningThresholdInSeconds"]);
        var threshold = TimeSpan.FromSeconds(thresholdValue == 0D ? thresholdDefault : thresholdValue);

        LogManager.GetLogger(LoggerName).InfoFormat("WarningThresholdInSeconds: {0}", threshold);

        var messages = new ConcurrentDictionary<IncomingMessage, DateTime>();

        var container = context.Container;

        container.ConfigureComponent<TrackProcessingDurationBehavior>(f => new TrackProcessingDurationBehavior(messages, threshold), NServiceBus.DependencyLifecycle.SingleInstance);
        context.Pipeline.Register(nameof(TrackProcessingDurationBehavior), typeof(TrackProcessingDurationBehavior), "Reports long running messages");
        context.RegisterStartupTask(new ReportLongRunningMessagesTask(messages, threshold, threshold));
    }
}