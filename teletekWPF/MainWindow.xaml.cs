using System;
using System.Collections.Generic;
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

namespace teletekWPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            // (sender as Button).Content = (sender as Button).Content;
            var btnCase = (sender as Button).Tag;
            switch (btnCase)
            {
                case "btnAccess": break;
                case "btnNetwork": break;
                case "btnPanels": break;
                case "btnInputs": break;
                case "btnInpGroups": break;
                case "btnOutputs": break;
                case "btnFatFBF": break;
                case "btnZones": break;
                case "btnEvacZone": break;
                case "btnPerifDevices": break;
                case "btnLoopDevices": break;
                default: break;
            };
            Console.WriteLine(e.ToString());
        }
    }
}
