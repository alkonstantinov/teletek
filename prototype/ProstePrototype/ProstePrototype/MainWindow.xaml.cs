using CefSharp;
using Microsoft.Win32;
using Newtonsoft.Json.Linq;
using ProstePrototype.POCO;
using System;
using System.Collections.Generic;
using System.Globalization;
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

        private double tempLeft { get; set; }
        private double tempTop { get; set; }
        private double tempHeight { get; set; }
        private double tempWidth { get; set; }
        public MainWindow()
        {
            InitializeComponent();
            MaxWidth = SystemParameters.MaximizedPrimaryScreenWidth; // not to cover the taskBar
            MaxHeight = SystemParameters.MaximizedPrimaryScreenHeight; // not to cover the taskBar
            #region temporary params
            // temporaty params initializing
            tempHeight = this.Height;
            tempWidth = this.Width;
            tempLeft = this.Left;
            tempTop = this.Top;
            #endregion
            Uri iconUri = new Uri("pack://application:,,,/ProstePrototype;component/Images/t_m_icon.png", UriKind.RelativeOrAbsolute);
            this.Icon = BitmapFrame.Create(iconUri);

            rw = new ReadWindow();
            //rw.LostFocus += _child_LostFocus;
            
            //// binding Column1 and wb1 Width trial
            //Binding binding = new Binding("Width");
            //binding.Source = Column1.DataContext;
            //wb1.SetBinding(DataGridColumn.WidthProperty, binding);

            applicationDirectory = System.IO.Path.GetDirectoryName(System.AppDomain.CurrentDomain.BaseDirectory);
            //string firstFile = System.IO.Path.Combine(applicationDirectory, "html", "index.html");
            //string myFile = System.IO.Path.Combine(applicationDirectory, "html", "index.html");

            //wb0.Load("file:///" + navigation);
            pages = JObject.Parse(File.ReadAllText(System.IO.Path.Combine(applicationDirectory, "html/pages.json")));
            DataContext = this;
            //wb1.Load("file:///" + firstFile);
            //if (myFile == firstFile)
            //{
            //    wb1.Width = 0; //if index.html is loaded
            //    Splitter1.Width = 0; //if index.html is loaded
            //    gsp3.Height = 0;
            //}
            //else
            //{
            //    wb1.Width = this.Width / 6;
            //    gsp3.Height = 3;
            //}
            //wb2.Load("file:///" + myFile);
            DarkMode = false;
            ChangeTheme(DarkMode);
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

            if (darkMode)
            {
                bgd = Color.FromRgb(51, 51, 34);
                fgd = Color.FromRgb(238, 238, 221);
                gridBrowsers.Background = new SolidColorBrush(bgd);
            }
            else
            {
                gridBrowsers.Background = new SolidColorBrush(Color.FromArgb(0, 255, 255, 255)); // Colors.Transparent;
            }
            mainGrid.Background = new SolidColorBrush(bgd);
            mainPanel.Background = new SolidColorBrush(bgd);
            //title.Background = new SolidColorBrush(bgd);
            //title.Foreground = new SolidColorBrush(fgd);
            topMenu.Background = new SolidColorBrush(bgd);
            topMenu.Foreground = new SolidColorBrush(fgd);
            changeTheme_btn.Background = new SolidColorBrush(bgd);
            changeTheme_btn.Foreground = new SolidColorBrush(fgd);
            minimize_btn.Background = new SolidColorBrush(bgd);
            minimize_btn.Foreground = new SolidColorBrush(fgd);
            maximize_btn.Background = new SolidColorBrush(bgd);
            maximize_btn.Foreground = new SolidColorBrush(fgd);
            exit_btn.Background = new SolidColorBrush(bgd);
            exit_btn.Foreground = new SolidColorBrush(fgd);
            breadCrumbsField.Background = new SolidColorBrush(bgd);
            lvBreadCrumbs.Background = new SolidColorBrush(bgd);
            foreach (var item in lvBreadCrumbs.Items)
            {
                ((Button)item).Foreground = new SolidColorBrush(fgd);
            }
            grid6.Background = new SolidColorBrush(bgd);
            textblock_bottom.Background = new SolidColorBrush(bgd);
            textblock_bottom.Foreground = new SolidColorBrush(fgd);
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
            rw.tabControl.Background = new SolidColorBrush(bgd);
            rw.tabControl.Foreground = new SolidColorBrush(fgd);
            rw.listBox.Background = new SolidColorBrush(bgd);
            rw.listBox.Foreground = new SolidColorBrush(fgd);
            rw.txtBlk.Foreground = new SolidColorBrush(fgd);
            rw.lbHost.Foreground = new SolidColorBrush(fgd);
            rw.lbPort.Foreground = new SolidColorBrush(fgd);
            rw.lbComPort.Foreground = new SolidColorBrush(fgd);
            rw.lbParity.Foreground = new SolidColorBrush(fgd);
            rw.lbBaudRate.Foreground = new SolidColorBrush(fgd);
            rw.lbStopBits.Foreground = new SolidColorBrush(fgd);
            rw.lbDataBits.Foreground = new SolidColorBrush(fgd);
        }
        #endregion

        #region PostMessage&LoadPage
        private void wb_PostMessage(object sender, JavascriptMessageReceivedEventArgs e)
        {
            JObject json = JObject.Parse(e.Message.ToString());
            switch (json["Command"].ToString())
            {
                case "LoadPage":
                    string highlight = json["Highlight"] == null ? null : json["Highlight"].Value<string>();
                    LoadPage(json["Params"].Value<string>(), highlight);
                    break;
            }
        }

        private void LoadPage(string page, string highlight)
        {
            var lpd = new LoadPageData()
            {
                RightBrowserUrl = pages[page].Value<JObject>()["right"].Value<string>(),
                LeftBrowserUrl = pages[page].Value<JObject>()["left"].Value<string>()
            };
            LoadBrowsers(lpd, highlight);
            this.Dispatcher.Invoke(() =>
            {
                initBreadCrumbs(pages[page].Value<JObject>()["breadcrumbs"].Value<JArray>());
            });
        }

        private void LoadBrowsers(LoadPageData data, string highlight)
        {

            if (!string.IsNullOrEmpty(data.LeftBrowserUrl))
            {
                string url = "file:///" + System.IO.Path.Combine(applicationDirectory, "html", data.LeftBrowserUrl);
                if (!string.IsNullOrEmpty(highlight))
                {
                    url += "?highlight=" + highlight;
                }
                wb1.Load(url);
                this.Dispatcher.Invoke(() =>
                {
                    if (Splitter1.Width == 0)
                    {
                        gridBrowsers.ColumnDefinitions[0].Width = new GridLength(this.Width / 6);
                        wb2.Margin = new Thickness(0);
                        wb1.MinWidth = this.Width / 6;
                        Splitter1.Width = 5;
                        gsp3.Height = 3;
                    }
                });

            }
            else
            {
                this.Dispatcher.Invoke(() =>
                {
                    gridBrowsers.ColumnDefinitions[0].Width = new GridLength(0);
                    wb2.Margin = new Thickness(this.Width / 5, 0, 0, 0);
                    wb1.Width = 0;
                    Splitter1.Width = 0;
                    gsp3.Height = 0;
                });
            }
            var wb2UrlAddress = "file:///" + System.IO.Path.Combine(applicationDirectory, "html", data.RightBrowserUrl);
            wb2.Load(wb2UrlAddress);
            ApplyTheme();
        }
        #endregion

        private void GridSplitter1_DragCompleted(object sender, System.Windows.Controls.Primitives.DragCompletedEventArgs e)
        {
            wb1.Width = Column1.Width.Value;
            //MessageBox.Show("Column1 : " + Column1.Width + "(type: "+ Column1.Width.GetType() + ")" + "\n" + "GridSplitter: " + Splitter1.Width + "\n" + "wb1: " + wb1.Width + "\n" + "wb2: " + wb2.Width);
        }

        #region mainDropdownMenusButtonsClicked
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
            settings.changeTheme(DarkMode);
            settings.ShowDialog();
        }
        private void Read_Clicked(object sender, RoutedEventArgs e)
        {
            rw.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            rw.Owner = this;
            ChangeThemeRW(DarkMode);
            rw.DarkMode = DarkMode;
            rw.ShowDialog();
            var c = rw.DialogResult;
            if ((bool)c)
            {
                string firstFile = System.IO.Path.Combine(applicationDirectory, "html", "index.html");
                string myFile = System.IO.Path.Combine(applicationDirectory, "html", "index.html");
                wb1.Load("file:///" + firstFile);
                if (myFile == firstFile)
                {
                    gridBrowsers.ColumnDefinitions[0].Width = new GridLength(0);
                    wb2.Margin = new Thickness(this.Width / 5, 0, 0, 0);
                    wb1.Width = 0; //if index.html is loaded
                    Splitter1.Width = 0; //if index.html is loaded
                    gsp3.Height = 0;
                }
                else if (Splitter1.Width == 0)
                {
                    gridBrowsers.ColumnDefinitions[0].Width = new GridLength(this.Width / 6);
                    wb2.Margin = new Thickness(0);
                    wb1.MinWidth = this.Width / 6;
                    Splitter1.Width = 5;
                    gsp3.Height = 3;
                }
                wb2.Load("file:///" + myFile);
            }
            else
            {
                gridBrowsers.ColumnDefinitions[0].Width = new GridLength(0);
                wb2.Margin = new Thickness(this.Width / 5, 0, 0, 0);
                wb1.Width = 0; //if index.html is loaded
                Splitter1.Width = 0; //if index.html is loaded
                gsp3.Height = 0;
                wb2.Load("file:///");
            }
            lvBreadCrumbs.Items.Clear();
            ApplyTheme();
            rw = new ReadWindow();
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
                this.Left = SystemParameters.VirtualScreenLeft;
                this.Top = SystemParameters.VirtualScreenTop;
                this.Width = SystemParameters.VirtualScreenWidth;
                this.Height = SystemParameters.VirtualScreenHeight;

                this.WindowState = WindowState.Maximized;
            } else
            {
                this.Left = tempLeft;
                this.Top = tempTop;
                this.Width = tempWidth;
                this.Height = tempHeight;
                this.WindowState = WindowState.Normal;
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
            var btn = new Button()
            {
                ClickMode = ClickMode.Press,
                Tag = page,
                Content = title,
                Background = Brushes.Transparent,
                Foreground = DarkMode ? new SolidColorBrush(Color.FromRgb(238, 238, 221)) : new SolidColorBrush(Color.FromRgb(124, 124, 125)),
                //Foreground = new SolidColorBrush(Color.FromRgb(238, 238, 221)),
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
    }
}
