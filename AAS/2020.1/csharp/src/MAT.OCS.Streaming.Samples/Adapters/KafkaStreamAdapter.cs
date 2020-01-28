using MAT.OCS.Streaming.Kafka;
namespace MAT.OCS.Streaming.Samples.Adapters
{
    public class KafkaStreamAdapter : StreamAdapter
    {
        private readonly KafkaStreamClient client;

        public KafkaStreamAdapter(string brokerList, string consumerGroup)
        {
            client = new KafkaStreamClient(brokerList) {ConsumerGroup = consumerGroup};
        }

        public override IOutputTopic OpenOutputTopic(string topicName)
        {
            return client.OpenOutputTopic(topicName);
        }

        public override IStreamPipelineBuilder OpenStreamTopic(string topicName)
        {
            return client.StreamTopic(topicName);
        }
    }
}
