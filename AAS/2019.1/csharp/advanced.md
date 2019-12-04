# ![logo](/Media/branding.png) Atlas Advanced Stream

### Table of Contents
- [**Introduction**](../README.md)<br>
- [**C# Samples**](README.md)<br>
  - [Samples project](./src)
  - [Read](read.md#basic-samples)
    - [TData](read.md#telemetry-data)
    - [Tamples](read.md#telemetry-samples)
  - [Write](write.md#basic-samples)
    - [TData](write.md#telemetry-data)
    - [Tamples](write.md#telemetry-samples)
  - Advanced Samples

# Advanced Samples

Advanced samples cover the usual use cases for reading, writing and reading and linking telemetry data in an structured and organized code.
According to that each sample .cs file has a Write(), Read() and ReadAndLink() methods and all of the sample files rely on the same structure. You can use them as working samples copying to your application code.
The .cs files in the [Samples folder](./src/MAT.OCS.Streaming.Samples/Samples) have documenting and descriptive comments, but lets take a look at a simple and a more complex sample in depth.

## Writing Telemetry Data with a parameter and default feed name to a Kafka topic.

First of all you need to create or use an [AtlasConfiguration](./src/MAT.OCS.Streaming.Samples/Samples/TDataSingleFeedSingleParameter.cs#L27-L53). You need to set the [details](./src/MAT.OCS.Streaming.Samples/Samples/TDataSingleFeedSingleParameter.cs#L13-L22) what AppGroupId, ParameterGroupId, ParameterID you want to use.

Once you have your AtlasConfiguration design, you need to set [details](./src/MAT.OCS.Streaming.Samples/Samples/TDataSingleFeedSingleParameter.cs#L117-L120) for the DependencyService URI, the stream broker address, the group name and the output topic name where you want to write. 
The DependencyService is used to handle requests for AtlasConfigurations and DataFormats, you must provide an URI for this service. 

A [KafkaStreamAdapter](./src/MAT.OCS.Streaming.Samples/Samples/TDataSingleFeedSingleParameter.cs#L121) is used to manage Kafka streams.

Using the KafkaStreamAdapter you must [open and output topic](./src/MAT.OCS.Streaming.Samples/Samples/TDataSingleFeedSingleParameter.cs#L122) in Kafka.

You need to persist your configuration, setup your DataFormat and setup a streaming session to be able to write to your broker. The [Writer.cs](https://github.com/McLarenAppliedTechnologies/mat.ocs.streaming.samples/blob/update_samples/src/MAT.OCS.Streaming.Samples/Samples/Writer.cs) code sample covers all of these, you only need to [use a Writer object](./src/MAT.OCS.Streaming.Samples/Samples/TDataSingleFeedSingleParameter.cs#L124) with the required parameters.

In this example we are usign the [default feed name](./src/MAT.OCS.Streaming.Samples/Samples/TDataSingleFeedSingleParameter.cs#L126), which is the empty string.
You must take care about [opening](./src/MAT.OCS.Streaming.Samples/Samples/TDataSingleFeedSingleParameter.cs#L127) and [closing](./src/MAT.OCS.Streaming.Samples/Samples/TDataSingleFeedSingleParameter.cs#L134) the session, otherwise it would mark the session as [truncated](./src/MAT.OCS.Streaming.Samples/Samples/Writer.cs#L52), just as if any error would occur during session usage.

In the samples we are working with random [generated data](./src/MAT.OCS.Streaming.Samples/Samples/TDataSingleFeedSingleParameter.cs#L79-L113), while in real scenarios they would come as real data from external systems.

To write to the output topic you only need to [invoke the Write method](./src/MAT.OCS.Streaming.Samples/Samples/TDataSingleFeedSingleParameter.cs#L132) on your writer object, passing in the feed name and the telemetry data. The Writer object already "knows" your AtlasConfiguration, the DataFormat and the output topic name.


## Reading Telemetry Data for a parameter from a Kafka stream.

First of all you need to create or use an [AtlasConfiguration](./src/MAT.OCS.Streaming.Samples/Samples/TDataSingleFeedSingleParameter.cs#L27-L53). You need to set the [details](./src/MAT.OCS.Streaming.Samples/Samples/TDataSingleFeedSingleParameter.cs#L13-L22) what AppGroupId, ParameterGroupId, ParameterID you want to use.

Once you have your AtlasConfiguration design, you need to set [details](./src/MAT.OCS.Streaming.Samples/Samples/TDataSingleFeedSingleParameter.cs#L117-L120) for the DependencyService URI, the stream broker address, the group name and the output topic name where you want to write. 
The DependencyService is used to handle requests for AtlasConfigurations and DataFormats, you must provide an URI for this service. 

A [KafkaStreamAdapter](./src/MAT.OCS.Streaming.Samples/Samples/TDataSingleFeedSingleParameter.cs#L145) is used to manage Kafka streams.

Using the KafkaStreamAdapter you must [open and output stream](./src/MAT.OCS.Streaming.Samples/Samples/TDataSingleFeedSingleParameter.cs#L145) in Kafka.

You need connect to the DependencyClient and the DataFormatClient and perist the StreamPipelineBuilder that was created by the KafkaStreamAdapter. Using a [Reader object](./src/MAT.OCS.Streaming.Samples/Samples/TDataSingleFeedSingleParameter.cs#L147) takes care about it.

You can start reading a stream with the [Read method](./src/MAT.OCS.Streaming.Samples/Samples/TDataSingleFeedSingleParameter.cs#L174) on your reader object. This takes 2 arguments, the first one is more trivial, it is the parameter id. The second must be a user specified method, aligning to the [TelemetryDataHandler](./src/MAT.OCS.Streaming.Samples/Samples/Models.cs#L12) delegate. With the help of this you can handle the streamed Telemetry Data as you would like to. In another example you will see how can you link it directly to another output topic.

Lastly, in our sample code we [invoke the Write()](./src/MAT.OCS.Streaming.Samples/Samples/TDataSingleFeedSingleParameter.cs#L151) method while the streaming session is live, to have some input to see that the streaming is working. Our sample delegates, called as Models are in the [Models.cs](https://github.com/McLarenAppliedTechnologies/mat.ocs.streaming.samples/blob/update_samples/src/MAT.OCS.Streaming.Samples/Samples/Models.cs) and in this example we use the [TraceData method](./src/MAT.OCS.Streaming.Samples/Samples/Models.cs#L14-L27) to trace the streamed telemetry data deatils.
