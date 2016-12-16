using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using RogyWatchCommon;

namespace PrimitiveServerModule
{
    public partial class PrimitiveServer
    {

        /// <summary>
        /// Check kinect connection. t1,t2 should be milliseconds time.
        /// </summary>
        /// <param name="t1"></param>
        /// <param name="t2"></param>
        /// <returns></returns>
        public static Tuple<bool, bool> CheckConnection(int t1, int t2)
        {
            var futureDepth1 = GetDepthAsync<short[]>(KinectVersion.V1, t1);
            var futureDepth2 = GetDepthAsync<short[]>(KinectVersion.V2, t2);
            var kinect1 = true;
            var kinect2 = true;
            try
            {
                futureDepth1.Wait();
            }catch(AggregateException ex)
            {
                Log.logger.Warn($"kinect1 threw Exception: {ex.InnerException.Message}\n{ex.InnerException.StackTrace}");
                kinect1 = false;
            }

            try
            {
                futureDepth2.Wait();
            }catch(AggregateException ex)
            {
                Log.logger.Warn($"kinect2 threw Exception: {ex.InnerException.Message}\n{ex.InnerException.StackTrace}");
                kinect2 = false;
            }

            return new Tuple<bool, bool>(kinect1, kinect2);
        }
    }
}
