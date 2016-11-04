using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using APIServerModule;
using System.Net;
using RogyWatchCommon;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Text;
using System.Threading;

namespace APIServerTest
{
    public class APIServerCoreTest : IAPIServerCore
    {
        public T Get<T>(string command)
        {
            return (T)(object)command;
        }

        public object GetDepth(KinectVersion v)
        {
            var ar = new ushort[300];
            ar[0] = 255;
            ar[ar.Length - 1] = 255;
            return ar;
        }

        public int HowManyPeople(string line) { return 0; }

        public string Test(string i) { return i; }
        public ushort[] Ushort() { return new ushort[] { 1, 2, 3, 4 }; }
    }

    [TestClass]
    public class APIServer_Test
    {
        [TestMethod]
        public void PipeServer_Test()
        {
            var core = new APIServerCoreTest();
            APIServerExterior.StartPipeServer(core);
            Thread.Sleep(500);

            var client = new System.IO.Pipes.NamedPipeClientStream(".", "Kinect", System.IO.Pipes.PipeDirection.InOut, System.IO.Pipes.PipeOptions.Asynchronous);
            client.Connect();

            var reader = new System.IO.StreamReader(client);
            var writer = new System.IO.StreamWriter(client);

            // api not found Test
            var msg = new char[] { (char)((int)KinectVersion.V1 | 0x50) };
            writer.Write(msg);
            writer.Flush();
            {
                byte[] buff;
                {
                    var _buff = new char[18];
                    buff = new byte[_buff.Length];
                    reader.Read(_buff, 0, _buff.Length);
                    for (var i_ = 0; i_ < _buff.Length; i_++) buff[i_] = (byte)_buff[i_];
                }
                
                var data = Encoding.UTF8.GetBytes("API Not defined");
                var expect = new byte[2 + 1 + 15];
                expect[0] = (int)KinectVersion.V1 | 0x50;
                expect[1] = 0x00 | 0x01;
                expect[2] = (byte)data.Length;
                Buffer.BlockCopy(data, 0, expect, 3, 15);

                CollectionAssert.AreEqual(expect, buff);
            }

            // get depth (dummy) test
            msg = new char[] { (char)((int)KinectVersion.V2 | 0x00 )};
            writer.WriteLine(msg);
            writer.Flush();
            {
                byte[] buff;
                {
                    var _buff = new char[18];
                    buff = new byte[_buff.Length];
                    reader.Read(_buff, 0, _buff.Length);
                    for (var i_ = 0; i_ < _buff.Length; i_++) buff[i_] = (byte)_buff[i_];
                }

                var size = 2 + 2;
                var expect = new byte[size];
                expect[0] = (int)KinectVersion.V2 | (int)API.GetDepth;
                expect[1] = (int)Status.Succeeded | 0x02;
                expect[2] = 88;
                expect[3] = 2;

                var actual = new byte[size];
                Buffer.BlockCopy(buff, 0, actual, 0, size);

                CollectionAssert.AreEqual(expect, actual);
            }

            APIServerExterior.ClosePipeSever();
        }

        [TestMethod]
        public void UDPServer_API_Test()
        {
            var core = new APIServerCoreTest();
            APIServerExterior.StartUDPServer(core);

            var _udp = new UdpClient(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 4500));

            // connect
            var remote = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 4000);
            var client = new UdpClient(4500);
            client.Connect(remote);

            // api not found Test
            var msg = new byte[] { (int)KinectVersion.V1 | 0x50 };
            client.Send(msg, msg.Length);
            {
                IPEndPoint _remote = null;
                var buff = _udp.Receive(ref _remote);
                
                var data = Encoding.UTF8.GetBytes("API Not defined");
                var expect = new byte[2 + 1 + 15];
                expect[0] = (int)KinectVersion.V1 | 0x50;
                expect[1] = 0x00 | 0x01;
                expect[2] = (byte)data.Length;
                Buffer.BlockCopy(data, 0, expect, 3, 15);

                CollectionAssert.AreEqual(expect, buff);
            }

            // get depth (dummy) test
            msg = new byte[] { (int)KinectVersion.V2 | 0x00 };
            client.Send(msg, msg.Length);
           {
                IPEndPoint _remote = null;
                var buff = _udp.Receive(ref _remote);

                var size = 2 + 2;
                var expect = new byte[size];
                expect[0] = (int)KinectVersion.V2 | (int)API.GetDepth;
                expect[1] = (int)Status.Succeeded | 0x02;
                expect[2] = 88;
                expect[3] =  2;

                var actual = new byte[size];
                Buffer.BlockCopy(buff, 0, actual, 0, size);

                CollectionAssert.AreEqual(expect, actual);
            }

            client.Close();
            APIServerExterior.CloseUDPServer();
        }

        [TestMethod]
        public void DataToBytes_Test()
        {
            var api = (API)(KinectVersion.V1 | (byte)API.GetDepth);
            var status = Status.Succeeded;
            var data = new byte[300];
            data[0] = 1;
            data[299] = 255;
            //            +-------- 300 --------+
            // set data = {1, 0 0 0 ...... , 255}
            var result = APIServerExterior.GenerateResult((byte)api, status, data);

            var expected = new byte[4 + 300];
            expected[0] = 0x80;                 // KinectVersion.V1 | API.GetDepth
            expected[1] = 0x80 | 0x02;          // Status.Succeeded | 0x02 ( requires 2 byte as size of "data size" )
            expected[2] = 0x2c;                 //  data size    little endian
            expected[3] = 0x01;                 //  data size    0x00 0x00 0x01 0x2c == 300 byte
            expected[4] = 1;                    // data.first
            expected[4 + 299] = 255;            // data.last
            CollectionAssert.AreEqual(expected, result);
        }

        [TestMethod]
        public void RequiredSizeofDataSize_Test()
        {
            // 1 8bitable
            var size = 1;
            Assert.AreEqual(1, APIServerExterior.RequiredSizeofDataSize(size));

            // 255 8bitable
            size = 255;
            Assert.AreEqual(1, APIServerExterior.RequiredSizeofDataSize(size));

            // 256 16bitable
            size = 256;
            Assert.AreEqual(2, APIServerExterior.RequiredSizeofDataSize(size));
        }
    }
}
