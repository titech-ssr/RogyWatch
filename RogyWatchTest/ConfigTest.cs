using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using RogyWatchCommon;
using System.IO;

/// <summary>
/// Test for General RogyWatch. Includes RogyWatchCommon too.
/// </summary>
namespace RogyWatchTest
{
    [TestClass]
    public class ConfigTest
    {
        private static string defult = @"{
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
        ""Port"": 8500
    },
    ""start"":[]
}";

        private static string changedJson = @"{
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
        }
    }
}
