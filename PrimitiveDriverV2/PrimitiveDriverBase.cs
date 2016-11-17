using RogyWatchCommon.Primitive.DriverInterface;

namespace PrimitiveDriverV2
{
    public abstract class PrimitiveDriverBase : IPrimitiveDriver<ushort[]>
    {
        public abstract ushort RANGE_X { get; }
        public abstract ushort RANGE_Y { get; }
        readonly ushort _RANGE_X;
        readonly ushort _RANGE_Y;

        public abstract ushort[] GetDepth(ushort limit = 10000);
        protected abstract void _Open();
    }
}
