using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RogyWatchCommon;

namespace APIServerModule
{
    static partial class APIServerExterior  // Common
    {
        /// <summary>
        /// Start API servers
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="core"></param>
        public static void StartAPIServer<T>(T core) where T : IAPIServerCore
        {
            //StartUDPServer(core);
            //StartPipeServer(core);
            //StartWebSocketServer(core);

            var tasks = new List<Task>();
            foreach (var server in core.Config.Start)
                tasks.Add((Task)typeof(APIServerExterior)
                    .GetMethod($"Start{server}Server")
                    .MakeGenericMethod(typeof(IAPIServerCore))
                    .Invoke(null, new object[] {core}));
            foreach (var task in tasks)
                task.Wait();
        }

        // Result ex.            
        //  data[0]              data[1]             data[2]         data[3]             data[4]...
        //
        //                        +------- Interpret Succeeded? ( 0:false, 1:true)
        //                        |+------+-- specify size of "data size" as byte unit  +------------------------- Data
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
