using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using RogyWatchCommon;

/// <summary>
/// Test for General RogyWatch. Includes RogyWatchCommon too.
/// </summary>
namespace RogyWatchTest
{
    [TestClass]
    public class ConfigTest
    {
        [TestMethod]
        public void SerializeDefaultTest()
        {
            var expected = @"{
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
    }
}";
            var _default = new RogyWatchCommon.Config.Config();
            var json = JsonConvert.SerializeObject(_default);

            var reg = new System.Text.RegularExpressions.Regex(@"\s+");
            Assert.AreEqual(reg.Replace(expected, ""), reg.Replace(json, ""));

        }

        [TestMethod]
        public void DeSerializeTest()
        {
            var json = @"{
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
    }
}";
            var config = JsonConvert.DeserializeObject<RogyWatchCommon.Config.Config>(json, 
                new JsonSerializerSettings { DefaultValueHandling = DefaultValueHandling.Populate });

            Assert.AreEqual(0, config.NamedPipe.Port);
            Assert.AreEqual(4000, config.UDP.Port);
            Assert.AreEqual(8400, config.WebSocketSTDERR.Port);

        }
    }
}
