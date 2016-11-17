using System;
using System.Linq;
using RogyWatchCommon;
using PrimitiveServerModule;

namespace APIServerModule
{
    /// <summary>
    /// UDP, WebSocket, NamedPipe
    /// </summary>
    public interface IAPIServerCore
    {
        T Get<T>(string command);
        object GetDepth(KinectVersion v);
        int HowManyPeople(string date);

        object Invoke<T>(string line);
        Config Config { get; set; }
    }

    /// <summary>
    /// API Server Core for production
    /// </summary>
    public partial class APIServerCore : IAPIServerCore
    {
        /// <summary>
        /// Dummy API
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="command"></param>
        /// <returns></returns>
        public T Get<T>(string command)
        {
            return (T)(object)0;
        }

        /// <summary>
        /// return depth of short[] for V1 or ushort[] for V2
        /// </summary>
        /// <param name="v">Kinect version</param>
        /// <returns></returns>
        public object GetDepth(KinectVersion v)
        {
            if (v == KinectVersion.V1)
                return PrimitiveServer.GetDepth<short[]>(v);
            else
                return PrimitiveServer.GetDepth<ushort[]>(v);
        }

        /// <summary>
        /// Count people. Not implemented yet.
        /// </summary>
        /// <param name="date"></param>
        /// <returns></returns>
        public int HowManyPeople(string date)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Invoke instance method dynamically. Mainly, for websocket.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="line"></param>
        /// <returns></returns>
        public object Invoke<T>(string line)
        {
            var tokens = (new System.Text.RegularExpressions.Regex(@"\s+")).Split(line);
            var arg = tokens.Where((el, i) => i > 0);
            return GetType().GetMethod(tokens[0], new[] { typeof(T) }).Invoke(this, new[] { arg.ToArray() });
        }

        /// <summary>
        /// config object for rogywatch instance.
        /// </summary>
        public Config Config { get; set; }
    }

}
