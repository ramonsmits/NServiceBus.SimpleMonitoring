# NServiceBus.SimpleMonitoring

Adds some additional diagnostics to NServiceBus 10, 9, 8, 7, 6 and 5.

## Version compatibility

| NServiceBus | NServiceBus.SimpleMonitoring |
| ----------- | ---------------------------- |
| v5.x        | v1.x                         |
| v6.x        | v2.x                         |
| v7.x        | v3.x                         |
| v8.x        | v4.x                         |
| v9.x        | v5.x                         |
| v10.x       | v6.x                         |

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

### Via Nuget

Install the Nuget package [NServiceBus.SimpleMonitoring](https://www.nuget.org/packages/NServiceBus.SimpleMonitoring)

### Zero configuration and deployment

Alternatively, download the nuget package, extract the DLL and drop-in the DLL in the endpoint installation path and restart the endpoint. Assembly scanning ensures this module will get auto loaded and enabled using default configuration values.

## Migrating to NServiceBus 11

In NServiceBus 10.x, this feature is still auto-enabled via assembly scanning. NServiceBus 11 will remove assembly scanning of features, so `EnableByDefault()` will stop working. To ensure a smooth upgrade, explicitly call `ReportLongRunningMessages()` on your `EndpointConfiguration`:

```c#
endpointConfiguration.ReportLongRunningMessages(TimeSpan.FromMinutes(3));
```

## Configuration

### Set the Warning threshold

*The default warning threshold is 15 seconds.*


The threshold value can be configured via code or via `IConfiguration`. The code API takes precedence.

Via code:
```c#
endpointConfiguration.ReportLongRunningMessages(TimeSpan.FromMinutes(3));
```

Via `IConfiguration` (e.g. `appsettings.json`):

```json
{
  "NServiceBus": {
    "SimpleMonitoring": {
      "WarningThresholdInSeconds": 180
    }
  }
}
```

> [!NOTE]
> `IConfiguration` support requires hosting via [`NServiceBus.Extensions.Hosting`](https://docs.particular.net/nservicebus/hosting/extensions-hosting), which registers `IConfiguration` in the dependency injection container. The threshold is read automatically as a fallback when the code API is not used.

### Show durations for all messages

Message processing durations are always logged with log level Debug however the default NServiceBus log level is Info since version 5. [Please set the NServiceBus logging log level to Debug if this is required](https://docs.particular.net/nservicebus/logging/).
