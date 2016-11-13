using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Media3D;
using RogyWatchCommon;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Windows.Media;

namespace Attei.PCL
{
    public partial class V2
    {
        static readonly int DEPTH_X, DEPTH_Y;


        static GeometryModel3D[] points;

        private static byte[] draw;
        private static bool[,] obj, obj2;

        private static Vector3D[,] allPoint;
        private static Vector3D[,] allPoint_high;

        private static Int32Rect _ScreenImageRect;
        private static WriteableBitmap _ScreenImage;

        private static Vector3D[] g;

        private static List<List<Point>> blobList;
        private static List<List<Point>> topPoints;
        private static List<List<Point>> integratedTopPoints;

        private static ushort[,] depthMap;


        static V2()
        {
            var conf = new PrimitiveDriverV2();
            DEPTH_X = conf.DEPTH_X;
            DEPTH_Y = conf.DEPTH_Y;
            init();
        }

        public static void init()
        {
            draw = new byte[DEPTH_X * DEPTH_Y * 4];
            obj = new bool[DEPTH_Y, DEPTH_X];

            _ScreenImage = new WriteableBitmap(DEPTH_X, DEPTH_Y, 96, 96, PixelFormats.Bgr32, null);
            _ScreenImageRect = new Int32Rect(0, 0, DEPTH_X, DEPTH_Y);

            allPoint = new Vector3D[DEPTH_Y, DEPTH_X];

            depthMap = new ushort[DEPTH_Y, DEPTH_X];
            points = new GeometryModel3D[DEPTH_X * DEPTH_Y];
        }

        private class PointComparer : IComparer<Point>
        {
            public int Compare(Point x, Point y)
            {
                if (x.Y != y.Y)
                {
                    return (int)(x.Y - y.Y);
                }
                else
                {
                    return (int)(x.X - y.X);
                }

            }
        }

        private static void spinPoint(ref double x, ref double y, ref double z, double theta, string axis)
        {
            theta = theta * Math.PI * 2 / 360;
            double _x = x;
            double _y = y;
            double _z = z;

            if (axis == "X")
            {
                _y = (y * Math.Cos(theta) + z * Math.Sin(-theta));
                _z = (y * Math.Sin(theta) + z * Math.Cos(theta));
            }
            else if (axis == "Y")
            {
                _z = (z * Math.Cos(theta) + x * Math.Sin(-theta));
                _x = (z * Math.Sin(theta) + x * Math.Cos(theta));
            }
            else if (axis == "Z")
            {
                _x = (x * Math.Cos(theta) + y * Math.Sin(-theta));
                _y = (x * Math.Sin(theta) + y * Math.Cos(theta));
            }
            x = _x;
            y = _y;
            z = _z;
        }

