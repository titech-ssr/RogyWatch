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
        static Attei()
        {

        }

        public static int PersonCounter<T>(string date, KinectVersion v, T depth)
        {
            if (v == KinectVersion.V1)
                return PCL.V1.KinectDisp4(date, (short[])(object)depth);
            else
                throw new NotImplementedException();
        }
    }
}
