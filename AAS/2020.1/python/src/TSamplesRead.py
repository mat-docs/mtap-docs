from mat.ocs.streaming.IO.SessionTelemetryDataInput import SessionTelemetryDataInput
from mat.ocs.streaming.TelemetryEventArgs import TelemetryEventArgs
from mat.ocs.streaming.clients.DataFormatClient import DataFormatClient
from mat.ocs.streaming.clients.KafkaStreamClient import KafkaStreamClient
from mat.ocs.streaming import (HttpDependencyClient)
from mat.ocs.streaming.models import TelemetrySamples
from mat.ocs.streaming.clients.pipeline.StreamInput import StreamInput
from mat.ocs.streaming.clients.pipeline.StreamPipeline import StreamPipeline

if __name__ == '__main__':
    """Setup details"""
    DEPENDENCY_SERVER_URI = 'http://localhost:8180/api/dependencies'
    DEPENDENCY_GROUP = 'dev'
    KAFKA_IP = 'localhost:9092'
    TOPIC_NAME = 'test_topic'

    dependency_client = HttpDependencyClient(DEPENDENCY_SERVER_URI, DEPENDENCY_GROUP)
    data_format_client = DataFormatClient(dependency_client)
    kafka_client = KafkaStreamClient(kafka_address=KAFKA_IP,
                                     consumer_group=DEPENDENCY_GROUP)


    def print_samples(sender, event_args: TelemetryEventArgs):
        s: TelemetrySamples = event_args.data
        print('tsamples for {0} with {1} parameters received'.format(
            str(event_args.message_origin.stream_id),
            str(len(s.parameters.keys()))))


    def stream_input_handler(stream_id: str) -> StreamInput:
        print("Streaming session: " + stream_id)
        telemetry_input = SessionTelemetryDataInput(stream_id=stream_id,
                                                    data_format_client=data_format_client)
        telemetry_input.samples_input.autobind_feeds += print_samples
        return telemetry_input


    pipeline: StreamPipeline = kafka_client.stream_topic(TOPIC_NAME).into(stream_input_handler)
