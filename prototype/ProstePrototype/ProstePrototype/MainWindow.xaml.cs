using CefSharp;
using Newtonsoft.Json.Linq;
using ProstePrototype.POCO;
using System;
using System.Collections.Generic;
using System.IO;
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
        private readonly string applicationDirectory;
        private readonly JObject pages;
        public MainWindow()
        {
            InitializeComponent();
            Uri iconUri = new Uri("pack://application:,,,/ProstePrototype;component/html/Icon.ico", UriKind.RelativeOrAbsolute);
            this.Icon = BitmapFrame.Create(iconUri);
            applicationDirectory = System.IO.Path.GetDirectoryName(System.AppDomain.CurrentDomain.BaseDirectory);
            //string navigation = System.IO.Path.Combine(applicationDirectory, "html", "nav.html");
            string firstFile = System.IO.Path.Combine(applicationDirectory, "html", "index.html");
            string myFile = System.IO.Path.Combine(applicationDirectory, "html", "index.html");
            //string myFile = System.IO.Path.Combine(applicationDirectory, "html/IRIS", "IRISPanel.html");
            //string myFile = System.IO.Path.Combine(applicationDirectory, "html/IRIS", "access.html");
            //string myFile = System.IO.Path.Combine(applicationDirectory, "html/IRIS", "panels_in_network.html");
            //string myFile = System.IO.Path.Combine(applicationDirectory, "html/IRIS", "input.html");
            //string myFile = System.IO.Path.Combine(applicationDirectory, "html/IRIS", "inputs_group.html");
            //string myFile = System.IO.Path.Combine(applicationDirectory, "html/IRIS", "output.html");
            //string myFile = System.IO.Path.Combine(applicationDirectory, "html/IRIS", "fat-fbf.html");
            //string myFile = System.IO.Path.Combine(applicationDirectory, "html/IRIS", "zone.html");
            //string myFile = System.IO.Path.Combine(applicationDirectory, "html/IRIS", "zone_evac.html");
            //string myFile = System.IO.Path.Combine(applicationDirectory, "html/IRIS", "peripherial_devices.html");
            //string myFile = System.IO.Path.Combine(applicationDirectory, "html/IRIS", "peripherial_devices_none.html");
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
            pages = JObject.Parse(File.ReadAllText(System.IO.Path.Combine(applicationDirectory, "html/pages.json")));
            this.DataContext = this;
            wb1.Load("file:///" + firstFile);
            if (myFile == firstFile)
            {
                wb1.Width = 0; //if index.html is loaded
                Splitter1.Width = 0; //if index.html is loaded
            }
            else
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

        private void LoadPage(LoadPageData data)
        {
            if (!string.IsNullOrEmpty(data.LeftBrowserUrl))
            {
                wb1.Load("file:///" + System.IO.Path.Combine(applicationDirectory, "html", data.LeftBrowserUrl) );
                this.Dispatcher.Invoke(() =>
                {
                        wb1.Width = this.Width / 5;
                        Splitter1.Width = 5;
                    
                });

            }
            else
            {
                this.Dispatcher.Invoke(() =>
                {
                    wb1.Width = 0;
                    Splitter1.Width = 0;

                });
            }
            var wb2UrlAddress = "file:///" + System.IO.Path.Combine(applicationDirectory, "html", data.RightBrowserUrl);
            //if (wb2.Address != wb2UrlAddress)
            //{
            wb2.Load(wb2UrlAddress);
            //}

        }

        private void wb_JSBreadCrumb(object sender, JavascriptMessageReceivedEventArgs e)
        {
            JObject json = JObject.Parse(e.Message.ToString());
            switch (json["Command"].ToString())
            {
                case "LoadPage":
                    var lpd = new LoadPageData()
                    {
                        RightBrowserUrl = pages[json["Params"].Value<string>()].Value<JObject>()["right"].Value<string>(),
                        LeftBrowserUrl = pages[json["Params"].Value<string>()].Value<JObject>()["left"].Value<string>()
                    };
                    LoadPage(lpd);
                    break;
            }

        }
        private void Read_Clicked(object sender, RoutedEventArgs e)
        {
            this.Dispatcher.Invoke(() =>
            {
                wb1.Width = 0;
                Splitter1.Width = 0;

            });
            wb2.Load("file:///" + System.IO.Path.Combine(applicationDirectory, "html", "modal.html"));
            
        }
        private void Button_Hide_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }
        private void Button_Maximize_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = (this.WindowState != WindowState.Maximized) ? WindowState.Maximized : WindowState.Normal;
        }
        private void Exit_Clicked(object sender, RoutedEventArgs e)
        {
            Environment.Exit(0);
        }
    }
}
