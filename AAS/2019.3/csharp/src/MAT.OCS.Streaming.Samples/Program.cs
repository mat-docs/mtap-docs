using System;
using MAT.OCS.Streaming.Samples.Models;
using MAT.OCS.Streaming.Samples.Samples;
using MAT.OCS.Streaming.Samples.Samples.Basic;

namespace MAT.OCS.Streaming.Samples
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            // Samples show how to read and write Telemetry Data and Telemetry Samples
            // For basic usage please look at the samples in the Samples/Basic folder

            // Uncomment the method which you wish to run

            // ReadTelemetryData();

            // WriteTelemetryData();

            // ReadTelemetrySamples();

            // WriteTelemetrySamples();

            ReadEvents();

            // WriteEvents();

            // RunAModel();

            // For advanced usage with structured code please look at the samples in the Samples folder

            // AdvancedExample();
        }

        /// <summary>
        /// Read telemetry data to kafka
        /// </summary>
        private static void ReadTelemetryData()
        {
            Console.WriteLine("Reading TData");
            var rtData = new TData();
            rtData.ReadTData();
            Console.WriteLine("Reading TData finished");
        }

        /// <summary>
        /// Write telemetry data to kafka
        /// </summary>
        private static void WriteTelemetryData()
        {
            Console.WriteLine("Writing TData");
            var tData = new TData();
            tData.WriteTData();
            Console.WriteLine("Writing TData finished");
        }

        /// <summary>
        /// Read telemetry samples from kafka
        /// </summary>
        private static void ReadTelemetrySamples()
        {
            Console.WriteLine("Reading TSamples");
            var rtSamples = new TSamples();
            rtSamples.ReadTSamples();
            Console.WriteLine("Reading TSamples finished");
        }

        /// <summary>
        /// Write telemetry samples to kafka
        /// </summary>
        private static void WriteTelemetrySamples()
        {
            Console.WriteLine("Writing TSamples");
            var wtSamples = new TSamples();
            wtSamples.WriteTSamples();
            Console.WriteLine("Writing TSamples finished");
        }

        /// <summary>
        /// Read events from kafka
        /// </summary>
        private static void ReadEvents()
        {
            Console.WriteLine("Reading Events");
            var eventsRead = new EventsRead();
            eventsRead.ReadEvents();
            Console.WriteLine("Reading Events finished");
        }

        /// <summary>
        /// Write events to kafka
        /// </summary>
        private static void WriteEvents()
        {
            Console.WriteLine("Writing Events");
            var eventsWrite = new EventsWrite();
            eventsWrite.WriteEvents();
            Console.WriteLine("Writing Events finished");
        }

        /// <summary>
        /// Run a read/write example with simple model executing
        /// code on incoming data to generate outgoing data
        /// </summary>
        private static void RunAModel()
        {
            Console.WriteLine("Running model");
            var model = new ModelSample();
            model.Run();
            Console.WriteLine("Running model finished");
        }

        /// <summary>
        /// Read/Write/Read and link TDataSingleFeedSingleParameter
        /// </summary>
        private static void AdvancedExample()
        {
            TDataSingleFeedSingleParameter.Read();
            TDataSingleFeedSingleParameter.Write();
            TDataSingleFeedSingleParameter.ReadAndLink();
        }
    }
}
 