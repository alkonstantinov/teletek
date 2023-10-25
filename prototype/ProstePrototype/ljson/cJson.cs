//using Newtonsoft.Json.Linq;
//using System;
//using System.Text.Json.Serialization;
//using System.Text.RegularExpressions;
//using System.Xml.Linq;
//using System.Xml;
using Microsoft.Win32;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Xml;
using System.Xml.Serialization;
using System.Text.RegularExpressions;
using System.Runtime.CompilerServices;
using System.Printing;
using lcommunicate;
using common;
using System.Windows.Shapes;
using System.Windows.Markup;
using System.Data;
using System.Windows.Media.Animation;
using System.Windows.Input;
using System.Security.Cryptography;
using System.Windows.Controls;
using System.Numerics;
using System.Runtime.Intrinsics.X86;

namespace ljson
{
    public class cJson
    {
        #region old xml configs
        private static JObject _write_relations = new JObject();

        private static Dictionary<string, cXmlConfigs> _panel_xmls = new Dictionary<string, cXmlConfigs>();
        private static object _cs_panel_xmls = new object();

        private static cXmlConfigs GetPanelXMLConfigs(string key)
        {
            cXmlConfigs cfg = null;
            Monitor.Enter(_cs_panel_xmls);
            if (_panel_xmls.ContainsKey(key))
                cfg = _panel_xmls[key];
            Monitor.Exit(_cs_panel_xmls);
            return cfg;
        }
        private static string PanelTemplatePath()
        {
            JObject _panel = CurrentPanel;
            if (_panel["~template_loaded_from"] != null)
                return _panel["~template_loaded_from"].ToString();
            else
                return null;
        }
        public static JObject ReadXML()
        {
            cXmlConfigs cfg = GetPanelXMLConfigs(PanelTemplatePath());
            if (cfg.ReadConfig == null)
            {
                string xml = File.ReadAllText(cfg.ReadConfigPath);
                XmlDocument xdoc = new XmlDocument();
                xdoc.LoadXml(xml);
                cfg.ReadConfig = xdoc;
            }
            string sjson = JsonConvert.SerializeXmlNode((XmlDocument)cfg.ReadConfig);
            return JObject.Parse(sjson);
        }
        public static JObject TemplateXML()
        {
            cXmlConfigs cfg = GetPanelXMLConfigs(PanelTemplatePath());
            string sjson = JsonConvert.SerializeXmlNode((XmlDocument)cfg.Config);
            return JObject.Parse(sjson);
        }
        private static void SetLNGID(cXmlConfigs cfg)
        {
            Dictionary<string, string> _lngids_set = new Dictionary<string, string>();
            //
            Dictionary<string, Dictionary<string, cWriteField>> ids = cfg.WriteXpaths;
            XmlDocument xml = (XmlDocument)cfg.Config;
            if (ids == null || xml == null)
                return;
            string[] keys = new string[ids.Count];
            ids.Keys.CopyTo(keys, 0);
            //
            StringBuilder err = new StringBuilder();
            //Dictionary<string, StringBuilder> lists = new Dictionary<string, StringBuilder>();
            //
            foreach (string key in keys)
            {
                Dictionary<string, cWriteField> flds = ids[key];
                string[] fldkeys = new string[flds.Count];
                flds.Keys.CopyTo(fldkeys, 0);
                foreach (string fkey in fldkeys)
                {
                    string xpath = flds[fkey].path;
                    //
                    //Match m = Regex.Match(xpath, @"ELEMENTS/ELEMENT[\w\W]*?(\[\s*?@ID\s*?=[\w\W]+?\])([\w\W]*?)(/ELEMENTS/ELEMENT[\w\W]*?)\[(\d+?)\D", RegexOptions.IgnoreCase);
                    //Match m = Regex.Match(xpath, @"(\[\s*?@ID\s*?=[\w\W]+?\])([\w\W]*?)(/ELEMENTS/ELEMENT[\w\W]*?)", RegexOptions.IgnoreCase);
                    //if (m.Success)
                    //{
                    //    string grp = m.Groups[1].Value + m.Groups[2].Value + m.Groups[3].Value;
                    //    //int idx = Convert.ToInt32(m.Groups[4].Value);
                    //    //if (idx <= 1)
                    //    //{
                    //        if (!lists.ContainsKey(grp))
                    //            lists.Add(grp, new StringBuilder());
                    //        lists[grp].Append(xpath + "\n");
                    //    //}
                    //    continue;
                    //}
                    //
                    //if (Regex.IsMatch(xpath, @"^properties", RegexOptions.IgnoreCase))
                    //    xpath = "ELEMENTS/ELEMENT[1]/" + xpath;
                    //if (Regex.IsMatch(xpath, @"ELEMENTS/ELEMENT[\w\W]*?/ELEMENTS/ELEMENT", RegexOptions.IgnoreCase))
                    //    xpath = Regex.Replace(xpath, @"^ELEMENTS/ELEMENT[\w\W]*?/", "", RegexOptions.IgnoreCase);
                    //xpath = Regex.Replace(xpath, @"^([\w\W]+?/ELEMENT\[[\w\W]+?=[\w\W]+?\])\[\d+?\](/[\w\W]+$)", "$1$2", RegexOptions.IgnoreCase);
                    XmlNode node = null;
                    try
                    {
                        node = xml.SelectSingleNode(xpath);
                    }
                    catch { }
                    if (node != null)
                    {
                        string propid = null;
                        string proppath = Regex.Replace(xpath, @"^([\w\W]+?PROPERTY)\[[\w\W]+$", "$1", RegexOptions.IgnoreCase);
                        XmlNode propnode = xml.SelectSingleNode(proppath);
                        if (propnode != null)
                            propid = propnode.Attributes["LNGID"].Value;
                        //
                        string lngid = node.Attributes["LNGID"].Value;
                        if (!_lngids_set.ContainsKey(lngid))
                        {
                            ids[key][fkey].lngid = lngid;
                            _lngids_set.Add(lngid, null);
                        }
                        else
                            err.Append(xpath + "\n");
                    }
                    else
                        err.Append(xpath + "\n");
                }
            }
            //foreach (string key in lists.Keys)
            //{
            //    string el = Regex.Replace(key, @"[\w\W]+?(/ELEMENTS[\w\W]+$)", "$1", RegexOptions.IgnoreCase);
            //    string keyid = null;
            //    XmlNode keynode = xml.SelectSingleNode(el);
            //    if (keynode != null)
            //        keyid = keynode.Attributes["LNGID"].Value;
            //    string[] paths = lists[key].ToString().Split('\n');
            //    string propkey = null;
            //    foreach (string xpath in paths)
            //    {
            //        string dkey = xpath;
            //        string proppath = Regex.Replace(xpath, @"[\w\W]+?(/ELEMENTS[\w\W]+$)", "$1", RegexOptions.IgnoreCase);
            //    }
            //}
            //int cnt = lists.Count;
            //
            string s = "";
        }

        private static void SetPanelXMLConfigs(string key, cXmlConfigs cfg)
        {
            Monitor.Enter(_cs_panel_xmls);
            if (_panel_xmls.ContainsKey(key))
                _panel_xmls[key] = cfg;
            else
                _panel_xmls.Add(key, cfg);
            SetLNGID(cfg);
            Dictionary<string, cWriteField> lngids = cfg.WriteFieldsByLNGID;
            Monitor.Exit(_cs_panel_xmls);
        }
        #endregion

        #region private structures & fields
        private static Dictionary<string, JObject> _panel_templates = new Dictionary<string, JObject>();
        private static object _cs_panel_templates = new object();

        private static Dictionary<string, string> _system_panels = new Dictionary<string, string>();
        private static string _current_panel_id = null;
        private static object _cs_current_panel = new object();

        private static Dictionary<string, string> _panel_names = new Dictionary<string, string>();

        private static object _cs_last_content = new object();
        private static string _last_content = null;
        #endregion

        #region private structure properties
        private static JObject GetPanelTemplate(string name)
        {
            JObject res = null;
            Monitor.Enter(_cs_panel_templates);
            if (_panel_templates.ContainsKey(name))
                res = _panel_templates[name];
            Monitor.Exit(_cs_panel_templates);
            return res;
        }

        private static void SetPanelTemplate(string name, JObject json)
        {
            Monitor.Enter(_cs_panel_templates);
            if (!_panel_templates.ContainsKey(name))
                _panel_templates.Add(name, json);
            else
                _panel_templates[name] = json;
            Monitor.Exit(_cs_panel_templates);
        }
        private static JObject CurrentPanel
        {
            get
            {
                Monitor.Enter(_cs_current_panel);
                if (_current_panel_id == null || !_system_panels.ContainsKey(_current_panel_id))
                {
                    Monitor.Exit(_cs_current_panel);
                    return null;
                }
                string filename = _system_panels[_current_panel_id];
                Monitor.Exit(_cs_current_panel);
                Monitor.Enter(_cs_panel_templates);
                if (!_panel_templates.ContainsKey(filename))
                {
                    Monitor.Exit(_cs_panel_templates);
                    return null;
                }
                JObject res = _panel_templates[filename];
                Monitor.Exit(_cs_panel_templates);
                return res;
            }
            //set
            //{
            //    Monitor.Enter(_cs_current_panel);
            //    _current_panel = value;
            //    Monitor.Exit(_cs_current_panel);
            //}
        }
        private static string LastContent
        {
            get
            {
                Monitor.Enter(_cs_last_content);
                string res = _last_content;
                Monitor.Exit(_cs_last_content);
                return res;
            }
            set
            {
                Monitor.Enter(_cs_last_content);
                _last_content = value;
                Monitor.Exit(_cs_last_content);
            }
        }
        #endregion

        #region public properties
        public static Dictionary<string, JObject> SystemPanels
        {
            get
            {
                Dictionary<string, JObject> res = new Dictionary<string, JObject>();
                Monitor.Enter(_cs_current_panel);
                Monitor.Enter(_cs_panel_templates);
                foreach (string _id in _system_panels.Keys)
                {
                    string filename = _system_panels[_id];
                    if (_panel_templates.ContainsKey(filename))
                        res.Add(_id, _panel_templates[filename]);
                }
                Monitor.Exit(_cs_panel_templates);
                Monitor.Exit(_cs_current_panel);
                return res;
            }
        }
        public static string CurrentPanelID
        {
            get
            {
                Monitor.Enter(_cs_current_panel);
                string res = _current_panel_id;
                Monitor.Exit(_cs_current_panel);
                return res;
            }
            set
            {
                Monitor.Enter(_cs_current_panel);
                _current_panel_id = value;
                constants.NO_LOOP = NoLoopKey(CurrentPanel.ToString());
                //_internal_relations_operator.CurrentPanelID = value;
                Monitor.Exit(_cs_current_panel);
            }
        }
        public static string CurrentPanelName
        {
            get
            {
                Monitor.Enter(_cs_current_panel);
                if (_current_panel_id == null || !_system_panels.ContainsKey(_current_panel_id))
                {
                    Monitor.Exit(_cs_current_panel);
                    return null;
                }
                if (_panel_names.ContainsKey(CurrentPanelID))
                {
                    string res = _panel_names[CurrentPanelID];
                    Monitor.Exit(_cs_current_panel);
                    return res;
                }
                string filename = _system_panels[_current_panel_id];
                Monitor.Exit(_cs_current_panel);
                Match m = Regex.Match(Regex.Replace(filename, @"\\Template[\w\W]+$", "", RegexOptions.IgnoreCase), @"[\w\W]+?\\([^\\]+)$");
                if (m.Success)
                    return m.Groups[1].Value.ToLower();
                return filename;
            }
            set
            {
                Monitor.Enter(_cs_current_panel);
                if (!_panel_names.ContainsKey(CurrentPanelID))
                    _panel_names.Add(CurrentPanelID, value);
                else
                    _panel_names[CurrentPanelID] = value;
                Monitor.Exit(_cs_current_panel);
            }
        }
        public static string CurrentPanelFullType
        {
            get
            {
                Monitor.Enter(_cs_current_panel);
                if (_current_panel_id == null || !_system_panels.ContainsKey(_current_panel_id))
                {
                    Monitor.Exit(_cs_current_panel);
                    return null;
                }
                string filename = _system_panels[_current_panel_id];
                Monitor.Exit(_cs_current_panel);
                Match m = Regex.Match(Regex.Replace(filename, @"\\Template[\w\W]+$", "", RegexOptions.IgnoreCase), @"[\w\W]+?\\([^\\]+)$");
                if (m.Success)
                    return m.Groups[1].Value.ToLower();
                return filename;
            }
        }
        public static string CurrentPanelType
        {
            get
            {
                JObject _panel = CurrentPanel;
                return _panel["~panel_type"].ToString();
            }
        }
        #endregion
        public static string PanelName(string _id)
        {
            Monitor.Enter(_cs_current_panel);
            if (!_system_panels.ContainsKey(_id))
            {
                Monitor.Exit(_cs_current_panel);
                return null;
            }
            if (_panel_names.ContainsKey(_id))
            {
                string res = _panel_names[_id];
                Monitor.Exit(_cs_current_panel);
                return res;
            }
            string filename = _system_panels[_id];
            Monitor.Exit(_cs_current_panel);
            Match m = Regex.Match(Regex.Replace(filename, @"\\Template[\w\W]+$", "", RegexOptions.IgnoreCase), @"[\w\W]+?\\([^\\]+)$");
            if (m.Success)
                return m.Groups[1].Value.ToLower();
            return filename;
        }
        public static void RenamePanel(string _id, string name)
        {
            Monitor.Enter(_cs_current_panel);
            if (!_panel_names.ContainsKey(_id))
                _panel_names.Add(_id, name);
            else
                _panel_names[_id] = name;
            Monitor.Exit(_cs_current_panel);
        }

        #region save/load
        private static byte[] Compress(byte[] bytes)
        {
            using (var msi = new MemoryStream(bytes))
            using (var mso = new MemoryStream())
            {
                using (var gs = new GZipStream(mso, CompressionMode.Compress))
                {
                    msi.CopyTo(gs);
                    //CopyTo(msi, gs);
                }

                return mso.ToArray();
            }
            //MemoryStream mstr = new MemoryStream();
            //GZipStream gz = new GZipStream(mstr, CompressionMode.Compress);
            //gz.Write(bytes, 0, bytes.Length);
            //gz.Dispose();
            //return mstr.ToArray();
        }
        private static byte[] Compress(StringBuilder sb)
        {
            return Compress(Encoding.UTF8.GetBytes(sb.ToString()));
        }
        private static byte[] Compress(string s)
        {
            return Compress(Encoding.UTF8.GetBytes(s));
        }
        private static byte[] Decompress(byte[] bytes)
        {
            using (var msi = new MemoryStream(bytes))
            using (var mso = new MemoryStream())
            {
                using (var gs = new GZipStream(msi, CompressionMode.Decompress))
                {
                    gs.CopyTo(mso);
                    //CopyTo(gs, mso);
                }

                return mso.ToArray();
                //MemoryStream mstr = new MemoryStream(bytes);
                //MemoryStream strout = new MemoryStream();
                //GZipStream gz = new GZipStream(mstr, CompressionMode.Decompress);
                //gz.CopyTo(strout);
                //gz.Dispose();
                //return strout.ToArray();
            }
        }
        private static string DecompressStr(byte[] bytes)
        {
            byte[] gz = Decompress(bytes);
            return Encoding.UTF8.GetString(gz);
        }
        public static void Save(string saveto)
        {
            JObject res = new JObject();
            res["cComm"] = cComm.Data2Save();
            res["panels"] = new JObject();
            JObject oPanels = (JObject)res["panels"];
            //
            Monitor.Enter(_cs_current_panel);
            Monitor.Enter(_cs_panel_templates);
            foreach (string pid in _system_panels.Keys)
            {
                oPanels[pid] = new JObject();
                string filename = _system_panels[pid];
                JObject _panel = _panel_templates[filename];
                oPanels[pid]["panel"] = _panel;
                cInternalRel ir = _panel_internal_operators[pid];
                JObject ir2save = ir.Data2Save();
                oPanels[pid]["internal_rel"] = ir2save;
                cXmlConfigs cfg = GetPanelXMLConfigs(filename);
                oPanels[pid]["cfg"] = cfg.Save();
            }
            Monitor.Exit(_cs_panel_templates);
            Monitor.Exit(_cs_current_panel);
            //
            //StringBuilder sb = new StringBuilder();
            //StringWriter sw = new StringWriter(sb);
            //JsonWriter writer = new JsonTextWriter(sw);
            //res.WriteTo(writer);
            //
            byte[] gz = Compress(res.ToString());
            File.WriteAllBytes(saveto, gz);
            //System.IO.StreamWriter file = new System.IO.StreamWriter(saveto);
            //file.Write(sb);
            //file.Close();
            //writer.
            //File.WriteAllText(saveto, res.ToString());
            //
            //return res;
        }
        public static void SaveAs(string filename)
        {
            Save(filename);
        }
        public static JArray LoadFile(string filename)
        {
            ClearCache();
            byte[] b = File.ReadAllBytes(filename);
            string s = DecompressStr(b);
            JObject osys = JObject.Parse(s);
            //
            if (osys["cComm"] != null) cComm.Load((JObject)osys["cComm"]);
            //
            if (osys["panels"] != null && osys["panels"].Type != JTokenType.Null)
            {
                JObject panels = (JObject)osys["panels"];
                Monitor.Enter(_cs_current_panel);
                Monitor.Enter(_cs_panel_templates);
                Monitor.Enter(_cs_panel_xmls);
                //
                _current_panel_id = null;
                _panel_templates.Clear();
                _system_panels.Clear();
                _panel_internal_operators.Clear();
                _panel_xmls.Clear();
                //
                foreach (JProperty _panel_prop in panels.Properties())
                {
                    string _panel_id = _panel_prop.Name;
                    if (_current_panel_id == null) _current_panel_id = _panel_id;
                    JObject o = (JObject)_panel_prop.Value;
                    JObject opanel = (JObject)o["panel"];
                    JObject ointernal_rel = (JObject)o["internal_rel"];
                    JObject ocfg = (JObject)o["cfg"];
                    //
                    string _panel_type = opanel["~panel_type"].ToString();
                    cInternalRel _internal = null;
                    if (Regex.IsMatch(_panel_type, @"iris", RegexOptions.IgnoreCase))
                        _internal = new cInternalrelIRIS();

                    else
                        _internal = new cInternalRel();
                    _internal.Load(ointernal_rel);
                    _panel_internal_operators.Add(_panel_id, _internal);
                    //
                    cXmlConfigs cfg = new cXmlConfigs(_panel_type, jobj2string, jstring2obj);
                    cfg.Load(ocfg);
                    string f = opanel["~template_loaded_from"].ToString();
                    _panel_xmls.Add(f, cfg);
                    _panel_templates.Add(f, opanel);
                    _system_panels.Add(_panel_id, f);
                }
                //
                Monitor.Exit(_cs_panel_xmls);
                Monitor.Exit(_cs_panel_templates);
                Monitor.Exit(_cs_current_panel);
            }
            //
            return PanelsInLeftBrowser();
        }
        #endregion

        #region read/write
        private static object _cs_write_read_merge = new object();
        private static Dictionary<string, Dictionary<string, cRWPath>> _panel_write_read_merge = new Dictionary<string, Dictionary<string, cRWPath>>();

        private static Dictionary<string, cRWPath> WriteReadMerge
        {
            get
            {
                Monitor.Enter(_cs_write_read_merge);
                if (!_panel_write_read_merge.ContainsKey(CurrentPanelID))
                {
                    Monitor.Exit(_cs_write_read_merge);
                    return null;
                }
                Dictionary<string, cRWPath> res = _panel_write_read_merge[CurrentPanelID];
                Monitor.Exit(_cs_write_read_merge);
                return res;
            }
            set
            {
                Monitor.Enter(_cs_write_read_merge);
                if (!_panel_write_read_merge.ContainsKey(CurrentPanelID))
                    _panel_write_read_merge.Add(CurrentPanelID, value);
                else
                    _panel_write_read_merge[CurrentPanelID] = value;
                Monitor.Exit(_cs_write_read_merge);
            }
        }

