using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
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
    /// Interaction logic for theme.xaml
    /// </summary>
    public partial class theme : UserControl
    {
        public static readonly DependencyProperty CommandProperty = DependencyProperty.Register(
            "Command",
            typeof(ICommand),
            typeof(theme),
            new PropertyMetadata(null));

        public ICommand Command
        {
            get { return (ICommand)GetValue(CommandProperty); }
            set { SetValue(CommandProperty, value); }
        }
        public theme()
        {
            InitializeComponent();
        }

        public static readonly DependencyProperty IsDarkModeProperty =
            DependencyProperty.Register(nameof(IsDarkMode), 
            typeof(bool), 
            typeof(theme), 
            new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnIsDarkModeChanged));

        public bool IsDarkMode
        {
            get { return (bool)GetValue(IsDarkModeProperty); }
            set { SetValue(IsDarkModeProperty, value); }
        }

        private static void OnIsDarkModeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (theme)d;
            bool isDarkMode = (bool)e.NewValue;

            if (!isDarkMode)
            {
                control.LightModeButton.Background = Brushes.White;
                control.LightModeTextIcon.Foreground = Brushes.Black;
                control.LightModeTextBlock.Foreground = Brushes.Black;
                control.DarkModeTextIcon.Foreground = Brushes.White;
                control.DarkModeTextBlock.Foreground = Brushes.White;
                control.DarkModeButton.Background = Brushes.Gray;
            }
            else
            {
                control.LightModeButton.Background = Brushes.Gray;
                control.LightModeTextIcon.Foreground = Brushes.White;
                control.LightModeTextBlock.Foreground = Brushes.White;
                control.DarkModeTextIcon.Foreground = Brushes.Black;
                control.DarkModeTextBlock.Foreground = Brushes.Black;
                control.DarkModeButton.Background = Brushes.White;
            }
        }

        private void theme_Clicked(object sender, RoutedEventArgs e)
        {
            IsDarkMode = !IsDarkMode;
        }
    }

    public class HalfValueMinusMarginConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            double doubleValue = System.Convert.ToDouble(value) - System.Convert.ToDouble(value) / 7.5;
            return doubleValue / 2;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class MarginConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            double doubleValue = System.Convert.ToDouble(value);
            return doubleValue * 0.05;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
