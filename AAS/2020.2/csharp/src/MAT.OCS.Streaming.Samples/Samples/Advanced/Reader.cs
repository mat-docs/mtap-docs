using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using MAT.OCS.Streaming.IO;
using static MAT.OCS.Streaming.Samples.Samples.Models;

namespace MAT.OCS.Streaming.Samples.Samples
{
    public class Reader : IDisposable
    {
        private readonly TimeSpan connectionTimeoutInSeconds = TimeSpan.FromSeconds(30);

        /// <summary>
        ///     Reader constructor to set up the initial connections to Dependency Client, DataFormatClient and the
        ///     StreamPipelineBuilder for reading the stream
        /// </summary>
        /// <param name="dependencyServiceUri">The URI where the dependency service is running and accessible</param>
        /// <param name="group">The environment specific group name</param>
        /// <param name="pipelineBuilder">The pipeline builder for the stream to read from</param>
        /// <param name="enableCache">Boolean flag to enable dependency caching in <see cref="HttpDependencyClient" /> </param>
        public Reader(Uri dependencyServiceUri, string group, IStreamPipelineBuilder pipelineBuilder,
            bool enableCache = true)
        {
            var dependenciesClient = new HttpDependencyClient(dependencyServiceUri, group, enableCache);
            DataFormatClient = new DataFormatClient(dependenciesClient);
            StreamPipelineBuilder = pipelineBuilder;
        }

        private DataFormatClient DataFormatClient { get; }
        public IStreamPipelineBuilder StreamPipelineBuilder { get; }

        public void Dispose()
        {
        }

        /// <summary>
        ///     Read telemetry data from the stream for the given list of dataformat parameters and handle the data by the
        ///     TelemetryDataHandler delegate
        /// </summary>
        /// <param name="parameterIdentifiers">List of dataformat parameter identifiers</param>
        /// <param name="handler">Use this for the streamed data handling, updating etc.</param>
        /// <returns>Pipeline representing the network resource streaming messages to the inputs.</returns>
        public IStreamPipeline ReadTData(List<string> parameterIdentifiers, TelemetryDataHandler handler)
        {
            var pipeline = StreamPipelineBuilder.Into(streamId => ReadTData(streamId, parameterIdentifiers, handler));

            if (!pipeline.WaitUntilConnected(connectionTimeoutInSeconds, CancellationToken.None))
                throw new Exception("Couldn't connect");

            return pipeline;
        }

        /// <summary>
        ///     Read telemetry data from the stream for the given dataformat parameter and handle the data by the
        ///     TelemetryDataHandler delegate
        /// </summary>
        /// <param name="parameterIdentifier">The dataformat parameter identifier</param>
        /// <param name="handler">Use this for the streamed data handling, updating etc.</param>
        /// <returns>Pipeline representing the network resource streaming messages to the inputs.</returns>
        public IStreamPipeline ReadTData(string parameterIdentifier, TelemetryDataHandler handler)
        {
            var pipeline = StreamPipelineBuilder.Into(streamId => Read(streamId, parameterIdentifier, handler));

            if (!pipeline.WaitUntilConnected(connectionTimeoutInSeconds, CancellationToken.None))
                throw new Exception("Couldn't connect");

            return pipeline;
        }

        /// <summary>
        ///     Read telemetry samples from the stream for the given list of dataformat parameters and handle the data by the
        ///     TelemetryDataHandler delegate
        /// </summary>
        /// <param name="parameterIdentifiers">List of dataformat parameter identifiers</param>
        /// <param name="handler">Use this for the streamed data handling, updating etc.</param>
        /// <returns>Pipeline representing the network resource streaming messages to the inputs.</returns>
        public IStreamPipeline ReadTSamples(List<string> parameterIdentifiers, TelemetrySamplesHandler handler)
        {
            var pipeline = StreamPipelineBuilder.Into(streamId => Read(streamId, handler));

            if (!pipeline.WaitUntilConnected(connectionTimeoutInSeconds, CancellationToken.None))
                throw new Exception("Couldn't connect");

            return pipeline;
        }

        /// <summary>
        ///     Read telemetry samples from the stream for the given dataformat parameter and handle the data by the
        ///     TelemetryDataHandler delegate
        /// </summary>
        /// <param name="parameterIdentifier">The dataformat parameter identifier</param>
        /// <param name="handler">Use this for the streamed data handling, updating etc.</param>
        /// <returns>Pipeline representing the network resource streaming messages to the inputs.</returns>
        public IStreamPipeline ReadTSamples(string parameterIdentifier, TelemetrySamplesHandler handler)
        {
            var pipeline = StreamPipelineBuilder.Into(streamId => Read(streamId, handler));

            if (!pipeline.WaitUntilConnected(connectionTimeoutInSeconds, CancellationToken.None))
                throw new Exception("Couldn't connect");

            return pipeline;
        }

