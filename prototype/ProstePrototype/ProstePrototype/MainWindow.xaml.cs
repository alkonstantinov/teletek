using CefSharp;
using CefSharp.Wpf;
using common;
using lcommunicate;
using ljson;
using Microsoft.Win32;
using Newtonsoft.Json.Linq;
using ProstePrototype.POCO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Text;
using Newtonsoft.Json;
using System.Xml;
using System.Diagnostics;
using System.Xml.Linq;
using ProstePrototype.WpfControls;
using System.Windows.Threading;

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
        private JObject useSetting;

        public bool DarkMode { get; set; }

        private double tempLeft { get; set; }
        private double tempTop { get; set; }
        private double tempHeight { get; set; }
        private double tempWidth { get; set; }
        private double tempMaxHeight { get; set; }
        private double tempMaxWidth { get; set; }
        private string currentFileName { get; set; }

        private class BrowserParams
        {
            public string Name { get; set; }
            public string JSfunc { get; set; }
            public string Params { get; set; }
        }

        private bool loadWb1 { get; set; }

        public MainWindow()
        {
            applicationDirectory = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location);
            if (applicationDirectory == null)
            {
                // handle null case
                throw new ArgumentNullException("You cannot launch the app from the root folder.");
            }
            else
            {
                string root = System.IO.Directory.GetDirectoryRoot(applicationDirectory);
            }
            
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
            try
            {
                useSetting = JObject.Parse(File.ReadAllText(System.IO.Path.Combine(applicationDirectory, "useSetting.json")));
            } catch 
            { 
                useSetting = new JObject();
            }

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

            //define_size_txt.Text = Encoding.UTF8.GetString(Convert.FromBase64String("74SA")); valid for fa_solid font
            loadWb1 = true;

        }
       
        private void NewCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        private void NewCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var i = e;
        }

        private void _child_LostFocus(object sender, RoutedEventArgs e)
        {
            rw.Hide();
        }

        #region MainButtons MouseEnter-MouseLeave
        private void Button_MouseEnter(object sender, MouseEventArgs e)
        {
            var button = sender as Button;
            if (button != null)
            {
                string imageName = null;
                switch (button.Name)
                {
                    case "scan_btn":
                        imageName = "scan_icon";
                        break;
                    case "filePanel":
                        imageName = "file_icon";
                        break;
                    case "write_hidden_btn":
                        imageName = "edit_hidden_icon";
                        break;
                    case "write_btn":
                        imageName = "edit_icon";
                        break;
                    case "settings_hidden_btn":
                        imageName = "settings_hidden_icon";
                        break;
                    case "settings_btn":
                        imageName = "settings_icon";
                        break;
                    case "help_btn":
                        imageName = "help_icon";
                        break;
                    case "exit_btn":
                        imageName = "log_out_img";
                        break;
                }

                if (imageName != null)
                {
                    var image = button.FindName(imageName) as TextBlock;
                    if (image != null)
                    {
                        image.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#0064FF"));
                    }
                }
            }
        }

        private void Button_MouseLeave(object sender, MouseEventArgs e)
        {
            var button = sender as Button;
            if (button != null)
            {
                string imageName = null;
                switch (button.Name)
                {
                    case "scan_btn":
                        imageName = "scan_icon";
                        break;
                    case "filePanel":
                        imageName = "file_icon";
                        break;
                    case "write_hidden_btn":
                        imageName = "edit_hidden_icon";
                        break;
                    case "write_btn":
                        imageName = "edit_icon";
                        break;
                    case "settings_hidden_btn":
                        imageName = "settings_hidden_icon";
                        break;
                    case "settings_btn":
                        imageName = "settings_icon";
                        break;
                    case "help_btn":
                        imageName = "help_icon";
                        break;
                    case "exit_btn":
                        imageName = "log_out_img";
                        break;
                }

                if (imageName != null)
                {
                    var image = button.FindName(imageName) as TextBlock;
                    if (image != null)
                    {
                        if (button.Name == "exit_btn")
                        {
                            image.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("White")); 
                        } else
                        {
                            image.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#818181"));
                        }
                    }
                }
            }
        }
        #endregion

        #region theme
        public void ChangeTheme_Click(object param)
        {
            DarkMode = !DarkMode;

            ChangeTheme(DarkMode);

            var fnm = System.IO.Path.Combine(applicationDirectory, "html/dark.css").Replace(@"\", "/");

            string script = $"toggleDarkMode({DarkMode.ToString().ToLower()}, '{fnm}')";
            wb1.ExecuteScriptAsync(script);
            wb2.ExecuteScriptAsync(script);
        }

        public void ApplyTheme()
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

        public static class VisualTreeHelperExtensions
        {
            public static IEnumerable<T> FindVisualChildren<T>(DependencyObject dependencyObject) where T : DependencyObject
            {
                if (dependencyObject != null)
                {
                    for (int i = 0; i < VisualTreeHelper.GetChildrenCount(dependencyObject); i++)
                    {
                        DependencyObject child = VisualTreeHelper.GetChild(dependencyObject, i);
                        if (child is T t)
                        {
                            yield return t;
                        }
                        foreach (T childOfChild in FindVisualChildren<T>(child))
                        {
                            yield return childOfChild;
                        }
                    }
                }
            }
        }

        private void ChangeTheme(bool darkMode)
        {
            var bgd = (Color)ColorConverter.ConvertFromString("White");
            var fgd = Color.FromRgb(124, 124, 125);
            var btn_bgd = (Color)ColorConverter.ConvertFromString("#f5f5f5");
            var btn_fgd = (Color)ColorConverter.ConvertFromString("Gray");

            if (darkMode)
            {
                //new SolidColorBrush((Color)ColorConverter.ConvertFromString("DarkGray"))

                bgd = Color.FromRgb(51, 51, 34);
                fgd = Color.FromRgb(238, 238, 221);
                btn_bgd = (Color)ColorConverter.ConvertFromString("DarkGray");
                btn_fgd = (Color)ColorConverter.ConvertFromString("#FF212121");
                gridBrowsers.Background = new SolidColorBrush(bgd);
            }
            else
            {
                gridBrowsers.Background = new SolidColorBrush(Color.FromArgb(0, 255, 255, 255)); // Colors.Transparent;
            }
            mainGrid.Background = new SolidColorBrush(bgd);

            IEnumerable<Button> buttons = VisualTreeHelperExtensions.FindVisualChildren<Button>(mainGrid);
            foreach (Button button in buttons)
            {
                button.Foreground = new SolidColorBrush(btn_fgd);
                if (button.IsEnabled)
                {
                    button.Background = new SolidColorBrush(btn_bgd);
                }
                else
                {
                    button.Background = Brushes.Transparent;
                }
            }

            //filePanel.Background = new SolidColorBrush(btn_bgd);
            fileMenu.Foreground = new SolidColorBrush(btn_fgd);

            settings_btn.Background = new SolidColorBrush(btn_bgd);
            settings_btn.Foreground = new SolidColorBrush(btn_fgd);

            help_btn.Background = new SolidColorBrush(btn_bgd);
            help_btn.Foreground = new SolidColorBrush(btn_fgd);
            breadCrumbsField.Background = new SolidColorBrush(bgd);
            lvBreadCrumbs.Background = new SolidColorBrush(bgd);
            foreach (var item in lvBreadCrumbs.Items)
            {
                if (item is Button)
                {
                    ((Button)item).Foreground = new SolidColorBrush(fgd);
                }
            }
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
            rw.ContentArea.Foreground = new SolidColorBrush(fgd);
            rw.ContentArea.Background = new SolidColorBrush(bgd);
        }
        #endregion

        #region Language
        private void ApplyLang(string currLang, string fn = "setLang")
        {
            string script = $"{fn}('{currLang}');";
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
                case "NewSystem":
                    //JObject panel = cJson.AddPanel(json["Params"]["schema"].ToString());
                    JObject panel = cJson.AddPanel((JObject)json["Params"]);
                    LoadPage(json["Params"]["schema"].Value<string>(), null);
                    loadWb1 = false;
                    break;
                case "LoadPage":
                    string highlight = json["Highlight"] == null ? null : json["Highlight"].Value<string>();
                    LoadPage(json["Params"].Value<string>(), highlight);
                    loadWb1 = false;
                    break;
                case "MainMenuBtn":
                    switch (json["Function"].ToString())
                    {
                        case "Update": break;
                        case "Read":
                            string currentPanelId = json["~panel_id"].ToString();
                            string oldPanelId = cJson.CurrentPanelID.ToString();
                            cJson.CurrentPanelID = currentPanelId;
                            Application.Current.Dispatcher.Invoke(() =>
                            {
                                Scan_Clicked(new object(), new RoutedEventArgs());
                            });
                                cJson.CurrentPanelID = oldPanelId;
                            break;
                        case "Delete":
                            currentPanelId = json["~panel_id"].ToString();
                            cJson.RemovePanel(currentPanelId);
                            wb2.Load("about:blank");
                            Application.Current.Dispatcher.Invoke(() =>
                            {
                                ApplyLang(languageButton.CurrentLanguage, "toggleLang");
                            });

                            break;
                        case "Write":
                            currentPanelId = json["~panel_id"].ToString();
                            oldPanelId = cJson.CurrentPanelID.ToString();
                            cJson.CurrentPanelID = currentPanelId;
                            Application.Current.Dispatcher.Invoke(() =>
                            {
                                Write_Clicked(new object(), new RoutedEventArgs());
                            });
                            cJson.CurrentPanelID = oldPanelId;
                            break;
                        case "Verify": break;
                        case "Rename":
                            currentPanelId = json["~panel_id"].ToString();
                            string newPanelName = json["newName"].ToString();
                            cJson.RenamePanel(currentPanelId, newPanelName);
                            break;
                        case "Help": break;
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
                    loadWb1 = false;
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
            if (page == "index") { New_Clicked(new object(), new RoutedEventArgs()); return; }
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
            if (Regex.IsMatch(_clean_key, "^repeater_iris_simpo", RegexOptions.IgnoreCase))
                _clean_key = "iris";
            if (Regex.IsMatch(_clean_key, "^simpo", RegexOptions.IgnoreCase) && 
                !Regex.IsMatch(_clean_key, "paneloutputs$", RegexOptions.IgnoreCase) &&
                !Regex.IsMatch(_clean_key, "mimicpanels$", RegexOptions.IgnoreCase))
                _clean_key = "iris";
            if (pages[_clean_key] == null && pages[_clean_key.ToLower()] != null)
                _clean_key = _clean_key.ToLower();
            var lpd = new LoadPageData()
            {
                RightBrowserUrl = cJson.htmlRight(jnode) + "?" + rightBrowsersURLParams,
                LeftBrowserUrl = cJson.htmlLeft(jnode),
                key = _clean_key
            };
            LoadBrowsers(lpd, highlight);
            this.Dispatcher.Invoke(() =>
            {
                initBreadCrumbs(
                    page, // in order to get the directory source to define the color type
                    pages[_clean_key].Value<JObject>()["breadcrumbs"].Value<JArray>() // takes the respective pages ["breadcrumbs"]
                    );
                addLastBreadCrumb(
                    (string)jnode["@PRODUCTNAME"], // get the directory source to define the color type
                    _clean_key
                    );
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

            if (loadWb1)
            {
                wb1.Load(url);
                this.Dispatcher.Invoke(() =>
                {
                    scan_btn.IsEnabled = true;
                    //mainMenuOpenTDF_btn.IsEnabled = true;
                    ChangeTheme(DarkMode);
                } );

                wb1.LoadingStateChanged += (sender, args) =>
                {
                    //Wait for the Page to finish loading
                    if (args.IsLoading == false)
                    {
                        if (wb1.ActualWidth > 280)
                        {
                            wb1.ExecuteScriptAsync("toggleClosedClass('open')");
                        }
                        else
                        {
                            wb1.ExecuteScriptAsync("toggleClosedClass()");
                        }
                    }
                };
            }
            else
            {
                OnStateChanged(wb1, new LoadingStateChangedEventArgs(new object() as IBrowser, false, false, false));
                if (!string.IsNullOrEmpty(highlight))
                {
                    this.Dispatcher.Invoke(() =>
                    {
                        wb1.ExecuteScriptAsync("highlighting", new object[] { highlight });
                    });
                }
            }

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

        #region lastUsedFolder
        private string GetLastUsedDirectory()
        {
            // Read last used directory from file or MyDocuments
            if (String.IsNullOrEmpty(Properties.Settings.Default.LastUsedDirectory))
                return Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments); 
            return Properties.Settings.Default.LastUsedDirectory;
        }

        private void SaveLastUsedDirectory(string fileName)
        {
            // Save last used directory to file or registry key
            Properties.Settings.Default.LastUsedDirectory = Path.GetDirectoryName(fileName);
            Properties.Settings.Default.Save();
        }
        #endregion

        #region mainButtonsClicked

        private void Scan_Clicked(object sender, RoutedEventArgs e)
        {
            //rw = new ReadWindow(0); // for delivery to Teletek
            rw = new ReadWindow(); // default 
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
                object conn_params = null;
                if (tabIdx == 0) 
                {
                    string ip = rw.uc0.Address;
                    int port = rw.uc0.Port;
                    conn_params = new cIPParams(ip, port);
                }
                else if (tabIdx == 1)
                {
                    conn_params = rw.uc1.USBDevice;
                }
                else if (tabIdx == 3)
                    conn_params = "read.log";
                CodeWindow cw = new CodeWindow();
                cw.ShowDialog();
                var codeEntered = cw.DialogResult;
                Thread funcThread = new Thread(() =>
                {
                    try
                    {
                        if ((bool)codeEntered)
                        {
                            // Use Dispatcher.Invoke to access the property from the UI thread.
                            string code = (string)Dispatcher.Invoke(new Func<string>(() => { return cw.Code; }));
                            ReadDevice(conn_params, popUpWindow, code);
                        } else
                        {
                            Dispatcher.Invoke(new Action(() => popUpWindow.Close()));
                        }
                    }
                    catch (Exception ex)
                    {
                        wb1.ExecuteScriptAsync($"alertScanFinished('{ex.Message}')");
                        Dispatcher.Invoke(new Action(() => popUpWindow.Close()));
                    }
                });
                funcThread.Start();

                popUpWindow.ShowDialog();

                funcThread.Join();

                popUpWindow.Close();
            }
        }
        private void ReadDevice(object conn_params, ScanPopUpWindow popUpWindow, string code)
        {
            eRWResult resp = cJson.ReadDevice(conn_params, code);
            switch (resp)
            {
                case eRWResult.Ok: wb1.ExecuteScriptAsync($"alertScanFinished('alert')"); break;
                case eRWResult.ConnectionError: 
                case eRWResult.BadLogin: 
                case eRWResult.NullLoginCMD: 
                case eRWResult.NullLoginOkByte: 
                case eRWResult.NullLoginOkVal: 
                case eRWResult.BadCommandResult:
                default:
                    string showMsg = (conn_params != null && (string)conn_params != "read.log") ?
                    $"Connection Error: Please, check the provided {((common.cIPParams)conn_params).address} or {((common.cIPParams)conn_params).port}" :
                    $"Connection Error: Please check your provided details";
                    wb1.ExecuteScriptAsync($"alertScanFinished('{showMsg}')"); 
                    break;
            }
            //set a flag
            Application.Current.Dispatcher.Invoke(() =>
            {
                popUpWindow._functionFinished = true;
            });
        }
        private void WriteDevice(object conn_params, ScanPopUpWindow popUpWindow, string code)
        {
            eRWResult resp = cJson.WriteDevice(conn_params, code);
            if (resp == eRWResult.Ok)
            {
                wb2.ExecuteScriptAsync($"alertScanFinished('alert')");
            }
            else
            {
                string showMsg = conn_params != null ?
                    $"Connection Error: Please, check the provided {((common.cIPParams)conn_params).address} or {((common.cIPParams)conn_params).port}" :
                    $"Connection Error: Please, provided some connection details";
                wb2.ExecuteScriptAsync($"alertScanFinished('{showMsg}')");
            }
            //set a flag
            Application.Current.Dispatcher.Invoke(() =>
            {
                popUpWindow._functionFinished = true;
            });
        }
        private void Open_Clicked(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Teletek Manager File (*.TMF)|*.TMF|All files (*.*)|*.*";
            openFileDialog.InitialDirectory = GetLastUsedDirectory();

            if (openFileDialog.ShowDialog() == true)
            {
                cJson.LoadFile(openFileDialog.FileName);
                currentFileName = openFileDialog.FileName;
                SaveLastUsedDirectory(openFileDialog.FileName);
            }
        }
        
        private void OpenTDF_Clicked(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Teletek Data File (*.TDF)|*.TDF|All files (*.*)|*.*";
            openFileDialog.InitialDirectory = GetLastUsedDirectory();
            if (openFileDialog.ShowDialog() == true)
            {
                ScanPopUpWindow popUpWindow = new ScanPopUpWindow();
                SaveLastUsedDirectory(openFileDialog.FileName);

                Thread funcThread = new Thread(() =>
                {
                    cTDFParams conn = new cTDFParams();
                    XmlDocument doc = new XmlDocument();
                    doc.LoadXml(File.ReadAllText(openFileDialog.FileName));
                    XmlNode element = doc.SelectSingleNode("//ELEMENT[@ID='SYSTEM']/ELEMENTS/ELEMENT[1]");
                    string panelType = element.Attributes["ID"].Value;
                    panelType = panelType.Split("_")[0].ToLower();
                    switch (panelType)
                    {
                        case "tftr":
                        case "r":
                            panelType = "repeater_iris_simpo"; break;
                        default: break;
                    }
                    
                    JObject arr = lcommunicate.cComm.Scan();
                    // adding newSystem
                    JArray jArray = new JArray(((JArray)arr["fire"]).Union((JArray)arr["guard"]));
                    JObject param = (JObject)jArray.FirstOrDefault(device => device["schema"].Value<string>() == panelType);
                    JObject panel = cJson.AddPanel(param);
                    LoadPage(param["schema"].Value<string>(), null);
                    loadWb1 = false;
                    AddPagesConstant();
                    ApplyTheme();
                    this.Dispatcher.Invoke(() =>
                    {
                        ApplyLang(languageButton.CurrentLanguage);
                    });
                    conn.tdf = JObject.Parse(JsonConvert.SerializeXmlNode(doc));
                    conn.readcfg = cJson.ReadXML();
                    conn.template = cJson.TemplateXML();
                    try
                    {
                        ReadDevice(conn, popUpWindow, "");
                    } catch (Exception)
                    {
                        string showMsg = $"Reading Error - Temporarily unable to read values for \"{panelType}\" panel type from .TDF";
                        wb1.ExecuteScriptAsync($"alertScanFinished('{showMsg}')");                        
                        popUpWindow._functionFinished = true;
                    }
                });
                funcThread.Start();

                popUpWindow.ShowDialog();

                funcThread.Join();

                popUpWindow.Close();
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

        private void Save_Clicked(object sender, RoutedEventArgs e)
        {
            if (String.IsNullOrEmpty(currentFileName))
            {
                SaveAs_Clicked(sender, e);
            }
            
            // Save document
            string filename = currentFileName;
            //TODO!!!
            cJson.SaveAs(filename);
            // no need to SaveLastUsedDirectory(filename)

        }

        private void SaveAs_Clicked(object sender, RoutedEventArgs e)
        {
            SaveFileDialog dlg = new SaveFileDialog();
            dlg.FileName = "Document"; // Default file name
            dlg.DefaultExt = ".TMF"; // Default file extension
            dlg.Filter = "Teletek Manager File (*.TMF)|*.TMF|All files (*.*)|*.*"; // Filter files by extension

            // Show save file dialog box
            Nullable<bool> result = dlg.ShowDialog();

            // Process save file dialog box results
            if (result == true)
            {
                // Save document
                string filename = dlg.FileName;
                cJson.SaveAs(filename);
                SaveLastUsedDirectory(filename);
            }
        }
        private void SettingsClicked(object sender, RoutedEventArgs e)
        {
            settings = new SettingsDialog(this);
            settings.Resources = this.Resources;
            settings.changeTheme(DarkMode);
            settings.ShowDialog();
        }
        private void NewSystem_Clicked(object sender, RoutedEventArgs e)
        {
            string index = "index-automatic.html";

            //string firstFile = System.IO.Path.Combine(applicationDirectory, "html", index);
            string myFile = System.IO.Path.Combine(applicationDirectory, "html", index);

            //wb1.Load("file:///" + firstFile); // firstFile should be the old content of wb1 - so nothing new
            wb1.Load("about:blank");
            loadWb1 = true;
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
        private void New_Clicked(object sender, RoutedEventArgs e)
        {
            string index = "index-automatic.html";

            string myFile = System.IO.Path.Combine(applicationDirectory, "html", index);

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

            if (lvBreadCrumbs.Items.Count > 0)
            {
                lvBreadCrumbs.Items.Clear();
            }

            AddPagesConstant();
            ApplyTheme();
            try
            {
                ApplyLang(languageButton.CurrentLanguage);
            } catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
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
                object conn_params = null;
                if (tabIdx == 0)
                {
                    string ip = rw.uc0.Address;
                    int port = rw.uc0.Port;
                    conn_params = new cIPParams(ip, port);
                }
                else if (tabIdx == 1)
                {
                    conn_params = rw.uc1.USBDevice;
                }
                else if (tabIdx == 3)
                    conn_params = "read.log";
                else
                    return;
                CodeWindow cw = new CodeWindow();
                cw.ShowDialog();
                var codeEntered = cw.DialogResult;
                Thread funcThread = new Thread(() => {
                    if ((bool)codeEntered)
                    {
                        string code = this.Dispatcher.Invoke(new Func<string>(() => cw.Code));
                        WriteDevice(conn_params, popUpWindow, code);
                    } else
                    {
                        Dispatcher.Invoke(new Action(() => popUpWindow.Close()));
                    }                   
                });
                funcThread.Start();

                popUpWindow.ShowDialog();

                funcThread.Join();

                popUpWindow.Close();
            }
        }

        private void Exit_Clicked(object sender, RoutedEventArgs e)
        {
            File.WriteAllTextAsync("useSetting.json", useSetting.ToString());
            Environment.Exit(0);
        }
        #endregion

        #region minimizeMaxmimize buttons
        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);

            // Begin dragging the window
            this.DragMove();
        }
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

        private void addLastBreadCrumb(string panel_type, string page)
        {
            string color = "Blue";
            if (panel_type == null) panel_type = "";
            if (panel_type.ToLower().StartsWith("iris") || panel_type.ToLower().StartsWith("simpo") || 
                page.ToLower().StartsWith("simpo") || page.ToLower().StartsWith("iris")) color = "Red";
            else if (panel_type.ToLower().StartsWith("tte")) color = "LightGreen";

            if (page == "iris" && panel_type.ToLower().StartsWith("simpo")) // unique case when page === "iris"
            {
                page = "simpo";
            }

            string title = pages[page].Value<JObject>()["title"].Value<string>();
            string icon = pages[page].Value<JObject>()["icon"].Value<string>();

            var lastBreadCrumb = new StackPanel()
            {
                Orientation = Orientation.Horizontal,
                Children = {
                    new TextBlock()
                    {
                        Text = " \ue914 ",
                        FontFamily = (FontFamily)Application.Current.Resources["ram_icons"],
                        FontSize = 10,
                        Padding = new Thickness(0, 4, 0, 0),
                        Foreground = DarkMode
                            ? new SolidColorBrush((Color)ColorConverter.ConvertFromString("LightGray"))
                            : new SolidColorBrush((Color)ColorConverter.ConvertFromString("#B5B5B5"))
                    },
                    new TextBlock()
                    {
                        Text = icon + " ",
                        FontFamily = (FontFamily)Application.Current.Resources["ram_icons"],
                        VerticalAlignment = VerticalAlignment.Center,
                        FontSize = 14,
                        Foreground = DarkMode
                            ? new SolidColorBrush((Color)ColorConverter.ConvertFromString("LightGray"))
                            : new SolidColorBrush((Color)ColorConverter.ConvertFromString(color))
                    },
                    new TextBlock() {
                        Text = title,
                        Background = Brushes.Transparent,
                        Foreground = DarkMode
                            ? new SolidColorBrush((Color)ColorConverter.ConvertFromString("LightGray"))
                            : new SolidColorBrush((Color)ColorConverter.ConvertFromString(color))
                    }
                }
            };

            lvBreadCrumbs.Items.Add(lastBreadCrumb);
        }

        private void addBreadCrumb(string title, string page, string color)
        {
            var DefaultForeground = DarkMode
                ? new SolidColorBrush((Color)ColorConverter.ConvertFromString("LightGray"))
                : new SolidColorBrush((Color)ColorConverter.ConvertFromString("#B5B5B5"));

            var OnHoverForeground = DarkMode
                ? new SolidColorBrush((Color)ColorConverter.ConvertFromString("LightGray"))
                : new SolidColorBrush((Color)ColorConverter.ConvertFromString(color));

            var btn = new Button()
            {
                OverridesDefaultStyle = true,
                ClickMode = ClickMode.Press,
                Tag = page,
                Background = Brushes.Transparent,
                BorderBrush = Brushes.Transparent,
                Cursor = Cursors.Hand
            };

            string text1 = " \ue914", text2 = " ", text3 = title;
            switch (page)
            {
                case "iris":
                    text2 = " \ue908 ";
                    break;
                case "tte":
                    text2 = " \ue907 ";
                    break;
                case "eclipse":
                    text2 = " \ue906 ";
                    break;
                case "index":
                    text1 = "";
                    break;
                default:
                    break;
            }
            btn.Content = new StackPanel()
            {
                Orientation = Orientation.Horizontal,
                Children = {
                    new TextBlock()
                    {
                        Text = text1,
                        FontFamily = (FontFamily)Application.Current.Resources["ram_icons"],
                        FontSize = 10,
                        Padding = new Thickness(0, 4, 0, 0) //,
                        //Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#B5B5B5"))
                    },
                    new TextBlock()
                    {
                        Text = text2,
                        FontFamily = (FontFamily)Application.Current.Resources["ram_icons"],
                        VerticalAlignment = VerticalAlignment.Center,
                        FontSize = 14
                    },
                    new TextBlock()
                    {
                        Text = text3,
                        FontFamily = (FontFamily)Application.Current.Resources["poppins_demibold"]
                    }
                }
            };
            btn.Click += breadCrumbItemClick;

            var style = new Style(typeof(Button));
            style.Setters.Add(new Setter(Button.BackgroundProperty, Brushes.Transparent));
            style.Setters.Add(new Setter(Button.ForegroundProperty, DefaultForeground));
            var template = new ControlTemplate(typeof(Button));
            var borderFactory = new FrameworkElementFactory(typeof(Border));
            borderFactory.SetValue(Border.BackgroundProperty, new TemplateBindingExtension(Border.BackgroundProperty));
            borderFactory.AppendChild(new FrameworkElementFactory(typeof(ContentPresenter)));
            template.VisualTree = borderFactory;
            style.Setters.Add(new Setter(Control.TemplateProperty, template));

            var trigger = new Trigger { Property = Button.IsMouseOverProperty, Value = true };
            trigger.Setters.Add(new Setter(Button.ForegroundProperty, OnHoverForeground));
            style.Triggers.Add(trigger);
            btn.Style = style;

            lvBreadCrumbs.Items.Add(btn);
        }

        private void breadCrumbItemClick(object sender, RoutedEventArgs e)
        {
            string page = ((Button)sender).Tag.ToString();
            LoadPage(page, "");
        }
        private void initBreadCrumbs(string page, JArray breadCrumbs)
        {   
            JObject jnode = new JObject(cJson.GetNode(page));
            //string panel_type = (string)jnode["@PRODUCTNAME"];
            string panel_type = (jnode["@PRODUCTNAME"] != null) ? (string)jnode["@PRODUCTNAME"] : "";
            lvBreadCrumbs.Items.Clear();            

            string color = "Blue";
            if (panel_type.ToLower().StartsWith("iris") || panel_type.ToLower().StartsWith("simpo") ||
                page.ToLower().StartsWith("simpo") || page.ToLower().StartsWith("iris")) color = "Red";
            else if (panel_type.ToLower().StartsWith("tte")) color = "LightGreen";
            foreach (string item in breadCrumbs.Select(x => x.Value<string>()))
            {
                string currItem = item;
                if (item == "iris" && jnode.ToString().Contains("SIMPO") && !jnode.ToString().Contains("_R")) // SIMPO_NETWORK_R, SIMPO_PANEL_R exceptions
                {
                    currItem = "simpo";
                }
                string title = pages[currItem].Value<JObject>()["title"].Value<string>();
                addBreadCrumb(title, currItem, color);
                //addBreadCrumb(title, currItem);
            }
        }
        #endregion

        #region Logging
        private void ShowSystemLog_Clicked(object sender, RoutedEventArgs e)
        {
            ShowLog("read-iris.log");
        }

        private void ShowEventsLog_Clicked(object sender, RoutedEventArgs e)
        {
            ShowLog("read-simpo.log");
        }

        private void ShowLog(string fileName)
        {
            LoggingPopUp LogPage = new LoggingPopUp(fileName);
            LogPage.Title = $"Showing {fileName.Split(".")[0]}";
            LogPage.Show();
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
            //define_size_txt.Text = // valid for fa_solid font
            //    (define_size_txt.Text) == Encoding.UTF8.GetString(Convert.FromBase64String("74SA")) ?
            //    Encoding.UTF8.GetString(Convert.FromBase64String("74SB")) :
            //    Encoding.UTF8.GetString(Convert.FromBase64String("74SA"));

            this.Dispatcher.Invoke(() =>
            {
                if (mainGrid.ColumnDefinitions[0].Width == new GridLength(90))
                {
                    mainGrid.ColumnDefinitions[0].Width = new GridLength(280);
                    editBtnRow.Height = new GridLength(0);
                    wb1Column.Width = new GridLength(280);
                    settingsBtnRow.Height = new GridLength(0);
                    defineSizeBtnRow.Height = new GridLength(0);
                    footerBtnRow.Height = new GridLength(70);
                }
                else
                {
                    mainGrid.ColumnDefinitions[0].Width = new GridLength(90);
                    editBtnRow.Height = new GridLength(50);
                    wb1Column.Width = new GridLength(90);
                    settingsBtnRow.Height = new GridLength(50);
                    defineSizeBtnRow.Height = new GridLength(50);
                    footerBtnRow.Height = new GridLength(0);
                }
            });
            
            if (!string.IsNullOrEmpty(wb1.Address))
            {
                if (wb1.ActualWidth < 280)
                {
                    wb1.ExecuteScriptAsync("toggleClosedClass('open')");
                } else
                {
                    wb1.ExecuteScriptAsync("toggleClosedClass()");
                }
            }
        }

        private void GridOrListView_Click(object sender, RoutedEventArgs e)
        {
            //if (GridView_or_ListView_text.Text == " Grid View")
            //{
            //    GridView_or_ListView_text.Text = " List View";
            //    GridView_or_ListView_image.Source = new BitmapImage(new Uri(@"/html/imports/webfonts/iconfont/listview2x.png", UriKind.RelativeOrAbsolute));
            //}
            //else
            //{
            //    GridView_or_ListView_text.Text = " Grid View";
            //    GridView_or_ListView_image.Source = new BitmapImage(new Uri(@"/html/imports/webfonts/iconfont/gridview2x.png", UriKind.RelativeOrAbsolute));
            //}
        }

    }

}
