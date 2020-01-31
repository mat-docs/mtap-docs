using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MAT.OCS.Streaming.Codecs.Protobuf;
using MAT.OCS.Streaming.IO;
using MAT.OCS.Streaming.IO.TelemetrySamples;
using MAT.OCS.Streaming.Kafka;
using MAT.OCS.Streaming.Model;
using MAT.OCS.Streaming.Model.AtlasConfiguration;
using MAT.OCS.Streaming.Model.DataFormat;

namespace MAT.OCS.Streaming.Samples.Samples.Basic
{
    public class TSamples
    {
        // The data tree structure in Atlas is built up by these values, and would look like this:
        // Chassis 
        //        |- State 
        //               |- vCar
        //                      (vCar:Chassis, kmh, ...)
        private const string AppGroupId = "Chassis";
        private const string ParameterGroupId = "State";
        private const string ParameterId = "vCar:Chassis";
        private const string ParameterName = "vCar";
        private const string ParameterUnits = "kmh";

        private const double Frequency = 100; // The frequency used in the DataFormat. The default frequency is 100 anyway.
        private const long Interval = (long)(1000 / Frequency * 1000000L); // interval in nanoseconds, calculated by the given frequency

        // A single group and single parameter Atlas configuration sample.
        public static AtlasConfiguration AtlasConfiguration = new AtlasConfiguration
        {
            AppGroups =
            {
                {
                    AppGroupId, new ApplicationGroup
                    {
                        Groups =
                        {
                            {
                                ParameterGroupId, new ParameterGroup
                                {
                                    Parameters =
                                    {
                                        {
                                            ParameterId,
                                            new Parameter {Name = ParameterName, Units = ParameterUnits}
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        };

        public void ReadTSamples()
        {
            ProtobufCodecs.RegisterCodecs(true); // Enable Protobuff codec if the streamed data is Protobuff encoded

            const string brokerList = "localhost:9092"; // The host and port where the Kafka broker is running
            const string groupName = "dev"; // The group name
            const string topicName = "sample_in"; // The existing topic's name in the Kafka broker. The *_announce topic name must exist too. In this case the sample_in_announce

            var dependencyServiceUri = new Uri("http://localhost:8180/api/dependencies/"); // The URI where the dependency services are running
            var client = new KafkaStreamClient(brokerList); // Create a new KafkaStreamClient for connecting to Kafka broker
            var dataFormatClient = new DataFormatClient(new HttpDependencyClient(dependencyServiceUri, groupName)); // Create a new DataFormatClient
            
            var pipeline = client.StreamTopic(topicName).Into(streamId => // Stream Kafka topic into the handler method
            {
                var input = new SessionTelemetryDataInput(streamId, dataFormatClient);

                input.SamplesInput.AutoBindFeeds((s, e) => // Take the input and bind feed to an event handler
                {
                    var data = e.Data;// The event handler here only takes the samples data 
                    Trace.WriteLine(data.Parameters.First().Key); // and prints some information to the debug console
                    Trace.WriteLine(data.Parameters.Count);
                });

                input.StreamFinished += (sender, e) => Trace.WriteLine("Finished"); // Handle the steam finished event
                return input;
            });

            if (!pipeline.WaitUntilConnected(TimeSpan.FromMinutes(2), default(CancellationToken))) // Wait until the connection is established
                throw new Exception("Couldn't connect");

            pipeline.WaitUntilFirstStream(TimeSpan.FromMinutes(5), CancellationToken.None); // Wait until the first stream is ready to read.
            pipeline.WaitUntilIdle(TimeSpan.FromMinutes(5), CancellationToken.None); // Wait for 5 minutes of the pipeline being idle before exit.

            pipeline.Dispose();
        }

        public void WriteTSamples()
        {
            const string brokerList = "localhost:9092"; // The host and port where the Kafka broker is running
            const string groupName = "dev"; // The group name
            const string topicName = "sample_in"; // The existing topic's name in the Kafka broker. The *_announce topic name must exist too. In this case the sample_in_announce
            var dependencyServiceUri = new Uri("http://localhost:8180/api/dependencies/"); // The URI where the dependency services are running

            var client = new KafkaStreamClient(brokerList); // Create a new KafkaStreamClient for connecting to Kafka broker
            var dataFormatClient = new DataFormatClient(new HttpDependencyClient(dependencyServiceUri, groupName)); // Create a new DataFormatClient
            var httpDependencyClient = new HttpDependencyClient(dependencyServiceUri, groupName); // DependencyClient stores the Data format, Atlas Configuration

            var atlasConfigurationId = new AtlasConfigurationClient(httpDependencyClient).PutAndIdentifyAtlasConfiguration(AtlasConfiguration); // Uniq ID created for the AtlasConfiguration
            var dataFormat = DataFormat.DefineFeed().Parameter(ParameterId).BuildFormat(); // Create a dataformat based on the parameters, using the parameter id
            var dataFormatId = dataFormatClient.PutAndIdentifyDataFormat(dataFormat); // Uniq ID created for the Data Format

            using (var outputTopic = client.OpenOutputTopic(topicName)) // Open a KafkaOutputTopic
            {
                const int sampleCount = 1000;
                var output = new SessionTelemetryDataOutput(outputTopic, dataFormatId, dataFormatClient);
                output.SessionOutput.AddSessionDependency(DependencyTypes.DataFormat, dataFormatId); // Add session dependencies to the output
                output.SessionOutput.AddSessionDependency(DependencyTypes.AtlasConfiguration, atlasConfigurationId);

                output.SessionOutput.SessionState = StreamSessionState.Open; // set the sessions state to open
                output.SessionOutput.SessionStart = DateTime.Now; // set the session start to current time
                output.SessionOutput.SessionDurationNanos = sampleCount * Interval; // duration should be time elapsed between session start time and last sample time
                output.SessionOutput.SessionIdentifier = "sample_" + DateTime.Now; // set a custom session identifier
                output.SessionOutput.SendSession(); // send session details

                var telemetrySamples = GenerateSamples(sampleCount, (DateTime)output.SessionOutput.SessionStart); // Generate some telemetry samples data

                const string feedName = ""; // As sample DataFormat uses default feed, we will leave this empty.
                var outputFeed = output.SamplesOutput.BindFeed(feedName); // bind your feed by its name to the Samples Output

                Task.WaitAll(outputFeed.SendSamples(telemetrySamples)); // send the samples to the output through the outputFeed

                output.SessionOutput.SessionState = StreamSessionState.Closed; // set session state to closed. In case of any unintended session close, set state to Truncated
                output.SessionOutput.SendSession(); // send session details
            }

        }

        /// <summary>
        /// Generates random TelemetrySamples. 
        /// </summary>
        /// <param name="sampleCount">The sample count that is used to set the size of each TelemetrySamples.Values' size.</param>
        /// <param name="sessionStart">Used to set the EpochNanos property of the TelemetrySamples.</param>
        /// <returns></returns>
        public static TelemetrySamples GenerateSamples(int sampleCount, DateTime sessionStart)
        {
            var sample = new TelemetryParameterSamples()
            {
                EpochNanos = sessionStart.ToTelemetryTime(),
                TimestampsNanos = new long[sampleCount],
                Values = new double[sampleCount]
            };

            var randomRangeWalker = new RandomRangeWalker(0, 1); // Used to generated random data

            for (int i = 0; i < sampleCount; i++)
            {
                var nextSample = randomRangeWalker.GetNext();
                sample.TimestampsNanos[i] = i * Interval;
                sample.Values[i] = nextSample;
            }

            var data = new TelemetrySamples()
            {
                Parameters = new Dictionary<string, TelemetryParameterSamples>()
                {
                    { ParameterId , sample} // If you had more samples to send for other parameters, this is where you would add them
                }
            };
            return data;
        }
    }
}
