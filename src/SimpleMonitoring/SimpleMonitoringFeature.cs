using System;
using System.Collections.Concurrent;
using System.Globalization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NServiceBus;
using NServiceBus.Features;
using NServiceBus.Logging;
using NServiceBus.Transport;

public sealed class SimpleMonitoringFeature : Feature
{
    internal const string LoggerName = "NServiceBus.SimpleMonitoring";
    const string ConfigurationSection = "NServiceBus:SimpleMonitoring";
    const string WarningThresholdKey = "WarningThresholdInSeconds";
    static readonly TimeSpan DefaultWarningThreshold = TimeSpan.FromSeconds(15);

    protected override void Setup(FeatureConfigurationContext context)
    {
        var messages = new ConcurrentDictionary<IncomingMessage, DateTime>();

        context.Services.AddSingleton(serviceProvider =>
        {
            var threshold = ResolveThreshold(context.Settings, serviceProvider.GetService<IConfiguration>());
            return new Properties { WarningThreshold = threshold };
        });

        context.Services.AddSingleton(serviceProvider =>
        {
            var props = serviceProvider.GetRequiredService<Properties>();
            return new TrackProcessingDurationBehavior(messages, props.WarningThreshold);
        });

        context.Pipeline.Register(
            nameof(TrackProcessingDurationBehavior),
            serviceProvider => serviceProvider.GetRequiredService<TrackProcessingDurationBehavior>(),
            "Reports long running messages");

        context.RegisterStartupTask<ReportLongRunningMessagesTask>(serviceProvider =>
        {
            var props = serviceProvider.GetRequiredService<Properties>();
            return new ReportLongRunningMessagesTask(messages, props.WarningThreshold, props.WarningThreshold / 2);
        });
    }

    static TimeSpan ResolveThreshold(NServiceBus.Settings.IReadOnlySettings settings, IConfiguration? configuration)
    {
        var logger = LogManager.GetLogger(LoggerName);
        TimeSpan threshold;

        if (settings.TryGet<Properties>(out var properties))
        {
            threshold = properties.WarningThreshold;
            logger.Info($"Warning threshold read via {nameof(SimpleMonitoringConfigurationExtension.ReportLongRunningMessages)} API.");
        }
        else if (TryReadFromConfiguration(configuration, out threshold))
        {
            logger.Info("Warning threshold read from configuration.");
        }
        else
        {
            threshold = DefaultWarningThreshold;
            logger.Info("No warning threshold set, using default.");
        }

        logger.InfoFormat("Warning threshold: {0}", threshold);
        return threshold;
    }

    static bool TryReadFromConfiguration(IConfiguration? configuration, out TimeSpan threshold)
    {
        threshold = default;
        var value = configuration?.GetSection(ConfigurationSection)?[WarningThresholdKey];
        if (value is null) return false;
        if (!double.TryParse(value, CultureInfo.InvariantCulture, out var seconds) || seconds <= 0) return false;
        threshold = TimeSpan.FromSeconds(seconds);
        return true;
    }
}
