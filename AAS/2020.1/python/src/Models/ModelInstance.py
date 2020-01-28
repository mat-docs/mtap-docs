import math
from pprint import pprint

from mat.ocs.streaming.IO.DataStatus import DataStatus
from mat.ocs.streaming.IO.SessionTelemetryDataInput import SessionTelemetryDataInput
from mat.ocs.streaming.IO.SessionTelemetryDataOutput import SessionTelemetryDataOutput
from mat.ocs.streaming.IO.TelemetryData.TelemetryDataFeedOutput import TelemetryDataFeedOutput
from mat.ocs.streaming.IO.DependencyTypes import DependencyTypes
from mat.ocs.streaming.TelemetryDataFeedEventArgs import TelemetryDataFeedEventArgs
from mat.ocs.streaming.clients.pipeline.StreamInput import StreamInput
from mat.ocs.streaming.models import Model, TransformedTelemetryData


class ModelInstance:
    def __init__(
            self,
            data_format_client,
            output_topic,
            output_data_format_id,
            output_atlas_conf_id):
        self._output_atlas_conf_id = output_atlas_conf_id
        self._output_data_format_id = output_data_format_id
        self._output_topic = output_topic
        self._data_format_client = data_format_client
        self._output_feed = None

    def stream_input_handler(self, stream_id: str) -> StreamInput:
        print("Streaming session: " + stream_id)

        telemetry_input = SessionTelemetryDataInput(stream_id=stream_id, data_format_client=self._data_format_client)
        output = SessionTelemetryDataOutput(output_topic=self._output_topic, data_format_id=self._output_data_format_id,
                                            data_format_client=self._data_format_client)
        self._output_feed: TelemetryDataFeedOutput = output.data_output.bind_default_feed()

        output.session_output.add_session_dependency(DependencyTypes.data_format, self._output_data_format_id)
        output.session_output.add_session_dependency(DependencyTypes.atlas_configuration, self._output_atlas_conf_id)

        telemetry_input.data_input.bind_default_feed().data_buffered += self.gTotal_model
        telemetry_input.laps_input.lap_completed += lambda s, e: output.laps_output.send(e.lap)
        telemetry_input.stream_finished += lambda x, y: pprint("Stream " + stream_id + " ended.")

        output.session_output.session.model = Model("Test", 2, "Par1=abc")

        telemetry_input.link_to_output(
            session_output=output.session_output,
            identifier_transform=lambda identifier: identifier + "_models")

        return telemetry_input

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

