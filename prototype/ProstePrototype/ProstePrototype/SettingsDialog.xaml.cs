﻿using System;
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

        public void changeTheme(bool DarkMode)
        {
            var bgd = Color.FromRgb(248, 249, 250);
            var fgd = Color.FromRgb(124, 124, 125);

            if (DarkMode)
            {

                bgd = Color.FromRgb(51, 51, 34);
                fgd = Color.FromRgb(238, 238, 221);
            }
            
            mainGrid.Background = new SolidColorBrush(bgd);
            mainTbl.Foreground = new SolidColorBrush(fgd);
            autoCheck.Foreground = new SolidColorBrush(fgd);
            exit_btn.Foreground = new SolidColorBrush(fgd);
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
