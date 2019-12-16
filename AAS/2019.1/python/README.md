# ![logo](/Media/branding.png) Atlas Advanced Stream

### Table of Contents
- [**Introduction**](../README.md)<br>
- [**C# Samples**](../csharp/README.md)<br>
- **Python Samples**<br>
  - [Samples project](./src)
  - [Read](read.md#basic-samples)
    - [TData](read.md#telemetry-data)
    - [TSamples](read.md#telemetry-samples)
  - [Write](write.md#basic-samples)
    - [TData](write.md#telemetry-data)
    - [TSamples](write.md#telemetry-samples)
  - [Model](models.md#model-sample)

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

# Knowledgebase
Be sure to look at our support knowledgebase on Zendesk: https://mclarenappliedtechnologies.zendesk.com/

# Scope
This pre-release version of the API demonstrates the event-based messaging approach, for sessions and simple telemetry data.

## Requirements
You need to install the following PIP packages from [MAT source](https://artifactory-elb.core.mat.production.matsw.com/artifactory/pypi-local/mat.ocs.streaming/)

* MAT.OCS.Streaming==1.5.3.11rc0

pip install --extra-index-url https://artifactory-elb.core.mat.production.matsw.com/artifactory/api/pypi/pypi-virtual/simple/ mat.ocs.streaming==1.5.3.11rc0

## API Documentation
There is a set of required steps that needs to be done, before you could actually start streaming.

 * Kafka broker address:
   * A valid and existing host name where a Kafka service is running.
 * Kafka topic name:
   * An existing topic on the Kafka broker.
 * Kafka announce topic name:
   * If your topic name is 'test_topic', then you must have a 'test_topic_announce' as well.
 * Kafka consumer group:
   * A unique group name for your consumers within the same logical group.
 * Dependency service URI:
   * Dependency service is used to store/provide dependencies, such as configuration details, data format information etc.
 * Dependency client:
   * Dependency client is used to connect to the dependency service, using the provided dependency service URI and group name.
 * Dataformat client:
   * Dataformat client is used to put/get data formats of your streamed data. For quicker access it is cacheing data formats.
 * Kafka stream client is used to connect to a Kafka service, using the provided Kafka borker address and consumer group.

#### Definitions:
##### Data Format:
Describes data feeds in terms of their parameters, frequency, and stream encoding. A Data format can contain multiple feeds, each with different name.
##### Feed:
A Feed must have a unique name and it also contains a list of parameters and a given frequency for all of them. By default it is 100hz. These are set/bind when producing the messages, and as a consumer you can bind your input message handler for a feed by its name. This could be useful if you are interested in only specific data, within a topic/stream/session.
##### Atlas Configuration:
A tree-like structure that defines the display structure of your data, using the following levels in the tree:

```python
AtlasConfiguration({"config_name":
        ApplicationGroup(groups={"group_name":
            ParameterGroup(parameters={"parameter_name":
                AtlasParameter(name="parameter_display_name")})})})
```
Of course there can be multiple of each, hence the dictionary based tree-like structure.
Atlas configuration needs to be set and put to the dependency service only if you want to display your data in Atlas10.
##### Buffer:
Telemetry Data type messages are stored in a buffer and can be accessed immediately or later, by accessing the buffer.
#### Concept of consuming streams:
There can be multiple streams simultenaously in a Kafka topic, but they can be handled separately by their stream id.
The concept is to stream a topic into a stream input handler, that receives the uniqie stream id as an input parameter.
This stream input handler is invoked when a new stream is identified in the Kafka topic and it is responsible to handle the input messages.
AAS stream messages are logically coupled within a session and you can access them by creating a SessionTelemetryDataInput object, prodiving the stream id and the DataFormat client for data parsing.
There are different messages types, and they have their own way to handle them. They can be accessed through the SessionTelemetryDataInput object.
The general concept behind accessing different type of messages is similar to C# event subscriptions. Based on the message types, they can be stored in a buffer and accessed through it, or accessed directly, but some messages types (Session, Lap, Event) are not accessible directly, but they fire specific events that can be handled.
All these message related events can be subscribed with the "+=" operator, and they pass 2 parameters to the handler method: the sender object and an event_args object, that is specific to the message type.

#### Methods and subscriptable events for accessing input messages by their types:

##### Session:
 * state_changed: Fired when the session state changed, compare to the previous state of the session.
 * dependencies_changed: Fired when the session dependencies changed, compare to the previous dependencies of the session.
 * label_changed: Fired when the session label changed, compare to the previous label of the session.
 * details_changed: Fired when the session details changed, compare to the previous details of the session.
 * identifier_changed: Fired when the session identifier changed, compare to the previous identifier of the session.
 * sources_changed: Fired when the session sources changed, compare to the previous sources of the session.
 
##### Telemetry Samples:
 * autobind_feeds:
Telemtry Samples (samples_input) input messages are neither separated by feeds nor stored in a buffer, so you can access them by subscribing to the autobind_feeds event

##### Telemetry Data:
 * autobind_feeds:
Telemtry Data (data_input) input messages are separated by feeds and also stored in a buffer, but you can access all of the messages for every feed immediately by subscribing to the autobind_feeds event.
 * bind_default_feed: The default feed is the feed with name "" (empty string) and feed input of default stream can be accessed by invoking the bind_default_feed method.
 * bind_feed: You can access messages through feed input only for a specific feed name as well by invoking the bind_feed(feed_name="specific feed name") method.
 * data_buffered: It is possible to access the input messages immediately when they were put into the buffer by subscribing to the data_buffered event. It can be chained with both the bind_default_feed and the bind_feed methods:
    * bind_default_feed().data_buffered
    * bind_feed("test_feed").data_buffered
 * buffer: The buffer can directly be accessed through the feed input object too:
```python
feed_input: TelemetryDataFeedInput = telemetry_input.data_input.bind_default_feed()
feed_input_buffer = feed_input.buffer
```
Buffer has a set of method to access its content with or without removing it from the buffer: get_first(), get_last(), read_first(), read_last()
Buffer content can be accessed also for only a given timeframe by invoking the get_within(start, end) or the read_within(start, end) methods.
Triggers can be defined for a buffer, which are used to monitor incoming messages and act based on their specified condition: buffer.add_trigger(condition_func, tigger, trigger_once)
The input messages is tested against the triggers condition function, and if it is true, the tigger will be executed with the input message as its parameter. If trigger once is set to true, the trigger will be executed only for the first time when its condition will be true.
The input messages are being put in the buffer regardless of the triggers.
An example for a trigger that invokes the get_buffer() method, passing the whole buffer object to it only when the current input data's time reached a threshold:
```python
feed_input: TelemetryDataFeedInput = telemetry_input.data_input.bind_feed()
feed_input.buffer.add_trigger(lambda d: any(d.epoch + t > 1555372800000000000 + 38813465000000 for t in d.time), lambda: get_buffer(feed_input.buffer))
```
This could be handy if we want to process the buffer content only if the input reached a given time on the clock when it was recorded.

##### BackFill Data:
 * autobind_feeds:
BackFill Data (backfill_input) input messages are separated by feeds but not stored in a buffer. You can access all of the messages for every feed immediately by subscribing to the autobind_feeds event.
 * bind_default_feed: The default feed is the feed with name "" (empty string) and feed input of default stream can be accessed by invoking the bind_default_feed method.
 * bind_feed: You can access messages through feed input only for a specific feed name as well by invoking the bind_feed(feed_name="specific feed name") method.
 * data_received: It is possible to access the input messages immediately when they are streamed by subscribing to the data_received event. It can be chained with both the bind_default_feed and the bind_feed methods:
    * bind_default_feed().data_received
    * bind_feed("test_feed").data_received

##### Lap:
 * lap_started: Fired when a new lap started.
 * lap_completed: Fired when a lap completed
 * new_fastest_lap

#### Concept of producing streams:

Before starting to upstream data, you need to specify its data format and upload to to the dependency server through the data format client. This is essential for the consumers to be able to parse and understand your data by its structure and format. The data_format_client is used to put_and_identify_data_format the data format and generate a data_format_id for it.

In case the data need to be consumed and displayed by Atlas10, the atlas_configuration of the data structure must be uploaded to to the dependency server through the atlas_configuration_client, using the put_and_identify_atlas_configuration. A unique atlas_configuration_id will be generated by this method.

A Kafka stream client object is used to access/open an output topic by its topic name. 
AAS stream messages are logically coupled within a session and you can upstream them by creating a SessionTelemetryDataOutput object, prodiving the stream id and the output topic object that you have opened and have access to, plus the data_format_id and the data_format_client that is used to parse the output data.
There can be different dependencies for the session_output which need to be set first, just as other properties of the session, like its state, start time, identifier and other session details.
Make sure to send the session message before sending any telemetry data. This will also start a session heartbeat, which sends session message in every 10 seconds.

##### Telemetry Samples:
Telemetry samples messages can be sent using the initally created SessionTelemetryDataOutput object and its samples_output member, by binding to either the default feed or a specific feed. Once bound to a feed simply invoke the send() method, passing your TelemetrySamples object to it.

##### Telemetry Data:
Telemetry data messages can be sent using the initally created SessionTelemetryDataOutput object and its data_output member, by binding to either the default feed or a specific feed. Once bound to a feed simply invoke the send() method, passing your TelemetryData object to it.

##### BackFill Data:
BackFill data messages can be sent using the initally created SessionTelemetryDataOutput object and its backfill_data_output member, by binding to either the default feed or a specific feed. Once bound to a feed simply invoke the send() method, passing your BackFillData object to it.

Make sure to close the streaming session after sending the data.

#### Session linking:
In case you want to process input messages and upstream them, but also would like to forward the session messages, you can easily link them to the upstream topic with the SessionLinker.
Beside your SessionTelemetryDataInput object that is used to stream input messages a SessionTelemetryDataOutput object is also required for the upstream. Simply invoke the link_to_output() method on the telemetry input object, passign your telemetry output object's session_output member to it, and a session identifier transformer method.
```python
telemetry_input.link_to_output(
    session_output=output.session_output,
    identifier_transform=lambda identifier: identifier + "_changed")
```

#### Modelling messages:
If you want to process input messages, applying your business logic models to it and upstream them, you can do it easily. 
Beside your SessionTelemetryDataInput object that is used to stream input messages a SessionTelemetryDataOutput object is also required for the upstream. Bind to an upstream feed. Subscribe your data modelling method to the data_buffered event, process the data by accessing to it through the event_args.buffer.get_last() method and upstream it using the output feed object's send() method.
