using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;

namespace RogyWatchCommon
{
    public interface IServer{
        string Host { get; set; }
        ushort Port { get; set; }
    }

    public class NamedPipe : IServer
    {
        [DefaultValue("Kinect")]
        public string Host { get; set; } = "Kinect";
        [DefaultValue(0)]
        public ushort Port { get; set; } = 0;
    }
    public class UDP : IServer
    {
        [DefaultValue("127.0.0.1")]
        public string Host { get; set; } = "127.0.0.1";
        [DefaultValue(4000)]
        public ushort Port { get; set; } = 4000;
    }
    public class WebSocketSTD : IServer
    {
        [DefaultValue("*")]
        public string Host { get; set; } = "*";
        [DefaultValue(8000)]
        public ushort Port { get; set; } = 8000;
    }
    public class WebSocketSTDERR : IServer
    {
        [DefaultValue("*")]
        public string Host { get; set; } = "*";
        [DefaultValue(8500)]
        public ushort Port { get; set; } = 8500;
        [DefaultValue(5)]
        public ushort ConnectionTimeout { get; set; } = 5;
    }

    public interface IPrimitiveDriver{
        int Limit { get; set; }

        ushort DEPTH_X { get; set; }
        ushort DEPTH_Y { get; set; }
    }

    public class PrimitiveDriverV1 : IPrimitiveDriver
    {
        [DefaultValue(10000)]
        public int Limit { get; set; } = 10000;

        [DefaultValue(640)]
        public ushort DEPTH_X { get; set; } = 640;
        [DefaultValue(480)]
        public ushort DEPTH_Y { get; set; } = 480;
    }
    public class PrimitiveDriverV2 : IPrimitiveDriver
    {
        [DefaultValue(10000)]
        public int Limit { get; set; } = 10000;

        [DefaultValue(512)]
        public ushort DEPTH_X { get; set; } = 512;
        [DefaultValue(424)]
        public ushort DEPTH_Y { get; set; } = 424;
    }
}
