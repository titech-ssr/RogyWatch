using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using RogyWatchCommon.Primitive.DriverInterface;
using RogyWatchCommon;


namespace PrimitivesTest
{
    [TestClass]
    public class PrimitivesTest
    {
        [TestMethod]
        public void PrimitiveDriver_GetDepth_Test()
        {
            using (var v1 = new PrimitiveDriverV1.PrimitiveDriver())
            {
                var depthV1 = v1.GetDepth();
                Assert.AreEqual(v1.RANGE_X * v1.RANGE_Y, depthV1.Length);
            }


            using (var v2 = new PrimitiveDriverV2.PrimitiveDriver())
            {
                var depthV2 = v2.GetDepth();
                Assert.AreEqual(v2.RANGE_X * v2.RANGE_Y, depthV2.Length);
            }
        }

        [TestMethod]
        public void PrimitiveServerModule_GetDepth_Test()
        {
            var depth1 = PrimitiveServerModule.PrimitiveServer.GetDepth<short[]>(KinectVersion.V1);
            var depth2 = PrimitiveServerModule.PrimitiveServer.GetDepth<ushort[]>(KinectVersion.V2);

            Assert.AreEqual(
                Defaults.DepthResolution[KinectVersion.V1].X*Defaults.DepthResolution[KinectVersion.V1].Y
                , depth1.Length);

            Assert.AreEqual(
                Defaults.DepthResolution[KinectVersion.V2].X*Defaults.DepthResolution[KinectVersion.V2].Y
                , depth2.Length);
        }
    }
}
