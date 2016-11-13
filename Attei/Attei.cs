using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RogyWatchCommon;

namespace Attei
{
    public partial class Attei  //  Common
    {
        static Attei() {}

        public static int PersonCounter<T>(string date, KinectVersion v, T depth, Config config)
        {
            if (v == KinectVersion.V1)
                return PCL.V1.KinectDisp4(date, (short[])(object)depth, config);
            else
                return PCL.V2.KinectDisp4(date, (ushort[])(object)depth, config);
        }
    }

    namespace PCL
    {
        public struct RGBData
        {
            public byte R;
            public byte G;
            public byte B;

            public RGBData(byte _R, byte _G, byte _B)
            {
                R = _R;
                G = _G;
                B = _B;
            }


            const short MAX = 5500;
            const short MIN = 1500;

            public static RGBData getContrast(short depth)
            {
                if (depth > MAX) depth = MAX;
                if (depth < MIN) depth = MIN;
                depth -= MIN;
                float H = 360.0f * depth / (MAX - MIN);
                float S = 1.0f;
                float V = 1.0f;

                int i = ((int)(5 - (H / 60)) % 6);
                float f = (H / 60.0f) - (float)Math.Floor(H / 60.0f);
                byte p = (byte)(V * (1.0f - S) * 255);
                byte q = (byte)(V * (1.0f - f * S) * 255);
                byte t = (byte)(V * (1.0f - f * S) * 255);
                byte v = (byte)(V * 255);

                switch (i)
                {
                    case 0:
                        return new RGBData(v, t, p);
                    case 1:
                        return new RGBData(q, v, p);
                    case 2:
                        return new RGBData(p, v, t);
                    case 3:
                        return new RGBData(p, q, v);
                    case 4:
                        return new RGBData(t, p, v);
                    case 5:
                        return new RGBData(v, p, q);
                }
                return new RGBData(0, 0, 0);
            }

            public static RGBData getContrast(int depth)
            {
                if (depth > MAX) depth = MAX;
                if (depth < MIN) depth = MIN;
                depth -= MIN;
                float H = 360.0f * depth / (MAX - MIN);
                float S = 1.0f;
                float V = 1.0f;

                int i = ((int)(5 - (H / 60)) % 6);
                float f = (H / 60.0f) - (float)Math.Floor(H / 60.0f);
                byte p = (byte)(V * (1.0f - S) * 255);
                byte q = (byte)(V * (1.0f - f * S) * 255);
                byte t = (byte)(V * (1.0f - f * S) * 255);
                byte v = (byte)(V * 255);

                switch (i)
                {
                    case 0:
                        return new RGBData(v, t, p);
                    case 1:
                        return new RGBData(q, v, p);
                    case 2:
                        return new RGBData(p, v, t);
                    case 3:
                        return new RGBData(p, q, v);
                    case 4:
                        return new RGBData(t, p, v);
                    case 5:
                        return new RGBData(v, p, q);
                }
                return new RGBData(0, 0, 0);
            }   //  getContrast
        }
    }
}
