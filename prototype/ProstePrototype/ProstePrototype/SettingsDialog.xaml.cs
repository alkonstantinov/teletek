using common;
using lupd;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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
        private bool isFirstActivation = true;
        private bool isClosing = false; // A flag to track whether the window is in the closing process
        public SettingsDialog(MainWindow mainWindow)
        {
            InitializeComponent();
            this.DataContext = this;
            _mainWindow = mainWindow;
            changeTheme_btn.Command = new RelayCommand(changeTheme_Click);
            currentVersion = System.Reflection.Assembly.GetEntryAssembly().GetName().Version.ToString();
            ver.Text = currentVersion;
            autoCheck.IsChecked = Properties.Settings.Default.AutoUpdate;
            
            this.Activated += Window_Activated;
        }

        private void Window_Activated(object sender, EventArgs e)
        {
            // code to execute every time the window is shown
            if (isFirstActivation)
            {
                if (UpdateLoop.UpdateExists)
                {
                    update_btn.IsEnabled = true;
                    progrBarText.Text = Utils.MakeTranslation("newUpdateAvailable");
                } else
                {
                    update_btn.IsEnabled = false;
                    progrBarText.Text = Utils.MakeTranslation("updated");
                    progrBar.Visibility = Visibility.Hidden;
                }
                isFirstActivation = false;
            }
        }

        private void OKSettings_Clicked(object sender, RoutedEventArgs e)
        {
            this.Dispatcher.Invoke(() =>
            {
                this.Hide();
                isFirstActivation = true;
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
                // CleanupAndClose(); // to be used if we can stop the proces of updating
                this.Hide();
                isFirstActivation = true;
            });
        }

        private void CleanupAndClose()
        {
            if (!isClosing)
            {
                isClosing = true;

                // Stop any processes or perform cleanup as needed
                // specially if updating is launched

                this.Close();

                _mainWindow.settings = null;
            }
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            CleanupAndClose();

            base.OnClosing(e);
        }

        private void Help_Clicked(object sender, RoutedEventArgs e)
        {
            
        }

        #region Update
        private static void crcprocess(string filename)
        {
            Console.WriteLine(filename);
        }

        private static void http_progress(string filename, int counter, int cntall, int bytes_downloaded, int bytes_all)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                SettingsDialog settingsWindow = Application.Current.Windows.OfType<SettingsDialog>().FirstOrDefault();
                ProgressBar progrBar = settingsWindow.progressBar.FindName("progrBar") as ProgressBar;
                ProgressBar progrBarAdd = settingsWindow.progressBar.FindName("progrBarAdd") as ProgressBar;

                double percentageCalc = 100 * (double)counter / (double)cntall;
                progrBar.Value = percentageCalc;
                progrBar.Maximum = 100;
                settingsWindow.progrBarText.Text = Utils.MakeTranslation("updating") + percentageCalc.ToString("##0.") + "%";

                if (bytes_all != 0)
                {
                    double percentageCalcAdd = 100 * (double)bytes_downloaded / (double)bytes_all;
                    progrBarAdd.Value = percentageCalcAdd;
                    progrBarAdd.Maximum = 100;
                    string f = filename.Split("/").Last();
                    settingsWindow.progrBarTextAdd.Text = $"{Utils.MakeTranslation("downloading")} {Utils.LimitCharacters(f, 32)}: " + percentageCalcAdd.ToString("##0.") + "%";
                }
            });
        }


        public void RunUpdate()
        {
            ProgressBarChange(Visibility.Visible, Utils.MakeTranslation("newUpdate") + "0 %", Visibility.Visible, "");
            string err = "";
            eUPDResult res = cUpd.DoUpdate(common.settings.updpath, crcprocess, http_progress, ref err);
            if (res != eUPDResult.Ok)
            {
                MessageBox.Show(
                    err,
                    Utils.MakeTranslation("UpdateError"),
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );
                progrBarText.Text = err;
            }
            else
            {
                ProgressBarChange(Visibility.Visible, Utils.MakeTranslation("updateSuccess"), Visibility.Collapsed, "");
                Application.Current.Dispatcher.Invoke(() =>
                {
                    if (_mainWindow != null)
                    {
                        _mainWindow.update_available.ToolTip = "";
                        _mainWindow.update_available.Text = "";
                        _mainWindow.update_available.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("Transparent"));
                    }
                });
                UpdateLoop.CheckUpdate();
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
            Process p = Process.Start(killRun);
            p.WaitForExit();
        }
        #endregion
        private void Update_Clicked(object sender, RoutedEventArgs e)
        {
            ProgressBarChange(Visibility.Visible, Utils.MakeTranslation("checkingUpdate"), Visibility.Collapsed, "");
            
            Task.Run(() =>
            {                
                if (UpdateLoop.UpdateExists)
                {
                    RunUpdate();
                    string locks = Regex.Replace(common.settings.updpath, @"[\\/]$", "") + System.IO.Path.DirectorySeparatorChar + "~locks" + System.IO.Path.DirectorySeparatorChar;
                    string del_locks = Regex.Replace(common.settings.updpath, @"[\\/]$", "") + System.IO.Path.DirectorySeparatorChar + "~files4del.lock";
                    if (
                        (Directory.Exists(locks) || File.Exists(del_locks)) &&
                        MessageBox.Show(
                            Utils.MakeTranslation("updateRestart"),
                            Utils.MakeTranslation("updateMsg"),
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
                    ProgressBarChange(Visibility.Visible, Utils.MakeTranslation("updated"), Visibility.Collapsed, "");
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
