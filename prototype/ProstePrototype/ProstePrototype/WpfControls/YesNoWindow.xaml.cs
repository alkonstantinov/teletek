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

namespace ProstePrototype.WpfControls
{
    /// <summary>
    /// Interaction logic for YesNoWindow.xaml
    /// </summary>
    public partial class YesNoWindow : Window
    {
        public YesNoWindow(string q, string yes = "", string no = "")
        {
            InitializeComponent();
            qText.Text = q;
            if (!String.IsNullOrEmpty(yes))
            {
                yesText.Text = yes;
            }
            if (!String.IsNullOrEmpty(no))
            {
                noText.Text = no;
            }
            // Subscribe to the Loaded event to set MaxWidth when the window is loaded.
            Loaded += YesNoWindow_Loaded;
        }

        private void YesNoWindow_Loaded(object sender, RoutedEventArgs e)
        {
            // Calculate the maximum width as 70% of the available width
            double maxWidthPercentage = 0.7;
            double maxWidth = SystemParameters.PrimaryScreenWidth * maxWidthPercentage;

            // Set the MaxWidth property of qText to "Auto" to allow it to expand as needed
            qText.MaxWidth = maxWidth;

            // Ensure the window width doesn't exceed the maximum width
            Width = Math.Min(qText.DesiredSize.Width + 40, maxWidth);
        }


        private void Yes_Clicked (object sender, RoutedEventArgs e) { DialogResult = true; }

        private void No_Clicked (object sender, RoutedEventArgs e) { DialogResult = false; }
    }
}
