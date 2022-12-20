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
        //private byte[] _ver_cmd = new byte[] { 0x07, 0x51, 0x12, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };
        internal byte[] _ver_cmd = new byte[] { 0x03, 0x51, 0x11, 0x00, 0x00, 0x07 };
        internal byte[] _cmd = new byte[] { 0x03, 0x51, 0x0F, 0x00, 0x00, 0x89 };
        internal byte[] _time_cmd = new byte[] { 0x00, 0x23, 0xFF };
        internal byte[] _panel_in_nework_0_cmd = new byte[] { 0x03, 0x51, 0x16, 0x00, 0x00, 0x60 };
        internal virtual object Connect(object o) { return null; }
        internal virtual void Close(object o) { }
        internal virtual byte[] SendCommand(object _connection, byte[] _command) { return null; }
    }
    public class cComm
    {
        /// <summary>
        /// Dictionary с ключ ID на панел и съдържание Dictionary<~path, value>
        /// </summary>
        private static Dictionary<string, Dictionary<string, string>> _cache_panels;
        /// <summary>
        /// Dictionary с ключ ID на панел и съдържание Dictionary<~path, Dictionary с шаблони>.
        /// Dictionary с шаблони е с ключ индекса и елемент Groups за съответния елемент с преправени пътища с добавен индекс.
        /// </summary>
        private static Dictionary<string, Dictionary<string, Dictionary<string, string>>> _cache_list_panels;
        private static object _cs_cache = new object();

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
            string res = null;
            Monitor.Enter(_cs_cache);
            if (_cache_panels != null && _cache_panels.ContainsKey(panel_id))
            {
                Dictionary<string, string> panel = _cache_panels[panel_id];
                if (panel != null && panel.ContainsKey(path))
                    res = panel[path];
            }
            Monitor.Exit(_cs_cache);
            return res;
        }

        /// <summary>
        /// Добавя в кеша шаблон на списъчен елемент(панел в мрежата, вход, изход...).
        /// В последствие този шаблон се ползва за съхраняване на стойности за съответния елемент.
        /// </summary>
        /// <param name="panel_id">ID на панела
        /// <param name="key">ID на елемента(напр. "IRIS8_PANELINNETWORK")</param>
        /// <param name="idx">Индекс, на който се добавя елемента</param>
        /// <param name="_template">Шаблон на елемента. Пътищата в него трябва да се преправят</param>
        public static void AddListElement(string panel_id, string key, string idx, string _template)
        {
            Monitor.Enter(_cs_cache);
            if (_cache_list_panels == null)
                _cache_list_panels = new Dictionary<string, Dictionary<string, Dictionary<string, string>>>();
            if (!_cache_list_panels.ContainsKey(panel_id))
                _cache_list_panels.Add(panel_id, new Dictionary<string, Dictionary<string, string>>());
            Dictionary<string, Dictionary<string, string>> el = _cache_list_panels[panel_id];
            if (!el.ContainsKey(key))
                el.Add(key, new Dictionary<string, string>());
            Dictionary<string, string> lst = el[key];
            if (!lst.ContainsKey(idx))
                lst.Add(idx, _template);
            Monitor.Exit(_cs_cache);
        }

        public static string GetListElement(string panel_id, string key, string idx)
        {
            string res = null;
            Monitor.Enter(_cs_cache);
            if (_cache_list_panels.ContainsKey(panel_id))
            {
                Dictionary<string, Dictionary<string, string>> el = _cache_list_panels[panel_id];
                if (el.ContainsKey(key))
                {
                    Dictionary<string, string> lst = el[key];
                    if (lst.ContainsKey(idx))
                        res = lst[idx];
                }
            }
            Monitor.Exit(_cs_cache);
            return (res != null) ? res : "{}";
        }
        public static JArray Scan()
        {
            cTransport t = new cIP();
            //object conn = t.Connect(new cIPParams("92.247.2.162", 7000));
            ////byte[] res = t.SendCommand(conn, t._ver_cmd);
            //byte[] res = t.SendCommand(conn, t._panel_in_nework_0_cmd);
            //t.Close(conn);
            return JArray.Parse("[{ deviceType: 'fire', schema: 'iris8', title: 'IRIS', interface: 'IP', address: '212.36.21.86:7000'}, { deviceType: '', schema: 'eclipse99', title: 'ECLIPSE', interface: 'COM', address: 'COM1'}]");
        }
    }
}