        private static cRWPath RWLoopPath(Dictionary<string, cRWPath> _merge, string path)
        {
            if (_merge == null)
                return null;
            cRWPath rw = null;
            string pathext = null;
            if (Regex.IsMatch(path, "/"))
            {
                string[] sarr = path.Split('/');
                path = sarr[0];
                pathext = sarr[1];
            }
            path = Regex.Replace(path, @"^NO_", "", RegexOptions.IgnoreCase);
            foreach (string key in _merge.Keys)
            {
                if (Regex.IsMatch(key, path + "$") || Regex.IsMatch(_merge[key].ReadPath, path + "$") || Regex.IsMatch(key, path + "[\\/]") || Regex.IsMatch(_merge[key].ReadPath, path + "[\\/]"))
                {
                    if (pathext == null)
                    {
                        rw = _merge[key];
                        break;
                    }
                    else if (Regex.IsMatch(key, pathext + "$") || Regex.IsMatch(_merge[key].ReadPath, pathext + "$"))
                    {
                        rw = _merge[key];
                        break;
                    }
                }
            }
            //
            return rw;
        }
        private static void RWData(JObject _node)
        {
            Dictionary<string, cRWPath> _merge = WriteReadMerge;
            if (_merge == null)
                return;
            JToken t = _node["~path"];
            if (t == null)
                return;
            string path = t.ToString();
            path = Regex.Replace(path, @"^ELEMENTS\.", "", RegexOptions.IgnoreCase);
            if (path == "iris_peripheral_devices")
                path = "iris_peripherial_devices";
            cRWPath rw = null;
            if (!Regex.IsMatch(path, "_"))
            {
                if (_merge.ContainsKey(""))
                    rw = _merge[""];
                else
                    foreach (string key in _merge.Keys)
                        if (Regex.IsMatch(_merge[key].ReadPath, "PANEL$", RegexOptions.IgnoreCase) ||
                            Regex.IsMatch(_merge[key].ReadPath, "GENERAL_SETTINGS_R$", RegexOptions.IgnoreCase) ||
                            Regex.IsMatch(_merge[key].ReadPath, @"^SIMPO_GENERAL_SETTINGS$", RegexOptions.IgnoreCase))
                        {
                            rw = _merge[key];
                            break;
                        }
            }
            else if (Regex.IsMatch(path, @"loop\d", RegexOptions.IgnoreCase))
                rw = RWLoopPath(_merge, path);
            else
            {
                foreach (string key in _merge.Keys)
                {
                    if (path == "natron_device")
                    {
                        rw = _merge[key];
                        break;
                    }
                    if (key.ToLower() == path.ToLower() || _merge[key].ReadPath.ToLower() == path.ToLower())
                    {
                        rw = _merge[key];
                        break;
                    }
                    string mimic_path = "SIMPO_MIMICPANELS/" + path;
                    if (key.ToLower() == mimic_path.ToLower())
                    {
                        rw = _merge[key];
                        break;
                    }
                    if (Regex.IsMatch(path, @"^SIMPO_MIMIC\d+$")) continue;
                    string path1 = Regex.Replace(path, @"(^[\w\W]+?)_([^_]+$)", "$1$2", RegexOptions.IgnoreCase);
                    if (key.ToLower() == path1.ToLower() || _merge[key].ReadPath.ToLower() == path1.ToLower())
                    {
                        rw = _merge[key];
                        break;
                    }
                    if (Regex.IsMatch(key, "^" + path1, RegexOptions.IgnoreCase) || Regex.IsMatch(_merge[key].ReadPath, "^" + path1, RegexOptions.IgnoreCase))
                    {
                        rw = _merge[key];
                        break;
                    }
                    if (Regex.IsMatch(key, path1 + "$", RegexOptions.IgnoreCase) || Regex.IsMatch(_merge[key].ReadPath, path1 + "$", RegexOptions.IgnoreCase))
                    {
                        rw = _merge[key];
                        break;
                    }
                    path1 = Regex.Replace(path, @"(^[\w\W]+?)_([^_]+$)", "$1$2", RegexOptions.IgnoreCase);
                    path1 = Regex.Replace(path1, @"(^[\w\W]+?)_([^_]+$)", "$1$2", RegexOptions.IgnoreCase);
                    if (key.ToLower() == path1.ToLower() || _merge[key].ReadPath.ToLower() == path1.ToLower())
                    {
                        rw = _merge[key];
                        break;
                    }
                    if (Regex.IsMatch(key, "^" + path1, RegexOptions.IgnoreCase) || Regex.IsMatch(_merge[key].ReadPath, "^" + path1, RegexOptions.IgnoreCase))
                    {
                        rw = _merge[key];
                        break;
                    }
                    if (Regex.IsMatch(key, path1 + "$", RegexOptions.IgnoreCase) || Regex.IsMatch(_merge[key].ReadPath, path1 + "$", RegexOptions.IgnoreCase))
                    {
                        rw = _merge[key];
                        break;
                    }
                    if (Regex.IsMatch(key, "^" + path, RegexOptions.IgnoreCase) || Regex.IsMatch(_merge[key].ReadPath, "^" + path, RegexOptions.IgnoreCase))
                    {
                        rw = _merge[key];
                        break;
                    }
                    if (Regex.IsMatch(key, path + "$", RegexOptions.IgnoreCase) || Regex.IsMatch(_merge[key].ReadPath, path + "$", RegexOptions.IgnoreCase))
                    {
                        rw = _merge[key];
                        break;
                    }
                    path1 = Regex.Replace(path, @"^\w+?_", "");
                    if (key != "" && (Regex.IsMatch(key, path1 + "$", RegexOptions.IgnoreCase) || Regex.IsMatch(_merge[key].ReadPath, path1 + "$", RegexOptions.IgnoreCase)))
                    {
                        rw = _merge[key];
                        break;
                    }
                    path1 = Regex.Replace(path1, @"_", "");
                    if (Regex.IsMatch(key, path1 + "$", RegexOptions.IgnoreCase) || Regex.IsMatch(_merge[key].ReadPath, path1 + "$", RegexOptions.IgnoreCase))
                    {
                        rw = _merge[key];
                        break;
                    }
                    if (Regex.IsMatch(path, @"panel[\w\W]+?network", RegexOptions.IgnoreCase) && Regex.IsMatch(key, @"panel[\w\W]+?network", RegexOptions.IgnoreCase))
                    {
                        rw = _merge[key];
                        break;
                    }
                    path1 = Regex.Replace(path, "^iris_", "simpo_");
                    if (Regex.IsMatch(key, "^" + path1, RegexOptions.IgnoreCase))
                    {
                        rw = _merge[key];
                        break;
                    }
                    path = path + "";
                }
            }
            if (rw != null)
            {
                JObject o = JObject.FromObject(rw);
                _node["~rw"] = o;
            }
            else
                return;
        }