        private static void parse3d(ushort[] dip)
        {
            double div_x = 0.70020 / (DEPTH_X / 2);
            double div_y = 0.57735 / (DEPTH_Y / 2);
            const double div_z = 1;
            const double div = 0.3;


            obj = new bool[DEPTH_Y, DEPTH_X];
            allPoint = new Vector3D[DEPTH_Y, DEPTH_X];
            allPoint_high = new Vector3D[DEPTH_Y, DEPTH_X];

            for (int y = 0; y < DEPTH_Y; y++)
            {
                for (int x = 0; x < DEPTH_X; x++)
                {
                    depthMap[y, x] = dip[y * DEPTH_X + x];


                    if (dip[y * DEPTH_X + x] == 0)
                    {
                        allPoint[y, x].X = 0;
                        allPoint[y, x].Y = 0;
                        allPoint[y, x].Z = 10;
                    }
                    else
                    {
                        double X = (DEPTH_X / 2 - x) * dip[y * DEPTH_X + x] * div_x / div;
                        double Y = (y - DEPTH_Y / 2) * dip[y * DEPTH_X + x] * div_y / div;
                        double Z = (dip[y * DEPTH_X + x]) * div_z / div;

                        //いい感じに座標変換。y=0が地面の高さになる感じ
                        spinPoint(ref X, ref Y, ref Z, 150, "X");
                        Y += 7000;
                        Z += 17000;
                        X += 2400;
                        spinPoint(ref X, ref Y, ref Z, 8, "Y");
                        X -= 1300;
                        Z += 300;


                        //地面を消す
                        if (Y < 0)
                        {
                            allPoint[y, x].X = 0;
                            allPoint[y, x].Y = 0;
                            allPoint[y, x].Z = 10;
                            continue;
                        }
                        //十分に高そうな地点より上を切る
                        if (Y > 4000)
                        {
                            allPoint[y, x].X = 0;
                            allPoint[y, x].Y = 0;
                            allPoint[y, x].Z = 10;
                            continue;
                        }
                        //窓際全消し
                        if (Z < 0)
                        {
                            allPoint[y, x].X = 0;
                            allPoint[y, x].Y = 0;
                            allPoint[y, x].Z = 10;
                            continue;
                        }
                        //回路領域側けす
                        if (X > 6000)
                        {
                            allPoint[y, x].X = 0;
                            allPoint[y, x].Y = 0;
                            allPoint[y, x].Z = 10;
                            continue;
                        }

                        //機械領域消す
                        if (Z > 11500)
                        {
                            allPoint[y, x].X = 0;
                            allPoint[y, x].Y = 0;
                            allPoint[y, x].Z = 10;
                            continue;
                        }

                        //Y軸を冷蔵庫のKinectの一番近い側に持ってくる
                        Z -= 4000;

                        //PC側消す
                        if (X < 0)
                        {
                            allPoint[y, x].X = 0;
                            allPoint[y, x].Y = 0;
                            allPoint[y, x].Z = 10;
                            continue;
                        }
                        //同じくPC側(冷蔵庫より窓側)
                        if (X < 500 && Z < 0)
                        {
                            allPoint[y, x].X = 0;
                            allPoint[y, x].Y = 0;
                            allPoint[y, x].Z = 10;
                            continue;
                        }

                        //Y軸をPC側と机側の境界に持ってくる
                        X -= 200;
                        Y -= 2100;
                        Z -= 2500;


                        //PC側消す
                        if (X < 0 && Z > 0)
                        {
                            allPoint[y, x].X = 0;
                            allPoint[y, x].Y = 0;
                            allPoint[y, x].Z = 10;
                            continue;
                        }

                        //PC側の地面消す
                        if (Y < 0 && Z > 0)
                        {
                            allPoint[y, x].X = 0;
                            allPoint[y, x].Y = 0;
                            allPoint[y, x].Z = 10;
                            continue;
                        }
                        X -= 1500;
                        Y += 1200;
                        Z += 1500;
                        spinPoint(ref X, ref Y, ref Z, -3, "Y");
                        //鞄消す
                        if (Y < 0)
                        {
                            allPoint[y, x].X = 0;
                            allPoint[y, x].Y = 0;
                            allPoint[y, x].Z = 10;
                            continue;
                        }
                        //机消す
                        if (X > 0 && X < 3500 && Z < 0)
                        {
                            allPoint[y, x].X = 0;
                            allPoint[y, x].Y = 0;
                            allPoint[y, x].Z = 10;
                            continue;
                        }
                        Z += 7500;
                        Z -= 5000;
                        Z *= -1.1;
                        Z += 3500;
                        X *= -1.0;
                        X += 700;
                        X *= 1.2;
                        Z *= 0.8;

                        allPoint[y, x].X = (int)X;
                        allPoint[y, x].Y = (int)Y;
                        allPoint[y, x].Z = (int)Z;
                        obj[y, x] = true;



                        /*
                        //いい感じに座標変換。y=0が地面の高さになる感じ
                        spinPoint(ref X, ref Y, ref Z, 150, "X");
                        Y += 5000;
                        Z += 13500;
                        X += 2400;

                        //十分に高そうな地点より上を切る
                        if (Y >3000)
                        {
                            allPoint[y, x].X = 0;
                            allPoint[y, x].Y = 0;
                            allPoint[y, x].Z = 10;
                            continue;
                        }

                        //冷蔵庫側のPCとかがある棚をを大きめに消す
                        if (X < 0)
                        {
                            allPoint[y, x].X = 0;
                            allPoint[y, x].Y = 0;
                            allPoint[y, x].Z = 10;
                            continue;
                        }

                        Z -= 100;
                        X -= 1000;

                        //冷蔵庫とかトラ技ある棚とか
                        if (X < 300)
                        {
                            allPoint[y, x].X = 0;
                            allPoint[y, x].Y = 0;
                            allPoint[y, x].Z = 10;
                            continue;
                        }
                        if (X < 300 && Z > 1000)
                        {
                            allPoint[y, x].X = 0;
                            allPoint[y, x].Y = 0;
                            allPoint[y, x].Z = 10;
                            continue;
                        }

                        //窓全消し(かなり余裕持ってる
                        //if (Z < -4400)  2016/02/05
                        if(Z<-3300)
                        {
                            allPoint[y, x].X = 0;
                            allPoint[y, x].Y = 0;
                            allPoint[y, x].Z = 10;
                            continue;
                        }

                        X -= 6000;
                        Z -= 400;
                        X += 200;
                        //領域側の削除
                        if (Z < 0 && X > 0)
                        {
                            allPoint[y, x].X = 0;
                            allPoint[y, x].Y = 0;
                            allPoint[y, x].Z = 10;
                            continue;
                        }

                        Z -= 2500;

                        spinPoint(ref X, ref Y, ref Z, 5, "Y");

                        //電子領域消去
                        if (Z > 0 && X > -300)
                        {
                            allPoint[y, x].X = 0;
                            allPoint[y, x].Y = 0;
                            allPoint[y, x].Z = 10;
                            continue;
                        }

                        //電子領域と研究室領域の間の微妙なアレを消去
                        if (X > 800)
                        {
                            allPoint[y, x].X = 0;
                            allPoint[y, x].Y = 0;
                            allPoint[y, x].Z = 10;
                            continue;
                        }
                        Z -= 4500;
                        X += 1000;

                        //時計とかぶら下がってるあの辺を消す
                        if (Z > 0 && X > 0)
                        {
                            allPoint[y, x].X = 0;
                            allPoint[y, x].Y = 0;
                            allPoint[y, x].Z = 10;
                            continue;
                        }

                        X += 4000;
                        Z -= 1000;

                        //生活領域以外を消す(木の板とか皆殺し)
                        if (Z > 0)
                        {
                            allPoint[y, x].X = 0;
                            allPoint[y, x].Y = 0;
                            allPoint[y, x].Z = 10;
                            continue;
                        }


                        //デバッグ用途で地面を消す

                        Y += 2000;
                        if (Y < 0)
                        {
                            allPoint[y, x].X = 0;
                            allPoint[y, x].Y = 0;
                            allPoint[y, x].Z = 10;
                            continue;
                        }
                        Y -= 2000;

                        //机抽出

                        Z += 8000;
                        Y += 1000;
                        X -= 300;

                        
                        if (X > 0 && X < 2500 && Z < 0)
                        {
                            allPoint[y, x].X = 0;
                            allPoint[y, x].Y = 0;
                            allPoint[y, x].Z = 10;
                            continue;
                        }
                         

                        X -= 2500;
                        Z -= 2000;

                        //机の高さよりした全消し
                        if (Y < 0)
                        { 
                            allPoint[y, x].X = 0;
                            allPoint[y, x].Y = 0;
                            allPoint[y, x].Z = 10;
                            continue;
                        }
                        Y -= 1200;

                        if (Z > 0 && Y < 0)
                        {
                            allPoint[y, x].X = 0;
                            allPoint[y, x].Y = 0;
                            allPoint[y, x].Z = 10;
                            continue;
                        }
                        Y -= 2600;

                        if (Y > 0)
                        {
                            allPoint[y, x].X = 0;
                            allPoint[y, x].Y = 0;
                            allPoint[y, x].Z = 10;
                            continue;
                        }

                        Z += 1000;
                        X *= 1.2;
                        X += 2000;
                        Z *= 0.8;
                        spinPoint(ref X, ref Y, ref Z, 180, "Y");

                        allPoint[y, x].X = (int)X;
                        allPoint[y, x].Y = (int)Y;
                        allPoint[y, x].Z = (int)Z;
                        obj[y, x] = true;
                        */
                    }
                }
            }
        }

