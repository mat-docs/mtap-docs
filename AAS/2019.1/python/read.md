# ![logo](/Media/branding.png) Atlas Advanced Stream

### Table of Contents
- [**Introduction**](../README.md)<br>
- [**C# Samples**](README.md)<br>
- **Python Samples**(./python/README.md)<br>
  - [Samples project](./src)
  - [Read](read.md)
    - [TData](read.md#telemetry-data)
    - [Tamples](read.md#telemetry-samples)
  - [Write](write.md#basic-samples)
    - [TData](write.md#telemetry-data)
    - [Tamples](write.md#telemetry-samples)

## Basic samples
Basic samples demonstrate the simple usage of Advanced Streams, covering all the bare-minimum steps to implement Telematry Data and Telemetry Samples read and write to and from Kafka or Mqtt streams.

## Read
First of all you need to configure the [dependencies]
(https://github.com/McLarenAppliedTechnologies/mat.ocs.streaming.python.samples/blob/develop/src/TDataRead.py#L11-L20)
```python
DEPENDENCY_SERVER_URI = 'http://10.228.4.9:8180/api/dependencies'
DEPENDENCY_GROUP = 'dev'
KAFKA_IP = '10.228.4.22:9092'
TOPIC_NAME = 'samples_test_topic'

dependency_client = HttpDependencyClient(DEPENDENCY_SERVER_URI, DEPENDENCY_GROUP)
data_format_client = DataFormatClient(dependency_client)
kafka_client = KafkaStreamClient(kafka_address=KAFKA_IP,
                                    consumer_group=DEPENDENCY_GROUP)
```

The dependency_client is used to handle requests for AtlasConfigurations and DataFormats. You must provide an URI for this service. 
The data_format_client handles the data formats through the dependency_client for the given group name.

Create a stream pipeline using the kafka_client and the TOPIC_NAME. Stream the messages [.Into your handler method](https://github.com/McLarenAppliedTechnologies/mat.ocs.streaming.python.samples/blob/develop/src/TDataRead.py#L36)
```python
pipeline: StreamPipeline = kafka_client.stream_topic(TOPIC_NAME).into(stream_input_handler)
```

Within your [stream_input_handler method](https://github.com/McLarenAppliedTechnologies/mat.ocs.streaming.python.samples/blob/develop/src/TDataRead.py#L29)
```python
def stream_input_handler(stream_id: str) -> StreamInput:
    print("Streaming session: " + stream_id)
```
Create a [SessionTelemetryDataInput](https://github.com/McLarenAppliedTechnologies/mat.ocs.streaming.python.samples/blob/develop/src/TDataRead.py#L31-L32) with the actual stream id and the dataFormatClient 
```python
telemetry_input = SessionTelemetryDataInput(stream_id=stream_id, data_format_client=data_format_client)
```

### Read Telemetry Data
In this example we [bind the **data_input** to the handler method](https://github.com/McLarenAppliedTechnologies/mat.ocs.streaming.python.samples/blob/develop/src/TDataRead.py#L33) using the default feed and simply [print out some details](https://github.com/McLarenAppliedTechnologies/mat.ocs.streaming.python.samples/blob/develop/src/TDataRead.py#L22-L27) about the incoming data.

```python
def print_data(sender, event_args: TelemetryDataFeedEventArgs):
    tdata: TelemetryData = event_args.buffer.get_first()
    print(len(tdata.parameters))
    print('tdata for {0} with length {1} received'.format(
        str(event_args.message_origin.stream_id),
        str(len(tdata.time))))

telemetry_input.data_input.bind_default_feed("").data_buffered += print_data

```

### Read Telemetry Samples
In this example we [bind the **samples_input** to the handler method](https://github.com/McLarenAppliedTechnologies/mat.ocs.streaming.python.samples/blob/develop/src/TSamplesRead.py#L34) and simply [print out some details](https://github.com/McLarenAppliedTechnologies/mat.ocs.streaming.python.samples/blob/develop/src/TSamplesRead.py#L23-L27) 
```python
def print_samples(sender, event_args: TelemetryEventArgs):
    s: TelemetrySamples = event_args.data
    print('tsamples for {0} with {1} parameters received'.format(
        str(event_args.message_origin.stream_id),
        str(len(s.parameters.keys()))))

telemetry_input.samples_input.autobind_feeds += print_samples
```

You can optionally handle the stream_finished event.
```python
telemetry_input.stream_finished += lambda x, y: print('Stream finished')
```