using System;
using System.Collections.Concurrent;
using System.Configuration;
using NServiceBus;
using NServiceBus.Features;
using NServiceBus.Logging;
using NServiceBus.Pipeline;

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

        var messages = new ConcurrentDictionary<TransportMessage, DateTime>();
        var instance = new TrackProcessingDurationBehavior(messages, threshold);

        context.Container.RegisterSingleton(instance);
        context.Pipeline.Register(nameof(TrackProcessingDurationBehavior), typeof(TrackProcessingDurationBehavior), "Reports long running messages");

        RegisterStartupTask<ReportLongRunningMessagesTask>();
    }

    public class Registration :
        RegisterStep
    {
        public Registration()
            : base(
                stepId: nameof(TrackProcessingDurationBehavior),
                behavior: typeof(TrackProcessingDurationBehavior),
                description: "Logs a warning if a handler take more than a specified time")
        {
            InsertBefore(WellKnownStep.ProcessingStatistics);
        }
    }
}