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
using System.Linq;
using System.Reflection;
using System.Xml.Linq;

namespace ljson
{
    internal class cIOChannel
    {
        private JObject _tab = null;
        private string _sidx = null;
        private eInOut _io;
        private JObject _panel = null;
        private string _panel_id = null;
        private dFindObjectByProperty FindObjectByProperty = null;
        internal cIOChannel(JObject tab, string idx, eInOut io, JObject panel, string panelID, dFindObjectByProperty _find)
        {
            _tab = tab;
            if (!Regex.IsMatch(idx, @"\.~index~"))
                idx = ".~index~" + idx;
            _sidx = idx;
            _io = io;
            _panel = panel;
            _panel_id = panelID;
            FindObjectByProperty = _find;
        }

        internal string ChannelValue
        {
            get
            {
                JObject odevaddr = FindObjectByProperty(_tab, "@ID", "P1");
                if (odevaddr == null || odevaddr["~path"] == null)
                    return null;
                string devaddr_path = odevaddr["~path"].ToString() + _sidx;
                string devaddr = cComm.GetPathValue(_panel_id, devaddr_path);
                if (devaddr == null || devaddr == "")
                    return null;
                JObject oloop = FindObjectByProperty(_tab, "@ID", "P2");
                if (oloop == null || oloop["~path"] == null)
                    return null;
                string loop_path = oloop["~path"].ToString() + _sidx;
                string loop = cComm.GetPathValue(_panel_id, loop_path);
                if (loop == null || loop == "")
                    return null;
                Dictionary<string, string> dloops = cComm.GetPseudoElementsList(_panel_id, constants.NO_LOOP);
                if (dloops == null || !dloops.ContainsKey(loop))
                    return null;
                string loop_template = dloops[loop];
                if (loop_template == null)
                    return null;
                oloop = JObject.Parse(loop_template);
                if (oloop["~loop_type"] == null)
                    return null;
                string looptype = oloop["~loop_type"].ToString();
                //IRIS_TTELOOP1
                JObject ol = (JObject)_panel["ELEMENTS"][looptype];
                JToken tel = ol["CONTAINS"]["ELEMENT"];
                if (tel["@ID"] == null)
                    return null;
                string loopsubtype = tel["@ID"].ToString();
                //IRIS_TTENONE
                Dictionary<string, string> d = cComm.GetPseudoElementDevices(_panel_id, constants.NO_LOOP, loop);
                if (!d.ContainsKey(devaddr))
                    return null;
                string sdev = d[devaddr];
                JObject odev = JObject.Parse(sdev);
                if (odev["~device"] == null)
                    return null;
                sdev = odev["~device"].ToString();
                //IRIS_MIO22
                JObject ochannel = FindObjectByProperty(_tab, "@ID", "CHANNEL");
                if (ochannel == null || ochannel["~path"] == null)
                    return null;
                string channel_path = ochannel["~path"].ToString() + _sidx;
                string channel = cComm.GetPathValue(_panel_id, channel_path);
                if (channel == null || channel == "")
                    return null;
                string chidx = ".~index~" + devaddr;/////////////
                channel = "TYPECHANNEL" + channel;
                //TYPECHANNEL1
                string newval = looptype + "/" + loopsubtype + "#" + sdev;
                odev = (JObject)_panel["ELEMENTS"][sdev]["PROPERTIES"]["Groups"];
                ochannel = (JObject)odev["~noname"]["fields"][channel];
                if (ochannel == null)
                    return null;
                channel_path = ochannel["~path"].ToString() + chidx;
                //ELEMENTS.IRIS_MIO22.PROPERTIES.Groups.~noname.fields.TYPECHANNEL1
                newval += "." + channel_path;
                return newval;
                //IRIS_TTELOOP1/IRIS_TTENONE#IRIS_MIO22.ELEMENTS.IRIS_MIO22.PROPERTIES.Groups.~noname.fields.TYPECHANNEL1.~index~0
            }
        }
    }
    internal class cInternalrelIRIS : cInternalRel
    {
        private enum eIO { Input = 1, Output = 2 };

        #region io&loops
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

