using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.WebSockets;
using System.Net;
using System.Net.Sockets;

namespace APIServerModule
{
    public partial class APIServerExterior  //  WeebSocket
    {
        private static HttpListener _httpListener;
        private static bool _wsRunnning;
        private static List<WebSocket> _wsclients = new List<WebSocket>();

        public static void StartWebSocket<T>(T core) where T : IAPIServerCore
        {
            _wsRunnning = true;
            Task.Run(()=>WSServer(core));
        }

        public static void CloseWebSocket()
        {
            _wsRunnning = false;
            _httpListener.Close();
            _wsclients.ForEach(ws => ws.Dispose());
        }
        
        
        private static void WSServer<T>(T core) where T : IAPIServerCore
        {
            try
            {
                _httpListener = new HttpListener();
                _httpListener.Prefixes.Add("http://*:8000/");
                _httpListener.Start();

                while (_wsRunnning)
                {
                    var listenerContext = _httpListener.GetContext();
                    if (listenerContext.Request.IsWebSocketRequest)
                    {
                        Task.Run(() => OnReceiveWS(listenerContext, core));
                    }
                    else
                    {
                        listenerContext.Response.StatusCode = 400;
                        listenerContext.Response.Close();
                    }
                }
            }
            catch(Exception ex)
            {

            }
        }

        private static async void OnReceiveWS<T>(HttpListenerContext lisCon, T core) where T : IAPIServerCore
        {
            Console.WriteLine($"{DateTime.Now}: new session {lisCon.Request.RemoteEndPoint.Address}");
            var ws = (await lisCon.AcceptWebSocketAsync(subProtocol: null)).WebSocket;
            _wsclients.Add(ws);

            var buff = new ArraySegment<byte>(new byte[512]);
            while (ws.State == WebSocketState.Open)
            {
                try
                {
                    var ret = await ws.ReceiveAsync(buff, System.Threading.CancellationToken.None);
                    var line = Encoding.UTF8.GetString(buff.Take(ret.Count).ToArray());

                    if (ret.MessageType == WebSocketMessageType.Text)
                    {
                        Console.WriteLine($"{DateTime.Now} String Received from {lisCon.Request.RemoteEndPoint.Address}\n{line}");
                        InterpretWS(ws, line, core);
                    }else if (ret.MessageType == WebSocketMessageType.Close) {
                        Console.WriteLine($"{DateTime.Now} Session Close {lisCon.Request.RemoteEndPoint.Address}");
                    }
                }catch(Exception ex)
                {
                    Console.WriteLine($"{DateTime.Now} session abort {lisCon.Request.RemoteEndPoint.Address}");
                    break;
                }
            }

            _wsclients.Remove(ws);
            ws.Dispose();
        }

        /// <summary>
        /// mediation between webssocket to ControlServerCore
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="ws"></param>
        /// <param name="line"></param>
        /// <param name="core"></param>
        private static void InterpretWS<T>(WebSocket ws, string line, T core) where T : IAPIServerCore
        {
            var result = ControlServerCore.Invoke(line);
            var response = Encoding.UTF8.GetBytes(result.ToString());
            ws.SendAsync(new ArraySegment<byte>(response), WebSocketMessageType.Text, true, System.Threading.CancellationToken.None);
        }
    }
}
