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
        private static Func<bool> generateWait<T>(T task, CancellationTokenSource cts, double timeout) where T : Task
        {
            return () =>
            {
                if (task.Wait(TimeSpan.FromSeconds(timeout)))
                    return true;
                else
                {
                    cts.Cancel();
                    return false;
                }
            };
        }

        public static Tuple<bool, bool> CheckConnection(double t1, double t2)
        {
            var cts1 = new CancellationTokenSource();
            var cts2 = new CancellationTokenSource();
            var task1 = Task.Run(() => GetDepth<short[]>(KinectVersion.V1), cts1.Token);
            var task2 = Task.Run(() => GetDepth<ushort[]>(KinectVersion.V2), cts2.Token);
            //var task1 = Task.Run(() => { throw new Exception(); }, cts1.Token);
            //var task2 = Task.Run(() => { throw new Exception(); }, cts2.Token);

            var ret1 = Task.Run(generateWait(task1, cts1, t1));
            var ret2 = Task.Run(generateWait(task2, cts2, t2));

            var kinect1 = false;
            try
            {
                kinect1 = ret1.Result;
            }catch(Exception ex)
            {
                Log.logger.Warn($"kinect1 threw Exception {ex.Message}\n{ex.StackTrace}");
                kinect1 = false;
            }

            var kinect2 = false;
            try
            {
                kinect2 = ret2.Result;
            }catch(Exception ex)
            {
                Log.logger.Warn($"kinect2 threw Exception {ex.Message}\n{ex.StackTrace}");
                kinect2 = false;
            }

            return new Tuple<bool, bool>(kinect1, kinect2);
        }
    }
}
