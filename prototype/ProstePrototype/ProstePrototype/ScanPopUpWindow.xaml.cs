using System;
using System.Collections.Generic;
using System.Security.Policy;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace ProstePrototype
{
    /// <summary>
    /// Interaction logic for ScanPopUpWindow.xaml
    /// </summary>
    public partial class ScanPopUpWindow : Window
    {
        public bool _functionFinished  = false;
        public ScanPopUpWindow()
        {
            InitializeComponent();

            //MediaElement1.Source = new Uri(@"C:\Users\vbb12\GitHub\Teletek\teletek\prototype\ProstePrototype\ProstePrototype\Images\barcode-scan.gif", UriKind.RelativeOrAbsolute);
            //MediaElement1.Position = TimeSpan.Zero;
            //MediaElement1.Play();
            this.Left = (SystemParameters.WorkArea.Width - this.Width) / 2;
            this.Top = (SystemParameters.WorkArea.Height / 2) - (this.Height * 1.6);
            string gifPath = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(System.AppDomain.CurrentDomain.BaseDirectory), "Images\\barcode-scan.gif");
            string html = $"<html><body style='overflow: hidden; display:grid; place-items: center; margin: 0; padding: 0;'><img src='{gifPath}' width='100%' height='100%' style='margin: auto;'/></body></html>";
            scanWb.NavigateToString(html);
            DispatcherTimer timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromMilliseconds(100);
            timer.Tick += Timer_Tick;
            timer.Start();
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            if (_functionFinished)
            {
                // stop the timer and close the window
                DispatcherTimer timer = (DispatcherTimer)sender;
                timer.Stop();
                Close();
            }
        }

        //private void MediaElement1_MediaEnded(object sender, RoutedEventArgs e)
        //{
        //    MediaElement1.Position = TimeSpan.Zero;
        //    MediaElement1.Play();
        //    //MediaElement me1 = sender as MediaElement;
        //    //me1.Position = TimeSpan.Zero;
        //    //me1.Play();
        //}

    }
}
