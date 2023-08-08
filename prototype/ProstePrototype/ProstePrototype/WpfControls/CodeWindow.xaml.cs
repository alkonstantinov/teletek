using System;
using System.Collections.Generic;
using System.ComponentModel.Composition.Primitives;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace ProstePrototype.WpfControls
{
    /// <summary>
    /// Interaction logic for CodeWindow.xaml
    /// </summary>
    public partial class CodeWindow : Window
    {
        public CodeWindow()
        {
            InitializeComponent();
            this.DataContext = this;
            codeField.dataBitsText.Password = null;
            Dispatcher.BeginInvoke(new Action(() => { codeField.dataBitsText.Focus(); }), System.Windows.Threading.DispatcherPriority.Loaded);
        }

        //private void CodeWindow_Loaded(object sender, RoutedEventArgs e)
        //{
        //    codeField.dataBitsText.Focus();
        //}

        public string Code
        {
            get
            {
                return Convert.ToString(codeField.dataBitsText.Password);
            }
        }

        private void OKCode_Clicked(object sender, RoutedEventArgs e)
        {
            this.Dispatcher.Invoke(() =>
            {
                this.DialogResult = true;
                this.Hide();
            });
        }

        private void CancelCode_Clicked(object sender, RoutedEventArgs e)
        {
            this.Dispatcher.Invoke(() =>
            {
                this.DialogResult = false;
                this.Hide();
            });
        }
    }
}
