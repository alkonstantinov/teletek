using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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
    /// Interaction logic for UserControl3.xaml
    /// </summary>
    public partial class UserControl3 : UserControl
    {
        public string[] StringArray1 { get; set; } //
        public int IsSelectedValue1 { get; set; }
        public string[] StringArray2 { get; set; } //
        public int IsSelectedValue2 { get; set; }
        public string[] StringArray3 { get; set; } //
        public int IsSelectedValue3 { get; set; }
        public string[] StringArray4 { get; set; } //
        public int IsSelectedValue4 { get; set; }
        
        public UserControl3()
        {
            InitializeComponent();

            StringArray1 = new[] { (string)Application.Current.Resources["ScanMenuRSComChoice"], "1", "3", "9" };
            IsSelectedValue1= 0;

            StringArray2 = new[] { 
                (string)Application.Current.Resources["ScanMenuRSParityChoice"], 
                (string)Application.Current.Resources["ScanMenuRSParityChoiceNone"], 
                (string)Application.Current.Resources["ScanMenuRSParityChoiceEven"], 
                (string)Application.Current.Resources["ScanMenuRSParityChoiceMark"], 
                (string)Application.Current.Resources["ScanMenuRSParityChoiceOdd"], 
                (string)Application.Current.Resources["ScanMenuRSParityChoiceSpace"]
            };
            IsSelectedValue2= 0;

            StringArray3 = new[] { (string)Application.Current.Resources["ScanMenuRSBaudRateChoice"], "2400", "4800", "9600", "19200", "38400" };
            IsSelectedValue3 = 0;

            StringArray4 = new[] { (string)Application.Current.Resources["ScanMenuRSStopBitsChoice"], "0", "1", "1.5", "2" };
            IsSelectedValue4 = 0;

            this.DataContext = this;
        }
        
    }
}
