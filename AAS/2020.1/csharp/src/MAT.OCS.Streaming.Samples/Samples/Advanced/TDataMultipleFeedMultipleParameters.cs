using MAT.OCS.Streaming.IO.TelemetryData;
using MAT.OCS.Streaming.Model.AtlasConfiguration;
using MAT.OCS.Streaming.Model.DataFormat;
using System;
using System.Collections.Generic;
using MAT.OCS.Streaming.Model;
using System.Linq;
using MAT.OCS.Streaming.Samples.Adapters;
using System.Threading;

namespace MAT.OCS.Streaming.Samples.Samples
{
    class TDataMultipleFeedMultipleParameters
    {
        // The data tree structure in Atlas is built up by these values, and would look like this:
        // Math 
        //        |- State 
        //               |- Sin(x)
        //                      (Sin(x):Math, ...)
        //               |- Cos(x)
        //                      (Cos(x):Math, ...)
        //               |- Radian
        //                      (Radian:Math, ...)
        //               |- Degree
        //                      (Degree:Math, ...)
        private const string AppGroupId = "Math";
        private const string ParameterGroupId = "State";
        private const string SinParameterId = "Sin(x):Math";
        private const string SinParameterName = "Sin(x)";
        private const string CosParameterId = "Cos(x):Math";
        private const string CosParameterName = "Cos(x)";
        private const string RadianParameterId = "Radian:Math";
        private const string RadianParameterName = "Radian";
        private const string DegreeParameterId = "Degree:Math";
        private const string DegreeParameterName = "Degree";

        private const double Frequency = 100; // The frequency used in the DataFormat. The default frequency is 100 anyway.
        private const long Interval = (long)(1000 / Frequency * 1000000L); // interval in nanoseconds, calculated by the given frequency

        private static readonly List<Tuple<string, double, string>> FeedNamFrequencyAndParameterList = new List<Tuple<string, double, string>>
        {
            Tuple.Create("feed1", 100D, "Sin(x):Math"),
            Tuple.Create("feed2", 100D, "Cos(x):Math"),
            Tuple.Create("feed3", 10D, "Cos(x):Math")
        };