        private static bool readed = false;
        private static bool reading = false;
        private static Dictionary<string, byte[]> _log_bytesreaded = new Dictionary<string, byte[]>();
        private static Dictionary<string, string> _log_byteswrite = new Dictionary<string, string>();
        private static Dictionary<string, Dictionary<string, Dictionary<string, byte[]>>> ReadSimpoLoopDevices(cTransport conn, cRWPath p, byte min, byte max, string read_path, JObject devtypes)
        {
            Dictionary<string, Dictionary<string, Dictionary<string, byte[]>>> res = new Dictionary<string, Dictionary<string, Dictionary<string, byte[]>>>();
            //
            return res;
        }
        private static Dictionary<string, Dictionary<string, Dictionary<string, byte[]>>> ReadLoopDevices(cTransport conn, cRWPath p, byte min, byte max, string read_path, JObject devtypes)
        {
            Dictionary<string, Dictionary<string, Dictionary<string, byte[]>>> res = new Dictionary<string, Dictionary<string, Dictionary<string, byte[]>>>();
            List<cRWCommand> cmds = new List<cRWCommand>();
            //Dictionary<string, cRWProperty> ReadProperties = new Dictionary<string, cRWProperty>();
            if (CurrentPanelType == "iris")
            {
                cRWPathIRIS pi = (cRWPathIRIS)p;
                foreach (cRWCommandIRIS cmd in pi.ReadCommands)
                    cmds.Add(cmd);
            }
            else
            {
                foreach (cRWCommand cmd in p.ReadCommands)
                    cmds.Add(cmd);
            }
            //
            Match m = Regex.Match(read_path, @"LOOP(\d+)", RegexOptions.IgnoreCase);
            if (!m.Success)
            {
                if (Regex.IsMatch(read_path, @"SIMPO_MIMICPANELS\s*?/\s*?SIMPO_MIMICOUT")) read_path = "SIMPO_MIMIC1/SIMPO_MIMICOUT";
                read_path = Regex.Replace(read_path, @"\s+", "");
                m = Regex.Match(read_path, @"SIMPO_MIMIC(\d+?)/");
                if (!m.Success)
                {
                    m = Regex.Match(read_path, @"SIMPO_TTENONE(\d+)$");
                    if (!m.Success)
                        return res;
                    read_path = "SIMPO_TTELOOP" + m.Groups[1].Value + "/" + "SIMPO_TTENONE";
                }
            }
            string num = m.Groups[1].Value;
            JObject panel = CurrentPanel;
            string panel_id = CurrentPanelID;
            string[] apath = read_path.Split('/');
            string dev_path = "";
            string none_element = "";
            string loop_type = "";
            JObject groups = null;
            for (int i = 0; i < apath.Length; i++)
                if (Regex.IsMatch(apath[i], @"(LOOP\d+|NONE)$"))
                {
                    if (Regex.IsMatch(apath[i], @"LOOP\d+$"))
                        loop_type = apath[i];
                    dev_path += ((dev_path != "") ? "/" : "") + apath[i];
                    if (Regex.IsMatch(apath[i], @"NONE$"))
                        none_element = apath[i];
                }
            string loopkey = null;
            string nonekey = null;
            m = Regex.Match(read_path, @"([\w\W]*?" + loop_type + @")/([\w\W]+)$");
            if (m.Success)
            {
                loopkey = m.Groups[1].Value;
                nonekey = m.Groups[2].Value;
            }
            if (!res.ContainsKey(loopkey))
                res.Add(loopkey, new Dictionary<string, Dictionary<string, byte[]>>());
            Dictionary<string, Dictionary<string, byte[]>> dnone = res[loopkey];
            if (!dnone.ContainsKey(nonekey))
                dnone.Add(nonekey, new Dictionary<string, byte[]>());
            Dictionary<string, byte[]> didx = dnone[nonekey];
            //
            string jnode = "{}";
            JToken t = panel["ELEMENTS"]["NO_LOOP" + num];
            if (t != null)
                jnode = t.ToString();
            dev_path = apath[apath.Length - 1];
            //
            JObject el = GetNode(dev_path);
            if (el != null && el["PROPERTIES"] != null && el["PROPERTIES"]["Groups"] != null)
                groups = (JObject)el["PROPERTIES"]["Groups"];
            JObject _rw = (JObject)el["~rw"];
            int typeidx = -1;
            if (_rw["ReadProperties"] != null)
            {
                JToken trp = _rw["ReadProperties"];
                foreach (JProperty tprop in trp)
                    if (tprop.Name.ToLower() == "type" || (Regex.IsMatch(dev_path, @"^SIMPO_") && tprop.Name.ToLower() == "activation1"))
                    {
                        JObject otype = (JObject)tprop.Value;
                        if (otype["offset"] != null)
                            typeidx = Convert.ToInt32(otype["offset"].ToString());
                        break;
                    }
            }
            int idxinc = 0;
            if (min == 1)
            {
                min--;
                max--;
                idxinc = 1;
            }
            for (byte idx = min; idx <= max; idx++)
            {
                int reslen = 0;
                List<byte[]> cmdresults = new List<byte[]>();
                foreach (cRWCommand cmd in cmds)
                {
                    cmd.io = eIO.ioRead;
                    string scmd = cmd.CommandString();
                    if (!Regex.IsMatch(dev_path, @"^SIMPO_") || Regex.IsMatch(dev_path, @"_TTENONE$"))
                        scmd = scmd.Substring(0, cmd.idxPosition() * 2) + idx.ToString("X2") + scmd.Substring((cmd.idxPosition() + 1) * 2);
                    else if (!Regex.IsMatch(dev_path, @"MIMICOUT"))
                        scmd = cmd.CommandString(Convert.ToInt32(num) - 1, idx);
                    else
                        scmd = cmd.CommandStringSubIdxOnly(idx);
                    byte[] cmdres = cComm.SendCommand(conn, scmd);
                    if (settings.logreads)
                    {
                        if (!_log_bytesreaded.ContainsKey(scmd))
                            _log_bytesreaded.Add(scmd, cmdres);
                    }
                    reslen += cmdres.Length;
                    cmdresults.Add(cmdres);
                }
                byte[] bcmdres = new byte[reslen];
                int bidx = 0;
                foreach (byte[] b in cmdresults)
                {
                    b.CopyTo(bcmdres, bidx);
                    bidx += b.Length;
                }
                if (idx > 56)
                {
                    bidx = bidx;
                }
                string devtype = Regex.Replace(nonekey, @"/[\w\W]*$", "");
                JObject devbytype = null;
                if (devtypes[devtype] != null)
                    devbytype = (JObject)devtypes[devtype];
                string sdevname = null;
                if (devbytype != null && devbytype[bcmdres[typeidx].ToString()] != null)
                    sdevname = devbytype[bcmdres[typeidx].ToString()].ToString();
                if (typeidx >= 0 && bcmdres[typeidx] != 0 && (sdevname != null || Regex.IsMatch(dev_path, @"^SIMPO_")))
                {
                    didx.Add((idx + idxinc).ToString(), bcmdres);
                }
            }
            //
            if (didx.Count == 0)
                res.Clear();
            return res;
        }
        private static void SetLoopDevicesValues(string loopkey, Dictionary<string, Dictionary<string, byte[]>> dreaded, JObject devtypes, Dictionary<string, string> missedkeys, Dictionary<string, string> foundkeys, Dictionary<string, string> dloopdevs)
        {
            if (dreaded == null || dreaded.Count <= 0)
                return;
            Dictionary<string, cRWPath> merge = WriteReadMerge;
            JObject panel = CurrentPanel;
            string panel_id = CurrentPanelID;
            Dictionary<string, string> dres = null;
            if (!Regex.IsMatch(loopkey, @"SIMPO_"))
                dres = cComm.GetPseudoElementsList(panel_id, constants.NO_LOOP);
            else
            {
                dres = cComm.GetPseudoElementsList(panel_id, constants.NO_LOOP, loopkey);
                if (dres == null && Regex.IsMatch(loopkey, @"^SIMPO_[\w\W]*?(LOOP|MIMIC)"))
                    dres = cComm.GetPseudoElementsList(panel_id, Regex.Replace(loopkey, @"\d+$", ""), loopkey);
            }
            //SIMPO_TTELOOP1
            int num = ((dres != null) ? dres.Count : 0) + 1;
            string read_path = loopkey + "/" + dreaded.Keys.First();
            string[] apath = read_path.Split('/');
            string dev_path = "";
            string none_element = "";
            string loop_type = "";
            bool loopadded = false;
            JObject groups = null;
            //
            for (int i = 0; i < apath.Length; i++)
                if (Regex.IsMatch(apath[i], @"(LOOP\d+|NONE)$") || Regex.IsMatch(apath[i], @"^SIMPO_MIMIC"))
                {
                    if (Regex.IsMatch(apath[i], @"LOOP\d+$") || Regex.IsMatch(apath[i], @"^SIMPO_MIMIC\d"))
                        loop_type = apath[i];
                    dev_path += ((dev_path != "") ? "/" : "") + apath[i];
                }
            if (!Regex.IsMatch(loop_type, @"MIMIC"))
                loop_type = Regex.Replace(loop_type, @"\d+$", num.ToString());
            //
            foreach (string snone in dreaded.Keys)
            {
                string nonekey = loopkey + "/" + snone;
                if (!merge.ContainsKey(nonekey))
                {
                    if (!Regex.IsMatch(nonekey, @"^SIMPO_TTE[\w\W]+?/"))
                        nonekey = Regex.Replace(nonekey, @"/[^/]+$", "");
                    else
                    {
                        Match mkey = Regex.Match(nonekey, @"^(SIMPO_TTE[\w\W]+?)(\d+?)([\w\W]+?)$");
                        if (mkey.Success)
                            nonekey = mkey.Groups[1].Value + mkey.Groups[3].Value + mkey.Groups[2].Value;
                    }
                }
                if (!merge.ContainsKey(nonekey))
                {
                    if (merge.ContainsKey("IRIS_LOOPDEVICES/" + nonekey))
                        nonekey = "IRIS_LOOPDEVICES/" + nonekey;
                    else if (merge.ContainsKey("IRIS8_LOOPDEVICES/" + nonekey))
                        nonekey = "IRIS8_LOOPDEVICES/" + nonekey;
                    else if (merge.ContainsKey("IRIS4_LOOPDEVICES/" + nonekey))
                        nonekey = "IRIS4_LOOPDEVICES/" + nonekey;
                    else
                    {
                        Match mnom = Regex.Match(loopkey, @"(\d+)$");
                        if (mnom.Success)
                        {
                            string snom = mnom.Groups[1].Value;
                            if (merge.ContainsKey("SIMPO_LOOPDEVICES/SIMPO_TTENONE" + snom))
                                nonekey = "SIMPO_LOOPDEVICES/SIMPO_TTENONE" + snom;
                        }
                    }
                }
                if (merge.ContainsKey(nonekey))
                {
                    Dictionary<string, cRWProperty> ReadProperties = new Dictionary<string, cRWProperty>();
                    cRWPath p = merge[nonekey];
                    int typeidx = -1;
                    foreach (string k in p.ReadProperties.Keys)
                    {
                        cRWProperty prop = p.ReadProperties[k];
                        ReadProperties.Add(k, prop);
                        if (typeidx <= 0 && (Regex.IsMatch(k, "^type$", RegexOptions.IgnoreCase) || Regex.IsMatch(k, "^activation1$", RegexOptions.IgnoreCase)))
                            typeidx = prop.offset;
                    }
                    if (typeidx < 0)
                    {
                        Match mnokey = Regex.Match(nonekey, @"^SIMPO_[\w\W]*?LOOP[\w\W]*?(\d+)$");
                        if (mnokey.Success)
                        {
                            string lnom = mnokey.Groups[1].Value;
                            foreach (string mkey in merge.Keys)
                                if (Regex.IsMatch(mkey, "SIMPO_TTENONE" + lnom + "$"))
                                {
                                    nonekey = mkey;
                                    break;
                                }
                            if (merge.ContainsKey(nonekey))
                            {
                                p = merge[nonekey];
                                foreach (string k in p.ReadProperties.Keys)
                                {
                                    cRWProperty prop = p.ReadProperties[k];
                                    ReadProperties.Add(k, prop);
                                    if (typeidx <= 0 && (Regex.IsMatch(k, "^type$", RegexOptions.IgnoreCase) || Regex.IsMatch(k, "^activation1$", RegexOptions.IgnoreCase)))
                                    {
                                        typeidx = prop.offset;
                                        break;
                                    }
                                }
                            }
                        }
                    }
                    //
                    if (!loopadded)
                    {
                        string noloop_key = constants.NO_LOOP;
                        string jnode = "{}";
                        JToken t = null;
                        if (!Regex.IsMatch(loopkey, @"^SIMPO_"))
                            t = panel["ELEMENTS"][constants.NO_LOOP + num.ToString()];
                        if (t == null && Regex.IsMatch(loopkey, @"^SIMPO_"))
                        {
                            t = panel["ELEMENTS"][loopkey];
                            noloop_key = loopkey;
                            if (Regex.IsMatch(noloop_key, @"LOOP"))
                                noloop_key = Regex.Replace(noloop_key, @"\d+$", "");
                        }
                        if (t != null)
                            jnode = t.ToString();
                        cComm.AddPseudoElement(panel_id, /*constants.NO_LOOP*/noloop_key, jnode);
                        JObject o = JObject.Parse(cComm.GetPseudoElement(panel_id, /*constants.NO_LOOP*/noloop_key));
                        o["~loop_type"] = loop_type;
                        cComm.SetPseudoElement(panel_id, /*constants.NO_LOOP*/noloop_key, o.ToString());
                        loopadded = true;
                    }
                    dev_path = Regex.Replace(snone, @"^[\w\W]*?/", "");// apath[apath.Length - 1];
                    none_element = dev_path;
                    //
                    JObject el = GetNode(dev_path);
                    el["~rw"] = JObject.FromObject(p);
                    if (el != null && el["PROPERTIES"] != null && el["PROPERTIES"]["Groups"] != null)
                        groups = (JObject)el["PROPERTIES"]["Groups"];
                    JObject _rw = (JObject)el["~rw"];
                    //
                    Dictionary<string, byte[]> didx = dreaded[snone];
                    foreach (string idx in didx.Keys)
                    {
                        byte btype = didx[idx][typeidx];
                        string devname = null;
                        if (!Regex.IsMatch(none_element, @"^SIMPO_") || Regex.IsMatch(loopkey, "loop", RegexOptions.IgnoreCase))
                            devname = (devtypes[btype.ToString()] != null) ? devtypes[btype.ToString()].ToString() : none_element;
                        else
                            devname = none_element;
                        el["~device"] = devname;
                        el["~device_type"] = none_element;
                        string dev_save_path = loop_type + "/" + none_element + "#" + devname;
                        //IRIS_LOOP1/IRIS_MNONE#IRIS_MM220E
                        //
                        JObject el1 = GetNode(devname);// new JObject(el);
                        el1 = (JObject)el1["PROPERTIES"]["Groups"];
                        el1 = cJson.ChangeGroupsElementsPath(el1, Regex.Replace(idx, "^0+", ""));
                        cJson.MakeRelativePath(el1, dev_save_path);
                        el1["~rw"] = _rw;
                        el1["~device"] = devname;
                        el1["~device_type"] = none_element;
                        groups = el1;
                        //
                        cComm.AddListElement(panel_id, dev_save_path, Regex.Replace(idx, "^0+", ""), el1.ToString());
                        //
                        byte[] devbytes = didx[idx];
                        foreach (string propname in ReadProperties.Keys)
                        {
                            cRWProperty prop = ReadProperties[propname];
                            byte[] pbytes = new byte[prop.bytescnt];
                            for (int i = prop.offset; i < prop.offset + prop.bytescnt; i++)
                                pbytes[i - prop.offset] = devbytes[i];
                            Tuple<string, string> path_val = _internal_relations_operator.GroupPropertyVal(CurrentPanelID, groups, propname, pbytes, prop.xmltag);
                            if (path_val == null)
                            {
                                if (!missedkeys.ContainsKey(dev_save_path + "/" + propname))
                                    missedkeys.Add(dev_save_path + "/" + propname, "");
                            }
                            else
                            {
                                cComm.SetPathValue(panel_id, path_val.Item1, path_val.Item2, FilterValueChanged);
                                if (!foundkeys.ContainsKey(dev_save_path + "/" + propname))
                                    foundkeys.Add(dev_save_path + "/" + propname, "");
                            }
                        }
                    }
                    //
                }
            }
        }
        private static Dictionary<string, Dictionary<string, Dictionary<string, byte[]>>> ReadAllMIMICPanels(cTransport conn, cRWPath p, byte min, byte max, string read_path, JObject devtypes)
        {
            Dictionary<string, Dictionary<string, Dictionary<string, byte[]>>> res = new Dictionary<string, Dictionary<string, Dictionary<string, byte[]>>>();
            Dictionary<string, cRWPath> _merge = WriteReadMerge;
            //string rp = Regex.Replace(read_path, @"\d+$", "");
            foreach (string key in _merge.Keys)
                if (Regex.IsMatch(key, "SIMPO_MIMICOUT$"))
                {
                    cRWPath loopp = _merge[key];
                    if (CurrentPanelType == "iris")
                    {
                        cRWPathIRIS pi = new cRWPathIRIS();
                        foreach (cRWCommandIRIS cmd in loopp.ReadCommands)
                            pi.ReadCommands.Add(cmd);
                        pi.ReadPath = loopp.ReadPath;
                        foreach (string prop in loopp.ReadProperties.Keys)
                            pi.ReadProperties.Add(prop, (cRWPropertyIRIS)loopp.ReadProperties[prop]);
                        //pi.ReadProperties = loopp.ReadProperties;
                        //cRWPathIRIS pi = (cRWPathIRIS)_merge[key];
                        //foreach (cRWCommandIRIS cmd in pi.ReadCommands)
                        //    cmds.Add(cmd);
                        loopp = pi;
                    }
                    else
                        loopp = _merge[key];
                    Dictionary<string, Dictionary<string, Dictionary<string, byte[]>>> lres = ReadLoopDevices(conn, loopp, min, max, loopp.ReadPath, devtypes);
                    foreach (string reskey in lres.Keys)
                        res.Add(reskey, lres[reskey]);
                }
            //
            return res;
        }
        private static Dictionary<string, Dictionary<string, Dictionary<string, byte[]>>> ReadAllSimpoLoopDevices(cTransport conn, cRWPath p, byte min, byte max, string read_path, JObject devtypes)
        {
            Dictionary<string, Dictionary<string, Dictionary<string, byte[]>>> res = new Dictionary<string, Dictionary<string, Dictionary<string, byte[]>>>();
            Dictionary<string, cRWPath> _merge = WriteReadMerge;
            string rp = Regex.Replace(read_path, @"\d+$", "");
            foreach (string key in _merge.Keys)
                if (Regex.IsMatch(key, rp + @"\d+$"))
                {
                    cRWPath loopp = _merge[key];
                    if (CurrentPanelType == "iris")
                    {
                        cRWPathIRIS pi = new cRWPathIRIS();
                        foreach (cRWCommandIRIS cmd in loopp.ReadCommands)
                            pi.ReadCommands.Add(cmd);
                        pi.ReadPath = loopp.ReadPath;
                        foreach (string prop in loopp.ReadProperties.Keys)
                            pi.ReadProperties.Add(prop, (cRWPropertyIRIS)loopp.ReadProperties[prop]);
                        //pi.ReadProperties = loopp.ReadProperties;
                        //cRWPathIRIS pi = (cRWPathIRIS)_merge[key];
                        //foreach (cRWCommandIRIS cmd in pi.ReadCommands)
                        //    cmds.Add(cmd);
                        loopp = pi;
                    }
                    else
                        loopp = _merge[key];
                    Dictionary<string, Dictionary<string, Dictionary<string, byte[]>>> lres = ReadLoopDevices(conn, loopp, min, max, loopp.ReadPath, devtypes);
                    foreach (string reskey in lres.Keys)
                        res.Add(reskey, lres[reskey]);
                }
            //
            return res;
        }
        private static Dictionary<string, Dictionary<string, Dictionary<string, byte[]>>> ReadAllLoopDevices(cTransport conn, cRWPath p, byte min, byte max, string read_path, JObject devtypes)
        {
            Dictionary<string, Dictionary<string, Dictionary<string, byte[]>>> res = new Dictionary<string, Dictionary<string, Dictionary<string, byte[]>>>();
            Dictionary<string, cRWPath> _merge = WriteReadMerge;
            //
            string rp = Regex.Replace(read_path, @"/IRIS\d*?_(TTENONE|MNONE|SNONE)$", "");
            string mask = Regex.Replace(rp, @"LOOP\d", @"LOOP\d");
            //
            List<string> loopkeys = new List<string>();
            foreach (string key in _merge.Keys)
                if (Regex.IsMatch(key, mask))
                {
                    string looptype = "";
                    if (Regex.IsMatch(key, "TTE")) looptype = "tte";
                    else if (Regex.IsMatch(key, "(SENSOR|MODULE)")) looptype = "sens";
                    int loopnom = -1;
                    Match mnom = Regex.Match(key, @"LOOP(\d+)");
                    if (mnom.Success) loopnom = Convert.ToInt32(mnom.Groups[1].Value);
                    if (looptype == "tte" && loopnom <= _tteloops_count_in_peripherial_devs ||
                        looptype == "sens" && loopnom <= _sensloops_count_in_peripherial_devs)
                        loopkeys.Add(key);
                }
            foreach (string key in loopkeys)
            {
                cRWPath loopp = _merge[key];
                if (CurrentPanelType == "iris")
                {
                    cRWPathIRIS pi = new cRWPathIRIS();
                    foreach (cRWCommandIRIS cmd in loopp.ReadCommands)
                        pi.ReadCommands.Add(cmd);
                    pi.ReadPath = loopp.ReadPath;
                    foreach (string prop in loopp.ReadProperties.Keys)
                        pi.ReadProperties.Add(prop, (cRWPropertyIRIS)loopp.ReadProperties[prop]);
                    //pi.ReadProperties = loopp.ReadProperties;
                    //cRWPathIRIS pi = (cRWPathIRIS)_merge[key];
                    //foreach (cRWCommandIRIS cmd in pi.ReadCommands)
                    //    cmds.Add(cmd);
                    loopp = pi;
                }
                else
                    loopp = _merge[key];
                Dictionary<string, Dictionary<string, Dictionary<string, byte[]>>> lres = ReadLoopDevices(conn, loopp, min, max, loopp.ReadPath, devtypes);
                foreach (string reskey in lres.Keys)
                    res.Add(reskey, lres[reskey]);
            }
            //
            return res;
        }
        private static int _tteloops_count_in_peripherial_devs = -1;
        private static int _sensloops_count_in_peripherial_devs = -1;
        private static Dictionary<string, Dictionary<string, Dictionary<string, byte[]>>> ReadSeriaDevices(cTransport conn, cRWPath p, byte min, byte max, string read_path, JObject devtypes)
        {
            //if (Regex.IsMatch(read_path, @"SIMPO_TTE"))
            //    return ReadSimpoLoopDevices(conn, p, min, max, read_path, devtypes);
            //
            if (Regex.IsMatch(read_path, @"LOOP"))
                //return ReadLoopDevices(conn, p, min, max, read_path, devtypes);
                return ReadAllLoopDevices(conn, p, min, max, read_path, devtypes);
            //
            if (Regex.IsMatch(read_path, @"SIMPO_MIMICOUT$"))
                return ReadAllMIMICPanels(conn, p, min, max, read_path, devtypes);
            if (Regex.IsMatch(read_path, @"SIMPO_TTE"))
                return ReadAllSimpoLoopDevices(conn, p, min, max, read_path, devtypes);
            Dictionary<string, Dictionary<string, Dictionary<string, byte[]>>> res = new Dictionary<string, Dictionary<string, Dictionary<string, byte[]>>>();
            JObject pdtypes = (JObject)CurrentPanel["~pdtypes"];
            if (pdtypes == null) pdtypes = new JObject();
            //if (Regex.IsMatch(read_path, @"(INPUTS_GR|INPUTS/|OUTPUTS|PERIPHER)"))
            //{
            List<cRWCommand> cmds = new List<cRWCommand>();
            int typeidx = -1;
            Dictionary<string, cRWProperty> read_props = new Dictionary<string, cRWProperty>();
            if (CurrentPanelType == "iris")
            {
                cRWPathIRIS pi = (cRWPathIRIS)p;
                foreach (cRWCommandIRIS cmd in pi.ReadCommands)
                    cmds.Add(cmd);
                foreach (string rpk in pi.ReadProperties.Keys)
                {
                    read_props.Add(rpk, (cRWPropertyIRIS)pi.ReadProperties[rpk]);
                    if (Regex.IsMatch(read_path, @"^IRIS\d*?_PERIPHER", RegexOptions.IgnoreCase) && Regex.IsMatch(rpk, @"type", RegexOptions.IgnoreCase))
                        typeidx = read_props[rpk].offset;
                }
            }
            else if (CurrentPanelType == "natron")
            {
                cRWPathNatron pn = (cRWPathNatron)p;
                foreach (cRWCommandNatron cmd in pn.ReadCommands)
                    cmds.Add(cmd);
                foreach (string rpk in pn.ReadProperties.Keys)
                {
                    read_props.Add(rpk, (cRWPropertyNatron)pn.ReadProperties[rpk]);
                    if (Regex.IsMatch(rpk, @"type", RegexOptions.IgnoreCase))
                        typeidx = read_props[rpk].offset;
                }
            }
            else
            {
                foreach (cRWCommand cmd in p.ReadCommands)
                    cmds.Add(cmd);
                foreach (string rpk in p.ReadProperties.Keys)
                    read_props.Add(rpk, p.ReadProperties[rpk]);
            }
            //
            if (!Regex.IsMatch(read_path, "/"))
                read_path += "/" + read_path;
            string[] apath = read_path.Split('/');
            res.Add(apath[0], new Dictionary<string, Dictionary<string, byte[]>>());
            Dictionary<string, Dictionary<string, byte[]>> droot = res[apath[0]];
            droot.Add(apath[1], new Dictionary<string, byte[]>());
            Dictionary<string, byte[]> didx = droot[apath[1]];
            int idxinc = 0;
            if (min == 1)
            {
                min--;
                max--;
                idxinc = 1;
            }
            //
            for (byte idx = min; idx <= max; idx++)
            {
                int reslen = 0;
                //if (idx == 64 && read_path == "IRIS_PANELSINNETWORK/IRIS_PANELINNETWORK")
                //    reslen = reslen;
                List<byte[]> cmdresults = new List<byte[]>();
                if (!p.ReadCommandsReplacement.ContainsKey(idx.ToString("X2")))
                    foreach (cRWCommand cmd in cmds)
                    {
                        string scmd = cmd.CommandString();
                        if (!Regex.IsMatch(read_path, @"SIMPO_MIMICOUT$") && !Regex.IsMatch(read_path, @"natron", RegexOptions.IgnoreCase))
                            scmd = scmd.Substring(0, cmd.idxPosition() * 2) + idx.ToString("X2") + scmd.Substring((cmd.idxPosition() + 1) * 2);
                        else if (Regex.IsMatch(read_path, @"natron", RegexOptions.IgnoreCase))
                        {
                            cmd.io = eIO.ioRead;
                            scmd = cmd.CommandString(idx);
                        }
                        else
                        {
                            cmd.io = eIO.ioRead;
                            scmd = cmd.CommandString(0, idx);
                        }
                        byte[] cmdres = cComm.SendCommand(conn, scmd);
                        ////
                        //if (Regex.IsMatch(read_path, @"(OUTPUT)"))
                        //{
                        //    string first = res.Keys.First();
                        //}
                        ////
                        if (settings.logreads)
                        {
                            if (!_log_bytesreaded.ContainsKey(scmd))
                                _log_bytesreaded.Add(scmd, cmdres);
                        }
                        reslen += cmdres.Length;
                        cmdresults.Add(cmdres);
                    }
                else
                    foreach (string scmd in p.ReadCommandsReplacement[idx.ToString("X2")])
                    {
                        byte[] cmdres = cComm.SendCommand(conn, scmd);
                        ////
                        //if (Regex.IsMatch(read_path, @"(OUTPUT)"))
                        //{
                        //    string first = res.Keys.First();
                        //}
                        ////
                        if (settings.logreads)
                        {
                            if (!_log_bytesreaded.ContainsKey(scmd))
                                _log_bytesreaded.Add(scmd, cmdres);
                        }
                        reslen += cmdres.Length;
                        cmdresults.Add(cmdres);
                    }
                byte[] bcmdres = new byte[reslen];
                int bidx = 0;
                foreach (byte[] b in cmdresults)
                {
                    b.CopyTo(bcmdres, bidx);
                    bidx += b.Length;
                }
                //
                JObject node = GetNode(apath[1]);
                bool adddev = true;
                if (typeidx >= 0 && typeidx < bcmdres.Length)
                {
                    byte btype = bcmdres[typeidx];
                    string devname = (pdtypes[btype.ToString()] != null) ? pdtypes[btype.ToString()].ToString() : "";
                    adddev = devname != "" && !Regex.IsMatch(devname, @"NONE$");
                    if (Regex.IsMatch(devname, @"^IRIS[\w\W]+?TTELOOP$"))
                    {
                        if (_tteloops_count_in_peripherial_devs > 0) _tteloops_count_in_peripherial_devs++;
                        else _tteloops_count_in_peripherial_devs = 1;
                    }
                    else if (Regex.IsMatch(devname, @"^IRIS[\w\W]+?LOOP$"))
                    {
                        if (_sensloops_count_in_peripherial_devs > 0) _sensloops_count_in_peripherial_devs++;
                        else _sensloops_count_in_peripherial_devs = 1;
                    }
                }
                //if (Regex.IsMatch(read_path, @"(OUTPUT)"))
                //{
                //    string first = res.Keys.First();
                //}
                if (_internal_relations_operator.AddSerialDevice(apath[1], node, bcmdres, (byte)(idx + idxinc), read_props))
                    didx.Add((idx + idxinc).ToString(), bcmdres);
            }
            //if (Regex.IsMatch(read_path, @"(OUTPUT)"))
            //{
            //    string first = res.Keys.First();
            //}
            //
            if (didx.Count == 0)
                res.Clear();
            return res;
            //}
            //
            //return res;
        }
        private static void SetSeriaDevicesValues(Dictionary<string, Dictionary<string, Dictionary<string, byte[]>>> dreaded, JObject devtypes, Dictionary<string, string> missedkeys, Dictionary<string, string> foundkeys, Dictionary<string, string> dloopdevs)
        {
            if (dreaded == null || dreaded.Count <= 0)
                return;
            foreach (string loopkey in dreaded.Keys)
            {
                if (Regex.IsMatch(loopkey, @"LOOP") || Regex.IsMatch(loopkey, @"SIMPO_MIMIC\d+$"))
                {
                    SetLoopDevicesValues(loopkey, dreaded[loopkey], devtypes, missedkeys, foundkeys, dloopdevs);
                    continue;
                }
                Dictionary<string, cRWPath> merge = WriteReadMerge;
                string readkey = loopkey;
                string readsubkey = dreaded[readkey].Keys.First();
                string key2adddev = readsubkey;
                string paneltypepref = Regex.Replace(CurrentPanelFullType, @"\d+$", "") + "_";
                key2adddev = Regex.Replace(key2adddev, "^" + paneltypepref.ToUpper(), CurrentPanelFullType.ToUpper() + "_");
                cRWPath rwpath = null;
                if (merge.ContainsKey(readkey))
                    rwpath = merge[readkey];
                else if (merge.ContainsKey(_internal_relations_operator.FindElementKey(readkey, CurrentPanel)))
                    rwpath = merge[_internal_relations_operator.FindElementKey(readkey, CurrentPanel)];
                else if (merge.ContainsKey(readsubkey))
                    rwpath = merge[readsubkey];
                else if (merge.ContainsKey(readkey + "/" + readsubkey))
                    rwpath = merge[readkey + "/" + readsubkey];
                if (rwpath == null && CurrentPanelType == "iris")
                {
                    string rk = Regex.Replace(readkey, "^IRIS_", "IRIS8_");
                    if (merge.ContainsKey(rk))
                        rwpath = merge[rk];
                }
                //!!!!!!!!!!!!!!!!!
                if (rwpath == null) continue;
                //
                Dictionary<string, cRWProperty> ReadProperties = rwpath.ReadProperties;
                JObject panel = CurrentPanel;
                string panel_id = CurrentPanelID;
                JObject groups = null;
                JObject node = GetNode(readsubkey);
                Dictionary<string, byte[]> didx = dreaded[readkey][readsubkey];
                int typeidx = -1;
                if (Regex.IsMatch(rwpath.ReadPath, "peripher", RegexOptions.IgnoreCase))
                {
                    foreach (string k in rwpath.ReadProperties.Keys)
                    {
                        cRWProperty prop = rwpath.ReadProperties[k];
                        //ReadProperties.Add(k, prop);
                        if (typeidx <= 0 && Regex.IsMatch(k, "^type$", RegexOptions.IgnoreCase))
                        {
                            typeidx = prop.offset;
                            break;
                        }
                    }
                }
                //
                byte? condidx = null;
                byte? condval = null;
                JObject fields2inc = settings.inc_fields_on_read;
                JObject incpaths = null;
                if (fields2inc != null)
                {
                    incpaths = (JObject)fields2inc[CurrentPanelFullType.ToUpper()];
                    if (incpaths == null)
                        incpaths = (JObject)fields2inc[CurrentPanelFullType.ToLower()];
                }
                JObject incfields = null;
                if (incpaths != null)
                {
                    incfields = (JObject)incpaths[readsubkey];
                    if (incfields == null)
                        incfields = (JObject)incpaths[readkey];
                }
                Dictionary<string, int> dincfields = new Dictionary<string, int>();
                if (incfields != null)
                    foreach (JProperty pfield in incfields.Properties())
                        if (!Regex.IsMatch(pfield.Name, @"^~if", RegexOptions.IgnoreCase))
                            dincfields.Add(pfield.Name, Convert.ToInt32(pfield.Value.ToString()));
                        else
                        {
                            string scond = pfield.Value.ToString();
                            string[] acond = scond.Split('=');
                            condidx = Convert.ToByte(Regex.Replace(acond[0].Trim(), @"^\D+", ""));
                            condval = Convert.ToByte(acond[1].Trim());
                        }
                //
                foreach (string sidx in didx.Keys)
                {
                    byte[] devbytes = didx[sidx];
                    if (Regex.IsMatch(rwpath.ReadPath, "peripher", RegexOptions.IgnoreCase))
                    {
                        if (typeidx >= 0)
                        {
                            byte typebyte = devbytes[typeidx];
                            string devclass = null;
                            if (panel["~pdtypes"][typebyte.ToString()] != null)
                            {
                                devclass = panel["~pdtypes"][typebyte.ToString()].ToString();
                                key2adddev = devclass;
                                node = (JObject)panel["ELEMENTS"][devclass];
                            }
                        }
                    }
                    if (node["PROPERTIES"] != null && node["PROPERTIES"]["Groups"] != null)
                    {
                        groups = (JObject)node["PROPERTIES"]["Groups"];
                        JObject rw = (JObject)node["~rw"];
                        JObject el = (JObject)node["PROPERTIES"]["Groups"];

                        JObject newpaths = cJson.ChangeGroupsElementsPath(el, sidx);
                        newpaths["~rw"] = rw;
                        SetNodeFilters(newpaths);
                        string _template = newpaths.ToString();
                        cComm.AddListElement(cJson.CurrentPanelID, key2adddev, sidx, _template);
                        //
                        foreach (string propname in ReadProperties.Keys)
                        {
                            //
                            int? incval = null;
                            if (dincfields.ContainsKey(propname))
                                incval = dincfields[propname];
                            //
                            cRWProperty prop = ReadProperties[propname];
                            byte[] pbytes = new byte[prop.bytescnt];
                            if (prop.offset + prop.bytescnt > devbytes.Length)
                                continue;
                            for (int i = prop.offset; i < prop.offset + prop.bytescnt; i++)
                                pbytes[i - prop.offset] = devbytes[i];
                            Tuple<string, string> path_val = _internal_relations_operator.GroupPropertyVal(CurrentPanelID, newpaths, propname, pbytes, prop.xmltag);
                            if (path_val == null)
                            {
                                JObject _element = (JObject)CurrentPanel["ELEMENTS"][readsubkey];
                                if (_element == null)
                                    _element = (JObject)CurrentPanel["ELEMENTS"][_internal_relations_operator.FindElementKey(readsubkey, CurrentPanel)];
                                JObject jgrp = (JObject)_element["PROPERTIES"]["Groups"];
                                if (jgrp["~invisible"] == null)
                                    jgrp["~invisible"] = new JObject();
                                if (jgrp["~invisible"][sidx] == null)
                                    jgrp["~invisible"][sidx] = new JObject();
                                jgrp["~invisible"][sidx][propname] = JObject.FromObject(ReadProperties[propname]);
                                JObject pp = (JObject)jgrp["~invisible"][sidx][propname];
                                pp["~path"] = pp.Path;
                                JArray jbytes = new JArray();
                                for (int bi = 0; bi < pbytes.Length; bi++)
                                    jbytes.Add(pbytes[bi]);
                                pp["~value"] = jbytes;
                                pp["@TYPE"] = "MISSED";
                                if (!missedkeys.ContainsKey(readsubkey + "/" + propname))
                                    missedkeys.Add(readsubkey + "/" + propname, "");
                            }
                            else
                            {
                                string sval = path_val.Item2;
                                if (incval != null)
                                {
                                    bool incflag = true;
                                    if (condidx != null && condval != null)
                                        incflag = devbytes[(int)condidx] == condval;
                                    if (incflag)
                                        sval = (Convert.ToUInt32(sval) + incval).ToString();
                                }
                                cComm.SetPathValue(panel_id, path_val.Item1, sval, FilterValueChanged);
                                if (!foundkeys.ContainsKey(readsubkey + "/" + propname))
                                    foundkeys.Add(readsubkey + "/" + propname, "");
                            }
                        }
                    }
                }
            }
        }
        private static object _connection_cache = null;
        private static eRWResult PanelLogin(cTransport conn, cXmlConfigs cfg, string _code)
        {
            //return eRWResult.Ok;
            //
            if (_code == null || _code.Trim() == "")
                return eRWResult.Ok;
            string _login_cmd = cfg.LoginCommand;
            if (_login_cmd == null)
                return eRWResult.NullLoginCMD;
            //
            int _login_ok_byte = cfg.LoginOkByte;
            if (_login_ok_byte < 0)
                return eRWResult.NullLoginOkByte;
            //
            int _login_ok_val = cfg.LoginOkVal;
            if (_login_ok_val < 0)
                return eRWResult.NullLoginOkVal;
            //
            _login_cmd = (_code.Length * 2).ToString("X2") + _login_cmd.Substring(2, _login_cmd.Length - 2);
            for (int i = 0; i < _code.Length; i++) _login_cmd += "00" + Convert.ToByte(_code[i].ToString()).ToString("X2");
            _login_cmd += "00";
            byte[] bres = cComm.SendCommand(conn, _login_cmd);
            if (_login_ok_byte >= bres.Length)
                return eRWResult.BadCommandResult;
            if (bres[_login_ok_byte] != _login_ok_val)
                return eRWResult.BadLogin;
            if (settings.logreads)
            {
                if (!_log_bytesreaded.ContainsKey(_login_cmd))
                    _log_bytesreaded.Add(_login_cmd, bres);
            }
            return eRWResult.Ok;
        }
        private static string VersionKey(object conn_params, string vercmd, cXmlConfigs cfg, string _code, out eRWResult rwres)
        {
            rwres = eRWResult.Ok;
            if ((_code == null || _code.Trim() == "") && !Regex.IsMatch(CurrentPanelType, "natron", RegexOptions.IgnoreCase))
                return cfg.CurrentVersion;
            cTransport conn = cComm.ConnectBase(conn_params, CurrentPanelType);
            if (conn == null)
            {
                rwres = eRWResult.ConnectionError;
                return null;
            }
            rwres = PanelLogin(conn, cfg, _code);
            if (rwres != eRWResult.Ok && rwres != eRWResult.NullLoginCMD) return null;
            byte[] bres = cComm.SendCommand(conn, vercmd);
            _connection_cache = conn.GetCache();
            cComm.CloseConnection(conn);
            if (settings.logreads && !_log_bytesreaded.ContainsKey(vercmd))
                _log_bytesreaded.Add(vercmd, bres);
            //cComm.CloseConnection(conn);
            string res = (bres != null) ? bres[bres.Length - 2].ToString("X2") + bres[bres.Length - 1].ToString("X2") : null;
            return res;
        }
        private static eRWResult SetRWFiles(object conn_params, string _code, ref string panel_version, ref string xml_version)
        {
            cXmlConfigs cfg = GetPanelXMLConfigs(PanelTemplatePath());
            eRWResult rwres = eRWResult.Ok;
            string ver = VersionKey(conn_params, cfg.VersionCommand, cfg, _code, out rwres);
            if (ver != null)
            {
                if ((rwres == eRWResult.Ok || rwres == eRWResult.NullLoginCMD) && ver != cfg.CurrentVersion || WriteReadMerge == null)
                {
                    cfg.SetRWFiles(ver);
                    WriteReadMerge = cfg.RWMerged();
                }
                panel_version = ver;
            }
            xml_version = cfg.CurrentVersion;
            return rwres;
        }
        public static string ReadLog(object conn_params)
        {
            cTransport conn = cComm.ConnectBase(conn_params, CurrentPanelType);
            byte[] blog = cComm.SendCommand(conn, settings.IRISLogCMD);
            cComm.CloseConnection(conn);
            if (settings.logreads)
            {
                if (!_log_bytesreaded.ContainsKey(settings.IRISLogCMD))
                    _log_bytesreaded.Add(settings.IRISLogCMD, blog);
            }
            //string rlog = File.ReadAllText("read.log");
            //string s = "";
            //for (int i = 0; i < blog.Length; i++)
            //    s += blog[i].ToString("X2");
            //s = settings.IRISLogCMD + ":" + s;
            //rlog += ((!Regex.IsMatch(rlog, @"\r\n$")) ? "\r\n" : "") + s;
            //File.WriteAllText("read.log", rlog);
            if (blog.Length > 6 && blog[1] == 0xff)
            {
                byte[] bres = new byte[blog.Length - 6];
                for (int i = 0; i < bres.Length; i++)
                    bres[i] = blog[i + 6];
                string slog = Encoding.Unicode.GetString(bres).Replace('\0', '|');
                return slog;
            }
            return "";
        }
        public static void ClearCache()
        {
            cComm.ClearCache();
            if (_internal_relations_operator != null) _internal_relations_operator.ClearCache();
            _tteloops_count_in_peripherial_devs = -1;
            _sensloops_count_in_peripherial_devs = -1;
        }
        public static void ClearPanelCache(string _panel_id)
        {
            cComm.ClearPanelCache(_panel_id);
            Monitor.Enter(_cs_current_panel);
            if (_panel_internal_operators.ContainsKey(_panel_id))
            {
                cInternalRel _inop = _panel_internal_operators[_panel_id];
                _inop.ClearCache();
                _tteloops_count_in_peripherial_devs = -1;
                _sensloops_count_in_peripherial_devs = -1;
            }
            Monitor.Exit(_cs_current_panel);
        }
        public static eRWResult ReadDevice(object conn_params, string _code, dConfirmVersionsDiff verdiff)
        {
            bool isRepeaterIris = false;
            bool isSimpoPanel = false;
            JToken templatepath = CurrentPanel["~template_loaded_from"];
            if (templatepath != null && Regex.IsMatch(templatepath.ToString(), @"repeater[\w\W]+?iris[\w\W]+?simpo", RegexOptions.IgnoreCase))
                isRepeaterIris = true;
            else if (templatepath != null && Regex.IsMatch(templatepath.ToString(), @"simpo\.xml", RegexOptions.IgnoreCase))
                isSimpoPanel = true;
            //if (readed)
            //    return;
            //if (reading)
            //    return;
            //reading = true;
            if (settings.logreads)
                _log_bytesreaded.Clear();
            //string slog = ReadLog(conn_params);
            //
            string panel_version = null;
            string xml_version = null;
            eRWResult rwres = SetRWFiles(conn_params, _code, ref panel_version, ref xml_version);
            if (rwres != eRWResult.Ok && rwres != eRWResult.NullLoginCMD)
                return rwres;
            if (String.Compare(panel_version, xml_version) > 0 && !Regex.IsMatch(CurrentPanelType, "natron", RegexOptions.IgnoreCase) && !verdiff(panel_version, xml_version))
                return eRWResult.VersionDiff;
            //
            ClearPanelCache(CurrentPanelID);
            string _panel_id = CurrentPanelID;
            Dictionary<string, cRWPath> drw = new Dictionary<string, cRWPath>();
            Dictionary<string, JObject> dnodes = new Dictionary<string, JObject>();
            Dictionary<string, string> missedkeys = new Dictionary<string, string>();
            Dictionary<string, string> foundkeys = new Dictionary<string, string>();
            Dictionary<string, string> dloopdevs = new Dictionary<string, string>();
            Dictionary<string, Dictionary<string, Dictionary<string, byte[]>>> dserias = new Dictionary<string, Dictionary<string, Dictionary<string, byte[]>>>();
            JObject o = new JObject(CurrentPanel);
            JObject devtypes = (JObject)o["~devtypes"];
            JObject devtypes_bynone = (JObject)o["~devtypes_bynone"];
            JObject _elements = (JObject)o["ELEMENTS"];
            //Type trw = typeof(cRWPath);
            string panel_type = CurrentPanelType;
            foreach (JToken t in (JToken)_elements)
                if (t.Type == JTokenType.Property && ((JProperty)t).Value.Type == JTokenType.Object)
                {
                    JObject _node = new JObject((JObject)((JProperty)t).Value);
                    string nodename = ((JProperty)t).Name;
                    //if (Regex.IsMatch(nodename, "loop", RegexOptions.IgnoreCase))
                    //    panel_type = panel_type;
                    //continue;
                    //if (Regex.IsMatch(nodename, "snone", RegexOptions.IgnoreCase))
                    //    panel_type = panel_type;
                    RWData(_node);
                    if (_node["~rw"] != null)
                    {
                        JObject jrw = (JObject)_node["~rw"];
                        if (!drw.ContainsKey(((JProperty)t).Name))
                        {
                            string tname = ((JProperty)t).Name;
                            if (panel_type == "iris")
                            {
                                cRWPathIRIS rwi = jrw.ToObject<cRWPathIRIS>();
                                drw.Add(((JProperty)t).Name, rwi);
                            }
                            else if (panel_type == "natron")
                            {
                                cRWPathNatron rwn = jrw.ToObject<cRWPathNatron>();
                                drw.Add(((JProperty)t).Name, rwn);
                            }
                            else
                            {
                                cRWPath rw = jrw.ToObject<cRWPath>();
                                drw.Add(((JProperty)t).Name, rw);
                            }
                            //if (panel_type == "iris")
                            //    rw = jrw.ToObject<cRWPathIRIS>();
                            //rw = jrw.ToObject<cRWPath>();
                            dnodes.Add(((JProperty)t).Name, _node);
                        }
                    }
                }
            if (drw.Count > 0)
            {
                Dictionary<string, Tuple<byte, byte>> dpath_minmax = new Dictionary<string, Tuple<byte, byte>>();
                if (isRepeaterIris || isSimpoPanel)
                {
                    JObject pinnet = (JObject)CurrentPanel["ELEMENTS"]["iris_panels_in_network"]["CONTAINS"];
                    JProperty ppanel = pinnet.Properties().First();
                    if (!dpath_minmax.ContainsKey(ppanel.Name))
                    {
                        JObject opanelminmax = (JObject)ppanel.Value;
                        byte pmin = Convert.ToByte(opanelminmax["@MIN"].ToString());
                        byte pmax = Convert.ToByte(opanelminmax["@MAX"].ToString());
                        dpath_minmax.Add(ppanel.Name, new Tuple<byte, byte>(pmin, pmax));
                    }
                }
                if (Regex.IsMatch(CurrentPanelType, "natron", RegexOptions.IgnoreCase))
                {
                    JObject natron = (JObject)CurrentPanel["ELEMENTS"]["Natron"]["CONTAINS"];
                    JProperty pdev = natron.Properties().First();
                    if (!dpath_minmax.ContainsKey(pdev.Name))
                    {
                        JObject opanelminmax = (JObject)pdev.Value;
                        byte pmin = Convert.ToByte(opanelminmax["@MIN"].ToString());
                        byte pmax = Convert.ToByte(opanelminmax["@MAX"].ToString());
                        dpath_minmax.Add(pdev.Name, new Tuple<byte, byte>(pmin, pmax));
                    }
                }
                Dictionary<string, Tuple<byte, byte>> dpath_log = new Dictionary<string, Tuple<byte, byte>>();
                cTransport conn = cComm.ConnectBaseCached(conn_params, CurrentPanelType, _connection_cache);
                //LOGIN!!!!!!!!!!!!!!!!!!!!!!!
                //byte[] loginres = cComm.SendCommand(conn, "076033333333");
                //
                //if (conn_params is cIPParams)
                //    conn = cComm.ConnectIP(((cIPParams)conn_params).address, ((cIPParams)conn_params).port);
                //else if (conn_params is string)
                //    conn = cComm.ConnectFile((string)conn_params);
                //else if (conn_params is cTDFParams)
                //    conn = cComm.ConnectTDF((cTDFParams)conn_params);
                if (conn == null)
                    return eRWResult.ConnectionError;
                foreach (string key in drw.Keys)
                {
                    cRWPath p = null;
                    List<cRWCommand> read_commands = null;
                    Dictionary<string, cRWProperty> ReadProperties = new Dictionary<string, cRWProperty>();
                    if (panel_type == "iris")
                    {
                        cRWPathIRIS pi = (cRWPathIRIS)drw[key];
                        List<cRWCommandIRIS> read_commandsi = pi.ReadCommands;
                        read_commands = new List<cRWCommand>();
                        foreach (cRWCommandIRIS c in read_commandsi)
                            read_commands.Add(c);
                        foreach (string pkey in pi.ReadProperties.Keys)
                        {
                            cRWPropertyIRIS propi = pi.ReadProperties[pkey];
                            ReadProperties.Add(pkey, propi);
                        }
                        p = pi;
                    }
                    else if (panel_type == "natron")
                    {
                        cRWPathNatron pn = (cRWPathNatron)drw[key];
                        List<cRWCommandNatron> read_commandsn = pn.ReadCommands;
                        read_commands = new List<cRWCommand>();
                        foreach (cRWCommandNatron c in read_commandsn)
                            read_commands.Add(c);
                        foreach (string pkey in pn.ReadProperties.Keys)
                        {
                            cRWPropertyNatron propn = pn.ReadProperties[pkey];
                            ReadProperties.Add(pkey, propn);
                        }
                        p = pn;
                    }
                    else
                    {
                        p = (cRWPath)drw[key];
                        read_commands = p.ReadCommands;
                        ReadProperties = p.ReadProperties;
                    }
                    JObject _node = dnodes[key];
                    string read_path = p.ReadPath;
                    JObject groups = null;
                    if (_node["PROPERTIES"] != null && _node["PROPERTIES"]["Groups"] != null)
                        groups = (JObject)_node["PROPERTIES"]["Groups"];
                    if (groups == null || Regex.IsMatch(key, @"^SIMPO_MIMIC\d+$") || Regex.IsMatch(key, @"^SIMPO_TTELOOP\d+$"))
                    {
                        JObject contains = (JObject)_node["CONTAINS"];
                        if (contains != null)
                        {
                            JToken tfirs = null;// contains.First;
                            foreach (JProperty cp in (JToken)contains)
                                if (cp.Value.Type == JTokenType.Object)
                                {
                                    tfirs = (JToken)cp;
                                    break;
                                }
                            if (tfirs != null && tfirs.Type == JTokenType.Property)
                            {
                                string fname = ((JProperty)tfirs).Name;
                                //if (Regex.IsMatch(read_path, @"sensor", RegexOptions.IgnoreCase))
                                //    fname += "";
                                JObject ofirst = (JObject)((JProperty)tfirs).Value;
                                JToken tmin = ofirst["@MIN"];
                                JToken tmax = ofirst["@MAX"];
                                if (tmin != null && tmax != null)
                                {
                                    byte min = Convert.ToByte(tmin.ToString());
                                    byte max = Convert.ToByte(tmax.ToString());
                                    if (max > min)
                                    {
                                        if (!Regex.IsMatch(key, @"^SIMPO_MIMIC\d+$") && !Regex.IsMatch(key, @"^SIMPO_TTELOOP\d+$"))
                                        {
                                            if (!dpath_minmax.ContainsKey(read_path))
                                                dpath_minmax.Add(read_path, new Tuple<byte, byte>(min, max));
                                            else
                                                dpath_minmax[read_path] = new Tuple<byte, byte>(min, max);
                                        }
                                        if (ofirst["@ID"] != null && fname == "ELEMENT")
                                            fname = ofirst["@ID"].ToString();
                                        if (!dpath_minmax.ContainsKey(fname))
                                            dpath_minmax.Add(fname, new Tuple<byte, byte>(min, max));
                                    }
                                    Match loopm = Regex.Match(read_path, @"SIMPO_TTELOOP(\d)$");
                                    if (loopm.Success)
                                    {
                                        fname += loopm.Groups[1].Value;
                                        if (!dpath_minmax.ContainsKey(fname))
                                            dpath_minmax.Add(fname, new Tuple<byte, byte>(min, max));
                                    }
                                }
                            }
                        }
                        if (!Regex.IsMatch(key, @"^SIMPO_MIMIC\d+$") && !Regex.IsMatch(key, @"^SIMPO_TTELOOP\d+$"))
                            continue;
                    }
                    //
                    if (groups == null)
                        continue;
                    if (Regex.IsMatch(key, "TTENONE"))
                        groups = groups;
                    List<string> lstCmdS = new List<string>();
                    List<cRWCommand> lstCmd = new List<cRWCommand>();
                    foreach (cRWCommand cmd in read_commands)
                    {
                        if (panel_type == "iris")
                            lstCmdS.Add(((cRWCommandIRIS)cmd).CommandString());
                        else if (panel_type == "natron")
                            lstCmdS.Add(((cRWCommandNatron)cmd).CommandString());
                        else
                            lstCmdS.Add(cmd.CommandString());
                        lstCmd.Add(cmd);
                    }
                    ///////////////////////
                    if (dpath_minmax.ContainsKey(read_path) && !dpath_log.ContainsKey(read_path))
                        dpath_log.Add(read_path, dpath_minmax[read_path]);
                    ///////////////////////
                    if (dpath_minmax.ContainsKey(read_path) || (Regex.IsMatch(key, @"(_MIMICOUT|natron_device)$") && dpath_minmax.ContainsKey(key)))
                    {
                        Tuple<byte, byte> tminmax = new Tuple<byte, byte>(0, 0);
                        if (dpath_minmax.ContainsKey(read_path))
                            tminmax = dpath_minmax[read_path];
                        else
                            tminmax = dpath_minmax[key];
                        Dictionary<string, Dictionary<string, Dictionary<string, byte[]>>> dread = ReadSeriaDevices(conn, p, tminmax.Item1, tminmax.Item2, read_path, devtypes_bynone);
                        foreach (string loopkey in dread.Keys)
                        {
                            if (!dserias.ContainsKey(loopkey))
                                dserias.Add(loopkey, new Dictionary<string, Dictionary<string, byte[]>>());
                            Dictionary<string, Dictionary<string, byte[]>> snoel = dserias[loopkey];
                            Dictionary<string, Dictionary<string, byte[]>> rnoel = dread[loopkey];
                            foreach (string nonekey in rnoel.Keys)
                            {
                                if (!snoel.ContainsKey(nonekey))
                                    snoel.Add(nonekey, new Dictionary<string, byte[]>());
                                Dictionary<string, byte[]> sidx = snoel[nonekey];
                                Dictionary<string, byte[]> ridx = rnoel[nonekey];
                                foreach (string idxkey in ridx.Keys)
                                    if (!sidx.ContainsKey(idxkey))
                                        sidx.Add(idxkey, ridx[idxkey]);
                            }
                        }
                        continue;
                    }
                    List<byte[]> lstRes = new List<byte[]>();
                    int len = 0;
                    foreach (string cmd in lstCmdS)
                    {
                        byte[] cmdres = cComm.SendCommand(conn, cmd);
                        lstRes.Add(cmdres);
                        if (settings.logreads)
                        {
                            if (!_log_bytesreaded.ContainsKey(cmd))
                                _log_bytesreaded.Add(cmd, cmdres);
                        }
                        len += lstRes[lstRes.Count - 1].Length;
                    }
                    byte[] result = new byte[len];
                    int idx = 0;
                    foreach (byte[] arr in lstRes)
                    {
                        arr.CopyTo(result, idx);
                        idx += arr.Length;
                    }
                    bool emacexists = false;
                    string[] emaca = new string[6];
                    foreach (string propname in ReadProperties.Keys)
                    {
                        //if (Regex.IsMatch(propname, "ORINPUTS", RegexOptions.IgnoreCase))
                        //{
                        //    emacexists = false;
                        //}
                        cRWProperty prop = ReadProperties[propname];
                        byte[] pbytes = new byte[prop.bytescnt];
                        for (int i = prop.offset; i < prop.offset + prop.bytescnt; i++)
                            pbytes[i - prop.offset] = (i < result.Length) ? result[i] : (byte)0;
                        Tuple<string, string> path_val = _internal_relations_operator.GroupPropertyVal(CurrentPanelID, groups, propname, pbytes, prop.xmltag);
                        if (path_val == null)
                        {
                            if (!missedkeys.ContainsKey(key + "/" + propname))
                                missedkeys.Add(key + "/" + propname, "");
                            JObject _panel = CurrentPanel;
                            JObject _element = (JObject)_panel["ELEMENTS"][key];
                            if (_element["PROPERTIES"] != null && _element["PROPERTIES"]["Groups"] != null)
                            {
                                JObject jgrp = (JObject)_element["PROPERTIES"]["Groups"];
                                if (jgrp["~invisible"] == null)
                                    jgrp["~invisible"] = new JObject();
                                jgrp["~invisible"][propname] = JObject.FromObject(ReadProperties[propname]);
                                JObject pp = (JObject)jgrp["~invisible"][propname];
                                pp["~path"] = pp.Path;
                                pp["@TYPE"] = "MISSED";
                                JObject groups1 = new JObject(jgrp);
                                path_val = _internal_relations_operator.GroupPropertyVal(CurrentPanelID, groups1, propname, pbytes, prop.xmltag);
                                Match m = Regex.Match(propname, @"emacETHADDR(\d)", RegexOptions.IgnoreCase);
                                if (m.Success)
                                {
                                    emaca[Convert.ToInt32(m.Groups[1].Value)] = pbytes[0].ToString("X2");
                                    emacexists = true;
                                }
                            }
                        }
                        if (path_val != null)
                        {
                            Match m = Regex.Match(propname, @"emacETHADDR(\d)", RegexOptions.IgnoreCase);
                            if (m.Success)
                            {
                                emaca[Convert.ToInt32(m.Groups[1].Value)] = pbytes[0].ToString("X2");
                                emacexists = true;
                            }
                            cComm.SetPathValue(_panel_id, path_val.Item1, path_val.Item2, FilterValueChanged);
                        }
                        else
                            continue;
                    }
                    if (emacexists)
                    {
                        string emac = "";
                        for (int i = 0; i < emaca.Length; i++)
                            emac += ((emac != "") ? ":" : "") + emaca[i];
                        JObject emaco = _internal_relations_operator.GetDeviceGroupsNode(groups.ToString(), "emacETHADDR");
                        string emacpath = emaco["~path"].ToString();
                        cComm.SetPathValue(_panel_id, emacpath, emac, FilterValueChanged);
                    }
                }
                //
                cComm.CloseConnection(conn);
                SetSeriaDevicesValues(dserias, devtypes, missedkeys, foundkeys, dloopdevs);
                _internal_relations_operator.AfterRead(CurrentPanelID, CurrentPanel, GetNode, FilterValueChanged);
                readed = true;
                reading = false;
                if (settings.logreads)
                {
                    string log = "";
                    foreach (string cmdkey in _log_bytesreaded.Keys)
                    {
                        string sbytes = "";
                        byte[] cmdres = _log_bytesreaded[cmdkey];
                        for (int i = 0; i < cmdres.Length; i++)
                            sbytes += cmdres[i].ToString("X2");
                        string ln = cmdkey + ":" + sbytes;
                        log += ((log != "") ? "\r\n" : "") + ln;
                    }
                    File.WriteAllText("read.log", log);
                }
            }
            if (rwres == eRWResult.NullLoginCMD && Regex.IsMatch(CurrentPanelType, "natron", RegexOptions.IgnoreCase))
                rwres = eRWResult.Ok;
            return rwres;
        }
        //Write
        private static JObject NONEElementRW(string key)
        {
            JObject res = null;
            string[] keys = key.Split('/');
            JObject jnone = (JObject)CurrentPanel["ELEMENTS"][keys[keys.Length - 1]];
            if (jnone == null)
            {
                Match mnone = Regex.Match(keys[keys.Length - 1], @"SIMPO_TTENONE(\d+)$");
                if (mnone.Success)
                    jnone = (JObject)CurrentPanel["ELEMENTS"]["SIMPO_TTELOOP" + mnone.Groups[1].Value];
            }
            JObject content = (JObject)jnone["CONTAINS"];
            if (content == null)
            {
                jnone = (JObject)CurrentPanel["ELEMENTS"][keys[0]];
                content = (JObject)jnone["CONTAINS"];
            }
            foreach (JProperty p in content.Properties())
                if (Regex.IsMatch(p.Name, "(NONE|MIMICOUT)$"))
                {
                    res = GetNode(p.Name);
                    break;
                }
                else if (p.Value.Type == JTokenType.Object && ((JObject)p.Value)["@ID"] != null && Regex.IsMatch(((JObject)p.Value)["@ID"].ToString(), "(NONE|MIMICOUT)$"))
                {
                    res = GetNode(((JObject)p.Value)["@ID"].ToString());
                    break;
                }
            //
            return res;
        }
        private static void FillJNodeCommands(int loop_idx, JObject jgroups, cRWPath rw, string devaddr, int command_len, int inc, Dictionary<string, string> cmds)
        {
            cRWCommand cmd = null;
            string scmd = null;
            string _params = "";
            bool isMIMIC = Regex.IsMatch(rw.WritePath, @"SIMPO_MIMICOUT$");
            bool isSIMPOTTE = Regex.IsMatch(rw.WritePath, @"SIMPO_TTENONE\d+$");
            string scmdlen = "00";
            int cmdlen = Convert.ToInt32(scmdlen, 16);
            foreach (cWriteOperation op in rw.WriteOperationOrder)
            {
                if (op.operation == eWriteOperation.woBytes)
                {
                    Match m = Regex.Match(op._xmltag, @"LENGTH\s*?=\s*?""(\d+?)""");
                    if (m.Success)
                    {
                        int len = Convert.ToInt32(m.Groups[1].ToString());
                        StringBuilder sb = new StringBuilder();
                        for (int i = 1; i <= len; i++)
                            sb.Append(op.value);
                        op.writeval = sb.ToString();
                    }
                    else
                        op.writeval = op.value;
                    //
                    if (cmd != null && op.value.Length == command_len && _params != null && _params != "")
                    {
                        //scmdlen = scmd.Substring(scmd.Length - 2, 2);
                        //cmdlen = Convert.ToInt32(scmdlen, 16);
                        while (_params.Length / 2 < cmdlen)
                            _params += "00";
                        if (cmds.ContainsKey(scmd) && isSIMPOTTE)
                        {
                            scmd += _params.Substring(0, 6);
                            _params = _params.Substring(5, _params.Length - 6);
                        }
                        cmds.Add(scmd, _params);
                        _params = "";
                    }
                    if (op.value.Length == command_len)
                    {
                        if (CurrentPanelType == "iris")
                            cmd = new cRWCommandIRIS();
                        else
                            cmd = new cRWCommand();
                        cmd.InitCommand(op.value);
                        if (!isMIMIC)
                        {
                            scmd = cmd.CommandString(Convert.ToInt32(devaddr) + inc);
                            scmdlen = scmd.Substring(scmd.Length - 2, 2);
                            cmdlen = Convert.ToInt32(scmdlen, 16);
                        }
                        else
                        {
                            cmd.subidx_cmd_len = 3;
                            //scmd = cmd.CommandStringSubIdxOnly(Convert.ToInt32(devaddr) + inc);
                            scmd = cmd.CommandString(loop_idx, Convert.ToInt32(devaddr) + inc);
                            scmdlen = scmd.Substring(scmd.Length - 8, 2);
                            cmdlen = Convert.ToInt32(scmdlen, 16) - 3;
                        }
                        _params = "";
                    }
                    else
                        _params += op.writeval;// op.value;
                }
                else
                {
                    op.writeval = _internal_relations_operator.WritePropertyVal(jgroups, op.value, rw.WriteProperties[op.value].xmltag);
                    _params += op.writeval;
                }
            }
            if (scmd != "" && _params != "")
            {
                //_params += "00";
                while (_params.Length / 2 < cmdlen)
                    _params += "00";
                if (cmds.ContainsKey(scmd) && isSIMPOTTE)
                {
                    scmd += _params.Substring(0, 6);
                    _params = _params.Substring(5, _params.Length - 6);
                }
                cmds.Add(scmd, _params);
            }
        }
        private static Dictionary<string, string> LoopCompiledWriteCommands(int idx, int min, int max, JObject nulldev)
        {
            int inc = 0;
            if (min == 1)
                inc = -1;
            Dictionary<string, string> cmds = new Dictionary<string, string>();
            //
            string _path = nulldev["~path"].ToString();
            bool isMIMIC = false;
            Dictionary<string, string> loopdevs = null;
            if (!Regex.IsMatch(_path, @"SIMPO_MIMIC")) loopdevs = cComm.GetPseudoElementDevices(CurrentPanelID, constants.NO_LOOP, idx.ToString());
            else
            {
                loopdevs = cComm.GetPseudoElementDevices(CurrentPanelID, "SIMPO_MIMIC", idx.ToString());
                isMIMIC = true;
            }
            bool isSIMPOTTENONE = Regex.IsMatch(_path, @"SIMPO_TTENONE");
            List<cWriteOperation> oTrunc = null;
            foreach (string devaddr in loopdevs.Keys)
            {
                JObject jdev = JObject.Parse(loopdevs[devaddr]);
                JObject orw = (JObject)jdev["~rw"];
                cRWPath rw = orw.ToObject<cRWPath>();
                jdev = GroupsWithValues(jdev);
                //
                List<string> commands = new List<string>();
                //cRWCommand cmd = null;
                //string scmd = null;
                //string _params = "";
                //
                cRWCommand cmdl = null;
                if (CurrentPanelType == "iris")
                    cmdl = new cRWCommandIRIS();
                else
                    cmdl = new cRWCommand();
                int command_len = cmdl.CommandLength();
                if (isMIMIC)
                {
                    if (oTrunc == null)
                    {
                        rw.RepareSimpoMIMICOUTCommands(command_len);
                        oTrunc = rw.WriteOperationOrder;
                    }
                    else
                        rw.WriteOperationOrder = oTrunc;
                }
                //
                FillJNodeCommands(idx - 1, jdev, rw, devaddr, command_len, inc, cmds);
                //
                //foreach (cWriteOperation op in rw.WriteOperationOrder)
                //{
                //    if (op.operation == eWriteOperation.woBytes)
                //    {
                //        op.writeval = op.value;
                //        //
                //        if (cmd != null && op.value.Length == command_len && _params != null && _params != "")
                //        {
                //            cmds.Add(scmd, _params);
                //            _params = "";
                //        }
                //        if (op.value.Length == command_len)
                //        {
                //            if (CurrentPanelType == "iris")
                //                cmd = new cRWCommandIRIS();
                //            else
                //                cmd = new cRWCommand();
                //            cmd.InitCommand(op.value);
                //            scmd = cmd.CommandString(Convert.ToInt32(devaddr) + inc);
                //            _params = "";
                //        }
                //        else
                //            _params += op.value;
                //    }
                //    else
                //    {
                //        op.writeval = _internal_relations_operator.WritePropertyVal(jdev, op.value, rw.WriteProperties[op.value].xmltag);
                //        _params += op.writeval;
                //    }
                //}
                //if (scmd != "" && _params != "")
                //{
                //    //_params += "00";
                //    cmds.Add(scmd, _params);
                //}
            }
            //
            string cmdfirst = cmds.Keys.First();
            string paramsfirst = cmds[cmdfirst];
            string zparam = "".PadLeft(paramsfirst.Length, '0');
            cRWCommand zcmd = null;
            if (CurrentPanelType == "iris")
                zcmd = new cRWCommandIRIS();
            else
                zcmd = new cRWCommand();
            zcmd.InitCommand(cmdfirst);
            for (int i = min + inc; i <= max + inc; i++)
            {
                string zaddr = null;
                if (!isMIMIC)
                    zaddr = zcmd.CommandString(i);
                else
                {
                    zcmd.subidx_cmd_len = 3;
                    zaddr = zcmd.CommandStringSubIdxOnly(i);
                }
                if (!cmds.ContainsKey(zaddr))
                    cmds.Add(zaddr, zparam);
            }
            //
            return cmds;
        }
        private static void FillJNodeCommands(JObject jgroups, cRWPath rw, int command_len, Dictionary<string, string> cmds)
        {
            if (Regex.IsMatch(rw.WritePath, "SIMPO_PANELOUTPUTS")) rw.WriteCmdReplacementsByAdditionalBytes(command_len);
            cRWCommand cmd = null;
            string scmd = null;
            string _params = "";
            foreach (cWriteOperation op in rw.WriteOperationOrder)
            {
                if (op.operation == eWriteOperation.woBytes)
                {
                    Match m = Regex.Match(op._xmltag, @"LENGTH\s*?=\s*?""(\d+?)""");
                    if (m.Success)
                    {
                        int len = Convert.ToInt32(m.Groups[1].ToString());
                        if (len != op.value.Length / 2)
                        {
                            StringBuilder sb = new StringBuilder();
                            for (int i = 1; i <= len; i++)
                                sb.Append(op.value);
                            op.writeval = sb.ToString();
                        }
                        else
                            op.writeval = op.value;
                    }
                    else
                        op.writeval = op.value;
                    //
                    if (cmd != null && op.value.Length == command_len && _params != null && _params != "")
                    {
                        string scmdlen = scmd.Substring(scmd.Length - 2, 2);
                        int cmdlen = Convert.ToInt32(scmdlen, 16);
                        while (_params.Length / 2 < cmdlen)
                            _params += "00";
                        string scmd2add = scmd;
                        if (rw.WriteCommandsReplacement.ContainsKey(scmd))
                        {
                            string repl = rw.WriteCommandsReplacement[scmd].First();
                            int addlen = repl.Length - scmd.Length;
                            if (addlen > 0 && _params.Length >= addlen)
                                scmd2add += "_" + _params.Substring(0, addlen);
                        }
                        cmds.Add(scmd2add, _params);
                        _params = "";
                    }
                    if (op.value.Length == command_len && !Regex.IsMatch(op.value, "^0+$"))
                    {
                        if (CurrentPanelType == "iris")
                            cmd = new cRWCommandIRIS();
                        else
                            cmd = new cRWCommand();
                        cmd.InitCommand(op.value);
                        scmd = cmd.CommandString();
                        _params = "";
                    }
                    else
                        _params += op.writeval;// op.value;
                }
                else
                {
                    op.writeval = _internal_relations_operator.WritePropertyVal(jgroups, op.value, rw.WriteProperties[op.value].xmltag);
                    _params += op.writeval;
                }
            }
            if (scmd != "" && _params != "")
            {
                //_params += "00";
                string scmdlen = scmd.Substring(scmd.Length - 2, 2);
                int cmdlen = Convert.ToInt32(scmdlen, 16);
                while (_params.Length / 2 < cmdlen)
                    _params += "00";
                string scmd2add = scmd;
                if (rw.WriteCommandsReplacement.ContainsKey(scmd))
                {
                    string repl = rw.WriteCommandsReplacement[scmd].First();
                    int addlen = repl.Length - scmd.Length;
                    if (addlen > 0 && _params.Length >= addlen)
                        scmd2add += "_" + _params.Substring(0, addlen);
                }
                cmds.Add(scmd2add, _params);
            }
        }
        private static Dictionary<string, string> WriteSingleElementCommands(string key, JObject _node)
        {
            Dictionary<string, string> res = new Dictionary<string, string>();
            JObject jrw = (JObject)_node["~rw"];
            cRWPath rw = jrw.ToObject<cRWPath>();
            cRWCommand cmd = null;
            if (CurrentPanelType == "iris")
                cmd = new cRWCommandIRIS();
            else
                cmd = new cRWCommand();
            int cmd_len = cmd.CommandLength();
            JObject groups = GroupsWithValues((JObject)_node["PROPERTIES"]["Groups"]);
            FillJNodeCommands(groups, rw, cmd_len, res);
            //
            return res;
        }
        private static Dictionary<string, string> SavedSeria(string key)
        {
            string[] analyse_key = key.Split('/');
            Dictionary<string, string> seria = cComm.GetElements(CurrentPanelID, analyse_key[analyse_key.Length - 1]);
            if (seria == null && analyse_key.Length == 1)
                seria = cComm.GetElements(CurrentPanelID, Regex.Replace(analyse_key[analyse_key.Length - 1], "S$", "", RegexOptions.IgnoreCase));
            if (seria == null && analyse_key.Length == 1)
                seria = cComm.GetElements(CurrentPanelID, Regex.Replace(analyse_key[analyse_key.Length - 1], "S_", "_", RegexOptions.IgnoreCase));
            if (seria == null && analyse_key.Length == 1)
            {
                string key1 = Regex.Replace(analyse_key[analyse_key.Length - 1], "S$", "", RegexOptions.IgnoreCase);
                seria = cComm.GetElements(CurrentPanelID, Regex.Replace(key1, "S_", "_", RegexOptions.IgnoreCase));
            }
            if (seria == null && analyse_key.Length == 1)
            {
                string key1 = Regex.Replace(analyse_key[analyse_key.Length - 1], "S$", "", RegexOptions.IgnoreCase);
                Match m = Regex.Match(key1, @"^(\w+?_)");
                if (m.Success)
                {
                    string panel_prefix = m.Groups[1].Value;
                    key1 = Regex.Replace(key1, "^" + panel_prefix, "");
                    key1 = panel_prefix + Regex.Replace(key1, "S_", "_", RegexOptions.IgnoreCase);
                    seria = cComm.GetElements(CurrentPanelID, key1);
                }
            }
            return seria;
        }
        private static Dictionary<string, string> WriteSeriaElementCommands(string key, List<JObject> _nodes)
        {
            Dictionary<string, string> res = new Dictionary<string, string>();
            //
            Dictionary<string, string> seria = SavedSeria(key);
            //if (seria == null)
            //    return res;
            //
            string[] keys = key.Split('/');
            JObject incontent = _internal_relations_operator.FindContentFromElement(CurrentPanel, keys[keys.Length - 1]);
            if (incontent == null)
                return res;
            if (incontent["@MIN"] == null || incontent["@MAX"] == null)
                return res;
            int min = Convert.ToInt32(incontent["@MIN"].ToString());
            int max = Convert.ToInt32(incontent["@MAX"].ToString());
            string element_name = _internal_relations_operator.ElementNameFromRWPath(CurrentPanel, keys[keys.Length - 1]);
            if (CurrentPanel["ELEMENTS"][element_name] == null)
                return res;
            JObject element = GetNode(element_name);
            JObject groups = element;
            if (groups["PROPERTIES"] != null)
                groups = (JObject)groups["PROPERTIES"];
            if (groups["Groups"] != null)
                groups = (JObject)groups["Groups"];
            JObject orw = (JObject)element["~rw"];
            cRWPath rw = orw.ToObject<cRWPath>();
            cRWCommand rwcmd = null;
            if (CurrentPanelType == "iris")
                rwcmd = new cRWCommandIRIS();
            else
                rwcmd = new cRWCommand();
            int cmdlen = rwcmd.CommandLength();
            int inc = min * -1;
            //
            if (seria != null)
                foreach (string devaddr in seria.Keys)
                {
                    JObject jgrp = ChangeGroupsElementsPath(groups, devaddr);
                    jgrp = GroupsWithValues(jgrp);
                    FillJNodeCommands(0, jgrp, rw, devaddr, cmdlen, inc, res);
                }
            //
            Dictionary<string, string> uniquecmds = new Dictionary<string, string>();
            foreach (cWriteOperation op in rw.WriteOperationOrder)
                if (op.operation == eWriteOperation.woBytes && op.value.Length == cmdlen && !uniquecmds.ContainsKey(op.value))
                {
                    byte paramlen = Convert.ToByte(op.value.Substring(op.value.Length - 2, 2), 16);
                    string param = "".PadLeft(paramlen * 2, '0');
                    uniquecmds.Add(op.value, param);
                }
            if (CurrentPanelType == "iris" && Regex.IsMatch(key, "panelinnet", RegexOptions.IgnoreCase))
                max--;
            foreach (string scmd in uniquecmds.Keys)
            {
                rwcmd.InitCommand(scmd);
                for (int i = min + inc; i <= max + inc; i++)
                {
                    string addrcmd = rwcmd.CommandString(i);
                    if (!res.ContainsKey(addrcmd))
                        res.Add(addrcmd, uniquecmds[scmd]);
                    //!!!!!!!!!!!!!
                    //if (i > 0)
                    //    break;
                }
            }
            //
            return res;
        }
        private static Dictionary<string, string> ExecuteWriteCommands(cTransport conn, Dictionary<string, string> cmds)
        {
            Dictionary<string, string> dres = new Dictionary<string, string>();
            foreach (string cmd in cmds.Keys)
            {
                string sfirs = (cmds[cmd].Length / 2 + 3).ToString("X2");
                string cmdnew = Regex.Replace(sfirs + cmd.Substring(2), @"_[\da-fA-F]+$", "");
                string scmd = cmdnew + cmds[cmd];
                byte[] res = cComm.SendCommand(conn, scmd);
                string sres = "";
                foreach (byte b in res)
                    sres += b.ToString("X2");
                dres.Add(scmd, sres);
            }
            byte[] resend = cComm.SendCommand(conn, "00fe00");
            string sresend = "";
            foreach (byte b in resend)
                sresend += b.ToString("X2");
            dres.Add("00fe00", sresend);
            return dres;
        }
        private static Tuple<int, int> NONEElementsRange(string key)
        {
            int min = int.MaxValue;
            int max = int.MinValue;
            Tuple<int, int> res = new Tuple<int, int>(min, max);
            string[] keys = key.Split('/');
            JObject jnone = (JObject)CurrentPanel["ELEMENTS"][keys[keys.Length - 1]];
            if (jnone == null)
            {
                Match mnone = Regex.Match(keys[keys.Length - 1], @"SIMPO_TTENONE(\d+)$");
                if (mnone.Success)
                    jnone = (JObject)CurrentPanel["ELEMENTS"]["SIMPO_TTELOOP" + mnone.Groups[1].Value];
            }
            JObject content = (JObject)jnone["CONTAINS"];
            if (content == null)
            {
                jnone = (JObject)CurrentPanel["ELEMENTS"][keys[0]];
                content = (JObject)jnone["CONTAINS"];
            }
            JObject onone = null;
            foreach (JProperty p in content.Properties())
                if (Regex.IsMatch(p.Name, "(NONE|MIMICOUT)$"))
                {
                    onone = (JObject)p.Value;
                    break;
                }
                else if (p.Value.Type == JTokenType.Object && ((JObject)p.Value)["@ID"] != null && Regex.IsMatch(((JObject)p.Value)["@ID"].ToString(), "(NONE|MIMICOUT)$"))
                {
                    onone = (JObject)p.Value;
                    break;
                }
            if (onone == null || onone["@MIN"] == null || onone["@MAX"] == null)
                return res;
            res = new Tuple<int, int>(Convert.ToInt32(onone["@MIN"].ToString()), Convert.ToInt32(onone["@MAX"].ToString()));
            //
            return res;
        }
        private static Dictionary<string, string> WriteLoopCommands(string key, List<JObject> _nodes)
        {
            Dictionary<string, string> res = new Dictionary<string, string>();
            //
            JObject _node = _nodes[0];
            string write_path = _node["~rw"]["WritePath"].ToString();
            string[] parr = write_path.Split('/');
            string loops_root = parr[0];
            JObject elements = (JObject)CurrentPanel["ELEMENTS"];
            foreach (JProperty p in elements.Properties())
            {
                if (p.Value.Type == JTokenType.Object && Regex.IsMatch(p.Name, "loop", RegexOptions.IgnoreCase) && Regex.IsMatch(loops_root, "^" + Regex.Replace(p.Name, @"[^a-zA-Z]", @"[\w\W]*?") + "$", RegexOptions.IgnoreCase))
                {
                    loops_root = p.Name;
                    break;
                }
                if (p.Value.Type == JTokenType.Object && Regex.IsMatch(loops_root, "SIMPO_LOOPDEVICE") && p.Name == "iris_loop_devices")
                {
                    loops_root = p.Name;
                    break;
                }
            }
            if (elements[loops_root] == null)
                return res;
            JObject content = (JObject)elements[loops_root]["CONTAINS"];
            int min = int.MaxValue;
            int max = int.MinValue;
            foreach (JProperty p in content.Properties())
                if (p.Value.Type == JTokenType.Object && ((JObject)p.Value)["@MIN"] != null && ((JObject)p.Value)["@MAX"] != null)
                {
                    min = Convert.ToInt32(((JObject)p.Value)["@MIN"].ToString());
                    max = Convert.ToInt32(((JObject)p.Value)["@MAX"].ToString());
                    break;
                }
            if (min >= max)
            {
                if (loops_root == "SIMPO_MIMICPANELS")
                {
                    min = int.MaxValue;
                    max = int.MinValue;
                    foreach (JProperty pmimic in content.Properties())
                    {
                        Match mmimic = Regex.Match(pmimic.Name, @"SIMPO_MIMIC(\d+)$");
                        if (!mmimic.Success) continue;
                        int imimic = Convert.ToInt32(mmimic.Groups[1].Value);
                        if (imimic < min) min = imimic;
                        if (imimic > max) max = imimic;
                    }
                }
                if (min >= max)
                    return res;
            }
            Dictionary<string, cRWPath> merge = WriteReadMerge;
            Dictionary<string, Dictionary<string, cRWPath>> rwdevs = new Dictionary<string, Dictionary<string, cRWPath>>();
            for (int i = min; i <= max; i++)
            {
                string mkey = Regex.Replace(key, @"LOOP\d+", "LOOP" + i.ToString(), RegexOptions.IgnoreCase);
                mkey = Regex.Replace(key, @"MIMIC\d+", "MIMIC" + i.ToString(), RegexOptions.IgnoreCase);
                if (Regex.IsMatch(mkey, @"^SIMPO_MIMIC\d+$")) mkey += "/SIMPO_MIMICOUT";
                if (!merge.ContainsKey(mkey))
                {
                    Match mnom = Regex.Match(mkey, @"SIMPO_TTELOOP(\d+)$");
                    if (mnom.Success)
                        mkey = "SIMPO_LOOPDEVICES/SIMPO_TTENONE" + mnom.Groups[1].Value;
                }
                if (!merge.ContainsKey(mkey))
                    continue;
                cRWPath rw = merge[mkey];
                Tuple<int, int> devrange = NONEElementsRange(mkey);
                JObject devnull = NONEElementRW(mkey);
                string remask = "(LOOP|SIMPO_MIMIC)" + i.ToString();
                string[] karr = mkey.Split('/');
                foreach (string s in karr)
                    if (Regex.IsMatch(s, remask + "$"))
                    {
                        remask = s;
                        break;
                    }
                string mimickey = remask;
                string simpoloopkey = "SIMPO_TTELOOP";
                string simpoloopmask = "SIMPO_TTELOOP" + i.ToString();
                remask = "^" + remask;
                bool lexists = cComm.PseudoElementExists(CurrentPanelID, constants.NO_LOOP, remask);
                if (!lexists && Regex.IsMatch(remask, "MIMIC"))
                    lexists = cComm.PseudoElementExists(CurrentPanelID, mimickey, remask);
                if (!lexists && Regex.IsMatch(remask, "MIMIC"))
                    lexists = cComm.PseudoElementExists(CurrentPanelID, simpoloopkey, simpoloopmask);
                if (!lexists)
                    continue;
                Dictionary<string, string> compiled = LoopCompiledWriteCommands(i, devrange.Item1, devrange.Item2, devnull);
                foreach (string ckey in compiled.Keys)
                    res.Add(ckey, compiled[ckey]);
            }
            //
            return res;
        }
        public static eRWResult WriteDevice(object conn_params, string _code, dConfirmVersionsDiff verdiff)
        {
            string panel_version = null;
            string xml_version = null;
            eRWResult rwres = SetRWFiles(conn_params, _code, ref panel_version, ref xml_version);
            if (panel_version != xml_version && !verdiff(panel_version, xml_version)) return eRWResult.VersionDiff;
            if (rwres != eRWResult.Ok)
                return rwres;
            //
            if (settings.logreads)
                _log_byteswrite.Clear();
            //
            JObject _panel = new JObject(CurrentPanel);
            JObject _elements = (JObject)_panel["ELEMENTS"];
            //
            Dictionary<string, List<JObject>> dwrite = new Dictionary<string, List<JObject>>();
            //
            foreach (JToken t in (JToken)_elements)
            {
                if (t.Type != JTokenType.Property || ((JProperty)t).Value.Type != JTokenType.Object)
                    continue;
                JObject _node = new JObject((JObject)((JProperty)t).Value);
                string nodename = ((JProperty)t).Name;
                if (Regex.IsMatch(nodename, @"(SENSORS|MODULES|SNONE|MNONE)"))
                {
                    RWData(_node);
                }
                else
                    RWData(_node);
                if (_node["~rw"] == null)
                    continue;
                string writekey = _node["~rw"]["WritePath"].ToString();
                if (CurrentPanelType == "iris" && Regex.IsMatch(writekey, @"(LOOP|SIMPO_MIMIC\d)"))
                {
                    if (_node["CONTAINS"] == null || _node["CONTAINS"].Type != JTokenType.Object)
                        continue;
                    JObject content = (JObject)_node["CONTAINS"];
                    JProperty pnone = null;
                    foreach (JProperty p in content.Properties())
                        if (p.Value.Type == JTokenType.Object && Regex.IsMatch(p.Name, @"(NONE|MIMICOUT)$"))
                        {
                            pnone = p;
                            break;
                        }
                        else if (p.Name == "ELEMENT" && p.Value.Type == JTokenType.Object && p.Value["@ID"] != null && Regex.IsMatch(p.Value["@ID"].ToString(), "(NONE|MIMICOUT)$"))
                        {
                            pnone = p;
                            break;
                        }
                    if (pnone == null && content["ELEMENT"] != null && content["ELEMENT"].Type == JTokenType.Object)
                    {
                        JObject el = (JObject)content["ELEMENT"];
                        if (el["@ID"] != null && Regex.IsMatch(el["@ID"].ToString(), "(NONE|MIMICOUT)$"))
                            pnone = new JProperty("@ID", el["@ID"]);
                    }
                    if (pnone == null)
                        continue;
                    writekey = Regex.Replace(writekey, @"LOOP\d+", "LOOP1");
                    writekey = Regex.Replace(writekey, @"^SIMPO_LOOPDEVICES/", "");
                    writekey = Regex.Replace(writekey, @"SIMPO_MIMIC\d+", "SIMPO_MIMIC1");
                    writekey = Regex.Replace(writekey, @"^SIMPO_MIMICPANELS/", "");
                    if (!dwrite.ContainsKey(writekey))
                    {
                        dwrite.Add(writekey, new List<JObject>());
                        dwrite[writekey].Add(_node);
                    }
                }
                else
                {
                    if (!dwrite.ContainsKey(writekey))
                        dwrite.Add(writekey, new List<JObject>());
                    dwrite[writekey].Add(_node);
                }
            }
            Dictionary<string, JObject> dnormal = new Dictionary<string, JObject>();
            Dictionary<string, List<JObject>> dseria = new Dictionary<string, List<JObject>>();
            Dictionary<string, List<JObject>> dloop = new Dictionary<string, List<JObject>>();
            foreach (string writepath in dwrite.Keys)
            {
                if (Regex.IsMatch(writepath, @"(loop|mimic\d)", RegexOptions.IgnoreCase))
                {
                    if (!dloop.ContainsKey(writepath))
                        dloop.Add(writepath, dwrite[writepath]);
                }
                else if (dwrite[writepath].Count > 1)
                {
                    if (!dseria.ContainsKey(writepath))
                        dseria.Add(writepath, dwrite[writepath]);
                }
                else if (writepath == "SIMPO_PANELS")
                {
                    if (!dseria.ContainsKey(writepath))
                        dseria.Add(writepath, dwrite[writepath]);
                }
                else
                {
                    if (!dnormal.ContainsKey(writepath))
                        dnormal.Add(writepath, dwrite[writepath][0]);
                }
            }
            Dictionary<string, string> compiled = new Dictionary<string, string>();
            foreach (string key in dnormal.Keys)
            {
                Dictionary<string, string> dsingle = WriteSingleElementCommands(key, dnormal[key]);
                foreach (string singlekey in dsingle.Keys)
                {
                    if (!compiled.ContainsKey(singlekey))
                        compiled.Add(singlekey, dsingle[singlekey]);
                    else
                        compiled[singlekey] = cRWPath.MergeWriteParams(compiled[singlekey], dsingle[singlekey]);
                    if (settings.logreads)
                    {
                        if (!_log_byteswrite.ContainsKey(singlekey))
                            _log_byteswrite.Add(singlekey, dsingle[singlekey]);
                        else
                            _log_byteswrite[singlekey] = cRWPath.MergeWriteParams(_log_byteswrite[singlekey], dsingle[singlekey]);
                    }
                }
            }
            foreach (string key in dseria.Keys)
            {
                //if (!Regex.IsMatch(key, @"(evac)", RegexOptions.IgnoreCase)) continue;
                Dictionary<string, string> dseriac = WriteSeriaElementCommands(key, dseria[key]);
                foreach (string seriakey in dseriac.Keys)
                {
                    compiled.Add(seriakey, dseriac[seriakey]);
                    if (settings.logreads)
                        _log_byteswrite.Add(seriakey, dseriac[seriakey]);
                }
            }
            Dictionary<string, string> loopscompiled = new Dictionary<string, string>();
            foreach (string key in dloop.Keys)
            {
                //continue;
                Dictionary<string, string> dcompiledloop = WriteLoopCommands(key, dloop[key]);
                string llog = "";
                foreach (string lkey in dcompiledloop.Keys)
                {
                    llog += ((llog != "") ? "\r\n" : "") + lkey + ":" + dcompiledloop[lkey];
                    loopscompiled.Add(lkey, dcompiledloop[lkey]);
                    //
                    if (settings.logreads)
                        _log_byteswrite.Add(lkey, dcompiledloop[lkey]);
                }
                //
                File.WriteAllText("loops.log", llog);
                //
                //foreach (string lkey in dcompiledloop.Keys)
                //    compiled.Add(lkey, dcompiledloop[lkey]);
            }
            //
            if (settings.logreads)
            {
                string _swrite = "";
                foreach (string wkey in _log_byteswrite.Keys)
                    _swrite += ((_swrite != "") ? "\r\n" : "") + wkey + ":" + _log_byteswrite[wkey];
                File.WriteAllText("write.log", _swrite);
            }
            //
            cTransport conn = cComm.ConnectBase(conn_params, CurrentPanelType);
            cXmlConfigs cfg = GetPanelXMLConfigs(PanelTemplatePath());
            rwres = PanelLogin(conn, cfg, _code);
            if (rwres != eRWResult.Ok)
            {
                cComm.CloseConnection(conn);
                return rwres;
            }
            /////////////////////
            //byte[] ver = cComm.SendCommand(conn, "035111000007");
            //cComm.CloseConnection(conn);
            //////////////////////////////
            //string sbytes = "";
            //string log = File.ReadAllText("read.log");
            //for (int i = 0; i < ver.Length; i++)
            //    sbytes += ver[i].ToString("X2");
            //string ln = "035111000007" + ":" + sbytes;
            //log += ((log != "") ? "\r\n" : "") + ln;
            //File.WriteAllText("read.log", log);
            ////////////////////////////
            //byte[] pass = new byte[] { 0x08, 0x60, 0x00, 0x03, 0x00, 0x03, 0x00, 0x03, 0x00, 0x03, 0x00 };
            //byte[] loginres = cComm.SendCommand(conn, pass);
            ////////////////////////////
            Dictionary<string, string> dres = ExecuteWriteCommands(conn, compiled);
            cComm.CloseConnection(conn);
            if (settings.logreads)
            {
                string wres = "";
                string werr = "";
                foreach (string wkey in dres.Keys)
                {
                    wres += ((wres != "") ? "\r\n" : "") + wkey + ":" + dres[wkey];
                    if (dres[wkey] != "0010FF")
                        werr += ((werr != "") ? "\r\n" : "") + wkey + ":" + dres[wkey];
                }
                File.WriteAllText("writeres.log", wres);
                File.WriteAllText("writeerr.log", werr);
            }
            return eRWResult.Ok;
        }
        #endregion
        private static void AddDefaultLoops(JObject _panel)
        {
            JObject elements = (JObject)_panel["ELEMENTS"];
            if (elements == null) return;
            JObject oloops = (JObject)elements["iris_loop_devices"];
            if (oloops == null) return;
            JObject content = (JObject)oloops["CONTAINS"];
            string loop_key = null;
            foreach (JProperty p in content.Properties())
            {
                if (p.Value == null || p.Value.Type != JTokenType.Object) continue;
                loop_key = p.Name;
                break;
            }
            if (loop_key == null) return;
            JArray aloops = new JArray();
            foreach (JProperty p in elements.Properties())
            {
                if (p.Value == null || p.Value.Type != JTokenType.Object || !Regex.IsMatch(p.Name, "^" + loop_key + @"\d*?$")) continue;
                JObject oloop = (JObject)p.Value;
                if (oloop["CHANGE"] != null) continue;
                aloops.Add(oloop);
            }
            foreach (JObject defloop in aloops)
            {
                string path = defloop["~path"].ToString();
                Match m = Regex.Match(path, @"\.([\w\W]+?)(\d+)$");
                if (m.Success)
                {
                    string elementType = m.Groups[1].Value;
                    string elementNumber = m.Groups[2].Value;
                    JObject el = cJson.GetNode(elementType + elementNumber);
                    cComm.AddPseudoElement(CurrentPanelID, elementType, elementNumber, el.ToString());
                    //
                    JObject o = JObject.Parse(cComm.GetPseudoElement(cJson.CurrentPanelID, elementType, elementNumber.ToString()));
                    o["~loop_type"] = elementType + elementNumber;
                    cComm.SetPseudoElement(cJson.CurrentPanelID, elementType, elementNumber.ToString(), o.ToString());
                }
            }
        }
        public static JObject AddPanel(JObject jSys)
        {
            string filename = FilePathFromSchema(jSys["schema"].ToString());
            JObject _panel = GetPanelTemplate(filename);
            //if (_panel != null)
            //    return (JObject)_panel["ELEMENTS"][jSys["schema"].ToString()];
            Monitor.Enter(_cs_current_panel);
            Monitor.Enter(_cs_panel_templates);
            Monitor.Enter(_cs_main_content_key);
            //_panel = _current_panel;
            Guid guid = Guid.NewGuid();
            string _panel_id = guid.ToString();
            _current_panel_id = _panel_id;
            //
            if (_panel == null)
                _panel = SchemaJSON(jSys["schema"].ToString());
            else
                _last_loaded_template_filepath = filename;
            if (_internal_relations_operator == null)
                _internal_relations_operator = new cInternalrelIRIS();
            if (_panel["~panel_type"] == null)
                _panel["~panel_type"] = _panel_type;
            _panel["~template_loaded_from"] = _last_loaded_template_filepath;
            //
            _system_panels.Add(_panel_id, filename);
            if (!_panel_templates.ContainsKey(filename))
                _panel_templates.Add(filename, _panel);
            else
                _panel_templates[filename] = _panel;

            _main_content_key = Regex.Replace(jSys["schema"].ToString(), @"\d+$", "");
            if (Regex.IsMatch(_main_content_key, "^repeater_iris_simpo", RegexOptions.IgnoreCase))
                _main_content_key = "iris";
            if (Regex.IsMatch(_main_content_key, "^tft", RegexOptions.IgnoreCase))
                _main_content_key = "iris";
            Monitor.Exit(_cs_main_content_key);
            Monitor.Exit(_cs_panel_templates);
            Monitor.Exit(_cs_current_panel);
            AddDefaultLoops(_panel);
            return _panel;
        }

