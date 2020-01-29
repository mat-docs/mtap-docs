# ![logo](/Media/branding.png) Motorsport Telemetry Analytics Platform Documentation

## Introduction

The McLaren Telemetry Analytics Platform (MTAP) is designed to work with and alongside ATLAS, but interoperate with a much wider ecosystem of time-series products and sources of data.

It includes two core capabilities:

**ATLAS Advanced Streams** provides engineering data streaming in and out of the ATLAS ecosystem in an open data format, with a supporting library in C# and Python. Key use cases include:

- Exporting live data to other systems
- Ingesting data streams from outside ATLAS
- Modernized, scalable model execution pipelines

Includes connectivity services for wideband and SQL Race databases, and an ATLAS 10 recorder, and .NET Core/Framework nuget package for software integration.

**Telemetry Analytics API** extends the ATLAS and SQL Race ecosystem with data search and aggregation capabilities across sessions.

- InfluxDB – a popular open source time-series database – is integrated to provide a high-level query language and powerful search and aggregation capabilities. This product is a good complement to the core session storage capabilities provided by SQL Race, with broad community support in tools like Grafana – for dashboarding.

- A web services query layer provides a common interface across InfluxDB and SQL Race: ideal for ad-hoc tooling and visualization.
These capabilities work together to create an integrated experience: ATLAS Advanced Streams carries data from the ECU, models and other racing applications, and the Telemetry Analytics API includes a service to feed this data live into InfluxDB for search and dashboarding.

## Table of Contents

- [**Atlas Advanced Streams**](/AAS/README.md)<br>
- [**Influx Writer**](/InfluxWriter/README.md)<br>
- [**Parameter Mapping**](/ParameterMapping/README.md)<br>
- [**Replay Service**](ReplayService/README.md)<br>
- [**SignalR**](/SignalR/README.md)<br>
- [**Identity Service**](/IdentityService/README.md)<br>
- [**Telemetry Analytics API**](/TAPApi/README.md)<br>
- [**TAP Helm charts**](/Helm/README.md)<br>
