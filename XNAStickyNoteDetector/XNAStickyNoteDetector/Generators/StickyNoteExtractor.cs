using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Emgu.CV;
using Emgu.CV.Structure;
using System.Drawing;

public class StickyNoteExtractor
{
    static object sync = new object();
    const int MARKER_SIZE = 40;
    const int NOTE_SIZE = 170;
    static Image<Gray, byte> FillAndThresholdImage(Image<Gray, byte> inputImg)
    {
        
        //the marker occludes the top left corner of the sticky note and can interfere the drawing extraction
        //so needs to be removed by being filled by white color
        inputImg.Draw(new System.Drawing.Rectangle(0, 0, MARKER_SIZE*3/4, MARKER_SIZE*3/4), new Gray(255), 0);
        double sumGray = inputImg.GetSum().Intensity;
        int marker_area = MARKER_SIZE * MARKER_SIZE * 9 / 16;
        double whiteRegion = 255 * marker_area;
        sumGray -= whiteRegion;
        double avgGray = sumGray / (inputImg.Width * inputImg.Height - marker_area);
        Image<Gray, Byte> subToMean = inputImg.Sub(new Gray(avgGray));
        double variance = 0;
        for (int x = 0; x < inputImg.Width; x++)
        {
            for(int y=0; y < inputImg.Height; y++)
            {
                if (x >= 0 && x < MARKER_SIZE * 3 / 4
                    && y >= 0 && y < MARKER_SIZE * 3 / 4)
                {
                    continue;
                }
                Point p = new Point(x, y);
                double dif = subToMean[p].Intensity;
                variance += dif * dif;
            }
        }
        double stdev = Math.Sqrt(variance / (inputImg.Width * inputImg.Height - marker_area));
        inputImg = inputImg.ThresholdBinary(new Gray(avgGray - 1.5*stdev), new Gray(255));
        
        return inputImg;
    }
    static Image<Gray,byte> ExtractStikyNoteRegion(Image<Gray, byte> inputImg,float markerPosX,float markerPosY,float markerOrientation)
    {
        Image<Gray, byte> toBeModified = inputImg;
        //inputImg.Draw(new CircleF(new PointF(markerPosX, markerPosY), 2), new Bgr(0, 0, 255), 2);
        //inputImg.Save("Raw.bmp");
        //CvInvoke.cvShowImage("Source", inputImg);
        //angle here in counter-clockwise direction
        float angleInDegree = Utilities.RadToDegree(markerOrientation);
        //toBeModified = toBeModified.Rotate(angleInDegree + 90, new Bgr(255, 255, 255));
        RotationMatrix2D<float> rotMat = new RotationMatrix2D<float>(new PointF(markerPosX, markerPosY), angleInDegree - 90, 1);
        toBeModified = toBeModified.WarpAffine<float>(rotMat, Emgu.CV.CvEnum.INTER.CV_INTER_CUBIC, Emgu.CV.CvEnum.WARP.CV_WARP_FILL_OUTLIERS, new Gray(255));
       /* PointF rotOrigin = new PointF(inputImg.Width / 2, inputImg.Height / 2);
        PointF rotatedPos = Utilities.rotatePoint(new PointF(markerPosX,markerPosY), 
                            markerOrientation + Utilities.DegreeToRad(90), rotOrigin);
        toBeModified.ROI = new Rectangle((int)rotatedPos.X - MARKER_SIZE / 4, (int)rotatedPos.Y - MARKER_SIZE / 4,
                            NOTE_SIZE - MARKER_SIZE / 2, NOTE_SIZE - MARKER_SIZE / 2);*/
        toBeModified.ROI = new Rectangle((int)markerPosX - MARKER_SIZE / 4, (int)markerPosY - MARKER_SIZE / 4,
                            NOTE_SIZE - MARKER_SIZE / 2, NOTE_SIZE - MARKER_SIZE / 2);
        return toBeModified.Copy();
    }
    public static Bitmap ProcessToExtract(Image<Gray, byte> inputImg, float markerPosX, float markerPosY, float markerOrientation)
    {
        lock (sync)
        {
            Image<Gray, byte> extractedNote = ExtractStikyNoteRegion(inputImg, markerPosX, markerPosY, markerOrientation);
            //CvInvoke.cvShowImage("ExtractedNote", extractedNote);
            Image<Gray, byte> thresholdedNote = FillAndThresholdImage(extractedNote);
            //thresholdedNote.Save("Thresholded_" + markerOrientation.ToString() + ".bmp");
            //thresholdedNote = thresholdedNote.SmoothMedian(3);
            //CvInvoke.cvShowImage("ThresholdedNote", thresholdedNote);
            //bool[][] boolMap = Utilities.EmguImage2Bool(thresholdedNote);
            //bool[][] thinnedBoolMap = ZhangZuenThinningExecuter.ZhangZuenThinning(boolMap);
            return thresholdedNote.Bitmap;
        }
    }
    public static Bitmap GetTheBestNoteExtraction(Bitmap bmp1, Bitmap bmp2)
    {
        lock (sync)
        {
            Image<Gray, byte> extraction_1 = new Image<Gray, byte>(bmp1);
            Image<Gray, byte> extraction_2 = new Image<Gray, byte>(bmp2);
            Image<Gray, byte> dif = extraction_1.AbsDiff(extraction_2);
            extraction_1 = extraction_1.Not();
            Gray sum_1 = extraction_1.GetSum();
            extraction_2 = extraction_2.Not();
            Gray sum_2 = extraction_2.GetSum();
            Gray sum_dif = dif.GetSum();
            Bitmap big = sum_1.Intensity >= sum_2.Intensity ? bmp1 : bmp2;
            Bitmap small = sum_1.Intensity < sum_2.Intensity ? bmp1 : bmp2;
            Gray bigSum = sum_1.Intensity >= sum_2.Intensity ? sum_1 : sum_2;
            Gray smallSum = sum_1.Intensity < sum_2.Intensity ? sum_1 : sum_2;
            if (smallSum.Intensity == 0)
            {
                return big;
            }
            else
            {
                if (sum_dif.Intensity / smallSum.Intensity > 0.3)
                {
                    return small;
                }
                else
                {
                    return big;
                }
            }
        }
    }
        
}