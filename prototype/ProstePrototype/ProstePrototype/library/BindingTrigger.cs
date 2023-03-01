using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Windows.Data;
using System.Windows;

namespace ProstePrototype.library
{
    public class BindingTrigger : INotifyPropertyChanged
    {

        public BindingTrigger()
            => Binding = new Binding()
            {
                Source = this,
                Path = new PropertyPath(nameof(Value))
            };

        public event PropertyChangedEventHandler PropertyChanged;

        public Binding Binding { get; }

        public void Refresh()
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Value)));

        public object Value { get; }
    }
}
