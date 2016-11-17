using RogyWatchCommon;

namespace PrimitiveServerModule
{
    public class PrimitiveServer
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
    }
}
