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

namespace ProstePrototype.WpfControls
{
    /// <summary>
    /// Interaction logic for WPFIPInput.xaml
    /// </summary>
    public partial class WPFIPInput : UserControl
    {
        public event EventHandler AddressChanged;

        private static readonly List<Key> DigitKeys = new List<Key> { 
            Key.D0, Key.D1, Key.D2, Key.D3, Key.D4, Key.D5, Key.D6, Key.D7, Key.D8, Key.D9, 
            Key.NumPad0, Key.NumPad1, Key.NumPad2, Key.NumPad3, Key.NumPad4, Key.NumPad5, Key.NumPad6, Key.NumPad7, Key.NumPad8, Key.NumPad9
        };
        private static readonly List<Key> MoveForwardKeys = new List<Key> { Key.Right };
        private static readonly List<Key> MoveBackwardKeys = new List<Key> { Key.Left };
        private static readonly List<Key> OtherAllowedKeys = new List<Key> { Key.Tab, Key.Delete };

        private readonly List<TextBox> _segments = new List<TextBox>();
        private bool _suppressAddressUpdate = false;
        public WPFIPInput()
        {
            InitializeComponent();
            _segments.Add(FirstSegment);
            _segments.Add(SecondSegment);
            _segments.Add(ThirdSegment);
            _segments.Add(LastSegment);
            this.DataContext = this;
        }

        public static readonly DependencyProperty LabelProperty =
            DependencyProperty.Register("Label", typeof(string),
            typeof(WPFIPInput));
        public string Label
        {
            get { return (string)GetValue(LabelProperty); }
            set { SetValue(LabelProperty, value); }
        }

        public static readonly DependencyProperty LabelFontSizeProperty =
            DependencyProperty.Register("LabelFontSize", typeof(int),
            typeof(WPFIPInput));
        public int LabelFontSize
        {
            get { return (int)GetValue(LabelFontSizeProperty); }
            set { SetValue(LabelFontSizeProperty, value); }
        }

        public static readonly DependencyProperty AddressProperty = DependencyProperty.Register(
            "Address", typeof(string), typeof(WPFIPInput), new FrameworkPropertyMetadata(default(string), OnAddressChanged)
            {
                BindsTwoWayByDefault = true
            });

        private static void OnAddressChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
        {
            var ipTextBox = dependencyObject as WPFIPInput;
            var text = e.NewValue as string;

            if (text != null && ipTextBox != null)
            {
                ipTextBox._suppressAddressUpdate = true;
                var i = 0;
                foreach (var segment in text.Split('.'))
                {
                    if (ipTextBox._segments.Count > i)
                        ipTextBox._segments[i].Text = segment;
                    i++;
                }
                ipTextBox._suppressAddressUpdate = false;
                ipTextBox.OnAddressChanged(e);
            }
        }

        protected virtual void OnAddressChanged(DependencyPropertyChangedEventArgs e)
        {
            // Raise the ValueChanged event
            AddressChanged?.Invoke(this, EventArgs.Empty);
        }

        public string Address
        {
            get { return (string)GetValue(AddressProperty); }
            set { SetValue(AddressProperty, value); }
        }

        private void MouseEnter_Event(object sender, MouseEventArgs e)
        {            
            mainBorder.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#dae4f0"));
        }
        private void MouseLeave_Event(object sender, MouseEventArgs e)
        {
            if (!FirstSegment.IsFocused && !SecondSegment.IsFocused && !ThirdSegment.IsFocused && !LastSegment.IsFocused)
            {
                mainBorder.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("White"));
            }
        }

