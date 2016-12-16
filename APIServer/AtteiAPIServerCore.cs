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
            var depth1 = PrimitiveServer.GetDepthAsync<short[]>(KinectVersion.V1, 10000);
            var depth2 = PrimitiveServer.GetDepthAsync<ushort[]>(KinectVersion.V2, 10000);
            string count1, count2;
            try
            {
                count1 = Attei.Attei.PersonCounter(date.First(), KinectVersion.V1, depth1.Result, Config).ToString();
                Log.logger.Info("     Got Depth1 data");
            }catch(Exception ex)
            {
                Log.logger.Error($"{ex.Message}\n{ex.StackTrace}");
                count1 = "?";
            }

            try
            {
                count2 = Attei.Attei.PersonCounter(date.First(), KinectVersion.V2, depth2.Result, Config).ToString();
                Log.logger.Info("     Got Depth2 data");
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
