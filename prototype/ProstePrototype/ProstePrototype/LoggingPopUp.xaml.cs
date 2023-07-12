using System.IO;
using System.Windows;
using System.Windows.Controls;

namespace ProstePrototype
{
    /// <summary>
    /// Interaction logic for LoggingPopUp.xaml
    /// </summary>
    public partial class LoggingPopUp : Window
    {
        public LoggingPopUp(string fileName)
        {
            InitializeComponent();
            LoadTextFile(fileName);
        }

        private void LoadTextFile(string fileName)
        {
            string filePath = System.IO.Path.Combine(Directory.GetCurrentDirectory(), fileName);
            if (File.Exists(filePath))
            {
                string fileContent = File.ReadAllText(filePath);
                txtContent.Text = fileContent;
            }
            else
            {
                MessageBox.Show($"File '{fileName}' not found in the current app location.");
            }
        }
    }
}
