using common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Policy;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
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
        private MainWindow _mainWindow;
        public bool _functionFinished  = false;
        //private string gifPath = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(System.AppDomain.CurrentDomain.BaseDirectory), "Images\\barcode-scan.gif");
        public ScanPopUpWindow(MainWindow mainWindow)
        {
            InitializeComponent();

            _mainWindow = mainWindow;
            Point maxPrimary = new Point(SystemParameters.MaximizedPrimaryScreenWidth, SystemParameters.MaximizedPrimaryScreenHeight);
            Point mainWindowTopLeft = new Point(_mainWindow.Left , _mainWindow.Top);

            double add = _mainWindow.WindowState == WindowState.Maximized ? 30 : 35;

            if (mainWindowTopLeft.X >= 0 && mainWindowTopLeft.X <= maxPrimary.X && mainWindowTopLeft.Y >= 0 && mainWindowTopLeft.Y <= maxPrimary.Y)
            {
                // we are on primary screen
                if (_mainWindow.WindowState == WindowState.Maximized)
                {
                    this.Left = SystemParameters.FullPrimaryScreenWidth - this.Width - 2;
                    this.Top = add - this.Height / 2;
                } else
                {
                    this.Left = _mainWindow.RestoreBounds.Right - this.Width - 7;
                    this.Top = _mainWindow.RestoreBounds.Top + add - this.Height / 2;
                }
            } else
            {
                // we are on secondary screen
                if (_mainWindow.WindowState == WindowState.Maximized)
                {                    
                    if (SystemParameters.VirtualScreenLeft == 0)
                    {
                        this.Left = SystemParameters.VirtualScreenWidth - this.Width - 2;
                        this.Top = (
                            SystemParameters.VirtualScreenTop < 0 ? 
                            SystemParameters.VirtualScreenTop : 
                            SystemParameters.VirtualScreenHeight - (_mainWindow.RenderSize.Height + 33.6)
                            ) + add - this.Height / 2;
                    } else
                    {
                        this.Left = SystemParameters.VirtualScreenLeft + _mainWindow.RenderSize.Width - 14.4 - this.Width - 2;
                        this.Top = (
                            SystemParameters.VirtualScreenTop < 0 ?
                            SystemParameters.VirtualScreenTop :
                            SystemParameters.VirtualScreenHeight - (_mainWindow.RenderSize.Height + 33.6)
                            ) + add - this.Height / 2;
                    }
                }
                else
                {
                    this.Left = _mainWindow.RestoreBounds.Right - this.Width - 7;
                    this.Top = _mainWindow.RestoreBounds.Top + add - this.Height / 2;
                }
            }
            // InitializeGifPlayer();

            DispatcherTimer timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromMilliseconds(100);
            timer.Tick += Timer_Tick;
            timer.Start();

            // Hook up the Deactivated event
            this.Deactivated += PopupWindow_Deactivated;
            this.Activated += ScanPopUpWindow_Activated;
        }

        private void ScanPopUpWindow_Activated(object sender, EventArgs e)
        {
            // When the window regains focus, set TopMost back to true
            this.Topmost = true;
        }

        private void PopupWindow_Deactivated(object sender, EventArgs e)
        {
            // When the window loses focus, set TopMost to false
            this.Topmost = false;
        }

        public void UpdateProgress(eRWOperation op, string node, string index, int counter, int cntall)
        {
            // Use the op, node, index, counter, and cntall parameters to update your progress bar and show text as needed.
            Application.Current.Dispatcher.Invoke(() =>
            {
                // Update a readOrWriteProgress bar
                readOrWriteProgress.Value = (double)counter * 100.0 / (double)cntall;

                //if (common.debugSettings.showReadWriteProgressSigns)
                //{
                    // Show text in statusTextBlock
                    statusTextBlock.Text = $"{Utils.LimitCharacters(node, 36)}" + (String.IsNullOrEmpty(index) ? "" : $" {index}");
                //}
            });
        }

        // Method to initialize the MediaElement and start playing the GIF
        private void InitializeGifPlayer()
        {
            //gifPlayer.Source = new Uri(gifPath, UriKind.RelativeOrAbsolute);
            //gifPlayer.MediaEnded += GifPlayer_MediaEnded;
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
            //if (gifPlayer != null)
            //{
            //    gifPlayer.Stop();
            //    gifPlayer.Close();
            //    gifPlayer.MediaEnded -= GifPlayer_MediaEnded;
            //    gifPlayer.Source = null;
            //}

            // Close the window
            this.Close();
        }

        private void GifPlayer_MediaEnded(object sender, RoutedEventArgs e)
        {
            //gifPlayer.Position = new TimeSpan(0, 0, 1);
            //gifPlayer.Play();
        }

    }
}