        /// <summary>
        ///     Read telemetry data from the stream for the given dataformat parameter and handle the data by the
        ///     TelemetryDataHandler delegate
        ///     and link to another output, using the <paramref name="writer" /> and the <paramref name="outputFeedName" />
        /// </summary>
        /// <param name="parameterIdentifier">The dataformat parameter identifier</param>
        /// <param name="handler">Use this for the streamed data handling, updating etc.</param>
        /// <param name="writer">Writer object used for writing the streamed input to another output.</param>
        /// <param name="outputFeedName">
        ///     The output feed name where the streamed input will be linked to, using the
        ///     <paramref name="writer" />
        /// </param>
        /// <returns>Pipeline representing the network resource streaming messages to the inputs.</returns>
        public IStreamPipeline ReadAndLinkTData(string parameterIdentifier, TelemetryDataHandler handler, Writer writer,
            string outputFeedName)
        {
            var pipeline = StreamPipelineBuilder.Into(streamId =>
                ReadAndLinkOutput(streamId, parameterIdentifier, writer, outputFeedName, handler));

            if (!pipeline.WaitUntilConnected(connectionTimeoutInSeconds, CancellationToken.None))
                throw new Exception("Couldn't connect");

            return pipeline;
        }

        /// <summary>
        ///     Read telemetry data from the stream for the given dataformat parameter and handle the data by the
        ///     TelemetryDataHandler delegate
        ///     and link to another output, using the <paramref name="writer" /> and the <paramref name="outputFeedNames" />
        /// </summary>
        /// <param name="parameterIdentifier">The dataformat parameter identifier</param>
        /// <param name="handler">Use this for the streamed data handling, updating etc.</param>
        /// <param name="writer">Writer object used for writing the streamed input to another output.</param>
        /// <param name="outputFeedNames">
        ///     List of output feed names where the streamed input will be linked to, using the
        ///     <paramref name="writer" />
        /// </param>
        /// <returns>Pipeline representing the network resource streaming messages to the inputs.</returns>
        public IStreamPipeline ReadAndLinkTData(string parameterIdentifier, TelemetryDataHandler handler, Writer writer,
            List<string> outputFeedNames)
        {
            var pipeline = StreamPipelineBuilder.Into(streamId =>
                ReadAndLinkOutput(streamId, parameterIdentifier, writer, outputFeedNames, handler));

            if (!pipeline.WaitUntilConnected(connectionTimeoutInSeconds, CancellationToken.None))
                throw new Exception("Couldn't connect");

            return pipeline;
        }

        /// <summary>
        ///     Read telemetry data from the stream for the given dataformat parameter and handle the data by the
        ///     TelemetryDataHandler delegate
        ///     and link to another output, using the <paramref name="writer" /> and the <paramref name="outputFeedName" />
        /// </summary>
        /// <param name="parameterIdentifiers">List of dataformat parameter identifiers</param>
        /// <param name="handler">Use this for the streamed data handling, updating etc.</param>
        /// <param name="writer">Writer object used for writing the streamed input to another output.</param>
        /// <param name="outputFeedName">
        ///     The output feed name where the streamed input will be linked to, using the
        ///     <paramref name="writer" />
        /// </param>
        /// <returns>Pipeline representing the network resource streaming messages to the inputs.</returns>
        public IStreamPipeline ReadAndLinkTData(List<string> parameterIdentifiers, TelemetryDataHandler handler,
            Writer writer, string outputFeedName)
        {
            var pipeline = StreamPipelineBuilder.Into(
                streamId => ReadAndLinkOutput(streamId, parameterIdentifiers, writer, outputFeedName, handler));

            if (!pipeline.WaitUntilConnected(connectionTimeoutInSeconds, CancellationToken.None))
                throw new Exception("Couldn't connect");

            return pipeline;
        }

