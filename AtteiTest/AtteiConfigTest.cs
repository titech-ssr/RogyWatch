using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RogyWatchCommon;
using Newtonsoft.Json;
using System.IO;

namespace AtteiTest
{
    [TestClass]
    public class ConfigTest
    {
        private static string defult = @"{
    ""attei_config"": {
        ""MakeAscDir1"": ""C:\\Users\\Maquinista\\Desktop\\hisui-1_13_0_0-20110927\\bin\\V1"",
        ""MakeAscDir2"": ""C:\\Users\\Maquinista\\Desktop\\hisui-1_13_0_0-20110927\\bin\\V2"",
        ""Template1"": ""C:\\template_V1.bmp"",
        ""Template2"": ""C:\\template_V2.bmp"",
        ""OutDir1"": ""C:\\Users\\Public\\share\\V1"",
        ""OutDir2"": ""C:\\Users\\Public\\share\\V2""
    },
    ""primitive_driver_v1"": {
        ""Limit"": 10000,
        ""DEPTH_X"": 640,
        ""DEPTH_Y"": 480
    },
    ""primitive_driver_v2"": {
        ""Limit"": 10000,
        ""DEPTH_X"": 512,
        ""DEPTH_Y"": 424
    },
    ""named_pipe"": {
        ""Host"": ""Kinect"",
        ""Port"": 0
    },
    ""udp"": {
        ""Host"": ""127.0.0.1"",
        ""Port"": 4000
    },
    ""websocket_std"": {
        ""Host"": ""*"",
        ""Port"": 8000
    },
    ""websocket_std_err"": {
        ""Host"": ""*"",
        ""Port"": 8500,
        ""ConnectionTimeout"": 5
    },
    ""start"": [ ]
}";

        private static string changedJson = @"{
    ""attei_config"": {
        ""MakeAscDir1"": ""C:\\Users\\Maquinista\\Desktop\\hisui-1_13_0_0-20110927\\bin\\V1"",
        ""MakeAscDir2"": ""C:\\Users\\Maquinista\\Desktop\\hisui-1_13_0_0-20110927\\bin\\V2"",
        ""Template1"": ""C:\\template_V1.bmp"",
        ""Template2"": ""C:\\template_V2.bmp"",
        ""OutDir1"": ""C:\\Users\\Public\\share\\V1"",
        /*""OutDir2"": ""C:\\Users\\Public\\share\\V2""*/
    },
    ""primitive_driver_v1"": {
        ""Limit"": 10000,
        ""DEPTH_X"": 640,
        ""DEPTH_Y"": 480
    },
    ""primitive_driver_v2"": {
        ""Limit"": 10000,
        ""DEPTH_X"": 512,
        ""DEPTH_Y"": 424
    },
    ""named_pipe"": {
        ""Host"": ""Kinect"",
        ""Part"": 0
    },
    ""udp"": {
        ""Host"": ""127.0.0.1"",
        /*""Port"": 4000*/
    },
    ""websocket_std"": {
        ""Host"": ""*"",
        ""Port"": 8000
    },
    ""websocket_std_err"": {
        ""Host"": ""*"",
        ""Port"": 8400
    },
    ""start"":[""WebSocket""]
}";
        [TestMethod]
        public void SerializeDefaultTest()
        {
            var expected = defult;
            var _default = new Config();
            var json = JsonConvert.SerializeObject(_default);

            var reg = new System.Text.RegularExpressions.Regex(@"\s+");
            Assert.AreEqual(reg.Replace(expected, ""), reg.Replace(json, ""));

        }

        [TestMethod]
        public void DeSerializeTest()
        {
            var json = changedJson;
            var config = JsonConvert.DeserializeObject<Config>(json,
                new JsonSerializerSettings { DefaultValueHandling = DefaultValueHandling.Populate });

            Assert.AreEqual(0, config.NamedPipe.Port);
            Assert.AreEqual(4000, config.UDP.Port);
            Assert.AreEqual(8400, config.WebSocketSTDERR.Port);
            Assert.AreEqual("WebSocket", config.Start[0]);
            Assert.AreEqual("C:\\Users\\Public\\share\\V2", config.AtteiConfig.OutDir2);

        }

        [TestMethod]
        public void DeSerializeFromNonExistFile_Test()
        {
            if (File.Exists(Config.ConfigFileName)) File.Delete(Config.ConfigFileName);
            var config = Config.DeSerialize();
            var expected = defult;

            var json = JsonConvert.SerializeObject(config);
            var reg = new System.Text.RegularExpressions.Regex(@"\s+");
            Assert.AreEqual(reg.Replace(expected, ""), reg.Replace(json, ""));
        }

        [TestMethod]
        public void DeSerializeFromExistFile_Test()
        {
            var config = Config.DeSerialize("test.json");

            Assert.AreEqual(0, config.NamedPipe.Port);
            Assert.AreEqual(4000, config.UDP.Port);
            Assert.AreEqual(8400, config.WebSocketSTDERR.Port);
            Assert.AreEqual("WebSocket", config.Start[0]);
            Assert.AreEqual("C:\\Users\\Public\\share\\V2", config.AtteiConfig.OutDir2);
        }
    }
}
