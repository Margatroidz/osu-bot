using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace IPFinal
{
    /// <summary>
    /// MainWindow.xaml 的互動邏輯
    /// </summary>
    public partial class MainWindow : Window
    {
        //Model model;
        ScreenCapture sc;
        ScreenProcessor sp;
        DispatcherTimer timer = new DispatcherTimer();
        public MainWindow()
        {
            sp = new ScreenProcessor();
            sc = new ScreenCapture(sp);
            InitializeComponent();
            timer.Tick += new EventHandler(timer_Tick);
            timer.Interval = TimeSpan.FromTicks(17);
        }

        private void Start_Click(object sender, RoutedEventArgs e)
        {
            timer.Start();
            sp.SetForeground();
        }

        private void Stop_Click(object sender, RoutedEventArgs e)
        {
            timer.Stop();
        }

        private void timer_Tick(object sender, EventArgs e)
        {
            sc.SnapShoot();
            previewImage.Source = sp.Result;

        }

        private void PreviewRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            sp.SetResultSoruce(Convert.ToInt32(((RadioButton)sender).CommandParameter));
            sp.SetForeground();
        }
    }
}
