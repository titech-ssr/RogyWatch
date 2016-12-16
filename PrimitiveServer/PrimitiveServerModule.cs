using RogyWatchCommon;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace PrimitiveServerModule
{
    /// <summary>
    /// Provide Unified Interface to access Kinect V1 and V2
    /// </summary>
    public partial class PrimitiveServer
    {
        private static object _lock1 = new object();
        private static object _lock2 = new object();

        public PrimitiveServer()
        {
        }

        /// <summary>
        /// Get depth from kinect with specified version and Cast depth data with type parameter. <para/>
        /// Without reason, Kinect V2 should output ushort[] and V1 should short[]
        /// </summary>
        /// <typeparam name="T"><code>T must be ushort[] or short[]</code></typeparam>
        /// <returns></returns>
        public static T GetDepth<T>(KinectVersion V)
        {
            T depth;

            if (V == KinectVersion.V2)
            {
                lock (_lock2)
                {
                    using (var V2 = new PrimitiveDriverV2.PrimitiveDriver())
                    {
                        depth = (T)(object)V2.GetDepth();
                    }
                }
                return depth;
            }
            else
            {
                lock (_lock1)
                {
                    using (var V1 = new PrimitiveDriverV1.PrimitiveDriver())
                    {
                        depth = (T)(object)V1.GetDepth();
                    }
                }
                return depth;
            }
        } // End of GetDepth

        /// <summary>
        /// Get depth Asynchronously from kinect with specified version and Cast depth data with type parameter. </para>
        /// Without reason, Kinect V2 should output ushort[] and V1 should short[] </para>
        /// Possibly, TimeoutException or OpenFailed Exception threw.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="V">Kinect Version</param>
        /// <param name="timeout">milli seconds</param>
        /// <returns></returns>
        public static async Task<T> GetDepthAsync<T>(KinectVersion V, int timeout)
        {
            var cts = new CancellationTokenSource();
            var task = Task.Run(() => GetDepth<T>(V), cts.Token);

            try
            {
                if (await Task.WhenAny(task, Task.Delay(timeout)) == task)
                {
                    return task.Result;
                }
                else
                {
                    cts.Cancel();
                    throw new TimeoutException($"GetDepthAsync Kinect{V} timeout!");
                }
            }catch(AggregateException ex)
            {
                throw ex.InnerException;
            }
        }
    }
}
