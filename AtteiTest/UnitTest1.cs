using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Attei;
using RogyWatchCommon;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace AtteiTest
{
    [TestClass]
    public class AtteITest
    {
        [TestMethod]
        public void PersonCount_V1_Test()
        {
            ushort[] d2;
            short[] d1;

            try
            {
                using (var fs = new FileStream(@"../../../Primitivestest/bin/Debug/DepthData/V1.obj", FileMode.Open, FileAccess.Read))
                {
                    var f = new BinaryFormatter();
                    d1 = (short[])f.Deserialize(fs);

                    var date = DateTime.Now;
                    var date_str = $"{date.Year}_{date.Month}_{date.Day}_{date.Hour}_{date.Minute}_Test";
                    var count = Attei.Attei.PersonCounter(date_str, KinectVersion.V1, d1);

                    // dependent when V1.obj gotten
                    Assert.AreEqual(0, count);
                }
            }catch(Exception ex)
            {

            }

            /*using (var fs = new FileStream(@"DepthData/V2.obj", FileMode.Open, FileAccess.Read))
            {
                var f = new BinaryFormatter();
                d2 = (ushort[])f.Deserialize(fs);
            }*/
            
        }
    }
}
