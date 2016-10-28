using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PrimitiveServerModule;
using RogyWatchCommon;

namespace RogyWatch
{
    class Program
    {
        static void Main(string[] args)
        {
            Servers.Set(new PrimitiveServer());
        }
    }
}
