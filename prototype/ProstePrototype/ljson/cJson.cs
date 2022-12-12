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

namespace ljson
{
    public class cJson
    {
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

        private static string FilePathFromSchema(string schema)
        {
            string path = Directory.GetCurrentDirectory();
            DirectoryInfo di = Directory.GetParent(path);
            for (int i = 1; i < 6; i++)
                di = di.Parent;
            path = di.ToString();
            if (path[path.Length - 1] != '\\')
                path += '\\';
            path += @"Configs\XML\Templates";
            string[] files = Directory.GetFiles(path);
            for (int i = 0; i < files.Length; i++)
            {
                string f = Path.GetFileName(files[i]);
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
                JObject res = JObject.Parse(ConvertXML(File.ReadAllText(filename), _pages));
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
            _panel = _current_panel;
            if (_panel == null)
                _panel = SchemaJSON(name);
            if (!_panel_temlates.ContainsKey(filename))
                _panel_temlates.Add(filename, _panel);
            else
                _panel_temlates[filename] = _panel;
            _current_panel = _panel;
            Monitor.Exit(_cs_panel_templates);
            Monitor.Exit(_cs_current_panel);
            return (JObject)_panel["ELEMENTS"][name];
        }
        public static string ConvertXML(string xml, JObject _pages)
        {
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(xml);
            string json = JsonConvert.SerializeXmlNode(doc);
            JObject o = JObject.Parse(json);
            JToken t = o["ELEMENTS"]["ELEMENT"][0];
            o = (JObject)t;
            string prod = o["@PRODUCTNAME"].ToString();
            if (Regex.IsMatch(prod, @"iris", RegexOptions.IgnoreCase))
                return cIRIS.Convert(json, _pages);
            return "";
        }

        public static string ContentBrowserParam(string key)
        {
            JObject json = CurrentPanel;
            JToken t = json["ELEMENTS"][key]["CONTAINS"];
            if (t != null)
                return t.ToString();
            else
                return "{}";
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
                        return jgrp.ToString();
                    else
                        return "{}";
                }
                else
                    return "{}";
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
    }
}
