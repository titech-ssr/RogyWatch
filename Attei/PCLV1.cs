using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Threading;
using RogyWatchCommon;
using System.Windows.Media.Media3D;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Attei.PCL
{
    public partial class V1
    {
        static readonly int DEPTH_X, DEPTH_Y;

        static DirectionalLight DirLight1;
        static PerspectiveCamera Camera1;

        static GeometryModel3D[] points;
        static Model3DGroup modelGroup;

        private static byte[] draw;
        private static short[] depthPixel;
        private static bool[,] obj, obj2;

        private static Vector3D[,] allPoint;

        private static Int32Rect _ScreenImageRect;
        private static WriteableBitmap _ScreenImage;

        private static Vector3D[] g;

        private static List<List<Point>> blobList;
        private static List<List<Point>> topPoints;
        private static List<List<Point>> integratedTopPoints;

        private static short[,] depthMap;
        
        static V1()
        {
            var conf = new PrimitiveDriverV1();
            DEPTH_X = conf.DEPTH_X;
            DEPTH_Y = conf.DEPTH_Y;
            init();
        }

        public static void init()
        {
            draw = new byte[DEPTH_X * DEPTH_Y * 4];
            depthPixel = new short[DEPTH_X * DEPTH_Y];
            obj = new bool[DEPTH_Y, DEPTH_X];

            _ScreenImage = new WriteableBitmap(DEPTH_X, DEPTH_Y, 96, 96, PixelFormats.Bgr32, null);
            _ScreenImageRect = new Int32Rect(0, 0, DEPTH_X, DEPTH_Y);

            allPoint = new Vector3D[DEPTH_Y, DEPTH_X];

            depthMap = new short[DEPTH_Y, DEPTH_X];
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

        private static void spinPoint(ref int x,ref int y,ref int z,double theta,string axis)
        {
            theta = theta * Math.PI * 2 / 360;
            int _x = x;
            int _y = y;
            int _z = z;

            if (axis == "X")
            {
                _y = (int)(y * Math.Cos(theta) + z * Math.Sin(-theta));
                _z = (int)(y * Math.Sin(theta) + z * Math.Cos(theta));
            }
            else if (axis == "Y")
            {
                _z = (int)(z * Math.Cos(theta) + x * Math.Sin(-theta));
                _x = (int)(z * Math.Sin(theta) + x * Math.Cos(theta));
            }
            else if (axis == "Z")
            {
                _x = (int)(x * Math.Cos(theta) + y * Math.Sin(-theta));
                _y = (int)(x * Math.Sin(theta) + y * Math.Cos(theta));
            }
            x = _x;
            y = _y;
            z = _z;
        }

        public static void parse3d(short[] depth)
        {
            const int div_x = 8;
            const int div_y = 5;
            const int div_z = 3000;
            const int div = 1000;

            
            obj = new bool[DEPTH_Y, DEPTH_X];
            allPoint = new Vector3D[DEPTH_Y, DEPTH_X];
            //allPoint_high = new Vector3D[DEPTH_Y, DEPTH_X];
            

            for (int y = 0; y < DEPTH_Y; y++)
            {
                for (int x = 0; x < DEPTH_X; x++)
                {
                    depthMap[y, x] = depth[y * DEPTH_X + x];


                    if (depth[y * DEPTH_X + x] == 0)
                    {
                        allPoint[y, x].X = 0;
                        allPoint[y, x].Y = 0;
                        allPoint[y, x].Z = 10;
                    }
                    else
                    {
                        int X = (320 - x) * depth[y * DEPTH_X + x] * div_x / div;
                        int Y = (y - 240) * depth[y * DEPTH_X + x] * div_y / div;
                        int Z = (int)(depth[y * DEPTH_X + x]) * div_z / div;

                        //いい感じに座標変換。y=0が地面の高さになる感じ
                        spinPoint(ref X, ref Y, ref Z, 155, "X");
                        Y += 6000;
                        Z += 10400;
                        spinPoint(ref X, ref Y, ref Z, 3, "Y");

                        //ドアより向こうを消す
                        if ((X > 0 && Z < 0) || (X <= 0 && Z < -750))
                        {
                            allPoint[y, x].X = 0;
                            allPoint[y, x].Y = 0;
                            allPoint[y, x].Z = 10;
                            continue;
                        }

                        //下駄箱を消す
                        if (Z < 3500 && X < -1800)
                        {
                            allPoint[y, x].X = 0;
                            allPoint[y, x].Y = 0;
                            allPoint[y, x].Z = 10;
                            continue;
                        }
                        //下駄箱付近に荷物増えると誤検出するから消す
                        if (Z < 3000 && X < -1500 && Y > 2000)
                        {
                            allPoint[y, x].X = 0;
                            allPoint[y, x].Y = 0;
                            allPoint[y, x].Z = 10;
                            continue;
                        }

                        spinPoint(ref X, ref Y, ref Z, -3, "Y");
                        //HAKUとかボール盤消す
                        if (X < -3600)
                        {
                            allPoint[y, x].X = 0;
                            allPoint[y, x].Y = 0;
                            allPoint[y, x].Z = 10;
                            continue;
                        }

                        //旋盤の机消す
                        if (X > 4200)
                        {
                            allPoint[y, x].X = 0;
                            allPoint[y, x].Y = 0;
                            allPoint[y, x].Z = 10;
                            continue;
                        }

                        //生活領域を消す
                        if (Z > 8000)
                        {
                            allPoint[y, x].X = 0;
                            allPoint[y, x].Y = 0;
                            allPoint[y, x].Z = 10;
                            continue;
                        }

                        //入口付近の両端を消す
                        if (Z > 6000 && (X > 1500 || X < -2000))
                        {
                            allPoint[y, x].X = 0;
                            allPoint[y, x].Y = 0;
                            allPoint[y, x].Z = 10;
                            continue;
                        }

                        //フライスを消す
                        if (Z > 4500 && X > 3000)
                        {
                            allPoint[y, x].X = 0;
                            allPoint[y, x].Y = 0;
                            allPoint[y, x].Z = 10;
                            continue;
                        }
                        //抽出
                        if (Y < 2600)
                        {
                            allPoint[y, x].X = 0;
                            allPoint[y, x].Y = 0;
                            allPoint[y, x].Z = 10;
                            continue;
                        }

                        Z -= 3000;
                        X = (int)(X * 0.9);
                        X -= 1000;


                        allPoint[y, x].X = X;
                        allPoint[y, x].Y = Y;
                        allPoint[y, x].Z = Z;
                        obj[y, x] = true;
                    }
                }
            }
        }

        private void PCLinit()
        {
            DirLight1 = 
            new DirectionalLight();
            DirLight1.Color = Colors.White;
            DirLight1.Direction = 
            new Vector3D(1, 1, 1);

            Camera1 =
                 new PerspectiveCamera();
            Camera1.FarPlaneDistance = 8000;
            Camera1.NearPlaneDistance = 100;
            Camera1.FieldOfView = 10;
            Camera1.Position =
                        new Point3D(160, 120, -1000);
            Camera1.LookDirection =
                        new Vector3D(0, 0, 1);
            Camera1.UpDirection =
                        new Vector3D(0, -1, 0);

            int s = 4;

            modelGroup =
                      new Model3DGroup();
            int i = 0;
            for (int y = 0; y < DEPTH_Y; y += s)
            {
                for (int x = 0; x < DEPTH_X; x += s)
                {
                    points[i] = Triangle(x, y, s);
                    points[i].Transform =
                      new TranslateTransform3D(0, 0, 0);
                    modelGroup.Children.Add(points[i]);
                    i++;
                }
            }
            modelGroup.Children.Add(DirLight1);
        }


        private GeometryModel3D Triangle(double x, double y, double s)
        {
            Point3DCollection corners =
                            new Point3DCollection();
            corners.Add(new Point3D(x, y, 0));
            corners.Add(new Point3D(x, y + s, 0));
            corners.Add(new Point3D(x + s, y + s, 0));
            Int32Collection Triangles =
                             new Int32Collection();
            Triangles.Add(0);
            Triangles.Add(1);
            Triangles.Add(2);

            MeshGeometry3D tmesh =
                    new MeshGeometry3D();
            tmesh.Positions = corners;
            tmesh.TriangleIndices = Triangles;

            tmesh.Normals.Add(new Vector3D(0, 0, -1));

            GeometryModel3D msheet =
               new GeometryModel3D();
            msheet.Geometry = tmesh;
            msheet.Material = new DiffuseMaterial(
                   new SolidColorBrush(Colors.Red));
            return msheet;
        }




        private static void SearchBlob(short[] depth)
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

                        search2(y, x, 0, blobList.Count-1);

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
        private static int[] roopX = { -1, -1,  0,  1,  1,  1,  0, -1, -1, -1,  0,  1,  1,  1,  0, -1 };
        private static int[] roopY = { 0 , -1, -1, -1,  0,  1,  1,  1,  0, -1, -1, -1,  0,  1,  1,  1 };


        private void search(int y, int x,short d,int num, bool[,] obj)
        {
            obj[y, x] = false;
            //if (Math.Abs(d - depthMap[y, x]) < 100)
            {
                blobList[num].Add(new Point(x, y));
                if (obj[y + 1, x]) search(y + 1, x, depthMap[y, x], num, obj);
                if (obj[y - 1, x]) search(y - 1, x, depthMap[y, x], num, obj);
                if (obj[y, x + 1]) search(y, x + 1, depthMap[y, x], num, obj);
                if (obj[y, x - 1]) search(y, x - 1, depthMap[y, x], num, obj);
            }
            ///else
            {
               // blobList[num].Add(new Vector3D(x, y, depthMap[y, x]));
            }
        }


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
                        short d1 = depthMap[y, x];
                        short d2 = depthMap[_y, _x];
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
        } //    end of search2

        public static int KinectDisp4(string date, short[] depth, Config config)
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
                        RGBData temp = RGBData.getContrast(depthPixel[ii]);
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

            MakeAsc(date, config.AtteiConfig.MakeAscDir1);


            SearchBlob(depthPixel);

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

            BMPDraw(Join, date, config.AtteiConfig.Template1, config.AtteiConfig.OutDir1);

            return Join.Count;
        }
    }
}