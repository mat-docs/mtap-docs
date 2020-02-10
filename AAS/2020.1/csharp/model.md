# ![logo](/Media/branding.png) Atlas Advanced Stream

### Table of Contents
<!--ts-->
- [**Introduction**](../README.md)<br>
- [**C# Samples**](README.md)<br>
  - [Read](read.md#basic-samples-of-read)
    - [TData](read.md#telemetry-data)
    - [TSamples](read.md#telemetry-samples)
    - [Events](read.md#events)
  - [Write](write.md#basic-samples-of-write)
    - [TData](write.md#telemetry-data)
    - [TSamples](write.md#telemetry-samples)
    - [Events](write.md#events)
  - [Model execution](model.md#model-sample)
  - [Advanced Samples](advanced.md#advanced-samples)
- [**Python Samples**](../python/README.md)<br>
<!--te-->

# Model sample

A basic example of a model calculating the total horizontal acceleration parameter called gTotal. 

gTotal = |gLat| + |gLong|. 

Model consists from two classes, [ModelSample](https://github.com/mat-docs/mtap-docs/tree/master/AAS/2020.1/csharp/src/MAT.OCS.Streaming.Samples/Samples/Models/ModelSample.cs) and [StreamModel](https://github.com/mat-docs/mtap-docs/tree/master/AAS/2020.1/csharp/src/MAT.OCS.Streaming.Samples/Samples/Models/StreamModel.cs)
.
## Environment setup
You need to prepare environment variables as follows:

[Environment variables](https://github.com/mat-docs/mtap-docs/tree/master/AAS/2020.1/csharp/src/MAT.OCS.Streaming.Samples/Samples/Models/ModelSample.cs#L17-L20)
```cs
private const string DependencyUrl = "http://localhost:8180/api/dependencies/";
private const string InputTopicName = "ModelsInput";
private const string OutputTopicName = "ModelsOutput";
private const string BrokerList = "localhost";
```

Before you start your model, create all the necessary topics using Topic management service.

## Output format 
Specify output data of the model and publish it to dependency service:

[Output format](https://github.com/mat-docs/mtap-docs/tree/master/AAS/2020.1/csharp/src/MAT.OCS.Streaming.Samples/Samples/Models/ModelSample.cs#L37-L47)
```cs
var outputDataFormat = DataFormat.DefineFeed()
                            .Parameters(new List<string> { "gTotal:vTag" })
                            .AtFrequency(100)
                            .BuildFormat();

this.dataFormatId = dataFormatClient.PutAndIdentifyDataFormat(outputDataFormat);

var atlasConfiguration = this.CreateAtlasConfiguration();
this.atlasConfId = this.acClient.PutAndIdentifyAtlasConfiguration(atlasConfiguration);
```
## Subscribe
[Subscribe](https://github.com/mat-docs/mtap-docs/tree/master/AAS/2020.1/csharp/src/MAT.OCS.Streaming.Samples/Samples/Models/ModelSample.cs#L49-L58) for input topic streams:

```cs
using (var client = new KafkaStreamClient(BrokerList))
using (var outputTopic = client.OpenOutputTopic(OutputTopicName))
using (var pipeline = client.StreamTopic(InputTopicName).Into(streamId => this.CreateStreamPipeline(streamId, outputTopic)))
{
    cancellationToken.WaitHandle.WaitOne();
    pipeline.Drain();
    pipeline.WaitUntilStopped(TimeSpan.FromSeconds(1), CancellationToken.None);
}
```

## Into
Each stream will raise callback [Into](https://github.com/mat-docs/mtap-docs/tree/master/AAS/2020.1/csharp/src/MAT.OCS.Streaming.Samples/Samples/Models/ModelSample.cs#L61-L66)() where a new instance of the model for the new stream is created.

This block of code is called for each new stream. 

```cs
private IStreamInput CreateStreamPipeline(string streamId, IOutputTopic outputTopic)
{
    var streamModel = new StreamModel(this.dataFormatClient, outputTopic, this.dataFormatId, this.atlasConfId);

    return streamModel.CreateStreamInput(streamId);
}
```

## Stream model
For each new stream, we create a instance of StreamModel class.
[StreamModel](https://github.com/mat-docs/mtap-docs/tree/master/AAS/2020.1/csharp/src/MAT.OCS.Streaming.Samples/Samples/Models/StreamModel.cs)

### Create stream input
At the beginning of each stream, we create new stream input and output.

[CreateStreamInput](https://github.com/mat-docs/mtap-docs/tree/master/AAS/2020.1/csharp/src/MAT.OCS.Streaming.Samples/Samples/Models/StreamModel.cs#L25-L51)

```cs
public IStreamInput CreateStreamInput(string streamId)
{
    // these templates provide commonly-combined data, but you can make your own
    var input = new SessionTelemetryDataInput(streamId, dataFormatClient);
    var output = new SessionTelemetryDataOutput(outputTopic, this.outputDataFormatId, dataFormatClient);

    this.outputFeed = output.DataOutput.BindFeed("");
    
    // we add output format reference to output session.
    output.SessionOutput.AddSessionDependency(DependencyTypes.DataFormat, this.outputDataFormatId);
    output.SessionOutput.AddSessionDependency(DependencyTypes.AtlasConfiguration, this.outputAtlasConfId);

    // automatically propagate session metadata and lifecycle
    input.LinkToOutput(output.SessionOutput, identifier => identifier + "_Models");

    // we simply formward laps.
    input.LapsInput.LapStarted += (s, e) => output.LapsOutput.SendLap(e.Lap);

    // we bind our models to specific feed and parameters.
    input.DataInput.BindDefaultFeed("gLat:Chassis", "gLong:Chassis").DataBuffered += this.gTotalModel;

    return input;
}
```
### gTotal function
In the callback, each bucket of data is calculated and the result is sent to the output topic.
[gTotal ](https://github.com/mat-docs/mtap-docs/tree/master/AAS/2020.1/csharp/src/MAT.OCS.Streaming.Samples/Samples/Models/StreamModel.cs#L53-L85)

```cs
private void gTotalModel(object sender, TelemetryDataFeedEventArgs e)
{
    var inputData = e.Buffer.GetData();

    var data = outputFeed.MakeTelemetryData(inputData.TimestampsNanos.Length, inputData.EpochNanos);

    data.TimestampsNanos = inputData.TimestampsNanos;

    data.Parameters[0].AvgValues = new double[inputData.TimestampsNanos.Length];
    data.Parameters[0].Statuses = new DataStatus[inputData.TimestampsNanos.Length];

    for (var index = 0; index < inputData.TimestampsNanos.Length; index++)
    {
        var gLat = inputData.Parameters[0].AvgValues[index];
        var gLong = inputData.Parameters[1].AvgValues[index];

        var gLatStatus = inputData.Parameters[0].Statuses[index];
        var gLongStatus = inputData.Parameters[1].Statuses[index];

        data.Parameters[0].AvgValues[index] = Math.Abs(gLat) + Math.Abs(gLong);
        data.Parameters[0].Statuses[index] = (gLatStatus & DataStatus.Sample) > 0 && (gLongStatus & DataStatus.Sample) > 0
                                                ? DataStatus.Sample
                                                : DataStatus.Missing;

    }
    outputFeed.EnqueueAndSendData(data);

    Console.Write(".");
}
```