        /// <summary>
        ///     Read telemetry data from the stream for the given dataformat parameter and handle the data by the
        ///     TelemetryDataHandler delegate
        ///     and link to another output, using the <paramref name="writer" /> and the <paramref name="outputFeedNames" />
        /// </summary>
        /// <param name="parameterIdentifiers">List of dataformat parameter identifiers</param>
        /// <param name="handler">Use this for the streamed data handling, updating etc.</param>
        /// <param name="writer">Writer object used for writing the streamed input to another output.</param>
        /// <param name="outputFeedNames">
        ///     List of output feed names where the streamed input will be linked to, using the
        ///     <paramref name="writer" />
        /// </param>
        /// <returns>Pipeline representing the network resource streaming messages to the inputs.</returns>
        public IStreamPipeline ReadAndLinkTData(List<string> parameterIdentifiers, TelemetryDataHandler handler,
            Writer writer, List<string> outputFeedNames)
        {
            var pipeline = StreamPipelineBuilder.Into(
                streamId => ReadAndLinkOutput(streamId, parameterIdentifiers, writer, outputFeedNames, handler));

            if (!pipeline.WaitUntilConnected(connectionTimeoutInSeconds, CancellationToken.None))
                throw new Exception("Couldn't connect");

            return pipeline;
        }

        /// <summary>
        ///     Read telemetry samples from the stream for the given dataformat parameter and handle the data by the
        ///     TelemetryDataHandler delegate
        ///     and link to another output, using the <paramref name="writer" /> and the <paramref name="outputFeedName" />
        /// </summary>
        /// <param name="parameterIdentifier">The dataformat parameter identifier</param>
        /// <param name="handler">Use this for the streamed data handling, updating etc.</param>
        /// <param name="writer">Writer object used for writing the streamed input to another output.</param>
        /// <param name="outputFeedName">
        ///     The output feed name where the streamed input will be linked to, using the
        ///     <paramref name="writer" />
        /// </param>
        /// <returns>Pipeline representing the network resource streaming messages to the inputs.</returns>
        public IStreamPipeline ReadAndLinkTSamples(string parameterIdentifier, TelemetrySamplesHandler handler,
            Writer writer, string outputFeedName)
        {
            var pipeline = StreamPipelineBuilder.Into(streamId =>
                ReadAndLinkOutput(streamId, parameterIdentifier, writer, outputFeedName, handler));

            if (!pipeline.WaitUntilConnected(connectionTimeoutInSeconds, CancellationToken.None))
                throw new Exception("Couldn't connect");

            return pipeline;
        }

        /// <summary>
        ///     Read telemetry samples from the stream for the given dataformat parameter and handle the data by the
        ///     TelemetryDataHandler delegate
        ///     and link to another output, using the <paramref name="writer" /> and the <paramref name="outputFeedNames" />
        /// </summary>
        /// <param name="parameterIdentifier">The dataformat parameter identifier</param>
        /// <param name="handler">Use this for the streamed data handling, updating etc.</param>
        /// <param name="writer">Writer object used for writing the streamed input to another output.</param>
        /// <param name="outputFeedNames">
        ///     List of output feed names where the streamed input will be linked to, using the
        ///     <paramref name="writer" />
        /// </param>
        /// <returns>Pipeline representing the network resource streaming messages to the inputs.</returns>
        public IStreamPipeline ReadAndLinkTSamples(string parameterIdentifier, TelemetrySamplesHandler handler,
            Writer writer, List<string> outputFeedNames)
        {
            var pipeline = StreamPipelineBuilder.Into(streamId =>
                ReadAndLinkOutput(streamId, parameterIdentifier, writer, outputFeedNames, handler));

            if (!pipeline.WaitUntilConnected(connectionTimeoutInSeconds, CancellationToken.None))
                throw new Exception("Couldn't connect");

            return pipeline;
        }

        /// <summary>
        ///     Read telemetry data from the stream for the given dataformat parameter and handle the data by the
        ///     TelemetryDataHandler delegate
        ///     and link to another output, using the <paramref name="writer" /> and the <paramref name="outputFeedName" />
        /// </summary>
        /// <param name="parameterIdentifiers">List of dataformat parameter identifiers</param>
        /// <param name="handler">Use this for the streamed data handling, updating etc.</param>
        /// <param name="writer">Writer object used for writing the streamed input to another output.</param>
        /// <param name="outputFeedName">
        ///     The output feed name where the streamed input will be linked to, using the
        ///     <paramref name="writer" />
        /// </param>
        /// <returns>Pipeline representing the network resource streaming messages to the inputs.</returns>
        public IStreamPipeline ReadAndLinkTSamples(List<string> parameterIdentifiers, TelemetrySamplesHandler handler,
            Writer writer, string outputFeedName)
        {
            var pipeline = StreamPipelineBuilder.Into(
                streamId => ReadAndLinkOutput(streamId, parameterIdentifiers, writer, outputFeedName, handler));

            if (!pipeline.WaitUntilConnected(connectionTimeoutInSeconds, CancellationToken.None))
                throw new Exception("Couldn't connect");

            return pipeline;
        }

