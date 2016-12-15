using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RogyWatchCommon;
using PrimitiveServerModule;
using System.Threading;

namespace APIServerModule
{
    public partial class APIServerCore : IAPIServerCore
    {
        public string kill(IEnumerable<string> line)
        {
            throw new NotImplementedException();
        }

        public string Eval(IEnumerable<string> code)
        {
            throw new NotImplementedException();
        }

        public string HowManyPeople(IEnumerable<string> date)
        {
            short[] depth1;
            string count1;
            try
            {
                var cts = new CancellationTokenSource();
                var task = Task.Run(() => PrimitiveServer.GetDepth<short[]>(KinectVersion.V1), cts.Token);
                if (task.Wait(TimeSpan.FromSeconds(10.0)))
                {
                    depth1 = task.Result;
                    Log.logger.Info("     Got Depth1 data");
                    count1 = Attei.Attei.PersonCounter(date.First(), KinectVersion.V1, depth1, Config).ToString();
                }
                else
                {
                    count1 = "?";
                    cts.Cancel();
                }
            }catch(Exception ex)
            {
                Log.logger.Error($"{ex.Message}\n{ex.StackTrace}");
                count1 = "?";
            }

            ushort[] depth2;
            string count2;
            try
            {
                var cts = new CancellationTokenSource();
                var task = Task.Run(() => PrimitiveServer.GetDepth<ushort[]>(KinectVersion.V2), cts.Token);
                if (task.Wait(TimeSpan.FromSeconds(10.0)))
                {
                    depth2 = task.Result;
                    Log.logger.Info("     Got Depth2 data");
                    count2 = Attei.Attei.PersonCounter(date.First(), KinectVersion.V2, depth2, Config).ToString();
                }else
                {
                    count2 = "?";
                    cts.Cancel();
                }
            }catch(Exception ex)
            {
                Log.logger.Error($"{ex.Message}\n{ex.StackTrace}");
                count2 = "?";
            }

            return $"{count1} {count2}";
        }

        public string Echo(IEnumerable<string> line)
        {
            return $"Hello {string.Join("", line)}";
        }
        public string Echo(string line)
        {
            return $"Hello {line}";
        }
    }
}
