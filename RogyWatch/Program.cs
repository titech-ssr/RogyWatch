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
                var core = new APIServerCore() { Config = Config.DeSerialize() };
                Console.WriteLine($"Config:\n{core.Config.ToString()}");
                APIServerExterior.StartAPIServer(core);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"{ex.Message}\n{ex.StackTrace}");
            }
        }
    }
}
