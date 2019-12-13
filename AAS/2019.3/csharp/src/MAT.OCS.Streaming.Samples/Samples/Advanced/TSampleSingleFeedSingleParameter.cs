using MAT.OCS.Streaming.Model.AtlasConfiguration;
using MAT.OCS.Streaming.Model.DataFormat;
using System;
using System.Collections.Generic;
using MAT.OCS.Streaming.IO.TelemetrySamples;
using MAT.OCS.Streaming.Model;
using MAT.OCS.Streaming.Samples.Adapters;
using System.Threading;

namespace MAT.OCS.Streaming.Samples.Samples
{
    class TSampleSingleFeedSingleParameter
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
                                            new Parameter {Name = ParameterName, Units = ParameterUnits, PhysicalRange = new Range(0, 400), WarningRange = new Range(0, 350)}
                                        }
                                    }
                                }
                            }
                        }, 
                    }
                }
            }
        };

        /// <summary>
        /// Creates a DataFormat object from the default feed and the given frequency and parameterId.
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

            // Adding parameter to the previously started feed by its parameterId.
            dataFormatBuilder.Parameter(ParameterId);

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

        public static void Write()
        {
            Uri dependencyUri = new Uri("http://localhost:8180/api/dependencies/"); // The URI where the dependency services are running
            string brokerList = "localhost:9092"; // The host and port where the Kafka broker is running
            string groupName = "dev"; // The group name
            string topicName = "sample_in"; // The existing topic's name in the Kafka broker. The *_announce topic name must exist too. In this case the sample_in_announce
            var streamAdapter = new KafkaStreamAdapter(brokerList, "writeConsumerGroup"); // The steam adapter used to manage streams and topics in the broker. Consumer group must be unique for each stream.
            using (var topicOutput = streamAdapter.OpenOutputTopic(topicName)) // Open a KafkaOutputTopic
            {
                using (var writer = new Writer(dependencyUri, AtlasConfiguration, GetDataFormat(), groupName, topicOutput)) // Create a Writer object that is used to manage the session and write data
                {
                    var feedName = ""; // As sample DataFormat uses default feed, we will leave this empty.
                    writer.OpenSession("sample_" + DateTime.Now); // The session must be opened before Write

                    for (var i = 0; i < 5; i++) // Write 5 times within the session, but different data.
                    {
                        if (writer.SessionStart == null)
                        {
                            continue;
                        }

                        var generatedData = GenerateSamples(10, (DateTime)writer.SessionStart); // Generate some TelemetryData
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
                using (var pipeline = reader.ReadTSamples(ParameterId, Models.TraceSamples)) // TelemetryDataHandler parameter can be used to handle the data read from the stream.
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
                        using (var pipeline = reader.ReadAndLinkTSamples(ParameterId, Models.TraceSamples, writer, outputFeedName)) // TelemetryDataHandler parameter can be used to handle the data read from the stream.
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
