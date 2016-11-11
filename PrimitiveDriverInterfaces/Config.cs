using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.ComponentModel;

namespace RogyWatchCommon.Config
{
    public interface IConfig
    {
        NamedPipe NamedPipe { get; set; }
        UDP UDP { get; set; }
        WebSocketSTD WebSocketSTD { get; set; }
        WebSocketSTDERR WebSocketSTDERR { get; set; }
        PrimitiveDriverV1 PrimitiveDriverV1{ get; set; }
        PrimitiveDriverV2 PrimitiveDriverV2{ get; set; }
    }

    [JsonObject()]
    public class Config : IConfig
    {
        [JsonProperty("primitive_driver_v1")]
        public PrimitiveDriverV1 PrimitiveDriverV1 { get; set; } = new PrimitiveDriverV1();

        [JsonProperty("primitive_driver_v2")]
        public PrimitiveDriverV2 PrimitiveDriverV2 { get; set; } = new PrimitiveDriverV2();

        [JsonProperty("named_pipe")]
        public NamedPipe NamedPipe { get; set; } = new NamedPipe();

        [JsonProperty("udp")]
        public UDP UDP { get; set; } = new UDP();

        [JsonProperty("websocket_std")]
        public WebSocketSTD WebSocketSTD { get; set; } = new WebSocketSTD();

        [JsonProperty("websocket_std_err")]
        public WebSocketSTDERR WebSocketSTDERR { get; set; } = new WebSocketSTDERR();
        
        public static readonly string ConfigFileName = "config.json";
    }

}
