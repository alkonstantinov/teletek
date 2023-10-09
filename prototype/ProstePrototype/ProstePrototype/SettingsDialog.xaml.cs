using common;
using lupd;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
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
    /// Interaction logic for SettingsDialog.xaml
    /// </summary>
    public partial class SettingsDialog : Window
    {
        private MainWindow _mainWindow;
        private string currentVersion;
        private bool updateCheck;
        public SettingsDialog(MainWindow mainWindow)
        {
            InitializeComponent();
            this.DataContext = this;
            _mainWindow = mainWindow;
            changeTheme_btn.Command = new RelayCommand(changeTheme_Click);
            currentVersion = System.Reflection.Assembly.GetEntryAssembly().GetName().Version.ToString();
            ver.Text = currentVersion;
            autoCheck.IsChecked = Properties.Settings.Default.AutoUpdate;
            updateCheck = lupd.cUpd.Check4Updates(common.settings.updpath);
            update_btn.IsEnabled = updateCheck;

            progrBarText.Text = updateCheck ? "Newer version is available for download." : "You are up to date!"; 
        }

        private void OKSettings_Clicked(object sender, RoutedEventArgs e)
        {
            this.Dispatcher.Invoke(() =>
            {
                this.Hide();
            });
        }

        private void changeTheme_Click(object param)
        {
            _mainWindow.ChangeTheme_Click(param);
            changeTheme(_mainWindow.DarkMode);
        }

        public void changeTheme(bool DarkMode)
        {
            var bgd = Color.FromRgb(248, 249, 250);
            var fgd = Color.FromRgb(124, 124, 125);
            var btn_bgd = (Color)ColorConverter.ConvertFromString("#FFebebeb");
            var btn_fgd = (Color)ColorConverter.ConvertFromString("Gray");

            if (DarkMode)
            {
                bgd = Color.FromRgb(51, 51, 34);
                fgd = Color.FromRgb(238, 238, 221);
                btn_bgd = (Color)ColorConverter.ConvertFromString("DarkGray");
                btn_fgd = (Color)ColorConverter.ConvertFromString("White");
            }
            
            mainGrid.Background = new SolidColorBrush(bgd);
            mainTbl.Foreground = new SolidColorBrush(fgd);
            autoCheck.Foreground = new SolidColorBrush(fgd);
            exit_btn.Foreground = new SolidColorBrush(fgd);
            //changeTheme_btn.Background = new SolidColorBrush(btn_bgd);
            changeTheme_btn.Foreground = new SolidColorBrush(btn_fgd);
            //update_btn.Background = new SolidColorBrush(btn_bgd);
            update_btn.Foreground = new SolidColorBrush(btn_fgd);
        }
        private void ExitSettings_Clicked(object sender, RoutedEventArgs e)
        {
            this.Dispatcher.Invoke(() =>
            {
                this.Hide();
            });
        }

        private void Help_Clicked(object sender, RoutedEventArgs e)
        {
            
        }

        #region Update
        private static void crcprocess(string filename)
        {
            Console.WriteLine(filename);
        }
        //private static void http_progress(string filename, int counter, int cntall, int bytes_downloaded, int bytes_all)
        //{
        //    SettingsDialog settingsWindow = Application.Current.Windows.OfType<SettingsDialog>().FirstOrDefault();
        //    ProgressBar progrBar = settingsWindow.progressBar.FindName("progrBar") as ProgressBar;
        //    double percentageCalc = counter / cntall;
        //    progrBar.Value = percentageCalc;
        //    progrBar.Maximum = 100;
        //    settingsWindow.progrBarText.Text = "Updating... - " + percentageCalc.ToString("0.##%");
        //    ProgressBar progrBarAdd = settingsWindow.progressBar.FindName("progrBarAdd") as ProgressBar;
        //    double percentageCalcAdd = counter / cntall;
        //    progrBarAdd.Value = percentageCalcAdd;
        //    progrBarAdd.Maximum = 100;
        //    settingsWindow.progrBarTextAdd.Text = $"Downloading {filename}: " + percentageCalcAdd.ToString("0.##%");
        //}

        private static void http_progress(string filename, int counter, int cntall, int bytes_downloaded, int bytes_all)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                SettingsDialog settingsWindow = Application.Current.Windows.OfType<SettingsDialog>().FirstOrDefault();
                ProgressBar progrBar = settingsWindow.progressBar.FindName("progrBar") as ProgressBar;
                ProgressBar progrBarAdd = settingsWindow.progressBar.FindName("progrBarAdd") as ProgressBar;

                //settingsWindow.Dispatcher.Invoke(() =>
                //{
                double percentageCalc = 100 * counter / cntall;
                progrBar.Value = percentageCalc;
                progrBar.Maximum = 100;
                settingsWindow.progrBarText.Text = "Updating... - " + percentageCalc.ToString("##0.##") + "%";

                if (bytes_all != 0)
                {
                    double percentageCalcAdd = 100 * bytes_downloaded / bytes_all;
                    progrBarAdd.Value = percentageCalcAdd;
                    progrBarAdd.Maximum = 100;
                    string f = filename.Split("/").Last();
                    settingsWindow.progrBarTextAdd.Text = $"Downloading {LimitCharacters(f, 32)}: " + percentageCalcAdd.ToString("##0.##") + "%";
                }
                //});
            });
        }

        public static string LimitCharacters(string text, int length)
        {
            if (string.IsNullOrEmpty(text))
            {
                return string.Empty;
            }

            // If text in shorter or equal to length, just return it
            if (text.Length <= length)
            {
                return text;
            }

            // Text is longer, so try to find out where to cut
            char[] delimiters = new char[] { ' ', '.', ',', ':', ';' };
            int index = text.LastIndexOfAny(delimiters, length - 3);

            if (index > (length / 2))
            {
                return text.Substring(0, index) + "...";
            }
            else
            {
                return text.Substring(0, length - 3) + "...";
            }
        }


        public void RunUpdate()
        {
            ProgressBarChange(Visibility.Visible, "Newer version found! Proceeding with update... 0%", Visibility.Visible, "");
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
                progrBarText.Text = err;
            }
            else
            {
                ProgressBarChange(Visibility.Visible, "Successfully updated!", Visibility.Collapsed, "");
            }
        }

        public static void KillRun(string appDir, string exePath, string updPath)
        {            
            ProcessStartInfo killRun = new ProcessStartInfo(
                appDir +
                Regex.Replace(common.settings.killrunpath, @"[\\/]$", "") + 
                System.IO.Path.DirectorySeparatorChar + 
                "kill_run.exe"
                );
            killRun.WindowStyle = ProcessWindowStyle.Normal;
            // killRun.Verb = "runas";
            killRun.Arguments = $"\"{exePath}\" \"{updPath}\"";
            Process.Start(killRun);
        }
        #endregion
        private void Update_Clicked(object sender, RoutedEventArgs e)
        {
            ProgressBarChange(Visibility.Visible, "Checking for updates... please, wait!", Visibility.Collapsed, "");
            
            Task.Run(() =>
            {                
                if (updateCheck)
                {
                    RunUpdate();
                    string locks = Regex.Replace(common.settings.updpath, @"[\\/]$", "") + System.IO.Path.DirectorySeparatorChar + "~locks" + System.IO.Path.DirectorySeparatorChar;
                    if (
                        Directory.Exists(locks) &&
                        MessageBox.Show(@$"Teletek Manager needs to restart to finish the update. Information on currently opened projects might be lost. Proceed?" +
                        "\n If you choose \"No\" the update will automatically finalize next time you relaunch.",
                            "Update Warning",
                            MessageBoxButton.YesNo,
                            MessageBoxImage.Warning) == MessageBoxResult.Yes
                        )
                    {
                        string appDir = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location);
                        SettingsDialog.KillRun(appDir, System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName, common.settings.updpath);
                    }
                }
                else
                {
                    ProgressBarChange(Visibility.Visible, "You are up to date!", Visibility.Collapsed, "");
                }
            });
            
        }

        private void ProgressBarChange(Visibility progrBarVisibility, string progrBarTextText, Visibility progrBarAddVisibility, string progrBarTextAddText)
        {
            this.Dispatcher.Invoke(() =>
            {
                progrBar.Visibility = progrBarVisibility;
                progrBarText.Text = progrBarTextText;
                progrBarAdd.Visibility = progrBarAddVisibility;
                progrBarTextAdd.Text = progrBarTextAddText;
            });
        }

        private void AutoCheck_Clicked(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.AutoUpdate = (bool)autoCheck.IsChecked;
            Properties.Settings.Default.Save();
        }
    }

    public class RelayCommand : ICommand
    {
        private readonly Action<object> _execute;
        private readonly Predicate<object> _canExecute;

        public RelayCommand(Action<object> execute, Predicate<object> canExecute = null)
        {
            if (execute == null)
                throw new ArgumentNullException("execute");
            _execute = execute;
            _canExecute = canExecute;
        }

        public bool CanExecute(object parameter)
        {
            return _canExecute == null || _canExecute(parameter);
        }

        public void Execute(object parameter)
        {
            _execute(parameter);
        }

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }
    }
}
