# ![logo](/Media/branding.png) Telemetry Analytics API

### Table of Contents
- **Introduction**<br>
  - [Changelog](#change-log)
- [**Installation**](docs/Installation.md)<br>
- [**Getting started**](docs/GettingStarted.md)<br>
- [**Authorization**](docs/Authorization.md)<br>
- [**Querying Metadata**](docs/Metadata.md)<br>
- [**Consuming Data**](docs/ConsumingData.md)<br>
- [**Session Versions**](docs/SessionVersions.md)<br>

The Telemetry Analytics API (TAP API) service provides programmatic access to persisted stream data. The REST API provides access to session metadata, telemetry data and model results. Additionally, the API exposes data analytic features such as data/metadata masking and filtering, aggregation and grouping. The service is configured via a REST API.

![](../Media/TapiDiagram.png)

# Change log

- Session version is no longer nullable.
- [Renamed 'items' optional metadata query parameter to 'details'](docs/Metadata.md#optional-parameters)
- [Metadata filter now supports 'contain'](docs/Metadata.md#filter-types)
- [Lap filter for data request now supports multiple laps](docs/ConsumingData.md#optional-parameters-for-both-base-urls)
- [Sessions now return the 'label'](docs/Metadata.md#query-all-available-sessions)
- Session delete now supports optional parameter 'streamId'
- [Laps now expose trigger source](docs/Metadata.md#query-all-laps-from-a-given-session)