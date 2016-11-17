using System;
using RogyWatchCommon;
using APIServerModule;

namespace RogyWatch
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                var core = new APIServerCore() { Config = Config.DeSerialize() };
                APIServerExterior.StartAPIServer(core);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"{ex.Message}\n{ex.StackTrace}");
            }
        }
    }
}
