﻿using CefSharp;
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
using System.Security.Policy;
using System.Windows.Markup;
using CefSharp.DevTools.DOM;
using lupd;
using System.Windows.Shapes;
using System.ComponentModel.Composition.Primitives;
using System.Timers;

namespace ProstePrototype
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly string applicationDirectory;
        public readonly JObject pages;
        private ReadWindow rw;
        public SettingsDialog settings;

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
        private void OnUpdTimer (object o, ElapsedEventArgs e)
        {
            _upd_timer.Stop();
            if (UpdateLoop.UpdateExists)
            {
                this.Dispatcher.Invoke(() =>
                {
                    update_available.ToolTip = "New update of Teletek Manager is available. Check Settings!";
                    update_available.Text = "\ue4c2";
                    update_available.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("Red"));
                });
            } else
            {
                this.Dispatcher.Invoke(() =>
                {
                    update_available.ToolTip = "";
                    update_available.Text = "";
                    update_available.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("Transparent"));
                });
            }
            _upd_timer.Start();
        }
        private System.Timers.Timer _upd_timer = new System.Timers.Timer(60000);

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
            
            Directory.SetCurrentDirectory(applicationDirectory);

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
            Uri iconUri = new Uri("pack://application:,,,/Images/t_m_icon.png", UriKind.RelativeOrAbsolute);
            this.Icon = BitmapFrame.Create(iconUri);

            pages = JObject.Parse(File.ReadAllText(System.IO.Path.Combine(applicationDirectory, "html/pages.json")));

            DataContext = this;

            DarkMode = Properties.Settings.Default.Theme;
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
            File.WriteAllText("eventlog.log", string.Empty);

            bool shouldLaunchAutoUpdate = Properties.Settings.Default.AutoUpdate;

            if (shouldLaunchAutoUpdate && lupd.cUpd.Check4Updates(common.settings.updpath))
            {
                Welcome w = Application.Current.Windows.OfType<Welcome>().FirstOrDefault();
                if (
                    MessageBox.Show(MakeTranslation("updateFound"),
                        MakeTranslation("updateMsg"),
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Warning) == MessageBoxResult.Yes
                    )
                {
                    w.RunUpdate();
                }
            }

            string locks = Regex.Replace(common.settings.updpath, @"[\\/]$", "") + System.IO.Path.DirectorySeparatorChar + "~locks" + System.IO.Path.DirectorySeparatorChar;
            if (Directory.Exists(locks))
            {
                SettingsDialog.KillRun(applicationDirectory, System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName, common.settings.updpath);
            }
            Thread th = new Thread(UpdateLoop.Loop);
            th.Start();
            _upd_timer.Elapsed += OnUpdTimer;
            _upd_timer.Start();
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

            Properties.Settings.Default.Theme = DarkMode;
            Properties.Settings.Default.Save(); // saving the new DarkModeValue in Theme property.

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
                if (button.IsEnabled && button.Name != "")
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

        public string MakeTranslation(string keyword) 
        {
            JObject translationsJSON = JObject.Parse(Properties.Settings.Default.translations);
            if (translationsJSON is null) return "Error attaching translations object";
            if (translationsJSON[keyword] != null) 
            {
                var t = translationsJSON[keyword];
                if (t[Properties.Settings.Default.Language]  != null)
                {
                    return t[Properties.Settings.Default.Language].ToString();
                } else
                {
                    return "Not found translation language";
                }
            } else
            {
                return $"Not found translation key";
            }
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
                    cJson.RenamePanel(cJson.CurrentPanelID.ToString(), json["Params"]["title"].Value<string>());
                    LoadPage(json["Params"]["schema"].Value<string>(), null);
                    loadWb1 = false;
                    if (json["Params"]["schema"].ToString() == "simpo")
                    {
                        JObject jnode = new JObject(cJson.GetNode("SIMPO_MIMICPANELS"));
                        if (jnode != null && jnode["CONTAINS"] != null)
                        {
                            JObject t = (JObject)jnode["CONTAINS"];
                            foreach( KeyValuePair<string, JToken> ele in t )
                            {
                                if (Regex.IsMatch(ele.Key, "^(simpo)", RegexOptions.IgnoreCase)) {
                                    JObject newMimic = cJson.GetNode(ele.Key);
                                    string s = Regex.Match(ele.Key, @"^.+?(?=\d)").Value;
                                    string n = Regex.Match(ele.Key, @"(\d+$)").Value;
                                    cComm.AddPseudoElement(cJson.CurrentPanelID, s, n, newMimic.ToString());
                                }
                            }
                        }
                    }
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

            string filePath = "eventlog.log";
            string logEntry = json["Command"].ToString();
            if (json["Params"] != null)
            {
                logEntry += " --> " + json["Params"].ToString() + "\n\n";
            } 

            using (FileStream fs = new FileStream(filePath, FileMode.Append, FileAccess.Write, FileShare.ReadWrite))
            using (StreamWriter writer = new StreamWriter(fs))
            {
                writer.Write(logEntry);
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
            if (Regex.IsMatch(_clean_key, "^(simpo|tft)", RegexOptions.IgnoreCase) && 
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
                addLastBreadCrumb(_clean_key);
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
                ((BrowserParams)wb2.Tag).Params = cJson.GroupsBrowserParam(data.key == "natron" ? "Natron" : data.key);
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
            Properties.Settings.Default.LastUsedDirectory = System.IO.Path.GetDirectoryName(fileName);
            Properties.Settings.Default.Save();
        }
        #endregion

        #region mainButtonsClicked

        private void Scan_Clicked(object sender, RoutedEventArgs e)
        {
            rw = new ReadWindow(Properties.Settings.Default.ReadWindowStartIndex); // default 
            rw.Resources = Application.Current.Resources;
            rw.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            rw.Owner = this;
            ChangeThemeRW(DarkMode);
            rw.DarkMode = DarkMode;
            try
            {
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
                    else if (tabIdx == 2)
                    {
                        string com = rw.uc2.Com;
                        string parity = rw.uc2.Parity;
                        string baudRate = rw.uc2.BaudRate;
                        string stopBits = rw.uc2.StopBits;
                        int dataBits = rw.uc2.DataBits;
                        conn_params = new cCOMParams(com, Convert.ToInt32(baudRate));
                    }
                    else if (tabIdx == 3)
                        conn_params = "read.log";

                    string panel_type = cJson.CurrentPanelType.ToString();
                    bool codeEntered = true;
                    CodeWindow cw = new CodeWindow();
                    if (panel_type != "natron")
                    {
                        cw.ShowDialog();
                        codeEntered = (bool)cw.DialogResult;
                    }
                
                    Thread funcThread = new Thread(() =>
                    {
                        try
                        {
                            if (codeEntered)
                            {
                                // Use Dispatcher.Invoke to access the property from the UI thread.
                                string code = panel_type != "natron" ? (string)Dispatcher.Invoke(new Func<string>(() => { return cw.Code; })) : "";
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
            catch
            {
                MessageBox.Show(
                    MakeTranslation("USBError"),
                    MakeTranslation("ConnectionError"),
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private void ReadDevice(object conn_params, ScanPopUpWindow popUpWindow, string code)
        {
            eRWResult resp = cJson.ReadDevice(conn_params, code, VersionDiff);
            HandleRespMessage(conn_params, resp, code, "Read");
            //set a flag
            Application.Current.Dispatcher.Invoke(() =>
            {
                popUpWindow._functionFinished = true;
            });
        }

        private void HandleRespMessage(object conn_params, eRWResult resp, string code, string readOrWrite)
        {
            string showMsg = $"Unexpected Error: Please check your provided details";
            switch (resp)
            {
                case eRWResult.Ok:
                    showMsg = "alert";
                    File.AppendAllText("eventlog.log", readOrWrite + "Device using code: " + code + "and connection parameters: " + conn_params.ToString() + "\n");
                    break;
                case eRWResult.ConnectionError:
                    showMsg = (conn_params != null && !(conn_params is string && (string)conn_params == "read.log")) ?
                        $"{MakeTranslation("ConnectionErrorFull")} {((common.cIPParams)conn_params).address}:{((common.cIPParams)conn_params).port}" :
                        MakeTranslation("ConnectionError");
                    break;
                case eRWResult.BadLogin:
                    showMsg = MakeTranslation("BadLogin"); break;
                case eRWResult.NullLoginCMD:
                    showMsg = MakeTranslation("NullLoginCMD"); break;
                case eRWResult.NullLoginOkByte:
                    showMsg = MakeTranslation("NullLoginOkByte"); break;
                case eRWResult.NullLoginOkVal:
                    showMsg = MakeTranslation("NullLoginOkVal"); break;
                case eRWResult.BadCommandResult:
                    showMsg = MakeTranslation("BadCommandResult"); break;
                case eRWResult.VersionDiff:
                    showMsg = MakeTranslation("VersionDiff");
                    File.AppendAllText("eventlog.log", readOrWrite + "Device using code: " + code + " and connection parameters: " + conn_params.ToString() +
                        " stopped due to Version Difference between panel and opened document." + "\n");
                    break;
                default:
                    break;
            }
            if (readOrWrite == "Read")
            {
                wb1.ExecuteScriptAsync($"alertScanFinished('{showMsg}')");
            }
            //else
            //{
            //    wb2.ExecuteScriptAsync($"alertScanFinished('{showMsg}')");
            //}
        }

        private bool VersionDiff(string panel_version, string xml_version)
        {
            if (panel_version != xml_version)
            {
                string q = MakeTranslation("VersionDiffQuery");
                return (MessageBox.Show(
                        q,
                        MakeTranslation("VersionDiffFound") + " -> " + MakeTranslation("panel") + ": " + panel_version + " | " + MakeTranslation("loaded") +": " + xml_version,
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Warning) == MessageBoxResult.Yes);

                //bool result = this.Dispatcher.Invoke(() =>
                //{
                //    YesNoWindow yesNoDialog = new YesNoWindow(q, "Yes", "No");
                //    return yesNoDialog.ShowDialog().Value;
                //});
                //return result;
            }
            return true;
        }
        private void WriteDevice(object conn_params, ScanPopUpWindow popUpWindow, string code)
        {
            eRWResult resp = cJson.WriteDevice(conn_params, code, VersionDiff);
            HandleRespMessage(conn_params, resp, code, "Write");
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
                try
                {
                    JToken panels = cJson.LoadFile(openFileDialog.FileName);
                    currentFileName = openFileDialog.FileName;
                    SaveLastUsedDirectory(openFileDialog.FileName);
                
                    this.Dispatcher.Invoke(() =>
                    {
                        ((BrowserParams)wb1.Tag).Params = panels.ToString();
                    });
                    AddPagesConstant();
                    ApplyTheme();
                    string url = "file:///" + System.IO.Path.Combine(applicationDirectory, "html", "menu-automatic.html");
                    wb1.Load(url);
                    wb2.Load("about:blank");
                
                    var script = $"toggleLang('{Properties.Settings.Default.Language}');";

                    this.Dispatcher.Invoke(() =>
                    {
                        wb1.ExecuteScriptAsyncWhenPageLoaded(script);
                        wb2.ExecuteScriptAsyncWhenPageLoaded(script);
                    });

                    cJson.CurrentPanelID = ((JObject)panels.Last())["~panel_id"].ToString();

                    this.Dispatcher.Invoke(() =>
                    {
                        scan_btn.IsEnabled = true;
                        ChangeTheme(DarkMode);
                    });

                    loadWb1 = false;
                } catch (Exception)
                {
                    string showMsg = $"{MakeTranslation("ReadingError")} \"{openFileDialog.SafeFileName}\"";
                    if (wb1.CanExecuteJavascriptInMainFrame)
                    {
                        wb1.ExecuteScriptAsync($"alertScanFinished('{showMsg}')");
                    } else
                    {
                        throw new Exception(showMsg);
                    }
                }
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
                        string showMsg = $"{MakeTranslation("ReadingErrorTemp")} \"{panelType}\" {MakeTranslation("ReadingErrorTempFin")}";
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
            cJson.Save(filename);
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
                currentFileName = dlg.FileName;
                cJson.SaveAs(currentFileName);
                SaveLastUsedDirectory(currentFileName);
            }
        }
        private void SettingsClicked(object sender, RoutedEventArgs e)
        {
            if (settings == null )
            {
                settings = new SettingsDialog(this);
                settings.Resources = this.Resources;
                settings.changeTheme(DarkMode);
                settings.ShowDialog();
            } else
            {
                settings.changeTheme(DarkMode);
                settings.Activate();
                settings.Show();
            }
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
                //JObject jparam = new JObject();
                //jparam["pageName"] = new JObject();
                JToken arr = lcommunicate.cComm.Scan();
                //jparam["pageName"]["wb2"] = arr;
                //((BrowserParams)wb2.Tag).Params = jparam.ToString();
                ((BrowserParams)wb2.Tag).Params = arr.ToString();
            });
            wb2.Load("file:///" + myFile);
            //wb1.
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
            rw = new ReadWindow(Properties.Settings.Default.ReadWindowStartIndex, MakeTranslation("ScanMenuHeaderW")); // default 

            rw.Resources = Application.Current.Resources;
            rw.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            rw.Owner = this;
            ChangeThemeRW(DarkMode);
            rw.DarkMode = DarkMode;
            try
            {
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
            } catch
            {
                MessageBox.Show(
                    MakeTranslation("USBError"),
                    MakeTranslation("ConnectionError"),
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private void Exit_Clicked(object sender, RoutedEventArgs e)
        {
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

        public void AddingSegmentElements(string elementType, string fieldName)
        {
            string type = string.Join("_", elementType.Split("_").Skip(1).ToArray<string>());
            switch (true)
            {
                case true when type == "INPUT": type = "iris_inputs_elements"; break;
                case true when type == "PANELINNETWORK" || type == "PANELS_R" : type = "iris_panels_in_network_elements"; break;
                case true when type == "INPUT_GROUP": type = "iris_inputs_group_elements"; break;
                case true when type == "OUTPUT": type = "iris_outputs_elements"; break;
                case true when type == "ZONE": type = "iris_zones_elements"; break;
                case true when type == "EVAC_ZONE_GROUP": type = "iris_evac_zones_elements"; break;
                case true when type.StartsWith("TTELOOP") && type.Length > 7: 
                    type = "loop_devices_teletek_loop";
                    JArray breadcrumbs = (JArray)pages["loop_devices"]["breadcrumbs"];
                    breadcrumbs[3] = type;
                    pages["loop_devices"]["breadcrumbs"] = breadcrumbs;
                    break;
                case true when type.StartsWith("LOOP") && type.Length > 4: 
                    type = "loop_devices_teletek_loop";
                    breadcrumbs = (JArray)pages["loop_devices"]["breadcrumbs"];
                    breadcrumbs[3] = type;
                    pages["loop_devices"]["breadcrumbs"] = breadcrumbs;
                    break;
                case true when type.StartsWith("MIMIC"): 
                    type = "simpo_mimicpanels_elements";
                    if (fieldName.Contains("Output")) {
                        type += "_outputs";
                    }
                    break;
                case true when type.StartsWith("NO_LOOP") || elementType.StartsWith("NO_LOOP") || elementType == "SIMPO_TTELOOP": type = "loop_devices"; break;
                case true when elementType.StartsWith("Natron"): type = "natron_none"; break;
                default: type = "iris_peripheral_devices_elements"; break;
            }
            pages[type]["title"] = fieldName;
            initBreadCrumbs(
                elementType.Split("_")[0].ToLower(), // in order to get the directory source to define the color type
                pages[type].Value<JObject>()["breadcrumbs"].Value<JArray>() // takes the respective pages ["breadcrumbs"]
                );
            addLastBreadCrumb(type);
        }

        private void addLastBreadCrumb(string page)
        {
            string color = "Blue";
            string panel_type = cJson.CurrentPanelType ?? ""; // (jnode["@PRODUCTNAME"] != null) ? (string)jnode["@PRODUCTNAME"] : "";
            if (panel_type == null) panel_type = "";
            string panel_full_type = cJson.CurrentPanelFullType;
            if (panel_type.ToLower().StartsWith("iris") || panel_type.ToLower().StartsWith("simpo") || 
                page.ToLower().StartsWith("simpo") || page.ToLower().StartsWith("iris") || page.ToLower().StartsWith("natron")) color = "Red";
            else if (panel_type.ToLower().StartsWith("tte")) color = "LightGreen";

            string currPage = page;
            if (page == "iris" && panel_full_type.ToLower().StartsWith("simpo")) // unique case when page === "iris"
            {
                currPage = "simpo";
            }

            string title = pages[currPage].Value<JObject>()["title"].Value<string>();
            string icon = pages[currPage].Value<JObject>()["icon"].Value<string>();

            Regex regex = new Regex(@"^(SIMPO Panel|IRIS Panel|Natron|TTE|Eclipse)$");
            if (regex.IsMatch(title) && cJson.ContentBrowserParam(page) != null)
            {
                title = (string)(JObject.Parse(cJson.ContentBrowserParam(page))["~panel_name"]);
            }

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

            bool noClick = false;
            string text1 = " \ue914", text2 = " ", text3 = title;
            switch (page)
            {
                case "simpo":
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
                    if (pages[page]["icon"] != null)
                    {
                        text2 = " " + pages[page]["icon"] + " ";
                    }
                    noClick = true;
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
            if (!noClick )
            {
                btn.Click += breadCrumbItemClick;
            }

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
            //JObject jnode = new JObject(cJson.GetNode(page));
            //string paneltype = (string)jnode["@PRODUCTNAME"];
            string panel_type = cJson.CurrentPanelType ?? ""; // (jnode["@PRODUCTNAME"] != null) ? (string)jnode["@PRODUCTNAME"] : "";
            string panel_full_type = cJson.CurrentPanelFullType;
            lvBreadCrumbs.Items.Clear();
            
            string color = "Blue";
            if (panel_type.ToLower().StartsWith("iris") || panel_type.ToLower().StartsWith("simpo") ||
                page.ToLower().StartsWith("simpo") || page.ToLower().StartsWith("iris") || page.ToLower().StartsWith("natron")) color = "Red";
            else if (panel_type.ToLower().StartsWith("tte")) color = "LightGreen";
            foreach (string item in breadCrumbs.Select(x => x.Value<string>()))
            {
                string currItem = item;
                if (item == "iris" && panel_full_type.ToUpper().Contains("SIMPO") && !panel_full_type.ToUpper().Contains("REPEATER")) // SIMPO_NETWORK_R, SIMPO_PANEL_R exceptions
                {
                    currItem = "simpo";
                }
                
                string title = pages[currItem].Value<JObject>()["title"].Value<string>();
                Regex regex = new Regex(@"^(SIMPO Panel|IRIS Panel|Natron|TTE|Eclipse)$");
                if (regex.IsMatch(title) && cJson.ContentBrowserParam(item) != null)
                {
                    title = (string)(JObject.Parse(cJson.ContentBrowserParam(item))["~panel_name"]);
                }
                
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
            ShowLog("eventlog.log");
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
            string script = $"setConfigConst({pages})";
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
