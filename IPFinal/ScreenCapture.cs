using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace IPFinal
{
    public class ScreenCapture
    {
        public struct RECT
        {
            public int left;
            public int top;
            public int right;
            public int bottom;
        }

        [DllImport("user32.DLL", CharSet = CharSet.Unicode)]
        public static extern IntPtr FindWindow(string lpClassName, string lpWindowName);
        [DllImport("user32.dll")]
        public static extern IntPtr GetWindowRect(IntPtr hWnd, ref RECT rect);

        IntPtr game;
        Bitmap screen;
        int width;
        int heigh;
        ScreenProcessor screenProcessor;

        public ScreenCapture(ScreenProcessor sp)
        {
            game = FindWindow(null, "osu!");
            screenProcessor = sp;
        }

        public void SnapShoot()
        {
            if (game != (IntPtr)0)
            {
                screen = ScreenShoot(game);
                screenProcessor.Process(screen);
            }
        }

        private Bitmap ScreenShoot(IntPtr hWnd)
        {
            //new Rectangle((int)(inputImage.Width / 6.28), inputImage.Height / 4, inputImage.Width / 7, (int)(inputImage.Height / 1.75))
            RECT rect = new RECT();
            GetWindowRect(hWnd, ref rect);
            int rectWidth = rect.right - rect.left;
            int rectHeight = rect.bottom - rect.top;
            width = rectWidth / 7;
            heigh = (int)(rectHeight / 1.75);
            Bitmap result = new Bitmap(width, heigh);
            Graphics g = Graphics.FromImage(result);
            g.CopyFromScreen(new Point((int)(rectWidth / 6.28), rectHeight / 4), new Point(0, 0), new Size(width, heigh));
            IntPtr dc1 = g.GetHdc();
            g.ReleaseHdc(dc1);
            return result;
        }

        public ImageSource Screen
        {
            get
            {
                using (var memory = new MemoryStream())
                {
                    screen.Save(memory, ImageFormat.Png);
                    memory.Position = 0;
                    BitmapImage bitmapImage = new BitmapImage();
                    bitmapImage.BeginInit();
                    bitmapImage.StreamSource = memory;
                    bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                    bitmapImage.EndInit();
                    return bitmapImage;
                }
            }
        }
    }
}