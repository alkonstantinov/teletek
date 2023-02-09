using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using lcommunicate;
using common;
using System.Windows.Input;
using System.Text.Encodings;

namespace ljson
{
    internal class cInternalrelIRIS : cInternalRel
    {
        private enum eIO { Input = 1, Output = 2 };
        private void SetInputFilters(string _panel_id, JObject _node)
        {

        }
        private void SetOutputFilters(string _panel_id, JObject _node)
        {

        }
        private Dictionary<string, Dictionary<string, Dictionary<string, string>>> LoopsInOuuts(string _panel_id, Dictionary<string, string> loops, string io, bool firstonly)
        {
            Dictionary<string, Dictionary<string, Dictionary<string, string>>> res = null;
            string loop_type = "";
            string device_name = "";
            string device_type = "";
            foreach (string key in loops.Keys)
            {
                string loop = loops[key];
                Match m = Regex.Match(loop, @"""~loop_type""\s*?:\s*?""([\w\W]+?)""");
                if (m.Success)
                    loop_type = m.Groups[1].Value;
                Dictionary<string, string> d = cComm.GetPseudoElementDevices(_panel_id, constants.NO_LOOP, key);
                foreach (string dkey in d.Keys)
                {
                    string dev = d[dkey];
                    //
                    JObject nameprop = GetDeviceGroupsNode(dev, "NAME");
                    string name_path = null;
                    if (nameprop != null && nameprop["~path"] != null)
                        name_path = nameprop["~path"].ToString();
                    string saved_name = cComm.GetPathValue(_panel_id, name_path);
                    //
                    if (saved_name == null)
                    {
                        m = Regex.Match(dev, @"""~device""\s*?:\s*?""([\w\W]+?)""");
                        if (m.Success)
                            device_name = m.Groups[1].Value;
                    }
                    else
                        device_name = saved_name;
                    //
                    m = Regex.Match(dev, @"""~device_type""\s*?:\s*?""([\w\W]+?)""");
                    if (m.Success)
                        device_type = m.Groups[1].Value;
                    //
                    string path_prefix = loop_type + "/" + device_type + "#" + device_name + ".";
                    JObject odev = JObject.Parse(dev);
                    JObject orw = new JObject((JObject)odev["~rw"]);
                    odev.Remove("~rw");
                    odev.Remove("~device");
                    odev.Remove("~device_type");
                    //
                    if (Regex.IsMatch(loop_type, "_LOOP", RegexOptions.IgnoreCase))
                    {
                        if (res == null)
                            res = new Dictionary<string, Dictionary<string, Dictionary<string, string>>>();
                        if (!res.ContainsKey(loop_type))
                            res.Add(loop_type, new Dictionary<string, Dictionary<string, string>>());
                        Dictionary<string, Dictionary<string, string>> dsm = res[loop_type];
                        if (!dsm.ContainsKey(device_name + "/" + dkey))
                            dsm.Add(device_name + "/" + dkey, new Dictionary<string, string>());
                        Dictionary<string, string> dsmch = dsm[device_name + "/" + dkey];
                        dsmch.Add("device", "ELEMENTS." + device_name);
                        if (firstonly)
                            return res;
                        continue;
                    }
                    //
                    foreach (JProperty p in (JToken)odev)
                    {
                        JObject po = (JObject)p.Value;
                        JObject fo = (JObject)po["fields"];
                        foreach (JProperty fp in (JToken)fo)
                            if (Regex.IsMatch(fp.Name, "^TYPECHANNEL", RegexOptions.IgnoreCase))
                            {
                                JObject tc = (JObject)fp.Value;
                                string path = tc["~path"].ToString();
                                string path_noidx = "";
                                string jidx = "";
                                m = Regex.Match(path, @"([\w\W]+?)(\.~index~\d+)$");
                                if (m.Success)
                                {
                                    path_noidx = m.Groups[1].Value;
                                    jidx = m.Groups[2].Value;
                                }
                                string val = cComm.GetPathValue(_panel_id, path);
                                if (val != null)
                                {
                                    string mypath = tc.Path + ".ITEMS.ITEM[" + val + "]";
                                    JToken tselected = odev.SelectToken(mypath);
                                    if (tselected != null)
                                    {
                                        JObject oselected = (JObject)tselected;
                                        string oname = oselected["@NAME"].ToString();
                                        bool ismatch = Regex.IsMatch(oname, io, RegexOptions.IgnoreCase);
                                        if (ismatch)
                                        {
                                            if (res == null)
                                                res = new Dictionary<string, Dictionary<string, Dictionary<string, string>>>();
                                            if (!res.ContainsKey(loop_type))
                                                res.Add(loop_type, new Dictionary<string, Dictionary<string, string>>());
                                            Dictionary<string, Dictionary<string, string>> ddev = res[loop_type];
                                            if (!ddev.ContainsKey(device_name + "/" + dkey))
                                                ddev.Add(device_name + "/" + dkey, new Dictionary<string, string>());
                                            m = Regex.Match(mypath, @"TYPECHANNEL(\d+?)\.");
                                            if (m.Success)
                                            {
                                                mypath = Regex.Replace(mypath, @"TYPECHANNEL[\w\W]+$", "") + "NAME_I" + m.Groups[1].Value;
                                                JToken tname = odev.SelectToken(mypath);
                                                JObject ochname = (JObject)tname;
                                                path = ochname["~path"].ToString();
                                                string chname = cComm.GetPathValue(_panel_id, path);
                                                if (chname == null)
                                                    chname = "";
                                                chname += "/TYPECHANNEL" + m.Groups[1].Value;
                                                Dictionary<string, string> dchname = ddev[device_name + "/" + dkey];
                                                if (!dchname.ContainsKey(chname))
                                                    dchname.Add(chname, path_noidx + jidx);
                                            }
                                            //
                                            if (firstonly)
                                                return res;
                                        }
                                        continue;
                                    }
                                }
                                else
                                {
                                    string mypath = tc.Path + ".ITEMS.ITEM";
                                    JArray items = (JArray)odev.SelectToken(mypath);
                                    int elnom = 0;
                                    foreach (JToken jel in items)
                                    {
                                        JObject oel = (JObject)jel;
                                        if (oel["@DEFAULT"] != null)
                                        {
                                            string def = oel["@DEFAULT"].ToString();
                                            if (Convert.ToInt32(def) == elnom)
                                            {
                                                if (oel["@NAME"] != null && !Regex.IsMatch(oel["@NAME"].ToString(), io, RegexOptions.IgnoreCase))
                                                    break;
                                                if (res == null)
                                                    res = new Dictionary<string, Dictionary<string, Dictionary<string, string>>>();
                                                if (!res.ContainsKey(loop_type))
                                                    res.Add(loop_type, new Dictionary<string, Dictionary<string, string>>());
                                                Dictionary<string, Dictionary<string, string>> ddev = res[loop_type];
                                                if (!ddev.ContainsKey(device_name + "/" + dkey))
                                                    ddev.Add(device_name + "/" + dkey, new Dictionary<string, string>());
                                                m = Regex.Match(mypath, @"TYPECHANNEL(\d+?)\.");
                                                if (m.Success)
                                                {
                                                    mypath = Regex.Replace(mypath, @"TYPECHANNEL[\w\W]+$", "") + "NAME_I" + m.Groups[1].Value;
                                                    JToken tname = odev.SelectToken(mypath);
                                                    JObject ochname = (JObject)tname;
                                                    path = ochname["~path"].ToString();
                                                    string chname = cComm.GetPathValue(_panel_id, path);
                                                    if (chname == null)
                                                        chname = "";
                                                    chname += "/TYPECHANNEL" + m.Groups[1].Value;
                                                    Dictionary<string, string> dchname = ddev[device_name + "/" + dkey];
                                                    if (!dchname.ContainsKey(chname))
                                                        dchname.Add(chname, path_noidx + jidx);
                                                }
                                                //
                                                if (firstonly)
                                                    return res;
                                            }
                                            else
                                                break;
                                        }
                                        //
                                        elnom++;
                                    }
                                }
                            }
                    }
                }
            }
            //
            return res;
        }

