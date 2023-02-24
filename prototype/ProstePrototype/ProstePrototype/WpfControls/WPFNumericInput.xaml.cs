using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ProstePrototype.WpfControls
{
    /// <summary>
    /// Interaction logic for WPFNumericInput.xaml
    /// </summary>
    public partial class WPFNumericInput : UserControl
    {
        public static readonly DependencyProperty LabelProperty = 
            DependencyProperty.Register("Label", typeof(string),
            typeof(WPFNumericInput));
        public string Label { 
            get { return (string)GetValue(LabelProperty); }
            set { SetValue(LabelProperty, value); }
        }

        public static readonly DependencyProperty LabelFontSizeProperty =
            DependencyProperty.Register("LabelFontSize", typeof(int),
            typeof(WPFNumericInput));
        public int LabelFontSize
        {
            get { return (int)GetValue(LabelFontSizeProperty); }
            set { SetValue(LabelFontSizeProperty, value); }
        }

        public static readonly DependencyProperty ValueProperty = 
            DependencyProperty.Register("Value", typeof(int),
            typeof(WPFNumericInput));
        public int Value { 
            get { return (int)GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }
        public WPFNumericInput()
        {
            InitializeComponent();

            this.DataContext = this;
            dataBitsBorder.Loaded += new RoutedEventHandler(LostFocus_handler);
        }

        private void NumericOnly(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }

        private void Click_handler(object sender, RoutedEventArgs e)
        {
            CommonGotFocus();
            dataBitsBorder.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#dae4f0"));
            dataBitsLabel.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("Black"));
            dataBitsText.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("Blue"));
            dataBitsText.Focus();
        }
        private void LostFocus_handler(object sender, RoutedEventArgs e)
        {            
            if (dataBitsText.Text.Length == 0)
            {
                Row2.Height = new GridLength(0, GridUnitType.Star);
                dataBitsBorder.Background = dataBitsBorder.IsEnabled ? new SolidColorBrush((Color)ColorConverter.ConvertFromString("#ffffff")) : new SolidColorBrush((Color)ColorConverter.ConvertFromString("LightGray"));
                dataBitsLabel.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("Gray"));
                dataBitsLabel.VerticalAlignment = VerticalAlignment.Center;
                dataBitsLabel.VerticalContentAlignment = VerticalAlignment.Center;
                dataBitsText.Visibility = Visibility.Hidden;
            }
            else
            {
                CommonGotFocus();
                dataBitsBorder.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#ffffff"));
                dataBitsLabel.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("Gray"));
                dataBitsText.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("Gray"));
            }
        }

        private void CommonGotFocus()
        {
            Row1.Height = new GridLength(1, GridUnitType.Star);
            Row2.Height = new GridLength(1, GridUnitType.Star);
            dataBitsLabel.Height = dataBitsBorder.Height * 0.5;
            dataBitsText.Height = dataBitsBorder.Height * 0.5;
            dataBitsText.FontSize = 12;
            dataBitsText.Visibility = Visibility.Visible;
        }

        private void MouseEnter_Event(object sender, MouseEventArgs e)
        {
            dataBitsBorder.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#dae4f0"));
        }
        private void MouseLeave_Event(object sender, MouseEventArgs e)
        {
            if (!dataBitsText.IsFocused)
            {
                dataBitsBorder.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("White"));
            }
        }
    }
}
