using CefSharp;
using Microsoft.Win32;
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
        private ReadWindow rw;
        private SettingsDialog settings;

        public bool DarkMode { get; set; }
        public MainWindow()
        {
            InitializeComponent();
            Uri iconUri = new Uri("pack://application:,,,/ProstePrototype;component/html/Icon.ico", UriKind.RelativeOrAbsolute);
            this.Icon = BitmapFrame.Create(iconUri);

            rw = new ReadWindow();
            //rw.LostFocus += _child_LostFocus;

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
            DarkMode = false;
        }

        private void NewCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        private void NewCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var i = e;
        }
        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);

            // Begin dragging the window
            this.DragMove();
        }

        private void ChangeTheme_Click(object sender, RoutedEventArgs e)
        {
            DarkMode = !DarkMode;
            var fnm = System.IO.Path.Combine(applicationDirectory, "html/dark.css").Replace(@"\","/");
            
            string script = $"toggleDarkMode({DarkMode.ToString().ToLower()}, '{fnm}')";
            wb1.ExecuteScriptAsync(script);
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
            wb2.Load(wb2UrlAddress);
            //if (DarkMode)
            //{
            var fnm = System.IO.Path.Combine(applicationDirectory, "html/dark.css").Replace(@"\", "/");

            string script = $"toggleDarkMode({DarkMode.ToString().ToLower()}, '{fnm}')";
            wb1.LoadingStateChanged += (sender, args) =>
            {
                //Wait for the Page to finish loading
                if (args.IsLoading == false)
                {
                    wb1.ExecuteScriptAsync(script);
                }
            };
            wb2.LoadingStateChanged += (sender, args) =>
            {
                //Wait for the Page to finish loading
                if (args.IsLoading == false)
                {
                    wb2.ExecuteScriptAsync(script);
                }
            };
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

        private void Open_Clicked(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Teletek Data File (*.TDF)|*.TDF|All files (*.*)|*.*";
            openFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            if (openFileDialog.ShowDialog() == true)
            {

            //    txtEditor.Text = File.ReadAllText(openFileDialog.FileName);
            }
        }
        private void Export_Clicked(object sender, RoutedEventArgs e)
        {
            SaveFileDialog dlg = new SaveFileDialog();
            dlg.FileName = "Document"; // Default file name
            dlg.DefaultExt = ".txt"; // Default file extension
            dlg.Filter = "Teletek Data File (*.TDF)|*.TDF|All files (*.*)|*.*"; // Filter files by extension

            // Show save file dialog box
            Nullable<bool> result = dlg.ShowDialog();

            // Process save file dialog box results
            if (result == true)
            {
                // Save document
                string filename = dlg.FileName;
            }
        }

        private void SaveAs_Clicked(object sender, RoutedEventArgs e)
        {
            SaveFileDialog dlg = new SaveFileDialog();
            dlg.FileName = "Document"; // Default file name
            dlg.DefaultExt = ".txt"; // Default file extension
            dlg.Filter = "Teletek Data File (*.TDF)|*.TDF|All files (*.*)|*.*"; // Filter files by extension

            // Show save file dialog box
            Nullable<bool> result = dlg.ShowDialog();

            // Process save file dialog box results
            if (result == true)
            {
                // Save document
                string filename = dlg.FileName;
            }
        }
        private void SettingsClicked(object sender, RoutedEventArgs e)
        {
            settings = new SettingsDialog();
            settings.ShowDialog();
        }
        private void Read_Clicked(object sender, RoutedEventArgs e)
        {
            
            rw.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            rw.Owner = this;
            rw.Show();
            
            //rw.Deactivated += (sender, args) => { rw.Hide(); };
            //this.Dispatcher.Invoke(() =>
            //{
            //    wb1.Width = 0;
            //    Splitter1.Width = 0;

            //});
            //wb2.Load("file:///" + System.IO.Path.Combine(applicationDirectory, "html", "modal.html"));

        }
        private void _child_LostFocus(object sender, RoutedEventArgs e)
        {
            rw.Hide();
        }
        private void Button_Minimize_Click(object sender, RoutedEventArgs e)
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
