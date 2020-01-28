using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using MAT.OCS.Streaming.IO;
using MAT.OCS.Streaming.IO.TelemetryData;
using MAT.OCS.Streaming.Kafka;
using MAT.OCS.Streaming.Model;
using MAT.OCS.Streaming.Model.AtlasConfiguration;
using MAT.OCS.Streaming.Model.DataFormat;

namespace MAT.OCS.Streaming.Samples.Samples.Basic
{
    class TData
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

        public void ReadTData()
        {
            const string brokerList = "localhost:9092"; // The host and port where the Kafka broker is running
            const string groupName = "dev"; // The group name
            const string topicName = "data_in"; // The existing topic's name in the Kafka broker. The *_announce topic name must exist too. In this case the data_in_announce
            var dependencyServiceUri = new Uri("http://localhost:8180/api/dependencies/"); // The URI where the dependency services are running

            var client = new KafkaStreamClient(brokerList); // Create a new KafkaStreamClient for connecting to Kafka broker
            var dataFormatClient = new DataFormatClient(new HttpDependencyClient(dependencyServiceUri, groupName)); // Create a new DataFormatClient

            var pipeline = client.StreamTopic(topicName).Into(streamId => // Stream Kafka topic into the handler method
            {
                var input = new SessionTelemetryDataInput(streamId, dataFormatClient);
                input.DataInput.BindDefaultFeed(ParameterId).DataBuffered += (sender, e) => // Bind the incoming feed and take the data
                {
                    var data = e.Buffer.GetData();
                    // In this sample we consume the incoming data and print it
                    var time = data.TimestampsNanos;
                    for (var i = 0; i < data.Parameters.Length; i++)
                    {
                        Trace.WriteLine($"Parameter[{i}]:");
                        var vCar = data.Parameters[i].AvgValues;
                        for (var j = 0; j < time.Length; j++)
                        {
                            var fromMilliseconds = TimeSpan.FromMilliseconds(time[j].NanosToMillis());
                            Trace.WriteLine($"{fromMilliseconds:hh\\:mm\\:ss\\.fff}, {  new string('.', (int)(50 * vCar[j])) }");
                        }
                    }
                };

                input.StreamFinished += (sender, e) => Trace.WriteLine("Finished"); // Handle the steam finished event
                return input;
            });

            if (!pipeline.WaitUntilConnected(TimeSpan.FromSeconds(30), CancellationToken.None)) // Wait until the connection is established
                throw new Exception("Couldn't connect");
            pipeline.WaitUntilFirstStream(TimeSpan.FromMinutes(1), CancellationToken.None); // Wait until the first stream is ready to read.
            pipeline.WaitUntilIdle(TimeSpan.FromMinutes(5), CancellationToken.None); // Wait for 5 minutes of the pipeline being idle before exit.

            pipeline.Dispose();
        }

        public void WriteTData()
        {
            const string brokerList = "localhost:9092"; // The host and port where the Kafka broker is running
            const string groupName = "dev"; // The group name
            const string topicName = "data_in"; // The existing topic's name in the Kafka broker. The *_announce topic name must exist too. In this case the data_in_announce
            var dependencyServiceUri = new Uri("http://localhost:8180/api/dependencies/"); // The URI where the dependency services are running

            var client = new KafkaStreamClient(brokerList); // Create a new KafkaStreamClient for connecting to Kafka broker
            var dataFormatClient = new DataFormatClient(new HttpDependencyClient(dependencyServiceUri, groupName)); // Create a new DataFormatClient
            var httpDependencyClient = new HttpDependencyClient(dependencyServiceUri, groupName); // DependencyClient stores the Data format, Atlas Configuration

            var atlasConfigurationId = new AtlasConfigurationClient(httpDependencyClient).PutAndIdentifyAtlasConfiguration(AtlasConfiguration); // Uniq ID created for the AtlasConfiguration
            var dataFormat = DataFormat.DefineFeed().Parameter(ParameterId).BuildFormat(); // Create a dataformat based on the parameters, using the parameter id
            var dataFormatId = dataFormatClient.PutAndIdentifyDataFormat(dataFormat); // Uniq ID created for the Data Format

            using (var outputTopic = client.OpenOutputTopic(topicName)) // Open a KafkaOutputTopic
            {
                const int sampleCount = 10000;
                var output = new SessionTelemetryDataOutput(outputTopic, dataFormatId, dataFormatClient);
                output.SessionOutput.AddSessionDependency(DependencyTypes.DataFormat, dataFormatId); // Add session dependencies to the output
                output.SessionOutput.AddSessionDependency(DependencyTypes.AtlasConfiguration, atlasConfigurationId);

                output.SessionOutput.SessionState = StreamSessionState.Open; // set the sessions state to open
                output.SessionOutput.SessionStart = DateTime.Now; // set the session start to current time
                output.SessionOutput.SessionDurationNanos = sampleCount * Interval; // duration should be time elapsed between session start time and last sample time
                output.SessionOutput.SessionIdentifier = "data_" + DateTime.Now; // set a custom session identifier
                output.SessionOutput.SendSession();

                var telemetryData = GenerateData(sampleCount, (DateTime)output.SessionOutput.SessionStart); // Generate some telemetry data

                const string feedName = ""; // As sample DataFormat uses default feed, we will leave this empty.
                var outputFeed = output.DataOutput.BindFeed(feedName); // bind your feed by its name to the Data Output

                Task.WaitAll(outputFeed.EnqueueAndSendData(telemetryData)); // enqueue and send the data to the output through the outputFeed

                output.SessionOutput.SessionState = StreamSessionState.Closed; // set session state to closed. In case of any unintended session close, set state to Truncated
                output.SessionOutput.SendSession(); // send session
            }
        }
        /// <summary>
        /// Generates random TelemetryData. 
        /// </summary>
        /// <param name="sampleCount">The sample count that is used to set the size of each TelemetryData.Parameter values' size.</param>
        /// <param name="sessionStart">Used to set the EpochNanos property of the TelemetryData. TelemetryData.Parameters
        /// share the same timestamps as they aligned to the same time.</param>
        /// <returns></returns>
        public static TelemetryData GenerateData(int sampleCount, DateTime sessionStart)
        {
            var data = new TelemetryData()
            {
                EpochNanos = sessionStart.ToTelemetryTime(),
                TimestampsNanos = new long[sampleCount],
                Parameters = new TelemetryParameterData[1] // The sample AtlasConfiguration has only 1 parameter
            };

            // The sample AtlasConfiguration has only 1 parameter, so data.Parameters[0] must be initialized only.
            data.Parameters[0] = new TelemetryParameterData()
            {
                AvgValues = new double[sampleCount], // The data will be stored in the AvgValues array, so this must be initialized
                Statuses = new DataStatus[sampleCount] // Status is stored for each data, so Statuses array must be initialized
            };

            var randomRangeWalker = new RandomRangeWalker(0, 1); // Used to generated random data

            var timestamp = sessionStart.ToTelemetryTime();
            for (int i = 0; i < sampleCount; i++)
            {
                var nextData = randomRangeWalker.GetNext();
                data.TimestampsNanos[i] = timestamp; // timestamps expressed in ns since the epoch, which is the start of the session
                data.Parameters[0].AvgValues[i] = nextData;
                data.Parameters[0].Statuses[i] = DataStatus.Sample;

                timestamp+=Interval;
            }

            return data;
        }
    }
}
