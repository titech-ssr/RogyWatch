using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RogyWatchCommon
{

    [Flags]
    public enum KinectVersion : byte
    {
        V1 = 0x80, // 1000 0000
        V2 = 0x00  // 0000 0000
    }

    public struct Point<T>
    {
        public T X;
        public T Y;
        public T Z;
    }

    /// <summary>
    /// Holds unique Server class &lt; - &gt; instance map. So, any server class must have only one instance.
    /// </summary>
    public static class Servers
    {
        private static Dictionary<Type, dynamic> _servers = new Dictionary<Type, dynamic>();
        public static T Get<T>()
        {
            return (T)_servers[typeof(T)];
        }

        public static void Set<T>(T s)
        {
            _servers.Add(typeof(T), s);
        }
    }

    [Serializable()]
    public class KinectNotFoundException : System.IO.IOException
    {
        public KinectNotFoundException() : base() { }
        public KinectNotFoundException(string message) : base(message) { }
        public KinectNotFoundException(string message, Exception inner) : base(message, inner) { }
        public KinectNotFoundException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context) { }
    }

}