        // A single group and multiple parameter Atlas configuration sample.
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
                                        {SinParameterId, new Parameter
                                        {
                                            Name = SinParameterName,
                                            PhysicalRange = new Range(-Math.PI/2, Math.PI/2)
                                        }},
                                        {CosParameterId, new Parameter
                                        {
                                            Name = CosParameterName,
                                            PhysicalRange = new Range(-Math.PI/2, Math.PI/2)
                                        }},
                                        {DegreeParameterId, new Parameter
                                        {
                                            Name = DegreeParameterName,
                                            PhysicalRange = new Range(0, 360)
                                        }},
                                        {RadianParameterId, new Parameter
                                        {
                                            Name = RadianParameterName,
                                            PhysicalRange = new Range(0, 6.28)
                                        }}
                                    }
                                }
                            }
                        }
                    }
                }
            }
        };

        /// <summary>
        /// Retrieves each parameter ID from the sample AtlasConfiguration
        /// </summary>
        /// <returns>List of parameter IDs used in the sample AtlasConfiguration.</returns>
        public static List<string> GetParameterIds()
        {
            List<string> result = new List<string>();
            foreach (var g in AtlasConfiguration.AppGroups.Values)
            {
                foreach (var pg in g.Groups.Values)
                {
                    result.AddRange(pg.Parameters.Keys);
                }
            }
            return result;
        }

        //DRS wing open. Raw data in samples. timestamps are completely random, not periodic, 0Hz

        /// <summary>
        /// Creates a DataFormat object with the provided feed names, frequencies and parameterIds (Tuples)
        /// </summary>
        /// <param name="feedNameAndFrequencyAndParameterList">Feed name, frequncy and parameterId pairs used for DataFormat creation.</param>
        /// <returns>The Dataformat object built by the configured DataformatBuilder.</returns>
        public static DataFormat GetDataFormat(List<Tuple<string, double, string>> feedNameAndFrequencyAndParameterList)
        {
            // Data formats can contain multiple feeds, each feed has a frequency, which is 100 by default.
            // Name is optional, but if you wish to define multiple feeds, give them names
            // else previous unnamed feed will be overwritten. The default is the empty string.
            var dataFormatBuilder = DataFormat.DefineFeed(feedNameAndFrequencyAndParameterList.First().Item1);// Use the first feed name for creating the DataFormatBuilder.
            // It will be overwritten in the foreach loop

            var parameterIds = GetParameterIds();

            foreach (var feedNameFrequencyAndParameter in feedNameAndFrequencyAndParameterList)
            {
                var feedName = feedNameFrequencyAndParameter.Item1; // Item1 of the feedNameFrequencyAndParameter is used as the feed name
                var frequency = feedNameFrequencyAndParameter.Item2; // Item2 of the feedNameFrequencyAndParameter is used as the frequency
                var parameterId = feedNameFrequencyAndParameter.Item3; // Item3 of the feedNameFrequencyAndParameter is used as the parameter

                dataFormatBuilder.NextFeed(feedName);

                // Setting frequency to the previously started feed. This can be only one value, if you set
                // it multiple times, previous value will be overwritten. If you wish to use multiple
                // frequencies, use new feed by calling .NextFeed
                dataFormatBuilder.AtFrequency(frequency);


                if (!parameterIds.Contains(parameterId)) // Check if the provided parameter is in the sample Atlas configuration
                    throw new ArgumentException($"Parameter id \"{parameterId}\" is invalid!");

                // VERY IMPORTANT information:
                // The order you add will define which index you'll have to use when adding data to the telemetry data
                // in this case Sin(x) will have index 0, Cos(x) will have index 1
                dataFormatBuilder.Parameter(parameterId); // Adding parameters to the previously started feed.

            }

            var dataFormat = dataFormatBuilder.BuildFormat();

            return dataFormat;
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
                Parameters = new TelemetryParameterData[4] // The sample AtlasConfiguration has 4 parameters
            };

            // The sample AtlasConfiguration has only 4 parameters, so each must be initialized
            var sinData = new TelemetryParameterData()
            {
                AvgValues = new double[sampleCount], // The data will be stored in the AvgValues array, so this must be initialized
                Statuses = new DataStatus[sampleCount] // Status is stored for each data, so Statuses array must be initialized
            };
            data.Parameters[0] = sinData;
            var cosData = new TelemetryParameterData()
            {
                AvgValues = new double[sampleCount],
                Statuses = new DataStatus[sampleCount]
            };
            data.Parameters[1] = cosData;
            var degreeData = new TelemetryParameterData()
            {
                AvgValues = new double[sampleCount],
                Statuses = new DataStatus[sampleCount]
            };
            data.Parameters[2] = degreeData;
            var radianData = new TelemetryParameterData()
            {
                AvgValues = new double[sampleCount],
                Statuses = new DataStatus[sampleCount]
            };
            data.Parameters[3] = radianData;

            for (var i = 0; i < sampleCount; i++) // Setting sin, cos, degree and radian parameter values for every sample
            {
                var degree = i % 360;
                var radian = (Math.PI / 180) * degree;

                data.TimestampsNanos[i] = i * Interval; // timestamps expressed in ns since the epoch, which is the start of the session

                degreeData.AvgValues[i] = degree;
                degreeData.Statuses[i] = DataStatus.Sample;

                radianData.AvgValues[i] = radian;
                radianData.Statuses[i] = DataStatus.Sample;

                //Write sin
                sinData.AvgValues[i] = Math.Sin(radian);
                sinData.Statuses[i] = DataStatus.Sample;

                //Write cos
                cosData.AvgValues[i] = Math.Cos(radian);
                cosData.Statuses[i] = DataStatus.Sample;
            }

            return data;
        }
        public static void Write()
        {
            var dependencyUri = new Uri("http://localhost:8180/api/dependencies/"); // The URI where the dependency services are running
            const string brokerList = "localhost:9092"; // The host and port where the Kafka broker is running
            const string groupName = "dev"; // The group name
            const string topicName = "sample_in"; // The existing topic's name in the Kafka broker. The *_announce topic name must exist too. In this case the sample_in_announce
            var streamAdapter = new KafkaStreamAdapter(brokerList, "writeConsumerGroup"); // The steam adapter used to manage streams and topics in the broker. Consumer group must be unique for each stream.
            using (var topicOutput = streamAdapter.OpenOutputTopic(topicName)) // Open a KafkaOutputTopic
            {
                var dataFormat = GetDataFormat(FeedNamFrequencyAndParameterList); // Get the data format for our feed, frequency and parameterId pairs
                var feedNames = dataFormat.Feeds.Keys; // Feed names must be the same that we just added above

                using (var writer = new Writer(dependencyUri, AtlasConfiguration, dataFormat, groupName, topicOutput)) // Create a Writer object that is used to manage the session and write data
                {
                    writer.OpenSession("sample_" + DateTime.Now); // The session must be opened before Write

                    for (var i = 0; i < 5; i++) // Write 5 times within the session, but different data.
                    {
                        var generatedData = GenerateData(100, (DateTime)writer.SessionStart); // Generate some TelemetryData
                        foreach (var feedName in feedNames)
                        {
                            writer.Write(feedName, generatedData); // Write the TelemetryData to each feed
                        }
                    }
                    writer.CloseSession(); // The session must be closed after writing, otherwise it would be marked as Truncated, which is used as an error flag.
                }
            }
        }

        public static void Read()
        {
            var dependencyUri = new Uri("http://localhost:8180/api/dependencies/"); // The URI where the dependency services are running
            const string brokerList = "localhost:9092"; // The host and port where the Kafka broker is running
            const string groupName = "dev"; // The group name
            const string topicName = "sample_in"; // The existing topic's name in the Kafka broker. The *_announce topic name must exist too. In this case the sample_in_announce
            var streamAdapter = new KafkaStreamAdapter(brokerList, "writeConsumerGroup"); // The steam adapter used to manage streams and topics in the broker. Consumer group must be unique for each stream.
            var stream = streamAdapter.OpenStreamTopic(topicName); // Open the topic for streaming.
            using (var reader = new Reader(dependencyUri, groupName, stream)) // Create a Reader to read from the stream
            {
                using (var pipeline = reader.ReadTData(GetParameterIds(), Models.TraceData)) // TelemetryDataHandler parameter can be used to handle the data read from the stream.
                {
                    Write(); //Write some data to have something to read while connection is open.
                }
            }
        }

        public static void ReadAndLink()
        {
            var dependencyUri = new Uri("http://localhost:8180/api/dependencies/"); // The URI where the dependency services are running
            const string brokerList = "localhost:9092"; // The host and port where the Kafka broker is running
            const string groupName = "dev"; // The group name
            const string readTopicName = "sample_in"; // The existing topic's name in the Kafka broker. The *_announce topic name must exist too. In this case the sample_in_announce
            const string linkTopicName = "sample_out"; // The existing topic's name in the Kafka broker. The *_announce topic name must exist too. In this case the sample_out_announce
            var readAdapter = new KafkaStreamAdapter(brokerList, "readConsumerGroup"); // The steam adapter used to manage streams and topics in the broker. Consumer group must be unique for each stream.
            var writeAdapter = new KafkaStreamAdapter(brokerList, "writeConsumerGroup"); // The steam adapter used to manage streams and topics in the broker. Consumer group must be unique for each stream.
            var stream = readAdapter.OpenStreamTopic(readTopicName); // Open the topic for streaming.

            using (var reader = new Reader(dependencyUri, groupName, stream)) // Create a Reader to read from the stream
            {
                using (var outputTopic = writeAdapter.OpenOutputTopic(linkTopicName)) // Open the output topic, where you want to link the streamed input
                {
                    var dataFormat = GetDataFormat(FeedNamFrequencyAndParameterList); // Get the data format for our feed, frequency and parameterId pairs
                    var feedNames = dataFormat.Feeds.Keys.ToList(); // Feed names must be the same that we just added above

                    using (var writer = new Writer(dependencyUri, AtlasConfiguration, dataFormat, groupName, outputTopic)) // Create a Writer for the output topic and pass
                    {
                        using (var pipeline = reader.ReadAndLinkTData(GetParameterIds(), Models.TraceData, writer, feedNames)) // TelemetryDataHandler parameter can be used to handle the data read from the stream.
                        {
                            Thread.Sleep(5000); // NOTE: without this doesn't seem to work
                            Write(); //Write some data to have something to read while connection is open.
                        }
                    }
                }
            }
        }
    }
}
