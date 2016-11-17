extern alias KinectV1;

using System;
using System.Collections.Generic;
using System.Linq;
using RogyWatchCommon;

namespace PrimitiveDriverV1
{
    using KinectV1::Microsoft.Kinect;

    /// <summary>
    /// This is a class to translate Kinect Verision dependent type difference into primitive data type (e.g. DepthImagePixel into short[]) <para/>
    /// for Kinect V1.6
    /// </summary>
    public class PrimitiveDriver : PrimitiveDriverBase, IDisposable
    {
        KinectSensor kinect;
        DepthImageStream diStream;
        readonly ushort _RANGE_X;
        readonly ushort _RANGE_Y;
        public override ushort RANGE_X => _RANGE_X;
        public override ushort RANGE_Y => _RANGE_Y;

        /// <summary>
        /// <para>Open KinectV1 and Instantiate Driver</para>
        /// Possibly KinectNotFoundException thrown
        /// </summary>
        /// <param name="rangex">X size of depth </param>
        /// <param name="rangey">Y size of depth </param>
        public PrimitiveDriver(ushort rangex = 640, ushort rangey = 480)
        {
            _Open();
            _RANGE_X = rangex;
            _RANGE_Y = rangey;
        }

        /// <summary>
        /// Open KinectV1 <para/>
        /// Exception: <para/>
        /// KinectNotFoundException
        /// </summary>
        protected override void _Open()
        {
           
            kinect = KinectSensor.KinectSensors.FirstOrDefault(x => x.Status == KinectStatus.Connected);
            if (kinect != null)
            {
                kinect.DepthStream.Enable(DepthImageFormat.Resolution640x480Fps30);

                diStream = kinect.DepthStream;
            }
            if (kinect == null) throw new KinectNotFoundException("Kinect V1 Open Failed");
            if (!kinect.IsRunning) kinect.Start();
        }

        /// <summary>
        /// Get Depth array of 640x480 and trial limited to 10000 milli seconds by default <para/>
        /// Exception: <para />
        /// TimeoutException
        /// </summary>
        /// <param name="limit">trial limit to get depth</param>
        /// <returns>Depth: ushort[]</returns>
        public override short[] GetDepth(ushort limit = 10000)
        {
            var depthPixel = new DepthImagePixel[RANGE_X * RANGE_Y];
            var depth = new short[RANGE_X * RANGE_Y];

            using (var diFrame = diStream.OpenNextFrame(limit))
            {
                if (diFrame == null) throw new TimeoutException($"Depth accession failed. Time({limit}[ms]) out!");

                diFrame.CopyDepthImagePixelDataTo(depthPixel);
            }

            // get depth data
            foreach (var di in depthPixel.Select((d, i) => new KeyValuePair<int, short>(i, d.Depth)))
                depth[di.Key] = di.Value;
            return depth;
        }

        public void Dispose()
        {
            kinect?.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}
