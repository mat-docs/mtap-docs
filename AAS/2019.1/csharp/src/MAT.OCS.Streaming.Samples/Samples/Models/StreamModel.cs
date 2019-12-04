using System;
using MAT.OCS.Streaming;
using MAT.OCS.Streaming.IO;
using MAT.OCS.Streaming.IO.TelemetryData;
using MAT.OCS.Streaming.Model;

namespace MAT.OCS.Streaming.Samples.Models
{
    public class StreamModel
    {
        private readonly DataFormatClient dataFormatClient;
        private readonly IOutputTopic outputTopic;
        private readonly string outputDataFormatId;
        private readonly string outputAtlasConfId;
        private TelemetryDataFeedOutput outputFeed;

        public StreamModel(DataFormatClient dataFormatClient, IOutputTopic outputTopic, string outputDataFormatId, string outputAtlasConfId)
        {
            this.dataFormatClient = dataFormatClient ?? throw new System.ArgumentNullException(nameof(dataFormatClient));
            this.outputTopic = outputTopic ?? throw new System.ArgumentNullException(nameof(outputTopic));
            this.outputDataFormatId = outputDataFormatId ?? throw new ArgumentNullException(nameof(outputDataFormatId));
            this.outputAtlasConfId = outputAtlasConfId ?? throw new ArgumentNullException(nameof(outputAtlasConfId));
        }

        public IStreamInput CreateStreamInput(string streamId)
        {
            Console.WriteLine($"Stream {streamId} started.");

            // these templates provide commonly-combined data, but you can make your own
            var input = new SessionTelemetryDataInput(streamId, dataFormatClient);
            var output = new SessionTelemetryDataOutput(outputTopic, this.outputDataFormatId, dataFormatClient);

            this.outputFeed = output.DataOutput.BindFeed("");
            
            // we add output format reference to output session.
            output.SessionOutput.AddSessionDependency(DependencyTypes.DataFormat, this.outputDataFormatId);
            output.SessionOutput.AddSessionDependency(DependencyTypes.AtlasConfiguration, this.outputAtlasConfId);

            // automatically propagate session metadata and lifecycle
            input.LinkToOutput(output.SessionOutput, identifier => identifier + "_Models");

            // we simply forward laps.
            input.LapsInput.LapStarted += (s, e) => output.LapsOutput.SendLap(e.Lap);

            // we bind our models to specific feed and parameters.
            input.DataInput.BindDefaultFeed("gLat:Chassis", "gLong:Chassis").DataBuffered += this.gTotalModel;

            input.StreamFinished += (s, e) => Console.WriteLine($"Stream {e.StreamId} ended.");

            return input;
        }

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
    }
}