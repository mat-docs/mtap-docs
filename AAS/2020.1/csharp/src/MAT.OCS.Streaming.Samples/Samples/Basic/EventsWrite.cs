using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MAT.OCS.Streaming.IO;
using MAT.OCS.Streaming.Kafka;
using MAT.OCS.Streaming.Model;
using MAT.OCS.Streaming.Model.AtlasConfiguration;
using MAT.OCS.Streaming.Model.DataFormat;

namespace MAT.OCS.Streaming.Samples.Samples.Basic
{
    class EventsWrite
    {
        // The data tree structure in Atlas is built up by these values, and would look like this:
        // Gearbox 
        //        |- State
        //             ("Gearbox state changed", "1to1", High)
        private const string AppGroupId = "Gearbox";
        private const string EventId = "State";
        private const string EventDescription = "Gearbox state changed";
        private static List<string> EventConversions = new List<string> { "1to1" };

        // A single group and single Event Atlas configuration sample.
        public static AtlasConfiguration AtlasConfiguration = new AtlasConfiguration
        {
            AppGroups =
            {
                {
                    AppGroupId, new ApplicationGroup
                    {
                        Events = 
                        {
                            { EventId, new EventDefinition {  Description = EventDescription, ConversionIds = EventConversions, Priority = EventPriority.High } }
                        }
                    }
                }
            }
        };

        public void WriteEvents()
        {
            const string brokerList = "localhost:9092"; // The host and port where the Kafka broker is running
            const string groupName = "dev"; // The dependency group name
            const string topicName = "events_sample"; // The existing topic's name in the Kafka broker. The *_announce topic name must exist too. In this case the data_in_announce
            var dependencyServiceUri = new Uri("http://localhost:8180/api/dependencies/"); // The URI where the dependency services are running

            var client = new KafkaStreamClient(brokerList); // Create a new KafkaStreamClient for connecting to Kafka broker
            var dataFormatClient = new DataFormatClient(new HttpDependencyClient(dependencyServiceUri, groupName)); // Create a new DataFormatClient
            var httpDependencyClient = new HttpDependencyClient(dependencyServiceUri, groupName); // DependencyClient stores the Data format, Atlas Configuration

            var atlasConfigurationId = new AtlasConfigurationClient(httpDependencyClient).PutAndIdentifyAtlasConfiguration(AtlasConfiguration); // Uniq ID created for the AtlasConfiguration
            var dataFormat = DataFormat.DefineFeed().BuildFormat(); // Create a dataformat based on the parameters, using the parameter id
            var dataFormatId = dataFormatClient.PutAndIdentifyDataFormat(dataFormat); // Uniq ID created for the Data Format

            using (var outputTopic = client.OpenOutputTopic(topicName)) // Open a KafkaOutputTopic
            {
                var output = new SessionTelemetryDataOutput(outputTopic, dataFormatId, dataFormatClient);
                output.SessionOutput.AddSessionDependency(DependencyTypes.DataFormat, dataFormatId); // Add session dependencies to the output
                output.SessionOutput.AddSessionDependency(DependencyTypes.AtlasConfiguration, atlasConfigurationId);

                output.SessionOutput.SessionState = StreamSessionState.Open; // set the sessions state to open
                output.SessionOutput.SessionStart = DateTime.Now; // set the session start to current time
                output.SessionOutput.SessionIdentifier = "events_" + DateTime.Now; // set a custom session identifier
                output.SessionOutput.SendSession();

                var events = GenerateEvents(20, (DateTime)output.SessionOutput.SessionStart); // Generate some events data
                var tasks = events.Select(ev => output.EventsOutput.SendEvent(ev)).ToArray(); // enqueue and send the events to the output through the EventsOutput
                Task.WaitAll(tasks); 

                output.SessionOutput.SessionState = StreamSessionState.Closed; // set session state to closed. In case of any unintended session close, set state to Truncated
                output.SessionOutput.SendSession(); // send session
            }
        }
        /// <summary>
        /// Generates random Events. 
        /// </summary>
        /// <param name="EventsCount">The events count we want to generate.</param>
        /// <param name="sessionStart">Used to set the EpochNanos property of the TelemetryData. TelemetryData.Parameters
        /// share the same timestamps as they aligned to the same time.</param>
        /// <returns></returns>
        public static List<Event> GenerateEvents(int eventsCount, DateTime sessionStart)
        {
            var interval = (long)(100000); // interval in nanoseconds between events
            var randomRangeWalker = new RandomRangeWalker(0, 1); // Used to generated random data

            var events = new List<Event>();

            var timestamp = sessionStart.ToTelemetryTime();
            for (int i = 0; i < eventsCount; i++)
            {
                var nextValue = randomRangeWalker.GetNext();

                var @event = new Event
                {
                    Id = EventId,
                    Values = new[] { nextValue },
                    EpochNanos = sessionStart.ToTelemetryTime(),
                    TimeNanos = timestamp
                };

                events.Add(@event);
                timestamp += interval;
            }

            return events;
        }
    }
}
