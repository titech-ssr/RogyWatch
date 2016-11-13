using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
                var config = Config.DeSerialize();
                var core = new APIServerCore();
                APIServerExterior.StartAPIServer(core, config);
            }catch(Exception ex)
            {
                Console.WriteLine($"{ex.Message}\n{ex.StackTrace}");
            }
        }
    }
}
