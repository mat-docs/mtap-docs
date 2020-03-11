# ![logo](/Media/branding.png) Atlas Advanced Streams

### Table of Contents
<!--ts-->
- [**Introduction**](../README.md)<br>
- [**Python Samples**](../python/README.md)<br>
- [**C# Samples**](README.md)<br>
  - [Read](read.md#basic-samples-of-read)
    - [TData](read.md#telemetry-data)
    - [TSamples](read.md#telemetry-samples)
    - [Events](read.md#events)
    - [Buffers](read.md#buffers)
  - [Write](write.md#basic-samples-of-write)
    - [TData](write.md#telemetry-data)
    - [TSamples](write.md#telemetry-samples)
    - [Events](write.md#events)
  - [Model execution](model.md#model-sample)
  - [Advanced Samples](advanced.md#advanced-samples)
- [**Python Samples**](../python/README.md)<br>
<!--te-->

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

# Requirements
You need to install the following Nuget packages from MAT source

- MAT.OCS.Streaming
- MAT.OCS.Streaming.Kafka
- MAT.OCS.Streaming.Mqtt
- MAT.OCS.Streaming.Codecs.Protobuf