        public static void RemovePanel(string _panel_id)
        {
            Monitor.Enter(_cs_current_panel);
            Monitor.Enter(_cs_panel_templates);
            if (_system_panels.ContainsKey(_panel_id))
            {
                ClearPanelCache(_panel_id);
                _system_panels.Remove(_panel_id);
                if (_panel_internal_operators.ContainsKey(_panel_id))
                    _panel_internal_operators.Remove(_panel_id);
            }
            Monitor.Exit(_cs_current_panel);
            Monitor.Exit(_cs_panel_templates);
        }
        private static void SetPanelIDInToken(JToken t)
        {
            t["~panel_id"] = CurrentPanelID;
        }
        private static void SetPanelNameInToken(JToken t)
        {
            t["~panel_name"] = CurrentPanelName;
        }
        public static JObject GetNode(string name)
        {
            JObject _panel = CurrentPanel;
            //if (_panel == null)
            //    _panel = AddPanel(name);
            if (_panel != null)
            {
                JObject _elements = (JObject)_panel["ELEMENTS"];
                if (_elements != null)
                {
                    if (Regex.IsMatch(name, CurrentPanelType + @"\d+$"))
                        name = Regex.Replace(name, @"\d+$", "");
                    if (Regex.IsMatch(name, "^repeater_iris_simpo", RegexOptions.IgnoreCase))
                        name = "iris";
                    if (Regex.IsMatch(name, "^simpo$", RegexOptions.IgnoreCase))
                        name = "iris";
                    if (Regex.IsMatch(name, @"^tft[\w\W]+?repeater$", RegexOptions.IgnoreCase))
                        name = "iris";
                    if (Regex.IsMatch(name, @"^natron[\w\W]*?none$", RegexOptions.IgnoreCase))
                        name = "natron_device";
                    JObject _res = null;
                    JToken jbyname = _elements[name];
                    if (jbyname != null)
                    {
                        _res = new JObject((JObject)_elements[name]);
                        //if (_res != null)
                        RWData(_res);
                    }
                    else
                    {
                        string[] names = name.Split('/');
                        string _el = Regex.Replace(_internal_relations_operator.FindElementKey(names[0], CurrentPanel), @"\d+$", "");
                        if (_elements[_el] == null && names.Length > 1)
                            _el = Regex.Replace(_internal_relations_operator.FindElementKey(names[1], CurrentPanel), @"\d+$", "");
                        _res = new JObject((JObject)_elements[_el]);
                        cRWPath rw = RWLoopPath(WriteReadMerge, name);
                        if (rw != null)
                        {
                            JObject o = JObject.FromObject(rw);
                            _res["~rw"] = o;
                        }
                    }
                    //
                    //ReadDevice("192.168.17.17", 7000);
                    //
                    SetPanelIDInToken(_res);
                    return _res;
                }
            }
            return null;
            //
            //string filename = FilePathFromSchema(name);
            //_panel = GetPanelTemplate(filename);
            //if (_panel != null)
            //    return (JObject)_panel["ELEMENTS"][name];
            //Monitor.Enter(_cs_current_panel);
            //Monitor.Enter(_cs_panel_templates);
            //Monitor.Enter(_cs_main_content_key);
            //_panel = _current_panel;
            //if (_panel == null)
            //    _panel = SchemaJSON(name);
            //_panel["~panel_type"] = _panel_type;
            //_panel["~template_loaded_from"] = _last_loaded_template_filepath;
            //Guid guid = Guid.NewGuid();
            //_panel["~guid"] = guid.ToString();
            //if (!_panel_templates.ContainsKey(filename))
            //    _panel_templates.Add(filename, _panel);
            //else
            //    _panel_templates[filename] = _panel;
            //_current_panel = _panel;
            //string _clean_name = Regex.Replace(name, @"[\d-]", string.Empty); // added by Viktor
            //_main_content_key = _clean_name; // replaced name by _clean_name by Viktor
            //Monitor.Exit(_cs_main_content_key);
            //Monitor.Exit(_cs_panel_templates);
            //Monitor.Exit(_cs_current_panel);
            //JObject _res1 = new JObject((JObject)_panel["ELEMENTS"][_clean_name]); // replaced name by _clean_name by Viktor
            //RWData(_res1);
            //return _res1;
        }

