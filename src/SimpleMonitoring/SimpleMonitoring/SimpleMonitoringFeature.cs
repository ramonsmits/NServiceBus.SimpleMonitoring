using System;
using System.Collections.Concurrent;
using System.Configuration;
using NServiceBus.Features;
using NServiceBus.Logging;

public class SimpleMonitoringFeature : Feature
{
    public SimpleMonitoringFeature()
    {
        EnableByDefault();
    }

    protected override void Setup(FeatureConfigurationContext context)
    {
        const double thressholdDefault = 15D;
        var thressholdValue = Convert.ToDouble(ConfigurationManager.AppSettings["NServiceBus/Extensions/LongRunningMessages/WarningThressholdInSeconds"]);
        var thresshold = TimeSpan.FromSeconds(thressholdValue == 0D ? thressholdDefault : thressholdValue);

        LogManager.GetLogger(nameof(TrackProcessingDurationBehavior)).InfoFormat("WarningThressholdInSeconds: {0}", thresshold);

        var messages = new ConcurrentDictionary<string, DateTime>();
        var instance = new TrackProcessingDurationBehavior(messages, thresshold);

        context.Container.RegisterSingleton(instance);
        context.Pipeline.Register(nameof(TrackProcessingDurationBehavior), typeof(TrackProcessingDurationBehavior), "Reports long running messages");
        context.RegisterStartupTask(new ReportLongRunningMessagesTask(messages, thresshold, thresshold));
    }
}