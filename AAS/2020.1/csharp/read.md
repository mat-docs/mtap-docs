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

## Basic samples of Read
The following chapters demonstrate the simple usage of Advanced Streams through basic samples, covering all the bare-minimum steps to implement Telematry Data, Telemetry Samples and Event **reads** from Kafka or Mqtt streams.\
The [full source code of the samples is here](./src).

### Configurations and dependencies

First of all you need to configure the [dependencies](./src/MAT.OCS.Streaming.Samples/Samples/Basic/TData.cs#L60-L63)
```cs
const string brokerList = "localhost:9092"; // The host and port where the Kafka broker is running
const string groupName = "dev"; // The group name
const string topicName = "data_in"; // The existing topic's name in the Kafka broker. The *_annonce topic name must exist too. In this case the data_in_announce
var dependencyServiceUri = new Uri("http://localhost:8180/api/dependencies/"); // The URI where the dependency services are running

var client = new KafkaStreamClient(brokerList); // Create a new KafkaStreamClient for connecting to Kafka broker
var dataFormatClient = new DataFormatClient(new HttpDependencyClient(dependencyServiceUri, groupName)); // Create a new DataFormatClient
```

The [DependencyService](https://mclarenappliedtechnologies.zendesk.com/hc/en-us/articles/115003531373-API-Reference-Dependencies-Service) is used to handle requests for [AtlasConfigurations](https://github.com/mat-docs/mtap-docs/blob/task/TAP-2839/AAS/2020.1/README.md#atlas-configuration) and [DataFormats](https://github.com/mat-docs/mtap-docs/blob/task/TAP-2839/AAS/2020.1/README.md#data-feeds-formats-and-views). You must provide an URI for this service. 
The DataFormatClient handles the data formats through the [DependencyService](https://mclarenappliedtechnologies.zendesk.com/hc/en-us/articles/115003531373-API-Reference-Dependencies-Service) for the given group name.

If you want to connect to MQTT, create a client of MqttStreamClient instead of KafkaStreamClient:
```cs
var client = new MqttStreamClient(new MqttConnectionConfig(brokerList, "userName", "password"));
```
Read more about [Topics, Streams and Sessions here](https://github.com/mat-docs/mtap-docs/blob/task/TAP-2839/AAS/2020.1/README.md#topics-streams-and-sessions).

### Stream pipeline

### Stream pipeline

Create a stream pipeline using the KafkaStreamClient and the topicName. Stream the messages [.Into your handler method](./src/MAT.OCS.Streaming.Samples/Samples/Basic/TData.cs#L68)
```cs
var pipeline = client.StreamTopic(topicName).Into(streamId => // Stream Kafka topic into the handler method
```
 - *IStreamPipeline **Into**(Func<string, IStreamInput> inputFactory):*\
Binds the specified input factory into an IStreamPipeline, which provides stream control and represents the disposable network resource. The factory is invoked for each child stream within a topic to allow a new instance of user processing code.
 - *IStreamPipeline **IntoMultiple**(ICollection<Func<string, IStreamInput>> inputFactories):*\
Binds multiple input factories into an IStreamPipeline, which provides stream control and represents the disposable network resource. Each factory is invoked for each child stream within a topic to allow a new instance of user processing code.

The stream pipeline (ISteamPipeline impl) will run a separate thread and starts polling messages from the Kafka topic, based on the topicName provided. If a new stream session is found on the Kafka topic, the above mentioned stream handler method will be invoked.
The stream pipeline exposes several public method and statuses for pipeline management, monitoring and error handling:

#### SSL connection

To connect to your Kafka broker through https using your SSL certificates, you must provide the following configuration details to *WithConsumerProperties* method:
```cs
var sslConfigurationDetails = new Dictionary<string, string>();
sslConfigurationDetails.Add("security.protocol", "ssl");
sslConfigurationDetails.Add("ssl.ca.location", @"C:\certificates\ca-cert");
sslConfigurationDetails.Add("ssl.certificate.location", @"C:\\certificates\certificate.pem");
sslConfigurationDetails.Add("ssl.key.location", @"C:\certificates\key.pem");
sslConfigurationDetails.Add("ssl.key.password", "password");


var pipeline = client.StreamTopic(topicName).WithConsumerProperties(sslConfigurationDetails).Into(streamId => // Stream Kafka topic into the handler method
```
  
  - **WaitUntilStopped(TimeSpan timeout, CancellationToken ct)**:\
Wait for the pipeline to stop.
  
  - **WaitUntilFirstStream(TimeSpan timeout, CancellationToken ct)**:\
Wait for at least one stream to start. Does not reset after the first stream. Returns true immediately if a stream has already started, even if it has since finished.

 - **WaitUntilStopped(TimeSpan timeout, CancellationToken ct)**:\
Wait for the pipeline to stop.

#### Pipeline statuses

 - **IsConnected**:\
Gets whether the pipeline is connected to an upstream source.
 - **IsStopped**:\
Gets whether the pipeline is stopped.
 - **IsFaulted**:\
Gets when the pipeline has stopped due to an unhandled exception.
 - **HasFirstStream**:\
Gets whether at least one stream has started.

#### Pipeline exception/error handling

As the pipeline runs on a separate thread, the exceptions may occur are not being propageted to the main thread.
You can check for errors through the *IsFaulted* status. In case of exception this would be stored in the pipelines *Exception* property.

#### SSL connection

To connect to your Kafka broker through https using your SSL certificates, you must use provide the following configuration details to *WithConsumerProperties* method:
```cs
var sslConfigurationDetails = new Dictionary<string, string>();
sslConfigurationDetails.Add("security.protocol", "ssl");
sslConfigurationDetails.Add("ssl.ca.location", @"C:\certificates\ca-cert");
sslConfigurationDetails.Add("ssl.certificate.location", @"C:\\certificates\certificate.pem");
sslConfigurationDetails.Add("ssl.key.location", @"C:\certificates\key.pem");
sslConfigurationDetails.Add("ssl.key.password", "password");


var pipeline = client.StreamTopic(topicName).WithConsumerProperties(sslConfigurationDetails).Into(streamId => // Stream Kafka topic into the handler method
```
  
  - **WaitUntilStopped(TimeSpan timeout, CancellationToken ct)**:\
Wait for the pipeline to stop.
  
  - **WaitUntilFirstStream(TimeSpan timeout, CancellationToken ct)**:\
Wait for at least one stream to start. Does not reset after the first stream. Returns true immediately if a stream has already started, even if it has since finished.

 - **WaitUntilStopped(TimeSpan timeout, CancellationToken ct)**:\
Wait for the pipeline to stop.

#### Pipeline statuses

 - **IsConnected**:\
Gets whether the pipeline is connected to an upstream source.
 - **IsStopped**:\
Gets whether the pipeline is stopped.
 - **IsFaulted**:\
Gets when the pipeline has stopped due to an unhandled exception.
 - **HasFirstStream**:\
Gets whether at least one stream has started.

#### Pipeline exception/error handling

As the pipeline runs on a separate thread, the exceptions may occur are not being propageted to the main thread.
You can check for errors through the *IsFaulted* status. In case of exception this would be stored in the pipelines *Exception* property.

#### Pipeline management methods

 - **Drain()**:\
Stops the pipeline when all currently active streams have ended. No further streams will be started. *Dispose()* must still be called. The stream will replay from this point when restarted, if capable of doing so. Synchronize by calling  *WaitUntilStopped()*.\
Replay from the point this call is made implies that some messages will be seen twice. These can be filtered out.

 - **Stop()**:\
Stops the pipeline by detaching inputs without reading further messages. *Dispose()* must still be called. The stream will replay from this point when restarted, if capable of doing so. Synchronize by calling *WaitUntilStopped()*.
 
  - **WaitUntilConnected(TimeSpan timeout, CancellationToken ct)**:\
Wait until the pipeline is connected to an upstream source.
 
  - **WaitUntilIdle(TimeSpan timeout, CancellationToken ct)**:\
Wait until the pipeline does not have an active stream. For topics with overlapping streams, this may never happen - consider using Drain().
  
  - **WaitUntilStopped(TimeSpan timeout, CancellationToken ct)**:\
Wait for the pipeline to stop.
  
  - **WaitUntilFirstStream(TimeSpan timeout, CancellationToken ct)**:\
Wait for at least one stream to start. Does not reset after the first stream. Returns true immediately if a stream has already started, even if it has since finished.

 - **WaitUntilStopped(TimeSpan timeout, CancellationToken ct)**:\
Wait for the pipeline to stop.

#### Pipeline statuses

 - **IsConnected**:\
Gets whether the pipeline is connected to an upstream source.
 - **IsStopped**:\
Gets whether the pipeline is stopped.
 - **IsFaulted**:\
Gets when the pipeline has stopped due to an unhandled exception.
 - **HasFirstStream**:\
Gets whether at least one stream has started.

#### Pipeline exception/error handling

As the pipeline runs on a separate thread, the exceptions may occur are not being propageted to the main thread.
You can check for errors through the *IsFaulted* status. In case of exception this would be stored in the pipelines *Exception* property.

### Stream session input
[Create a SessionTelemetryDataInput](./src/MAT.OCS.Streaming.Samples/Samples/Basic/TData.cs#L70) with the actual stream id and the dataFormatClient 
```cs
var input = new SessionTelemetryDataInput(streamId, dataFormatClient);
```

### Telemetry Data
In this example we [bind the **DataInput** to the handler method](./src/MAT.OCS.Streaming.Samples/Samples/Basic/TData.cs#L71) using the default feed and simply [print out some details](./src/MAT.OCS.Streaming.Samples/Samples/Basic/TData.cs#L72-L86) about the incoming data.
```cs
input.DataInput.BindDefaultFeed(ParameterId).DataBuffered += (sender, e) => // Bind the incoming feed and take the data
{
  var data = e.Buffer.GetData();
  // In this sample we consume the incoming data and print it
  var time = data.TimestampsNanos;
  for (var i = 0; i < data.Parameters.Length; i++)
  {
      Trace.WriteLine($"Parameter[{i}]:");
      var vCar = data.Parameters[i].AvgValues;
      for (var j = 0; j < time.Length; j++)
      {
          var fromMilliseconds = TimeSpan.FromMilliseconds(time[j].NanosToMillis());
          Trace.WriteLine($"{fromMilliseconds:hh\\:mm\\:ss\\.fff}, {  new string('.', (int)(50 * vCar[j])) }");
      }
  }
};
```

Read more about [Data Feeds, Formats, and Views here](https://github.com/mat-docs/mtap-docs/blob/task/TAP-2839/AAS/2020.1/README.md#data-feeds-formats-and-views).

Read more about [TData here](https://github.com/mat-docs/mtap-docs/blob/task/TAP-2839/AAS/2020.1/README.md#tdata).

### Telemetry Samples
In this example we [bind the **SamplesInput** to the handler method](./src/MAT.OCS.Streaming.Samples/Samples/Basic/TSamples.cs#L77) and simply [print out some details](./src/MAT.OCS.Streaming.Samples/Samples/Basic/TSamples.cs#L78-L82) 
```cs
input.SamplesInput.AutoBindFeeds((s, e) => // Take the input and bind feed to an event handler
{
    var data = e.Data;// The event handler here only takes the samples data 
    Trace.WriteLine(data.Parameters.First().Key); // and prints some information to the debug console
    Trace.WriteLine(data.Parameters.Count);
});
```

Read more about [TSamples here](https://github.com/mat-docs/mtap-docs/blob/task/TAP-2839/AAS/2020.1/README.md#tsamples).

### Events

In this example we [subscribe to **EventsInput** with a handler method](./src/MAT.OCS.Streaming.Samples/Samples/Basic/EventsRead.cs#L45) and simply [print out some details of each event received](./src/MAT.OCS.Streaming.Samples/Samples/Basic/EventsRead.cs#L52-L63).

```cs
input.EventsInput.EventsBuffered += (sender, e) => // Subscribe to incoming events
{
    if (atlasConfiguration == null)
    {
        return;
    }

    var events = e.Buffer.GetData(); // read incoming events from buffer

    // In this sample we consume the incoming events and print it
    foreach (var ev in events)
    {
        var eventDefinition = atlasConfiguration.AppGroups?.First().Value?.Events.GetValueOrDefault(ev.Id);
        if (eventDefinition == null)
        {
            continue;
        }

        Console.WriteLine($"- Event: {ev.Id} - {eventDefinition.Description} - Priority: {eventDefinition.Priority.ToString()} - Value: {ev.Values?.First()}");
    }
};
```

Notice that we are [querying the Atlas configuration dependency](./src/MAT.OCS.Streaming.Samples/Samples/Basic/EventsRead.cs#L57) for event details. These details include properties like `Description`, `Priority`. You must [subscribe to session dependencies change](./src/MAT.OCS.Streaming.Samples/Samples/Basic/EventsRead.cs#L35-L43) to get this Atlas configuration dependency.

Read more about [Events here](https://github.com/mat-docs/mtap-docs/blob/task/TAP-2839/AAS/2020.1/README.md#events).

### Waits for completion
In order to successfully read and consume the stream, make sure to [wait until connected](./src/MAT.OCS.Streaming.Samples/Samples/Basic/TData.cs#L92-L93) and [wait for the first stream](./src/MAT.OCS.Streaming.Samples/Samples/Basic/TData.cs#L94). Optionally you can tell the pipeline to wait for a specific time [while the stream is being idle](./src/MAT.OCS.Streaming.Samples/Samples/Basic/TData.cs#L95), before exiting from the process.
```cs
if (!pipeline.WaitUntilConnected(TimeSpan.FromSeconds(30), CancellationToken.None)) // Wait until the connection is established
     throw new Exception("Couldn't connect");
pipeline.WaitUntilFirstStream(TimeSpan.FromMinutes(1), CancellationToken.None); // Wait until the first stream is ready to read.
pipeline.WaitUntilIdle(TimeSpan.FromMinutes(5), CancellationToken.None); // Wait for 5 minutes of the pipeline being idle before exit.
```

You can optionally handle the [StreamFinished event](./src/MAT.OCS.Streaming.Samples/Samples/Basic/TData.cs#L88).
```cs
input.StreamFinished += (sender, e) => Trace.WriteLine("Finished"); // Handle the steam finished event
```

### Buffers

TData and Events messages are getting buffered once polled from the Kafka stream. The following samples show the usage of the buffer and some practical use cases.

#### TData buffer

As you could see in the [TData](read.md#telemetry-data) example, you can subscribe to the *DataBuffered* event and receive the polled TData message immediately from the buffer.
You can also create a reference to the buffer and use it or read from it directly, whenever you need to.

```csharp
TelemetryDataBuffer buffer = input.DataInput.BindDefaultFeed(ParameterId).Buffer;
```

The *TelemetryDataBuffer* type has a few public methods:
 - void PutData(TelemetryData data) 
 - TelemetryData GetData(TelemetryData data)
 - TelemetryData GetDataInWindow(ITimeWindowCursor cursor):\
Gets buffered data in the cursor window. Data before the cursor window is discarded. The window may not be complete. Returns Buffered data in the cursor window.
 - TelemetryData GetDataInCompleteWindow(ITimeWindowCursor cursor):\
 Gets buffered data in the cursor window if there are samples beyond the end of the window. Returns Buffered data in the cursor window, or empty data if the window is incomplete.
 - bool IsEmpty()
 - TimeRange TimeRange():\
Gets the time range covered by data in the buffer, or *TimeRange.Invalid* if the buffer is empty.

A typical use case could be that you would read the buffer content only when a Lap is completed:
```csharp
var pipeline = client.StreamTopic(topicName).Into(streamId => // Stream Kafka topic into the handler method
{
	var input = new SessionTelemetryDataInput(streamId, dataFormatClient);
	var buffer = input.DataInput.BindDefaultFeed(ParameterId).Buffer;
	input.LapsInput.LapCompleted += (sender, args) =>
		{
			while (Equals(!buffer.IsEmpty))
			{
				var telemetryData = buffer.GetData();
			}
		};
```

You can create your own conditions, for example reaching a specific date time.
Here is a combined example, where you hold a reference to the buffer, but also subscribing to new TData messages and once yout date time condition is met, you read a set of TData from the buffer for a given time frame, using the *GetDataInCompleteWindow*:
```csharp
var pipeline = client.StreamTopic(topicName).Into(streamId => // Stream Kafka topic into the handler method
{
	var input = new SessionTelemetryDataInput(streamId, dataFormatClient);
	var buffer = input.DataInput.BindDefaultFeed(ParameterId).Buffer;
	input.DataInput.BindDefaultFeed(ParameterId).DataBuffered += (sender, e) => // Bind the incoming feed and take the data
	{
		if (DateTime.Now >= new DateTime(2020, 2,17, 11, 45, 9, DateTimeKind.Utc))
		{
			var telemetryData = buffer.GetDataInCompleteWindow(new TimeStepCursor(1000, 0, 0));
		}
	}
}
```

#### Events buffer

Events buffer uses a generic *DataBuffer* implementation, with a little bit different set of public methods than the TData's buffer implementation:

 - void PutData(T data) 
 - void PutData(IEnumerable<T> data) 
 - IList<T> GetData(TelemetryData data)
 - IList<T> GetDataInWindow(TimeRange window):\
Gets and removes all buffered data in the specified time window. Returns Buffered data in the time window.
 - IList<T> GetDataInCompleteWindow(TimeRange window):\
Gets and removes all buffered data in the specified time window IF there is data extending up to, or past the end of the specified window. Returns Buffered data in the time window, or an empty list if the window is incomplete.
 - int Count()
 - bool IsEmpty()
 - TimeRange TimeRange():\
Gets the time range covered by data in the buffer.

The usage is very similar to the TData buffer, you create a reference to the *EventsInput.Buffer*:
```csharp
var input = new SessionTelemetryDataInput(streamId, dataFormatClient);
var buffer = input.EventsInput.Buffer;
```
