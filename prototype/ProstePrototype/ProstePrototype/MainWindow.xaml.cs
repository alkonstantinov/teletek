using CefSharp;
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
            //string navigation = System.IO.Path.Combine(applicationDirectory, "html", "nav.html");
            string firstFile = System.IO.Path.Combine(applicationDirectory, "html", "index.html");
            string myFile = System.IO.Path.Combine(applicationDirectory, "html", "index.html");
            //string myFile = System.IO.Path.Combine(applicationDirectory, "html", "IRISPanel.html");
            //string myFile = System.IO.Path.Combine(applicationDirectory, "html", "access.html");
            //string myFile = System.IO.Path.Combine(applicationDirectory, "html", "panels_in_network.html");
            //string myFile = System.IO.Path.Combine(applicationDirectory, "html", "input.html");
            //string myFile = System.IO.Path.Combine(applicationDirectory, "html", "inputs_group.html");
            //string myFile = System.IO.Path.Combine(applicationDirectory, "html", "output.html");
            //string myFile = System.IO.Path.Combine(applicationDirectory, "html", "fat-fbf.html");
            //string myFile = System.IO.Path.Combine(applicationDirectory, "html", "zone.html");
            //string myFile = System.IO.Path.Combine(applicationDirectory, "html", "zone_evac.html");
            //string myFile = System.IO.Path.Combine(applicationDirectory, "html", "peripherial_devices.html");
            //string myFile = System.IO.Path.Combine(applicationDirectory, "html", "peripherial_devices_none.html");
            //string myFile = System.IO.Path.Combine(applicationDirectory, "html/LoopDevices/Teletek", "M22.html");
            //string myFile = System.IO.Path.Combine(applicationDirectory, "html/LoopDevices/System Sensor/Modules", "Default - normal.html");
            //string myFile = System.IO.Path.Combine(applicationDirectory, "html/LoopDevices/System Sensor/Sensors", "1251E.html");
            //string myFile = System.IO.Path.Combine(applicationDirectory, "html/TTE GPRS", "General Settings.html");
            //string myFile = System.IO.Path.Combine(applicationDirectory, "html/TTE GPRS", "Phones.html");
            //string myFile = System.IO.Path.Combine(applicationDirectory, "html/TTE GPRS", "Input_Output.html");
            //string myFile = System.IO.Path.Combine(applicationDirectory, "html/TTE GPRS", "Mobile Operators.html");
            //string myFile = System.IO.Path.Combine(applicationDirectory, "html/TTE GPRS", "Receiver Settings.html");
            //string myFile = System.IO.Path.Combine(applicationDirectory, "html/ECLIPSE", "General Settings.html");

            //wb0.Load("file:///" + navigation);
            this.DataContext = this;
            wb1.Load("file:///" + firstFile);
            if (myFile == firstFile)
            {
                wb1.Width = 0; //if index.html is loaded
                Splitter1.Width = 0; //if index.html is loaded
            } else
            {
                wb1.Width = this.Width / 5;
            }
            wb2.Load("file:///" + myFile);
        }

        private void NewCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        private void NewCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var i = e;
        }

        //private void wb_JavascriptMessageReceived(object sender, CefSharp.JavascriptMessageReceivedEventArgs e)
        //{
        //    Environment.Exit(0);
        //}

        private void ChangeTheme_Click(object sender, RoutedEventArgs e)
        {
            string script = @"$().click(()=>{
                $(["".light [class*='-light']"", "".dark [class*='-dark']""]).each((i, ele) => {
                    $(ele).toggleClass('bg-light bg-dark')
                    $(ele).toggleClass('text-light text-dark')
                    $(ele).toggleClass('navbar-light navbar-dark')
                })
                    // toggle body class selector
                    $('body').toggleClass('light dark')
                })";
            wb2.ExecuteScriptAsync(script);
        }

        private void wb_JSBreadCrumb(object sender, JavascriptMessageReceivedEventArgs e)
        {
            var i = ((string)e.Message).Split('|');
            string str = i[0];
            for (int j = 1; j < i.Length; j++)
            {
                str += " \\ " + i[j];
            }
            this.Dispatcher.Invoke(() =>
            {
                if (i.Length > 1)
                {
                    if (i.Length > 2)
                    {
                        string wb1CommandLine = "div" + i[i.Length - 2];
                        wb1.ExecuteScriptAsync("loadDiv(" + wb1CommandLine +", " + i[0] + "|" + i[i.Length - 2]+ ")"); // not working
                    }
                    wb1.Width = this.Width / 5;
                    Splitter1.Width = 5;
                }
                NavField.Text = str;// your code here.
            });
        }
        private void Read_Clicked(object sender, RoutedEventArgs e)
        {
            string script = "$('#divModal').modal('show')";
            wb2.ExecuteScriptAsync(script);
        }
        private void Exit_Clicked(object sender, RoutedEventArgs e)
        {
            Environment.Exit(0);
        }
    }
}