        #region internal relations operations
        private static Dictionary<string, cInternalRel> _panel_internal_operators = new Dictionary<string, cInternalRel>();
        //private static cInternalRel __internal_relations_operator = null;
        private static cInternalRel _internal_relations_operator
        {
            get
            {
                Monitor.Enter(_cs_current_panel);
                cInternalRel res = null;
                if (CurrentPanelID != null && _panel_internal_operators.ContainsKey(CurrentPanelID))
                    res = _panel_internal_operators[CurrentPanelID];
                Monitor.Exit(_cs_current_panel);
                return res;
            }
            set
            {
                Monitor.Enter(_cs_current_panel);
                if (_panel_internal_operators.ContainsKey(CurrentPanelID))
                    _panel_internal_operators[CurrentPanelID] = value;
                else
                    _panel_internal_operators.Add(CurrentPanelID, value);
                Monitor.Exit(_cs_current_panel);
            }
        }
        public static void SetNodeFilters(JObject _node)
        {
            _internal_relations_operator.SetNodeFilters(CurrentPanelID, _node);
        }

        private static void AddLoopIOPaths(JObject o, string path)
        {
            foreach (JProperty p1 in (JToken)o)
            {
                JObject o1 = (JObject)p1.Value;
                foreach (JProperty p2 in (JToken)o1)
                {
                    JObject o2 = (JObject)p2.Value;
                    foreach (JProperty p3 in (JToken)o2)
                    {
                        string ch = p3.Name;
                        string chpath = p3.Value.ToString();
                        JObject o3 = new JObject();
                        o3["path"] = path;
                        o3["channel_path"] = chpath;
                        o2[ch] = o3;
                    }
                }
            }
        }
        private static void AddUses(JObject o, string path)
        {
            foreach (JProperty p1 in (JToken)o)
            {
                JObject o1 = (JObject)p1.Value;
                foreach (JProperty p2 in (JToken)o1)
                {
                    JObject o2 = (JObject)p2.Value;
                    foreach (JProperty p3 in (JToken)o2)
                    {
                        string ch = p3.Name;
                        JObject o3 = (JObject)p3.Value;
                        string chpath = o3["channel_path"].ToString();
                        List<string> lst = _internal_relations_operator.ChannelUsedIn(chpath, path);
                        JArray jarr = (lst != null) ? JArray.FromObject(lst) : null;
                        o3["uses"] = jarr;
                        //JObject o3 = new JObject();
                        //o3["path"] = path;
                        //o3["channel_path"] = chpath;
                        //o2[ch] = o3;
                    }
                }
            }
        }
        public static JArray ChannelUses(string path)
        {
            List<string> lst = _internal_relations_operator.ChannelUsedIn(path);
            JArray jarr = (lst != null) ? JArray.FromObject(lst) : null;
            //
            return jarr;
        }
        public static JObject LoopRelations(string noloop, int loopnom)
        {
            Dictionary<string, List<string>> reverse = cComm.ReversePaths(CurrentPanelID, @"^[^/]+?LOOP\d+?/[\w\W]+?#");
            Dictionary<string, List<string>> dres = new Dictionary<string, List<string>>();
            foreach (string key in reverse.Keys)
            {
                Match m = Regex.Match(key, @"^[^/]+?LOOP" + loopnom.ToString() + @"/[\w\W]+?#([\w\W]+?)\.[\w\W]+?~index~(\d+)", RegexOptions.IgnoreCase);
                if (!m.Success)
                    continue;
                string dev = m.Groups[1].Value;
                string idx = m.Groups[2].ToString();
                string reskey = idx + ". " + dev;
                if (!dres.ContainsKey(reskey))
                    dres.Add(reskey, new List<string>());
                List<string> revlst = reverse[key];
                List<string> reslst = dres[reskey];
                foreach (string iopath in revlst)
                {
                    m = Regex.Match(iopath, @"\.([^\.]+?)(INPUT|OUTPUT)[\w\W]+?~index~(\d+)");
                    if (!m.Success)
                        continue;
                    string io = m.Groups[1].Value + m.Groups[2].Value + m.Groups[3].Value;
                    reslst.Add(io);
                }
            }
            return JObject.FromObject(dres);
            //Dictionary<string, string> d = cComm.GetPseudoElementDevices(CurrentPanelID, constants.NO_LOOP, loopnom.ToString());
            //Dictionary<string, Dictionary<string, Dictionary<string, string>>> dres = _internal_relations_operator.UnionInOuts(CurrentPanelID, "input");
        }
        public static JObject LoopsInputs(string path)
        {
            Dictionary<string, Dictionary<string, Dictionary<string, string>>> dres = _internal_relations_operator.UnionInOuts(CurrentPanelID, "input");
            JObject res = JObject.FromObject(dres);
            AddLoopIOPaths(res, path);
            AddUses(res, path);
            return res;
        }
        public static JObject LoopsOutputs(string path)
        {
            Dictionary<string, Dictionary<string, Dictionary<string, string>>> dres = _internal_relations_operator.UnionInOuts(CurrentPanelID, "output");
            JObject res = JObject.FromObject(dres);
            AddLoopIOPaths(res, path);
            AddUses(res, path);
            return res;
        }
        public static JArray ZoneDevices(string zone_key)
        {
            JArray res = new JArray();
            Dictionary<string, string> devall = cComm.GetPseudoElementsDevices(cJson.CurrentPanelID);
            if (devall == null) return new JArray();
            foreach (string key in devall.Keys)
            {
                string[] keys = Regex.Split(key, "~~~");
                string loop_nom = "1";
                Match m = Regex.Match(keys[0], @"(\d+)$");
                if (m.Success)
                    loop_nom = m.Groups[1].Value;
                JObject odev = JObject.Parse(devall[key]);
                if (odev["~device"] == null)
                    continue;
                odev.Remove("~rw");
                JToken first = odev.First;
                if (first == null || first.Type != JTokenType.Property)
                    continue;
                JProperty pfirst = (JProperty)first;
                JObject grp = (JObject)pfirst.Value;
                if (grp == null || grp["fields"] == null || grp["fields"]["ZONE"] == null)
                    continue;
                JObject zone = (JObject)grp["fields"]["ZONE"];
                if (zone["~path"] == null)
                    continue;
                string path = zone["~path"].ToString();
                string val = cComm.GetPathValue(CurrentPanelID, path);
                if (val == null && zone["@VALUE"] != null)
                    val = zone["@VALUE"].ToString();
                if (val == null || val != zone_key)
                    continue;
                //
                string devname = "";
                JObject oname = (JObject)grp["fields"]["NAME"];
                if (oname["~path"] == null)
                    continue;
                path = oname["~path"].ToString();
                val = cComm.GetPathValue(CurrentPanelID, path);
                if (val != null)
                    devname = val;
                odev["~devname"] = devname;
                odev["~devaddr"] = keys[1];
                odev["~loop"] = keys[0];
                odev["~loop_nom"] = loop_nom;
                odev["~loop_type"] = constants.NO_LOOP;
                odev = GroupsWithValues(odev);
                odev.Remove("~noname");
                res.Add(odev);
            }
            return res;
        }
        #endregion

