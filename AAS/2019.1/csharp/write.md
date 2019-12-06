# ![logo](/Media/branding.png) Atlas Advanced Stream

### Table of Contents
- [**Introduction**](../README.md)<br>
- [**C# Samples**](README.md)<br>
  - [Samples project](./src)
  - [Read](read.md#basic-samples)
    - [TData](read.md#telemetry-data)
    - [TSamples](read.md#telemetry-samples)
  - Write
    - [TData](write.md#telemetry-data)
    - [TSamples](write.md#telemetry-samples)
  - [Advanced Samples](advanced.md#advanced-samples)
- [**Python Samples**](./python/README.md)<br>

## Basic samples
Basic samples demonstrate the simple usage of Advanced Streams, covering all the bare-minimum steps to implement Telematry Data and Telemetry Samples read and write to and from Kafka or Mqtt streams.

First of all you need to configure the [dependencies](./src/MAT.OCS.Streaming.Samples/Samples/Basic/TData.cs#L102-L113)
```cs
const string brokerList = "localhost:9092"; // The host and port where the Kafka broker is running
const string groupName = "dev"; // The group name
const string topicName = "data_in"; // The existing topic's name in the Kafka broker. The *_annonce topic name must exist too. In this case the data_in_announce
var dependencyServiceUri = new Uri("http://localhost:8180/api/dependencies/"); // The URI where the dependency services are running

var client = new KafkaStreamClient(brokerList); // Create a new KafkaStreamClient for connecting to Kafka broker
var dataFormatClient = new DataFormatClient(new HttpDependencyClient(dependencyServiceUri, groupName)); // Create a new DataFormatClient
var httpDependencyClient = new HttpDependencyClient(dependencyServiceUri, groupName); // DependencyClient stores the Data format, Atlas Configuration

var atlasConfigurationId = new AtlasConfigurationClient(httpDependencyClient).PutAndIdentifyAtlasConfiguration(AtlasConfiguration); // Uniq ID created for the AtlasConfiguration
var dataFormat = DataFormat.DefineFeed().Parameter(ParameterId).BuildFormat(); // Create a dataformat based on the parameters, using the parameter id
var dataFormatId = dataFormatClient.PutAndIdentifyDataFormat(dataFormat); // Uniq ID created for the Data Format
```

The DependencyService is used to handle requests for AtlasConfigurations and DataFormats. You must provide an URI for this service. 
The DataFormatClient handles the data formats through the DependencyService for the given group name.
DataFormat is required when writing to stream, as it is used to define the structre of the data and dataFormatId is used to retrieve dataformat from the dataFormatClient.

AtlasConfigurationId is needed only if you want to display your data in Atlas10.

If you want to connect to MQTT, create a client of MqttStreamClient instead of KafkaStreamClient:
```cs
var client = new MqttStreamClient(new MqttConnectionConfig(brokerList, "userName", "password"));
```

[Open the output topic](./src/MAT.OCS.Streaming.Samples/Samples/Basic/TData.cs#L115) using the preferred client (KafkaStreamClient or MqttStreamClient) and the topicName.
```cs
using (var outputTopic = client.OpenOutputTopic(topicName)) // Open a KafkaOutputTopic
{
}
```

[Create a SessionTelemetryDataOutput](./src/MAT.OCS.Streaming.Samples/Samples/Basic/TData.cs#L118) and configure session output [properties](./src/MAT.OCS.Streaming.Samples/Samples/Basic/TData.cs#L118-L125).
```cs
var output = new SessionTelemetryDataOutput(outputTopic, dataFormatId, dataFormatClient);
output.SessionOutput.AddSessionDependency(DependencyTypes.DataFormat, dataFormatId); // Add session dependencies to the output
output.SessionOutput.AddSessionDependency(DependencyTypes.AtlasConfiguration, atlasConfigurationId);

output.SessionOutput.SessionState = StreamSessionState.Open; // set the sessions state to open
output.SessionOutput.SessionStart = DateTime.Now; // set the session start to current time
output.SessionOutput.SessionIdentifier = "data_" + DateTime.Now; // set a custom session identifier
```

You must add dataFormatId and atlasConfigurationId to session dependencies to be able to use them during the streaming session.

Once it is done, [send the session](./src/MAT.OCS.Streaming.Samples/Samples/Basic/TData.cs#L126) details to the output.
```cs
output.SessionOutput.SendSession();
```


### Telemetry Data
You will need **TelemetryData** to write to the output. In this example we [generate some random telemetryData](./src/MAT.OCS.Streaming.Samples/Samples/Basic/TData.cs#L128) for the purpose of demonstration.
```cs
var telemetryData = GenerateData(10, (DateTime)output.SessionOutput.SessionStart); // Generate some telemetry data
```

[Bind the feed to **DataOutput**](./src/MAT.OCS.Streaming.Samples/Samples/Basic/TData.cs#L130-L131) by its name. You can use the default feedname or use a custom one.
```cs
const string feedName = ""; // As sample DataFormat uses default feed, we will leave this empty.
var outputFeed = output.DataOutput.BindFeed(feedName); // bind your feed by its name to the Data Output
```

[Enqueue and send](./src/MAT.OCS.Streaming.Samples/Samples/Basic/TData.cs#L133) the telemetry data.
```cs
Task.WaitAll(outputFeed.EnqueueAndSendData(telemetryData)); // enqueue and send the data to the output through the outputFeed
```

### Telemetry Samples
You will need **TelemetrySamples** to write to the output. In this example we [generate some random telemetrySamples](./src/MAT.OCS.Streaming.Samples/Samples/Basic/TSamples.cs#L125) for the purpose of demonstration.
```cs
var telemetrySamples = GenerateSamples(10, (DateTime)output.SessionOutput.SessionStart); // Generate some telemetry samples
```

[Bind the feed to **SamplesOutput**](./src/MAT.OCS.Streaming.Samples/Samples/Basic/TSamples.cs#L127-L128) by its name. You can use the default feedname or use a custom one.
```cs
const string feedName = ""; // As sample DataFormat uses default feed, we will leave this empty.
var outputFeed = output.SamplesOutput.BindFeed(feedName); // bind your feed by its name to the SamplesOutput
```

[Send Samples](./src/MAT.OCS.Streaming.Samples/Samples/Basic/TSamples.cs#L130).
```cs
Task.WaitAll(outputFeed.SendSamples(telemetrySamples)); // send the samples to the output through the outputFeed
```


Once you sent all your data, don't forget to [set the session state to closed](./src/MAT.OCS.Streaming.Samples/Samples/Basic/TData.cs#L132) 
```cs
output.SessionOutput.SessionState = StreamSessionState.Closed; // set session state to closed. In case of any unintended session close, set state to Truncated
```

and [send the session details](./src/MAT.OCS.Streaming.Samples/Samples/Basic/TData.cs#L133).
```cs
output.SessionOutput.SendSession(); // send session
```


