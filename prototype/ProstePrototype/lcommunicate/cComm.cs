using Microsoft.Win32;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Xml;
using System.Xml.Serialization;
using System.Text.RegularExpressions;
using common;

namespace lcommunicate
{
    internal class cTransport
    {
        internal virtual object Connect(object o) { return null; }
    }
    public class cComm
    {
        public static JArray Scan()
        {
            cTransport t = new cIP();
            object conn = t.Connect(new cIPParams("212.36.21.86", 7000));
            return JArray.Parse("[{ deviceType: 'fire', schema: 'iris8', title: 'IRIS', interface: 'IP', address: '212.36.21.86:7000'}, { deviceType: '', schema: 'eclipse99', title: 'ECLIPSE', interface: 'COM', address: 'COM1'}]");
        }
    }
}
