using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.WebSockets;
using System.Net;

namespace APIServerModule
{
    public partial class APIServerExterior  //  WebSocket
    {
        private static HttpListener _httpListener, _errHttplistener;
        private static bool _wsRunnning;
        private static List<WebSocket> _wsclients = new List<WebSocket>();

        /// <summary>
        /// Start named websocket server. Receive APIServerCore and Task.Run()
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="core"></param>
        /// <returns></returns>
        public static Task StartWebSocketServer<T>(T core) where T : IAPIServerCore
        {
            _wsRunnning = true;
            return Task.Run(()=>WSServer(core));
        }

        /// <summary>
        /// Close named webssocket server.
        /// </summary>
        public static void CloseWebSocket()
        {
            _wsRunnning = false;
            _httpListener.Close();
            _wsclients.ForEach(ws => ws.Dispose());
        }
        
        
        /// <summary>
        /// Main body of WebSocketServer. requires APIServerCore.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="core"></param>
        private static void WSServer<T>(T core) where T : IAPIServerCore
        {
            try
            {
                _httpListener = new HttpListener();
                _httpListener.Prefixes.Add($"http://{core.Config.WebSocketSTD.Host}:{core.Config.WebSocketSTD.Port}/");
                _httpListener.Start();

                _errHttplistener = new HttpListener();
                _errHttplistener.Prefixes.Add($"http://{core.Config.WebSocketSTDERR.Host}:{core.Config.WebSocketSTDERR.Port}/");
                _errHttplistener.Start();

                while (_wsRunnning)
                {
                    var listenerContext = _httpListener.GetContext();
                    if (listenerContext.Request.IsWebSocketRequest)
                    {
                        WebSocket ws_err = null;
                        var errContext = _errHttplistener.GetContextAsync();
                        var ws = listenerContext.AcceptWebSocketAsync(subProtocol: null).Result.WebSocket;
                        if (errContext.Wait(TimeSpan.FromSeconds(core.Config.WebSocketSTDERR.ConnectionTimeout)))
                        {
                            if (errContext.Result.Request.IsWebSocketRequest)
                            {
                                ws_err = (errContext.Result.AcceptWebSocketAsync(subProtocol: null)).Result.WebSocket;
                            }
                            else
                            {
                                errContext.Result.Response.StatusCode = 400;
                                errContext.Result.Response.Close();
                            }
                        }
                        Task.Run(() => OnReceiveWS(listenerContext, ws, ws_err, core));
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
                Console.WriteLine($"{ex.Message}\n{ex.StackTrace}");
            }
        }

        /// <summary>
        /// Invoked when NamedPipeServer received data.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="lisCon"></param>
        /// <param name="ws"></param>
        /// <param name="ws_err"></param>
        /// <param name="core"></param>
        private static async void OnReceiveWS<T>(HttpListenerContext lisCon, WebSocket ws, WebSocket ws_err, T core) where T : IAPIServerCore
        {
            Console.WriteLine($"{DateTime.Now}: new session {lisCon.Request.RemoteEndPoint.Address}");
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
                        InterpretWS(ws, ws_err, line, core);
                    }else if (ret.MessageType == WebSocketMessageType.Close) {
                        Console.WriteLine($"{DateTime.Now} Session Close {lisCon.Request.RemoteEndPoint.Address}");
                    }
                }catch(Exception ex)
                {
                    Console.WriteLine($"{DateTime.Now} session abort {lisCon.Request.RemoteEndPoint.Address}");
                    Console.WriteLine($"{ex.Message}\n{ex.StackTrace}");
                    break;
                }
            }

            _wsclients.Remove(ws);
            ws.Dispose();
        }

        /// <summary>
        /// Mediation between webssocket to APIServerCore
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="ws"></param>
        /// <param name="line"></param>
        /// <param name="core"></param>
        private static void InterpretWS<T>(WebSocket ws, WebSocket ws_err, string line, T core) where T : IAPIServerCore
        {
            try
            {
                var result = core.Invoke<IEnumerable<string>>(line);
                var response = Encoding.UTF8.GetBytes(result.ToString());
                ws.SendAsync(new ArraySegment<byte>(response), WebSocketMessageType.Text, true, System.Threading.CancellationToken.None);
            }catch(Exception ex)
            {
                (ws_err??ws).SendAsync(
                    new ArraySegment<byte>(Encoding.UTF8.GetBytes($"{ex.Message}\n{ex.StackTrace}")), 
                    WebSocketMessageType.Text, 
                    true, 
                    System.Threading.CancellationToken.None);
            }
        }
    }
}