        private void UIElement_GotFocus(object sender, RoutedEventArgs e)
        {
            lbHost.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("Black"));
            TextBox SenderElement = (TextBox)sender;
            SenderElement.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("Blue"));
            mainBorder.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#dae4f0"));
        }

        private void UIElement_LostFocus(object sender, RoutedEventArgs e)
        {
            lbHost.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("Gray"));
            TextBox SenderElement = (TextBox)sender;
            SenderElement.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("Gray"));
            mainBorder.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("White"));
        }

        private void UIElement_OnPreviewKeyDown(object sender, KeyEventArgs e)
        {
            var currentTextBox = (TextBox)sender;
            if (DigitKeys.Contains(e.Key))
            {
                e.Handled = ShouldCancelDigitKeyPress(currentTextBox);
                if (e.Handled == true) MoveFocusToNextSegment(currentTextBox); // HandleDigitPress(currentTextBox); // left ofr validation logic?
            }
            else if (MoveBackwardKeys.Contains(e.Key))
            {
                e.Handled = ShouldCancelBackwardKeyPress(currentTextBox);
                if (e.Handled == true && currentTextBox.SelectedText.Length == 0) 
                    MoveFocusToPreviousSegment(currentTextBox); // HandleBackwardKeyPress(currentTextBox);
            }
            else if (MoveForwardKeys.Contains(e.Key))
            {
                e.Handled = ShouldCancelForwardKeyPress(currentTextBox);
                HandleForwardKeyPress(currentTextBox);
            }
            else if (e.Key == Key.Back)
            {
                HandleBackspaceKeyPress(currentTextBox);
            }
            else if (e.Key == Key.OemPeriod || e.Key == Key.Decimal)
            {
                e.Handled = true;
                HandlePeriodKeyPress(currentTextBox);
            }
            else
            {
                e.Handled = !AreOtherAllowedKeysPressed(e);
            }
        }

        private bool AreOtherAllowedKeysPressed(KeyEventArgs e)
        {
            return e.Key == Key.C && ((e.KeyboardDevice.Modifiers & ModifierKeys.Control) != 0) ||
                   e.Key == Key.V && ((e.KeyboardDevice.Modifiers & ModifierKeys.Control) != 0) ||
                   e.Key == Key.A && ((e.KeyboardDevice.Modifiers & ModifierKeys.Control) != 0) ||
                   e.Key == Key.X && ((e.KeyboardDevice.Modifiers & ModifierKeys.Control) != 0) ||
                   OtherAllowedKeys.Contains(e.Key);
        }

        private void HandleDigitPress(TextBox currentTextBox)
        {
            if (currentTextBox != null && currentTextBox.Text.Length == 3 &&
                currentTextBox.CaretIndex == 3 && currentTextBox.SelectedText.Length == 0)
            {
                MoveFocusToNextSegment(currentTextBox);
            }
        }

        private bool ShouldCancelDigitKeyPress(TextBox currentTextBox)
        {
            return currentTextBox != null &&
                   currentTextBox.Text.Length == 3 &&
                   currentTextBox.CaretIndex == 3 &&
                   currentTextBox.SelectedText.Length == 0;
        }

        private void TextBoxBase_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            if (!_suppressAddressUpdate)
            {
                Address = string.Format("{0}.{1}.{2}.{3}", (FirstSegment != null) ? FirstSegment.Text : "0", (SecondSegment != null) ? SecondSegment.Text : "0", (ThirdSegment != null) ? ThirdSegment.Text : "0", (LastSegment != null) ? LastSegment.Text : "0");
            }

            var currentTextBox = sender as TextBox;

            if (currentTextBox != null && currentTextBox.Text.Length == 3 && currentTextBox.CaretIndex == 3)
            {
                MoveFocusToNextSegment(currentTextBox);
            }
        }

        private bool ShouldCancelBackwardKeyPress(TextBox currentTextBox)
        {
            return currentTextBox != null && currentTextBox.CaretIndex == 0;
        }

        private void HandleBackspaceKeyPress(TextBox currentTextBox)
        {
            if (currentTextBox != null && currentTextBox.CaretIndex == 0 && currentTextBox.SelectedText.Length == 0)
            {
                MoveFocusToPreviousSegment(currentTextBox);
            }
        }

        private void HandleBackwardKeyPress(TextBox currentTextBox)
        {
            if (currentTextBox != null && currentTextBox.CaretIndex == 0)
            {
                MoveFocusToPreviousSegment(currentTextBox);
            }
        }

        private bool ShouldCancelForwardKeyPress(TextBox currentTextBox)
        {
            return currentTextBox != null && currentTextBox.CaretIndex == 3;
        }

        private void HandleForwardKeyPress(TextBox currentTextBox)
        {
            if (currentTextBox != null && currentTextBox.CaretIndex == currentTextBox.Text.Length)
            {
                MoveFocusToNextSegment(currentTextBox);
            }
        }

        private void HandlePeriodKeyPress(TextBox currentTextBox)
        {
            //var currentTextBox = FocusManager.GetFocusedElement(this) as TextBox;
            if (currentTextBox != null && currentTextBox.Text.Length > 0 && currentTextBox.CaretIndex == currentTextBox.Text.Length)
            {
                MoveFocusToNextSegment(currentTextBox);
            }
        }

        private void MoveFocusToPreviousSegment(TextBox currentTextBox)
        {
            if (!ReferenceEquals(currentTextBox, FirstSegment))
            {
                var previousSegmentIndex = _segments.FindIndex(box => ReferenceEquals(box, currentTextBox)) - 1;
                currentTextBox.SelectionLength = 0;
                currentTextBox.MoveFocus(new TraversalRequest(FocusNavigationDirection.Previous));
                _segments[previousSegmentIndex].CaretIndex = _segments[previousSegmentIndex].Text.Length;
            }
        }

        private void MoveFocusToNextSegment(TextBox currentTextBox)
        {
            if (!ReferenceEquals(currentTextBox, LastSegment))
            {
                currentTextBox.SelectionLength = 0;
                currentTextBox.MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
            }
        }

        private void DataObject_OnPasting(object sender, DataObjectPastingEventArgs e)
        {
            var isText = e.SourceDataObject.GetDataPresent(DataFormats.UnicodeText, true);
            if (!isText)
            {
                e.CancelCommand();
                return;
            }

            var text = e.SourceDataObject.GetData(DataFormats.UnicodeText) as string;

            int num;

            if (!int.TryParse(text, out num))
            {
                e.CancelCommand();
            }
        }
    }
}
