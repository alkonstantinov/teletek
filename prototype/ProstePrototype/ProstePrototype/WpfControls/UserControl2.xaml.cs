using HidSharp;
using lcommunicate;
using System;
using System.Collections.Generic;
using System.Text;
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
    /// Interaction logic for UserControl2.xaml
    /// </summary>
    public partial class UserControl2 : UserControl
    {
        private readonly Dictionary<string, HidDevice> devices = new Dictionary<string, HidDevice>();
        public UserControl2()
        {
            InitializeComponent();

            this.DataContext = this;

            devices = cComm.ScanHID();

            foreach(var device in devices) {
                ListBoxItem item = new ListBoxItem();
                item.FontSize = 16;
                item.Padding = new Thickness(30, 15, 30, 15);
                item.FontFamily = (FontFamily)Application.Current.Resources["poppins_regular"];
                item.BorderThickness = new Thickness(0, 0, 0, 1);
                item.BorderBrush = Brushes.Gray;
                item.Content = device.Key;
                item.Tag = device.Value;

                listBox.Items.Add(item);
            }

        }
        public object USBDevice
        {
            get
            {
                if (listBox.SelectedItem != null && listBox.SelectedItem is ListBoxItem)
                {
                    var item = (ListBoxItem)listBox.SelectedItem;
                    if (item.Tag != null && item.Tag is HidDevice)
                    {
                        return (HidDevice)item.Tag;
                    }
                }
                return null;
            }
        }
    }
}