        /// <summary>
        ///     Read telemetry samples from the stream for the given dataformat parameter and handle the data by the
        ///     TelemetryDataHandler delegate
        ///     and link to another output, using the <paramref name="writer" /> and the <paramref name="outputFeedNames" />
        /// </summary>
        /// <param name="parameterIdentifiers">List of dataformat parameter identifiers</param>
        /// <param name="handler">Use this for the streamed data handling, updating etc.</param>
        /// <param name="writer">Writer object used for writing the streamed input to another output.</param>
        /// <param name="outputFeedNames">
        ///     List of output feed names where the streamed input will be linked to, using the
        ///     <paramref name="writer" />
        /// </param>
        /// <returns>Pipeline representing the network resource streaming messages to the inputs.</returns>
        public IStreamPipeline ReadAndLinkTSamples(List<string> parameterIdentifiers, TelemetrySamplesHandler handler,
            Writer writer, List<string> outputFeedNames)
        {
            var pipeline = StreamPipelineBuilder.Into(
                streamId => ReadAndLinkOutput(streamId, parameterIdentifiers, writer, outputFeedNames, handler));

            if (!pipeline.WaitUntilConnected(connectionTimeoutInSeconds, CancellationToken.None))
                throw new Exception("Couldn't connect");

            return pipeline;
        }

        private IStreamInput ReadTData(
            string streamId,
            List<string> parameterIdentifiers,
            TelemetryDataHandler handler)
        {
            var input = new SessionTelemetryDataInput(streamId, DataFormatClient);
            input.DataInput.BindDefaultFeed(parameterIdentifiers).DataBuffered +=
                (sender, e) => //Subscribing to data event
                {
                    handler(e.Buffer.GetData());
                };

            input.StreamFinished += HandleStreamFinished;
            return input;
        }

        private IStreamInput Read(
            string streamId,
            string parameterIdentifier,
            TelemetryDataHandler handler)
        {
            var input = new SessionTelemetryDataInput(streamId, DataFormatClient);
            input.DataInput.BindDefaultFeed(parameterIdentifier).DataBuffered += (sender, e) =>
            {
                handler(e.Buffer.GetData());
            };

            input.StreamFinished += HandleStreamFinished;
            return input;
        }

        private IStreamInput Read(string streamId, TelemetrySamplesHandler handler)
        {
            var input = new SessionTelemetryDataInput(streamId, DataFormatClient);

            input.SamplesInput.AutoBindFeeds((s, e) => // Take the input and bind feed to an event handler
            {
                handler(e.Data);
            });

            input.StreamFinished += (sender, e) => Trace.WriteLine("Finished"); // Handle the steam finished event

            return input;
        }

        private IStreamInput ReadAndLinkOutput(
            string streamId,
            string parameterIdentifier,
            Writer writer,
            string outputFeedName,
            TelemetryDataHandler handler)
        {
            var input = new SessionTelemetryDataInput(streamId, DataFormatClient);
            Debug.WriteLine($"Linking stream {streamId}");
            // automatically propagate session metadata and lifecycle
            input.LinkToOutput(writer.SessionOutput, identifier => identifier + "_" + writer.TopicName);

            // react to data
            input.DataInput.BindDefaultFeed(parameterIdentifier).DataBuffered += (sender, e) =>
            {
                Debug.WriteLine($"Parameter for {streamId} received {parameterIdentifier}");
                var data = e.Buffer.GetData();
                handler(data);
                writer.Write(outputFeedName, data);
            };

            input.StreamFinished += HandleStreamFinished;
            return input;
        }

        private IStreamInput ReadAndLinkOutput(
            string streamId,
            string parameterIdentifier,
            Writer writer,
            List<string> outputFeedNames,
            TelemetryDataHandler handler)
        {
            var input = new SessionTelemetryDataInput(streamId, DataFormatClient);
            Debug.WriteLine($"Linking stream {streamId}");
            // automatically propagate session metadata and lifecycle
            input.LinkToOutput(writer.SessionOutput, identifier => identifier + "_" + writer.TopicName);

            // react to data
            input.DataInput.BindDefaultFeed(parameterIdentifier).DataBuffered += (sender, e) =>
            {
                Debug.WriteLine($"Parameter for {streamId} received {parameterIdentifier}");
                var data = e.Buffer.GetData();
                handler(data);
                foreach (var outputFeedName in outputFeedNames) writer.Write(outputFeedName, data);
            };

            input.StreamFinished += HandleStreamFinished;
            return input;
        }

