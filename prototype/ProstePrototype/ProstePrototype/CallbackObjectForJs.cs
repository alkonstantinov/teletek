using Newtonsoft.Json.Linq;
using ProstePrototype.POCO;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Markup;

namespace ProstePrototype
{
    public class CallbackObjectForJs
    {
        public string showMessage(string msg)
        {//Read Note
            // MessageBox.Show(msg);
            return "Hi from " + msg;
        }

        public string getJsonForElement(string elementType, int elementNumber) {

            var e = elementType;
            return @"{ ""pageName"": ""wb1"" }";
        }
    }
}
