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
using System.Windows.Media.Animation;
using System.Windows.Input;
using System.Security.Cryptography;

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

        #region read/write
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

        private static bool readed = false;
        private static bool reading = false;
        private static Dictionary<string, byte[]> _log_bytesreaded = new Dictionary<string, byte[]>();
        private static Dictionary<string, Dictionary<string, Dictionary<string, byte[]>>> ReadLoopDevices(cTransport conn, cRWPath p, byte min, byte max, string read_path, JObject devtypes)
        {
            Dictionary<string, Dictionary<string, Dictionary<string, byte[]>>> res = new Dictionary<string, Dictionary<string, Dictionary<string, byte[]>>>();
            List<cRWCommand> cmds = new List<cRWCommand>();
            //Dictionary<string, cRWProperty> ReadProperties = new Dictionary<string, cRWProperty>();
            if (_panel_type == "iris")
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
                return res;
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
                    if (tprop.Name.ToLower() == "type")
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
                    string scmd = cmd.CommandString();
                    scmd = scmd.Substring(0, cmd.idxPosition() * 2) + idx.ToString("X2") + scmd.Substring((cmd.idxPosition() + 1) * 2);
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
                string devtype = Regex.Replace(nonekey, @"/[\w\W]*$", "");
                JObject devbytype = null;
                if (devtypes[devtype] != null)
                    devbytype = (JObject)devtypes[devtype];
                string sdevname = null;
                if (devbytype[bcmdres[typeidx].ToString()] != null)
                    sdevname = devbytype[bcmdres[typeidx].ToString()].ToString();
                if (typeidx >= 0 && bcmdres[typeidx] != 0 && sdevname != null)
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
            Dictionary<string, string> dres = cComm.GetPseudoElementsList(panel_id, constants.NO_LOOP);
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
                if (Regex.IsMatch(apath[i], @"(LOOP\d+|NONE)$"))
                {
                    if (Regex.IsMatch(apath[i], @"LOOP\d+$"))
                        loop_type = apath[i];
                    dev_path += ((dev_path != "") ? "/" : "") + apath[i];
                }
            loop_type = Regex.Replace(loop_type, @"\d+$", num.ToString());
            //
            foreach (string snone in dreaded.Keys)
            {
                string nonekey = loopkey + "/" + snone;
                if (!merge.ContainsKey(nonekey))
                    nonekey = Regex.Replace(nonekey, @"/[^/]+$", "");
                if (!merge.ContainsKey(nonekey))
                    nonekey = "IRIS_LOOPDEVICES/" + nonekey;
                if (merge.ContainsKey(nonekey))
                {
                    Dictionary<string, cRWProperty> ReadProperties = new Dictionary<string, cRWProperty>();
                    cRWPath p = merge[nonekey];
                    int typeidx = -1;
                    foreach (string k in p.ReadProperties.Keys)
                    {
                        cRWProperty prop = p.ReadProperties[k];
                        ReadProperties.Add(k, prop);
                        if (typeidx <= 0 && Regex.IsMatch(k, "^type$", RegexOptions.IgnoreCase))
                            typeidx = prop.offset;
                    }
                    //
                    if (!loopadded)
                    {
                        string jnode = "{}";
                        JToken t = panel["ELEMENTS"]["NO_LOOP" + num.ToString()];
                        if (t != null)
                            jnode = t.ToString();
                        cComm.AddPseudoElement(panel_id, "NO_LOOP", jnode);
                        JObject o = JObject.Parse(cComm.GetPseudoElement(panel_id, constants.NO_LOOP));
                        o["~loop_type"] = loop_type;
                        cComm.SetPseudoElement(panel_id, constants.NO_LOOP, o.ToString());
                        loopadded = true;
                    }
                    dev_path = Regex.Replace(snone, @"^[\w\W]*?/", "");// apath[apath.Length - 1];
                    none_element = dev_path;
                    //
                    JObject el = GetNode(dev_path);
                    if (el != null && el["PROPERTIES"] != null && el["PROPERTIES"]["Groups"] != null)
                        groups = (JObject)el["PROPERTIES"]["Groups"];
                    JObject _rw = (JObject)el["~rw"];
                    //
                    Dictionary<string, byte[]> didx = dreaded[snone];
                    foreach (string idx in didx.Keys)
                    {
                        byte btype = didx[idx][typeidx];
                        string devname = devtypes[btype.ToString()].ToString();
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
                            Tuple<string, string> path_val = _internal_relations_operator.GroupPropertyVal(groups, propname, pbytes);
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
        private static Dictionary<string, Dictionary<string, Dictionary<string, byte[]>>> ReadSeriaDevices(cTransport conn, cRWPath p, byte min, byte max, string read_path, JObject devtypes)
        {
            if (Regex.IsMatch(read_path, @"LOOP"))
                return ReadLoopDevices(conn, p, min, max, read_path, devtypes);
            Dictionary<string, Dictionary<string, Dictionary<string, byte[]>>> res = new Dictionary<string, Dictionary<string, Dictionary<string, byte[]>>>();
            //if (Regex.IsMatch(read_path, @"(INPUTS_GR|INPUTS/|OUTPUTS|PERIPHER)"))
            //{
            List<cRWCommand> cmds = new List<cRWCommand>();
            if (_panel_type == "iris")
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
            string[] apath = read_path.Split('/');
            res.Add(apath[0], new Dictionary<string, Dictionary<string, byte[]>>());
            Dictionary<string, Dictionary<string, byte[]>> droot = res[apath[0]];
            droot.Add(apath[1], new Dictionary<string, byte[]>());
            Dictionary<string, byte[]> didx = droot[apath[1]];
            //
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
                List<byte[]> cmdresults = new List<byte[]>();
                if (!p.ReadCommandsReplacement.ContainsKey(idx.ToString("X2")))
                    foreach (cRWCommand cmd in cmds)
                    {
                        string scmd = cmd.CommandString();
                        scmd = scmd.Substring(0, cmd.idxPosition() * 2) + idx.ToString("X2") + scmd.Substring((cmd.idxPosition() + 1) * 2);
                        byte[] cmdres = cComm.SendCommand(conn, scmd);
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
                if (_internal_relations_operator.AddSerialDevice(apath[1], node, bcmdres, (byte)(idx + idxinc)))
                    didx.Add((idx + idxinc).ToString(), bcmdres);
            }
            //if (Regex.IsMatch(read_path, @"(PERIPHER)"))
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
                if (Regex.IsMatch(loopkey, @"LOOP"))
                {
                    SetLoopDevicesValues(loopkey, dreaded[loopkey], devtypes, missedkeys, foundkeys, dloopdevs);
                    continue;
                }
                Dictionary<string, cRWPath> merge = WriteReadMerge;
                string readkey = loopkey;
                string readsubkey = dreaded[readkey].Keys.First();
                string key2adddev = readsubkey;
                cRWPath rwpath = null;
                if (merge.ContainsKey(readkey))
                    rwpath = merge[readkey];
                else if (merge.ContainsKey(readsubkey))
                    rwpath = merge[readsubkey];
                else if (merge.ContainsKey(readkey + "/" + readsubkey))
                    rwpath = merge[readkey + "/" + readsubkey];
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
                            cRWProperty prop = ReadProperties[propname];
                            byte[] pbytes = new byte[prop.bytescnt];
                            if (prop.offset + prop.bytescnt > devbytes.Length)
                                continue;
                            for (int i = prop.offset; i < prop.offset + prop.bytescnt; i++)
                                pbytes[i - prop.offset] = devbytes[i];
                            Tuple<string, string> path_val = _internal_relations_operator.GroupPropertyVal(newpaths, propname, pbytes);
                            if (path_val == null)
                            {
                                if (!missedkeys.ContainsKey(readsubkey + "/" + propname))
                                    missedkeys.Add(readsubkey + "/" + propname, "");
                            }
                            else
                            {
                                cComm.SetPathValue(panel_id, path_val.Item1, path_val.Item2, FilterValueChanged);
                                if (!foundkeys.ContainsKey(readsubkey + "/" + propname))
                                    foundkeys.Add(readsubkey + "/" + propname, "");
                            }
                        }
                    }
                }
            }
        }
        public static void ReadDevice(cTransport conn)
        {
            //if (readed)
            //    return;
            //if (reading)
            //    return;
            //reading = true;
            cComm.ClearCache();
            if (settings.logreads)
                _log_bytesreaded.Clear();
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
            string panel_type = null;
            foreach (JToken t in (JToken)_elements)
                if (t.Type == JTokenType.Property && ((JProperty)t).Value.Type == JTokenType.Object)
                {
                    JObject _node = new JObject((JObject)((JProperty)t).Value);
                    string nodename = ((JProperty)t).Name;
                    if (Regex.IsMatch(nodename, "loop", RegexOptions.IgnoreCase))
                        panel_type = panel_type;
                    //continue;
                    if (Regex.IsMatch(nodename, "snone", RegexOptions.IgnoreCase))
                        panel_type = panel_type;
                    RWData(_node);
                    if (_node["~rw"] != null)
                    {
                        JObject jrw = (JObject)_node["~rw"];
                        if (!drw.ContainsKey(((JProperty)t).Name))
                        {
                            string tname = ((JProperty)t).Name;
                            if (panel_type == null && Regex.IsMatch(tname, "^iris", RegexOptions.IgnoreCase))
                                panel_type = "iris";
                            if (panel_type == "iris")
                            {
                                cRWPathIRIS rwi = jrw.ToObject<cRWPathIRIS>();
                                drw.Add(((JProperty)t).Name, rwi);
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
                Dictionary<string, Tuple<byte, byte>> dpath_log = new Dictionary<string, Tuple<byte, byte>>();
                //cTransport conn = cComm.ConnectIP(ip, port);
                //if (conn == null)
                //    conn = cComm.ConnectFile("read.log");
                if (conn == null)
                    return;
                foreach (string key in drw.Keys)
                {
                    cRWPath p = null;
                    List<cRWCommand> read_commands = null;
                    if (panel_type == "iris")
                    {
                        cRWPathIRIS pi = (cRWPathIRIS)drw[key];
                        List<cRWCommandIRIS> read_commandsi = pi.ReadCommands;
                        read_commands = new List<cRWCommand>();
                        foreach (cRWCommandIRIS c in read_commandsi)
                            read_commands.Add(c);
                        p = pi;
                    }
                    else
                    {
                        p = (cRWPath)drw[key];
                        read_commands = p.ReadCommands;
                    }
                    JObject _node = dnodes[key];
                    string read_path = p.ReadPath;
                    JObject groups = null;
                    if (_node["PROPERTIES"] != null)
                        groups = (JObject)_node["PROPERTIES"]["Groups"];
                    else
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
                                    if (!dpath_minmax.ContainsKey(read_path))
                                        dpath_minmax.Add(read_path, new Tuple<byte, byte>(min, max));
                                    else
                                        dpath_minmax[read_path] = new Tuple<byte, byte>(min, max);
                                }
                            }
                        }
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
                        else
                            lstCmdS.Add(cmd.CommandString());
                        lstCmd.Add(cmd);
                    }
                    ///////////////////////
                    if (dpath_minmax.ContainsKey(read_path))
                        dpath_log.Add(read_path, dpath_minmax[read_path]);
                    ///////////////////////
                    if (dpath_minmax.ContainsKey(read_path))
                    {
                        Tuple<byte, byte> tminmax = dpath_minmax[read_path];
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
                    foreach (string propname in p.ReadProperties.Keys)
                    {
                        //if (Regex.IsMatch(propname, "ORINPUTS", RegexOptions.IgnoreCase))
                        //{
                        //    emacexists = false;
                        //}
                        cRWProperty prop = p.ReadProperties[propname];
                        byte[] pbytes = new byte[prop.bytescnt];
                        for (int i = prop.offset; i < prop.offset + prop.bytescnt; i++)
                            pbytes[i - prop.offset] = result[i];
                        Tuple<string, string> path_val = _internal_relations_operator.GroupPropertyVal(groups, propname, pbytes);
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
                                jgrp["~invisible"][propname] = JObject.FromObject(p.ReadProperties[propname]);
                                JObject pp = (JObject)jgrp["~invisible"][propname];
                                pp["~path"] = pp.Path;
                                pp["@TYPE"] = "MISSED";
                                JObject groups1 = new JObject(jgrp);
                                path_val = _internal_relations_operator.GroupPropertyVal(groups1, propname, pbytes);
                                Match m = Regex.Match(propname, @"emacETHADDR(\d)", RegexOptions.IgnoreCase);
                                if (m.Success)
                                {
                                    emaca[Convert.ToInt32(m.Groups[1].Value)] = pbytes[0].ToString("X2");
                                    emacexists = true;
                                }
                            }
                        }
                        if (path_val != null)
                            cComm.SetPathValue(_panel_id, path_val.Item1, path_val.Item2, FilterValueChanged);
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
        }
        #endregion

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
                    //
                    //ReadDevice("192.168.17.17", 7000);
                    //
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
            string _clean_name = Regex.Replace(name, @"[\d-]", string.Empty); // added by Viktor
            _main_content_key = _clean_name; // replaced name by _clean_name by Viktor
            Monitor.Exit(_cs_main_content_key);
            Monitor.Exit(_cs_panel_templates);
            Monitor.Exit(_cs_current_panel);
            JObject _res1 = new JObject((JObject)_panel["ELEMENTS"][_clean_name]); // replaced name by _clean_name by Viktor
            RWData(_res1);
            return _res1;
        }

        #region internal relations operations
        private static cInternalRel _internal_relations_operator = null;
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
                {
                    string fname = System.IO.Path.GetFileName(files[i]);
                    if (!Regex.IsMatch(fname, @"\.xml$", RegexOptions.IgnoreCase))
                        continue;
                    if (Regex.IsMatch(fname, @"^Read[\w\W]+?" + schema + @"[_\.]", RegexOptions.IgnoreCase))
                    {
                        f = files[i];
                        break; ;
                    }
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
                {
                    string fname = System.IO.Path.GetFileName(files[i]);
                    if (!Regex.IsMatch(fname, @"\.xml$", RegexOptions.IgnoreCase))
                        continue;
                    if (Regex.IsMatch(fname, @"^Write[\w\W]+?" + schema + @"[_\.]", RegexOptions.IgnoreCase))
                    {
                        f = files[i];
                        break; ;
                    }
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
            string sj = "{}";
            if (Regex.IsMatch(prod, @"iris", RegexOptions.IgnoreCase))
            {
                _internal_relations_operator = new cInternalrelIRIS();
                _panel_type = "iris";
                sj = cIRIS.Convert(json, _pages);
            }
            else if (Regex.IsMatch(prod, @"eclipse", RegexOptions.IgnoreCase))
            {
                sj = cEclipse.Convert(json, _pages);
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
        public static void FilterValueChanged(string path, string _new_val)
        {
            bool remove_path = false;
            _internal_relations_operator.FilterValueChanged(path, _new_val, ref remove_path);
            if (remove_path)
                cComm.RemovePathValue(CurrentPanelID, path);
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
        #endregion
    }
}
