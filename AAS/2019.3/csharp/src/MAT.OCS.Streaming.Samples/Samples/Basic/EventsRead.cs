using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
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
    class EventsRead
    {
        public void ReadEvents()
        {
            const string brokerList = "localhost:9092"; // The host and port where the Kafka broker is running
            const string groupName = "dev"; // The dependency group name
            const string topicName = "events_sample"; // The existing topic's name in the Kafka broker. The *_announce topic name must exist too. In this case the data_in_announce
            var dependencyServiceUri = new Uri("http://localhost:8180/api/dependencies/"); // The URI where the dependency services are running

            var client = new KafkaStreamClient(brokerList); // Create a new KafkaStreamClient for connecting to Kafka broker
            var dataFormatClient = new DataFormatClient(new HttpDependencyClient(dependencyServiceUri, groupName)); // Create a new DataFormatClient
            var atlasConfigurationClient = new AtlasConfigurationClient(new HttpDependencyClient(dependencyServiceUri, groupName)); // Create a new AtlasConfigurationClient

            var pipeline = client.StreamTopic(topicName).Into(streamId => // Stream Kafka topic into the handler method
            {
                var input = new SessionTelemetryDataInput(streamId, dataFormatClient);

                AtlasConfiguration atlasConfiguration = null;

                input.SessionInput.SessionDependenciesChanged += (s, a) =>
                {
                    if (!a.Session.Dependencies.TryGetValue("atlasConfiguration", out var atlasConfigIds))
                    {
                        return;
                    }

                    atlasConfiguration = atlasConfigurationClient.GetAtlasConfiguration(atlasConfigIds[0]); // Taking first atlas configuration for this example
                };

                input.EventsInput.EventsBuffered += (sender, e) => // Subscribe to incoming events
                {
                    if (atlasConfiguration == null)
                    {
                        return;
                    }

                    var events = e.Buffer.GetData(); // read incoming events from buffer

                    // In this sample we consume the incoming events and print it
                    foreach (var ev in events)
                    {
                        var eventDefinition = atlasConfiguration.AppGroups?.First().Value?.Events.GetValueOrDefault(ev.Id);
                        if (eventDefinition == null)
                        {
                            continue;
                        }

                        Console.WriteLine($"- Event: {ev.Id} - {eventDefinition.Description} - Priority: {eventDefinition.Priority.ToString()} - Value: {ev.Values?.First()}");
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

    }
}
