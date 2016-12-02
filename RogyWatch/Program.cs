using System;
using RogyWatchCommon;
using APIServerModule;

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
