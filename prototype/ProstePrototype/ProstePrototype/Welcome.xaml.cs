using common;
using lupd;
using System;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace ProstePrototype
{
    /// <summary>
    /// Interaction logic for Welcome.xaml
    /// </summary>
    public partial class Welcome : Window, INotifyPropertyChanged
    {
        private string myAddValue;
        public string MyAddValue
        {
            get { return myAddValue; }
            set
            {
                myAddValue = value;
                NotifyPropertyChanged("myAddValue");
            }
        }
        private string myValue; 
        public string MyValue
        {
            get { return myValue; }
            set
            {
                myValue = value;
                NotifyPropertyChanged("myValue");
            }
        }

        private void NotifyPropertyChanged(string propName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propName));
                this.Activate();
            }
        }
        public event PropertyChangedEventHandler PropertyChanged;
        private static Action EmptyDelegate = delegate () { };

        DispatcherTimer dT = new DispatcherTimer();
        public Welcome()
        {
            InitializeComponent();
            this.DataContext = this;

            ver.Text = System.Reflection.Assembly.GetEntryAssembly().GetName().Version.ToString();
            MyValue = "Loading...";

            dT.Tick += new EventHandler(dt_Tick);
            dT.Interval = new TimeSpan(0, 0, 2);
            dT.Start();

            Activated += Welcome_Activated;
        }

        private void Welcome_Activated(object sender, EventArgs e)
        {
            welcomeMsg.Dispatcher.Invoke(DispatcherPriority.Render, EmptyDelegate);
        }

        private void dt_Tick(object sender, EventArgs e)
        {
            MainWindow mainWindow = new MainWindow();
            mainWindow.Show();

            dT.Stop();
            this.Close();
        }

        #region Update
        private static void crcprocess(string filename)
        {
            Console.WriteLine(filename);
        }
        private static void http_progress(string filename, int counter, int cntall, int bytes_downloaded, int bytes_all)
        {
            double percentageCalcAdd = 100 * (double)bytes_downloaded / bytes_all;
            string f = filename.Split("/").Last();
            double percentageCalc = 100 * (double)counter / cntall;
            Application.Current.Dispatcher.Invoke(() =>
            {
                Welcome welcomeWindow = Application.Current.Windows.OfType<Welcome>().FirstOrDefault();
                welcomeWindow.progrBar.Value = percentageCalc;
                welcomeWindow.MyAddValue = $"Downloading {SettingsDialog.LimitCharacters(f, 45)}: {percentageCalcAdd.ToString("0.##")}%";
                welcomeWindow.MyValue = "A newer version of Teletek Manager found! Proceeding with update... - " + percentageCalc.ToString("0.##") + "%";
                welcomeWindow.Welcome_Activated(welcomeWindow, new EventArgs());
            });
        }

        public void RunUpdate()
        {
            this.Dispatcher.Invoke(() =>
            {
                progrBar.Visibility = Visibility.Visible;

                MyValue = "A newer version of Teletek Manager found! Proceeding with update...";

            });

            string err = "";
            eUPDResult res = cUpd.DoUpdate(common.settings.updpath, crcprocess, http_progress, ref err);
            if (res != eUPDResult.Ok)
            {
                MessageBox.Show(
                        err,
                        "Update Message - Error",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error
                    );
            }
            else
            {
                this.Dispatcher.Invoke(() =>
                {
                    progrBar.Value = 100;
                    MyValue = "Teletek Manager has been successfully updated! Loading...";
                });
            }
        }
        #endregion
    }
}

