# ATLAS Advanced Streaming Samples

![Build Status](https://mat-ocs.visualstudio.com/Telemetry%20Analytics%20Platform/_apis/build/status/MAT.OCS.Streaming/Streaming%20Samples?branchName=develop)

Table of Contents
=================
<!--ts-->
* [Introduction - MAT.OCS.Streaming library](/../../README.md)
* [Python# Samples](/../../README.md)
* [Model sample](/README.md)
<!--te-->

# Model sample

A basic example of a model calculating the total horizontal acceleration parameter called gTotal. 

gTotal = |gLat| + |gLong|. 

Model consists from two classes, [ModelSample](https://github.com/McLarenAppliedTechnologies/mat.ocs.streaming.python.samples/blob/develop/src/Models/ModelExample.py) and [StreamModel](https://github.com/McLarenAppliedTechnologies/mat.ocs.streaming.python.samples/blob/develop/src/Models/ModelInstance.py)
.
## Environment setup
You need to prepare environment variables as follows:

[Environment variables](https://github.com/McLarenAppliedTechnologies/mat.ocs.streaming.python.samples/blob/develop/src/Models/ModelExample.py#L18-L22)
```python
DEPENDENCY_SERVER_URI = 'http://10.228.4.9:8180/api/dependencies'
DEPENDENCY_GROUP = 'dev'
KAFKA_IP = '10.228.4.22:9092'
INPUT_TOPIC_NAME = 'ModelsInput'
OUTPUT_TOPIC_NAME = 'ModelsOutput'
```

Before you start your model, create all the necessary topics using Topic management service.

## Output format 
Specify output data of the model and publish it to dependency service:

[Output format](https://github.com/McLarenAppliedTechnologies/mat.ocs.streaming.python.samples/blob/develop/src/Models/ModelExample.py#L25-L33)
```python
dependency_client = HttpDependencyClient(DEPENDENCY_SERVER_URI, DEPENDENCY_GROUP)
data_format_client = DataFormatClient(dependency_client)
parameter: DataFeedParameter = DataFeedParameter(identifier="gTotal:vTag", aggregates_enum=[Aggregates.avg])
parameters: List[DataFeedParameter] = [parameter]
    
feed = DataFeedDescriptor(frequency=100, parameters=parameters)
feed_name = ""
data_format = DataFormat({feed_name: feed})
data_format_id = data_format_client.put_and_identify_data_format(data_format)
```

If you want to use your data in Atlas, you need to specify and upload (put) your [Atlas configuration](https://github.com/McLarenAppliedTechnologies/mat.ocs.streaming.python.samples/blob/develop/src/Models/ModelExample.py#L35-L44)
```python
atlas_configuration_client = AtlasConfigurationClient(dependency_client)
atlas_configuration = AtlasConfiguration({"Models":
    ApplicationGroup(groups={"vTag":
        ParameterGroup(
            parameters={"gTotal:vTag":
                AtlasParameter(
                    name="gTotal",
                    physical_range=Range(0, 10))})})})

atlas_configuration_id = atlas_configuration_client.put_and_identify_atlas_configuration(atlas_configuration)
```

## Subscribe
[Subscribe](https://github.com/McLarenAppliedTechnologies/mat.ocs.streaming.python.samples/blob/develop/src/Models/ModelExample.py#L46-L47) for input topic streams:

```python
kafka_client = KafkaStreamClient(kafka_address=KAFKA_IP, consumer_group=DEPENDENCY_GROUP)
output_topic = kafka_client.open_output_topic(OUTPUT_TOPIC_NAME)
```

## Into
Each stream will raise callback [Into](https://github.com/McLarenAppliedTechnologies/mat.ocs.streaming.python.samples/blob/develop/src/Models/ModelExample.py#L58)() where a new instance of the model for the new stream is created.

```python
pipeline: StreamPipeline = kafka_client.stream_topic(INPUT_TOPIC_NAME).into(stream_input_handler)
```

This block of code is called for each new stream as a [stream_input_handler](https://github.com/McLarenAppliedTechnologies/mat.ocs.streaming.python.samples/blob/develop/src/Models/ModelExample.py#L50-L55)

```python
def stream_input_handler(stream_id: str) -> StreamInput:
    print("Streaming session: " + stream_id)

    model_instance = ModelInstance(data_format_client, output_topic, data_format_id, atlas_configuration_id)
    telemetry_input = model_instance.stream_input_handler(stream_id)
    return telemetry_input
```

## Stream model
For each new stream, we create a instance of ModelInstance class.
[ModelInstance](https://github.com/McLarenAppliedTechnologies/mat.ocs.streaming.python.samples/blob/develop/src/Models/ModelInstance.py#L14-L25)

### Create stream input
At the beginning of each stream, we create new stream input handler

[stream_input_handler](https://github.com/McLarenAppliedTechnologies/mat.ocs.streaming.python.samples/blob/develop/src/Models/ModelInstance.py#L27-L48)

```python
def stream_input_handler(self, stream_id: str) -> StreamInput:
    print("Streaming session: " + stream_id)

    telemetry_input = SessionTelemetryDataInput(stream_id=stream_id, data_format_client=self._data_format_client)
    output = SessionTelemetryDataOutput(output_topic=self._output_topic, data_format_id=self._output_data_format_id,
                                        data_format_client=self._data_format_client)
    self._output_feed: TelemetryDataFeedOutput = output.data_output.bind_default_feed()

    output.session_output.add_session_dependency(DependencyTypes.data_format, self._output_data_format_id)
    output.session_output.add_session_dependency(DependencyTypes.atlas_configuration, self._output_atlas_conf_id)
    output.session_output.add_session_detail("tamas is testing", "if it is working")
    telemetry_input.data_input.bind_default_feed("").data_buffered += self.gTotal_model
    telemetry_input.laps_input.lap_completed += lambda s, e: output.laps_output.send(e.lap)
    telemetry_input.stream_finished += lambda x, y: pprint("Stream " + stream_id + " ended.")

    output.session_output.session.model = Model("Test", 2, "Par1=abc")

    telemetry_input.link_to_output(
        session_output=output.session_output,
        identifier_transform=lambda identifier: identifier + "_models")

    return telemetry_input
```
### gTotal function
In the callback, each bucket of data is calculated and the result is sent to the output topic.
[gTotal_model](https://github.com/McLarenAppliedTechnologies/mat.ocs.streaming.python.samples/blob/develop/src/Models/ModelInstance.py#L50-L67)

```python
def gTotal_model(self, sender, event_args: TelemetryDataFeedEventArgs):
    input_data = event_args.buffer.get_first()

    data: TransformedTelemetryData = self._output_feed.make_transformed_telemetry_data(samples=10, epoch=input_data.epoch)
    data.time = input_data.time

    for i, obj in enumerate(input_data.time):
        g_lat = input_data.parameters["gLat:Chassis"].avg[i]
        g_long = input_data.parameters["gLong:Chassis"].avg[i]

        g_lat_status = input_data.parameters["gLat:Chassis"].status[i]
        g_long_status = input_data.parameters["gLong:Chassis"].status[i]

        data.parameters["gTotal:vTag"].avg[i] = math.fabs(g_lat) + math.fabs(g_long)
        data.parameters["gTotal:vTag"].status[i] = DataStatus.Sample if ((g_lat_status & DataStatus.Sample) > 0 and (
                g_long_status & DataStatus.Sample) > 0) else DataStatus.Missing

    self._output_feed.send(data)
```