using CefSharp;
using CefSharp.Wpf;
using common;
using lcommunicate;
using ljson;
using Microsoft.Win32;
using Newtonsoft.Json.Linq;
using ProstePrototype.POCO;
using ProstePrototype.WpfControls;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Text;
using System.Xml.Serialization;

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

        private double tempLeft { get; set; }
        private double tempTop { get; set; }
        private double tempHeight { get; set; }
        private double tempWidth { get; set; }
        private double tempMaxHeight { get; set; }
        private double tempMaxWidth { get; set; }

        private class BrowserParams
        {
            public string Name { get; set; }
            public string JSfunc { get; set; }
            public string Params { get; set; }
        }

        public MainWindow()
        {
            applicationDirectory = System.IO.Path.GetDirectoryName(System.AppDomain.CurrentDomain.BaseDirectory);
            
            InitializeComponent();

            //MaxWidth = SystemParameters.MaximizedPrimaryScreenWidth; // not to cover the taskBar
            //MaxHeight = SystemParameters.MaximizedPrimaryScreenHeight; // not to cover the taskBar
            #region temporary params
            //// temporaty params initializing
            //tempHeight = Height;
            //tempWidth = Width;
            //tempLeft = Left;
            //tempTop = Top;
            #endregion
            Uri iconUri = new Uri("pack://application:,,,/ProstePrototype;component/Images/t_m_icon.png", UriKind.RelativeOrAbsolute);
            this.Icon = BitmapFrame.Create(iconUri);

            //string firstFile = System.IO.Path.Combine(applicationDirectory, "html", "index.html");
            //string myFile = System.IO.Path.Combine(applicationDirectory, "html", "index.html");

            //// binding Column1 and wb1 Width trial
            //Binding binding = new Binding("Width");
            //binding.Source = Column1.DataContext;
            //wb1.SetBinding(DataGridColumn.WidthProperty, binding);

            //wb0.Load("file:///" + navigation);
            pages = JObject.Parse(File.ReadAllText(System.IO.Path.Combine(applicationDirectory, "html/pages.json")));
            DataContext = this;

            DarkMode = false;
            ChangeTheme(DarkMode);
            wb1.Tag = new BrowserParams { Name = "wb1", JSfunc = "receiveMessageWPF" };
            wb2.Tag = new BrowserParams { Name = "wb2", JSfunc = "receiveMessageWPF" };
            wb1.LoadingStateChanged += OnStateChanged;
            wb2.LoadingStateChanged += OnStateChanged;

            wb1.JavascriptObjectRepository.Settings.LegacyBindingEnabled = true;
            wb1.JavascriptObjectRepository.Register("boundAsync", new CallbackObjectForJs(), options: BindingOptions.DefaultBinder);
            wb2.JavascriptObjectRepository.Settings.LegacyBindingEnabled = true;
            wb2.JavascriptObjectRepository.Register("boundAsync", new CallbackObjectForJs(), options: BindingOptions.DefaultBinder);

            define_size_txt.Text = Encoding.UTF8.GetString(Convert.FromBase64String("74SA"));
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

        private void _child_LostFocus(object sender, RoutedEventArgs e)
        {
            rw.Hide();
        }

        #region theme
        private void ChangeTheme_Click(object sender, RoutedEventArgs e)
        {
            DarkMode = !DarkMode;

            ChangeTheme(DarkMode);

            var fnm = System.IO.Path.Combine(applicationDirectory, "html/dark.css").Replace(@"\", "/");

            string script = $"toggleDarkMode({DarkMode.ToString().ToLower()}, '{fnm}')";
            wb1.ExecuteScriptAsync(script);
            wb2.ExecuteScriptAsync(script);
        }

        private void ApplyTheme()
        {
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


        private void ChangeTheme(bool darkMode)
        {
            var bgd = Color.FromRgb(248, 249, 250);
            var fgd = Color.FromRgb(124, 124, 125);
            var btn_bgd = (Color)ColorConverter.ConvertFromString("#FFebebeb");
            var btn_fgd = (Color)ColorConverter.ConvertFromString("Gray");

            if (darkMode)
            {
                //new SolidColorBrush((Color)ColorConverter.ConvertFromString("DarkGray"))
                
                bgd = Color.FromRgb(51, 51, 34);
                fgd = Color.FromRgb(238, 238, 221);
                btn_bgd = (Color)ColorConverter.ConvertFromString("DarkGray");
                btn_fgd = (Color)ColorConverter.ConvertFromString("White");
                gridBrowsers.Background = new SolidColorBrush(bgd);
            }
            else
            {
                gridBrowsers.Background = new SolidColorBrush(Color.FromArgb(0, 255, 255, 255)); // Colors.Transparent;
            }
            mainGrid.Background = new SolidColorBrush(bgd);
            //mainPanel.Background = new SolidColorBrush(bgd);
            filePanel.Background = new SolidColorBrush(btn_bgd);
            fileMenu.Foreground = new SolidColorBrush(btn_fgd);
            languageButton.Background = new SolidColorBrush(bgd);
            write_btn.Background = new SolidColorBrush(btn_bgd);
            write_btn.Foreground = new SolidColorBrush(btn_fgd);
            //log_btn.Background = new SolidColorBrush(bgd);
            //log_btn.Foreground = new SolidColorBrush(fgd);
            //clock_btn.Background = new SolidColorBrush(bgd);
            //clock_btn.Foreground = new SolidColorBrush(fgd);
            //verify_btn.Background = new SolidColorBrush(bgd);
            //verify_btn.Foreground = new SolidColorBrush(fgd);
            scan_btn.Background = new SolidColorBrush(btn_bgd);
            scan_btn.Foreground = new SolidColorBrush(btn_fgd);
            //export_btn.Background = new SolidColorBrush(btn_bgd);
            //export_btn.Foreground = new SolidColorBrush(btn_fgd);
            settings_btn.Background = new SolidColorBrush(btn_bgd);
            settings_btn.Foreground = new SolidColorBrush(btn_fgd);
            //changeTheme_btn.Background = new SolidColorBrush(bgd);
            //changeTheme_btn.Foreground = new SolidColorBrush(fgd);
            //update_btn.Background = new SolidColorBrush(bgd);
            //update_btn.Foreground = new SolidColorBrush(fgd);
            help_btn.Background = new SolidColorBrush(btn_bgd);
            help_btn.Foreground = new SolidColorBrush(btn_fgd);
            //minimize_btn.Background = new SolidColorBrush(bgd);
            //minimize_btn.Foreground = new SolidColorBrush(fgd);
            //maximize_btn.Background = new SolidColorBrush(bgd);
            //maximize_btn.Foreground = new SolidColorBrush(fgd);
            breadCrumbsField.Background = new SolidColorBrush(bgd);
            lvBreadCrumbs.Background = new SolidColorBrush(bgd);
            foreach (var item in lvBreadCrumbs.Items)
            {
                ((Button)item).Foreground = new SolidColorBrush(fgd);
            }
            //textblock_bottom.Background = new SolidColorBrush(bgd);
            //textblock_bottom.Foreground = new SolidColorBrush(fgd);
        }

        private void ChangeThemeRW(bool darkMode)
        {
            var bgd = Color.FromRgb(248, 249, 250);
            var fgd = Color.FromRgb(124, 124, 125);

            if (darkMode)
            {

                bgd = Color.FromRgb(51, 51, 34);
                fgd = Color.FromRgb(238, 238, 221);
            }

            rw.ReadWindowMain.Background = new SolidColorBrush(bgd);
            rw.txtBlk.Foreground = new SolidColorBrush(fgd);
            rw.ContentArea.Foreground= new SolidColorBrush(fgd);
            rw.ContentArea.Background= new SolidColorBrush(bgd);
        }
        #endregion

        #region Language
        private void ApplyLang(string currLang)
        {
            string script = $"setLang('{currLang}');";
            wb1.ExecuteScriptAsyncWhenPageLoaded(script);
            wb2.ExecuteScriptAsyncWhenPageLoaded(script);
        }
        #endregion

        #region PostMessage&LoadPage
        private void wb_PostMessage(object sender, JavascriptMessageReceivedEventArgs e)
        {
            //var msg = e.ConvertMessageTo<ClassWithJSFunc>();
            JObject json = JObject.Parse(e.Message.ToString());
            switch (json["Command"].ToString())
            {
                case "LoadPage":
                    string highlight = json["Highlight"] == null ? null : json["Highlight"].Value<string>();
                    LoadPage(json["Params"].Value<string>(), highlight);
                    break;
                case "MainMenuBtn":
                    switch (json["Function"].ToString())
                    {
                        case "Firmware update": break;
                        case "Read":
                        case "Write":
                        case "Verify":
                        case "Help":
                        default:
                            // New_Clicked(new object(), new RoutedEventArgs()); // not working!
                            break;
                    }
                    break;
                case "changedValue":
                    cComm.SetPathValue(cJson.CurrentPanelID, json["Params"]["path"].ToString(), json["Params"]["newValue"].ToString(), cJson.FilterValueChanged);
                    Dictionary<string, Dictionary<string, string>> analysis = cJson.AnalysePath(json["Params"]["path"].ToString(), json["Params"]["newValue"].ToString());
                    break;
                case "GoToDeviceInLoop":
                    string query_string = "";
                    JObject parms = (JObject)json["Params"];
                    foreach (JProperty prop in parms.Properties())
                        query_string += ((query_string != "") ? "&" : "") + prop.Name + "=" + prop.Value.ToString();
                    if (query_string != "")
                        query_string = "?" + query_string;
                    LoadPage("iris_loop_devices" + query_string, "iris_loop_devices");
                    break;
                case "AddingElement":
                    string elementType = json["Params"]["elementType"].ToString();
                    string elementNumber = json["Params"]["elementNumber"].ToString();
                    AddingElement(elementType, elementNumber);
                    break;
                case "RemovingElement":
                    cJson.RemoveElement(json["Params"]["elementType"].ToString(), json["Params"]["elementNumber"].ToString());
                    break;
                case "AddingLoop":
                    elementType = json["Params"]["elementType"].ToString();
                    elementType = Regex.Replace(Regex.Replace(elementType, @"^'", ""), @"'$", "");
                    elementNumber = json["Params"]["elementNumber"].ToString();
                    JObject el = cJson.GetNode(elementType + elementNumber);
                    cComm.AddPseudoElement(cJson.CurrentPanelID, elementType, elementNumber, el.ToString());
                    break;
                case "AddingLoopElement":
                    //"noneElement": "IRIS_MNONE"
                    el = cJson.GetNode(json["Params"]["loopType"].ToString() + "/" + json["Params"]["noneElement"].ToString());
                    JObject _rw = (JObject)el["~rw"];
                    if (el["PROPERTIES"] != null)
                    {
                        if (el["PROPERTIES"]["Groups"] != null)
                            el = (JObject)el["PROPERTIES"]["Groups"];
                        else
                            el = (JObject)el["PROPERTIES"];
                        el["~rw"] = _rw;
                    }
                    elementNumber = json["Params"]["deviceAddress"].ToString();
                    string loopNumber = json["Params"]["loopNumber"].ToString();
                    string loopType = json["Params"]["loopType"].ToString();
                    string noneElement = json["Params"]["noneElement"].ToString();
                    string deviceName = json["Params"]["deviceName"].ToString();
                    string key = loopType + "/" + noneElement + "#" + deviceName;
                    JObject jdev = new JObject(cJson.GetNode(json["Params"]["deviceName"].ToString()));
                    //"deviceName": "IRIS_MIO04",
                    if (jdev != null)
                    {
                        jdev = (JObject)jdev["PROPERTIES"]["Groups"];
                        el = cJson.ChangeGroupsElementsPath(jdev, elementNumber);
                        cJson.MakeRelativePath(el, key);
                    }
                    el["~rw"] = _rw;
                    el["~device"] = deviceName;
                    el["~device_type"] = noneElement;
                    cComm.AddListElement(cJson.CurrentPanelID, key, elementNumber, el.ToString());
                    //
                    //CallbackObjectForJs cb = new CallbackObjectForJs();
                    //string devs = cb.getLoopDevices("NO_LOOP", Convert.ToInt32(loopNumber));
                    //
                    break;
                /*
{
    "Command": "AddingLoopElement",
    "Params": {
        "NO_LOOP": "NO_LOOP",
        "loopType": "IRIS_LOOP1",
        "loopNumber": "1",
        "noneElement": "IRIS_SNONE",
        "deviceName": "IRIS_S2251EM",
        "deviceAddress": 1
    }
}
                 */
                case "RemovingLoop":
                    cJson.RemoveLoop(json["Params"]["elementType"].ToString().Replace("'", ""), json["Params"]["elementNumber"].ToString().Replace("'", ""));
                    //{
                    //    "Command": "RemovingLoop",
                    //    "Params": {
                    //        "elementType": "'NO_LOOP'",
                    //        "elementNumber": "'1'"
                    //    }
                    //}
                    break;
                case "RemovingLoopElement":
                    cJson.RemoveLoopElement(json["Params"]["loopType"].ToString(), json["Params"]["loopNumber"].ToString(), json["Params"]["deviceName"].ToString(), json["Params"]["deviceAddress"].ToString());
                    break;
            }

            if (json["Callback"] != null)
            {
                ChromiumWebBrowser browser = (ChromiumWebBrowser)sender;
                if (json["CallBackParams"] != null)
                {
                    browser.ExecuteScriptAsync(json["Callback"].ToString(), JArray.Parse(json["CallBackParams"].ToString()).ToObject<object[]>());
                }
                else
                {
                    browser.ExecuteScriptAsync(json["Callback"].ToString(), new object[] { });
                }
            }
        }

        public static void AddingElement(string elementType, string elementNumber)
        {
            elementType = Regex.Replace(Regex.Replace(elementType, @"^'", ""), @"'$", "");
            JObject el = cJson.GetNode(elementType);
            if (el == null)
            {
                //elementType += elementNumber;
                el = cJson.GetNode(elementType + elementNumber);
                cComm.AddPseudoElement(cJson.CurrentPanelID, elementType, elementNumber, el.ToString());
            }
            else
            {
                JObject rw = (JObject)el["~rw"];
                el = (JObject)el["PROPERTIES"]["Groups"];
                JObject newpaths = cJson.ChangeGroupsElementsPath(el, elementNumber);
                newpaths["~rw"] = rw;
                cJson.SetNodeFilters(newpaths);
                string _template = newpaths.ToString();
                cComm.AddListElement(cJson.CurrentPanelID, elementType, elementNumber, _template);
            }
        }

        private void LoadPage(string page, string highlight)
        {
            string rightBrowsersURLParams = page.Split('?').Length > 1 ? page.Split('?')[1] : "";
            page = page.Split('?')[0];
            JObject jnode = new JObject(cJson.GetNode(page));
            JToken t = jnode["PROPERTIES"];
            if (t != null)
            {
                t = ((JObject)t)["Groups"];
                if (t != null)
                    jnode["PROPERTIES"]["Groups"] = cJson.GroupsWithValues(jnode["PROPERTIES"]["Groups"].ToString());
            }
            string _clean_key = Regex.Replace(page, @"[\d-]", string.Empty);
            var lpd = new LoadPageData()
            {
                RightBrowserUrl = cJson.htmlRight(jnode) + "?" + rightBrowsersURLParams,
                LeftBrowserUrl = cJson.htmlLeft(jnode),
                key = _clean_key
            };
            LoadBrowsers(lpd, highlight);
            this.Dispatcher.Invoke(() =>
            {
                initBreadCrumbs(pages[_clean_key].Value<JObject>()["breadcrumbs"].Value<JArray>());
            });
            
        }

        private void LoadBrowsers(LoadPageData data, string highlight)
        {

            string url = "file:///" + System.IO.Path.Combine(applicationDirectory, "html", data.LeftBrowserUrl);
            if (!string.IsNullOrEmpty(highlight))
            {
                url += "?highlight=" + highlight;
            }
            this.Dispatcher.Invoke(() =>
            {
                ((BrowserParams)wb1.Tag).Params = cJson.ContentBrowserParam(data.key);
            });

            wb1.Load(url);

            // use the code below for the define_size_btn clicked function
            //{
            //this.Dispatcher.Invoke(() =>
            //{
            //    gridBrowsers.ColumnDefinitions[0].Width = new GridLength(0);
            //    wb2.Margin = new Thickness(this.Width / 4, 0, 0, 0);
            //    wb1.Width = 0;
            //    Splitter1.Width = 0;
            //    gsp3.Height = 0;
            //});
            //}
            var wb2UrlAddress = "file:///" + System.IO.Path.Combine(applicationDirectory, "html", data.RightBrowserUrl);
            this.Dispatcher.Invoke(() =>
            {
                //((BrowserParams)wb2.Tag).Params = $@"{{ ""pageName"": ""wb2: {data.RightBrowserUrl}"" }}";
                ((BrowserParams)wb2.Tag).Params = cJson.GroupsBrowserParam(data.key);
            });
            wb2.Load(wb2UrlAddress);

            AddPagesConstant();
            ApplyTheme();
        }
        #endregion
        #region sendJavaScriptToBrowsers

        private void OnStateChanged(object sender, LoadingStateChangedEventArgs args)
        {
            if (args.IsLoading == false)
            {
                this.Dispatcher.Invoke(() =>
                {
                    ChromiumWebBrowser b = (ChromiumWebBrowser)sender;
                    object btag = b.Tag;
                    string jsFunc = ((BrowserParams)btag).JSfunc;
                    string jsParams = ((BrowserParams)btag).Params;
                    string browserName = ((BrowserParams)btag).Name;
                    ChromiumWebBrowser bCall = (browserName == "wb1") ? wb1 : wb2;
                    Send_JSCommand(bCall, jsParams);
                });
            }
        }
        private void Send_JSCommand(ChromiumWebBrowser browser, string jsonTxt, string jsFuncName = "receiveMessageWPF")
        {
            if (browser.Name == "wb2") File.WriteAllTextAsync("wb2.json", jsonTxt);
            if (browser.Name == "wb1") File.WriteAllTextAsync("wb1.json", jsonTxt);
            browser.ExecuteScriptAsync(jsFuncName, new object[] { jsonTxt });
        }
        #endregion

        #region mainDropdownMenusButtonsClicked

        private void Scan_Clicked(object sender, RoutedEventArgs e)
        {
            rw = new ReadWindow();
            rw.Resources = Application.Current.Resources;
            rw.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            rw.Owner = this;
            ChangeThemeRW(DarkMode);
            rw.DarkMode = DarkMode;
            rw.ShowDialog();
            var c = rw.DialogResult;
            ScanPopUpWindow popUpWindow= new ScanPopUpWindow();
            if ((bool)c)
            {
                int tabIdx = rw.selectedIndex;
                //cTransport t = null;
                string ip = rw.uc0.Address;
                int port = rw.uc0.Port;
                object conn_params = null;
                if (tabIdx == 0)
                    conn_params = new cIPParams(ip, port);
                else if (tabIdx == 3)
                    conn_params = "read.log";
                Thread funcThread = new Thread(() => ReadDevice(conn_params, popUpWindow));
                funcThread.Start();

                popUpWindow.ShowDialog();

                funcThread.Join();

                popUpWindow.Close();
            }            
        }
        private void ReadDevice(object conn_params, ScanPopUpWindow popUpWindow)
        {
            cJson.ReadDevice(conn_params);
            wb2.ExecuteScriptAsync($"alertScanFinished('alert')");
            //set a flag
            Application.Current.Dispatcher.Invoke(() => {
                popUpWindow._functionFinished = true;
            });
        }
        private void WriteDevice(object conn_params, ScanPopUpWindow popUpWindow)
        {
            cJson.WriteDevice(conn_params);
            wb2.ExecuteScriptAsync($"alertScanFinished('alert')");
            //set a flag
            Application.Current.Dispatcher.Invoke(() =>
            {
                popUpWindow._functionFinished = true;
            });
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
            settings.Resources = this.Resources;
            settings.changeTheme(DarkMode);
            settings.ShowDialog();
        }
        private void New_Clicked(object sender, RoutedEventArgs e)
        {
            string index = "index-automatic.html";

            //string firstFile = System.IO.Path.Combine(applicationDirectory, "html", index);
            string myFile = System.IO.Path.Combine(applicationDirectory, "html", index);

            //wb1.Load("file:///" + firstFile); // firstFile should be the old content of wb1 - so nothing new
            this.Dispatcher.Invoke(() =>
            {
                JObject jparam = new JObject();
                jparam["pageName"] = new JObject();
                JToken arr = lcommunicate.cComm.Scan();
                jparam["pageName"]["wb2"] = arr;
                ((BrowserParams)wb2.Tag).Params = jparam.ToString();
                //((BrowserParams)wb2.Tag).Params = $@"{{ ""pageName"": ""wb2: {index}"" }}";rw.Resources
            });
            wb2.Load("file:///" + myFile);

            lvBreadCrumbs.Items.Clear();

            AddPagesConstant();
            ApplyTheme();
            ApplyLang(languageButton.CurrentLanguage);
        }

        private void Write_Clicked(object sender, RoutedEventArgs e)
        {
            //Duration duration = new Duration(TimeSpan.FromSeconds(2));
            //DoubleAnimation doubleanimation = new DoubleAnimation(100.00, 0, duration); // progressBar1.Value + 10
            //progressBar1.BeginAnimation(ProgressBar.ValueProperty, doubleanimation);
            //progressBar1.FlowDirection = FlowDirection.RightToLeft;
            rw = new ReadWindow(0);
            rw.Resources = Application.Current.Resources;
            rw.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            rw.Owner = this;
            ChangeThemeRW(DarkMode);
            rw.DarkMode = DarkMode;
            rw.ShowDialog();
            var c = rw.DialogResult;
            ScanPopUpWindow popUpWindow = new ScanPopUpWindow();
            if ((bool)c)
            {
                int tabIdx = rw.selectedIndex;
                //cTransport t = null;
                string ip = rw.uc0.Address;
                int port = rw.uc0.Port;
                object conn_params = null;
                if (tabIdx == 0)
                    conn_params = new cIPParams(ip, port);
                else
                    return;
                Thread funcThread = new Thread(() => WriteDevice(conn_params, popUpWindow));
                funcThread.Start();

                popUpWindow.ShowDialog();

                funcThread.Join();

                popUpWindow.Close();
            }
        }

        private void Exit_Clicked(object sender, RoutedEventArgs e)
        {
            Environment.Exit(0);
        }
        #endregion

        #region minimizeMaxmimize buttons
        private void Button_Minimize_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        private void Button_Maximize_Click(object sender, RoutedEventArgs e)
        {

            if (this.WindowState != WindowState.Maximized)
            {
                tempHeight = this.Height;
                tempWidth = this.Width;
                tempLeft = this.Left;
                tempTop = this.Top;
                tempMaxHeight = this.MaxHeight;
                tempMaxWidth = this.MaxWidth;
                WindowStyle = WindowStyle.SingleBorderWindow;
                WindowState = WindowState.Maximized;
                WindowStyle = WindowStyle.None;
            }
            else
            {
                this.Left = tempLeft;
                this.Top = tempTop;
                this.Width = tempWidth;
                this.Height = tempHeight;
                this.MaxHeight = tempMaxHeight;
                this.MaxWidth = tempMaxWidth;
                WindowStyle = WindowStyle.SingleBorderWindow;
                WindowState = WindowState.Normal;
                WindowStyle = WindowStyle.None;
            }
        }
        // exit button is available in the mainMenuButtonsClicked
        private void mainPanel_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
            {
                Button_Maximize_Click(sender, e);
            }
        }
        #endregion

        #region breadcrumb
        private void addBreadCrumb(string title, string page)
        {
            foreach(var item in lvBreadCrumbs.Items)
            {
                ((Button)item).Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("Gray"));
            }
            var btn = new Button()
            {
                ClickMode = ClickMode.Press,
                Tag = page,
                Content = title,
                Background = Brushes.Transparent,
                Foreground = DarkMode ? new SolidColorBrush((Color)ColorConverter.ConvertFromString("LightGray")) : new SolidColorBrush((Color)ColorConverter.ConvertFromString("Blue")),
                //Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("Gray"));
                BorderBrush = Brushes.Transparent
            };
            btn.Click += breadCrumbItemClick;

            lvBreadCrumbs.Items.Add(btn);
        }

        private void breadCrumbItemClick(object sender, RoutedEventArgs e)
        {
            string page = ((Button)sender).Tag.ToString();
            LoadPage(page, "");
        }
        private void initBreadCrumbs(JArray breadCrumbs)
        {
            lvBreadCrumbs.Items.Clear();
            foreach (var item in breadCrumbs.Select(x => x.Value<string>()))
            {
                string title = pages[item].Value<JObject>()["title"].Value<string>();
                if (title != "Start")
                {
                    title = $">  {title}";
                }
                addBreadCrumb(title, item);
            }
        }
        #endregion

        private void AddPagesConstant()
        {
            //string addConstScript = $"const CONFIG_CONST = {pages}";
            string script = $"setConfigConst({pages.ToString()})";
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
        }
        private void progressBar1_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {

        }

        private void wb1Size_Click(object sender, RoutedEventArgs e)
        {
            define_size_txt.Text = 
                (define_size_txt.Text) == Encoding.UTF8.GetString(Convert.FromBase64String("74SA")) ?
                Encoding.UTF8.GetString(Convert.FromBase64String("74SB")) :
                Encoding.UTF8.GetString(Convert.FromBase64String("74SA"));
            
            this.Dispatcher.Invoke(() =>
            {
                if (mainGrid.ColumnDefinitions[0].Width == new GridLength(70))
                {
                    mainGrid.ColumnDefinitions[0].Width = new GridLength(250);
                    wb1Column.Width = new GridLength(250);
                    settingsBtnRow.Height = new GridLength(0);
                    defineSizeBtnRow.Height = new GridLength(0);
                    footerBtnRow.Height = new GridLength(50);
                } else
                {
                    mainGrid.ColumnDefinitions[0].Width = new GridLength(70);
                    wb1Column.Width = new GridLength(70);
                    settingsBtnRow.Height = new GridLength(40);
                    defineSizeBtnRow.Height = new GridLength(40);
                    footerBtnRow.Height = new GridLength(0);
                }
            });
        }
    }

}