        #region serialisation
        public static string jobj2string(object o)
        {
            return JsonConvert.SerializeObject(o);
        }

        public static object jstring2obj(string s, System.Type typ)
        {
            return JsonConvert.DeserializeObject(s, typ);
        }
        #endregion

        #region XML to JSON initialization
        private static string FilePathFromSchema(string schema)
        {
            string path = Directory.GetCurrentDirectory();
            if (path[path.Length - 1] != '\\')
                path += @"\";
            path += @"Configs\XML\Systems\";
            string[] dirs = Directory.GetDirectories(path);
            string dir = null;
            foreach (string d in dirs)
                if (Regex.IsMatch(d, @"\\" + schema + "$", RegexOptions.IgnoreCase))
                {
                    dir = d;
                    break;
                }
            if (dir == null)
                return null;
            path = dir + "\\Template";
            string[] files = Directory.GetFiles(path);
            for (int i = 0; i < files.Length; i++)
            {
                string f = System.IO.Path.GetFileName(files[i]);
                if (Regex.IsMatch(f, "^" + schema, RegexOptions.IgnoreCase))
                {
                    string filename = files[i];
                    return filename;
                }
            }
            if (files.Length == 1)
                return files[0];
            return null;
        }
        private static JObject SchemaJSON(string schema)
        {
            string filename = FilePathFromSchema(schema);
            string path = Directory.GetCurrentDirectory();
            if (path[path.Length - 1] != '\\')
                path += @"\";
            path += @"html\pages.json";
            JObject _pages = JObject.Parse(File.ReadAllText(path));
            if (filename != null)
            {
                string xml = File.ReadAllText(filename);
                JObject res = JObject.Parse(ConvertXML(xml, _pages, filename));
                return res;
            }
            return null;
        }
        private static string NoLoopKey(string sj)
        {
            string res = constants.NO_LOOP;
            JObject o = JObject.Parse(sj);
            o = (JObject)o["ELEMENTS"];
            foreach (JProperty p in (JToken)o)
            {
                string pname = p.Name;
                if (Regex.IsMatch(pname, @"NO_LOOP\d*$", RegexOptions.IgnoreCase))
                {
                    pname = Regex.Replace(pname, @"\d*$", "");
                    res = pname;
                    break;
                }
            }
            //
            return res;
        }

