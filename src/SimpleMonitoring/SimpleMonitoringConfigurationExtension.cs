using System;
using NServiceBus.Configuration.AdvancedExtensibility;

namespace NServiceBus;

public static class SimpleMonitoringConfigurationExtension
{
    /// <summary>
    /// Set a duration threshold after which a Warning Log Entry is written.
    /// </summary>
    public static void ReportLongRunningMessages(this EndpointConfiguration instance, TimeSpan warningThreshold)
    {
        ArgumentNullException.ThrowIfNull(instance);
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(warningThreshold, TimeSpan.Zero);
        var properties = new Properties
        {
            WarningThreshold = warningThreshold,
        };
        instance.GetSettings().Set(properties);
        instance.EnableFeature<SimpleMonitoringFeature>();
    }
}
