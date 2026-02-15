# Changelog

## [6.0.0] - 2026-02-15

Target: NServiceBus 10.x | .NET 10.0

Major version targeting NServiceBus 10.x on .NET 10.0. AppSettings-based configuration has been replaced with `IConfiguration` support. The code configuration API (`ReportLongRunningMessages`) remains unchanged.

### Breaking changes

- Drop `AppSettings`-based configuration (`NServiceBus/SimpleMonitoring/LongRunningMessages/WarningThresholdInSeconds`) — use the code API or `IConfiguration` instead (a5525e2)
- Remove `System.Configuration.ConfigurationManager` dependency (a5525e2)
- Feature no longer auto-enables via `EnableByDefault()` — must be explicitly enabled via the `ReportLongRunningMessages` configuration API (b0dec34)

### Improvements

- Support NServiceBus 10.x (a5525e2)
- Add `IConfiguration` support (`NServiceBus:SimpleMonitoring:WarningThresholdInSeconds`) as a fallback when the code API is not used (requires `NServiceBus.Extensions.Hosting`)
- Faster detection of long-running messages — background monitoring interval now checks at half the warning threshold
- Fix resource leak: `CancellationTokenSource` is now properly disposed on stop

### Internal

- Target net10.0 (a5525e2)
- Code cleanup: primary constructors, modern argument validation, remove dead AssemblyInfo (b0dec34)
- Remove redundant `Microsoft.SourceLink.GitHub` (built into .NET 8+ SDK) (53cc0b3)
- Switch from GitVersion to MinVer for tag-based versioning (c56247f)
- Remove legacy `packages.config` (a5525e2)
- Remove orphaned `.nuspec` file
- Add justfile for build, pack, and publish workflows (5656c69)
- Add GitHub Actions CI workflow (3d7a6bf)
- Enable nullable reference types
- Seal all classes

### Dependencies

- NServiceBus [10.0.0, 11.0.0)
- MinVer 7.0.0

## [5.0.0] - 2024-06-14

Target: NServiceBus 9.x | .NET 8.0

- Support NServiceBus 9.x
- Enable `EmbedUntrackedSources`

### Dependencies

- System.Configuration.ConfigurationManager 8.0.0
- Microsoft.SourceLink.GitHub 8.0.0
- GitVersion.MsBuild 5.12.0

## [4.0.0] - 2022-12-10

Target: NServiceBus 8.x

- Support NServiceBus 8.x
- Optimize `Log.DebugFormat` invocations by caching `IsDebugEnabled`

## [3.1.0] - 2022-12-09

- Enable SourceLink for deterministic builds
- Add code configuration API (`ReportLongRunningMessages`)
- Zero configuration support (uses default 15s threshold)

## [3.0.0] - 2019-04-26

Target: NServiceBus 7.x

- Support NServiceBus 7.x
- Resolve `System.Configuration` usage differences between .NET Core and .NET Framework

## [2.0.0] - 2022-12-10

Target: NServiceBus 6.x

- Support NServiceBus 6.x
- Support concurrent processing of multiple transport messages with same message ID

## [1.0.0] - 2022-12-10

Target: NServiceBus 5.x

- Initial release

[6.0.0]: https://github.com/ramonsmits/NServiceBus.SimpleMonitoring/compare/5.0.0...6.0.0
[5.0.0]: https://github.com/ramonsmits/NServiceBus.SimpleMonitoring/compare/4.0.0...5.0.0
[4.0.0]: https://github.com/ramonsmits/NServiceBus.SimpleMonitoring/compare/3.1.0...4.0.0
[3.1.0]: https://github.com/ramonsmits/NServiceBus.SimpleMonitoring/compare/3.0.0...3.1.0
[3.0.0]: https://github.com/ramonsmits/NServiceBus.SimpleMonitoring/compare/2.0.0...3.0.0
[2.0.0]: https://github.com/ramonsmits/NServiceBus.SimpleMonitoring/compare/1.0.0...2.0.0
[1.0.0]: https://github.com/ramonsmits/NServiceBus.SimpleMonitoring/releases/tag/1.0.0
