namespace RogyWatchCommon
{
    namespace Primitive.DriverInterface
    {
        /// <summary>
        /// Primitive Driver should implement.
        /// </summary>
        /// <typeparam name="T"></typeparam>
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