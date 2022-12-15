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
        /// <summary>
        /// Dictionary с ключ ID на панел и съдържание Dictionary<~path, value>
        /// </summary>
        private static Dictionary<string, Dictionary<string, string>> _cache_panels;
        private static object _cs_cache;

        public static void SetPathValue(string panel_id, string path, string value)
        {
            Monitor.Enter(_cs_cache);
            if (_cache_panels == null)
                _cache_panels = new Dictionary<string, Dictionary<string, string>>();
            if (!_cache_panels.ContainsKey(panel_id))
                _cache_panels.Add(panel_id, new Dictionary<string, string>());
            Dictionary<string, string> panel = _cache_panels[panel_id];
            if (panel.ContainsKey(path))
                panel[path] = value;
            else
                panel.Add(path, value);
            Monitor.Exit(_cs_cache);
        }

        public static string GetPathValue(string panel_id, string path)
        {
            Monitor.Enter(_cs_cache);
            if (!_cache_panels.ContainsKey(panel_id))
                return null;
            Dictionary<string, string> panel = _cache_panels[panel_id];
            if (!panel.ContainsKey(path))
                return null;
            string val = panel[path];
            Monitor.Exit(_cs_cache);
            return val;
        }
        public static JArray Scan()
        {
            cTransport t = new cIP();
            object conn = t.Connect(new cIPParams("212.36.21.86", 7000));
            return JArray.Parse("[{ deviceType: 'fire', schema: 'iris8', title: 'IRIS', interface: 'IP', address: '212.36.21.86:7000'}, { deviceType: '', schema: 'eclipse99', title: 'ECLIPSE', interface: 'COM', address: 'COM1'}]");
        }
    }
}
