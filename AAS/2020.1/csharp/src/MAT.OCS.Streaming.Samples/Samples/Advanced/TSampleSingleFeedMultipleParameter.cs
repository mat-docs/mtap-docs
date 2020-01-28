using MAT.OCS.Streaming.Model.AtlasConfiguration;
using MAT.OCS.Streaming.Model.DataFormat;
using System;
using System.Collections.Generic;
using System.Linq;
using MAT.OCS.Streaming.IO.TelemetrySamples;
using MAT.OCS.Streaming.Model;
using MAT.OCS.Streaming.Samples.Adapters;
using System.Threading;

namespace MAT.OCS.Streaming.Samples.Samples
{
    class TSampleSingleFeedMultipleParameter
    {
        // The data tree structure in Atlas is built up by these values, and would look like this:
        // Chassis 
        //        |- State 
        //               |- vCar
        //                      (vCar:Chassis, kmh ...)
        //        |- GearBox 
        //               |- NGear
        //                      (NGear:Chassis, ...)
        private const string VcarAppGroupId = "Chassis";
        private const string VcarParameterGroupId = "State";
        private const string VcarParameterId = "vCar:Chassis";
        private const string VcarParameterName = "vCar";
        private const string VcarParameterUnits = "kmh";

        private const string GearAppGroupId = "Chassis";
        private const string GearParameterGroupId = "GearBox";
        private const string GearParameterId = "NGear:Chassis";
        private const string GearParameterName = "NGear";

        private const double Frequency = 100;
        private const long Interval = (long)(1000 / Frequency * 1000000L);

        // A multiple group and single parameter Atlas configuration sample.
        public static AtlasConfiguration AtlasConfiguration = new AtlasConfiguration
        {
            AppGroups =
            {
                {
                    VcarAppGroupId, new ApplicationGroup
                    {
                        Groups =
                        {
                            {
                                VcarParameterGroupId, new ParameterGroup
                                {
                                    Parameters =
                                    {
                                        {
                                            VcarParameterId,
                                            new Parameter {Name = VcarParameterName, Units = VcarParameterUnits, PhysicalRange = new Range(0, 300), WarningRange = new Range(0, 350)}
                                        }
                                    }
                                }
                            }
                        }
                    }
                },
                {
                    GearAppGroupId, new ApplicationGroup
                    {
                        Groups =
                        {
                            {
                                GearParameterGroupId, new ParameterGroup
                                {
                                    Parameters =
                                    {
                                        {
                                            GearParameterId,
                                            new Parameter {Name = GearParameterName, PhysicalRange = new Range(-1, 8), WarningRange = new Range(-1, 8)}
                                        }
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

        /// <summary>
        /// Creates a DataFormat object from the default feed and the given frequency and sample parameterId.
        /// </summary>
        /// <returns>The Dataformat object built by the configured DataformatBuilder.</returns>
        public static DataFormat GetDataFormat()
        {
            // Data formats can contain multiple feeds, each feed has a frequency, which is 100 by default.
            // Name is optional, but if you wish to define multiple feeds, give them names
            // else previous unnamed feed will be overwritten. The default is the empty string.
            var dataFormatBuilder = DataFormat.DefineFeed();

            // Setting frequency to the previously started feed. This can be only one value, if you set
            // it multiple times, previous value will be overwritten. If you wish to use multiple
            // frequencies, use new feed by calling .NextFeed()
            dataFormatBuilder.AtFrequency(Frequency);

            // VERY IMPORTANT information:
            // The order you add will define which index you'll have to use when adding data to the telemetry data
            // in this case vCar will have index 0,NGear will have index 1
            dataFormatBuilder.Parameters(VcarParameterId, GearParameterId); // Adding parameters to the previously started feed.

            var dataFormat = dataFormatBuilder.BuildFormat();

            return dataFormat;
        }

        /// <summary>
        /// Generates random TelemetrySamples. 
        /// </summary>
        /// <param name="sampleCount">The sample count that is used to set the size of each TelemetrySamples.Values' size.</param>
        /// <param name="sessionStart">Used to set the EpochNanos property of the TelemetrySamples.</param>
        /// <returns></returns>
        public static TelemetrySamples GenerateSamples(int sampleCount, DateTime sessionStart)
        {
            var vcarSamples = new TelemetryParameterSamples()
            {
                EpochNanos = sessionStart.ToTelemetryTime(),
                TimestampsNanos = new long[sampleCount],
                Values = new double[sampleCount]
            };

            var gearSamples = new TelemetryParameterSamples()
            {
                EpochNanos = sessionStart.ToTelemetryTime(),
                TimestampsNanos = new long[sampleCount],
                Values = new double[sampleCount]
            };

            var randomRangeWalker = new RandomRangeWalker(0, 1); // Used to generated random data

            for (int i = 0; i < sampleCount; i++)
            {
                var nextSample = randomRangeWalker.GetNext();

                // each data has own timestamps, frequencies
                vcarSamples.TimestampsNanos[i] = i * Interval;
                vcarSamples.Values[i] = nextSample * 300;
                gearSamples.TimestampsNanos[i] = i * Interval;
                gearSamples.Values[i] = nextSample;
            }

            var max = vcarSamples.Values.Max();
            var modVal = max / 7 + 1; // Assuming a car has 8 gears, we are splitting the samples to 8 equal ranges and adding 1 to it, so it will be 1 .. 8

            for (int i = 0; i < sampleCount; i++)
            {
                gearSamples.TimestampsNanos[i] = vcarSamples.TimestampsNanos[i];
                gearSamples.Values[i] = Math.Round(vcarSamples.Values[i] / modVal);
            }
            var data = new TelemetrySamples()
            {
                Parameters = new Dictionary<string, TelemetryParameterSamples>()
                {
                    { VcarParameterId , vcarSamples},
                    { GearParameterId , gearSamples} // If you had more samples to send for other parameters, this is where you would add them
                }
            };
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
                using (var writer = new Writer(dependencyUri, AtlasConfiguration, GetDataFormat(), groupName, topicOutput)) // Create a Writer object that is used to manage the session and write data
                {
                    const string feedName = ""; // As sample DataFormat uses default feed, we will leave this empty.
                    writer.OpenSession("sample_" + DateTime.Now); // The session must be opened before Write

                    for (var i = 0; i < 5; i++) // Write 5 times within the session, but different data.
                    {
                        var generatedData = GenerateSamples(100, (DateTime)writer.SessionStart); // Generate some TelemetryData
                        writer.Write(feedName, generatedData); // Write the TelemetryData to the given feed (default "" in this case)
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
                using (var pipeline = reader.ReadTSamples(GetParameterIds(), Models.TraceSamples)) // TelemetryDataHandler parameter can be used to handle the data read from the stream.
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
                    using (var writer = new Writer(dependencyUri, AtlasConfiguration, GetDataFormat(), groupName, outputTopic)) // Create a Writer for the output topic and pass
                    {
                        const string outputFeedName = ""; // As sample DataFormat uses default feed, we will leave this empty.
                        using (IStreamPipeline pipeline = reader.ReadAndLinkTSamples(GetParameterIds(), Models.TraceSamples, writer, outputFeedName)) // TelemetryDataHandler parameter can be used to handle the data read from the stream.
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
