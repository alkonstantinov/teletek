using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ProstePrototype.WpfControls
{
    /// <summary>
    /// Interaction logic for WPFSelectInput.xaml
    /// </summary>
    public partial class WPFSelectInput : UserControl
    {
        public static readonly DependencyProperty LabelProperty =
            DependencyProperty.Register("Label", typeof(string),
            typeof(WPFSelectInput));
        public string Label
        {
            get { return (string)GetValue(LabelProperty); }
            set { SetValue(LabelProperty, value); }
        }

        public static readonly DependencyProperty LabelFontSizeProperty =
            DependencyProperty.Register("LabelFontSize", typeof(int),
            typeof(WPFSelectInput));
        public int LabelFontSize
        {
            get { return (int)GetValue(LabelFontSizeProperty); }
            set { SetValue(LabelFontSizeProperty, value); }
        }

        public static readonly DependencyProperty IsSelectedValueProperty =
            DependencyProperty.Register("IsSelectedValue", typeof(string),
            typeof(WPFSelectInput));
        public string IsSelectedValue
        {
            get { return (string)GetValue(IsSelectedValueProperty); }
            set { SetValue(IsSelectedValueProperty, value); }
        }

        public static readonly DependencyProperty ValueListProperty =
            DependencyProperty.Register(
                nameof(ValueList), //or simply "ValueList", 
                typeof(IEnumerable<string>),
                typeof(WPFSelectInput),
                new PropertyMetadata(default(IEnumerable<string>)));
        public IEnumerable<string> ValueList
        {
            get { return (IEnumerable<string>)GetValue(ValueListProperty); }
            set { SetValue(ValueListProperty, value); }
        }

        public WPFSelectInput()
        {
            InitializeComponent();

            this.DataContext = this;
        }
    }
}