        private bool HaveInputs(string _panel_id, Dictionary<string, string> loops)
        {
            Dictionary<string, Dictionary<string, Dictionary<string, string>>> res = LoopsInOuuts(_panel_id, loops, "input", true);
            return res != null && res.Count > 0;
        }

        private bool HaveOutputs(string _panel_id, Dictionary<string, string> loops)
        {
            Dictionary<string, Dictionary<string, Dictionary<string, string>>> res = LoopsInOuuts(_panel_id, loops, "output", true);
            return res != null && res.Count > 0;
        }

        public override Dictionary<string, Dictionary<string, Dictionary<string, string>>> UnionInOuts(string _panel_id, string io)
        {
            Dictionary<string, Dictionary<string, Dictionary<string, string>>> dres = new Dictionary<string, Dictionary<string, Dictionary<string, string>>>();
            Dictionary<string, Dictionary<string, string>> loopsbytype = LoopsByType(_panel_id);
            foreach (string key in loopsbytype.Keys)
            {
                Dictionary<string, string> l = loopsbytype[key];
                Dictionary<string, Dictionary<string, Dictionary<string, string>>> d = LoopsInOuuts(_panel_id, l, io, false);
                foreach (string lkey in d.Keys)
                {
                    if (!dres.ContainsKey(lkey))
                        dres.Add(lkey, new Dictionary<string, Dictionary<string, string>>());
                    Dictionary<string, Dictionary<string, string>> ddev = dres[lkey];
                    foreach (string dkey in d[lkey].Keys)
                    {
                        if (!ddev.ContainsKey(dkey))
                            ddev.Add(dkey, new Dictionary<string, string>());
                        Dictionary<string, string> dch = ddev[dkey];
                        foreach (string chkey in d[lkey][dkey].Keys)
                        {
                            if (!dch.ContainsKey(chkey))
                                dch.Add(chkey, d[lkey][dkey][chkey]);
                        }
                    }
                }
            }
            return dres;
        }
        private Dictionary<string, Dictionary<string, string>> LoopsByType(string _panel_id)
        {
            Dictionary<string, Dictionary<string, string>> loopsbytype = null;
            Dictionary<string, string> loops = cComm.GetPseudoElementsList(_panel_id, constants.NO_LOOP);
            if (loops != null)
            {
                loopsbytype = new Dictionary<string, Dictionary<string, string>>();
                foreach (string key in loops.Keys)
                {
                    string j = loops[key];
                    Match m = Regex.Match(j, @"""~loop_type""\s*?:\s*?""([\w\W]+?)""");
                    if (m.Success)
                    {
                        string ltype = Regex.Replace(m.Groups[1].Value, @"\d+$", "");
                        if (!loopsbytype.ContainsKey(ltype))
                            loopsbytype.Add(ltype, new Dictionary<string, string>());
                        Dictionary<string, string> lt = loopsbytype[ltype];
                        if (!lt.ContainsKey(key))
                            lt.Add(key, loops[key]);
                    }
                }
            }
            return loopsbytype;
        }
        private void SetIOFilters(string _panel_id, JObject _node, eIO io)
        {
            //Dictionary<string, Dictionary<string, Dictionary<string, string>>> dunion = UnionInOuts(_panel_id, "input");
            //
            Dictionary<string, Dictionary<string, string>> loopsbytype = LoopsByType(_panel_id);// null;
            Dictionary<string, bool> ioexists = new Dictionary<string, bool>();
            bool someioexists = false;
            if (loopsbytype != null)
            {
                foreach (string key in loopsbytype.Keys)
                {
                    Dictionary<string, string> l = loopsbytype[key];
                    string ioloop = Regex.Replace(key, @"^[\w\W]*?_", "");
                    bool haveio = false;
                    if (io == eIO.Input)
                        haveio = HaveInputs(_panel_id, l);
                    else
                        haveio = HaveOutputs(_panel_id, l);
                    if (!ioexists.ContainsKey(ioloop))
                        ioexists.Add(ioloop, haveio);
                    someioexists |= haveio;
                }
            }
            //
            JObject looptab = FindObjectByNAMEProp((JToken)_node, "LOOP_DEVICE");
            if (looptab != null)
            {
                bool enabled = cComm.PseudoElementExists(_panel_id) && someioexists;
                looptab["~enabled"] = enabled;
                JObject sloop = FindObjectByNAMEProp((JObject)looptab, "SYSTEM SENSOR LOOP");
                if (sloop != null)
                {
                    enabled = cComm.PseudoElementExists(_panel_id, constants.NO_LOOP, "_LOOP");
                    enabled &= ioexists.ContainsKey("LOOP") && ioexists["LOOP"];
                    sloop["~enabled"] = enabled;
                }
                JObject tteloop = FindObjectByNAMEProp((JObject)looptab, "TELETEK LOOP");
                if (tteloop != null)
                {
                    enabled = cComm.PseudoElementExists(_panel_id, constants.NO_LOOP, "_TTELOOP");
                    enabled &= ioexists.ContainsKey("TTELOOP") && ioexists["TTELOOP"];
                    tteloop["~enabled"] = enabled;
                }
            }
        }

