using System;
using System.Collections.Generic;
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
    /// Interaction logic for UserControl1.xaml
    /// </summary>
    public partial class UserControl1 : UserControl
    {
        public UserControl1()
        {
            InitializeComponent();
            this.DataContext = this;
            txtHostIp.Address = Properties.Settings.Default.TCPHostIp;
            txtPort.Value = Properties.Settings.Default.TCPPort;

            // Add event handlers for ValueChanged and AddressChanged events
            txtPort.ValueChanged += TxtPort_ValueChanged;
            txtHostIp.AddressChanged += TxtHostIp_AddressChanged;
        }

        public string Address
        {
            get
            {
                return txtHostIp.Address.ToString();
            }
        }

        public int Port
        {
            get
            {
                return Convert.ToInt32(txtPort.Value);
            }
        }

        private void TxtHostIp_AddressChanged(object sender, EventArgs e)
        {
            // Update Properties.Settings.Default.TCPHostIp when the Address property changes
            Properties.Settings.Default.TCPHostIp = txtHostIp.Address.ToString();
            Properties.Settings.Default.Save();
        }

        private void TxtPort_ValueChanged(object sender, EventArgs e)
        {
            // Update Properties.Settings.Default.TCPPort when the Value property changes
            Properties.Settings.Default.TCPPort = txtPort.Value;
            Properties.Settings.Default.Save();
        }
}
}
