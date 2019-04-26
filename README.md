# NServiceBus.SimpleMonitoring

Adds some additional diagnostics to NServiceBus 7, 6 and 5.

## Version compatibility

| NServiceBus | NServiceBus.SimpleMonitoring |
| ----------- | ---------------------------- |
| v5.x        | v1.x                         |
| v6.x        | v2.x                         |
| v7.x        | v3.x                         |

Please note that there might be versions targeting other NServiceBus versions. [Please check the Releases for all versions.](https://github.com/ramonsmits/nservicebus.simplemonitoring/releases) or [check the root of the  `master` branch of the repository](https://github.com/ramonsmits/nservicebus.simplemonitoring).

## Use cases

Additional processing duration information about processed messages is written to the log. A threshold can be configured so that a **Warning** log entry will be written indicating that a message is still processing. For example, a message should take less then 10 seconds to be processed, if it takes longer a **Warning** log entry is written while the message is still being processed. Messages that hang an endpoint can now easily be identified. When a message completes processing its total processing duration is written to the log with level **Debug** but a message that took more then the threshold will also be logged with log level **Warning**.

## Log event examples

Log event written for every completed message:

> DEBUG NServiceBus.SimpleMonitoring Message '000526b2-44cd-4d6d-9aa3-a8ad0126e6e6' total processing duration: '0:00:00,1349758'

Log event written for completed messages where the duration exceeds the threshold:

> WARN  NServiceBus.SimpleMonitoring Message '6625296c-50b1-4f07-ac90-a8ad0127304a' total processing duration (0:00:23,0370282) is larger than the threshold '0:00:15'.

Log event written every interval for each message currently being processed but exceeding the threshold

> WARN  NServiceBus.SimpleMonitoring Message '6625296c-50b1-4f07-ac90-a8ad0127304a' is already running for '0:00:20,1372179' which is larger than the thresshold '0:00:15'.

Note that the formatting of the log events is different per used logging framework but the message part is the same.

## Installation

Install the Nuget package [NServiceBus.SimpleMonitoring](https://www.nuget.org/packages/NServiceBus.SimpleMonitoring)

## Configuration

### Set the Warning threshold

*The default warning threshold is 15 seconds.*

The threshold value can only be specified as an AppSetting. Add a setting as shown in the following sample:

```xml
  <appSettings>
    <add key="NServiceBus/SimpleMonitoring/LongRunningMessages/WarningThresholdInSeconds" value="180"/>
  </appSettings>
```

Note that the above snippet will report messages with log level Warning if their processing duration is over 180 seconds (3 minutes).

### Show durations for all messages

Message processing durations are always logged with log level Debug however the default NServiceBus log level is Info since version 5. [Please set the NServiceBus logging log level to Debug if this is required](https://docs.particular.net/nservicebus/logging/).
