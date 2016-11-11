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
            var depth1 = PrimitiveServer.GetDepth<short[]>(KinectVersion.V1);
            var depth2 = PrimitiveServer.GetDepth<ushort[]>(KinectVersion.V2);

            var count1 = Attei.Attei.PersonCounter(date.First(), KinectVersion.V1, depth1);
            var count2 = Attei.Attei.PersonCounter(date.First(), KinectVersion.V2, depth2);

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

        public object Invoke<T>(string line)
        {
            var tokens = (new System.Text.RegularExpressions.Regex(@"\s+")).Split(line);
            var arg = tokens.Where((el, i) => i > 0);
            return GetType().GetMethod(tokens[0], new[] { typeof(T) }).Invoke(this, new[] { arg.ToArray() });
        }
    }
}
