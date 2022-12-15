using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;

namespace ProstePrototype
{
    public class CallbackObjectForJs
    {
        public string showMessage(string msg)
        {//Read Note
            // MessageBox.Show(msg);
            return "Hi from " + msg;
        }
    }
}
