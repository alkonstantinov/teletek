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
using System.Windows.Input;
//using LibUsbDotNet;
//using LibUsbDotNet.Main;
using HidSharp;
using System.Security.Cryptography;
using System.IO.Ports;
//using LibUsbDotNet.Info;
//using System.Collections.ObjectModel;

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
        internal cPacker _packer = new cPacker();
        internal string _panel_type
        {
            set
            {
                if (Regex.IsMatch(value, @"natron", RegexOptions.IgnoreCase))
                    _packer = new cPackerNatron();
            }
        }
        //
        internal virtual object Connect(object o) { return null; }
        public virtual object GetCache() { return null; }
        internal virtual object ConnectCached(object o, object _cache) { return null; }
        internal virtual void Close(object o) { }
        internal virtual void Close() { }
        internal virtual byte[] SendCommand(object _connection, byte[] _command) { return null; }
        internal virtual byte[] SendCommand(object _connection, string _command) { return null; }
        internal virtual byte[] SendCommand(byte[] _command) { return null; }
        internal virtual byte[] SendCommand(string _command) { return null; }
        private int _sleep_after_write_milliseconds = 0;
        public int SleepAfterWriteMilliseconds
        {
            get
            {
                return _sleep_after_write_milliseconds;
            }
            set
            {
                _sleep_after_write_milliseconds = value;
            }
        }
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
        public static Dictionary<string, string> GetPathValues(string panel_id, string remask)
        {
            Dictionary<string, string> res = new Dictionary<string, string>();
            Monitor.Enter(_cs_cache);
            if (_cache_panels != null && _cache_panels.ContainsKey(panel_id))
            {
                Dictionary<string, string> panel_paths = _cache_panels[panel_id];
                foreach (string key in panel_paths.Keys)
                    if (Regex.IsMatch(key, remask))
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
        public static void RemovePathValues(string panel_id, string path)
        {
            string idx = "";
            Match m = Regex.Match(path, @"~index~(\d+)$");
            if (m.Success)
                idx = m.Groups[1].Value;
            Monitor.Enter(_cs_cache);
            if (_cache_panels != null && _cache_panels.ContainsKey(panel_id))
            {
                Dictionary<string, string> panel = _cache_panels[panel_id];
                if (panel != null && panel.ContainsKey(path))
                {
                    Dictionary<string, string> todel = new Dictionary<string, string>();
                    todel.Add(path, "");
                    string pathval = panel[path];
                    m = Regex.Match(pathval, @"([0-9a-fA-f]{8}\-[0-9a-fA-F]{4}\-[0-9a-fA-F]{4}\-[0-9a-fA-F]{4}\-[0-9a-fA-F]{12})$");
                    if (m.Success)
                    {
                        string tab = m.Groups[1].Value;
                        foreach (string pkey in panel.Keys)
                            if (Regex.IsMatch(pkey, @"\.TABS\." + tab + @"[\w\W]*?~index~" + idx, RegexOptions.IgnoreCase) && !todel.ContainsKey(pkey))
                                todel.Add(pkey, "");
                    }
                    //
                    foreach (string del in todel.Keys)
                        panel.Remove(del);
                }
            }
            Monitor.Exit(_cs_cache);
        }
        public static void RemoveElementPaths(string panel_id, string element, string idx, ref string tab)
        {
            string path_prefix = "ELEMENTS." + element;
            string path_sufix = "~index~" + idx;
            tab = null;
            //string grp = null;
            List<string> lstdel = new List<string>();
            Monitor.Enter(_cs_cache);
            if (_cache_panels != null && _cache_panels.ContainsKey(panel_id))
            {
                Dictionary<string, string> panel = _cache_panels[panel_id];
                foreach (string path in panel.Keys)
                    if (Regex.IsMatch(path, @"^" + path_prefix + @"[\w\W]+?" + path_sufix + "$"))
                    {
                        if (tab == null && Regex.IsMatch(element, @"(INPUT|OUTPUT)$"))
                        {
                            string val = panel[path];
                            Match m = Regex.Match(val, @"([0-9a-fA-f]{8}\-[0-9a-fA-F]{4}\-[0-9a-fA-F]{4}\-[0-9a-fA-F]{4}\-[0-9a-fA-F]{12})$");
                            if (m.Success)
                                tab = m.Groups[1].Value;
                            //if (grp == null && Regex.IsMatch(element, "INPUT$") && Regex.IsMatch(val, @"\.Settings\.fields\.Group\." + path_sufix + "$", RegexOptions.IgnoreCase))
                            //    grp = val;
                        }
                        lstdel.Add(path);
                    }
            }
            //
            Monitor.Exit(_cs_cache);
            //
            foreach (string todel in lstdel)
                RemovePathValue(panel_id, todel);
            //
            RemoveListElement(panel_id, element, idx);
            //if (grp != null)
            //    RemoveListElement(panel_id, "INPUT_GROUP", grp);
        }
        public static void RemoveLoopDevice(string _panel_id, string loop_type, string loop_nom, string dev_name, string dev_addr)
        {
            Monitor.Enter(_cs_pseudo_element_cache);
            if (_cache_list_panels.ContainsKey(_panel_id))
            {
                Dictionary<string, Dictionary<string, string>> panel = _cache_list_panels[_panel_id];
                Dictionary<string, Dictionary<string, string>> loops = new Dictionary<string, Dictionary<string, string>>();
                foreach (string pkey in panel.Keys)
                    if (Regex.IsMatch(pkey, "^" + loop_type + "/"))
                        loops.Add(pkey, panel[pkey]);
                List<string> loop2del = new List<string>();
                foreach (string lkey in loops.Keys)
                {
                    Dictionary<string, string> ddev = panel[lkey];
                    if (ddev.ContainsKey(dev_addr))
                        ddev.Remove(dev_addr);
                    if (ddev.Count == 0)
                        loop2del.Add(lkey);
                }
                foreach (string delkey in loop2del)
                    panel.Remove(delkey);
            }
            Monitor.Exit(_cs_pseudo_element_cache);
        }
        public static void RemoveLoopDevicePathValues(string _panel_id, string loop_type, string loop_nom, string dev_name, string dev_addr)
        {
            Monitor.Enter(_cs_cache);
            if (_cache_panels == null || !_cache_panels.ContainsKey(_panel_id))
            {
                Monitor.Exit(_cs_cache);
                return;
            }
            Dictionary<string, string> dpaths = _cache_panels[_panel_id];
            List<string> lstdel = new List<string>();
            string del_pattern = "^" + loop_type + @"[\w\W]+?#" + dev_name + @"[\w\W]+?~index~" + dev_addr + "$";
            foreach (string path in dpaths.Keys)
                if (Regex.IsMatch(path, del_pattern))
                    lstdel.Add(path);
            foreach (string sdel in lstdel)
                dpaths.Remove(sdel);
            Monitor.Exit(_cs_cache);
        }
        public static Dictionary<string, List<string>> ReversePaths(string panel_id)
        {
            Dictionary<string, List<string>> res = new Dictionary<string, List<string>>();
            Monitor.Enter(_cs_cache);
            if (_cache_panels != null && _cache_panels.ContainsKey(panel_id))
            {
                Dictionary<string, string> panel = _cache_panels[panel_id];
                foreach (string path in panel.Keys)
                {
                    if (!res.ContainsKey(panel[path]))
                        res.Add(panel[path], new List<string>());
                    List<string> lst = res[panel[path]];
                    lst.Add(path);
                }
            }
            //
            Monitor.Exit(_cs_cache);
            return res;
        }
        public static Dictionary<string, List<string>> ReversePaths(string panel_id, string filter)
        {
            Dictionary<string, List<string>> res = new Dictionary<string, List<string>>();
            Monitor.Enter(_cs_cache);
            if (_cache_panels != null && _cache_panels.ContainsKey(panel_id))
            {
                Dictionary<string, string> panel = _cache_panels[panel_id];
                foreach (string path in panel.Keys)
                {
                    if (!Regex.IsMatch(panel[path], filter, RegexOptions.IgnoreCase))
                        continue;
                    if (!res.ContainsKey(panel[path]))
                        res.Add(panel[path], new List<string>());
                    List<string> lst = res[panel[path]];
                    lst.Add(path);
                }
            }
            //
            Monitor.Exit(_cs_cache);
            return res;
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
            if (!el.ContainsKey(key) && Regex.IsMatch(key, @"^SIMPO_[\w\W]*?LOOP"))
                key = constants.NO_LOOP;
            if (!el.ContainsKey(key))
            {
                Monitor.Exit(_cs_pseudo_element_cache);
                return null;
            }
            Dictionary<string, string> lst = el[key];
            Monitor.Exit(_cs_pseudo_element_cache);
            return lst;
        }
        public static Dictionary<string, string> GetPseudoElementsList(string panel_id, string key, string match)
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
            Dictionary<string, string> res = new Dictionary<string, string>();
            match = Regex.Replace(match, @"\d+$", "");
            foreach (string lkey in lst.Keys)
            {
                string lt = lst[lkey];
                if (Regex.IsMatch(lt, @"""~loop_type""\s*?:\s*?""" + match, RegexOptions.IgnoreCase))
                    res.Add(lkey, lt);
            }
            if (res.Count == 0)
                res = null;
            Monitor.Exit(_cs_pseudo_element_cache);
            return res;
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
        public static void RemovePseudoElement(string panel_id, string key, string idx)
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
            if (lst.ContainsKey(idx))
                lst.Remove(idx);
            if (el[key].Count == 0)
                el.Remove(key);
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
        public static Dictionary<string, string> GetPseudoElementsDevices(string panel_id)
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
            string key = constants.NO_LOOP;
            string loopkey = Regex.Replace(key, @"^[^_]*?_", "");
            while (Regex.IsMatch(loopkey, "_"))
                loopkey = Regex.Replace(loopkey, @"^[^_]*?_", "");
            foreach (string elkey in el.Keys)
            {
                string loop = Regex.Replace(elkey, @"/[\w\W]+$", "");
                if (Regex.IsMatch(loop, loopkey + @"\d+$"))
                    foreach (string addr in el[elkey].Keys)
                        res.Add(loop + "~~~" + addr, el[elkey][addr]);
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
        public static void ChangeElementAddress(string panel_id, string key, string idx, string addrnew)
        {
            Monitor.Enter(_cs_cache);
            if (_cache_list_panels == null)
                _cache_list_panels = new Dictionary<string, Dictionary<string, Dictionary<string, string>>>();
            if (!_cache_list_panels.ContainsKey(panel_id))
                _cache_list_panels.Add(panel_id, new Dictionary<string, Dictionary<string, string>>());
            Dictionary<string, Dictionary<string, string>> el = _cache_list_panels[panel_id];
            if (!el.ContainsKey(key))
                return;
            Dictionary<string, string> lst = el[key];
            if (!lst.ContainsKey(idx))
                return;
            string _template = lst[idx];
            string _template_new = Regex.Replace(_template, @"~index~\d+", "~index~" + addrnew);
            if (!lst.ContainsKey(addrnew))
            {
                lst.Remove(idx);
                lst.Add(addrnew, _template_new);
            }
            //
            Monitor.Exit(_cs_cache);
        }
        public static void ChangeDeviceAddress(string panel_id, string oldAddress, string loopType, string newAddress)
        {
            Monitor.Enter(_cs_cache);
            if (_cache_list_panels == null)
                _cache_list_panels = new Dictionary<string, Dictionary<string, Dictionary<string, string>>>();
            if (!_cache_list_panels.ContainsKey(panel_id))
                _cache_list_panels.Add(panel_id, new Dictionary<string, Dictionary<string, string>>());
            Dictionary<string, Dictionary<string, string>> el = _cache_list_panels[panel_id];
            foreach (string loopkey in el.Keys)
                if (Regex.IsMatch(loopkey, @"^" + loopType, RegexOptions.IgnoreCase))
                {
                    Dictionary<string, string> dloop = el[loopkey];
                    Dictionary<string, string> deldev = new Dictionary<string, string>();
                    foreach (string addr in dloop.Keys)
                        if (addr == oldAddress)
                        {
                            string j = dloop[addr];
                            j = Regex.Replace(j, @"~index~" + oldAddress, "~index~" + newAddress);
                            deldev.Add(addr, j);
                        }
                    foreach (string addr in deldev.Keys)
                    {
                        dloop.Remove(addr);
                        dloop.Add(newAddress, deldev[addr]);
                    }
                }
            Monitor.Exit(_cs_cache);
            //
            Dictionary<string, string> pvals = GetPathValues(panel_id);
            Dictionary<string, string> pathchanges = new Dictionary<string, string>();
            foreach (string path in pvals.Keys)
                if (Regex.IsMatch(path, "^" + loopType + @"[\w\W]+?#[\w\W]+?~index~" + oldAddress + "$"))
                    pathchanges.Add(path, pvals[path]);
            foreach (string path in pathchanges.Keys)
            {
                string newpath = Regex.Replace(path, "~index~" + oldAddress + "$", "~index~" + newAddress);
                RemovePathValue(panel_id, path);
                SetPathValue(panel_id, newpath, pathchanges[path]);
            }
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
                    if (res != null)
                    {
                        Dictionary<string, string> repl = new Dictionary<string, string>();
                        foreach (string elkey in res.Keys)
                        {
                            string sel = res[elkey];
                            JObject oel = JObject.Parse(sel);
                            if (oel["~noname"] == null || oel["~noname"]["fields"] == null || oel["~noname"]["fields"]["TYPE"] == null) continue;
                            JObject tp = (JObject)oel["~noname"]["fields"]["TYPE"];
                            string path = "";
                            if (tp["~path"] == null) break;
                            path = tp["~path"].ToString();
                            if (!Regex.IsMatch(path, "natron_device")) break;
                            if (_cache_panels == null || !_cache_panels.ContainsKey(panel_id)) break;
                            Dictionary<string, string> cache = _cache_panels[panel_id];
                            string val = cache.ContainsKey(path) ? cache[path] : null;
                            if (val != null) oel["~value"] = val;
                            repl.Add(elkey, oel.ToString());
                        }
                        foreach (string rkey in repl.Keys)
                            res[rkey] = repl[rkey];
                    }
                }
            }
            Monitor.Exit(_cs_cache);
            return res;
        }
        public static void RemoveListElement(string panel_id, string element, string idx)
        {
            Monitor.Enter(_cs_cache);
            if (_cache_list_panels != null)
            {
                if (_cache_list_panels.ContainsKey(panel_id))
                {
                    Dictionary<string, Dictionary<string, string>> panel = _cache_list_panels[panel_id];
                    if (panel.ContainsKey(element))
                    {
                        Dictionary<string, string> el = panel[element];
                        if (el.ContainsKey(idx))
                            el.Remove(idx);
                    }
                }
            }
            Monitor.Exit(_cs_cache);
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

        #region transport
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
        public static cTransport ConnectTDF(cTDFParams p)
        {
            cTransport t = new cTDF();
            object conn = t.Connect(p);
            if (conn != null)
                return t;
            else
                return null;
        }
        public static cTransport ConnectTDFCached(cTDFParams p, object _cache)
        {
            cTransport t = new cTDF();
            object conn = t.ConnectCached(p, _cache);
            if (conn != null)
                return t;
            else
                return null;
        }
        public static cTransport ConnectHID(HidDevice p)
        {
            cTransport t = new cUSB();
            object conn = t.Connect(p);
            if (conn != null)
                return t;
            else
                return null;
        }
        public static cTransport ConnectCOM(cCOMParams p)
        {
            cTransport t = new cCOM();
            object conn = t.Connect(new cCOMParams(p.COMName, p.rate));
            return t;
        }
        public static cTransport ConnectBase(object conn_params, string panel_type, string panel_name)
        {
            cTransport conn = null;
            if (conn_params is cIPParams)
                conn = cComm.ConnectIP(((cIPParams)conn_params).address, ((cIPParams)conn_params).port);
            else if (conn_params is string)
                conn = cComm.ConnectFile((string)conn_params);
            else if (conn_params is cTDFParams)
                conn = cComm.ConnectTDF((cTDFParams)conn_params);
            else if (conn_params is HidDevice)
                conn = cComm.ConnectHID((HidDevice)conn_params);
            else if (conn_params is cCOMParams)
                conn = cComm.ConnectCOM((cCOMParams)conn_params);
            if (conn != null)
            {
                conn._panel_type = panel_type;
                conn.SleepAfterWriteMilliseconds = settings.Sleep(panel_name);
            }
            return conn;
        }
        public static cTransport ConnectBaseCached(object conn_params, string panel_type, string _panel_name, object _cache)
        {
            cTransport conn = null;
            if (conn_params is cIPParams)
                conn = cComm.ConnectIP(((cIPParams)conn_params).address, ((cIPParams)conn_params).port);
            else if (conn_params is string)
                conn = cComm.ConnectFile((string)conn_params);
            else if (conn_params is cTDFParams)
                conn = cComm.ConnectTDFCached((cTDFParams)conn_params, _cache);
            else if (conn_params is HidDevice)
                conn = cComm.ConnectHID((HidDevice)conn_params);
            else if (conn_params is cCOMParams)
                conn = cComm.ConnectCOM((cCOMParams)conn_params);
            conn._panel_type = panel_type;
            conn.SleepAfterWriteMilliseconds = settings.Sleep(_panel_name);
            return conn;
        }
        public static byte[] SendCommand(cTransport conn, string cmd)
        {
            byte[] res = conn.SendCommand(cmd);
            return res;
        }
        public static byte[] SendCommand(cTransport conn, byte[] cmd)
        {
            byte[] res = conn.SendCommand(cmd);
            return res;
        }
        public static void CloseConnection(cTransport conn)
        {
            conn.Close();
        }
        #endregion

        #region save/load
        public static void ClearPanelCache(string _panel_id)
        {
            Monitor.Enter(_cs_cache);
            Monitor.Enter(_cs_pseudo_element_cache);
            if (_cache_panels != null && _cache_panels.ContainsKey(_panel_id))
            {
                _cache_panels[_panel_id].Clear();
                _cache_panels.Remove(_panel_id);
            }
            if (_cache_list_panels != null && _cache_list_panels.ContainsKey(_panel_id))
            {
                _cache_list_panels[_panel_id].Clear();
                _cache_list_panels.Remove(_panel_id);
            }
            if (_cache_pseudo_element_panels != null && _cache_pseudo_element_panels.ContainsKey(_panel_id))
            {
                _cache_pseudo_element_panels[_panel_id].Clear();
                _cache_pseudo_element_panels.Remove(_panel_id);
            }
            //
            Monitor.Exit(_cs_pseudo_element_cache);
            Monitor.Exit(_cs_cache);
        }
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
        public static JObject Data2Save()
        {
            Monitor.Enter(_cs_cache);
            Monitor.Enter(_cs_pseudo_element_cache);
            JObject _j_cache_panels = (_cache_panels != null) ? JObject.FromObject(_cache_panels) : null;
            JObject _j_cache_list_panels = (_cache_list_panels != null) ? JObject.FromObject(_cache_list_panels) : null;
            JObject _j_cache_pseudo_element_panels = (_cache_pseudo_element_panels != null) ? JObject.FromObject(_cache_pseudo_element_panels) : null;
            Monitor.Exit(_cs_pseudo_element_cache);
            Monitor.Exit(_cs_cache);
            //
            JObject res = new JObject();
            res["_cache_panels"] = _j_cache_panels;
            res["_cache_list_panels"] = _j_cache_list_panels;
            res["_cache_pseudo_element_panels"] = _j_cache_pseudo_element_panels;
            //
            return res;
        }
        public static void Load(JObject o)
        {
            ClearCache();
            Monitor.Enter(_cs_cache);
            Monitor.Enter(_cs_pseudo_element_cache);
            //
            if (o["_cache_panels"] != null && o["_cache_panels"].Type != JTokenType.Null)
            {
                JObject _panels = (JObject)o["_cache_panels"];
                if (_cache_panels == null) _cache_panels = new Dictionary<string, Dictionary<string, string>>();
                foreach (JProperty p in _panels.Properties())
                {
                    _cache_panels.Add(p.Name, new Dictionary<string, string>());
                    Dictionary<string, string> d = _cache_panels[p.Name];
                    JObject _rows = (JObject)p.Value;
                    foreach (JProperty r in _rows.Properties())
                        d.Add(r.Name, r.Value.ToString());
                }
            }
            //
            if (o["_cache_list_panels"] != null && o["_cache_list_panels"].Type != JTokenType.Null)
            {
                JObject _list_panels = (JObject)o["_cache_list_panels"];
                if (_cache_list_panels == null) _cache_list_panels = new Dictionary<string, Dictionary<string, Dictionary<string, string>>>();
                foreach (JProperty p in _list_panels.Properties())
                {
                    _cache_list_panels.Add(p.Name, new Dictionary<string, Dictionary<string, string>>());
                    Dictionary<string, Dictionary<string, string>> d = _cache_list_panels[p.Name];
                    JObject _panels = (JObject)p.Value;
                    foreach (JProperty pp in _panels.Properties())
                    {
                        d.Add(pp.Name, new Dictionary<string, string>());
                        Dictionary<string, string> dd = d[pp.Name];
                        JObject _rows = (JObject)pp.Value;
                        foreach (JProperty r in _rows.Properties())
                            dd.Add(r.Name, r.Value.ToString());
                    }
                }
            }
            //
            if (o["_cache_pseudo_element_panels"] != null && o["_cache_pseudo_element_panels"].Type != JTokenType.Null)
            {
                JObject _pseudo_panels = (JObject)o["_cache_pseudo_element_panels"];
                if (_cache_pseudo_element_panels == null) _cache_pseudo_element_panels = new Dictionary<string, Dictionary<string, Dictionary<string, string>>>();
                foreach (JProperty p in _pseudo_panels.Properties())
                {
                    _cache_pseudo_element_panels.Add(p.Name, new Dictionary<string, Dictionary<string, string>>());
                    Dictionary<string, Dictionary<string, string>> d = _cache_pseudo_element_panels[p.Name];
                    JObject _panels = (JObject)p.Value;
                    foreach (JProperty pp in _panels.Properties())
                    {
                        d.Add(pp.Name, new Dictionary<string, string>());
                        Dictionary<string, string> dd = d[pp.Name];
                        JObject _rows = (JObject)pp.Value;
                        foreach (JProperty r in _rows.Properties())
                            dd.Add(r.Name, r.Value.ToString());
                    }
                }
            }
            //
            Monitor.Exit(_cs_pseudo_element_cache);
            Monitor.Exit(_cs_cache);
        }
        #endregion

        #region scan&new system
        private static Dictionary<string, List<string>> _version_commands = null;
        private static object _cs_ver_cmds = new object();
        private static string VerCommandsNatron(string read_xml)
        {
            string res = null;
            foreach (Match m in Regex.Matches(read_xml, @"<COMMANDS>[\w\W]+?</PROPERTIES>"))
            {
                string cmds = m.Value;
                Match mm = Regex.Match(cmds, @"<PROPERTIES>[\w\W]+?</PROPERTIES>");
                if (mm.Success && Regex.IsMatch(mm.Value, @"ID\s*?=\s*?""VERSION_PANEL"""))
                {
                    mm = Regex.Match(cmds, @"<COMMANDS>([\w\W]+?)</COMMANDS>");
                    if (mm.Success)
                    {
                        foreach (Match mcmd in Regex.Matches(mm.Groups[1].Value, @"<COMMAND\s+?BYTES\s*?=\s*?""(\w+?)"""))
                        {
                            if (res == null) res = "";
                            res += ((res != "") ? "\n" : "") + mcmd.Groups[1].Value;
                        }
                        if (res != null) return res;
                    }
                }
            }
            return res;
        }
        public static Dictionary<string, List<string>> VersionCommands
        {
            get
            {
                Dictionary<string, List<string>> res = new Dictionary<string, List<string>>();
                Monitor.Enter(_cs_ver_cmds);
                if (_version_commands == null)
                {
                    _version_commands = new Dictionary<string, List<string>>();
                    string sysdir = System.Environment.CurrentDirectory;
                    if (!Regex.IsMatch(sysdir, @"[\\/]$")) sysdir += @"\";
                    sysdir += @"Configs\XML\Systems\";
                    string[] systems = System.IO.Directory.GetDirectories(sysdir);
                    foreach (string sys in systems)
                    {
                        string readdir = sys;
                        if (!Regex.IsMatch(sys, @"[\\/]$")) readdir += @"\";
                        readdir += "Read";
                        string[] rfiles = System.IO.Directory.GetFiles(readdir);
                        foreach (string rf in rfiles)
                        {
                            string s = System.IO.Path.GetFileName(rf);
                            if (!Regex.IsMatch(s, @"^read[\w\W]+?\.xml$", RegexOptions.IgnoreCase)) continue;
                            if (Regex.IsMatch(s, @"0\d+?\.xml$", RegexOptions.IgnoreCase)) continue;
                            if (Regex.IsMatch(s, @"version\d+?\.xml$", RegexOptions.IgnoreCase)) continue;
                            string xml = System.IO.File.ReadAllText(rf);
                            string cmd = null;
                            if (Regex.IsMatch(s, "natron", RegexOptions.IgnoreCase)) cmd = VerCommandsNatron(xml);
                            else
                            {
                                Match m = Regex.Match(xml, @"<COMMAND\s[\w\W]+?/>", RegexOptions.IgnoreCase);
                                if (!m.Success) continue;
                                cmd = m.Value;
                                m = Regex.Match(cmd, @"SAVEAS\s*?=\s*?""VERSION""", RegexOptions.IgnoreCase);
                                if (!m.Success) continue;
                                m = Regex.Match(cmd, @"BYTES\s*?=\s*?""([\w\W]+?)""", RegexOptions.IgnoreCase);
                                if (!m.Success) continue;
                                cmd = m.Groups[1].Value;
                            }
                            string sysname = System.IO.Path.GetFileName(sys);
                            if (!_version_commands.ContainsKey(cmd)) _version_commands.Add(cmd, new List<string>());
                            _version_commands[cmd].Add(sysname);
                        }
                    }
                }
                foreach (string key in _version_commands.Keys)
                    res.Add(key, _version_commands[key]);
                Monitor.Exit(_cs_ver_cmds);
                return res;
            }
        }
        private static Dictionary<string, SerialPort> ScanCOM()
        {
            Dictionary<string, SerialPort> res = new Dictionary<string, SerialPort>();
            Dictionary<string, List<string>> cmds = VersionCommands;
            string[] ports = cCOM.GetCOMPorts();
            if (ports == null) return res;
            foreach (string port in ports)
            {
                cCOM com = new cCOM();
                object conn = com.Connect(new cCOMParams(port, 9600));
                if (conn == null) continue;
                foreach (string cmd in cmds.Keys)
                {
                    string[] cmda = cmd.Split('\n');
                    byte[] aver = null;
                    List<byte[]> lstVer = new List<byte[]>();
                    int len = 0;
                    foreach (string cmdln in cmda)
                    {
                        byte[] averln = com.SendCommand(conn, cmdln);
                        if (averln != null)
                        {
                            lstVer.Add(averln);
                            len += averln.Length;
                        }
                    }
                    if (len > 0)
                    {
                        aver = new byte[len];
                        int offs = 0;
                        foreach (byte[] b in lstVer)
                        {
                            b.CopyTo(aver, offs);
                            offs += b.Length;
                        }
                    }
                    if (aver == null) continue;
                }
                com.Close();
                //bool f = cCOM.TryOpen(port);
                //if (!f) continue;
            }
            //
            return res;
        }
        private static JObject SystemsHIDIds()
        {
            List<JObject> dirs = AvailableSystems();
            JObject res = new JObject();
            foreach (JObject d in dirs)
                if (d["HIDs"] != null)
                    res[d["HIDs"]["product"].ToString()] = d["HIDs"]["HIDs"];
            if (res.Properties().Count() == 0) res = null;
            return res;
        }
        private static string HidFound(JObject HIDs, HidDevice _dev)
        {
            foreach (JProperty p in HIDs.Properties())
            {
                JArray a = (JArray)p.Value;
                foreach (JObject d in a)
                {
                    int vid = Convert.ToInt32(d["vid"].ToString());
                    int pid = Convert.ToInt32(d["pid"].ToString());
                    if (_dev.VendorID == vid && _dev.ProductID == pid) return p.Name;
                }
            }
            return null;
        }
        public static Dictionary<string, HidDevice> ScanHID()
        {
            JObject HIDs = SystemsHIDIds();
            Dictionary<string, HidDevice> res = new Dictionary<string, HidDevice>();
            //
            //var device = loader.GetDevices(0x1234, 0x9876).First();
            //
            HidDeviceLoader _loader = new HidDeviceLoader();
            //USB\VID_0483 & PID_5710
            //HidDevice dev = _loader.GetDeviceOrDefault(0x0483, 0x5710, null, null);
            IEnumerable<HidDevice> _devs = _loader.GetDevices();
            foreach (HidDevice _dev in _devs)
            {
                if (Regex.IsMatch(_dev.Manufacturer, "teletek", RegexOptions.IgnoreCase) && !res.ContainsKey(_dev.ToString()))
                    res.Add(_dev.ProductName, _dev);
                else
                {
                    string product = HidFound(HIDs, _dev);
                    if (product != null)
                    {
                        if (_dev.ProductName != null && _dev.ProductName.Trim() != "") product = _dev.ProductName;
                        res.Add(product, _dev);
                    }
                }
            }
            //DeviceList dLst = new FilteredDeviceList();
            //IEnumerable<Device> _devs = dLst.GetAllDevices();// GetHidDevices();
            //
            return res;
        }
        private static JObject _usb_devs(string sysdir)
        {
            if (!Regex.IsMatch(sysdir, @"[\\/]$")) sysdir += Path.DirectorySeparatorChar;
            if (!Directory.Exists(sysdir + "Template")) return null;
            string[] files = Directory.GetFiles(sysdir + "Template");
            string template = null;
            foreach (string f in files)
                if (Regex.IsMatch(f, @"\.xml$", RegexOptions.IgnoreCase))
                {
                    template = f;
                    break;
                }
            if (template != null)
            {
                string xml = File.ReadAllText(template);
                Match m = Regex.Match(xml, @"<ELEMENT\s[\w\W]+?>");
                if (m.Success)
                {
                    string prod = null;
                    Match mm = Regex.Match(m.Value, @"PRODUCTNAME\s*?=\s*?""([\w\W]+?)""");
                    if (mm.Success) prod = mm.Groups[1].Value;
                    else return null;
                    m = Regex.Match(m.Value, @"USBDEVICES\s*?=\s*?""([\w\W]+?)""");
                    if (m.Success)
                    {
                        JObject res = new JObject();
                        JArray adevs = new JArray();
                        string[] _devs = m.Groups[1].Value.Split(';');
                        foreach (string d in _devs)
                        {
                            string[] _dev = d.Split(':');
                            JObject odev = new JObject();
                            odev["vid"] = Convert.ToInt32(_dev[0], 16);
                            odev["pid"] = Convert.ToInt32(_dev[1], 16);
                            adevs.Add(odev);
                        }
                        res["HIDs"] = adevs;
                        res["product"] = prod;
                        return res;
                    }
                }
            }
            return null;
        }
        public static List<JObject> AvailableSystems()
        {
            List<JObject> res = new List<JObject>();
            string[] dirs = Directory.GetDirectories("Configs\\XML\\Systems");
            List<JObject> lstFire = new List<JObject>();
            List<JObject> lstGuard = new List<JObject>();
            List<JObject> lstOther = new List<JObject>();
            foreach (string s in dirs)
            {
                string dir = Path.GetFileName(s);
                JObject jSys = new JObject();
                JObject _hids = _usb_devs(s);
                if (_hids != null) jSys["HIDs"] = _hids;
                if (Regex.IsMatch(dir, "^(iris|simpo|natron|tft)", RegexOptions.IgnoreCase) ||
                    Regex.IsMatch(dir, @"^repeater[\w\W]+?simpo$", RegexOptions.IgnoreCase))
                    jSys["deviceType"] = "fire";
                else if (Regex.IsMatch(dir, "^eclipse", RegexOptions.IgnoreCase))
                    jSys["deviceType"] = "guard";
                else
                    jSys["deviceType"] = "";
                jSys["schema"] = dir.ToLower();
                jSys["title"] = dir;
                if (jSys["deviceType"].ToString() == "fire")
                    jSys["interface"] = "IP";
                else
                    jSys["interface"] = "COM";
                jSys["address"] = "";
                //
                if (jSys["deviceType"].ToString() == "fire")
                    lstFire.Add(jSys);
                else if (jSys["deviceType"].ToString() == "guard")
                    lstGuard.Add(jSys);
                else lstOther.Add(jSys);
            }
            foreach (JObject o in lstFire) res.Add(o);
            foreach (JObject o in lstGuard) res.Add(o);
            foreach (JObject o in lstOther) res.Add(o);
            //
            return res;
        }
        private static string HidSchema(HidDevice _dev)
        {
            if (Regex.IsMatch(_dev.ProductName, @"repeater[\w\W]*?iris[\w\W]*?simpo", RegexOptions.IgnoreCase))
                return "Repeater_Iris_Simpo";
            if (Regex.IsMatch(_dev.ProductName, @"Fire\s+?panel\s+?simpo", RegexOptions.IgnoreCase))
                return "SIMPO";
            if (Regex.IsMatch(_dev.ProductName, @"REPEATER\s+?TFT", RegexOptions.IgnoreCase))
                return "TFT_REPEATER";
            Match m = Regex.Match(_dev.ProductName.Trim(), @"^(iris\d*)", RegexOptions.IgnoreCase);
            if (m.Success) return m.Groups[1].Value.ToLower();
            return "";
        }
        private static List<JObject> SystemsFound(Dictionary<string, HidDevice> hid)
        {
            List<JObject> res = new List<JObject>();
            //
            List<JObject> lstFire = new List<JObject>();
            List<JObject> lstGuard = new List<JObject>();
            List<JObject> lstOther = new List<JObject>();
            foreach (string s in hid.Keys)
            {
                HidDevice _dev = hid[s];
                JObject jSys = new JObject();
                if (Regex.IsMatch(s, "^(iris|simpo|natron|tft)", RegexOptions.IgnoreCase) ||
                    Regex.IsMatch(s, @"repeater[\w\W]+?iris[\w\W]+?simpo", RegexOptions.IgnoreCase) ||
                    Regex.IsMatch(s, @"Fire\s+?panel\s+?simpo", RegexOptions.IgnoreCase) ||
                    Regex.IsMatch(s, @"^repeater[\w\W]+?tft", RegexOptions.IgnoreCase))
                    jSys["deviceType"] = "fire";
                else if (Regex.IsMatch(s, "^eclipse", RegexOptions.IgnoreCase))
                    jSys["deviceType"] = "guard";
                else
                    jSys["deviceType"] = "";
                string _schema = HidSchema(_dev);
                jSys["schema"] = _schema.ToLower();
                jSys["title"] = _dev.ProductName;
                jSys["interface"] = "USB";
                jSys["address"] = _dev.VendorID.ToString() + "&" + _dev.ProductID.ToString();
                //
                if (jSys["deviceType"].ToString() == "fire")
                    lstFire.Add(jSys);
                else if (jSys["deviceType"].ToString() == "guard")
                    lstGuard.Add(jSys);
                else lstOther.Add(jSys);
            }
            foreach (JObject o in lstFire) res.Add(o);
            foreach (JObject o in lstGuard) res.Add(o);
            foreach (JObject o in lstOther) res.Add(o);
            //
            return res;
        }
        public static JObject Scan()
        {
            JObject res = new JObject();
            //
            //Dictionary<string, SerialPort> comdevs = ScanCOM();
            //
            Dictionary<string, HidDevice> devs = ScanHID();
            List<JObject> lstFound = SystemsFound(devs);
            if (lstFound.Count > 0)
            {
                JObject oFound = new JObject();
                JArray aFound = new JArray();
                foreach (JObject o in lstFound) aFound.Add(o);
                res["found"] = aFound;
            }
            //
            List<JObject> dirs = AvailableSystems();
            JArray aFire = new JArray();
            JArray aGuard = new JArray();
            JArray aOther = new JArray();
            foreach (JObject o in dirs)
                if (o["deviceType"].ToString() == "fire") aFire.Add(o);
                else if (o["deviceType"].ToString() == "guard") aGuard.Add(o);
                else aOther.Add(o);
            if (aFire.Count > 0) res["fire"] = aFire;
            if (aGuard.Count > 0) res["guard"] = aGuard;
            if (aOther.Count > 0) res["other"] = aOther;
            return res;
            //
            cTransport t = new cIP();
            //object conn = t.Connect(new cIPParams("92.247.2.162", 7000));
            ////byte[] res = t.SendCommand(conn, t._ver_cmd);
            //byte[] res = t.SendCommand(conn, t._panel_in_nework_0_cmd);
            //t.Close(conn);
            //return JArray.Parse("[{ deviceType: 'fire', schema: 'iris', title: 'IRIS', interface: 'IP', address: '92.247.2.162:7000'}, { deviceType: 'fire', schema: 'iris8', title: 'IRIS8', interface: 'IP', address: '212.36.21.86:7000'}, { deviceType: '', schema: 'eclipse', title: 'ECLIPSE99', interface: 'COM', address: 'COM1'}]");
        }
        #endregion
    }
}
