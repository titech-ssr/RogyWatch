extern alias KinectV2;

using System;
using RogyWatchCommon;

namespace PrimitiveDriverV2
{
    using KinectV2::Microsoft.Kinect;

    /// <summary>
    /// This is a class to translate Kinect Verision dependent type difference into primitive data type (e.g. DepthImagePixel into short[]) <para/>
    /// for Kinect V2.0
    /// </summary>
    public class PrimitiveDriver : PrimitiveDriverBase, IDisposable
    {
        KinectSensor kinect;
        readonly ushort _RANGE_X;
        readonly ushort _RANGE_Y;
        public override ushort RANGE_X => _RANGE_X;
        public override ushort RANGE_Y => _RANGE_Y;

        /// <summary>
        /// <para>Open KinectV2 and Instantiate Driver</para>
        /// Possibly KinectNotFoundException thrown
        /// </summary>
        /// <param name="rangex">X size of depth </param>
        /// <param name="rangey">Y size of depth </param>
        public PrimitiveDriver(ushort rangex=512, ushort rangey=424)
        {
            _Open();
            _RANGE_X = rangex;
            _RANGE_Y = rangey;
        }

        /// <summary>
        /// Open KinectV2 <para/>
        /// Exception: <para/>
        /// KinectNotFoundException
        /// </summary>
        protected override void _Open()
        {
            kinect = KinectSensor.GetDefault();
            if (kinect == null) throw new KinectNotFoundException("Kinect V2 Open Failed");
            if (!kinect.IsOpen || !kinect.IsAvailable) kinect.Open();
        }
        
        /// <summary>
        /// Get Depth array of 512x424 and trial limited to 10000 times by default <para/>
        /// Exception: <para />
        /// TimeoutException
        /// </summary>
        /// <param name="limit">trial limit to get depth</param>
        /// <returns>Depth: ushort[]</returns>
        public override ushort[] GetDepth(ushort limit = 10000)
        {
            var depthPixel = new ushort[RANGE_X * RANGE_Y];
            while (true)
            {
                var depthFrame = kinect.DepthFrameSource.OpenReader().AcquireLatestFrame();
                if (depthFrame == null)
                {
                    if (limit-- < 0)
                        throw new TimeoutException($"Depth accession failed. Limit Count exceeded!");
                    else
                        continue;
                }

                depthFrame.CopyFrameDataToArray(depthPixel);
                depthFrame.Dispose();
                return depthPixel;
            }
        }

        public void Dispose()
        {
            kinect?.Close();
            GC.SuppressFinalize(this);
        }
    }
}
