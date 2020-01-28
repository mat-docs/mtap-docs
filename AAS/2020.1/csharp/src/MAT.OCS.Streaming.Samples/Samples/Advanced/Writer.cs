using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MAT.OCS.Streaming.IO;
using MAT.OCS.Streaming.IO.Session;
using MAT.OCS.Streaming.IO.TelemetryData;
using MAT.OCS.Streaming.IO.TelemetrySamples;
using MAT.OCS.Streaming.Model;
using MAT.OCS.Streaming.Model.AtlasConfiguration;
using MAT.OCS.Streaming.Model.DataFormat;

namespace MAT.OCS.Streaming.Samples.Samples
{
    public class Writer : IDisposable
    {
        private readonly Dictionary<string, TelemetryDataFeedOutput> dataOutputs =
            new Dictionary<string, TelemetryDataFeedOutput>();

        private readonly Dictionary<string, TelemetrySamplesFeedOutput> sampleOutputs =
            new Dictionary<string, TelemetrySamplesFeedOutput>();

        private readonly SessionTelemetryDataOutput session;

        public readonly SessionOutput SessionOutput;

        public Writer(Uri dependencyServiceUri, AtlasConfiguration atlasConfiguration, DataFormat dataFormat,
            string group, IOutputTopic topic, bool enableCache = true)
        {
            var httpDependencyClient =
                new HttpDependencyClient(dependencyServiceUri, group,
                    enableCache); // DependencyClient stores the Data format, Atlas Configuration
            var dataFormatClient = new DataFormatClient(httpDependencyClient);
            var atlasConfigurationClient = new AtlasConfigurationClient(httpDependencyClient);
            var atlasConfigurationId =
                atlasConfigurationClient
                    .PutAndIdentifyAtlasConfiguration(atlasConfiguration); // Uniq ID created for the AtlasConfiguration
            var dataFormatId =
                dataFormatClient.PutAndIdentifyDataFormat(dataFormat); // Uniq ID created for the Data Format
            TopicName = topic.TopicName;

            //Init Session
            session = new SessionTelemetryDataOutput(topic, dataFormatId, dataFormatClient);
            session.SessionOutput.AddSessionDependency(DependencyTypes.DataFormat, dataFormatId);
            session.SessionOutput.AddSessionDependency(DependencyTypes.AtlasConfiguration, atlasConfigurationId);
            SessionOutput = session.SessionOutput;
        }

        public string TopicName { get; }
        public DateTime? SessionStart => session?.SessionOutput?.SessionStart;


        public void Dispose()
        {
            if (session.SessionOutput.SessionState == StreamSessionState.Closed) return;
            session.SessionOutput.SessionState = StreamSessionState.Truncated;
            session.SessionOutput.SendSession();
        }

        public void OpenSession(string sessionIdentifier)
        {
            if (session.SessionOutput.SessionState == StreamSessionState.Open) return;
            session.SessionOutput.SessionState = StreamSessionState.Open;
            session.SessionOutput.SessionStart = DateTime.Now;
            session.SessionOutput.SessionIdentifier = sessionIdentifier;
            session.SessionOutput.SendSession();
        }

        public void CloseSession()
        {
            if (session.SessionOutput.SessionState == StreamSessionState.Closed) return;
            // should be closed not truncated
            session.SessionOutput.SessionState = StreamSessionState.Closed;
            session.SessionOutput.SendSession();
        }

        public void Write(string feedName, TelemetryData telemetryData)
        {
            if (!dataOutputs.ContainsKey(feedName) || dataOutputs[feedName] == null)
                dataOutputs[feedName] = session.DataOutput.BindFeed(feedName);

            //get the feed from data feed outputs
            if (!dataOutputs.TryGetValue(feedName, out var outputFeed)) throw new Exception("Feed not found");

            Task.WaitAll(outputFeed.EnqueueAndSendData(telemetryData));
            // Set earliest epochnano
            // set latest timestamp offset by its epoch
            // Figure out Session duration based on the difference of the above 2
            // output.SessionOutput.SessionDurationNanos =  output.SessionOutput.Sess
        }

        public void Write(List<string> feedNames, TelemetryData telemetryData)
        {
            foreach (var feedName in feedNames) Write(feedName, telemetryData);
        }

        public void Write(string feedName, TelemetrySamples telemetrySamples)
        {
            if (!sampleOutputs.ContainsKey(feedName) || sampleOutputs[feedName] == null)
                sampleOutputs[feedName] = session.SamplesOutput.BindFeed(feedName);

            //get the feed from data feed outputs
            if (!sampleOutputs.TryGetValue(feedName, out var outputFeed)) throw new Exception("Feed not found");

            Task.WaitAll(outputFeed.SendSamples(telemetrySamples));
        }
    }
}