# ![logo](/Media/branding.png) Atlas Advanced Streams

### Table of Contents
<!--ts-->
- [**Introduction**](../README.md)<br>
- [**Python Samples**](README.md)<br>
  - [Read](read.md#basic-samples-of-read)
    - [TData](read.md#telemetry-data)
    - [TSamples](read.md#telemetry-samples)
  - [Write](write.md#basic-samples-of-write)
    - [TData](write.md#telemetry-data)
    - [TSamples](write.md#telemetry-samples)
  - [Model execution](model.md#model-sample)
- [**C# Samples**](../csharp/README.md)<br>
<!--te-->

## Basic samples of Read
The following chapters demonstrate the simple usage of Advanced Streams through basic samples, covering all the bare-minimum steps to implement Telematry Data, Telemetry Samples and Event **reads** from Kafka or Mqtt streams.\
The [full source code of the samples is here](./src).

### Configurations and dependencies

First of all you need to configure the [dependencies](./src/TDataRead.py#L11-L20)
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

### Stream pipeline

Create a stream pipeline using the kafka_client and the TOPIC_NAME. Stream the messages [.Into your handler method](./src/TDataRead.py#L36)
```python
pipeline: StreamPipeline = kafka_client.stream_topic(TOPIC_NAME).into(stream_input_handler)
```
- **Into**(input_factory: Callable[[str], StreamInput]):*\
Binds the specified input factory into an StreamPipeline, which provides stream control and represents the disposable network resource. The factory is invoked for each child stream within a topic to allow a new instance of user processing code.
 - **IntoMultiple**(input_factories: List[Callable[[str], StreamInput]]):*\
Binds multiple input factories into an StreamPipeline, which provides stream control and represents the disposable network resource. Each factory is invoked for each child stream within a topic to allow a new instance of user processing code.

The stream pipeline (SteamPipeline impl) will run a separate thread and starts polling messages from the Kafka topic, based on the topicName provided. If a new stream session is found on the Kafka topic, the above mentioned stream handler method will be invoked.
The stream pipeline exposes several public method and statuses for pipelen management, monitoring and error handling:

#### Pipeline management methods

 - **drain()**:\
Not yet implemented.

 - **stop()**:\
Stops the pipeline by detaching inputs without reading further messages.
 
  - **wait_until_connected(seconds)**:\
Not yet implemented.
 
  - **wait_until_idle(seconds)**:\
Not yet implemented.

  - **wait_until_first_stream(seconds)**:\
Not yet implemented.

 - **wait_until_stopped(seconds)**:\
Not yet implemented.

#### Pipeline statuses

 - **is_connected**:\
Gets whether the pipeline is connected to an upstream source.
 - **is_idle**:\
Gets whether the pipeline is idle.
 - **is_stopped**:\
Gets whether the pipeline is stopped.
 - **is_faulted**:\
Gets when the pipeline has stopped due to an unhandled exception.
 - **has_first_stream**:\
Gets whether at least one stream has started.

#### Pipeline exception/error handling

As the pipeline runs on a separate thread, the exceptions may occur are not being propageted to the main thread.
You can check for errors through the *IsFaulted* status. In case of exceptions they would be stored in the pipelines *exceptions* property.
 - **exception_raised**:\
Exception raised event. Fired when an exception is caught during message processing. You can subscribe to this event to be notified when an error occurs.

### Stream session input
Within your [stream_input_handler method](./src/TDataRead.py#L29)
```python
def stream_input_handler(stream_id: str) -> StreamInput:
    print("Streaming session: " + stream_id)
```
Create a [SessionTelemetryDataInput](./src/TDataRead.py#L31-L32) with the actual stream id and the dataFormatClient 
```python
telemetry_input = SessionTelemetryDataInput(stream_id=stream_id, data_format_client=data_format_client)
```

### Telemetry Data
In this example we [bind the **data_input** using the default feed and subscribe to the data_buffered event with the handler method](./src/TDataRead.py#L33) and simply [print out some details](./src/TDataRead.py#L22-L27) about the incoming data.

```python
def print_data(sender, event_args: TelemetryDataFeedEventArgs):
    tdata: TelemetryData = event_args.buffer.get_first()
    print(len(tdata.parameters))
    print('tdata for {0} with length {1} received'.format(
        str(event_args.message_origin.stream_id),
        str(len(tdata.time))))

telemetry_input.data_input.bind_default_feed("").data_buffered += print_data

```

### Telemetry Samples
In this example we [bind the **samples_input** by subscribing to the autobind_feeds event with the handler method](./src/TSamplesRead.py#L34) and simply [print out some details](./src/TSamplesRead.py#L23-L27) 
```python
def print_samples(sender, event_args: TelemetryEventArgs):
    s: TelemetrySamples = event_args.data
    print('tsamples for {0} with {1} parameters received'.format(
        str(event_args.message_origin.stream_id),
        str(len(s.parameters.keys()))))

telemetry_input.samples_input.autobind_feeds += print_samples
```

### Telemetry Events
In this example we [subscribe to the **events_input**'s data_buffered event with the handler method](./src/EventRead.py#L30) and simply [print out some details](./src/EventRead.py#L22-L24) 
```python
    def print_event(sender, event_args: EventsEventArgs):
        event: Event = event_args.buffer.get_first()
        print(event.status)

telemetry_input.events_input.data_buffered += print_event
```

You can optionally handle the stream_finished event.
```python
telemetry_input.stream_finished += lambda x, y: print('Stream finished')
```
