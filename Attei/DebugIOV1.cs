﻿using System.Collections.Generic;
using System.IO;
using System.Drawing;
using System.Windows.Media.Media3D;
using System;
using System.Drawing.Imaging;

namespace Attei.PCL
{
    public partial class V1
    {

        private static void MakeAsc(string dateTitle)
        {
            StreamWriter sw = new StreamWriter("C:\\Users\\Maquinista\\Desktop\\hisui-1_13_0_0-20110927\\bin\\V1\\" + dateTitle + "data.asc");
            for (int y = 0; y < DEPTH_Y; y++)
            {
                for (int x = 0; x < DEPTH_X; x++)
                {
                    sw.Write(allPoint[y, x].X.ToString() + ",");
                    sw.Write(allPoint[y, x].Y.ToString() + ",");
                    sw.Write(allPoint[y, x].Z.ToString() + "\r\n");
                }
            }

            sw.Close();
        }


        public static void BMPDraw(List<Vector3D> points, string date)
        {
            try
            {
                string tempPath = @"C:\template_V1.bmp";

                Bitmap bmp = new Bitmap(tempPath);

                using (var ms_to_byte = new MemoryStream())
                {
                    bmp.Save(ms_to_byte, ImageFormat.Bmp);
                    var output = ms_to_byte.GetBuffer();
                    bmp.Dispose();

                    for (int y = 0; y < DEPTH_Y; y++)
                    {
                        for (int x = 0; x < DEPTH_X; x++)
                        {
                            //モノクロ出力
                            int temp = depthMap[DEPTH_Y - 1 - y, DEPTH_X - 1 - x];
                            temp -= 1500;
                            if (temp < 0) temp = 0;
                            if (temp > 4000) temp = 4000;
                            byte color = (byte)(temp * 255 / 4000);
                            int i = (DEPTH_Y + y) * DEPTH_X + x;
                            output[i * 3 + 0 + 54] = color;
                            output[i * 3 + 1 + 54] = color;
                            output[i * 3 + 2 + 54] = color;

                        }
                    }

                    for (int i = 0; i < points.Count; i++)
                    {
                        int _x = 5000 - (int)points[i].Z;
                        int _y = 10000 - (6000 + (int)points[i].X);

                        if (_x < 0) _x = 0;
                        if (_x >= 12000) _x = 12000 - 1;
                        if (_y < 0) _y = 0;
                        if (_y >= 10000) _y = 10000 - 1;

                        _x = _x * DEPTH_X / 12000;
                        _y = _y * DEPTH_Y / 10000;

                        putPoint(_x, _y, ref output);
                    }

                    using (var ms_to_bmp = new MemoryStream(output))
                    {
                        var bmp2 = new Bitmap(ms_to_bmp);
                        bmp2.Save($"C:\\V1_png\\{date}.png", ImageFormat.Png);
                    }
                }
            }
            catch (Exception ex)
            {
            }
        }

        const int outX = 640;
        const int outY = 480;

        public static void putPoint(int X, int Y,ref byte[] output)
        {
            const int r=25;
            for (int x = -r; x <= r; x++)
            {
                for (int y = -r; y <= r; y++)
                {
                    int hx = x + X;
                    int hy = y + Y;

                    if (x * x + y * y < r * r)
                    {
                        if (0 <= hx && hx < outX && 0 <= hy && hy < outY)
                        {
                            int i = (hy) * outX + hx;
                            output[i * 3 + 0 + 54] = 0;
                            output[i * 3 + 1 + 54] = 0;
                            output[i * 3 + 2 + 54] = 255;
                        }
                    }
                }
            }
        }   //  putPoint
    }
}