namespace MAT.OCS.Streaming.Samples.Adapters
{
    public abstract class StreamAdapter
    {
        public abstract IOutputTopic OpenOutputTopic(string topicName);
        public abstract IStreamPipelineBuilder OpenStreamTopic(string topicName);
    }
}
