using MAT.OCS.Streaming.IO.TelemetryData;
using MAT.OCS.Streaming.IO.TelemetrySamples;
using MAT.OCS.Streaming.Model;
using System;
using System.Diagnostics;
using System.Linq;

namespace MAT.OCS.Streaming.Samples.Samples
{
    public static class Models
    {
        public delegate void TelemetryDataHandler(TelemetryData data);
        public delegate void TelemetrySamplesHandler(TelemetrySamples samples);
        public static void TraceData(TelemetryData data)
        {
            var time = data.TimestampsNanos;
            for (int i = 0; i < data.Parameters.Length; i++)
            {
                Trace.WriteLine($"Parameter[{i}]:");
                var vCar = data.Parameters[i].AvgValues;
                for (var j = 0; j < time.Length; j++)
                {
                    var fromMilliseconds = TimeSpan.FromMilliseconds(time[j].NanosToMillis());
                    Trace.WriteLine($"{fromMilliseconds:hh\\:mm\\:ss\\.fff}, { new string('.', (int)(50 * vCar[j])) }");
                }
            }
        }
        public static void TraceSamples(TelemetrySamples data)
        {
            Trace.WriteLine(data.Parameters.First().Key);
            Trace.WriteLine(data.Parameters.Count);
        }

        public static void DoubleAvgModel(TelemetryData data)
        {
            var p1 = data.Parameters[0];
            // Data can generally be safely modified inline
            for (var i = 0; i < p1.AvgValues.Length; i++)
                p1.AvgValues[i] = p1.AvgValues[i] * 2;
        }

        public static void SinCosModel(TelemetryData data, long elapsedNanos)
        {
            var sinParam = data.Parameters[0];
            var cosParam = data.Parameters[1];
            sinParam.Statuses[0] = DataStatus.Sample;
            sinParam.AvgValues[0] = Math.Sin(1 / 50.0);

            cosParam.Statuses[0] = DataStatus.Sample;
            cosParam.AvgValues[0] = Math.Cos(1 / 100.0);
        }
        
        public static void AbsAvgModel(TelemetryData data)
        {
            for (var i = 0; i < data.Parameters[0].AvgValues.Length; i++)
                data.Parameters[0].AvgValues[i] = Math.Abs(data.Parameters[0].AvgValues[i]);
        }
    }
}