        private static void SearchBlob(ushort[] dip)
        {
            blobList = new List<List<Point>>();
            topPoints = new List<List<Point>>();

            for (int y = 0; y < DEPTH_Y; y++)
            {
                obj[y, 0] = false;
                obj[y, DEPTH_X - 1] = false;
            }
            for (int x = 0; x < DEPTH_X; x++)
            {
                obj[0, x] = false;
                obj[DEPTH_Y - 1, x] = false;
            }
            for (int y = 0; y < DEPTH_Y; y++)
            {
                for (int x = 0; x < DEPTH_X; x++)
                {
                    if (obj2[y, x])
                    {
                        for (; x < DEPTH_X; x++)
                        {
                            if (!obj[y, x])
                            {
                                break;
                            }
                        }
                    }
                    if (obj[y, x])
                    {
                        blobList.Add(new List<Point>());
                        topPoints.Add(new List<Point>());

                        search2(y, x, 0, blobList.Count - 1);

                        for (; x < DEPTH_X; x++)
                        {
                            if (!obj[y, x])
                            {
                                break;
                            }
                        }
                    }
                }
            }
        }

        /*
         * 1,2,3
         * 0   4
         * 7,6,5
         * 
         * としたときの移動先対応ぐるぐるリストをつくる。
         * サイズは15
         * 
         */
        private static int[] roopX = { -1, -1, 0, 1, 1, 1, 0, -1, -1, -1, 0, 1, 1, 1, 0, -1 };
        private static int[] roopY = { 0, -1, -1, -1, 0, 1, 1, 1, 0, -1, -1, -1, 0, 1, 1, 1 };


