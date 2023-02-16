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

        public bool DarkMode { get; set; }
        public UserControl1 uc1 { get; set; }
        public UserControl2 uc2 { get; set; }
        public UserControl3 uc3 { get; set; }
        public UserControl uc4 { get; set; }
        private Color selectedColor = Color.FromRgb(30, 115, 190);
        public ReadWindow()
        {
            InitializeComponent();
            this.DataContext = this;
            uc1 = new UserControl1();
            uc2 = new UserControl2();
            uc3 = new UserControl3();
            uc4 = new UserControl();
            uc4.Resources = this.Resources;
            ContentArea.Content = uc4;
            Button4.Background = new SolidColorBrush(this.selectedColor);
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
        
        private void removeHoverColor()
        {
            switch (this.selectedIndex)
            {
                case 0:
                    Button1.Background = this.Background; break; 
                case 1:
                    Button2.Background = this.Background; break;
                case 2:
                    Button3.Background = this.Background; break;
                default: 
                    Button4.Background= this.Background; break;
            }
        }

        private void Button1_Click(object sender, RoutedEventArgs e)
        {
            removeHoverColor();
            selectedIndex = 0;            
            uc1.Resources = this.Resources;
            ContentArea.Content = uc1;
            Button1.Background = new SolidColorBrush(this.selectedColor);

        }
        private void Button2_Click(object sender, RoutedEventArgs e)
        {
            removeHoverColor();
            selectedIndex = 1;
            uc2.Resources = this.Resources;
            ContentArea.Content = uc2;
            Button2.Background = new SolidColorBrush(this.selectedColor);

        }
        private void Button3_Click(object sender, RoutedEventArgs e)
        {
            removeHoverColor();
            selectedIndex = 2;
            uc3.Resources = this.Resources;
            ContentArea.Content = uc3;
            Button3.Background = new SolidColorBrush(this.selectedColor);
        }
        
        private void Button4_Click(object sender, RoutedEventArgs e)
        {
            removeHoverColor();
            selectedIndex = 3;
            uc4.Resources = this.Resources;
            ContentArea.Content = uc4;
            Button4.Background = new SolidColorBrush(this.selectedColor);
        }
    }
}
