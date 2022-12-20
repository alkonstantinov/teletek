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

namespace ljson
{
    public class cJson
    {
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

        private static void SetPanelXMLConfigs(string key, cXmlConfigs cfg)
        {
            Monitor.Enter(_cs_panel_xmls);
            if (_panel_xmls.ContainsKey(key))
                _panel_xmls[key] = cfg;
            else
                _panel_xmls.Add(key, cfg);
            Monitor.Exit(_cs_panel_xmls);
        }

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

        public static JObject GetNode(string name)
        {
            JObject _panel = CurrentPanel;
            if (_panel != null)
            {
                JObject _elements = (JObject)_panel["ELEMENTS"];
                if (_elements != null)
                {
                    JObject _res = (JObject)_elements[name];
                    if (_res != null)
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
            return (JObject)_panel["ELEMENTS"][name];
        }
        public static string ConvertXML(string xml, JObject _pages, string filename)
        {
            string schema = Regex.Replace(System.IO.Path.GetFileName(filename), @"\.\w+$", "");
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(xml);
            cXmlConfigs cfg = new cXmlConfigs();
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
            SetPanelXMLConfigs(filename, cfg);
            //
            string json = JsonConvert.SerializeXmlNode(doc);
            JObject o = JObject.Parse(json);
            JToken t = o["ELEMENTS"]["ELEMENT"][0];
            o = (JObject)t;
            string prod = o["@PRODUCTNAME"].ToString();
            if (Regex.IsMatch(prod, @"iris", RegexOptions.IgnoreCase))
                return cIRIS.Convert(json, _pages);
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
                            if (nnp.Value.Type != JTokenType.Object)
                                continue;
                            JObject nnf = (JObject)nnp.Value;
                            if (nnf != null)
                                nnf["~path"] += ".~index~" + idx;
                        }
                        continue;
                    }
                    foreach (JProperty fprop in (JToken)fields)
                    {
                        if (fprop.Value.Type != JTokenType.Object)
                            continue;
                        JObject oprop = (JObject)fprop.Value;
                        oprop["~path"] += ".~index~" + idx;
                    }
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
    }
}
