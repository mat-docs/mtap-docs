using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MAT.OCS.Streaming;
using MAT.OCS.Streaming.IO;
using MAT.OCS.Streaming.IO.TelemetryData;
using MAT.OCS.Streaming.Kafka;
using MAT.OCS.Streaming.Model;
using MAT.OCS.Streaming.Model.AtlasConfiguration;
using MAT.OCS.Streaming.Model.DataFormat;

namespace MAT.OCS.Streaming.Samples.Models
{
    public class ModelSample
    {
        private const string DependencyUrl = "http://localhost:8180/api/dependencies/";
        private const string InputTopicName = "ModelsInput";
        private const string OutputTopicName = "ModelsOutput";
        private const string BrokerList = "localhost";

        private string dataFormatId;
        private string atlasConfId;
        private HttpDependencyClient dependencyClient;
        private DataFormatClient dataFormatClient;
        private AtlasConfigurationClient acClient;

        public ModelSample()
        {
            this.dependencyClient = new HttpDependencyClient(new Uri(DependencyUrl), "dev", false);

            this.dataFormatClient = new DataFormatClient(dependencyClient);
            this.acClient = new AtlasConfigurationClient(dependencyClient);
        }

        public void Run(CancellationToken cancellationToken = default(CancellationToken))
        {
            var outputDataFormat = DataFormat.DefineFeed()
                            .Parameter("gTotal:vTag")
                            .AtFrequency(100)
                            .BuildFormat();

            this.dataFormatId = dataFormatClient.PutAndIdentifyDataFormat(outputDataFormat);


            var atlasConfiguration = this.CreateAtlasConfiguration();
            this.atlasConfId = this.acClient.PutAndIdentifyAtlasConfiguration(atlasConfiguration);

            using (var client = new KafkaStreamClient(BrokerList))
            using (var outputTopic = client.OpenOutputTopic(OutputTopicName))
            using (var pipeline = client.StreamTopic(InputTopicName).Into(streamId => this.CreateStreamPipeline(streamId, outputTopic)))
            {
                cancellationToken.WaitHandle.WaitOne();
                pipeline.Drain();
                pipeline.WaitUntilStopped(TimeSpan.FromSeconds(1), CancellationToken.None);
            }
        }

        private IStreamInput CreateStreamPipeline(string streamId, IOutputTopic outputTopic)
        {
            var streamModel = new StreamModel(this.dataFormatClient, outputTopic, this.dataFormatId, this.atlasConfId);

            return streamModel.CreateStreamInput(streamId);
        }

        protected AtlasConfiguration CreateAtlasConfiguration()
        {
            return new AtlasConfiguration
            {
                AppGroups = new Dictionary<string, ApplicationGroup>
                {
                    {
                        "Models", new ApplicationGroup
                        {
                            Groups = new Dictionary<string, ParameterGroup>
                            {
                                {
                                    "vTag", new ParameterGroup
                                    {
                                        Parameters = new Dictionary<string, Parameter>
                                        {
                                            {
                                                "gTotal:vTag", new Parameter
                                                {
                                                    Name = "gTotal",
                                                    PhysicalRange = new Range(0,10)
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            };
        }
    }
}
