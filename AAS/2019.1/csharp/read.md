# ![logo](/Media/branding.png) Atlas Advanced Stream

### Table of Contents
- [**Introduction**](../README.md)<br>
- [**C# Samples**](README.md)<br>
  - [Samples project](./src)
  - Read
    - [TData](read.md#telemetry-data)
    - [Tamples](read.md#telemetry-samples)
  - [Write](write.md#basic-samples)
    - [TData](write.md#telemetry-data)
    - [Tamples](write.md#telemetry-samples)
  - [Advanced Samples](advanced.md#advanced-samples)
- [**Python Samples**](./python/README.md)<br>

## Basic samples
Basic samples demonstrate the simple usage of Advanced Streams, covering all the bare-minimum steps to implement Telematry Data and Telemetry Samples read and write to and from Kafka or Mqtt streams.

First of all you need to configure the [dependencies](./src/MAT.OCS.Streaming.Samples/Samples/Basic/TData.cs#L60-L63)
```cs
const string brokerList = "localhost:9092"; // The host and port where the Kafka broker is running
const string groupName = "dev"; // The group name
const string topicName = "data_in"; // The existing topic's name in the Kafka broker. The *_annonce topic name must exist too. In this case the data_in_announce
var dependencyServiceUri = new Uri("http://localhost:8180/api/dependencies/"); // The URI where the dependency services are running

var client = new KafkaStreamClient(brokerList); // Create a new KafkaStreamClient for connecting to Kafka broker
var dataFormatClient = new DataFormatClient(new HttpDependencyClient(dependencyServiceUri, groupName)); // Create a new DataFormatClient
```

The DependencyService is used to handle requests for AtlasConfigurations and DataFormats. You must provide an URI for this service. 
The DataFormatClient handles the data formats through the DependencyService for the given group name.

If you want to connect to MQTT, create a client of MqttStreamClient instead of KafkaStreamClient:
```cs
var client = new MqttStreamClient(new MqttConnectionConfig(brokerList, "userName", "password"));
```

Create a stream pipeline using the KafkaStreamClient and the topicName. Stream the messages [.Into your handler method](./src/MAT.OCS.Streaming.Samples/Samples/Basic/TData.cs#L68)
```cs
var pipeline = client.StreamTopic(topicName).Into(streamId => // Stream Kafka topic into the handler method
```

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

You can optionally handle the [StreamFinished event](./src/MAT.OCS.Streaming.Samples/Samples/Basic/TData.cs#L88).
```cs
input.StreamFinished += (sender, e) => Trace.WriteLine("Finished"); // Handle the steam finished event
```

In order to successfully read and consume the stream, make sure to [wait until connected](./src/MAT.OCS.Streaming.Samples/Samples/Basic/TData.cs#L92-L93) and [wait for the first stream](./src/MAT.OCS.Streaming.Samples/Samples/Basic/TData.cs#L94). Optionally you can tell the pipeline to wait for a specific time [while the stream is being idle](./src/MAT.OCS.Streaming.Samples/Samples/Basic/TData.cs#L95), before exiting from the process.
```cs
if (!pipeline.WaitUntilConnected(TimeSpan.FromSeconds(30), CancellationToken.None)) // Wait until the connection is established
     throw new Exception("Couldn't connect");
pipeline.WaitUntilFirstStream(TimeSpan.FromMinutes(1), CancellationToken.None); // Wait until the first stream is ready to read.
pipeline.WaitUntilIdle(TimeSpan.FromMinutes(5), CancellationToken.None); // Wait for 5 minutes of the pipeline being idle before exit.
```