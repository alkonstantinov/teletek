using System;
using System.Collections.Generic;
using System.Text;
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
    /// Interaction logic for SettingsDialog.xaml
    /// </summary>
    public partial class SettingsDialog : Window
    {
        public SettingsDialog()
        {
            InitializeComponent();
        }

        private void OKSettings_Clicked(object sender, RoutedEventArgs e)
        {
            this.Dispatcher.Invoke(() =>
            {
                this.Hide();
            });
        }

        private void ExitSettings_Clicked(object sender, RoutedEventArgs e)
        {
            this.Dispatcher.Invoke(() =>
            {
                this.Hide();
            });
        }

        private void Help_Clicked(object sender, RoutedEventArgs e)
        {
            
        }
    }
}
