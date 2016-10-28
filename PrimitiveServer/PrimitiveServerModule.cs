using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RogyWatchCommon;

namespace PrimitiveServerModule
{
    public class PrimitiveServer
    {
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
            if (V == KinectVersion.V2)
            {
                T depth;
                using (var V2 = new PrimitiveDriverV2.PrimitiveDriver())
                {
                    depth = (T)(object)V2.GetDepth();
                }
                return depth;
            } else
            {
                T depth;
                using (var V1 = new PrimitiveDriverV1.PrimitiveDriver())
                {
                    depth = (T)(object)V1.GetDepth();
                }
                return depth;
            }
        }
    }
}
