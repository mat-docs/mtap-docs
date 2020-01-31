# ![logo](/Media/branding.png) Telemetry Analytics API

### Table of Contents
- **Introduction**<br>
  - [Changelog](#change-log)
- [**Installation**](docs/Installation.md)<br>
- [**Getting started**](docs/GettingStarted.md)<br>
- [**Authorization**](docs/Authorization.md)<br>
- [**Querying Metadata**](docs/Metadata.md)<br>
- [**Consuming Data**](docs/ConsumingData.md)<br>
- [**Consuming Events**](docs/ConsumingEvents.md)<br>
- [**Session Versions**](docs/SessionVersions.md)<br>

The Telemetry Analytics API (TAP API) service provides programmatic access to persisted stream data. The REST API provides access to session metadata, telemetry data and model results. Additionally, the API exposes data analytic features such as data/metadata masking and filtering, aggregation and grouping. The service is configured via a REST API.

![](../Media/TapiDiagram.png)

# Change log

- [Added endpoints for query Event definitions metadata](docs/Metadata.md#events)
- [Added endpoints for query Events data](docs/ConsumingEvents.md)