                    m = Regex.Match(dev, @"""~device""\s*?:\s*?""([\w\W]+?)""");
                    if (m.Success)
                        device_name = m.Groups[1].Value;
                    if (saved_name != null)
                        device_name += "/" + saved_name;
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
                        dsmch.Add("device", path_prefix + "ELEMENTS." + device_name);
                        //if (saved_name != null && saved_name.Trim() != "")
                        //    dsmch.Add("name", saved_name);
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
                                                //if (saved_name != null && saved_name.Trim() != "" && !dchname.ContainsKey("name"))
                                                //    dchname.Add("name", saved_name);
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
        #endregion

        #region filters
        private void SetInputFilters(string _panel_id, JObject _node)
        {

        }
        private void SetOutputFilters(string _panel_id, JObject _node)
        {

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
        #endregion

        #region after changes
        public override void AfterDevicesChanged(string _panel_id, JObject panel, dGetNode _get_node)
        {
            foreach (JProperty p in panel["ELEMENTS"])
            {
                if (p.Value.Type == JTokenType.Object && Regex.IsMatch(p.Name, @"_(INPUT|OUTPUT)$", RegexOptions.IgnoreCase))
                {
                    Dictionary<string, string> cached_ios_tmp = cComm.GetElements(_panel_id, p.Name);
                    if (cached_ios_tmp != null)
                    {
                        Dictionary<string, string> cached_ios = new Dictionary<string, string>();
                        foreach (string k in cached_ios_tmp.Keys)
                            cached_ios.Add(k, cached_ios_tmp[k]);
                        foreach (string idx in cached_ios.Keys)
                        {
                            string _template = cComm.GetListElement(_panel_id, p.Name, idx);
                            JObject jo = JObject.Parse(_template);
                            SetNodeFilters(_panel_id, jo);
                            cComm.SetListElementJson(_panel_id, p.Name, idx, jo.ToString());
                        }
                    }
                    JObject jio = (JObject)p.Value;
                    if (jio["PROPERTIES"] != null && jio["PROPERTIES"]["Groups"] != null)
                        SetNodeFilters(_panel_id, (JObject)jio["PROPERTIES"]["Groups"]);
                }
            }
        }
        public override void AfterDeviceRemoved(string _panel_id, JObject panel, string loop_type, string dev_addr)
        {
            List<string> channels2del = new List<string>();
            Dictionary<string, string> newpathvals = new Dictionary<string, string>();
            Dictionary<string, string> path_values = cComm.GetPathValues(_panel_id);
            foreach (string uc in _used_channels.Keys)
            {
                if (Regex.IsMatch(uc, "^" + loop_type + @"[\w\W]+?~index~" + dev_addr, RegexOptions.IgnoreCase))
                {
                    channels2del.Add(uc);
                    Dictionary<string, string> ios = _used_channels[uc];
                    foreach (string io in ios.Keys)
                    {
                        Match m = Regex.Match(io, @"^([\w\W]+?\.fields\.Type)(\.TABS\.[\w\W]+?)\.[\w\W]*?(~index~\d+)$", RegexOptions.IgnoreCase);
                        if (m.Success)
                        {
                            string type_path = m.Groups[1].Value;
                            string oldpath = m.Groups[1].Value + m.Groups[2].Value;
                            string idx = m.Groups[3].Value;
                            //
                            List<string> pv2del = new List<string>();
                            foreach (string oldkey in path_values.Keys)
                                if (Regex.IsMatch(oldkey, "^" + oldpath, RegexOptions.IgnoreCase))
                                {
                                    cComm.RemovePathValue(_panel_id, oldkey);
                                    pv2del.Add(oldkey);
                                }
                            foreach (string pathdel in pv2del)
                                path_values.Remove(pathdel);
                            //
                            JObject otabs = (JObject)panel.SelectToken(type_path + ".TABS");
                            string none_tab = null;
                            string none_val = null;
                            foreach (JProperty p in otabs.Properties())
                                if (p.Value.Type == JTokenType.Object)
                                {
                                    JObject otab = (JObject)p.Value;
                                    if (otab["@NAME"] != null && otab["@NAME"].ToString().ToUpper() == "NONE")
                                    {
                                        none_tab = p.Name;
                                        none_val = otab["@VALUE"].ToString();
                                        break;
                                    }
                                }
                            if (none_tab != null && none_val != null)
                                newpathvals.Add(type_path + "." + idx, none_val + "_" + none_tab);
                        }
                    }
                }
            }
            List<string> io2del = new List<string>();
            foreach (string chdel in channels2del)
            {
                _used_channels.Remove(chdel);
                foreach (string iokey in _io_channels.Keys)
                    if (_io_channels[iokey] == chdel)
                        io2del.Add(iokey);
            }
            foreach (string pv in newpathvals.Keys)
                cComm.SetPathValue(_panel_id, pv, newpathvals[pv]);
            foreach (string iodel in io2del)
                if (_io_channels.ContainsKey(iodel))
                    _io_channels.Remove(iodel);
        }
        private void SetPathValuesDependedOnType(string _panel_id, JObject panel, Dictionary<string, string> paths_left)
        {
            foreach (string path in paths_left.Keys)
            {
                Dictionary<string, string> dvalues = cPanelField.ValuesDependedOnType(panel, path, paths_left[path]);
                foreach (string valpath in dvalues.Keys)
                    cComm.SetPathValue(_panel_id, valpath, dvalues[valpath]);
            }
        }
        private string ChangeIncorrectPaths(string _panel_id, JObject panel, Dictionary<string, string> _old_paths, Dictionary<string, Dictionary<string, string>> _old_byio, Dictionary<string, string> tabindexes)
        {
            string res = null;
            foreach (string sio in _old_byio.Keys)
            {
                Dictionary<string, string> diovalues = _old_byio[sio];
                string tabpath = diovalues.Keys.First();
                Match m = Regex.Match(tabpath, @"^([\w\W]+?\.TABS)");
                if (m.Success)
                    tabpath = m.Groups[1].Value;
                else
                    continue;
                JToken t = panel.SelectToken(tabpath);
                if (t == null)
                    continue;
                string stab = null;
                foreach (JProperty p in t)
                {
                    if (p.Value.Type != JTokenType.Object)
                        continue;
                    JObject pval = (JObject)p.Value;
                    if (pval["@VALUE"] != null && pval["@VALUE"].ToString() == tabindexes[sio])
                    {
                        stab = p.Name;
                        break;
                    }
                }
                if (stab == null)
                    continue;
                //
                Dictionary<string, string> drepl = new Dictionary<string, string>();
                Dictionary<string, string> back = new Dictionary<string, string>();
                foreach (string opath in diovalues.Keys)
                {
                    m = Regex.Match(opath, @"^([\w\W]+?\.TABS\.)([\w\W]+?)(\.[\w\W]+)$");
                    if (!m.Success || m.Groups[2].Value == stab)
                        continue;
                    string npath = m.Groups[1].Value + stab + m.Groups[3].Value;
                    drepl.Add(opath, npath);
                    if (!back.ContainsKey(npath))
                        back.Add(npath, opath);
                }
                foreach (string opath in drepl.Keys)
                {
                    string oval = diovalues[opath];
                    string opath_idx = "";
                    //
                    m = Regex.Match(opath, @"[\w\W]+?(\.~index~\d+)$");
                    if (m.Success)
                        opath_idx = m.Groups[1].Value;
                    string new_path = Regex.Replace(drepl[opath], @"\.~index~\d+$", "");
                    new_path = Regex.Replace(new_path, stab + @"\.[\w\W]+$", stab);
                    JObject newfield = cPanelField.FieldByWrongPath(panel, Regex.Replace(opath, @"\.~index~\d+$", ""), new_path, FindObjectByProperty);
                    if (newfield != null)
                    {
                        new_path = newfield["~path"].ToString() + opath_idx;
                        diovalues.Remove(opath);
                        cComm.RemovePathValue(_panel_id, opath);
                        diovalues.Add(new_path, oval);
                        Dictionary<string, string> new_values = cPanelField.ValuesDependedOnType(panel, new_path, oval);
                        foreach (string np in new_values.Keys)
                            cComm.SetPathValue(_panel_id, np, new_values[np]);
                        if (res == null)
                        {
                            m = Regex.Match(new_path, @"^([\w\W]+?\.TABS\.[\w\W]+?)\.");
                            if (m.Success)
                                res = m.Groups[1].Value;
                        }
                    }
                    else
                    {
                        JObject grp = cPanelField.FindGroup(panel, Regex.Replace(opath, @"\.~index~\d+$", ""));
                        Dictionary<string, string> pathvals = cPanelField.GroupPathValues(grp, oval, opath_idx);
                        diovalues.Remove(opath);
                        cComm.RemovePathValue(_panel_id, opath);
                        if (pathvals != null)
                            foreach (string key in pathvals.Keys)
                            {
                                diovalues.Add(key, pathvals[key]);
                                Dictionary<string, string> new_values = cPanelField.ValuesDependedOnType(panel, key, pathvals[key]);
                                foreach (string np in new_values.Keys)
                                    cComm.SetPathValue(_panel_id, np, new_values[np]);
                            }
                    }
                }
            }
            return res;
        }
        private string GetChannelValue(string _panel_id, JObject panel, JObject tab, string sidx)
        {
            eInOut io = eInOut.Input;
            if (tab["~path"] != null)
            {
                string tabpath = tab["~path"].ToString();
                Match m = Regex.Match(tabpath, @"ELEMENTS\.([\w\W]+?)\.");
                if (m.Success && Regex.IsMatch(m.Groups[1].Value, "OUTPUT"))
                    io = eInOut.Output;
            }
            cIOChannel io_channel = new cIOChannel(tab, sidx, io, panel, _panel_id, FindObjectByProperty);
            return io_channel.ChannelValue;
        }
        private void SetIOTabsAfterRead(string _panel_id, JObject panel, Dictionary<string, string> _old_paths)
        {
            Dictionary<string, string> paths_left = new Dictionary<string, string>();
            //
            Dictionary<string, Dictionary<string, string>> _old_byio = new Dictionary<string, Dictionary<string, string>>();
            Dictionary<string, string> tabindexes = new Dictionary<string, string>();
            foreach (string path in _old_paths.Keys)
            {
                if (!Regex.IsMatch(path, @"^ELEMENTS\.[^\.]+?(INPUT|OUTPUT)\.", RegexOptions.IgnoreCase))
                {
                    paths_left.Add(path, _old_paths[path]);
                    continue;
                }
                string ioidx = "";
                Match m = Regex.Match(path, @"^(ELEMENTS\.[\w\W]+?)\.[\w\W]+?~index~(\d+)", RegexOptions.IgnoreCase);
                if (m.Success)
                {
                    string iokey = m.Groups[1].Value + m.Groups[2].Value;
                    ioidx = m.Groups[2].Value;
                    if (!_old_byio.ContainsKey(iokey))
                        _old_byio.Add(iokey, new Dictionary<string, string>());
                    Dictionary<string, string> dio = _old_byio[iokey];
                    Match mt = Regex.Match(path, @"^[\w\W]+?Groups\.(Input|Output)Type\.fields\.Type", RegexOptions.IgnoreCase);
                    if (mt.Success)
                    {
                        string typekey = mt.Value;
                        if (!Regex.IsMatch(path, @"\.TABS\.", RegexOptions.IgnoreCase))
                        {
                            string tabidx = _old_paths[path];
                            if (!tabindexes.ContainsKey(iokey))
                                tabindexes.Add(iokey, tabidx);
                        }
                        if (!Regex.IsMatch(path, @"\.TABS\."))
                            continue;
                        if (!dio.ContainsKey(typekey))
                            dio.Add(path, _old_paths[path]);
                    }
                    else
                        paths_left.Add(path, _old_paths[path]);
                }
            }
            //
            SetPathValuesDependedOnType(_panel_id, panel, paths_left);
            //
            string tab_path = ChangeIncorrectPaths(_panel_id, panel, _old_paths, _old_byio, tabindexes);
            foreach (string key in _old_byio.Keys)
            {
                Dictionary<string, string> dold = _old_byio[key];
                string sidx = "";
                string oldpath = dold.Keys.First();
                Match m = Regex.Match(oldpath, @"([\w\W]+?\.TABS\.[\w\W]+?)\.");
                if (m.Success)
                    tab_path = m.Groups[1].Value;
                m = Regex.Match(oldpath, @"(\.~index~\d+$)");
                if (m.Success)
                    sidx = m.Groups[1].Value;
                else
                    continue;
                if (tab_path != null)
                {
                    JToken jttab = panel.SelectToken(tab_path);
                    if (jttab != null && jttab.Type == JTokenType.Object)
                    {
                        JObject otab = (JObject)jttab;
                        string channel_value = GetChannelValue(_panel_id, panel, otab, sidx);
                        if (channel_value != null && channel_value != "" && otab["@NAME"] != null && Regex.IsMatch(otab["@NAME"].ToString(), @"^LOOP[\w\W]*?DEVICE$", RegexOptions.IgnoreCase))
                        {
                            string channel_key = otab["~path"].ToString() + sidx;
                            cComm.SetPathValue(_panel_id, channel_key, channel_value);
                            bool remove_value = false;
                            FilterValueChanged(channel_key, channel_value, ref remove_value);
                        }
                        else
                        {
                            //m = Regex.Match(tab_path, @"^([\w\W]+?)\.TABS\.([\w\W]+)$");
                            //if (m.Success)
                            //{
                            //    string s = m.Groups[1].Value + sidx;
                            //    string orgval = cComm.GetPathValue(_panel_id, s);
                            //    //if (Regex.IsMatch(key, "input", RegexOptions.IgnoreCase))
                            //    //    orgval += "_input_type_";
                            //    //else
                            //    //    orgval += "_output_type_";
                            //    string tab = m.Groups[2].Value;
                            //    cComm.SetPathValue(_panel_id, s, orgval + "_" + tab);
                            //}
                        }
                        m = Regex.Match(tab_path, @"^([\w\W]+?)\.TABS\.([\w\W]+)$");
                        if (m.Success)
                        {
                            string s = m.Groups[1].Value + sidx;
                            string orgval = cComm.GetPathValue(_panel_id, s);
                            string tab = m.Groups[2].Value;
                            cComm.SetPathValue(_panel_id, s, orgval + "_" + tab);
                        }
                    }
                }
            }
        }
        private void EvacZones(string _panel_id, JObject panel, dGetNode _get_node)
        {
            JObject _elements = (JObject)panel["ELEMENTS"];
            JObject _evacz_group = null;
            foreach (JProperty p in _elements.Properties())
                if (Regex.IsMatch(p.Name, @"evac[\w\W]*?group", RegexOptions.IgnoreCase))
                {
                    _evacz_group = (JObject)p.Value;
                    break;
                }
            if (_evacz_group == null)
                return;
            JObject groups = (JObject)_evacz_group["PROPERTIES"]["Groups"];
            if (groups["~invisible"] == null)
                return;
            JObject invisible = (JObject)groups["~invisible"];
            foreach (JProperty pidx in invisible.Properties())
            {
                string sidx = pidx.Name;
                JObject oidx = (JObject)pidx.Value;
                foreach (JProperty ppanel in oidx.Properties())
                {
                    JObject opanel = (JObject)ppanel.Value;
                    JObject grp = null;
                    foreach (JProperty pgrp in groups.Properties())
                        if (pgrp.Name.ToLower() == ppanel.Name.ToLower())
                        {
                            grp = (JObject)pgrp.Value;
                            break;
                        }
                    if (grp == null)
                        continue;
                    JArray aval = (JArray)opanel["~value"];
                    string sval = cPanelField.SValByArray(aval, grp);
                    byte[] bval = cPanelField.BValByArray(aval, grp/*_evacz_group*/);
                    //ushort uval = (ushort)(Convert.ToUInt16(aval[1]) * 16 + Convert.ToUInt16(aval[0]));
                    Dictionary<string, string> paths = cPanelField.GroupPathValues(grp, bval /*uval.ToString()*/, "~index~" + sidx);
                    foreach (string path in paths.Keys)
                        cComm.SetPathValue(_panel_id, path, paths[path]);
                }
            }
            groups.Remove("~invisible");
        }
        private void FilterINGroups(string _panel_id, Dictionary<string, string> path_values)
        {
            //ELEMENTS.IRIS_INPUT.PROPERTIES.Groups.Settings.fields.Group.~index~1
            //ELEMENTS.INPUT_GROUP.PROPERTIES.Groups.~noname.fields.Input_Logic.~index~17
            Dictionary<string, string> inputs = cComm.GetElements(_panel_id, "IRIS_INPUT");
            Dictionary<string, string> groups_used = new Dictionary<string, string>();
            foreach (string inkey in inputs.Keys)
            {
                JObject io = JObject.Parse(inputs[inkey]);
                string gpath = io["Settings"]["fields"]["Group"]["~path"].ToString();
                string sgrp = path_values[gpath];
                if (sgrp != null && !groups_used.ContainsKey((Convert.ToInt32(sgrp) - 1).ToString()))
                    groups_used.Add((Convert.ToInt32(sgrp) - 1).ToString(), inkey);
            }
            //if (Regex.IsMatch(inkey, @"^ELEMENTS\.IRIS_INPUT\.PROPERTIES\.Groups\.Settings\.fields\.Group"))
            //    groups_used.Add((Convert.ToInt32(inputs[inkey]) - 1).ToString(), inkey);
            foreach (string pathkey in path_values.Keys)
            {
                Match m = Regex.Match(pathkey, @"^ELEMENTS\.INPUT_GROUP\.PROPERTIES\.Groups\.~noname\.fields\.Input_Logic\.~index~(\d+)", RegexOptions.IgnoreCase);
                if (m.Success && !groups_used.ContainsKey(m.Groups[1].Value))
                    cComm.RemovePathValue(_panel_id, pathkey);
            }
            Dictionary<string, string> ingroups = cComm.GetElements(_panel_id, "INPUT_GROUP");
            List<string> grp2del = new List<string>();
            foreach (string g in ingroups.Keys)
                if (!groups_used.ContainsKey(g))
                    grp2del.Add(g);
            foreach (string del in grp2del)
                cComm.RemoveListElement(_panel_id, "INPUT_GROUP", del);
        }
        public override void AfterRead(string _panel_id, JObject panel, dGetNode _get_node, dFilterValueChanged _filter_func)
        {
            AfterDevicesChanged(_panel_id, panel, _get_node);
            Dictionary<string, string> path_values = cComm.GetPathValues(_panel_id);
            SetIOTabsAfterRead(_panel_id, panel, path_values);
            EvacZones(_panel_id, panel, _get_node);
            FilterINGroups(_panel_id, path_values);
        }
        public override void AfterInputRemoved(string _panel_id)
        {
            Dictionary<string, string> path_values = cComm.GetPathValues(_panel_id);
            FilterINGroups(_panel_id, path_values);
        }
        #endregion
        #region events
        public override void OnElementAddressChanged(string oldAddress, string elementType, string newAddress)
        {
            foreach (string chkey in _used_channels.Keys)
            {
                Dictionary<string, string> dch = _used_channels[chkey];
                Dictionary<string, string> uc_changes = new Dictionary<string, string>();
                foreach (string iokey in dch.Keys)
                {
                    if (Regex.IsMatch(iokey, @"[\w\W]+?\." + elementType + @"\.[\w\W]+?~index~" + oldAddress + "$"))
                    {
                        string newkey = Regex.Replace(iokey, "~index~" + oldAddress + "$", "~index~" + newAddress);
                        uc_changes.Add(iokey, newkey);
                    }
                }
                foreach (string delkey in uc_changes.Keys)
                {
                    dch.Remove(delkey);
                    dch.Add(uc_changes[delkey], "");
                }
            }
            Dictionary<string, Tuple<string, string>> iochanges = new Dictionary<string, Tuple<string, string>>();
            foreach (string iokey in _io_channels.Keys)
            {
                if (Regex.IsMatch(iokey, @"[\w\W]+?\." + elementType + @"\.[\w\W]+?~index~" + oldAddress + "$"))
                {
                    string newkey = Regex.Replace(iokey, "~index~" + oldAddress + "$", "~index~" + newAddress);
                    Tuple<string, string> t = new Tuple<string, string>(newkey, _io_channels[iokey]);
                    iochanges.Add(iokey, t);
                }
            }
            foreach (string delkey in iochanges.Keys)
            {
                Tuple<string, string> t = iochanges[delkey];
                _io_channels.Remove(delkey);
                _io_channels.Add(t.Item1, t.Item2);
            }
        }
        #endregion

        #region path values
        private Dictionary<string, Dictionary<string, string>> _used_channels = new Dictionary<string, Dictionary<string, string>>();
        private Dictionary<string, string> _io_channels = new Dictionary<string, string>();
        public override void ClearCache()
        {
            _used_channels.Clear();
            _io_channels.Clear();
        }
        public override void RemoveTABCache(string tab, string idx)
        {
            List<string> uc2del = new List<string>();
            foreach (string uckey in _used_channels.Keys)
            {
                Dictionary<string, string> d = _used_channels[uckey];
                List<string> ucio2del = new List<string>();
                foreach (string k in d.Keys)
                    if (Regex.IsMatch(k, @"\.TABS\." + tab + @"[\w\W]*?~index~" + idx))
                        ucio2del.Add(k);
                foreach (string k in ucio2del)
                    d.Remove(k);
                if (d.Count == 0)
                    uc2del.Add(uckey);
            }
            foreach (string ucdel in uc2del)
                _used_channels.Remove(ucdel);
            //
            List<string> io2del = new List<string>();
            foreach (string k in _io_channels.Keys)
                if (Regex.IsMatch(k, @"\.TABS\." + tab + @"[\w\W]*?~index~" + idx))
                    io2del.Add(k);
            foreach (string iodel in io2del)
                _io_channels.Remove(iodel);
        }
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
            Dictionary<string, string> ios = null;
            if (_used_channels.ContainsKey(channel_path))
                ios = _used_channels[channel_path];
            else
            {
                Match m = Regex.Match(channel_path, @"([\w\W]+?)(\.~index~\d+$)", RegexOptions.IgnoreCase);
                if (m.Success)
                {
                    string basepath = m.Groups[1].Value;
                    string idx = m.Groups[2].Value;
                    m = Regex.Match(basepath, @"\d+$");
                    if (m.Success)
                    {
                        string chidx = m.Groups[0].Value;
                        basepath = Regex.Replace(basepath, @"[^.]+$", "");
                        channel_path = basepath + "TYPECHANNEL" + chidx + idx;
                        if (_used_channels.ContainsKey(channel_path))
                            ios = _used_channels[channel_path];
                    }
                }
            }
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
            //
            return res;
        }
        private string TranslatePropertyName(string PropertyName)
        {
            string res = null;
            if (Regex.IsMatch(PropertyName, "router", RegexOptions.IgnoreCase))
                res = "Gateway";
            else if (Regex.IsMatch(PropertyName, "protocol", RegexOptions.IgnoreCase))
                res = "PRINTER";
            else if (Regex.IsMatch(PropertyName, "devicestate", RegexOptions.IgnoreCase))
                res = "DEVICE_STATE";
            else if (Regex.IsMatch(PropertyName, "panel", RegexOptions.IgnoreCase))
                res = "ReceiveMessages";
            else if (Regex.IsMatch(PropertyName, "repeater", RegexOptions.IgnoreCase))
                res = "ReceiveCommands";
            else if (Regex.IsMatch(PropertyName, "zonegroup$", RegexOptions.IgnoreCase))
                res = "ZoneGroupA";
            else if (Regex.IsMatch(PropertyName, "zonegroup2$", RegexOptions.IgnoreCase))
                res = "ZoneGroupB";
            else if (Regex.IsMatch(PropertyName, "zonegroup3$", RegexOptions.IgnoreCase))
                res = "ZoneGroupC";
            else if (Regex.IsMatch(PropertyName, "soundergroup$", RegexOptions.IgnoreCase))
                res = "SounderGroupA";
            else if (Regex.IsMatch(PropertyName, "soundergroup2$", RegexOptions.IgnoreCase))
                res = "SounderGroupB";
            else if (Regex.IsMatch(PropertyName, "soundergroup3$", RegexOptions.IgnoreCase))
                res = "SounderGroupC";
            return res;
        }
        public override string WritePropertyVal(JObject groups, string PropertyName, string _xmltag)
        {
            JObject prop = GetDeviceGroupsNode(groups.ToString(), PropertyName);
            if (prop == null)
            {
                string translated = TranslatePropertyName(PropertyName);
                if (translated != null)
                    prop = GetDeviceGroupsNode(groups.ToString(), translated);
            }
            if (prop == null)
            {
                Match m = Regex.Match(_xmltag, @"DEFAULT\s*?=\s*?""(\d+?)""");
                if (m.Success)
                    return m.Groups[1].Value;
                return null;
            }
            else
                return cPanelField.WriteValue(prop, _xmltag);
        }
        public override Tuple<string, string> GroupPropertyVal(JObject groups, string PropertyName, byte[] val, string _xmltag)
        {
            JObject prop = GetDeviceGroupsNode(groups.ToString(), PropertyName);
            if (prop == null)
            {
                string translated = TranslatePropertyName(PropertyName);
                if (translated != null)
                    prop = GetDeviceGroupsNode(groups.ToString(), translated);
            }
            if (prop == null)
                return null;
            cPanelField.BytesByXmlTag(val, _xmltag, prop);
            //
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
                    sval = (val[1] * 256 + val[0]).ToString();
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
                else if (prop["@TYPE"].ToString().ToLower() == "intlist")
                {
                    sval = "";
                    for (int i = 0; i < val.Length; i++)
                        sval += ((sval != "") ? "," : "") + val[i].ToString();
                    return new Tuple<string, string>(path, sval);
                }
                sval = val[0].ToString() + val[1].ToString();
            }
            //
            return new Tuple<string, string>(path, sval);
        }
        public override bool AddSerialDevice(string key, JObject node, byte[] val, byte address, Dictionary<string, cRWProperty> read_props)
        {
            if (val.Length == 0)
                return false;
            if (Regex.IsMatch(key, @"input[\w\W]+?group", RegexOptions.IgnoreCase))
            {
                //return true;
                if (node["PROPERTIES"] != null && node["PROPERTIES"]["Groups"] != null && node["PROPERTIES"]["Groups"]["~noname"] != null)
                {
                    JObject grp = (JObject)node["PROPERTIES"]["Groups"]["~noname"];
                    if (grp["fields"] != null && grp["fields"]["Input_Logic"] != null)
                    {
                        return true;
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
                cRWPropertyIRIS prop = (cRWPropertyIRIS)read_props["Group"];
                int group_byte = prop.offset;
                prop = (cRWPropertyIRIS)read_props["Type"];
                int type_byte = prop.offset;
                prop = (cRWPropertyIRIS)read_props["CHANNEL"];
                int channel_byte = prop.offset;
                byte btype = val[type_byte];
                if (val[group_byte] != address + 1)
                    return true;
                for (int i = 2; i < val.Length; i++)
                    if (i == group_byte)
                        continue;
                    else
                    {
                        if (btype == 0 && i == channel_byte)
                            continue;
                        if (val[i] != 0)
                            return true;
                    }
                return false;
            }
            else if (Regex.IsMatch(key, @"output$", RegexOptions.IgnoreCase))
            {
                //return true;
                if (read_props.ContainsKey("Type"))
                {
                    cRWPropertyIRIS prop = (cRWPropertyIRIS)read_props["Type"];
                    int type_byte = prop.offset;
                    return val[type_byte] != 0;
                }
                //if (node["PROPERTIES"] != null && node["PROPERTIES"]["Groups"] != null && node["PROPERTIES"]["Groups"]["Parameters"] != null)
                //{
                //    JObject grp = (JObject)node["PROPERTIES"]["Groups"]["Parameters"];
                //    if (grp["fields"] != null && grp["fields"]["Parameters"] != null)
                //    {
                //        JObject field = (JObject)grp["fields"]["Parameters"];
                //        if (field["@VALUE"] != null)
                //            return val[9] != Convert.ToByte(field["@VALUE"].ToString());
                //    }
                //}
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
            return base.AddSerialDevice(key, node, val, address, read_props);
        }
        #endregion
    }
}