        private static string _panel_type = null;
        private static string _panel_subtype = null;
        private static string _last_loaded_template_filepath = null;
        public static string ConvertXML(string xml, JObject _pages, string filename)
        {
            string schema = Regex.Replace(System.IO.Path.GetFileName(filename), @"\.\w+$", "");
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(xml);
            cXmlConfigs cfg = new cXmlConfigs(schema, jobj2string, jstring2obj);
            cfg.ConfigPath = filename;
            cfg.Config = doc;
            string xmldir = Directory.GetParent(Directory.GetParent(filename).ToString()).ToString();
            if (xmldir[xmldir.Length - 1] != '\\')
                xmldir += "\\";
            //if (Directory.Exists(xmldir + "Read"))
            //{
            //    string dir = xmldir + "Read";
            //    string f = null;
            //    string[] files = Directory.GetFiles(dir);
            //    for (int i = 0; i < files.Length; i++)
            //    {
            //        string fname = System.IO.Path.GetFileName(files[i]);
            //        if (!Regex.IsMatch(fname, @"\.xml$", RegexOptions.IgnoreCase))
            //            continue;
            //        if (Regex.IsMatch(fname, @"^Read[\w\W]+?" + schema + @"[_\.]", RegexOptions.IgnoreCase))
            //        {
            //            f = files[i];
            //            break; ;
            //        }
            //    }
            //    if (f != null)
            //    {
            //        cfg.ReadConfigPath = f;
            //        doc = new XmlDocument();
            //        doc.LoadXml(File.ReadAllText(f));
            //        cfg.ReadConfig = doc;
            //    }
            //}
            //if (Directory.Exists(xmldir + "Write"))
            //{
            //    string dir = xmldir + "Write";
            //    string f = null;
            //    string[] files = Directory.GetFiles(dir);
            //    for (int i = 0; i < files.Length; i++)
            //    {
            //        string fname = System.IO.Path.GetFileName(files[i]);
            //        if (!Regex.IsMatch(fname, @"\.xml$", RegexOptions.IgnoreCase))
            //            continue;
            //        if (Regex.IsMatch(fname, @"^Write[\w\W]+?" + schema + @"[_\.]", RegexOptions.IgnoreCase))
            //        {
            //            f = files[i];
            //            break; ;
            //        }
            //    }
            //    if (f != null)
            //    {
            //        cfg.WriteConfigPath = f;
            //        doc = new XmlDocument();
            //        doc.LoadXml(File.ReadAllText(f));
            //        cfg.WriteConfig = doc;
            //    }
            //}
            doc = (XmlDocument)cfg.Config;
            cfg.SetRWFilesToLastVersion();
            WriteReadMerge = cfg.RWMerged();
            SetPanelXMLConfigs(filename, cfg);
            _last_loaded_template_filepath = filename;
            //
            string json = JsonConvert.SerializeXmlNode(doc);
            JObject o = JObject.Parse(json);
            JToken t = o["ELEMENTS"]["ELEMENT"][0];
            o = (JObject)t;
            string prod = o["@PRODUCTNAME"].ToString();
            string sj = "{}";
            if (Regex.IsMatch(prod, @"(iris|simpo|tft)", RegexOptions.IgnoreCase))
            {
                _internal_relations_operator = new cInternalrelIRIS();
                _panel_type = "iris";
                _panel_subtype = prod;
                sj = cIRIS.Convert(json, _pages);
            }
            else if (Regex.IsMatch(prod, @"natron", RegexOptions.IgnoreCase))
            {
                _internal_relations_operator = new cInternalrelIRIS();
                _panel_type = "natron";
                _panel_subtype = prod;
                sj = cNatron.Convert(json, _pages);
            }
            else if (Regex.IsMatch(prod, @"eclipse", RegexOptions.IgnoreCase))
            {
                sj = cEclipse.Convert(json, _pages);
                _panel_type = "eclipse";
            }
            constants.NO_LOOP = NoLoopKey(sj);
            return sj;
        }
        #endregion

        #region Browser content
        private static string _main_content_key = null;
        private static object _cs_main_content_key = new object();
        public static string MainContentKey
        {
            get
            {
                Monitor.Enter(_cs_main_content_key);
                string res = _main_content_key;
                Monitor.Exit(_cs_main_content_key);
                return res;
            }
            set
            {
                Monitor.Enter(_cs_main_content_key);
                _main_content_key = value;
                Monitor.Exit(_cs_main_content_key);
            }
        }

        public static string ContentBrowserParam(string key)
        {
            JObject json = CurrentPanel;
            string content_key = MainContentKey;
            if (content_key == null && json["~panel_type"] != null) content_key = json["~panel_type"].ToString();
            if (json["ELEMENTS"][content_key] == null) content_key = key;
            if (content_key != null)
            {
                if (json["ELEMENTS"][content_key] == null)
                    content_key = content_key.ToUpper();
                JToken tc = json["ELEMENTS"][content_key]["CONTAINS"];
                if (tc != null)
                {
                    SetPanelIDInToken(tc);
                    SetPanelNameInToken(tc);
                    string lc = tc.ToString();
                    return lc;
                }
            }
            JToken t = json["ELEMENTS"][key]["CONTAINS"];
            if (t != null)
            {
                SetPanelIDInToken(t);
                SetPanelNameInToken(t);
                string lc = t.ToString();
                LastContent = lc;
                return lc;
            }
            else
                return LastContent;
        }

