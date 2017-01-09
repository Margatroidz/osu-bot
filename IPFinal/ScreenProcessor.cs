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
        int[] auxiliaryPointX;
        char[] key;
        List<int>[] edgeDistance;
        int resultImage = -1;
        //File f;

        public ScreenProcessor()
        {
            key = new char[] { 'D', 'F', 'J', 'K' };
            edgeDistance = new List<int>[] { new List<int>(), new List<int>(), new List<int>(), new List<int>() };
            outputSender = new OutputSender();

        }

        public void Process(Bitmap screen)
        {
            Image<Bgr, byte> inputImage = new Image<Bgr, byte>(screen);
            if (auxiliaryPointX == null) DrawAuxiliaryLine(screen.Width, screen.Height);
            Image<Gray, byte> image = inputImage.Convert<Gray, byte>().ThresholdBinary(new Gray(128), new Gray(255)).Erode(1).Dilate(2);

            AnalysisOutput(image);

            result = image.ToBitmap();
            //result.Save("D:/result/" + number++ + ".png");
            //inputImage.Save("C:/result/" + number++ + ".png");
            //image.Save("C:/result/" + number++ + ".png");
            //result.Save("C:/result/" + number++ + ".png");
        }

        private void DrawAuxiliaryLine(int width, int height)
        {
            auxiliaryPointX = new int[4];
            for (int i = 0; i < 4; i++) auxiliaryPointX[i] = width / 8 * (i * 2 + 1);
        }

        private void AnalysisOutput(Image<Gray, byte> image)
        {
            for (int i = 0; i < 4; i++)
            {
                //從底部往上 25 ~ 75 尋找白色部分
                //                ↑希望之後可以變成可動參數
                int objectHeight = 25;
                for (; objectHeight < 75; objectHeight++) if (image[image.Height - objectHeight, auxiliaryPointX[i]].Intensity > 64) break;
                if (objectHeight >= 75)
                {
                    if (outputSender.GetKeyDown(i)) outputSender.SendKeyUp(i);
                    continue;
                }

                //如果不是從25 ~ 75開始出現白色，先按下按鍵
                if (objectHeight > 25)
                {

                    int secondObjectHeight = 0;
                    int secondLimit = 30;
                    for (; secondObjectHeight <= secondLimit; secondObjectHeight++) if (image[image.Height - objectHeight - secondObjectHeight, auxiliaryPointX[i]].Intensity < 64) break;

                    if (secondObjectHeight <= 25 && secondObjectHeight >= 20) outputSender.SendKey(i);
                    else outputSender.SendKeyDown(i);
                }
                //如果從底下開始就是白色的，判定為常壓
                else if(outputSender.GetKeyDown(i))
                {
                    int whiteHeight = 0;
                    for (; whiteHeight < 75; whiteHeight++) if (image[image.Height - objectHeight - whiteHeight, auxiliaryPointX[i]].Intensity < 64) break;
                    if (whiteHeight < 75) outputSender.SendKeyUp(i);
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
