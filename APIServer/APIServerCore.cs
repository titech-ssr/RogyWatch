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
        public static string kill(IEnumerable<string> line)
        {
            throw new NotImplementedException();
        }

        public static string Eval(IEnumerable<string> code)
        {
            throw new NotImplementedException();
        }

        public static int HowManyPeople(IEnumerable<string> date)
        {
            throw new NotImplementedException();
        }

        public static string Echo(IEnumerable<string> line)
        {
            return $"Hello {string.Join("", line)}";
        }

        public static object Invoke(string line)
        {
            var tokens = (new System.Text.RegularExpressions.Regex(@"\s+")).Split(line);
            var arg = tokens.Where((el, i) => i > 0);
            try
            {
                return typeof(ControlServerCore).GetMethod(tokens[0]).Invoke(null, new[] { arg.ToArray() });
            }catch(Exception ex)
            {
                return $"{ex.Message}\n{ex.StackTrace}";
            }
        }
    }
}
