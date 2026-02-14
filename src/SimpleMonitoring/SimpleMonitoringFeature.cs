using System;
using System.Collections.Concurrent;
using System.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NServiceBus;
using NServiceBus.Features;
using NServiceBus.Logging;
using NServiceBus.Pipeline;
using NServiceBus.Transport;

public class SimpleMonitoringFeature : Feature
{
    static internal string LoggerName = "NServiceBus.SimpleMonitoring";
    static readonly TimeSpan DefaultWarningThreshold = TimeSpan.FromSeconds(15);

    protected override void Setup(FeatureConfigurationContext context)
    {
        var logger = LogManager.GetLogger(LoggerName);
        var thresholdValue = Convert.ToDouble(ConfigurationManager.AppSettings["NServiceBus/SimpleMonitoring/LongRunningMessages/WarningThresholdInSeconds"]);

        TimeSpan threshold;

        if (thresholdValue != 0D)
        {
            threshold = TimeSpan.FromSeconds(thresholdValue);
            logger.Info("Warning threshold read from AppSettings.");
        }
        else if (context.Settings.TryGet<Properties>(out var properties))
        {
            threshold = properties.WarningThreshold;
            logger.Info($"Warning threshold read via {nameof(SimpleMonitoringConfigurationExtension.ReportLongRunningMessages)} API.");
        }
        else
        {
            threshold = DefaultWarningThreshold;
            logger.Info($"No warning threshold set, using default.");
        }

        logger.InfoFormat("Warning threshold: {0}", threshold);

        var messages = new ConcurrentDictionary<IncomingMessage, DateTime>();

        var services = context.Services;
        services.AddSingleton(f => new TrackProcessingDurationBehavior(messages, threshold));
        context.Pipeline.Register(nameof(TrackProcessingDurationBehavior), serviceProvider => serviceProvider.GetRequiredService<TrackProcessingDurationBehavior>(), "Reports long running messages");
        context.RegisterStartupTask(new ReportLongRunningMessagesTask(messages, threshold, threshold));
    }
}

