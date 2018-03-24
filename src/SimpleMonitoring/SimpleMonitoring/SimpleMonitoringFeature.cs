using System;
using System.Collections.Concurrent;
using System.Configuration;
using System.Globalization;
using NServiceBus;
using NServiceBus.Features;
using NServiceBus.Logging;
using NServiceBus.Pipeline;

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
        var thresholdValue = Convert.ToDouble(ConfigurationManager.AppSettings["NServiceBus/SimpleMonitoring/LongRunningMessages/WarningThresholdInSeconds"], CultureInfo.InvariantCulture);
        var threshold = TimeSpan.FromSeconds(thresholdValue == 0D ? thresholdDefault : thresholdValue);
        var interval = TimeSpan.FromTicks(threshold.Ticks / 2);

        LogManager.GetLogger(LoggerName).InfoFormat("WarningThresholdInSeconds: {0}", threshold);

        var messages = new ConcurrentDictionary<TransportMessage, DateTime>();
        var instance = new TrackProcessingDurationBehavior(messages, threshold);

        var container = context.Container;
        container.RegisterSingleton(instance);
        container.ConfigureComponent<ReportLongRunningMessagesTask>(f => new ReportLongRunningMessagesTask(messages, threshold, interval), DependencyLifecycle.SingleInstance);

        context.Pipeline.Register<Registration>();

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
