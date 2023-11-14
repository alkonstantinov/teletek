using HidSharp;
using lcommunicate;
using ljson;
using System;
using System.Collections.Generic;
using System.Security.Policy;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace ProstePrototype
{
    /// <summary>
    /// Interaction logic for ReadWindow.xaml
    /// </summary>
    public partial class ReadWindow : Window
    {
        public int selectedIndex = 3;
        public int prevIndex = 3;
        public string clickedName = "";

        public bool DarkMode { get; set; }
        public UserControl1 uc0 { get; set; } = new UserControl1();
        public UserControl2 uc1 { get; set; } = new UserControl2();
        public UserControl3 uc2 { get; set; } = new UserControl3();
        public UserControl uc3 { get; set; } = new UserControl();
        public BitmapImage imgsource = new BitmapImage(new Uri("pack://application:,,,/html/imports/images/01.IRIS.ico", UriKind.RelativeOrAbsolute)); //@" / Images/01.IRIS.ico", UriKind.RelativeOrAbsolute));
        private SolidColorBrush defaultColorBrush = (SolidColorBrush)App.Current.FindResource("DefaultColor");
        private SolidColorBrush grayColorBrush = (SolidColorBrush)App.Current.FindResource("GrayColor");
        public ReadWindow()
        {
            InitializeComponent();
            KeyDown += ReadWindow_KeyDown;
            this.DataContext = this;
            uc3.Resources = Application.Current.Resources;
            img3.Source = imgsource;
            ContentArea.Content = uc3;
            Button3.Focus();
            clickedName = Button3.Name;
            if (common.debugSettings.readLog)
            {
                Button3.Visibility = Visibility.Visible;
            }
        }
        public ReadWindow(int startidx, string topSign = "Read")
        {
            InitializeComponent();
            KeyDown += ReadWindow_KeyDown;
            this.DataContext = this;
            if (topSign != "Read")
            {
                txtBlk.Text = topSign;
            }
            string panelType = cJson.CurrentPanelFullType;
            if (panelType == "natron")
            {
                startidx = 2;
                Button0.Visibility = Visibility.Collapsed;
                Button1.Visibility = Visibility.Collapsed;
            } else  
            {
                Button2.Visibility = Visibility.Collapsed;
                if (panelType == "simpo")
                {
                    Button0.Visibility = Visibility.Collapsed;
                }
                Dictionary<string, HidDevice> devices = cComm.ScanHID();
                if (devices.Count == 0)
                {
                    if (startidx == 1 && panelType != "simpo")
                    {
                        startidx = 0;
                        Button1.Visibility = Visibility.Collapsed;
                    } else if (startidx == 1 && panelType == "simpo")
                    {                        
                        this.Close();
                    } else
                    {
                        Button1.Visibility = Visibility.Collapsed;
                    }
                }
            }

            if (common.debugSettings.readLog)
            {
                Button3.Visibility = Visibility.Visible;
            } else
            {
                Button3.Visibility = Visibility.Collapsed;
            }

            selectedIndex = startidx;
            if (startidx == 0)
            {
                uc0.Resources = Application.Current.Resources;
                tcp_icon.Foreground = defaultColorBrush;
                ContentArea.Content = uc0;
                Button0.Focus();
                clickedName = Button0.Name;                
            }
            else if (startidx == 1)
            {
                uc1.Resources = Application.Current.Resources;
                usb_icon.Foreground = defaultColorBrush;
                ContentArea.Content = uc1;
                Button1.Focus();
                clickedName = Button1.Name;
            }
            else if (startidx == 2)
            {
                uc2.Resources = Application.Current.Resources;
                rs232_icon.Foreground = defaultColorBrush;
                ContentArea.Content = uc2;
                Button2.Focus();
                clickedName = Button2.Name;
            }
            else if (startidx == 3)
            {
                uc3.Resources = Application.Current.Resources;
                img3.Source = imgsource;
                ContentArea.Content = uc3;
                Button3.Focus();
                clickedName = Button3.Name;
            }
        }
        private void ReadWindow_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                Exit_Clicked(sender, e );
                //Close();
            }
        }
        private void Exit_Clicked(object sender, RoutedEventArgs e)
        {
            this.Dispatcher.Invoke(() =>
            {
                this.DialogResult = false;
                this.Close();
            });
        }

        private void Connect_Clicked(object sender, RoutedEventArgs e)
        {
            this.Dispatcher.Invoke(() =>
            {
                this.DialogResult = true;
                this.Close();
            });
        }
        
        private void Button_LostFocus(object sender, RoutedEventArgs e)
        {
            string img = "img3";
            switch (prevIndex)
            {
                case 0: img = "tcp_icon"; break;
                case 1: img = "usb_icon"; break;
                case 2: img = "rs232_icon"; break;
                default: break;
            }
            var a = sender as Button;
            object im = a.FindName(img);
            if (im is Image)
            {
                // Following executed if Text element was found.
                Image image = im as Image;
                var b = imgsource as BitmapImage;
                image.Source = (ImageSource)(new FormatConvertedBitmap(b, PixelFormats.Gray8, null, 0));
            }
            else if (im is TextBlock)
            {
                TextBlock textBlock = im as TextBlock;
                textBlock.Foreground = grayColorBrush;
            }
            prevIndex = selectedIndex;
        }

        private void Button0_Click(object sender, RoutedEventArgs e)
        {
            ChangeSelectedIndex(0);
            Button_LostFocus(sender, e);
            uc0.Resources = Application.Current.Resources;
            tcp_icon.Foreground = defaultColorBrush;
            ContentArea.Content = uc0;
            clickedName = Button0.Name;
        }
        private void Button1_Click(object sender, RoutedEventArgs e)
        {
            ChangeSelectedIndex(1);
            Button_LostFocus(sender, e);
            uc1.Resources = Application.Current.Resources;
            usb_icon.Foreground = defaultColorBrush;
            ContentArea.Content = uc1;
            clickedName = Button1.Name;
        }
        private void Button2_Click(object sender, RoutedEventArgs e)
        {
            ChangeSelectedIndex(2);
            Button_LostFocus(sender, e);
            uc2.Resources = Application.Current.Resources;
            rs232_icon.Foreground = defaultColorBrush;
            ContentArea.Content = uc2;
            clickedName = Button2.Name;
        }
        
        private void Button3_Click(object sender, RoutedEventArgs e)
        {
            ChangeSelectedIndex(3);
            Button_LostFocus(sender, e);
            uc3.Resources = Application.Current.Resources;
            img3.Source = imgsource;
            ContentArea.Content = uc3;
            clickedName = Button3.Name;
        }

        private void ChangeSelectedIndex(short index)
        {
            selectedIndex = index;
            Properties.Settings.Default.ReadWindowStartIndex = index;
            Properties.Settings.Default.Save();
            //prevIndex = index;
        }

        #region MouseEnter-MouseLeave
        private void Button_MouseEnter(object sender, MouseEventArgs e)
        {
            var button = sender as Button;
            if (button != null)
            {
                string imageName = null;
                switch (button.Name)
                {
                    case "Button0":
                        imageName = "tcp_icon";
                        break;
                    case "Button1":
                        imageName = "usb_icon";
                        break;
                    case "Button2":
                        imageName = "rs232_icon";
                        break;
                }

                if (imageName != null)
                {
                    var image = button.FindName(imageName) as TextBlock;
                    if (image != null)
                    {
                        image.Foreground = defaultColorBrush;
                        button.Foreground = new SolidColorBrush(Color.FromRgb(0,0,0));
                    }
                }
            }
        }

        private void Button_MouseLeave(object sender, MouseEventArgs e)
        {
            var button = sender as Button;
            if (button != null)
            {
                string imageName = null;
                switch (button.Name)
                {
                    case "Button0":
                        imageName = "tcp_icon";
                        break;
                    case "Button1":
                        imageName = "usb_icon";
                        break;
                    case "Button2":
                        imageName = "rs232_icon";
                        break;
                }

                if (imageName != null)
                {
                    var image = button.FindName(imageName) as TextBlock;
                    if (image != null && button.Name != clickedName)
                    {
                        image.Foreground = grayColorBrush;
                        button.Foreground = grayColorBrush;
                    }
                }
            }
        }
        #endregion
    }
}
