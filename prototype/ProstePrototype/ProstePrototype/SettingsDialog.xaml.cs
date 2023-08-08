using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace ProstePrototype
{
    /// <summary>
    /// Interaction logic for SettingsDialog.xaml
    /// </summary>
    public partial class SettingsDialog : Window
    {
        private MainWindow _mainWindow;
        private string currentVersion;
        public SettingsDialog(MainWindow mainWindow)
        {
            InitializeComponent();
            _mainWindow = mainWindow;
            changeTheme_btn.Command = new RelayCommand(changeTheme_Click);
            currentVersion = System.Reflection.Assembly.GetEntryAssembly().GetName().Version.ToString();
            ver.Text = currentVersion;
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

        private void Update_Clicked(object sender, RoutedEventArgs e)
        {
            // send for comparison in current version
            // Get the path to the folder.
            string folderPath = Directory.GetParent(Directory.GetCurrentDirectory()).Parent.FullName + "\\Release";

            // Check if the folder exists.
            if (Directory.Exists(folderPath))
            // if (System.Net.NetworkInformation.NetworkInterface.GetIsNetworkAvailable()) // checks for internet
            {
                /*
                FtpWebRequest request = (FtpWebRequest)WebRequest.Create("ftp://ftp.example.com/remote/path/");
                request.Method = WebRequestMethods.Ftp.ListDirectoryDetails;
                request.Credentials = new NetworkCredential("username", "password");
                FtpWebResponse response = (FtpWebResponse)request.GetResponse();
                Stream responseStream = response.GetResponseStream();
                StreamReader reader = new StreamReader(responseStream);
                string fileData = reader.ReadToEnd();
                reader.Close();
                response.Close();
                string[] files = fileData.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
                
                foreach (string file in files)
        {
            string[] details = file.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            string fileName = details[details.Length - 1];

            FtpWebRequest requestFile = (FtpWebRequest)WebRequest.Create("ftp://ftp.example.com/remote/path/" + fileName);
            requestFile.Method = WebRequestMethods.Ftp.GetDateTimestamp;
            requestFile.Credentials = new NetworkCredential("username", "password");
            FtpWebResponse responseFile = (FtpWebResponse)requestFile.GetResponse();
            DateTime creationDate = responseFile.LastModified;
            responseFile.Close();

            // Download the file to a temporary location
            WebClient client = new WebClient();
            client.Credentials = new NetworkCredential("username", "password");
            client.DownloadFile("ftp://ftp.example.com/remote/path/" + fileName, Path.GetTempPath() + fileName);

            FileVersionInfo myFileVersionInfo = FileVersionInfo.GetVersionInfo(Path.GetTempPath() + fileName);

            Console.WriteLine("File Name: " + fileName);
            Console.WriteLine("Creation Date: " + creationDate.ToString());
            Console.WriteLine("Version Info: " + myFileVersionInfo.FileVersion);
            
                // If the file is the required update (TODO this check) Run the file (teh file shoudl run the setup in silent mode
                Process.Start(Path.GetTempPath() + fileName);
                // Wait for the process to finish
                Process.WaitForExit()

            // Delete the temporary file
            File.Delete(Path.GetTempPath() + fileName);
                }
                 */

                // Get the list of all files in the folder.
                string[] files = Directory.GetFiles(folderPath, "*.exe");

                // Get the creation date and version of each file.
                DateTime[] creationDates = new DateTime[files.Length];
                string[] versions = new string[files.Length];
                string[] descr = new string[files.Length];
                for (int i = 0; i < files.Length; i++)
                {
                    creationDates[i] = File.GetCreationTime(files[i]);
                    versions[i] = FileVersionInfo.GetVersionInfo(files[i]).FileVersion;
                    descr[i] = FileVersionInfo.GetVersionInfo(files[i]).FileDescription;
                }

                Debug.WriteLine("currentVersion" + currentVersion);
                // Display the creation date and version of each file.
                for (int i = 0; i < files.Length; i++)
                {
                    Debug.WriteLine("File: " + files[i] + ", Creation Date: " + creationDates[i] + ", Version: " + versions[i]+ ", Descr: " + descr[i]);
                    if (!string.IsNullOrEmpty(versions[i]) && currentVersion.CompareTo(versions[i].Trim()) < 0)
                    {
                        // perform install this update
                        Debug.WriteLine("Hurray");
                    }
                }
            }
            else
            {
                // No Connection to the Internet
                MessageBox.Show("Please check your Internet Connection and then try again", "No Connection to the Internet", MessageBoxButton.OK, MessageBoxImage.Warning);
                // The folder is wrong.
                Debug.WriteLine("No Connection to the Internet. Path used: " + folderPath);
            }
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
