using System.Text;
using Newtonsoft.Json;
using System.IO;

namespace RogyWatchCommon
{
    public interface IConfig
    {
        NamedPipe NamedPipe { get; set; }
        UDP UDP { get; set; }
        WebSocketSTD WebSocketSTD { get; set; }
        WebSocketSTDERR WebSocketSTDERR { get; set; }
        PrimitiveDriverV1 PrimitiveDriverV1{ get; set; }
        PrimitiveDriverV2 PrimitiveDriverV2{ get; set; }
        string[] Start { get; set; }
    }

    [JsonObject()]
    public partial class Config : IConfig
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

        [JsonProperty("start")]
        public string[] Start { get; set; } = new string[] { };
        
        public static readonly string ConfigFileName = "config.json";

        public static Config DeSerialize(string _filename = null)
        {
            var filename = _filename ?? ConfigFileName;
            Config result = null;

            if (File.Exists(filename))
            {
                var json = File.ReadAllText(filename, Encoding.UTF8);
                result = JsonConvert.DeserializeObject<Config>(json,
                    new JsonSerializerSettings { DefaultValueHandling = DefaultValueHandling.Populate });
            }
            else
            {
                using (var stream = new FileStream(filename, FileMode.Create))
                {
                    var jsonBytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(result = new Config()));
                    stream.Write(jsonBytes, 0, jsonBytes.Length);
                }
            }
            return result;
        }

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }

}
