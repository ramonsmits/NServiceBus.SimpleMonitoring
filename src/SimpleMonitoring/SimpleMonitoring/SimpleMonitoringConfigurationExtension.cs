using System;
using NServiceBus.Configuration.AdvancedExtensibility;

namespace NServiceBus
{
    public static class SimpleMonitoringConfigurationExtension
    {
        /// <summary>
        /// Set a duration thresshold after which a Warning Log Entry is written. 
        /// </summary>
        public static void ReportLongRunningMessages(this EndpointConfiguration instance, TimeSpan warningThreshold)
        {
            if (instance == null) throw new ArgumentNullException(nameof(instance));
            if (warningThreshold <= TimeSpan.Zero) throw new ArgumentOutOfRangeException(nameof(warningThreshold), warningThreshold, "Must be larger than 0");
            var properties = new Properties
            {
                WarningThreshold = warningThreshold,
            };
            var settings = instance.GetSettings();
            settings.Set<Properties>(properties);
            instance.EnableFeature<SimpleMonitoringFeature>();
        }
    }
}
