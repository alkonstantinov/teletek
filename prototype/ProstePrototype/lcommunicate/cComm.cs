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
    public class cTransport
    {
        //private byte[] _ver_cmd = new byte[] { 0x07, 0x51, 0x12, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };
        internal byte[] _ver_cmd = new byte[] { 0x03, 0x51, 0x11, 0x00, 0x00, 0x07 };
        internal byte[] _cmd = new byte[] { 0x03, 0x51, 0x0F, 0x00, 0x00, 0x89 };
        internal byte[] _time_cmd = new byte[] { 0x00, 0x23, 0xFF };
        internal byte[] _panel_in_nework_0_cmd = new byte[] { 0x03, 0x51, 0x16, 0x00, 0x00, 0x60 };
        //
        internal object _conn = null;
        //
        internal virtual object Connect(object o) { return null; }
        internal virtual void Close(object o) { }
        internal virtual void Close() { }
        internal virtual byte[] SendCommand(object _connection, byte[] _command) { return null; }
        internal virtual byte[] SendCommand(object _connection, string _command) { return null; }
        internal virtual byte[] SendCommand(byte[] _command) { return null; }
        internal virtual byte[] SendCommand(string _command) { return null; }
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
        /// <summary>
        /// Dictionary с ключ ID на панел и съдържание Dictionary<~path, Dictionary с шаблони>.
        /// Dictionary с шаблони е с ключ индекса и елемент псевдо-списъчни елементи(NO_LOOP за IRIS).
        /// </summary>
        private static Dictionary<string, Dictionary<string, Dictionary<string, string>>> _cache_pseudo_element_panels;
        private static object _cs_pseudo_element_cache = new object();

        public static void ClearCache()
        {
            Monitor.Enter(_cs_cache);
            Monitor.Enter(_cs_pseudo_element_cache);
            //
            if (_cache_panels != null)
                _cache_panels.Clear();
            if (_cache_list_panels != null)
                _cache_list_panels.Clear();
            if (_cache_pseudo_element_panels != null)
                _cache_pseudo_element_panels.Clear();
            //
            Monitor.Exit(_cs_pseudo_element_cache);
            Monitor.Exit(_cs_cache);
        }

        #region path values
        public static void SetPathValue(string panel_id, string path, string value, dFilterValueChanged filter)
        {
            Monitor.Enter(_cs_cache);
            filter(path, value);
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
        public static Dictionary<string, string> GetPathValues(string panel_id)
        {
            Dictionary<string, string> res = new Dictionary<string, string>();
            Monitor.Enter(_cs_cache);
            if (_cache_panels != null && _cache_panels.ContainsKey(panel_id))
            {
                Dictionary<string, string> panel_paths = _cache_panels[panel_id];
                foreach (string key in panel_paths.Keys)
                    res.Add(key, panel_paths[key]);
            }
            Monitor.Exit(_cs_cache);
            return res;
        }
        public static void RemovePathValue(string panel_id, string path)
        {
            Monitor.Enter(_cs_cache);
            if (_cache_panels != null && _cache_panels.ContainsKey(panel_id))
            {
                Dictionary<string, string> panel = _cache_panels[panel_id];
                if (panel != null && panel.ContainsKey(path))
                    panel.Remove(path);
            }
            Monitor.Exit(_cs_cache);
        }
        #endregion

        #region pseudo elements
        /// <summary>
        /// Добавя в кеша шаблон на псевдо-списъчен елемент(NO_LOOP1, 2, 3...).
        /// В последствие този шаблон се ползва за пренасочване към физическото устройство.
        /// </summary>
        /// <param name="panel_id">ID на панела
        /// <param name="key">ID на елемента(напр. "IRIS8_NO_LOOP")</param>
        /// <param name="idx">Индекс, на който се добавя елемента</param>
        /// <param name="_template">Шаблон на елемента.</param>
        public static void AddPseudoElement(string panel_id, string key, string idx, string _template)
        {
            Monitor.Enter(_cs_pseudo_element_cache);
            if (_cache_pseudo_element_panels == null)
                _cache_pseudo_element_panels = new Dictionary<string, Dictionary<string, Dictionary<string, string>>>();
            if (!_cache_pseudo_element_panels.ContainsKey(panel_id))
                _cache_pseudo_element_panels.Add(panel_id, new Dictionary<string, Dictionary<string, string>>());
            Dictionary<string, Dictionary<string, string>> el = _cache_pseudo_element_panels[panel_id];
            if (!el.ContainsKey(key))
                el.Add(key, new Dictionary<string, string>());
            Dictionary<string, string> lst = el[key];
            if (!lst.ContainsKey(idx))
                lst.Add(idx, _template);
            Monitor.Exit(_cs_pseudo_element_cache);
        }
        public static void AddPseudoElement(string panel_id, string key, string _template)
        {
            Monitor.Enter(_cs_pseudo_element_cache);
            if (_cache_pseudo_element_panels == null)
                _cache_pseudo_element_panels = new Dictionary<string, Dictionary<string, Dictionary<string, string>>>();
            if (!_cache_pseudo_element_panels.ContainsKey(panel_id))
                _cache_pseudo_element_panels.Add(panel_id, new Dictionary<string, Dictionary<string, string>>());
            Dictionary<string, Dictionary<string, string>> el = _cache_pseudo_element_panels[panel_id];
            if (!el.ContainsKey(key))
                el.Add(key, new Dictionary<string, string>());
            Dictionary<string, string> lst = el[key];
            string idx = (lst.Count > 0) ? lst.Keys.Last() : "0";
            idx = (Convert.ToInt32(idx) + 1).ToString();
            if (!lst.ContainsKey(idx))
                lst.Add(idx, _template);
            Monitor.Exit(_cs_pseudo_element_cache);
        }
        public static bool PseudoElementExists(string panel_id)
        {
            bool res = false;
            Monitor.Enter(_cs_pseudo_element_cache);
            if (_cache_pseudo_element_panels != null)
            {
                if (_cache_pseudo_element_panels.ContainsKey(panel_id))
                {
                    Dictionary<string, Dictionary<string, string>> el = _cache_pseudo_element_panels[panel_id];
                    res = el.Count > 0;
                }
            }
            Monitor.Exit(_cs_pseudo_element_cache);
            //
            return res;
        }
        public static bool PseudoElementExists(string panel_id, string typ, string re_mask)
        {
            bool res = false;
            Monitor.Enter(_cs_pseudo_element_cache);
            if (_cache_pseudo_element_panels != null)
            {
                if (_cache_pseudo_element_panels.ContainsKey(panel_id))
                {
                    Dictionary<string, Dictionary<string, string>> el = _cache_pseudo_element_panels[panel_id];
                    if (el.ContainsKey(typ))
                    {
                        Dictionary<string, string> lst = el[typ];
                        foreach (string key in lst.Keys)
                        {
                            string val = lst[key];
                            Match m = Regex.Match(val, @"""~loop_type""\s*?:\s*?""([\w\W]+?)""", RegexOptions.IgnoreCase);
                            if (m.Success)
                            {
                                bool f = Regex.IsMatch(m.Groups[1].Value, re_mask, RegexOptions.IgnoreCase);
                                if (f)
                                {
                                    res = true;
                                    break;
                                }
                            }
                        }
                    }
                }
            }
            Monitor.Exit(_cs_pseudo_element_cache);
            //
            return res;
        }
        public static string GetPseudoElement(string panel_id, string key, string idx)
        {
            Monitor.Enter(_cs_pseudo_element_cache);
            if (_cache_pseudo_element_panels == null)
            {
                Monitor.Exit(_cs_pseudo_element_cache);
                return null;
            }
            if (!_cache_pseudo_element_panels.ContainsKey(panel_id))
            {
                Monitor.Exit(_cs_pseudo_element_cache);
                return null;
            }
            Dictionary<string, Dictionary<string, string>> el = _cache_pseudo_element_panels[panel_id];
            if (!el.ContainsKey(key))
            {
                Monitor.Exit(_cs_pseudo_element_cache);
                return null;
            }
            Dictionary<string, string> lst = el[key];
            if (!lst.ContainsKey(idx))
            {
                Monitor.Exit(_cs_pseudo_element_cache);
                return null;
            }
            string _template = lst[idx];
            Monitor.Exit(_cs_pseudo_element_cache);
            return _template;
        }
        public static string GetPseudoElement(string panel_id, string key)
        {
            Monitor.Enter(_cs_pseudo_element_cache);
            if (_cache_pseudo_element_panels == null)
            {
                Monitor.Exit(_cs_pseudo_element_cache);
                return null;
            }
            if (!_cache_pseudo_element_panels.ContainsKey(panel_id))
            {
                Monitor.Exit(_cs_pseudo_element_cache);
                return null;
            }
            Dictionary<string, Dictionary<string, string>> el = _cache_pseudo_element_panels[panel_id];
            if (!el.ContainsKey(key))
            {
                Monitor.Exit(_cs_pseudo_element_cache);
                return null;
            }
            Dictionary<string, string> lst = el[key];
            string idx = lst.Keys.Last();
            if (!lst.ContainsKey(idx))
            {
                Monitor.Exit(_cs_pseudo_element_cache);
                return null;
            }
            string _template = lst[idx];
            Monitor.Exit(_cs_pseudo_element_cache);
            return _template;
        }
        public static Dictionary<string, string> GetPseudoElementsList(string panel_id, string key)
        {
            Monitor.Enter(_cs_pseudo_element_cache);
            if (_cache_pseudo_element_panels == null)
            {
                Monitor.Exit(_cs_pseudo_element_cache);
                return null;
            }
            if (!_cache_pseudo_element_panels.ContainsKey(panel_id))
            {
                Monitor.Exit(_cs_pseudo_element_cache);
                return null;
            }
            Dictionary<string, Dictionary<string, string>> el = _cache_pseudo_element_panels[panel_id];
            if (!el.ContainsKey(key))
            {
                Monitor.Exit(_cs_pseudo_element_cache);
                return null;
            }
            Dictionary<string, string> lst = el[key];
            Monitor.Exit(_cs_pseudo_element_cache);
            return lst;
        }
        public static void SetPseudoElement(string panel_id, string key, string idx, string _template)
        {
            Monitor.Enter(_cs_pseudo_element_cache);
            if (_cache_pseudo_element_panels == null)
            {
                Monitor.Exit(_cs_pseudo_element_cache);
                return;
            }
            if (!_cache_pseudo_element_panels.ContainsKey(panel_id))
            {
                Monitor.Exit(_cs_pseudo_element_cache);
                return;
            }
            Dictionary<string, Dictionary<string, string>> el = _cache_pseudo_element_panels[panel_id];
            if (!el.ContainsKey(key))
            {
                Monitor.Exit(_cs_pseudo_element_cache);
                return;
            }
            Dictionary<string, string> lst = el[key];
            if (!lst.ContainsKey(idx))
            {
                Monitor.Exit(_cs_pseudo_element_cache);
                return;
            }
            lst[idx] = _template;
            Monitor.Exit(_cs_pseudo_element_cache);
        }
        public static void SetPseudoElement(string panel_id, string key, string _template)
        {
            Monitor.Enter(_cs_pseudo_element_cache);
            if (_cache_pseudo_element_panels == null)
            {
                Monitor.Exit(_cs_pseudo_element_cache);
                return;
            }
            if (!_cache_pseudo_element_panels.ContainsKey(panel_id))
            {
                Monitor.Exit(_cs_pseudo_element_cache);
                return;
            }
            Dictionary<string, Dictionary<string, string>> el = _cache_pseudo_element_panels[panel_id];
            if (!el.ContainsKey(key))
            {
                Monitor.Exit(_cs_pseudo_element_cache);
                return;
            }
            Dictionary<string, string> lst = el[key];
            string idx = lst.Keys.Last();
            if (!lst.ContainsKey(idx))
            {
                Monitor.Exit(_cs_pseudo_element_cache);
                return;
            }
            lst[idx] = _template;
            Monitor.Exit(_cs_pseudo_element_cache);
        }
        public static Dictionary<string, string> GetPseudoElementDevices(string panel_id, string key, string idx)
        {
            Monitor.Enter(_cs_cache);
            if (_cache_list_panels == null)
            {
                Monitor.Exit(_cs_cache);
                return null;
            }
            if (!_cache_list_panels.ContainsKey(panel_id))
            {
                Monitor.Exit(_cs_cache);
                return null;
            }
            Dictionary<string, Dictionary<string, string>> el = _cache_list_panels[panel_id];
            Dictionary<string, string> res = new Dictionary<string, string>();
            string loopkey = Regex.Replace(key, @"^[^_]*?_", "");
            while (Regex.IsMatch(loopkey, "_"))
                loopkey = Regex.Replace(loopkey, @"^[^_]*?_", "");
            loopkey += idx;
            foreach (string elkey in el.Keys)
            {
                string loop = Regex.Replace(elkey, @"/[\w\W]+$", "");
                if (Regex.IsMatch(loop, loopkey + "$"))
                    foreach (string addr in el[elkey].Keys)
                        res.Add(addr, el[elkey][addr]);
            }
            Monitor.Exit(_cs_cache);
            return res;
        }
        #endregion

        #region list elements
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
        public static Dictionary<string, string> GetElements(string panel_id, string key)
        {
            Dictionary<string, string> res = null;
            Monitor.Enter(_cs_cache);
            if (_cache_list_panels != null)
            {
                if (_cache_list_panels.ContainsKey(panel_id))
                {
                    Dictionary<string, Dictionary<string, string>> el = _cache_list_panels[panel_id];
                    if (el.ContainsKey(key))
                        res = el[key];
                }
            }
            Monitor.Exit(_cs_cache);
            return res;
        }
        /// <summary>
        /// Връща кеширана структура със стойности за списъчни обекти. Съответния обект е добавен с AddListElement.
        /// </summary>
        /// <param name="panel_id">ID на панела
        /// <param name="key">ID на елемента(напр. "IRIS8_PANELINNETWORK")</param>
        /// <param name="idx">Индекс, на който се добавя елемента</param>
        /// <returns>Попълнена със стойности кеширана структура</returns>
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
        public static void SetListElementJson(string panel_id, string key, string idx, string _template)
        {
            Monitor.Enter(_cs_cache);
            if (_cache_list_panels.ContainsKey(panel_id))
            {
                Dictionary<string, Dictionary<string, string>> el = _cache_list_panels[panel_id];
                if (el.ContainsKey(key))
                {
                    Dictionary<string, string> lst = el[key];
                    if (lst.ContainsKey(idx))
                        lst[idx] = _template;
                }
            }
            Monitor.Exit(_cs_cache);
        }
        /// <summary>
        /// Връща елемент от кеширана структура със стойности за списъчни обекти или от псевдо-елементите. Съответния обект е добавен с AddListElement или AddPseudoElement.
        /// </summary>
        /// <param name="panel_id">ID на панела
        /// <param name="key">ID на елемента(напр. "IRIS8_PANELINNETWORK")</param>
        /// <param name="idx">Индекс, на който се добавя елемента</param>
        /// <param name="node">Entity ключ</param>
        /// <returns>Entity  по ключ "node"</returns>
        public static string GetListElementNode(string panel_id, string key, string idx, string node)
        {
            string res = null;
            Monitor.Enter(_cs_cache);
            if (_cache_list_panels != null && _cache_list_panels.ContainsKey(panel_id))
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
            if (res == null)
            {
                Monitor.Enter(_cs_pseudo_element_cache);
                if (_cache_pseudo_element_panels.ContainsKey(panel_id))
                {
                    Dictionary<string, Dictionary<string, string>> el = _cache_pseudo_element_panels[panel_id];
                    if (!el.ContainsKey(key))
                        key += idx;
                    if (el.ContainsKey(key))
                    {
                        Dictionary<string, string> lst = el[key];
                        if (lst.ContainsKey(idx))
                            res = lst[idx];
                    }
                }
                Monitor.Exit(_cs_pseudo_element_cache);
            }
            if (res != null)
            {
                JObject obj = JObject.Parse(res);
                JToken t = obj[node];
                if (t == null)
                    t = obj[node.ToUpper()];
                if (t != null)
                    res = t.ToString();
            }
            return (res != null) ? res : "{}";
        }
        #endregion

        #region trensport
        public static cTransport ConnectIP()
        {
            cTransport t = new cIP();
            object conn = t.Connect(new cIPParams("192.168.17.17", 7000));
            if (conn != null)
                return t;
            else
                return null;
        }
        public static cTransport ConnectIP(string ip, int port)
        {
            cTransport t = new cIP();
            object conn = t.Connect(new cIPParams(ip, port));
            if (conn != null)
                return t;
            else
                return null;
        }
        public static cTransport ConnectFile(string filename)
        {
            cTransport t = new cFile();
            object conn = t.Connect(filename);
            if (conn != null)
                return t;
            else
                return null;
        }
        public static byte[] SendCommand(cTransport conn, string cmd)
        {
            byte[] res = conn.SendCommand(cmd);
            return res;
        }

        public static void CloseConnection(cTransport conn)
        {
            conn.Close();
        }
        #endregion

        public static JArray Scan()
        {
            cTransport t = new cIP();
            //object conn = t.Connect(new cIPParams("92.247.2.162", 7000));
            ////byte[] res = t.SendCommand(conn, t._ver_cmd);
            //byte[] res = t.SendCommand(conn, t._panel_in_nework_0_cmd);
            //t.Close(conn);
            return JArray.Parse("[{ deviceType: 'fire', schema: 'iris', title: 'IRIS', interface: 'IP', address: '92.247.2.162:7000'}, { deviceType: 'fire', schema: 'iris8', title: 'IRIS8', interface: 'IP', address: '212.36.21.86:7000'}, { deviceType: '', schema: 'eclipse', title: 'ECLIPSE99', interface: 'COM', address: 'COM1'}]");
        }
    }
}
