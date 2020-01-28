from datetime import datetime
from typing import List

from mat.ocs.streaming.IO.SessionTelemetryDataOutput import SessionTelemetryDataOutput
from mat.ocs.streaming.IO.TelemetrySamples.TelemetrySamplesFeedOutput import TelemetrySamplesFeedOutput
from mat.ocs.streaming.IO.DependencyTypes import DependencyTypes
from mat.ocs.streaming.IO.StreamSessionState import StreamSessionState
from mat.ocs.streaming.clients.AtlasConfigurationClient import AtlasConfigurationClient
from mat.ocs.streaming.clients.DataFormatClient import DataFormatClient
from mat.ocs.streaming.clients.KafkaStreamClient import KafkaStreamClient
from mat.ocs.streaming import (HttpDependencyClient)
from mat.ocs.streaming.models import AtlasConfiguration, ApplicationGroup, ParameterGroup, \
    AtlasParameter, DataFeedDescriptor, DataFormat, Aggregates, TelemetrySamples, \
    TelemetryParameterSamples
from mat.ocs.streaming.models.dataformat.DataFeedParameter import DataFeedParameter
from mat.ocs.streaming.utils import to_telemetry_time, RandomRangeWalker


def generate_samples(sample_count: int, session_start: datetime, parameter_id: str, frequency=100) -> TelemetrySamples:
    sample = TelemetryParameterSamples()
    sample.epoch = to_telemetry_time(session_start)
    sample.time = [float] * sample_count
    sample.values = [float] * sample_count

    interval = 1000 / frequency * 1000000
    range_walker = RandomRangeWalker(0, 1)
    for s in range(sample_count):
        next_sample = range_walker.get_next()
        sample.time[s] = s * interval
        sample.values[s] = next_sample

    data = TelemetrySamples()
    data.parameters[parameter_id] = sample

    return data


if __name__ == '__main__':
    """Setup details"""
    DEPENDENCY_SERVER_URI = 'http://10.228.4.9:8180/api/dependencies'
    DEPENDENCY_GROUP = 'dev'
    KAFKA_IP = '10.228.4.22:9092'
    TOPIC_NAME = 'samples_test_topic'

    frequency = 100
    feed_name = ""

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
    parameter: DataFeedParameter = DataFeedParameter(identifier="vCar:Chassis",
                                                     aggregates_enum=[Aggregates.avg])
    parameters: List[DataFeedParameter] = [parameter]
    feed = DataFeedDescriptor(frequency=frequency,
                              parameters=parameters)

    data_format = DataFormat({feed_name: feed})

    data_format_client = DataFormatClient(dependency_client)
    data_format_id = data_format_client.put_and_identify_data_format(data_format)

    """Create a Kafka client"""
    client = KafkaStreamClient(kafka_address=KAFKA_IP,
                               consumer_group=DEPENDENCY_GROUP)

    output: SessionTelemetryDataOutput = None
    with client.open_output_topic(TOPIC_NAME) as output_topic:
        try:
            output = SessionTelemetryDataOutput(output_topic=output_topic,
                                                data_format_id=data_format_id,
                                                data_format_client=data_format_client)

            output.session_output.add_session_dependency(DependencyTypes.atlas_configuration, atlas_configuration_id)
            output.session_output.add_session_dependency(DependencyTypes.data_format, data_format_id)

            output.session_output.session_state = StreamSessionState.Open
            output.session_output.session_start = datetime.utcnow()
            output.session_output.session_identifier = "test_" + str(datetime.utcnow())
            output.session_output.session_details = {"test_session": "sample test session details"}
            output.session_output.send_session()

            """Making 1 TelemetrySamples with 10 samples"""
            telemetry_samples = generate_samples(sample_count=10,
                                                 session_start=datetime.utcnow(),
                                                 parameter_id="vCar:Chassis",
                                                 frequency=frequency)

            #output_feed: TelemetryDataFeedOutput = output.samples_output.bind_default_feed()
            output_feed: TelemetrySamplesFeedOutput = output.samples_output.bind_feed(feed_name=feed_name)
            """Sending 1 TelemetrySamples"""
            output_feed.send(telemetry_samples)

            output.session_output.session_state = StreamSessionState.Closed
        except Exception as e:
            print(e)
            if output is not None:
                output.session_output.session_state = StreamSessionState.Truncated
        finally:
            if output is not None:
                output.session_output.send_session()
