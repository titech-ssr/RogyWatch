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

        public static Task StartPipeServer<T>(T core) where T : IAPIServerCore
        {
            _pipeRunnning = true;
            return Task.Factory.StartNew(()=>PipeServer(core));
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

        private static void PipeStart(Config config)
        {
            _pipe = new NamedPipeServerStream(config.NamedPipe.Host, PipeDirection.InOut, 1, PipeTransmissionMode.Byte, PipeOptions.Asynchronous);
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

        public static Task StartUDPServer<T>(T core, Config config) where T : IAPIServerCore
        {
            _udpRunning = true;
            return Task.Factory.StartNew(()=>UDPServer(core, config));
        }

        public static void CloseUDPServer()
        {
            _udpRunning = false;
            _udp.Close();
            _client.Close();
        }

        private static void UDPServer<T>(T core, Config config) where T : IAPIServerCore
        {
            _udp = new UdpClient(new IPEndPoint(IPAddress.Parse(config.UDP.Host), config.UDP.Port));
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
}
