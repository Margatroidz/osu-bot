using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace IPFinal
{
    class OutputSender
    {
        [DllImport("user32.DLL", CharSet = CharSet.Unicode)]
        public static extern IntPtr FindWindow(string lpClassName, string lpWindowName);
        [DllImport("user32.DLL")]
        public static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        public static extern void keybd_event(byte bVk, byte bScan, int dwFlags, IntPtr dwExtraInfo);

        const byte KEYEVENTF_EXTENDEDKEY = 0x01;
        const byte KEYEVENTF_KEYUP = 0x02;

        IntPtr game;
        byte[] keyCode;
        Queue<int> keyDownQueue;
        Queue<int> keyUpQueue;
        Queue<char> keyQueue;

        public OutputSender()
        {
            //Process _game = Process.GetProcessesByName("osu!").Single();
            //game = _game.Handle;
            game = FindWindow(null, "osu!");

            keyCode = new byte[] { Convert.ToByte('D'), Convert.ToByte('F'), Convert.ToByte('J'), Convert.ToByte('K') };
            keyDownQueue = new Queue<int>();
            keyUpQueue = new Queue<int>();
            keyQueue = new Queue<char>();

            //Task.Factory.StartNew(() =>
            //{
            //    while (true)
            //    {
            //        if (keyDownQueue.Count > 0) keybd_event(keyCode[keyDownQueue.Dequeue()], 0, KEYEVENTF_EXTENDEDKEY, (IntPtr)0);
            //        if (keyUpQueue.Count > 0) keybd_event(keyCode[keyUpQueue.Dequeue()], 0, KEYEVENTF_KEYUP, (IntPtr)0);
            //    }
            //});
        }

        public void SetForeground()
        {
            SetForegroundWindow(game);
        }

        public void SendKeyUp(int keyNumber)
        {
            //keyUpQueue.Enqueue(keyNumber);
            keybd_event(keyCode[keyNumber], 0, KEYEVENTF_KEYUP, (IntPtr)0);
        }

        public void SendKeyDown(int keyNumber)
        {
            //keyDownQueue.Enqueue(keyNumber);
            keybd_event(keyCode[keyNumber], 0, KEYEVENTF_EXTENDEDKEY, (IntPtr)0);
        }

        public void SendKey(char key)
        {
            keyQueue.Enqueue(key);
            //SendKeys.SendWait(key);
            //SendKeys.Flush();
        }

        public void Send()
        {
            if (keyQueue.Count > 0)
            {
                string msg = string.Empty;
                while (keyQueue.Count > 0)
                {
                    if (!msg.Contains<char>(keyQueue.Peek())) msg += keyQueue.Dequeue();
                    else keyQueue.Dequeue();
                }
                SendKeys.SendWait(msg);
                SendKeys.Flush();
            };
        }
    }
}
