using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace RogyWatchCommon {
    namespace Primitive.DriverInterface
    {
        public interface IPrimitiveDriver<T>
        {
            ushort RANGE_X { get; }
            ushort RANGE_Y { get; }
            T GetDepth(ushort limit = 10000);
        }
    }

    namespace Primitive.ServerInterface
    {
    }
}    