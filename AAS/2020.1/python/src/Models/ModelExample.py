from typing import List

from mat.ocs.streaming.clients.AtlasConfigurationClient import AtlasConfigurationClient
from mat.ocs.streaming.clients.DataFormatClient import DataFormatClient
from mat.ocs.streaming.clients.KafkaStreamClient import KafkaStreamClient
from mat.ocs.streaming import (HttpDependencyClient)
from mat.ocs.streaming.clients.pipeline.StreamInput import StreamInput
from mat.ocs.streaming.clients.pipeline.StreamPipeline import StreamPipeline
from mat.ocs.streaming.models import DataFeedDescriptor, DataFormat, Aggregates, \
    AtlasConfiguration, ApplicationGroup, ParameterGroup, AtlasParameter, Range
from mat.ocs.streaming.models.dataformat.DataFeedParameter import DataFeedParameter

from Models.ModelInstance import ModelInstance

"""Setup details"""
# Populate these constants with the correct values for your project.

DEPENDENCY_SERVER_URI = 'http://10.228.4.9:8180/api/dependencies'
DEPENDENCY_GROUP = 'dev'
KAFKA_IP = '10.228.4.22:9092'
INPUT_TOPIC_NAME = 'ModelsInput'
OUTPUT_TOPIC_NAME = 'ModelsOutput'

"""Create Dataformat"""
dependency_client = HttpDependencyClient(DEPENDENCY_SERVER_URI, DEPENDENCY_GROUP)
data_format_client = DataFormatClient(dependency_client)
parameter: DataFeedParameter = DataFeedParameter(identifier="gTotal:vTag", aggregates_enum=[Aggregates.avg])
parameters: List[DataFeedParameter] = [parameter]

feed = DataFeedDescriptor(frequency=100, parameters=parameters)
feed_name = ""
data_format = DataFormat({feed_name: feed})
data_format_id = data_format_client.put_and_identify_data_format(data_format)

atlas_configuration_client = AtlasConfigurationClient(dependency_client)
atlas_configuration = AtlasConfiguration({"Models":
    ApplicationGroup(groups={"vTag":
        ParameterGroup(
            parameters={"gTotal:vTag":
                AtlasParameter(
                    name="gTotal",
                    physical_range=Range(0, 10))})})})

atlas_configuration_id = atlas_configuration_client.put_and_identify_atlas_configuration(atlas_configuration)

kafka_client = KafkaStreamClient(kafka_address=KAFKA_IP, consumer_group=DEPENDENCY_GROUP)
output_topic = kafka_client.open_output_topic(OUTPUT_TOPIC_NAME)


def stream_input_handler(stream_id: str) -> StreamInput:
    print("Streaming session: " + stream_id)

    model_instance = ModelInstance(data_format_client, output_topic, data_format_id, atlas_configuration_id)
    telemetry_input = model_instance.stream_input_handler(stream_id)
    return telemetry_input


pipeline: StreamPipeline = kafka_client.stream_topic(INPUT_TOPIC_NAME).into(stream_input_handler)