        private IStreamInput ReadAndLinkOutput(
            string streamId,
            List<string> parameterIdentifiers,
            Writer writer,
            string outputFeedName,
            TelemetryDataHandler handler)
        {
            var input = new SessionTelemetryDataInput(streamId, DataFormatClient);
            Debug.WriteLine($"Linking stream {streamId}");
            // automatically propagate session metadata and lifecycle
            input.LinkToOutput(writer.SessionOutput, identifier => identifier + "_" + writer.TopicName);

            // react to data
            input.DataInput.BindDefaultFeed(parameterIdentifiers).DataBuffered += (sender, e) =>
            {
                var data = e.Buffer.GetData();
                handler(data);
                writer.Write(outputFeedName, data);
            };

            input.StreamFinished += HandleStreamFinished;
            return input;
        }

        private IStreamInput ReadAndLinkOutput(
            string streamId,
            List<string> parameterIdentifiers,
            Writer writer,
            List<string> outputFeedNames,
            TelemetryDataHandler handler)
        {
            var input = new SessionTelemetryDataInput(streamId, DataFormatClient);
            Debug.WriteLine($"Linking stream {streamId}");
            // automatically propagate session metadata and lifecycle
            input.LinkToOutput(writer.SessionOutput, identifier => identifier + "_" + writer.TopicName);

            // react to data
            input.DataInput.BindDefaultFeed(parameterIdentifiers).DataBuffered += (sender, e) =>
            {
                var data = e.Buffer.GetData();
                handler(data);
                foreach (var outputFeedName in outputFeedNames) writer.Write(outputFeedName, data);
            };

            input.StreamFinished += HandleStreamFinished;
            return input;
        }

        private IStreamInput ReadAndLinkOutput(
            string streamId,
            string parameterIdentifier,
            Writer writer,
            string outputFeedName,
            TelemetrySamplesHandler handler)
        {
            var input = new SessionTelemetryDataInput(streamId, DataFormatClient);
            Debug.WriteLine($"Linking stream {streamId}");
            // automatically propagate session metadata and lifecycle
            input.LinkToOutput(writer.SessionOutput, identifier => identifier + "_" + writer.TopicName);

            // react to data
            input.SamplesInput.GetFeed(outputFeedName).DataReceived += (s, e) => { handler(e.Data); };

            input.StreamFinished += HandleStreamFinished;
            return input;
        }

        private IStreamInput ReadAndLinkOutput(
            string streamId,
            string parameterIdentifier,
            Writer writer,
            List<string> outputFeedNames,
            TelemetrySamplesHandler handler)
        {
            var input = new SessionTelemetryDataInput(streamId, DataFormatClient);
            Debug.WriteLine($"Linking stream {streamId}");
            // automatically propagate session metadata and lifecycle
            input.LinkToOutput(writer.SessionOutput, identifier => identifier + "_" + writer.TopicName);

            // react to data
            foreach (var outputFeedName in outputFeedNames)
                input.SamplesInput.GetFeed(outputFeedName).DataReceived += (s, e) => { handler(e.Data); };

            input.StreamFinished += HandleStreamFinished;
            return input;
        }

        private IStreamInput ReadAndLinkOutput(
            string streamId,
            List<string> parameterIdentifiers,
            Writer writer,
            string outputFeedName,
            TelemetrySamplesHandler handler)
        {
            var input = new SessionTelemetryDataInput(streamId, DataFormatClient);
            Debug.WriteLine($"Linking stream {streamId}");
            // automatically propagate session metadata and lifecycle
            input.LinkToOutput(writer.SessionOutput, identifier => identifier + "_" + writer.TopicName);

            // react to data
            input.SamplesInput.GetFeed(outputFeedName).DataReceived += (s, e) => { handler(e.Data); };


            input.StreamFinished += HandleStreamFinished;
            return input;
        }

        private IStreamInput ReadAndLinkOutput(
            string streamId,
            List<string> parameterIdentifiers,
            Writer writer,
            List<string> outputFeedNames,
            TelemetrySamplesHandler handler)
        {
            var input = new SessionTelemetryDataInput(streamId, DataFormatClient);
            Debug.WriteLine($"Linking stream {streamId}");
            // automatically propagate session metadata and lifecycle
            input.LinkToOutput(writer.SessionOutput, identifier => identifier + "_" + writer.TopicName);

            // react to data
            foreach (var outputFeedName in outputFeedNames)
                input.SamplesInput.GetFeed(outputFeedName).DataReceived += (s, e) => { handler(e.Data); };

            input.StreamFinished += HandleStreamFinished;
            return input;
        }

        private void HandleStreamFinished(object sender, StreamEventArgs e)
        {
            //Dispose objects, close sessions if needed.
        }
    }
}