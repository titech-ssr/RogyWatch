using RogyWatchCommon.Primitive.DriverInterface;

namespace PrimitiveDriverV1
{
    public abstract class PrimitiveDriverBase : IPrimitiveDriver<short[]>
    {
        public abstract ushort RANGE_X { get; }
        public abstract ushort RANGE_Y { get; }
        readonly ushort _RANGE_X;
        readonly ushort _RANGE_Y;

        public abstract short[] GetDepth(ushort limit = 10000);
        protected abstract void _Open();
    }
}
