using System;
using System.Collections.Generic;
using System.Threading;
using MAT.OCS.Streaming.Samples.Models;
using MAT.OCS.Streaming.Samples.Samples;
using MAT.OCS.Streaming.Samples.Samples.Basic;

using NLog;

namespace MAT.OCS.Streaming.Samples
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            // Samples show how to read and write Telemetry Data and Telemetry Samples
            // For basic usage please look at the samples in the Samples/Basic folder

            /*// Read Telemetry Data
            Console.WriteLine("Reading TData");
            var rtData = new TData();
            rtData.ReadTData();
            Console.WriteLine("Reading TData finished");
            */

            // Write Telemetry Data
            Console.WriteLine("Writing TData");
            var tData = new TData();
            tData.WriteTData();
            Console.WriteLine("Writing TData finished");
            
            /*
            // Read Telemetry Samples
            Console.WriteLine("Reading TSamples");
            var rtSamples = new TSamples();
            rtSamples.ReadTSamples();
            Console.WriteLine("Reading TSamples finished");
            */

            /*// Write Telemetry Samples
            Console.WriteLine("Writing TSamples");
            var wtSamples = new TSamples();
            wtSamples.WriteTSamples();
            Console.WriteLine("Writing TSamples finished");
            */

            /*// Run a model
            Console.WriteLine("Running model");
            var model = new ModelSample();
            model.Run();
            Console.WriteLine("Running model finished");
            */

            // For advanced usage with structured code please look at the samples in the Samples folder

            /* Read/Write/Read and link TDataSingleFeedSingleParameter

            TDataSingleFeedSingleParameter.Read();
            TDataSingleFeedSingleParameter.Write();
            TDataSingleFeedSingleParameter.ReadAndLink();

             */
        }
    }
}
 