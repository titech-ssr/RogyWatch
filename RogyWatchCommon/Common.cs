using System;

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

    [Serializable()]
    public class KinectNotFoundException : System.IO.IOException
    {
        public KinectNotFoundException() : base() { }
        public KinectNotFoundException(string message) : base(message) { }
        public KinectNotFoundException(string message, Exception inner) : base(message, inner) { }
        public KinectNotFoundException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context) { }
    }

}
