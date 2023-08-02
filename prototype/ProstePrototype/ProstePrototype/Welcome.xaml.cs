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
using System.Windows.Threading;

namespace ProstePrototype
{
    /// <summary>
    /// Interaction logic for Welcome.xaml
    /// </summary>
    public partial class Welcome : Window
    {
        DispatcherTimer dT = new DispatcherTimer();
        public Welcome()
        {
            InitializeComponent();

            ver.Text = System.Reflection.Assembly.GetEntryAssembly().GetName().Version.ToString();

            dT.Tick += new EventHandler(dt_Tick);
            dT.Interval = new TimeSpan(0, 0, 2);
            dT.Start();
        }

        private void dt_Tick(object sender, EventArgs e)
        {
            MainWindow mainWindow = new MainWindow();
            mainWindow.Show();

            dT.Stop();
            this.Close();
        }
    }
}

