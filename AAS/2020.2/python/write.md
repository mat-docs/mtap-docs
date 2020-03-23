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


## Basic samples of **Write**
The following chapters demonstrate the simple usage of Advanced Streams through basic samples, covering all the bare-minimum steps to implement Telematry Data, Telemetry Samples and Event **write** to Kafka or Mqtt streams.\
The [full source code of the samples is here](./src).
First of all you need to configure the [dependencies](./src/TDataWrite.py#L33-L66)
```python
"""Setup details"""
# Populate these constants with the correct values for your project.
DEPENDENCY_SERVER_URI = 'http://10.228.4.9:8180/api/dependencies'
DEPENDENCY_GROUP = 'dev'
KAFKA_IP = '10.228.4.22:9092'
TOPIC_NAME = 'samples_test_topic'

frequency = 100
"""Create a dependency client"""
dependency_client = HttpDependencyClient(DEPENDENCY_SERVER_URI, DEPENDENCY_GROUP)

"""Create Atlas configurations"""
atlas_configuration_client = AtlasConfigurationClient(dependency_client)
atlas_configuration = AtlasConfiguration({"Chassis":
    ApplicationGroup(groups={"State":
        ParameterGroup(parameters={"vCar:Chassis":
            AtlasParameter(name="vCar")})})})

atlas_configuration_id = atlas_configuration_client.put_and_identify_atlas_configuration(atlas_configuration)

"""Create Dataformat"""
parameter: DataFeedParameter = DataFeedParameter(identifier="vCar:Chassis", aggregates_enum=[Aggregates.avg])
parameters: List[DataFeedParameter] = [parameter]
feed = DataFeedDescriptor(frequency=frequency, parameters=parameters)

feed_name = ""
data_format = DataFormat({feed_name: feed})

data_format_client = DataFormatClient(dependency_client)
data_format_id = data_format_client.put_and_identify_data_format(data_format)

"""Create a Kafka client"""
client = KafkaStreamClient(kafka_address=KAFKA_IP,
                            consumer_group=DEPENDENCY_GROUP)
```

#### SSL connection

To connect to your Kafka broker through https using your SSL certificates, you must use provide the following configuration details to *KafkaStreamClient* constructor:
```python
ssl_config = {"security.protocol": "SSL",
			  "ssl.ca.location": "\\certificates\\ca-cert",
			  "ssl.certificate.location": "\\certificates\\certificate.pem",
			  "ssl.key.location": "\\certificates\\key.pem",
			  "ssl.key.password": "password"}

client = KafkaStreamClient(kafka_address=KAFKA_IP,
                            consumer_group=DEPENDENCY_GROUP,
							producer_properties=ssl_config)
							```

#### SSL connection

To connect to your Kafka broker through https using your SSL certificates, you must provide the following configuration details to *KafkaStreamClient* constructor:
```python
ssl_config = {"security.protocol": "SSL",
			  "ssl.ca.location": "\\certificates\\ca-cert",
			  "ssl.certificate.location": "\\certificates\\certificate.pem",
			  "ssl.key.location": "\\certificates\\key.pem",
			  "ssl.key.password": "password"}

client = KafkaStreamClient(kafka_address=KAFKA_IP,
                            consumer_group=DEPENDENCY_GROUP,
							producer_properties=ssl_config)
							```

The dependency_client is used to handle requests for AtlasConfigurations and DataFormats. You must provide an URI for this service. 
The data_format_client handles the data formats through the dependency_client for the given group name.
DataFormat is required when writing to stream, as it is used to define the structre of the data and data_format_id is used to retrieve dataformat from the dataFormatClient.

AtlasConfigurationId is needed only if you want to display your data in Atlas10.

[Open the output topic](./src/TDataWrite.py#L68-L69) using the preferred client (KafkaStreamClient or MqttStreamClient) and the topicName.
```python
output: SessionTelemetryDataOutput = None
with client.open_output_topic(TOPIC_NAME) as output_topic:
	...
```

[Create a SessionTelemetryDataOutput](./src/TDataWrite.py#L71-L73) and configure session output [properties](./src/TDataWrite.py#L75-L84).
```python
try:
    output = SessionTelemetryDataOutput(output_topic=output_topic,
                                        data_format_id=data_format_id,
                                        data_format_client=data_format_client)

    output.session_output.add_session_dependency(
        DependencyTypes.atlas_configuration, atlas_configuration_id)
    output.session_output.add_session_dependency(
        DependencyTypes.data_format, data_format_id)

    output.session_output.session_state = StreamSessionState.Open
    output.session_output.session_start = datetime.utcnow()
    output.session_output.session_identifier = "test_" + str(datetime.utcnow())
    output.session_output.session_details = {"test_session": "sample test session details"}
    output.session_output.send_session()

	....


except Exception as e:
    print(e)
    if output is not None:
        output.session_output.session_state = StreamSessionState.Truncated
finally:
    if output is not None:
        output.session_output.send_session()
```

Open the session within a Try Except block and handle sesseion status sending as shown above.
You must add data_format_id and atlas_configuration_id to session dependencies to be able to use them during the streaming session.

If you want to use Protobuf encoded messages instead of JSON, specify to *use_protobuf* when creating the SessionTelemetryDataOutput:

```python
output = SessionTelemetryDataOutput(output_topic=output_topic,
                                    data_format_id=data_format_id,
                                    data_format_client=data_format_client,
                                    use_protobuf=True)
```

### Telemetry Data

[Bind the feed to **output.data_output**](./src/TDataWrite.py#L86) by its name. You can use the default feedname or use a custom one.
```python
output_feed: TelemetryDataFeedOutput = output.data_output.bind_default_feed()
```

You will need **TelemetryData** to write to the output. In this example we [generate some random TelemetryData](./src/TDataWrite.py#L88-L91) for the purpose of demonstration.
```python
data: TelemetryData = output_feed.make_telemetry_data(samples=10, epoch=to_telemetry_time(datetime.utcnow()))
data = generate_data(data, frequency)
```

[send](./src/TDataWrite.py#L94) the telemetry data.
```python
output_feed.send(data)
```

### Telemetry Samples
You will need **TelemetrySamples** to write to the output. In this example we [generate some random telemetrySamples](./src/TSamplesWrite.py#L92-L96) for the purpose of demonstration.
```python
telemetry_samples = generate_samples(sample_count=10, session_start=datetime.utcnow(), parameter_id="vCar:Chassis", frequency=frequency)
```

[Bind the feed to **output.samples_output**](./src/TSamplesWrite.py#L98-L99) by its name. You can use the default feedname or use a custom one.
```python
output_feed: TelemetrySamplesFeedOutput = output.samples_output.bind_feed(feed_name="")
```

[Send Samples](./src/TSamplesWrite.py#L101).
```python
output_feed.send(telemetry_samples)
```

### Telemetry Events

You will need **Event** to write to the output. In this example we [use some test Event data](./src/EventWrite.py#L60) for the purpose of demonstration.
```python
event = Event("testEvent", 1, 1, "testing", [1.0, 2.0])
```

We simply [send](./src/TDataWrite.py#L63) the Event data through the **event_output***
```python
output.events_output.send(event)
```

Once you sent all your data, don't forget to [set the session state to closed](./src/TDataWrite.py#L95) 
```python
output.session_output.session_state = StreamSessionState.Closed
```

and [send the session details](./src/TDataWrite.py#L100-L102) or do it in the finally block as recommended above.
```python
output.SessionOutput.SendSession(); // send session
```