        private static void search2(int y, int x, int roop, int num)
        {
            int i;
            int _x = 0;
            int _y = 0;

            blobList[num].Add(new Point(x, y));
            obj2[y, x] = true;


            while (!(_x == blobList[num][0].X && _y == blobList[num][0].Y))
            {
                bool alone = true;
                for (i = 0; i < 8; i++)
                {
                    _x = x + roopX[roop + i];
                    _y = y + roopY[roop + i];
                    if (obj[_y, _x])
                    {
                        int d1 = (int)depthMap[y, x];
                        int d2 = (int)depthMap[_y, _x];
                        if (d1 == 0 && d2 == 0)
                        {
                            d1++;
                        }
                        if (Math.Abs(d1 - d2) < 250)
                        {
                            alone = false;
                            break;
                        }
                        else
                        {
                            alone = true;
                            //つらい
                        }
                    }
                }
                if (alone)
                {
                    break;
                }
                blobList[num].Add(new Point(_x, _y));
                obj2[_y, _x] = true;

                if ((roop == 7 || roop == 0 || roop == 1) && (roop + i == 5 || roop + i == 6 || roop + i == 7))
                {
                    topPoints[num].Add(new Point(_x, _y));
                }

                roop = (roop + i + 4 + 1) % 8;
                y = _y;
                x = _x;
            }
        }

