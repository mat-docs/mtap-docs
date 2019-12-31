from mat.ocs.streaming.IO.Events.EventsEventArgs import EventsEventArgs
from mat.ocs.streaming.IO.SessionTelemetryDataInput import SessionTelemetryDataInput
from mat.ocs.streaming.clients.DataFormatClient import DataFormatClient
from mat.ocs.streaming.clients.KafkaStreamClient import KafkaStreamClient
from mat.ocs.streaming import (HttpDependencyClient)
from mat.ocs.streaming.models import Event
from mat.ocs.streaming.clients.pipeline.StreamInput import StreamInput
from mat.ocs.streaming.clients.pipeline.StreamPipeline import StreamPipeline

if __name__ == '__main__':
    """Setup details"""
    DEPENDENCY_SERVER_URI = 'http://10.228.4.9:8180/api/dependencies'
    DEPENDENCY_GROUP = 'dev'
    KAFKA_IP = '10.228.4.22:9092'
    TOPIC_NAME = 'samples_test_topic'

    dependency_client = HttpDependencyClient(DEPENDENCY_SERVER_URI, DEPENDENCY_GROUP)
    data_format_client = DataFormatClient(dependency_client)
    kafka_client = KafkaStreamClient(kafka_address=KAFKA_IP,
                                     consumer_group=DEPENDENCY_GROUP)

    def print_event(sender, event_args: EventsEventArgs):
        event: Event = event_args.buffer.get_first()
        print(event.status)

    def stream_input_handler(stream_id: str) -> StreamInput:
        print("Streaming session: " + stream_id)
        telemetry_input = SessionTelemetryDataInput(stream_id=stream_id,
                                                    data_format_client=data_format_client)
        telemetry_input.events_input.data_buffered += print_event
        return telemetry_input

    pipeline: StreamPipeline = kafka_client.stream_topic(TOPIC_NAME).into(stream_input_handler)