        public static string GroupsBrowserParam(string key)
        {
            JObject json = CurrentPanel;
            JObject jkey = (JObject)json["ELEMENTS"][key];
            if (jkey == null) jkey = (JObject)json["ELEMENTS"][key.ToUpper()];
            if (jkey != null)
            {
                JObject jprop = (JObject)jkey["PROPERTIES"];
                if (jprop != null && jprop["Groups"] != null)
                {
                    JObject jgrp = (JObject)jprop["Groups"];
                    if (jgrp != null)
                    {
                        SetPanelIDInToken(jgrp);
                        return GroupsWithValues(jgrp).ToString();
                    }
                    else
                        return "{}";
                }
                else
                {
                    JObject jcontains = (JObject)jkey["CONTAINS"];
                    if (jcontains != null)
                    {
                        SetPanelIDInToken(jcontains);
                        return jcontains.ToString();
                    }
                    else
                        return "{}";
                }
            }
            else
                return "{}";
        }

        private static object _cs_html_left = new object();
        private static string _last_html_left = null;

        public static string htmlLeft(JObject jnode)
        {
            string s = null;
            Monitor.Enter(_cs_html_left);
            JToken t = jnode["left"];
            if (t != null)
                s = t.ToString();
            if (s != null)
                _last_html_left = s;
            s = _last_html_left;
            Monitor.Exit(_cs_html_left);
            return (s != null) ? s : "";
        }

        private static object _cs_html_right = new object();
        private static string _last_html_right = null;

        public static string htmlRight(JObject jnode)
        {
            string s = null;
            Monitor.Enter(_cs_html_right);
            JToken t = jnode["right"];
            if (t != null)
                s = t.ToString();
            if (s != null)
                _last_html_right = s;
            s = _last_html_right;
            Monitor.Exit(_cs_html_right);
            return (s != null) ? s : "";
        }
        private static JObject PagesByType(JObject pages, string _type)
        {
            JObject res = new JObject();
            foreach (JProperty p in pages.Properties())
                if (Regex.IsMatch(p.Name, "^" + _type, RegexOptions.IgnoreCase))
                    res[p.Name] = p.Value;
            //
            return res;
        }
        public static JArray PanelsInLeftBrowser()
        {
            JArray res = new JArray();
            Dictionary<string, JObject> _sys_panels = SystemPanels;
            string spages = File.ReadAllText(@"html\pages.json");
            JObject pages = JObject.Parse(spages);
            foreach (string _id in _system_panels.Keys)
            {
                JObject json = _sys_panels[_id];
                string content_key = json["~panel_type"].ToString();
                if (content_key != null)
                {
                    JToken tc = json["ELEMENTS"][content_key]["CONTAINS"];
                    if (tc != null)
                    {
                        tc["~panel_id"] = _id;
                        tc["~panel_name"] = PanelName(_id);
                        res.Add(tc);
                    }
                }
            }
            //
            return res;
        }
        #endregion

        #region paths
        public static void MakeRelativePath(JToken token, string from)
        {
            if (from == null || from.Trim() == "")
                return;
            if (token.Type == JTokenType.Object)
            {
                JObject o = (JObject)token;
                JToken tp = o["~path"];
                if (tp != null)
                {
                    string path = from + "." + tp.ToString();
                    o["~path"] = path;
                }
                foreach (JToken t in token)
                    MakeRelativePath(t, from);
            }
            else if (token.Type == JTokenType.Array)
            {
                JArray jarr = (JArray)token;
                foreach (JToken t in jarr)
                    MakeRelativePath(t, from);
            }
            else if (token.Type == JTokenType.Property)
            {
                JProperty p = (JProperty)token;
                MakeRelativePath(p.Value, from);
            }
        }

        public static void ChangePath(JToken token, string idx)
        {
            if (token.Type == JTokenType.Object)
            {
                JObject o = (JObject)token;
                JToken tp = o["~path"];
                if (tp != null)
                {
                    string path = tp.ToString();
                    if (!Regex.IsMatch(path, @"\.~index~\d+$"))
                        path += ".~index~" + idx;
                    o["~path"] = path;
                }
                foreach (JToken t in token)
                    ChangePath(t, idx);
            }
            else if (token.Type == JTokenType.Array)
            {
                JArray jarr = (JArray)token;
                foreach (JToken t in jarr)
                    ChangePath(t, idx);
            }
            else if (token.Type == JTokenType.Property)
            {
                JProperty p = (JProperty)token;
                ChangePath(p.Value, idx);
            }
        }
        public static JObject ChangeGroupsElementsPath(JObject groups, string idx)
        {
            JObject res = new JObject(groups);
            foreach (JProperty pgrp in (JToken)res)
                if (pgrp.Value.Type == JTokenType.Object)
                {
                    JObject grp = (JObject)pgrp.Value;
                    ChangePath(grp, idx);
                    JObject fields = (JObject)grp["fields"];
                    if (fields == null)
                    {
                        foreach (JProperty nnp in (JToken)grp)
                        {
                            ChangePath(nnp.Value, idx);
                            continue;
                        }
                        continue;
                    }
                    foreach (JProperty fprop in (JToken)fields)
                    {
                        if (fprop.Value.Type != JTokenType.Object)
                            continue;
                        ChangePath(fprop.Value, idx);
                    }
                }
            //
            return res;
        }

        public static Dictionary<string, Dictionary<string, string>> AnalysePath(string _path, string _newval)
        {
            Dictionary<string, Dictionary<string, string>> res = new Dictionary<string, Dictionary<string, string>>();
            JObject panel = new JObject(CurrentPanel);
            string p = "";
            Match mpath = Regex.Match(_path, @"^([\w\W]+?)\.(ELEMENTS\.[\w\W]+)$");
            if (mpath.Success)
            {
                _path = mpath.Groups[2].Value;
                //
                string prepath = mpath.Groups[1].Value;
                mpath = Regex.Match(prepath, @"([\w\W]+?)#([\w\W]+)$");
                if (mpath.Success)
                {
                    string dev = mpath.Groups[2].Value;
                    prepath = mpath.Groups[1].Value;
                    mpath = Regex.Match(prepath, @"([\w\W]+?)/([\w\W]+)$");
                    if (mpath.Success)
                    {
                        p = mpath.Groups[1].Value;
                        res.Add(p, new Dictionary<string, string>());
                        res[p].Add("LOOPTYPE", mpath.Groups[1].Value);
                        p += "." + mpath.Groups[2].Value;
                        res.Add(p, new Dictionary<string, string>());
                        res[p].Add("NOELEMENT", mpath.Groups[2].Value);
                    }
                    p += ((p != "") ? "." : "") + dev;
                    res.Add(p, new Dictionary<string, string>());
                    res[p].Add("DEVICE", dev);
                }
            }
            p = "";
            string[] _patharr = _path.Split('.');
            for (int i = 0; i < _patharr.Length; i++)
            {
                p += ((p != "") ? "." : "") + _patharr[i];
                Match m = Regex.Match(_patharr[i], @"~index~(\d+)", RegexOptions.IgnoreCase);
                if (m.Success)
                {
                    Dictionary<string, string> didx = new Dictionary<string, string>();
                    didx.Add("INDEX", m.Groups[1].Value);
                    res.Add(p, didx);
                    continue;
                }
                if (p == "ELEMENTS")
                    continue;
                if (Regex.IsMatch(_patharr[i], "(INPUT|OUTPUT|SNONE|MNONE|TTENONE)"))
                {
                    Dictionary<string, string> dio = new Dictionary<string, string>();
                    string typ = Regex.Replace(_patharr[i], @"^[\w\W]*?_", "");
                    dio.Add("TYPE", typ);
                    JToken tlst = panel.SelectToken(p);
                    if (tlst.Type == JTokenType.Object)
                    {
                        JObject olst = (JObject)tlst;
                        JToken rules = olst["RULES"];
                        if (rules != null)
                            dio.Add("RULES", rules.ToString());
                    }
                    res.Add(p, dio);
                }
                JToken t = panel.SelectToken(p);
                Dictionary<string, string> d = null;
                if (res.ContainsKey(p))
                    d = res[p];
                if (t != null && t.Type == JTokenType.Object)
                {
                    JObject o = (JObject)t;
                    JToken tprop = o["@ID"];
                    if (tprop != null)
                    {
                        if (d == null)
                            d = new Dictionary<string, string>();
                        if (!d.ContainsKey("@ID"))
                            d.Add("@ID", tprop.ToString());
                    }
                    tprop = o["@NAME"];
                    if (tprop != null)
                    {
                        if (d == null)
                            d = new Dictionary<string, string>();
                        if (!d.ContainsKey("@NAME"))
                            d.Add("@NAME", tprop.ToString());
                    }
                    if (i == _patharr.Length - 2 && Regex.IsMatch(_patharr[i + 1], "~index~", RegexOptions.IgnoreCase))
                        if (!Regex.IsMatch(_newval, @"\D"))
                        {
                            JToken titems = o["ITEMS"];
                            if (titems != null)
                            {
                                JToken titem = panel.SelectToken(p + ".ITEMS.ITEM[" + _newval + "]");
                                if (titem != null)
                                {
                                    JObject oitem = (JObject)titem;
                                    tprop = oitem["@NAME"];
                                    if (tprop != null)
                                    {
                                        if (d == null)
                                            d = new Dictionary<string, string>();
                                        if (!d.ContainsKey("ITEM/@NAME"))
                                            d.Add("ITEM/@NAME", tprop.ToString());
                                    }
                                    tprop = oitem["@ID"];
                                    if (tprop != null)
                                    {
                                        if (d == null)
                                            d = new Dictionary<string, string>();
                                        if (!d.ContainsKey("ITEM/@ID"))
                                            d.Add("ITEM/@ID", tprop.ToString());
                                    }
                                }
                            }
                        }
                }
                if (!res.ContainsKey(p) && d != null)
                    res.Add(p, d);
            }
            //
            return res;
        }

        private static void ObjectsWithPath(JObject root, List<JObject> lst)
        {
            if (root == null || lst == null)
                return;
            if (root["~path"] != null)
                lst.Add(root);
            foreach (JProperty p in root.Properties())
            {
                if (p.Value.Type == JTokenType.Object)
                    ObjectsWithPath((JObject)p.Value, lst);
                else if (p.Value.Type == JTokenType.Array)
                {
                    JArray a = (JArray)p.Value;
                    foreach (JToken t in a)
                        if (t.Type == JTokenType.Object)
                            ObjectsWithPath((JObject)t, lst);
                }
            }
            //foreach (JToken t in (JToken)root)
            //{
            //    if (t.Type == JTokenType.Object)
            //    {
            //        string path = ((JObject)t)["~path"].ToString();
            //        if (path != null)
            //            lst.Add((JObject)t);
            //        ObjectsWithPath((JObject)t, lst);
            //    }
            //    else if (t.Type == JTokenType.Property && ((JProperty)t).Value.Type == JTokenType.Object)
            //    {
            //        JProperty p = (JProperty)t;
            //        JObject o = (JObject)p.Value;
            //        ObjectsWithPath(o, lst);
            //    }
            //    else if (t.Type == JTokenType.Property && ((JProperty)t).Value.Type == JTokenType.String)
            //    {
            //        JProperty p = (JProperty)t;
            //        if (p.Name == "~path" && p.Parent.Type == JTokenType.Object)
            //            lst.Add((JObject)p.Parent);
            //    }
            //    else
            //    {
            //        string s = "";
            //    }
            //}
        }
        #endregion

        #region values
        public static string InputsElementName()
        {
            JObject _panel = CurrentPanel;
            JObject elements = (JObject)_panel["ELEMENTS"];
            foreach (JProperty p in elements.Properties()) if (Regex.IsMatch(p.Name, @"IRIS\d*?_INPUT$")) return p.Name;
            return null;
        }
        public static List<string> SelectPathValues(string match)
        {
            Dictionary<string, string> d = cComm.GetPathValues(CurrentPanelID, match);
            List<string> res = new List<string>();
            foreach (string key in d.Keys) res.Add(d[key]);
            return res;
        }
        public static List<string> SelectPathValuesDistinctStrSort(string match)
        {
            Dictionary<string, string> d = cComm.GetPathValues(CurrentPanelID, match);
            Dictionary<string, string> dchk = new Dictionary<string, string>();
            foreach (string key in d.Keys) if (!dchk.ContainsKey(d[key])) dchk.Add(d[key], null);
            List<string> res = dchk.Keys.ToList<string>();
            res.Sort();
            return res;
        }
        public static List<string> SelectPathValuesDistinctIntSort(string match)
        {
            Dictionary<string, string> d = cComm.GetPathValues(CurrentPanelID, match);
            Dictionary<int, string> dchk = new Dictionary<int, string>();
            foreach (string key in d.Keys) if (!dchk.ContainsKey(Convert.ToInt32(d[key]))) dchk.Add(Convert.ToInt32(d[key]), null);
            List<int> lsti = dchk.Keys.ToList<int>();
            lsti.Sort();
            List<string> res = new List<string>();
            foreach (int i in lsti) res.Add(i.ToString());
            return res;
        }
        public static void RemoveElement(string element, string idx)
        {
            string tab = null;
            cComm.RemoveElementPaths(CurrentPanelID, element, idx, ref tab);
            if (Regex.IsMatch(element, "INPUT$", RegexOptions.IgnoreCase))
                _internal_relations_operator.AfterInputRemoved(CurrentPanelID);
            if (tab != null)
                _internal_relations_operator.RemoveTABCache(tab, idx);
        }
        public static void RemoveLoopElement(string loop_type, string loop_nom, string dev_name, string dev_addr)
        {
            cComm.RemoveLoopDevice(CurrentPanelID, loop_type, loop_nom, dev_name, dev_addr);
            _internal_relations_operator.AfterDevicesChanged(CurrentPanelID, CurrentPanel, GetNode);
            _internal_relations_operator.AfterDeviceRemoved(CurrentPanelID, CurrentPanel, loop_type, dev_addr);
            cComm.RemoveLoopDevicePathValues(CurrentPanelID, loop_type, loop_nom, dev_name, dev_addr);
        }
        public static void RemoveLoop(string loop_none, string loop_nom)
        {
            string loop = cComm.GetPseudoElement(CurrentPanelID, loop_none, loop_nom);
            if (loop == null)
                return;
            JObject o = JObject.Parse(loop);
            if (o["~loop_type"] == null)
                return;
            string loop_type = o["~loop_type"].ToString();
            //"~loop_type": "IRIS_TTELOOP1"
            Dictionary<string, string> devs = cComm.GetPseudoElementDevices(CurrentPanelID, loop_none, loop_nom);
            if (devs == null)
            {
                cComm.RemovePseudoElement(CurrentPanelID, loop_none, loop_nom);
                return;
            }
            foreach (string dev_addr in devs.Keys)
            {
                o = JObject.Parse(devs[dev_addr]);
                if (o["~device"] == null)
                    continue;
                string dev_name = o["~device"].ToString();
                RemoveLoopElement(loop_type, loop_nom, dev_name, dev_addr);
            }
            cComm.RemovePseudoElement(CurrentPanelID, loop_none, loop_nom);
        }
        public static void FilterValueChanged(string path, string _new_val)
        {
            bool remove_path = false;
            _internal_relations_operator.FilterValueChanged(path, _new_val, ref remove_path);
            if (remove_path)
            {
                Match m = Regex.Match(path, @"~index~(\d+)");
                if (m.Success)
                {
                    string idx = m.Groups[1].Value;
                    string val = cComm.GetPathValue(CurrentPanelID, path);
                    string tab = "";
                    if (val != null)
                    {
                        m = Regex.Match(val, @"_([\w\W]+)$");
                        if (m.Success)
                            tab = m.Groups[1].Value;
                        _internal_relations_operator.RemoveTABCache(tab, idx);
                    }
                }
                //
                cComm.RemovePathValues(CurrentPanelID, path);
            }
        }
        public static string DevName(string devtype)
        {
            if (CurrentPanel["~devtypes"] == null) return null;
            JObject devs = (JObject)CurrentPanel["~devtypes"];
            if (devs[devtype] != null) return Regex.Replace(devs[devtype].ToString(), "^natron_device_", "");
            return "NONE";
        }
        public static JObject GroupsWithValues(JObject grp)
        {
            List<JObject> lst = new List<JObject>();
            JObject res = new JObject(grp);
            ObjectsWithPath(res, lst);
            string panel_id = CurrentPanelID;
            foreach (JObject o in lst)
            {
                string path = o["~path"].ToString();
                if (Regex.IsMatch(path, @"1ef9a\."))
                {
                    string s = "";
                }
                string val = cComm.GetPathValue(panel_id, path);
                if (val != null)
                    o["~value"] = val;
            }
            if (res["~noname"] != null && res["~noname"]["fields"] != null && res["~noname"]["fields"]["TYPE"] != null)
            {
                JObject otype = (JObject)res["~noname"]["fields"]["TYPE"];
                string tpath = "";
                if (otype["~path"] != null) tpath = otype["~path"].ToString();
                if (Regex.IsMatch(tpath, "natron_device") && otype["~value"] != null)
                {
                    string sval = otype["~value"].ToString();
                    if (CurrentPanel["~pdtypes"] != null && CurrentPanel["~pdtypes"][sval] != null)
                        sval = Regex.Replace(CurrentPanel["~pdtypes"][sval].ToString(), "^natron_device_", "");
                    else sval = "NONE";
                    otype["~strtype"] = sval;
                }
            }
            return res;
        }

        public static JObject GroupsWithValues(string grp)
        {
            return GroupsWithValues(JObject.Parse(grp));
        }
        public static string GetListElement(string elementType, string elementNumber)
        {
            string res = cComm.GetListElement(CurrentPanelID, elementType, elementNumber);
            JObject o = JObject.Parse(res);
            SetNodeFilters(o);
            res = o.ToString();
            return res;
        }
        //cJson.SetNodeFilters(newpaths);
        public static void ChangeElementAddress(string oldAddress, string elementType, string newAddress)
        {
            cComm.ChangeElementAddress(CurrentPanelID, elementType, oldAddress, newAddress);
            _internal_relations_operator.OnElementAddressChanged(oldAddress, elementType, newAddress);
            Dictionary<string, string> pv = cComm.GetPathValues(CurrentPanelID);
            Dictionary<string, string> old_new_path = new Dictionary<string, string>();
            foreach (string path in pv.Keys)
                if (Regex.IsMatch(path, @"[\w\W]+?\." + elementType + @"\.[\w\W]+?~index~" + oldAddress))
                {
                    string newpath = Regex.Replace(path, @"~index~" + oldAddress + "$", "~index~" + newAddress);
                    old_new_path.Add(path, newpath);
                }
            //
            foreach (string path in old_new_path.Keys)
            {
                string val = pv[path];
                //Match m = Regex.Match(val, oldAddress + @"_([0-9a-fA-f]{8}\-[0-9a-fA-F]{4}\-[0-9a-fA-F]{4}\-[0-9a-fA-F]{4}\-[0-9a-fA-F]{12})$");
                //if (m.Success)
                //    val = newAddress + "_" + m.Groups[1].ToString();
                cComm.RemovePathValue(CurrentPanelID, path);
                cComm.SetPathValue(CurrentPanelID, old_new_path[path], val);
            }
        }
        public static void ChangeDeviceAddress(string oldAddress, string loopType, string newAddress)
        {
            cComm.ChangeDeviceAddress(CurrentPanelID, oldAddress, loopType, newAddress);
            _internal_relations_operator.OnDeviceAddressChanged(oldAddress, loopType, newAddress);
        }
        public static JArray MIMICPanels()
        {
            JArray res = new JArray();
            JObject _panel = CurrentPanel;
            JObject _main = (JObject)_panel["ELEMENTS"]["iris"];
            if (_main == null) return res;
            JObject _content = (JObject)_main["CONTAINS"];
            foreach (JProperty _p in _content.Properties())
                if (Regex.IsMatch(_p.Name, @"^SIMPO[\w\W]+?MIMIC", RegexOptions.IgnoreCase))
                {
                    JObject _mimic = (JObject)_panel["ELEMENTS"][_p.Name];
                    if (_mimic == null) return res;
                    JObject _mimicc = (JObject)_mimic["CONTAINS"];
                    foreach (JProperty ppanel in _mimicc.Properties())
                    {
                        string _id = ppanel.Name;
                        JObject node = GetNode(_id);
                        if (node != null) res.Add(node);
                    }
                }
            //
            return res;
        }
        #endregion
    }
}