        public static int KinectDisp4(string date, ushort[] depth, Config config)
        {
            obj2 = new bool[DEPTH_Y, DEPTH_X];

            parse3d(depth);

            int x, y;
            for (x = 0; x < DEPTH_X; x++)
            {
                for (y = 0; y < DEPTH_Y; y++)
                {
                    int i = y * DEPTH_X + x;
                    int ii = (/*DEPTH_Y - 1 - */y) * DEPTH_X + x;


                    int _temp = depthMap[y, x];
                    _temp -= 1500;
                    if (_temp < 0) _temp = 0;
                    if (_temp > 4000) _temp = 4000;
                    byte color = (byte)(_temp * 255 / 4000);
                    if (obj[/*DEPTH_Y - 1 - */y, x])
                    {
                        RGBData temp = RGBData.getContrast(depth[ii]);
                        draw[i * 4 + 0] = temp.B;
                        draw[i * 4 + 1] = temp.G;
                        draw[i * 4 + 2] = temp.R;
                        //draw[i * 4 + 0] = 50;
                        //draw[i * 4 + 1] = 50;
                        //draw[i * 4 + 2] = 50;
                    }
                    else
                    {
                        int iii = (/*DEPTH_Y - 1 - */y) * DEPTH_X + x;
                        draw[i * 4 + 0] = color/*Pixel[iii * 4  + 0]*/;
                        draw[i * 4 + 1] = color/*Pixel[iii * 4  + 1]*/;
                        draw[i * 4 + 2] = color/*Pixel[iii * 4  + 2]*/;
                    }
                }
                //;if (!teemp)
                {
                    //Calibration(depthStream);
                    //teemp = true;
                }
            }

            MakeAsc(date, config.AtteiConfig.MakeAscDir2);


            SearchBlob(depth);

            integratedTopPoints = new List<List<Point>>();

            for (int i = 0; i < topPoints.Count; i++)
            {
                topPoints[i].Sort(new PointComparer());
                integratedTopPoints.Add(new List<Point>());

                for (int l = 0; l < topPoints[i].Count; l++)
                {
                    bool temp = true;
                    for (int k = 0; k < integratedTopPoints[i].Count; k++)
                    {
                        double dx = (integratedTopPoints[i][k].X - topPoints[i][l].X);
                        double dy = (integratedTopPoints[i][k].Y - topPoints[i][l].Y);
                        if (Math.Sqrt(dx * dx + dy * dy) < 50)
                        {
                            temp = false;
                            break;
                        }
                    }
                    if (temp)
                    {
                        integratedTopPoints[i].Add(topPoints[i][l]);
                    }
                }
            }

            int[] area = new int[blobList.Count];
            g = new Vector3D[blobList.Count];

            List<Vector3D> Join = new List<Vector3D>();

            //塊の質量っぽいの調べる。ついでにGに重心の位置を
            for (int i = 0; i < blobList.Count; i++)
            {
                blobList[i].Sort(new PointComparer());
                int ybak = 0;

                int _X = 0;
                int _Z = 0;
                for (int l = 0; l < blobList[i].Count; l++)
                {

                    int __X = (int)allPoint[(int)blobList[i][l].Y, (int)blobList[i][l].X].X;
                    int __Y = (int)allPoint[(int)blobList[i][l].Y, (int)blobList[i][l].X].Y;
                    int __Z = (int)allPoint[(int)blobList[i][l].Y, (int)blobList[i][l].X].Z;

                    g[i].X += __X;
                    g[i].Y += __Y;
                    g[i].Z += __Z;

                    if (blobList[i][l].Y == ybak)
                    {
                        int ds = (int)Math.Sqrt((_X - __X) * (_X - __X) + (_Z - __Z) * (_Z - __Z));
                        area[i] += ds;
                    }
                    else
                    {
                        ybak = (int)blobList[i][l].Y;
                        _X = (int)allPoint[(int)blobList[i][l].Y, (int)blobList[i][l].X].X;
                        _Z = (int)allPoint[(int)blobList[i][l].Y, (int)blobList[i][l].X].Z;
                    }
                }

                g[i].X /= blobList[i].Count;
                g[i].Y /= blobList[i].Count;
                g[i].Z /= blobList[i].Count;
            }

            List<List<Point>> headScan = new List<List<Point>>();

            for (int i = 0; i < blobList.Count; i++)
            {
                if (area[i] > 10000)
                {
                    bool notfound = true;
                    for (int l = 0; l < Join.Count; l++)
                    {
                        double xx = Join[l].X - g[i].X;
                        double zz = Join[l].Z - g[i].Z;
                        double dist = Math.Sqrt(xx * xx + zz * zz);
                        if (dist < 1400)
                        {
                            Vector3D temp = new Vector3D();
                            temp.X = (Join[l].X + g[i].X) / 2;
                            temp.Y = (Join[l].Y + g[i].Y) / 2;
                            temp.Z = (Join[l].Z + g[i].Z) / 2;
                            Join[l] = temp;

                            for (int k = 0; k < blobList[i].Count; k++)
                            {
                                headScan[l].Add(blobList[i][k]);
                            }
                            notfound = false;
                            break;
                        }
                    }
                    if (notfound)
                    {
                        Join.Add(g[i]);
                        headScan.Add(blobList[i]);
                    }
                }
            }

            for (int i = 0; i < headScan.Count; i++)
            {
                headScan[i].Sort(new PointComparer());
            }


            for (int i = 0; i < blobList.Count; i++)
            {
                if (area[i] > 15000)
                {
                    for (int l = 0; l < blobList[i].Count; l++)
                    {
                        int c = (int)(/*DEPTH_Y - 1 - */blobList[i][l].Y) * DEPTH_X + (int)blobList[i][l].X;
                        draw[c * 4 + 0] = 255;
                        draw[c * 4 + 1] = 255;
                        draw[c * 4 + 2] = 255;
                    }
                }
            }

            BMPDraw(Join, date, config.AtteiConfig.Template2, config.AtteiConfig.OutDir2);

            return Join.Count;
        }
    }
}
