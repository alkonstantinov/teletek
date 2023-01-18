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

        private static Dictionary<string, JObject> _panel_temlates = new Dictionary<string, JObject>();
        private static object _cs_panel_templates = new object();

        private static JObject GetPanelTemplate(string name)
        {
            JObject res = null;
            Monitor.Enter(_cs_panel_templates);
            if (_panel_temlates.ContainsKey(name))
                res = _panel_temlates[name];
            Monitor.Exit(_cs_panel_templates);
            return res;
        }

        private static void SetPanelTemplate(string name, JObject json)
        {
            Monitor.Enter(_cs_panel_templates);
            if (!_panel_temlates.ContainsKey(name))
                _panel_temlates.Add(name, json);
            else
                _panel_temlates[name] = json;
            Monitor.Exit(_cs_panel_templates);
        }

        private static JObject _current_panel = null;
        private static object _cs_current_panel = new object();
        private static JObject CurrentPanel
        {
            get
            {
                Monitor.Enter(_cs_current_panel);
                JObject res = _current_panel;
                Monitor.Exit(_cs_current_panel);
                return res;
            }
            set
            {
                Monitor.Enter(_cs_current_panel);
                _current_panel = value;
                Monitor.Exit(_cs_current_panel);
            }
        }

        public static string CurrentPanelID
        {
            get
            {
                JObject _panel = CurrentPanel;
                return _panel["@LNGID"].ToString();
            }
        }

        private static object _cs_last_content = new object();
        private static string _last_content = null;

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

        private static object _cs_write_read_merge = new object();
        private static Dictionary<string, cRWPath> _write_read_merge = null;

        private static Dictionary<string, cRWPath> WriteReadMerge
        {
            get
            {
                Monitor.Enter(_cs_write_read_merge);
                Dictionary<string, cRWPath> res = _write_read_merge;
                Monitor.Exit(_cs_write_read_merge);
                return res;
            }
            set
            {
                Monitor.Enter(_cs_write_read_merge);
                _write_read_merge = value;
                Monitor.Exit(_cs_write_read_merge);
            }
        }

        private static string FilePathFromSchema(string schema)
        {
            string path = Directory.GetCurrentDirectory();
            if (path[path.Length - 1] != '\\')
                path += @"\";
            path += @"Configs\XML\Templates\";
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

        private static cRWPath RWLoopPath(Dictionary<string, cRWPath> _merge, string path)
        {
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
                        if (Regex.IsMatch(_merge[key].ReadPath, "PANEL$", RegexOptions.IgnoreCase))
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
                    if (key.ToLower() == path.ToLower() || _merge[key].ReadPath.ToLower() == path.ToLower())
                    {
                        rw = _merge[key];
                        break;
                    }
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
                    if (Regex.IsMatch(key, path1 + "$", RegexOptions.IgnoreCase) || Regex.IsMatch(_merge[key].ReadPath, path1 + "$", RegexOptions.IgnoreCase))
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
        }

        public static JObject GetNode(string name)
        {
            JObject _panel = CurrentPanel;
            if (_panel != null)
            {
                JObject _elements = (JObject)_panel["ELEMENTS"];
                if (_elements != null)
                {
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
                        _res = new JObject((JObject)_elements[names[0]]);
                        cRWPath rw = RWLoopPath(WriteReadMerge, name);
                        if (rw != null)
                        {
                            JObject o = JObject.FromObject(rw);
                            _res["~rw"] = o;
                        }
                    }
                    return _res;
                }
            }
            //
            string filename = FilePathFromSchema(name);
            _panel = GetPanelTemplate(filename);
            if (_panel != null)
                return (JObject)_panel["ELEMENTS"][name];
            Monitor.Enter(_cs_current_panel);
            Monitor.Enter(_cs_panel_templates);
            Monitor.Enter(_cs_main_content_key);
            _panel = _current_panel;
            if (_panel == null)
                _panel = SchemaJSON(name);
            if (!_panel_temlates.ContainsKey(filename))
                _panel_temlates.Add(filename, _panel);
            else
                _panel_temlates[filename] = _panel;
            _current_panel = _panel;
            _main_content_key = name;
            Monitor.Exit(_cs_main_content_key);
            Monitor.Exit(_cs_panel_templates);
            Monitor.Exit(_cs_current_panel);
            JObject _res1 = new JObject((JObject)_panel["ELEMENTS"][name]);
            RWData(_res1);
            return _res1;
        }

        #region internal relations operations
        private static cInternalRel _internal_relations_operator = null;
        public static void SetNodeFilters(JObject _node)
        {
            _internal_relations_operator.SetNodeFilters(CurrentPanelID, _node);
        }
        #endregion

        public static string jobj2string(object o)
        {
            return JsonConvert.SerializeObject(o);
        }

        public static object jstring2obj(string s, System.Type typ)
        {
            return JsonConvert.DeserializeObject(s, typ);
        }
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
            if (Directory.Exists(xmldir + "Read"))
            {
                string dir = xmldir + "Read";
                string f = null;
                string[] files = Directory.GetFiles(dir);
                for (int i = 0; i < files.Length; i++)
                    if (Regex.IsMatch(System.IO.Path.GetFileName(files[i]), @"^Read[\w\W]+?" + schema, RegexOptions.IgnoreCase))
                    {
                        f = files[i];
                        break; ;
                    }
                if (f != null)
                {
                    cfg.ReadConfigPath = f;
                    doc = new XmlDocument();
                    doc.LoadXml(File.ReadAllText(f));
                    cfg.ReadConfig = doc;
                }
            }
            if (Directory.Exists(xmldir + "Write"))
            {
                string dir = xmldir + "Write";
                string f = null;
                string[] files = Directory.GetFiles(dir);
                for (int i = 0; i < files.Length; i++)
                    if (Regex.IsMatch(System.IO.Path.GetFileName(files[i]), @"^Write[\w\W]+?" + schema, RegexOptions.IgnoreCase))
                    {
                        f = files[i];
                        break; ;
                    }
                if (f != null)
                {
                    cfg.WriteConfigPath = f;
                    doc = new XmlDocument();
                    doc.LoadXml(File.ReadAllText(f));
                    cfg.WriteConfig = doc;
                }
            }
            doc = (XmlDocument)cfg.Config;
            WriteReadMerge = cfg.RWMerged();
            SetPanelXMLConfigs(filename, cfg);
            //
            string json = JsonConvert.SerializeXmlNode(doc);
            JObject o = JObject.Parse(json);
            JToken t = o["ELEMENTS"]["ELEMENT"][0];
            o = (JObject)t;
            string prod = o["@PRODUCTNAME"].ToString();
            if (Regex.IsMatch(prod, @"iris", RegexOptions.IgnoreCase))
            {
                _internal_relations_operator = new cInternalrelIRIS();
                return cIRIS.Convert(json, _pages);
            }
            else if (Regex.IsMatch(prod, @"eclipse", RegexOptions.IgnoreCase))
                return cEclipse.Convert(json, _pages);
            return "";
        }

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
            if (content_key != null)
            {
                JToken tc = json["ELEMENTS"][content_key]["CONTAINS"];
                if (tc != null)
                {
                    string lc = tc.ToString();
                    return lc;
                }
            }
            JToken t = json["ELEMENTS"][key]["CONTAINS"];
            if (t != null)
            {
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
            if (jkey != null)
            {
                JObject jprop = (JObject)jkey["PROPERTIES"];
                if (jprop != null)
                {
                    JObject jgrp = (JObject)jprop["Groups"];
                    if (jgrp != null)
                        return GroupsWithValues(jgrp).ToString();
                    else
                        return "{}";
                }
                else
                {
                    JObject jcontains = (JObject)jkey["CONTAINS"];
                    if (jcontains != null)
                        return jcontains.ToString();
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
                    string path = tp.ToString() + ".~index~" + idx;
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
            foreach (JToken t in (JToken)root)
            {
                if (t.Type == JTokenType.Object)
                {
                    string path = ((JObject)t)["~path"].ToString();
                    if (path != null)
                        lst.Add((JObject)t);
                    ObjectsWithPath((JObject)t, lst);
                }
                else if (t.Type == JTokenType.Property && ((JProperty)t).Value.Type == JTokenType.Object)
                {
                    JProperty p = (JProperty)t;
                    JObject o = (JObject)p.Value;
                    ObjectsWithPath(o, lst);
                }
                else if (t.Type == JTokenType.Property && ((JProperty)t).Value.Type == JTokenType.String)
                {
                    JProperty p = (JProperty)t;
                    if (p.Name == "~path" && p.Parent.Type == JTokenType.Object)
                        lst.Add((JObject)p.Parent);
                }
                else
                {
                    string s = "";
                }
            }
        }
        #endregion

        #region values
        public static JObject GroupsWithValues(JObject grp)
        {
            List<JObject> lst = new List<JObject>();
            JObject res = new JObject(grp);
            ObjectsWithPath(res, lst);
            string panel_id = CurrentPanelID;
            foreach (JObject o in lst)
            {
                string path = o["~path"].ToString();
                string val = cComm.GetPathValue(panel_id, path);
                if (val != null)
                    o["~value"] = val;
            }
            return res;
        }

        public static JObject GroupsWithValues(string grp)
        {
            return GroupsWithValues(JObject.Parse(grp));
        }
        #endregion
    }
}
