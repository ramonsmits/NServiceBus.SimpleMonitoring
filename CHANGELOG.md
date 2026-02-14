# Changelog

## [Unreleased]

Target: NServiceBus 10.x | .NET 10.0

- Support NServiceBus 10.x
- Target net10.0
- Remove `EnableByDefault()`, require explicit `EnableFeature` via configuration API
- Code cleanup: primary constructor, modern argument validation, remove dead AssemblyInfo
- Remove redundant `Microsoft.SourceLink.GitHub` (built into .NET 8+ SDK)
- Switch from GitVersion to MinVer for tag-based versioning
- Remove legacy `packages.config`
- Add justfile for build, pack, and publish workflows
- Add GitHub Actions CI workflow

### Dependencies

- NServiceBus [10.0.0, 11.0.0)
- System.Configuration.ConfigurationManager 10.0.3
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

[Unreleased]: https://github.com/ramonsmits/NServiceBus.SimpleMonitoring/compare/5.0.0...HEAD
[5.0.0]: https://github.com/ramonsmits/NServiceBus.SimpleMonitoring/compare/4.0.0...5.0.0
[4.0.0]: https://github.com/ramonsmits/NServiceBus.SimpleMonitoring/compare/3.1.0...4.0.0
[3.1.0]: https://github.com/ramonsmits/NServiceBus.SimpleMonitoring/compare/3.0.0...3.1.0
[3.0.0]: https://github.com/ramonsmits/NServiceBus.SimpleMonitoring/compare/2.0.0...3.0.0
[2.0.0]: https://github.com/ramonsmits/NServiceBus.SimpleMonitoring/compare/1.0.0...2.0.0
[1.0.0]: https://github.com/ramonsmits/NServiceBus.SimpleMonitoring/releases/tag/1.0.0
