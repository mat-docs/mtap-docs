# ![logo](/Media/branding.png) Atlas Advanced Streams

### Table of Contents
<!--ts-->
- [**Introduction**](../README.md)<br>
- [**Python Samples**](README.md)<br>
  - [Read](read.md#basic-samples-of-read)
    - [TData](read.md#telemetry-data)
    - [TSamples](read.md#telemetry-samples)
    - [Buffers](read.md#buffers)
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

#### SSL connection

 - **drain()**:\
Not yet implemented.

To connect to your Kafka broker through https using your SSL certificates, you must use provide the following configuration details to *with_consumer_properties* method:
```python
ssl_config = {"security.protocol": "SSL",
			  "ssl.ca.location": "\\certificates\\ca-cert",
			  "ssl.certificate.location": "\\certificates\\certificate.pem",
			  "ssl.key.location": "\\certificates\\key.pem",
			  "ssl.key.password": "password"}

pipeline: StreamPipeline = kafka_client.stream_topic(TOPIC_NAME).with_consumer_properties(ssl_config).into(stream_input_handler)
```

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

### Buffers

TData and Events messages are getting buffered once polled from the Kafka stream. The following samples show the usage of the buffer and some practical use cases.

#### TData buffer

As you could see in the [TData](read.md#telemetry-data) example, you can subscribe to the *data_buffered* event and receive the polled TData message immediately from the buffer.
You can also create a reference to the buffer and use it or read from it directly, whenever you need to.

```python
buffer = telemetry_input.data_input.bind_feed("").buffer
```

The *TelemetryDataBuffer* type has a few public methods:
 - void put(data):\
Put data into the buffer.
 - TelemetryData get_first():\
Get the oldest item out of the buffer.
 - TelemetryData read_first():\
Read the oldest item in the buffer without removing it.
 - TelemetryData get_last():\
Get the newest item out of the buffer.
 - TelemetryData read_last():\ 
Read the newest item in the buffer without removing it.
 - List[TelemetryData] get_within(start: int, end: int):\
Get data out of the buffer within a specified start and end time
 - List[TelemetryData] read_within(start: int, end: int):\
Read data out of the buffer within a specified start and end time, without removing them from the buffer.

A typical use case could be that you would read the buffer content only when a Lap is completed:
```python
def on_laps_completed(buffer: TelemetryDataBuffer):
	while not buffer._q.empty():
            data: TelemetryData = buffer.get()
	    
	data_within_time_range = buffer.get_within(69226230000000, 69254750000000)
	
 def stream_input_handler(stream_id: str) -> StreamInput:
        print("Streaming session: " + stream_id)
        telemetry_input = SessionTelemetryDataInput(stream_id=stream_id, data_format_client=data_format_client)
        buffer = telemetry_input.data_input.bind_feed("").buffer
        telemetry_input.laps_input.lap_completed += lambda sender, data: on_laps_completed(buffer)
```

You can create your own conditions, for example reaching a specific date time.
Here is a combined example, where you hold a reference to the buffer, but also subscribing to new TData messages and once yout date time condition is met, you read a set of TData from the buffer for a given time frame, using the *GetDataInCompleteWindow*:
```python
def on_data_received(buffer: TelemetryDataBuffer):
	data_within_time_range = buffer.get_within(69226230000000, 69254750000000)
	
 def stream_input_handler(stream_id: str) -> StreamInput:
        print("Streaming session: " + stream_id)
        telemetry_input = SessionTelemetryDataInput(stream_id=stream_id, data_format_client=data_format_client)
        buffer = telemetry_input.data_input.bind_feed("").buffer
	telemetry_input.data_input.bind_feed("").buffer.data_buffered += lambda sender, data: on_data_received(buffer)
```

#### Events buffer

Events buffer uses the base *DataBuffer* implementation, with almost identical set of public methods as the TData's buffer implementation:

- void put(data):\
Put data into the buffer.
 - TelemetryData get_first():\
Get the oldest item out of the buffer.
 - TelemetryData read_first():\
Read the oldest item in the buffer without removing it.
 - TelemetryData get_last():\
Get the newest item out of the buffer.
 - TelemetryData read_last():\ 
Read the newest item in the buffer without removing it.
 - List[TelemetryData] get_within(start: int, end: int):\
Get data out of the buffer within a specified start and end time
 - List[TelemetryData] read_within(start: int, end: int):\
Read data out of the buffer within a specified start and end time, without removing them from the buffer.

The usage is very similar to the TData buffer, you create a reference to the *events_input* buffer:
```csharp
buffer = telemetry_input.events_input.bind_feed("").buffer
```
