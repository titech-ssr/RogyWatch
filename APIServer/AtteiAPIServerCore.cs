using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RogyWatchCommon;
using PrimitiveServerModule;

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
            var depth1 = PrimitiveServer.GetDepth<short[]>(KinectVersion.V1);   Console.WriteLine("     Got Depth1 data");
            var depth2 = PrimitiveServer.GetDepth<ushort[]>(KinectVersion.V2);  Console.WriteLine("     Got Depth2 data");

            var count1 = Attei.Attei.PersonCounter(date.First(), KinectVersion.V1, depth1, Config);
            var count2 = Attei.Attei.PersonCounter(date.First(), KinectVersion.V2, depth2, Config);

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
