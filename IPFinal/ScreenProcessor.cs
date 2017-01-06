using Emgu.CV;
using Emgu.CV.Structure;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Media;
using System.Windows.Media.Imaging;

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
            Image<Gray, byte> image = inputImage.Convert<Gray, byte>().Erode(2).Dilate(2).ThresholdBinary(new Gray(128), new Gray(255)).Dilate(2).Erode(2);
            AnalysisOutput(image);

            //result = image.ToBitmap();
            //result.Save("D:/result/" + number++ + ".png");
            //inputImage.Save("C:/result/" + number++ + ".png");
            //image.Save("C:/result/" + number++ + ".png");
            //result.Save("C:/result/" + number++ + ".png");
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
                //從底部往上 10 ~ 115 尋找白色部分
                int objectHeight = 10;
                for (; objectHeight < 115; objectHeight++) if (image[image.Height - objectHeight, auxiliaryPointX[i]].Intensity > 64) break;
                if (objectHeight >= 115)
                {
                    if (outputSender.GetKeyDown(i)) outputSender.SendKeyUp(i);
                    continue;
                }

                //如果不是從10 ~ 115開始出現白色，先按下按鍵
                if (objectHeight > 10)
                {

                    int secondObjectHeight = 0;
                    int secondLimit = 90 - objectHeight;
                    for (; secondObjectHeight <= secondLimit; secondObjectHeight++) if (image[image.Height - objectHeight - secondObjectHeight, auxiliaryPointX[i]].Intensity < 64) break;

                    //click按鍵的需要的高度比較低
                    if (secondObjectHeight <= 25 && objectHeight <= 75)
                    {
                        outputSender.SendKey(i);
                        //if (isKeyDown) 
                        //else outputSender.SendKeyUp(i);
                    }
                    //長壓得提早按，原因我也不知道
                    else if (secondObjectHeight > 25) outputSender.SendKeyDown(i);
                }
                else
                {
                    int whiteHeight = 0;
                    for (; whiteHeight < 65; whiteHeight++) if (image[image.Height - objectHeight - whiteHeight, auxiliaryPointX[i]].Intensity < 64) break;
                    if (whiteHeight < 65) outputSender.SendKeyUp(i);
                }
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
                }
                return null;
            }
        }
    }
}
