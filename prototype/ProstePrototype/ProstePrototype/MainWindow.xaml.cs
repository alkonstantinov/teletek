using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ProstePrototype
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            //Uri iconUri = new Uri("pack://application:,,,/html/Icon.ico", UriKind.RelativeOrAbsolute);
            //this.Icon = BitmapFrame.Create(iconUri);
            string applicationDirectory = System.IO.Path.GetDirectoryName(System.AppDomain.CurrentDomain.BaseDirectory);
            //string myFile = System.IO.Path.Combine(applicationDirectory, "html", "index.html");
            //string myFile = System.IO.Path.Combine(applicationDirectory, "html", "access.html");
            //string myFile = System.IO.Path.Combine(applicationDirectory, "html", "panels_in_network.html");
            //string myFile = System.IO.Path.Combine(applicationDirectory, "html", "input.html");
            //string myFile = System.IO.Path.Combine(applicationDirectory, "html", "inputs_group.html");
            //string myFile = System.IO.Path.Combine(applicationDirectory, "html", "output.html");
            //string myFile = System.IO.Path.Combine(applicationDirectory, "html", "fat-fbf.html");
            //string myFile = System.IO.Path.Combine(applicationDirectory, "html", "zone.html");
            string myFile = System.IO.Path.Combine(applicationDirectory, "html", "zone_evac.html");

            wb.Load("file:///" + myFile);
        }

        private void NewCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        private void NewCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var i = e;
        }

        private void wb_JavascriptMessageReceived(object sender, CefSharp.JavascriptMessageReceivedEventArgs e)
        {
            Environment.Exit(0);
        }

        private void Exit_Clicked(object sender, RoutedEventArgs e)
        {
            Environment.Exit(0);
        }
    }
}
