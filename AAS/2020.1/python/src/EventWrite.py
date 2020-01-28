from datetime import datetime
from typing import List

from mat.ocs.streaming.IO.TelemetryData.TelemetryDataFeedOutput import TelemetryDataFeedOutput
from mat.ocs.streaming.IO.SessionTelemetryDataOutput import SessionTelemetryDataOutput
from mat.ocs.streaming.IO.DependencyTypes import DependencyTypes
from mat.ocs.streaming.IO.StreamSessionState import StreamSessionState
from mat.ocs.streaming.clients.DataFormatClient import DataFormatClient
from mat.ocs.streaming.clients.KafkaStreamClient import KafkaStreamClient
from mat.ocs.streaming import HttpDependencyClient
from mat.ocs.streaming.models import Event, DataFeedDescriptor, DataFormat, Aggregates
from mat.ocs.streaming.models.dataformat.DataFeedParameter import DataFeedParameter

if __name__ == '__main__':
    """Setup details"""
    # Populate these constants with the correct values for your project.
    DEPENDENCY_SERVER_URI = 'http://10.228.4.9:8180/api/dependencies'
    DEPENDENCY_GROUP = 'dev'
    KAFKA_IP = '10.228.4.22:9092'
    TOPIC_NAME = 'samples_test_topic'

    frequency = 100
    """Create a dependency client"""
    dependency_client = HttpDependencyClient(DEPENDENCY_SERVER_URI, DEPENDENCY_GROUP)

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

    output: SessionTelemetryDataOutput = None
    with client.open_output_topic(TOPIC_NAME) as output_topic:
        try:
            output = SessionTelemetryDataOutput(output_topic=output_topic,
                                                data_format_id=data_format_id,
                                                data_format_client=data_format_client)

            output.session_output.add_session_dependency(
                DependencyTypes.data_format, data_format_id)

            output.session_output.session_state = StreamSessionState.Open
            output.session_output.session_start = datetime.utcnow()
            output.session_output.session_identifier = "test_" + str(datetime.utcnow())
            output.session_output.session_details = {"test_session": "sample test session details"}
            output.session_output.send_session()

            output_feed: TelemetryDataFeedOutput = output.data_output.bind_default_feed()

            """Making 1 Event with some test data"""
            event = Event("testEvent", 1, 1, "testing", [1.0, 2.0])

            """Sending 1 Event"""
            output.events_output.send(event)
            output.session_output.session_state = StreamSessionState.Closed
        except Exception as e:
            print(e)
            if output is not None:
                output.session_output.session_state = StreamSessionState.Truncated
        finally:
            if output is not None:
                output.session_output.send_session()

