using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO.Pipes;
using System.IO;
using System.Net;
using System.Net.Sockets;
using PrimitiveServerModule;
using RogyWatchCommon;

namespace APIServerModule
{
    /// <summary>
    /// Provide NamedPipe and UDP API
    /// </summary>
    public partial class APIServerExterior  // Pipe
    {
        private static NamedPipeServerStream _pipe;
        private static StreamReader _reader;
        private static StreamWriter _writer;
        private static bool _pipeRunnning;

        static APIServerExterior()
        {
        }

        public static void StartPipeServer<T>(T core) where T : IAPIServerCore
        {
            _pipeRunnning = true;
            Task.Factory.StartNew(()=>PipeServer(core));
        }

        public static void ClosePipeSever()
        {
            if (_pipeRunnning)
            {
                _pipeRunnning = false;
                try
                {
                    _reader.Close();
                    _writer.Close();
                    _pipe.Close();
                }catch(Exception ex) { Console.WriteLine($"{ex.Message}\n{ex.StackTrace}"); }
            }
        }

        private static void PipeStart()
        {
            _pipe = new NamedPipeServerStream("Kinect", PipeDirection.InOut, 1, PipeTransmissionMode.Byte, PipeOptions.Asynchronous);
            _pipe.WaitForConnection();
            _reader = new StreamReader(_pipe);
            _writer = new StreamWriter(_pipe);
        }

        private static void OnReceivePipe<T>(byte[] data,  T core) where T : IAPIServerCore
        {
            try
            {
                var result = Interpret(data, core).Select(i=>(char)i).ToArray();
                _writer.Write(result);
                _writer.Flush();
            }catch(Exception ex) {
                Console.WriteLine($"{ex.Message}\n{ex.StackTrace}");
            }
        }

        private static void PipeServer<T>(T core) where T : IAPIServerCore
        {
            PipeStart();
            while (_pipeRunnning)
            {
                try
                {
                    var buff = new byte[257];               // max api length
                    {
                        var _buff = new char[buff.Length];
                        _reader.Read(_buff, 0, buff.Length);
                        Buffer.BlockCopy(_buff, 0, buff, 0, buff.Length);
                    }
                    Task.Factory.StartNew(() => OnReceivePipe(buff, core));
                }
                catch (IOException ex)
                {
                    Console.WriteLine($"{ex.Message}\n{ex.StackTrace}");

                    // エラー処理
                    _pipe.Close();
                    PipeStart();
                    continue;
                }
            }
        }
    }

    static partial class APIServerExterior   // UDP
    {
        private static UdpClient _udp;
        private static UdpClient _client;
        private static IPEndPoint _remote = null;
        private static bool _udpRunning;

        public static void StartUDPServer<T>(T core) where T : IAPIServerCore
        {
            _udpRunning = true;
            Task.Factory.StartNew(()=>UDPServer(core));
        }

        public static void CloseUDPServer()
        {
            _udpRunning = false;
            _udp.Close();
            _client.Close();
        }

        private static void UDPServer<T>(T core) where T : IAPIServerCore
        {
            _udp = new UdpClient(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 4000));
            _client = new UdpClient(4000);

            while (_udpRunning)
            {
                var buff = _udp.Receive(ref _remote);
                Task.Factory.StartNew(() => OnReceiveUDP(buff, _remote, core));
            }
        }

        private static void OnReceiveUDP<T>(byte[] data, IPEndPoint remote, T core) where T : IAPIServerCore
        {
            try
            {
                var result = Interpret(data, core);
                _client.Connect(remote);
                _client.Send(result, result.Length);
            }catch(Exception ex) {
                Console.WriteLine($"{ex.Message}\n{ex.StackTrace}");
            }
        }
    }

    static partial class APIServerExterior  // Common
    {
        public static void StartAPIServer<T>(T core) where T : IAPIServerCore
        {
            StartUDPServer(core);
            StartPipeServer(core);
        }

        // Result ex.            
        //                        +------- Interpret Succeeded? ( 0:false, 1:true)
        // data[0]                |+------+-- specify size of "data size" as byte unit  +------------------------- Data
        // 0b0000 0000          0b1000 0010         0b0000 0001     0b0000 0000         0b0000 0000     0b0010 1100 .....
        //   |+------+---- API                      +-------------------------+-- specify data size as byte unit
        //   +------------ Kinect Version. 1 => V1, 0 => V2
        // Response to GetDepth of KinectV2, Interpret Succeeded and size of "data size" is 2 bytes(Uint16) and data size is 256 bytes
        // i.e, Got 256 bytes of Depth Data from KinectV2.
        private static byte[] Interpret<T>(byte[] data,T core) where T : IAPIServerCore
        {
            var d = data[0];
            var ver = (KinectVersion)(d & 0x80 );
            var api = (API)(d & 0x7F);

            try
            {
                switch (api)
                {
                    case API.GetDepth:
                        var depth = (Array)core.GetDepth(ver);
                        if (depth == null) throw new NullReferenceException("Depth data null");

                        var depthByte = new byte[depth.Length*2];
                        for(var i = 0; i < depth.Length; i++)
                        {
                            var byts = ver == KinectVersion.V1 ?
                                BitConverter.GetBytes((short)depth.GetValue(i)) :
                                BitConverter.GetBytes((ushort)depth.GetValue(i));
                            depthByte[i * 2] = byts[0];
                            depthByte[i * 2 + 1] = byts[1];
                        }
                        return GenerateResult((byte)((int)api | (byte)ver), Status.Succeeded, depthByte);
                    case API.RunMethod:
                        //return ParseLine(Encoding.UTF8.GetString(data, 1, data.Length-1), core);
                        throw new NotImplementedException();
                    default:
                        throw new ArgumentException("API Not defined");
                }
            }catch(Exception ex)
            {
                var mesg = Encoding.UTF8.GetBytes(ex.Message);
                return GenerateResult((byte)((int)ver | (byte)api), Status.Failed, mesg);
            }
        }

        /// <summary>
        /// Generate result bytes from KinectVersion, api, status, data bytes
        /// </summary>
        /// <param name="head"></param>
        /// <param name="status"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public static byte[] GenerateResult(byte head, Status status, byte[] data)
        {
            var req = RequiredSizeofDataSize(data.Length);
            var result = new byte[2 + req + data.Length];
            result[0] = head;
            result[1] = (byte)((int)status | req);
            var datasize = BitConverter.GetBytes(data.Length);
            var i = 0;
            for (i = 0; i < req; i++)
                result[2 + i] = datasize[i];
            for (var i2 = 0; i2 < data.Length; i2++)
                result[2 + i + i2] = data[i2];

            return result;
        }

        /// <summary>
        /// calculate size [bytes] of "data size" area from actual data size
        /// </summary>
        /// <returns></returns>
        public static byte RequiredSizeofDataSize(int dsize)
        {
            byte i;
            for (i = 0; (dsize >> i) != 0; i++) ;
            return (byte)(i / 8 + (i % 8 > 0 ? 1 : 0));
        }

        /// <summary>
        /// parse command to server. Nullable return.
        /// </summary>
        /// <param name="line"></param>
        /// <returns></returns>
        private static object ParseLine<T>(string line, T core) where T : IAPIServerCore
        {
            try
            {
                return typeof(T).GetMethod(line)?.Invoke(core, new[] { line });
            }catch(Exception ex)
            {
                Console.WriteLine($"{ex.Message}\n{ex.StackTrace}");
                return null;
            }
        }
    }
}
