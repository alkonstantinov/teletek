using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace ProstePrototype
{
    /// <summary>
    /// Interaction logic for ReadWindow.xaml
    /// </summary>
    public partial class ReadWindow : Window
    {
        public int selectedIndex = 3;
        public int prevIndex = 3;

        public bool DarkMode { get; set; }
        public UserControl1 uc0 { get; set; }
        public UserControl2 uc1 { get; set; }
        public UserControl3 uc2 { get; set; }
        public UserControl uc3 { get; set; }
        public BitmapImage imgsource = new BitmapImage(new Uri(@"/Images/01.IRIS.ico", UriKind.RelativeOrAbsolute));
        public ReadWindow()
        {
            InitializeComponent();
            KeyDown += ReadWindow_KeyDown;
            this.DataContext = this;
            uc0 = new UserControl1();
            uc1 = new UserControl2();
            uc2 = new UserControl3();
            uc3 = new UserControl();
            uc3.Resources = Application.Current.Resources;
            img3.Source = imgsource;
            ContentArea.Content = uc3;
            Button3.Focus();
        }
        public ReadWindow(int startidx)
        {
            InitializeComponent();
            KeyDown += ReadWindow_KeyDown;
            this.DataContext = this;
            uc0 = new UserControl1();
            uc1 = new UserControl2();
            uc2 = new UserControl3();
            uc3 = new UserControl();
            selectedIndex = startidx;
            if (startidx == 0)
            {
                uc0.Resources = Application.Current.Resources;
                img0.Source = imgsource;
                ContentArea.Content = uc0;
                Button0.Focus();
            }
            else if (startidx == 1)
            {
                uc1.Resources = Application.Current.Resources;
                img1.Source = imgsource;
                ContentArea.Content = uc1;
                Button1.Focus();
            }
            else if (startidx == 2)
            {
                uc2.Resources = Application.Current.Resources;
                img2.Source = imgsource;
                ContentArea.Content = uc2;
                Button2.Focus();
            }
            else if (startidx == 3)
            {
                uc3.Resources = Application.Current.Resources;
                img3.Source = imgsource;
                ContentArea.Content = uc3;
                Button3.Focus();
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
                this.Hide();
            });
        }

        private void Connect_Clicked(object sender, RoutedEventArgs e)
        {
            this.Dispatcher.Invoke(() =>
            {
                this.DialogResult = true;
                this.Hide();
            });
        }
        
        private void Button_LostFocus(object sender, RoutedEventArgs e)
        {
            string img = "img3";
            switch (prevIndex)
            {
                case 0: img = "img0"; break;
                case 1: img = "img1"; break;
                case 2: img = "img2"; break;
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
        }

        private void Button0_Click(object sender, RoutedEventArgs e)
        {
            selectedIndex = 0;
            Button_LostFocus(sender, e);
            uc0.Resources = Application.Current.Resources;
            img0.Source = new BitmapImage(new Uri(@"/Images/01.IRIS.ico", UriKind.RelativeOrAbsolute));
            ContentArea.Content = uc0;
            prevIndex= 0;
        }
        private void Button1_Click(object sender, RoutedEventArgs e)
        {
            selectedIndex = 1;
            Button_LostFocus(sender, e);
            uc1.Resources = Application.Current.Resources;
            img1.Source = new BitmapImage(new Uri(@"/Images/01.IRIS.ico", UriKind.RelativeOrAbsolute));
            ContentArea.Content = uc1;
            prevIndex= 1;
        }
        private void Button2_Click(object sender, RoutedEventArgs e)
        {
            selectedIndex = 2;
            Button_LostFocus(sender, e);
            uc2.Resources = Application.Current.Resources;
            img2.Source = new BitmapImage(new Uri(@"/Images/01.IRIS.ico", UriKind.RelativeOrAbsolute));
            ContentArea.Content = uc2;
            prevIndex= 2;
        }
        
        private void Button3_Click(object sender, RoutedEventArgs e)
        {
            selectedIndex = 3;
            Button_LostFocus(sender, e);
            uc3.Resources = Application.Current.Resources;
            img3.Source = new BitmapImage(new Uri(@"/Images/01.IRIS.ico", UriKind.RelativeOrAbsolute));
            ContentArea.Content = uc3;
            prevIndex= 3;
        }
    }
}