        internal override void SetNodeFilters(string _panel_id, JObject _node)
        {
            string path = _node["~path"].ToString();
            if (Regex.IsMatch(path, @"INPUT", RegexOptions.IgnoreCase))
                SetIOFilters(_panel_id, _node, eIO.Input);
            else if (Regex.IsMatch(path, @"OUTPUT", RegexOptions.IgnoreCase))
                SetIOFilters(_panel_id, _node, eIO.Output);
        }

        #region path values
        private Dictionary<string, Dictionary<string, string>> _used_channels = new Dictionary<string, Dictionary<string, string>>();
        private Dictionary<string, string> _io_channels = new Dictionary<string, string>();
        public override void FilterValueChanged(string path, string _new_val, ref bool remove_value)
        {
            Match m = Regex.Match(_new_val, @"([0-9a-fA-f]{8}\-[0-9a-fA-F]{4}\-[0-9a-fA-F]{4}\-[0-9a-fA-F]{4}\-[0-9a-fA-F]{12})$");
            if (m.Success)
            {
                string tab = m.Groups[1].Value;
                remove_value = true;
            }
            else if (Regex.IsMatch(_new_val, @"^[\w\W]+?/[\w\W]+?#[\w\W]+?\.ELEMENTS", RegexOptions.IgnoreCase))
            {
                if (!_io_channels.ContainsKey(path))
                    _io_channels.Add(path, _new_val);
                else if (_io_channels[path] != _new_val)
                {
                    string _old_val = _io_channels[path];
                    _io_channels[path] = _new_val;
                    if (_used_channels.ContainsKey(_old_val))
                    {
                        Dictionary<string, string> dch = _used_channels[_old_val];
                        if (dch.ContainsKey(path))
                            dch.Remove(path);
                        if (_used_channels[_old_val].Count == 0)
                            _used_channels.Remove(_old_val);
                    }
                }
                //
                if (!_used_channels.ContainsKey(_new_val))
                    _used_channels.Add(_new_val, new Dictionary<string, string>());
                Dictionary<string, string> dpath = _used_channels[_new_val];
                if (!dpath.ContainsKey(path))
                    dpath.Add(path, "");
            }
        }
        public override List<string> ChannelUsedIn(string channel_path, string myIOPath)
        {
            List<string> res = null;
            if (_used_channels.ContainsKey(channel_path))
            {
                Dictionary<string, string> ios = _used_channels[channel_path];
                if (ios != null && ios.Count > 0)
                {
                    res = new List<string>();
                    foreach (string io in ios.Keys)
                        if (io != myIOPath)
                        {
                            Match m = Regex.Match(io, @"^ELEMENTS\.([\w\W]+?)\.[\w\W]+?~index~(\d+)$", RegexOptions.IgnoreCase);
                            if (m.Success)
                                res.Add(m.Groups[1].Value + m.Groups[2].Value);
                            else
                                res.Add(io);
                        }
                }
            }
            //
            return res;
        }
        public override List<string> ChannelUsedIn(string channel_path)
        {
            List<string> res = null;
            if (_used_channels.ContainsKey(channel_path))
            {
                Dictionary<string, string> ios = _used_channels[channel_path];
                if (ios != null && ios.Count > 0)
                {
                    res = new List<string>();
                    foreach (string io in ios.Keys)
                    {
                        Match m = Regex.Match(io, @"^ELEMENTS\.([\w\W]+?)\.[\w\W]+?~index~(\d+)$", RegexOptions.IgnoreCase);
                        if (m.Success)
                            res.Add(m.Groups[1].Value + m.Groups[2].Value);
                        else
                            res.Add(io);
                    }
                }
            }
            //
            return res;
        }
        public override Tuple<string, string> GroupPropertyVal(JObject groups, string PropertyName, byte[] val)
        {
            JObject prop = GetDeviceGroupsNode(groups.ToString(), PropertyName);
            if (prop == null)
            {
                if (Regex.IsMatch(PropertyName, "router", RegexOptions.IgnoreCase))
                    prop = GetDeviceGroupsNode(groups.ToString(), "Gateway");
                else if (Regex.IsMatch(PropertyName, "protocol", RegexOptions.IgnoreCase))
                    prop = GetDeviceGroupsNode(groups.ToString(), "PRINTER");
                else if (Regex.IsMatch(PropertyName, "devicestate", RegexOptions.IgnoreCase))
                    prop = GetDeviceGroupsNode(groups.ToString(), "DEVICE_STATE");
                else if (Regex.IsMatch(PropertyName, "panel", RegexOptions.IgnoreCase))
                    prop = GetDeviceGroupsNode(groups.ToString(), "ReceiveMessages");
                else if (Regex.IsMatch(PropertyName, "repeater", RegexOptions.IgnoreCase))
                    prop = GetDeviceGroupsNode(groups.ToString(), "ReceiveCommands");
                else if (Regex.IsMatch(PropertyName, "zonegroup$", RegexOptions.IgnoreCase))
                    prop = GetDeviceGroupsNode(groups.ToString(), "ZoneGroupA");
                else if (Regex.IsMatch(PropertyName, "zonegroup2$", RegexOptions.IgnoreCase))
                    prop = GetDeviceGroupsNode(groups.ToString(), "ZoneGroupB");
                else if (Regex.IsMatch(PropertyName, "zonegroup3$", RegexOptions.IgnoreCase))
                    prop = GetDeviceGroupsNode(groups.ToString(), "ZoneGroupC");
                else if (Regex.IsMatch(PropertyName, "soundergroup$", RegexOptions.IgnoreCase))
                    prop = GetDeviceGroupsNode(groups.ToString(), "SounderGroupA");
                else if (Regex.IsMatch(PropertyName, "soundergroup2$", RegexOptions.IgnoreCase))
                    prop = GetDeviceGroupsNode(groups.ToString(), "SounderGroupB");
                else if (Regex.IsMatch(PropertyName, "soundergroup3$", RegexOptions.IgnoreCase))
                    prop = GetDeviceGroupsNode(groups.ToString(), "SounderGroupC");
                if (prop == null)
                    return null;
            }
            string path = prop["~path"].ToString();
            string sval = null;
            if (val.Length == 1)
                sval = val[0].ToString();
            else
            {
                if (prop["@TYPE"].ToString().ToLower() == "week")
                {
                    sval = GetWeekValue(val);
                    return new Tuple<string, string>(path, sval);
                }
                else if (prop["@TYPE"].ToString().ToLower() == "text")
                {
                    sval = Encoding.Unicode.GetString(val).TrimEnd((Char)0);
                    return new Tuple<string, string>(path, sval);
                }
                else if (prop["@TYPE"].ToString().ToLower() == "int" && val.Length == 2)
                {
                    sval = (val[1] * 16 + val[0]).ToString();
                    return new Tuple<string, string>(path, sval);
                }
                else if (val.Length == 4 && prop["@TYPE"].ToString().ToLower() == "ip")
                {
                    sval = val[0].ToString() + "." + val[1].ToString() + "." + val[2].ToString() + "." + val[3].ToString();
                    return new Tuple<string, string>(path, sval);
                }
                else if (val.Length == 2 && prop["@VALUE"] != null && !Regex.IsMatch(prop["@VALUE"].ToString(), @"\D"))
                {
                    sval = (val[1] * 16 + val[0]).ToString();
                    return new Tuple<string, string>(path, sval);
                }
                sval = val[0].ToString() + val[1].ToString();
            }
            //
            return new Tuple<string, string>(path, sval);
        }
        public override bool AddSerialDevice(string key, JObject node, byte[] val, byte address)
        {
            if (Regex.IsMatch(key, @"input[\w\W]+?group", RegexOptions.IgnoreCase))
            {
                if (node["PROPERTIES"] != null && node["PROPERTIES"]["Groups"] != null && node["PROPERTIES"]["Groups"]["~noname"] != null)
                {
                    JObject grp = (JObject)node["PROPERTIES"]["Groups"]["~noname"];
                    if (grp["fields"] != null && grp["fields"]["Input_Logic"] != null)
                    {
                        JObject field = (JObject)grp["fields"]["Input_Logic"];
                        JArray item = (JArray)field["ITEMS"]["ITEM"];
                        int defaultidx = -1;
                        foreach (JObject o in item)
                            if (o["@DEFAULT"] != null)
                            {
                                defaultidx = Convert.ToInt32(o["@DEFAULT"].ToString());
                                break;
                            }
                        if (defaultidx >= 0)
                        {
                            JObject odef = (JObject)item[defaultidx];
                            if (odef["@VALUE"] != null)
                                return Convert.ToByte(odef["@VALUE"].ToString()) != val[2];
                        }
                    }
                }
            }
            else if (Regex.IsMatch(key, @"input$", RegexOptions.IgnoreCase))
            {
                if (val[12] != address + 1)
                    return true;
                for (int i = 2; i < val.Length; i++)
                    if (i == 12)
                        continue;
                    else
                    {
                        if (val[i] != 0)
                            return true;
                    }
                return false;
            }
            else if (Regex.IsMatch(key, @"output$", RegexOptions.IgnoreCase))
            {
                if (node["PROPERTIES"] != null && node["PROPERTIES"]["Groups"] != null && node["PROPERTIES"]["Groups"]["Parameters"] != null)
                {
                    JObject grp = (JObject)node["PROPERTIES"]["Groups"]["Parameters"];
                    if (grp["fields"] != null && grp["fields"]["Parameters"] != null)
                    {
                        JObject field = (JObject)grp["fields"]["Parameters"];
                        if (field["@VALUE"] != null)
                            return val[9] != Convert.ToByte(field["@VALUE"].ToString());
                    }
                }
            }
            else if (Regex.IsMatch(key, @"panel", RegexOptions.IgnoreCase))
            {
                for (int i = 2; i <= 5; i++)
                    if (val[i] != 0)
                        return true;
                for (int i = 86; i < val.Length; i++)
                    if (val[i] != 0)
                        return true;
                return false;
            }
            else if (Regex.IsMatch(key, @"zone$", RegexOptions.IgnoreCase))
            {
                if (val[2] != 60 || val[4] != 60)
                    return true;
                for (int i = 5; i < val.Length; i++)
                    if (val[i] != 0)
                        return true;
                return false;
            }
            else if (Regex.IsMatch(key, @"evac[\w\W]+?zone[\w\W]+?group$", RegexOptions.IgnoreCase))
            {
                for (int i = 2; i < val.Length; i++)
                    if (val[i] != 0)
                        return true;
                return false;
            }
            return base.AddSerialDevice(key, node, val, address);
        }
        #endregion
    }
}
