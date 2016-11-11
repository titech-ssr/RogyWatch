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
    public partial class APIServerExterior  //  WebSocket
    {
        private static HttpListener _httpListener, _errHttplistener;
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

                _errHttplistener = new HttpListener();
                _errHttplistener.Prefixes.Add("http://*:8500/");
                _errHttplistener.Start();

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


            WebSocket ws_err = null;

            var errContext = _errHttplistener.GetContextAsync();
            if (errContext.Wait(TimeSpan.FromSeconds(20)))
            {
                if (errContext.Result.Request.IsWebSocketRequest)
                {
                    ws_err = (await errContext.Result.AcceptWebSocketAsync(subProtocol: null)).WebSocket;
                }
                else
                {
                    errContext.Result.Response.StatusCode = 400;
                    errContext.Result.Response.Close();
                }
            }
            else
            {

            }


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
