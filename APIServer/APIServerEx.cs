using System;
using System.Linq;
using System.Threading.Tasks;
using System.IO.Pipes;
using System.IO;
using System.Net;
using System.Net.Sockets;
using RogyWatchCommon;

namespace APIServerModule
{
    /// <summary>
    /// Provide NamedPipe and UDP API Server exterior.
    /// </summary>
    public partial class APIServerExterior  // Pipe
    {
        private static NamedPipeServerStream _pipe;
        private static StreamReader _reader;
        private static StreamWriter _writer;
        private static bool _pipeRunnning;

        /// <summary>
        /// empty
        /// </summary>
        static APIServerExterior()
        {
        }

        /// <summary>
        /// Start named pipe server. Receive APIServerCore and Task.Run()
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="core"></param>
        /// <returns></returns>
        public static Task StartPipeServer<T>(T core) where T : IAPIServerCore
        {
            _pipeRunnning = true;
            return Task.Factory.StartNew(()=>PipeServer(core));
        }

        /// <summary>
        /// Close named pipe server.
        /// </summary>
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

        /// <summary>
        /// Makes pipe,reader,writer stream instance and wait for pipe connnection.
        /// </summary>
        /// <param name="config"></param>
        private static void PipeStart(Config config)
        {
            _pipe = new NamedPipeServerStream(config.NamedPipe.Host, PipeDirection.InOut, 1, PipeTransmissionMode.Byte, PipeOptions.Asynchronous);
            _pipe.WaitForConnection();
            _reader = new StreamReader(_pipe);
            _writer = new StreamWriter(_pipe);
        }

        /// <summary>
        /// Invoked when NamedPipeServer received data.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="data">received data series</param>
        /// <param name="core"></param>
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

        /// <summary>
        /// Main body of PipeServer. requires APIServerCore.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="core"></param>
        private static void PipeServer<T>(T core) where T : IAPIServerCore
        {
            PipeStart(core.Config);
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
                    PipeStart(core.Config);
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

        /// <summary>
        /// Start udp server. Receive APIServerCore and Task.Run()
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="core"></param>
        /// <param name="config"></param>
        /// <returns></returns>
        public static Task StartUDPServer<T>(T core) where T : IAPIServerCore
        {
            _udpRunning = true;
            return Task.Factory.StartNew(()=>UDPServer(core));
        }


        /// <summary>
        /// Close udp server.
        /// </summary>
        public static void CloseUDPServer()
        {
            _udpRunning = false;
            _udp.Close();
            _client.Close();
        }

        /// <summary>
        /// Main body of UDPServer. requires APIServerCore.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="core"></param>
        private static void UDPServer<T>(T core) where T : IAPIServerCore
        {
            _udp = new UdpClient(new IPEndPoint(IPAddress.Parse(core.Config.UDP.Host), core.Config.UDP.Port));
            _client = new UdpClient(4000);

            while (_udpRunning)
            {
                var buff = _udp.Receive(ref _remote);
                Task.Factory.StartNew(() => OnReceiveUDP(buff, _remote, core));
            }
        }

        /// <summary>
        /// Invoked when UDPServer received data.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="data">received data series</param>
        /// <param name="remote"></param>
        /// <param name="core"></param>
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
}
