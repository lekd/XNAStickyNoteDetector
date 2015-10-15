using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.IO;
using Emgu.CV.Structure;


public class Utilities
{

    public static double distanceBetween2Points(double x1, double y1, double x2, double y2)
    {
        return Math.Sqrt((x2 - x1) * (x2 - x1) + (y2 - y1) * (y2 - y1));
    }
    public static string getNameWithDateTime(string prefix, string extension)
    {
        return prefix + "_" + DateTime.Now.ToString("yyyyMMddHHmmssfff") + extension;
    }
    public static void saveCapturedImage(Bitmap img, string fileName)
    {
        string savedFileName =  fileName;
        if (File.Exists(savedFileName))
        {
            File.Delete(savedFileName);
        }
        img.Save(savedFileName, System.Drawing.Imaging.ImageFormat.Bmp);
    }
    public static Bitmap ResizeImage(Bitmap imgToResize, Size size)
    {
        try
        {
            Bitmap b = new Bitmap(size.Width, size.Height);
            using (Graphics g = Graphics.FromImage((Image)b))
            {
                g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                g.DrawImage(imgToResize, 0, 0, size.Width, size.Height);
                return b;
            }
            
        }
        catch {
            return imgToResize;
        }
    }
    static public PointF rotatePoint(PointF source, float angleInRads, PointF coordinateOrigin)
    {
        PointF temp = new PointF(source.X, source.Y);
        temp.X -= coordinateOrigin.X;
        temp.Y -= coordinateOrigin.Y;
        float newX = (float)(temp.X * Math.Cos(angleInRads) - temp.Y * Math.Sin(angleInRads));
        float newY = (float)(temp.X * Math.Sin(angleInRads) + temp.Y * Math.Cos(angleInRads));
        return new PointF(newX + coordinateOrigin.X, newY + coordinateOrigin.Y);
    }
    static public float DegreeToRad(float degree)
    {
        return (float)(degree * Math.PI / 180);
    }
    public static float RadToDegree(float rads)
    {
        return (float)(rads * 180 / Math.PI);
    }
    static public bool[][] EmguImage2Bool(Emgu.CV.Image<Gray, byte> img)
    {
        bool[][] booleanMap = new bool[img.Height][];
        for (int y = 0; y < img.Height; y++)
        {
            booleanMap[y] = new bool[img.Width];
            for (int x = 0; x < img.Width; x++)
            {
                booleanMap[y][x] = (img[x, y].Intensity < 0.3);
            }
        }
        return booleanMap;
    }
    static public Bitmap Bool2Bitmap(bool[][] boolMap)
    {
        Bitmap bmp = new Bitmap(boolMap[0].Length, boolMap.Length);
        using (Graphics g = Graphics.FromImage(bmp)) g.Clear(Color.White);
        for (int y = 0; y < bmp.Height; y++)
        {
            for (int x = 0; x < bmp.Width; x++)
            {
                if (boolMap[y][x])
                {
                    bmp.SetPixel(y, x, Color.Black);
                }
            }
        }
        return bmp;
    }
    static public byte[] ImageToBytes(System.Drawing.Image img)
    {
        byte[] bytes = null;
        using (MemoryStream stream = new MemoryStream())
        {
            img.Save(stream, System.Drawing.Imaging.ImageFormat.Png);
            stream.Close();

            bytes = stream.ToArray();
        }
        return bytes;
    }
    static public Stream ImageToStream(System.Drawing.Image img)
    {
        MemoryStream stream = new MemoryStream();
        img.Save(stream, System.Drawing.Imaging.ImageFormat.Png);
        stream.Seek(0,0);
        return stream;
    }
    static public Bitmap ByteArrayToBitmap(byte[] data)
    {
        MemoryStream mStream = new MemoryStream(data);
        return new Bitmap(mStream);
    }
    static public byte[] Int2Bytes(int val)
    {
        byte[] bytes = BitConverter.GetBytes(val);
        if (BitConverter.IsLittleEndian)
        {
            System.Array.Reverse(bytes);
        }
        return bytes;
    }
    static public byte[] Float2Bytes(float val)
    {
        byte[] bytes = BitConverter.GetBytes(val);
        if (BitConverter.IsLittleEndian)
        {
            System.Array.Reverse(bytes);
        }
        return bytes;
    }
}

