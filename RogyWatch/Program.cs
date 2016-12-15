using System;
using RogyWatchCommon;
using APIServerModule;
using PrimitiveServerModule;

namespace RogyWatch
{
    /// <summary>
    /// RogyWatch Main
    /// </summary>
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                Log.logger = NLog.LogManager.GetCurrentClassLogger();
                Log.logger.Info("RogyWatch Started");

                Log.logger.Info("Connection checking...");
                var check = PrimitiveServer.CheckConnection(10.0, 10.0);
                if (!(check.Item1 || check.Item2)) throw new System.IO.IOException("All connection to kinect unavailable!");
                Log.logger.Info($"Connection result V1:{check.Item1}, V2:{check.Item2}");

                var core = new APIServerCore() { Config = Config.DeSerialize() };
                Log.logger.Debug($"Config:\n{core.Config.ToString()}");
                APIServerExterior.StartAPIServer(core);
            }
            catch (Exception ex)
            {
                Log.logger.Fatal($"{ex.Message}\n{ex.StackTrace}");
            }
        }
    }
}
