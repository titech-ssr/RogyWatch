using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RogyWatchCommon;
using PrimitiveServerModule;

namespace APIServerModule
{
    /// <summary>
    /// for Pipe or UDP Server
    /// </summary>
    public interface IAPIServerCore
    {
        T Get<T>(string command);
        object GetDepth(KinectVersion v);
        int HowManyPeople(string date);

        object Invoke<T>(string line);
        Config Config { get; set; }
    }

    public partial class APIServerCore : IAPIServerCore
    {
        public T Get<T>(string command)
        {
            return (T)(object)0;
        }

        public object GetDepth(KinectVersion v)
        {
            if (v == KinectVersion.V1)
                return PrimitiveServer.GetDepth<short[]>(v);
            else
                return PrimitiveServer.GetDepth<ushort[]>(v);
        }

        public int HowManyPeople(string date)
        {
            throw new NotImplementedException();
        }

        public object Invoke<T>(string line)
        {
            var tokens = (new System.Text.RegularExpressions.Regex(@"\s+")).Split(line);
            var arg = tokens.Where((el, i) => i > 0);
            return GetType().GetMethod(tokens[0], new[] { typeof(T) }).Invoke(this, new[] { arg.ToArray() });
        }

        public Config Config { get; set; }
    }

}
