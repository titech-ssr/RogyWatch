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
    }

    public static class ControlServerCore
    {
        public static string kill(string line)
        {
            throw new NotImplementedException();
        }

        public static string Eval(string code)
        {
            throw new NotImplementedException();
        }

        public static int HowManyPeople(string date)
        {
            throw new NotImplementedException();
        }
    }
}
