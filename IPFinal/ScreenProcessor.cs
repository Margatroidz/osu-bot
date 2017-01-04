using Emgu.CV;
using Emgu.CV.Structure;
using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Threading.Tasks;

namespace IPFinal
{
    public class ScreenProcessor
    {
        OutputSender outputSender;
        Image result;
        //輔助線，用來判斷相交
        LineSegment2D[] auxiliaryLine;
        int[] auxiliaryPointX;
        char[] key;
        List<int>[] edgeDistance;
        int resultImage = -1;
        int number = 0;
        public ScreenProcessor()
        {
            key = new char[] { 'D', 'F', 'J', 'K' };
            edgeDistance = new List<int>[] { new List<int>(), new List<int>(), new List<int>(), new List<int>() };
            outputSender = new OutputSender();
        }

        public void Process(Bitmap screen)
        {
            Image<Bgr, byte> inputImage = new Image<Bgr, byte>(screen);
            if (auxiliaryLine == null) DrawAuxiliaryLine(screen.Width, screen.Height);

            Image<Gray, byte> image = inputImage.Convert<Gray, byte>().Erode(2).Dilate(2).ThresholdBinary(new Gray(128), new Gray(255));
            LineSegment2D[][] segments = image.HoughLines(16, 255, 0.005, Math.PI / 2, 25, 50, 0);
            Image<Bgr, byte> result = new Image<Bgr, byte>(inputImage.ToBitmap());
            foreach (LineSegment2D[] lines in segments)
            {
                foreach (LineSegment2D line in lines)
                {
                    if (Math.Abs(line.P1.Y - line.P2.Y) < 3)
                    {
                        result.Draw(line, new Bgr(0, 0, 255), 3);
                        for (int i = 0; i < 4; i++)
                        {
                            if (CheckCross(line, i)) edgeDistance[i].Add(screen.Height - line.P1.Y);
                        }
                    }
                }
            }
            AnalysisOutput(image);

            inputImage.Save("C:/result/" + number++ + ".png");
            image.Save("C:/result/" + number++ + ".png");
            result.Save("C:/result/" + number++ + ".png");
            switch (resultImage)
            {
                case (0):
                    this.result = inputImage.ToBitmap();
                    //inputImage.Save("C:/result/" + number++ + ".png");
                    break;
                case (1):
                    this.result = image.ToBitmap();
                    //image.Save("C:/result/" + number++ + ".png");
                    break;
                case (2):
                    //result.Save("C:/result/" + number++ + ".png");
                    this.result = result.ToBitmap();
                    break;
            }
        }

        private void DrawAuxiliaryLine(int width, int height)
        {
            auxiliaryLine = new LineSegment2D[4];
            auxiliaryPointX = new int[4];
            for (int i = 0; i < 4; i++)
            {
                auxiliaryLine[i].P1 = new Point(width / 8 * (i * 2 + 1), 0);
                auxiliaryLine[i].P2 = new Point(width / 8 * (i * 2 + 1), height);
                auxiliaryPointX[i] = width / 8 * (i * 2 + 1);
            }
        }

        private void AnalysisOutput(Image<Gray, byte> image)
        {
            for (int i = 0; i < 4; i++)
            {
                edgeDistance[i].Sort();
                //判定點靠近下方
                if (edgeDistance[i].Count > 0 && edgeDistance[i][0] < 60)
                {
                    if (edgeDistance[i].Count > 1)
                    {
                        //判斷第二條線是否靠近
                        if (edgeDistance[i][1] - edgeDistance[i][0] < 25)
                        {
                            outputSender.SendKey(key[i]);
                        }
                    }
                    if (image[image.Height - edgeDistance[i][0] - 10, auxiliaryPointX[i]].Intensity < 64)
                    {
                        outputSender.SendKeyUp(i);
                    }
                    else
                    {
                        outputSender.SendKeyDown(i);
                    }
                }

                edgeDistance[i].Clear();
            }
            outputSender.Send();
        }

        private bool CheckCross(LineSegment2D line, int key)
        {
            if (line.P1.X < auxiliaryPointX[key] && line.P2.X > auxiliaryPointX[key] ||
                line.P1.X > auxiliaryPointX[key] && line.P2.X < auxiliaryPointX[key])
            {
                return true;
            }
            return false;
        }

        public void SetForeground()
        {
            outputSender.SetForeground();
        }

        public void SetResultSoruce(int resultSource)
        {
            resultImage = resultSource;
        }

        public ImageSource Result
        {
            get
            {
                if (result != null)
                {
                    using (MemoryStream memory = new MemoryStream())
                    {
                        result.Save(memory, ImageFormat.Png);
                        memory.Position = 0;
                        BitmapImage bitmapImage = new BitmapImage();
                        bitmapImage.BeginInit();
                        bitmapImage.StreamSource = memory;
                        bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                        bitmapImage.EndInit();
                        return bitmapImage;
                    }
                }return null;
            }
        }
    }
}
