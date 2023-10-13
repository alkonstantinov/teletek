using System;
using System.Collections.Generic;
using System.Security.Policy;
using System.Text;
using System.Threading;
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
        private string gifPath = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(System.AppDomain.CurrentDomain.BaseDirectory), "Images\\barcode-scan.gif");
        public ScanPopUpWindow()
        {
            InitializeComponent();
            this.Left = (SystemParameters.WorkArea.Width - this.Width) / 2;
            this.Top = (SystemParameters.WorkArea.Height / 2) - (this.Height * 1.6);           
            //string html = $"<html><body style='overflow: hidden; display:grid; place-items: center; margin: 0; padding: 0;'><img src='{gifPath}' width='100%' height='100%' style='margin: auto;'/></body></html>";
            //scanWb.NavigateToString(html);
            InitializeGifPlayer();
            DispatcherTimer timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromMilliseconds(100);
            timer.Tick += Timer_Tick;
            timer.Start();
        }

        // Method to initialize the MediaElement and start playing the GIF
        private void InitializeGifPlayer()
        {
            gifPlayer.Source = new Uri(gifPath, UriKind.RelativeOrAbsolute);
            gifPlayer.MediaEnded += GifPlayer_MediaEnded;
        }


        private void Timer_Tick(object sender, EventArgs e)
        {
            if (_functionFinished)
            {
                // stop the timer and close the window
                DispatcherTimer timer = (DispatcherTimer)sender;
                timer.Stop();
                CloseWindow();
            }
        }

        private void CloseWindow()
        {
            // Dispose of the gifPlayer if it exists
            if (gifPlayer != null)
            {
                gifPlayer.Stop();
                gifPlayer.Close();
                gifPlayer.MediaEnded -= GifPlayer_MediaEnded;
                gifPlayer.Source = null;
            }

            // Close the window
            this.Close();
        }

        private void GifPlayer_MediaEnded(object sender, RoutedEventArgs e)
        {
            gifPlayer.Position = new TimeSpan(0, 0, 1);
            gifPlayer.Play();
        }

    }
}
