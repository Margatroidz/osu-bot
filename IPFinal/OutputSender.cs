using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

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
        bool[] isKeyDown;
        List<int> keyBuffer;
        Queue<int[]> recoverKeyBuffer;

        public OutputSender()
        {
            //Process _game = Process.GetProcessesByName("osu!").Single();
            //game = _game.Handle;
            game = FindWindow(null, "osu!");
            keyCode = new byte[] { Convert.ToByte('D'), Convert.ToByte('F'), Convert.ToByte('J'), Convert.ToByte('K') };
            isKeyDown = new bool[] { false, false, false, false };
            keyBuffer = new List<int>();
            recoverKeyBuffer = new Queue<int[]>();

            Task.Run(async () =>
            {
                while (true)
                {
                    if (recoverKeyBuffer.Count > 0)
                    {
                        int[] keys = recoverKeyBuffer.Dequeue();
                        await Task.Delay(20);
                        foreach (int key in keys) keybd_event(keyCode[key], 0, KEYEVENTF_KEYUP, (IntPtr)0);
                    }
                }
            });
        }

        public void SetForeground()
        {
            SetForegroundWindow(game);
        }

        public void SendKeyUp(int keyNumber)
        {
            //keyUpQueue.Enqueue(keyNumber);
            if (isKeyDown[keyNumber])
            {
                keybd_event(keyCode[keyNumber], 0, KEYEVENTF_KEYUP, (IntPtr)0);
                isKeyDown[keyNumber] = false;
            }
        }

        public void SendKeyDown(int keyNumber)
        {
            //keyDownQueue.Enqueue(keyNumber);
            if (!isKeyDown[keyNumber])
            {
                keybd_event(keyCode[keyNumber], 0, KEYEVENTF_EXTENDEDKEY, (IntPtr)0);
                isKeyDown[keyNumber] = true;
            }
        }

        public void SendKey(int keyNumber)
        {
            keyBuffer.Add(keyNumber);
            //Task.Factory.StartNew(async () =>
            //{
            //    await Task.Delay(20);
            //    keybd_event(keyCode[keyNumber], 0, KEYEVENTF_KEYUP, (IntPtr)0);
            //});
        }

        public void Send()
        {
            if (keyBuffer.Count > 0)
            {
                foreach (int key in keyBuffer)
                {
                    keybd_event(keyCode[key], 0, KEYEVENTF_EXTENDEDKEY, (IntPtr)0);
                }
                recoverKeyBuffer.Enqueue(keyBuffer.ToArray());
                keyBuffer.Clear();
            }
        }

        public bool GetKeyDown(int keyNumber)
        {
            return isKeyDown[keyNumber];
        }
    }
}
