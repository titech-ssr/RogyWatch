using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RogyWatchCommon;
using PrimitiveServerModule;

namespace APIServerModule
{
    public interface IAPIServerCore
    {
        T Get<T>(string command);
        object GetDepth(KinectVersion v);
    }

    public class APIServerCore : IAPIServerCore
    {
        public T Get<T>(string command)
        {
            return (T)(object)0;
        }

        public object GetDepth(KinectVersion v)
        {
            if (v == KinectVersion.V1)
                return (short[])(object)PrimitiveServer.GetDepth<short[]>(v);
            else
                return (ushort[])(object)PrimitiveServer.GetDepth<ushort[]>(v);
        }
    }
}
