# ![logo](/Media/branding.png) Atlas Advanced Stream

### Table of Contents
- [**Introduction**](../README.md)<br>
- [**Python Samples**](../python/README.md)<br>
- [**C# Samples**](README.md)<br>
  - [Read](read.md)
    - [TData](read.md#telemetry-data)
    - [TSamples](read.md#telemetry-samples)
    - [Events](read.md#events)
  - [Write](write.md)
    - [TData](write.md#telemetry-data)
    - [TSamples](write.md#telemetry-samples)
    - [Events](write.md#events)
  - [Advanced Samples](advanced.md)

# Introduction
This API provides infrastructure for streaming data around the ATLAS technology platform.

Using this API, you can:
- Subscribe to streams of engineering values - no ATLAS recorder required
- Inject parameters and aggregates back into the ATLAS ecosystem
- Build up complex processing pipelines that automatically process new sessions as they are generated

With support for Apache Kafka, the streaming API also offers:
- Late-join capability - clients can stream from the start of a live session
- Replay of historic streams
- Fault-tolerant, scalable broker architecture

The [full source code of the samples is here](./src).

# API Documentation
- [**0.10.x**](https://mclarenappliedtechnologies.github.io/mat.atlas.advancedstreams-docs/0.10.0/api/index.html)<br>
- [**0.11.x (MTAP 2019.1.x)**](https://mclarenappliedtechnologies.github.io/mat.atlas.advancedstreams-docs/0.11.0/api/index.html)

# Knowledgebase
Be sure to look at our support knowledgebase on Zendesk: https://mclarenappliedtechnologies.zendesk.com/

# Scope
This pre-release version of the API demonstrates the event-based messaging approach, for sessions and simple telemetry data.

# Requirements
You need to install the following Nuget packages from MAT source

- MAT.OCS.Streaming
- MAT.OCS.Streaming.Kafka
- MAT.OCS.Streaming.Mqtt
- MAT.OCS.Streaming.Codecs.Protobuf
