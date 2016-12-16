using Microsoft.VisualStudio.TestTools.UnitTesting;
using RogyWatchCommon;
using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace PrimitivesTest
{
    [TestClass]
    public class PrimitivesTest
    {
        /// <summary>
        /// Get Depth Test. This Test also save depth data to disk.
        /// </summary>
        [TestMethod]
        public void PrimitiveDriver_GetDepth_Test()
        {
            using (var v1 = new PrimitiveDriverV1.PrimitiveDriver())
            {
                var depthV1 = v1.GetDepth();
                Assert.AreEqual(v1.RANGE_X * v1.RANGE_Y, depthV1.Length);

                // Serialize
                using (var fs = new FileStream(@"DepthData/V1.obj", FileMode.Create, FileAccess.Write))
                {
                    BinaryFormatter bf = new BinaryFormatter();
                    bf.Serialize(fs, depthV1);
                }
            }


            using (var v2 = new PrimitiveDriverV2.PrimitiveDriver())
            {
                var depthV2 = v2.GetDepth();
                Assert.AreEqual(v2.RANGE_X * v2.RANGE_Y, depthV2.Length);

                // Serialize
                using (var fs = new FileStream(@"DepthData/V2.obj", FileMode.Create, FileAccess.Write))
                {
                    BinaryFormatter bf = new BinaryFormatter();
                    bf.Serialize(fs, depthV2);
                }
            }
        }

        [TestMethod]
        public void DepthData_deserialize_Test()
        {
            ushort[] d2;
            short[] d1;

            using (var fs = new FileStream(@"DepthData/V1.obj", FileMode.Open, FileAccess.Read))
            {
                var f = new BinaryFormatter();
                d1 = (short[])f.Deserialize(fs);
            }

            using (var fs = new FileStream(@"DepthData/V2.obj", FileMode.Open, FileAccess.Read))
            {
                var f = new BinaryFormatter();
                d2 = (ushort[])f.Deserialize(fs);
            }
        }

        [TestMethod]
        public void PrimitiveServerModule_GetDepth_Test()
        {
            var depth1 = PrimitiveServerModule.PrimitiveServer.GetDepth<short[]>(KinectVersion.V1);
            var depth2 = PrimitiveServerModule.PrimitiveServer.GetDepth<ushort[]>(KinectVersion.V2);
            var defult = new Config();

            Assert.AreEqual(
                defult.PrimitiveDriverV1.DEPTH_X*defult.PrimitiveDriverV1.DEPTH_Y
                , depth1.Length);

            Assert.AreEqual(
                defult.PrimitiveDriverV2.DEPTH_X*defult.PrimitiveDriverV2.DEPTH_Y
                , depth2.Length);
        }

        /// <summary>
        /// Check connection to Kinect V1 & V2.
        /// Result dependents on hardware.
        /// </summary>
        [TestMethod]
        public void CheckConnection_Test()
        {
            Log.logger = NLog.LogManager.GetCurrentClassLogger();
            Log.logger.Info("Connection Test Started");
            var result = PrimitiveServerModule.PrimitiveServer.CheckConnection(10000, 10000);
            Assert.IsTrue(result.Item1);
            Assert.IsTrue(result.Item2);
        }

        [TestMethod]
        public void GetDepthAsync_Test()
        {
            try
            {
                var futureDepth = PrimitiveServerModule.PrimitiveServer.GetDepthAsync<short[]>(KinectVersion.V1, 5000);
                Console.WriteLine($"depth size is {futureDepth.Result.Length}");
            }catch(AggregateException ex)
            {
               throw ex.InnerException;
            }
        }
    }
}
