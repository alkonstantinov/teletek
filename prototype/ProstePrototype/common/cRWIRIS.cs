using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Linq;
using System.ComponentModel.DataAnnotations;

namespace common
{
    public class cRWCommandIRIS : cRWCommand
    {
        public override string ToString()
        {
            if (sio == null || sio == "")
            {
                string sio = "0000";
                if (io == eIO.ioRead)
                    sio = "0351";
                else if (io == eIO.ioWrite)
                    sio = "0072";
            }
            return sio + sDataType + sindex + sbuffoffset + slen;
        }

        public override int CommandLength() { return 12; }
    }
    public class cRWPropertyIRIS : cRWProperty
    {
        private string ReadPropertyValue(string PropertyName)
        {
            string quote = xml_tag_quote();
            Match m = Regex.Match(xmltag, @"\s" + PropertyName + @"\s*?=\s*?" + quote + @"([\w\W]+?)" + quote);
            if (m.Success)
                return m.Groups[1].Value;
            return null;
        }

        private string WritePropertyValue(string PropertyName)
        {
            Match m = Regex.Match(xmltag, @"\s" + PropertyName + @"\s*?=\s*?""([\w\W]+?)""");
            if (m.Success)
                return m.Groups[1].Value;
            return null;
        }
        public override string PropertyValue(eIO rw, string PropertyName)
        {
            if (rw == eIO.ioRead)
                return ReadPropertyValue(PropertyName);
            else if (rw == eIO.ioWrite)
                return WritePropertyValue(PropertyName);
            return null;
        }
    }
    internal class cRWIRIS : cRW
    {
        #region command analysis
        internal override string CommandIO(string _cmd)
        {
            return _cmd.Substring(0, 4);
        }
        internal override string CommandDataType(string _cmd)
        {
            return (_cmd.Length >= 6) ? _cmd.Substring(4, 2) : null;
        }
        internal override string CommandKey(string _cmd)
        {
            return CommandIO(_cmd) + CommandDataType(_cmd);
        }
        internal override string CommandIndexS(string _cmd)
        {
            return (_cmd.Length >= 8) ? _cmd.Substring(6, 2) : null;
        }
        internal override byte CommandIndexB(string _cmd)
        {
            return Convert.ToByte(CommandIndexS(_cmd), 16);
        }
        internal override string CommandBytesOffset(string _cmd)
        {
            return (_cmd.Length >= 10) ? _cmd.Substring(8, 2) : null;
        }
        internal override string CommandBytesCnt(string _cmd)
        {
            return (_cmd.Length >= 12) ? _cmd.Substring(10, 2) : null;
        }
        internal override cRWCommand CommandObject(string _cmd)
        {
            cRWCommandIRIS res = new cRWCommandIRIS();
            res.sio = CommandIO(_cmd);
            res.sDataType = CommandDataType(_cmd);
            res.sindex = CommandIndexS(_cmd);
            res.sbuffoffset = CommandBytesOffset(_cmd);
            res.slen = CommandBytesCnt(_cmd);
            //
            if (res.sio == "0351" || res.sio == "0352" || res.sio == "0353")
                res.io = eIO.ioRead;
            else if (res.sio == "0072")
                res.io = eIO.ioWrite;
            else
                res.io = eIO.ioNull;
            if (res.sDataType != null && res.sDataType != "")
                res.DataType = Convert.ToByte(res.sDataType, 16);
            if (res.sindex != null && res.sindex != "")
                res.index = Convert.ToByte(res.sindex, 16);
            if (res.sbuffoffset != null && res.sbuffoffset != "")
                res.buffoffset = Convert.ToByte(res.sbuffoffset, 16);
            if (res.slen != null && res.slen != "")
                res.len = Convert.ToByte(res.slen, 16);
            return res;
        }
        #endregion

        #region command synthesis
        internal override string SynCMD(string io, string datatype, string idx, string bytesoffset, string bytescnt)
        {
            return "00" + io + datatype + idx + bytesoffset + bytescnt;
        }

        internal override string SynCMD(cRWCommand _cmd)
        {
            string io = _cmd.sio;
            if (io == null)
            {
                if (_cmd.io == eIO.ioRead)
                    io = "51";
                else if (_cmd.io == eIO.ioWrite)
                    io = "72";
            }
            string datatype = _cmd.sDataType;
            if (datatype == null)
                datatype = (_cmd.DataType != null) ? ((byte)_cmd.DataType).ToString("X2") : null;
            string idx = _cmd.sindex;
            if (idx == null)
                idx = (_cmd.index != null) ? ((byte)_cmd.index).ToString("X2") : null;
            string bytesoffset = _cmd.sbuffoffset;
            if (bytesoffset == null)
                bytesoffset = (_cmd.buffoffset != null) ? ((byte)_cmd.buffoffset).ToString("X2") : null;
            string bytescnt = _cmd.slen;
            if (bytescnt == null)
                bytescnt = (_cmd.len != null) ? ((byte)_cmd.len).ToString("X2") : null;
            return SynCMD(io, datatype, idx, bytesoffset, bytescnt);
        }
        #endregion

        internal override void ParseReadSeriaIRIS(string elements, ref string last_path)
        {
            string element_root = "";
            Match m = Regex.Match(elements, @"^\s*?<ELEMENT[\w\W]+?>");
            if (m.Success)
            {
                m = Regex.Match(m.Value, @"ID\s*?=\s*?""([\w\W]*?)""");
                if (m.Success)
                    element_root = m.Groups[1].Value;
            }
            //
            string path = "";
            Match mstart = Regex.Match(elements, @"<COMMANDS");
            if (mstart.Success)
            {
                string start = elements.Substring(0, mstart.Index);
                foreach (Match mp in Regex.Matches(start, @"<ELEMENT\sID\s*?=\s*?""([\w\W]*?)"""))
                    if (mp.Groups[1].Value != "")
                        path += mp.Groups[1].Value + "/";
                if (path != "")
                    path = path.Substring(0, path.Length - 1);
            }
            if (Regex.IsMatch(path, @"(IRIS8_SENSORS|IRIS8_MODULES)"))
            {
                string[] path_arr = Regex.Split(path, @"[\\/]");
                string[] last_path_arr = Regex.Split(last_path, @"[\\/]");
                if (last_path_arr == null)
                {
                    last_path = path;
                }
                else
                {
                    if (last_path_arr.Length > path_arr.Length)
                    {
                        List<string> lpath = new List<string>();
                        for (int i = 0; i < last_path_arr.Length - path_arr.Length; i++)
                            lpath.Add(last_path_arr[i]);
                        for (int i = 0; i < path_arr.Length; i++)
                            lpath.Add(path_arr[i]);
                        path = String.Join('/', lpath.ToArray());
                        last_path = path;
                    }
                    else if (last_path_arr.Length < path_arr.Length)
                        last_path = path;
                    //else
                    //    last_path = "";
                }
            }
            else if ((last_path == null || last_path == "") && Regex.IsMatch(path, @"(IRIS_MODULES|IRIS_MNONE)") && !Regex.IsMatch(path, @"\d"))
            {
                string lastkey = _dread_prop.Keys.Last();
                path = Regex.Replace(lastkey, @"(\d/)[\w\W]+$", "$1") + path;
                //IRIS_LOOP2/IRIS_SENSORS/IRIS_SNONE
            }
            //
            elements = Regex.Replace(elements, @"^\s*?<ELEMENT[\w\W]*?>\s*?<ELEMENTS>", "");
            elements = Regex.Replace(elements, @"</ELEMENTS>\s*?</ELEMENT>\s*$", "").Trim();
            foreach (Match mel in Regex.Matches(elements, @"<ELEMENT[\w\W]*?</ELEMENT>"))
            {
                string el = mel.Value;
                if (Regex.IsMatch(el, "\"IRIS_MNONE\""))
                {
                    el = el;
                }
                m = Regex.Match(el, @"ID\s*?=\s*?""([\w\W]+?)""");
                if (m.Success)
                {
                    //string element = ((element_root != "") ? element_root + "/" : "") + m.Groups[1].Value;
                    string element = path;
                    if (!Regex.IsMatch(path, "/" + m.Groups[1].Value + "/"))
                    {
                        if (Regex.IsMatch(path, @"IRIS_SENSORS") && Regex.IsMatch(m.Groups[1].Value, "(IRIS_MODULES|IRIS_MNONE)"))
                        {
                            element = Regex.Replace(path, @"IRIS_SENSORS[\w\W]+$", "IRIS_MODULES/IRIS_MNONE");
                        }
                        else
                        {
                            string[] sarr = path.Split('/');
                            sarr[sarr.Length - 1] = m.Groups[1].Value;
                            element = String.Join('/', sarr);
                        }
                    }
                    //string element = path;
                    m = Regex.Match(el, @"<COMMANDS>([\w\W]*?)</COMMANDS>");
                    if (m.Success)
                    {
                        foreach (Match mc in Regex.Matches(m.Groups[1].Value, @"<COMMAND\s+?BYTES\s*?=\s*?""([\w\W]+?)""[\w\W]*?/>"))
                        {
                            string cmdkey = CommandKey(mc.Groups[1].Value);
                            string cmdidxs = CommandIndexS(mc.Groups[1].Value);
                            if (cmdkey == null || cmdidxs == null)
                                continue;
                            if (!_dread_prop.ContainsKey(element))
                                _dread_prop.Add(element, new Dictionary<string, Dictionary<string, List<string>>>());
                            Dictionary<string, Dictionary<string, List<string>>> dcmd = _dread_prop[element];
                            if (!dcmd.ContainsKey(cmdkey))
                                dcmd.Add(cmdkey, new Dictionary<string, List<string>>());
                            Dictionary<string, List<string>> didx = dcmd[cmdkey];
                            if (!didx.ContainsKey(cmdidxs))
                                didx.Add(cmdidxs, new List<string>());
                            didx[cmdidxs].Add(el);
                        }
                    }
                }
            }
        }

        internal override List<cSeria> cfgReadSeries(string xml)
        {
            if (_dread_prop == null)
                _dread_prop = new Dictionary<string, Dictionary<string, Dictionary<string, List<string>>>>();
            else
                _dread_prop.Clear();
            //
            string last_path = "";
            List<cSeria> res = new List<cSeria>();
            xml = Regex.Replace(xml, @"<!\-+?[\w\W]*?\-+?>", "");
            MatchCollection matches = Regex.Matches(xml, @"(<ELEMENT[^<]*?>\s*?<ELEMENTS[\w\W]*?<ELEMENT[\w\W]*?</ELEMENT>\s*?</ELEMENTS>\s*?</ELEMENT>)", RegexOptions.IgnoreCase);
            foreach (Match m in matches)
                ParseReadSeriaIRIS(m.Groups[1].Value, ref last_path);
            //
            if (matches != null && matches.Count > 0)
            {
                int istart = matches[0].Index;
                int iend = matches[matches.Count - 1].Index + matches[matches.Count - 1].Length;
                _read_xml_no_serias = xml.Substring(0, istart) + "\r\n" + xml.Substring(iend, xml.Length - iend);
                last_path = "";
                foreach (Match m in Regex.Matches(_read_xml_no_serias, @"<ELEMENT\s+?ID[\w\W]+?(</ELEMENT>|<ELEMENTS>)"))
                {
                    string s = Regex.Replace(m.Value, @"<ELEMENTS>\s*$", "</ELEMENT>");
                    s = "<ELEMENT ID=\"\">\r\n<ELEMENTS>\r\n" + s + "\r\n</ELEMENTS>\r\n</ELEMENT>";
                    ParseReadSeriaIRIS(s, ref last_path);
                }
            }
            //
            return res;
        }

        internal override void ParseWriteProperties(string write_operation)
        {
            if (_write_property_groups == null)
                _write_property_groups = new Dictionary<string, Dictionary<string, List<cSeriaProperty>>>();
            Match m = Regex.Match(write_operation, @"<BYTE\W[\w\W]*?PATH\s*?=\s*?""([\w\W]+?)""[\w\W]*?/>");
            if (m.Success)
            {
                string path = m.Groups[1].Value;
                m = Regex.Match(path, @"([\w\W]*?)PROPERTIES/PROPERTY([\w\W]+$)");
                if (m.Success)
                {
                    string key = Regex.Replace(m.Groups[1].Value, @"/$", "");
                    if (key != "")
                    {
                        m = Regex.Match(key, @"\[\s*?@ID\s*?=\s*?\W(\w+?)\W[\w\W]*?\]$");
                        if (m.Success)
                            key = m.Groups[1].Value;
                    }
                    m = Regex.Match(write_operation, @"<BYTE\W[\w\W]*?VALUE\s*?=\s*?""([\w\W]+?)""[\w\W]*?/>");
                    if (m.Success)
                    {
                        string cmd = m.Groups[1].Value;
                        List<cSeriaProperty> lstp = new List<cSeriaProperty>();
                        foreach (Match mp in Regex.Matches(write_operation, @"<BYTE[\w\W]+?/>"))
                        {
                            string sb = mp.Value;
                            m = Regex.Match(sb, @"\WXPATH\s*?=\s*?""([\w\W]+?)""");
                            if (m.Success)
                            {
                                m = Regex.Match(m.Groups[1].Value, @"PROPERTY\s*?\[\s*?@ID\s*?=\s*?\W(\w+?)\W", RegexOptions.IgnoreCase);
                                if (m.Success)
                                {
                                    cSeriaProperty sp = new cSeriaProperty();
                                    sp.id = m.Groups[1].Value;
                                    m = Regex.Match(sb, @"\WLENGTH\s*?=\s*?""(\d+?)""");
                                    if (m.Success)
                                        sp.len = Convert.ToInt32(m.Groups[1].Value);
                                    lstp.Add(sp);
                                }
                            }
                        }
                        if (!_write_property_groups.ContainsKey(key))
                            _write_property_groups.Add(key, new Dictionary<string, List<cSeriaProperty>>());
                        Dictionary<string, List<cSeriaProperty>> cmdict = _write_property_groups[key];
                        if (!cmdict.ContainsKey(cmd))
                            cmdict.Add(cmd, lstp);
                        else
                            cmdict[cmd] = lstp;
                    }
                }
            }
        }

        private string write_element_from_path(string wo)
        {
            Match m = Regex.Match(wo, @"<BYTE\s+?XPATH\s*?=\s*?([\w\W]+?)""");
            if (m.Success)
            {
                string path = m.Groups[1].Value;
                m = Regex.Match(path, @"^([\w\W]+?)/PROPERTIES");
                if (m.Success)
                {
                    string start = Regex.Replace(m.Groups[1].Value, @"\[\d+\]$", "");
                    string res = "";
                    foreach (Match match in Regex.Matches(start, @"\[[\w\W]+?\]"))
                    {
                        m = Regex.Match(match.Value, @"@ID\s*?=\s*?[""']([\w\W]+?)[""']");
                        if (m.Success)
                            res += ((res != "") ? "/" : "") + m.Groups[1].Value; ;
                    }
                    return res;
                    //MatchCollection matches = Regex.Matches(start, @"\[[\w\W]+?\]");
                    //if (matches.Count > 0)
                    //{
                    //    m = matches[matches.Count - 1];
                    //    m = Regex.Match(m.Value, @"@ID\s*?=\s*?[""']([\w\W]+?)[""']");
                    //    if (m.Success)
                    //        return m.Groups[1].Value;
                    //    else
                    //        return "";
                    //}
                    //else
                    //    return "";
                }
                else
                    return "";
            }
            return null;
        }

        internal override List<cSeria> cfgWriteSerias(string xml)
        {
            List<cSeria> res = new List<cSeria>();
            Dictionary<string, List<string>> dserias = new Dictionary<string, List<string>>();
            Dictionary<string, Dictionary<string, Dictionary<string, List<string>>>> dcmdop = new Dictionary<string, Dictionary<string, Dictionary<string, List<string>>>>();
            if (_dwrite_prop == null)
                _dwrite_prop = new Dictionary<string, Dictionary<string, Dictionary<string, List<string>>>>();
            else
                _dwrite_prop.Clear();
            string inputscmd = null;
            string outputscmd = null;
            int inidx = 1;
            int outidx = 1;
            foreach (Match mwo in Regex.Matches(xml, @"<WRITEOPERATION[\w\W]+?</WRITEOPERATION[\w\W]*?>", RegexOptions.IgnoreCase))
            {
                string swo = mwo.Value;
                //
                Match mcmd = Regex.Match(swo, @"<BYTE\s+?VALUE\s*?=\s*?""([\w\W]+?)""");
                if (mcmd.Success)
                {
                    string cmdkey = CommandKey(mcmd.Groups[1].Value);
                    string cmdidxs = CommandIndexS(mcmd.Groups[1].Value);
                    string element = write_element_from_path(swo);
                    if (element == null)
                        continue;
                    if (inputscmd == null)
                    {
                        if (Regex.IsMatch(element, @"INPUTS$"))
                            inputscmd = cmdkey;
                    }
                    if (outputscmd == null)
                        if (Regex.IsMatch(element, @"OUTPUTS$"))
                            outputscmd = cmdkey;
                    //
                    if (cmdkey == inputscmd && !Regex.IsMatch(element, @"INPUTS$"))
                    {
                        element += "_IN" + inidx.ToString();
                        inidx++;
                    }
                    if (cmdkey == outputscmd && !Regex.IsMatch(element, @"OUTPUTS$"))
                    {
                        element += "_OUT" + outidx.ToString();
                        outidx++;
                    }
                    //if (!dcmdop.ContainsKey(cmdkey))
                    //    dcmdop.Add(cmdkey, new Dictionary<string, Dictionary<string, List<string>>>());
                    //Dictionary<string, Dictionary<string, List<string>>> delement = dcmdop[cmdkey];
                    //if (!delement.ContainsKey(element))
                    //    delement.Add(element, new Dictionary<string, List<string>>());
                    //Dictionary<string, List<string>> dcmdidx = delement[element];
                    //if (!dcmdidx.ContainsKey(cmdidxs))
                    //    dcmdidx.Add(cmdidxs, new List<string>());
                    //dcmdidx[cmdidxs].Add(Regex.Replace(swo, @"<!\-\-[\w\W]*?\-\->", ""));
                    //
                    if (!_dwrite_prop.ContainsKey(element))
                        _dwrite_prop.Add(element, new Dictionary<string, Dictionary<string, List<string>>>());
                    Dictionary<string, Dictionary<string, List<string>>> dpropcmdkey = _dwrite_prop[element];
                    if (!dpropcmdkey.ContainsKey(cmdkey))
                        dpropcmdkey.Add(cmdkey, new Dictionary<string, List<string>>());
                    Dictionary<string, List<string>> dcmdidx = dpropcmdkey[cmdkey];
                    if (!dcmdidx.ContainsKey(cmdidxs))
                        dcmdidx.Add(cmdidxs, new List<string>());
                    dcmdidx[cmdidxs].Add(Regex.Replace(swo, @"<!\-\-[\w\W]*?\-\->", ""));
                }
                //
                if (!Regex.IsMatch(swo, @"\[\d+\]/PROPERTIES/PROPERTY", RegexOptions.IgnoreCase))
                {
                    ParseWriteProperties(swo);
                    continue;
                }
                Match m = Regex.Match(swo, @"<BYTE\W[\w\W]*?XPATH\s*?=\s*?""([\w\W]+?)\[\d+\]/PROPERTIES/PROPERTY");
                if (m.Success)
                {
                    string key = m.Groups[1].Value;
                    if (!dserias.ContainsKey(key))
                        dserias.Add(key, new List<string>());
                    dserias[key].Add(swo);
                }
            }
            foreach (string key in dserias.Keys)
            {
                List<string> lst = dserias[key];
                cSeria seria = new cSeria();
                seria.element_id = key;
                seria._commands = new List<cPanelCommand>();
                byte[] bbase = null;
                byte[] bmask = null;
                List<byte[]> lstcommands = new List<byte[]>();
                cPanelCommand _cmd = new cPanelCommand();
                //StringBuilder regexdel = new StringBuilder();
                for (int i = 0; i < lst.Count; i++)
                {
                    //regexdel.Append(@"[\w\W]*?" + lst[i]);
                    Match m = Regex.Match(lst[i], @"<BYTE\W[\w\W]*?VALUE\s*?=\s*?""([\w\W]+?)""");
                    if (!m.Success)
                        continue;
                    string scmd = m.Groups[1].Value;
                    byte[] bcmd = new byte[scmd.Length / 2];
                    for (int j = 0; j < scmd.Length / 2; j++)
                        bcmd[j] = byte.Parse(scmd.Substring(j * 2, 2), System.Globalization.NumberStyles.HexNumber);
                    if (i == 0)
                    {
                        _cmd.scmd = scmd;
                        _cmd._bcmd = new byte[bcmd.Length];
                        bcmd.CopyTo(_cmd._bcmd, 0);
                        bbase = new byte[_cmd._bcmd.Length];
                        _cmd._bcmd.CopyTo(bbase, 0);
                        bmask = new byte[_cmd.bcmd.Length];
                        for (int j = 0; j < bmask.Length; j++)
                            bmask[j] = 0;
                    }
                    byte[] bxor = new byte[_cmd.bcmd.Length];
                    for (int j = 0; j < bbase.Length; j++)
                    {
                        bxor[j] = (byte)(bcmd[j] ^ bbase[j]);
                        bmask[j] = (byte)(bxor[j] | bmask[j]);
                    }
                    lstcommands.Add(bcmd);
                }
                //
                //xml = Regex.Replace(xml, regexdel.ToString(), "");
                //regexdel.Clear();
                //
                int prefcnt = 0;
                byte idxcnt = 0;
                int sufcnt = 0;
                bool isidx = false;
                for (int i = 0; i < bmask.Length; i++)
                    if (bmask[i] == 0)
                        if (!isidx)
                            prefcnt++;
                        else
                            sufcnt++;
                    else
                    {
                        idxcnt++;
                        isidx = true;
                    }
                _cmd._idxbytescnt = idxcnt;
                _cmd._bidxmask = new byte[bmask.Length];
                bmask.CopyTo(_cmd._bidxmask, 0);
                _cmd._idxbytes = new List<byte[]>();
                for (int i = 0; i < lstcommands.Count; i++)
                {
                    int idx = 0;
                    byte[] idxbytes = new byte[idxcnt];
                    for (int j = 0; j < bmask.Length; j++)
                        if (bmask[j] != 0)
                        {
                            idxbytes[idx] = (byte)(lstcommands[i][j] & bmask[j]);
                            idx++;
                        }
                    _cmd._idxbytes.Add(idxbytes);
                }
                _cmd._prefix = new byte[prefcnt];
                for (int i = 0; i < prefcnt; i++)
                    _cmd._prefix[i] = _cmd._bcmd[i];
                _cmd._sufix = new byte[_cmd.bcmd.Length - prefcnt - idxcnt];
                for (int i = prefcnt + idxcnt; i < _cmd.bcmd.Length; i++)
                    _cmd._sufix[i - prefcnt - idxcnt] = _cmd._bcmd[i];
                seria._commands.Add(_cmd);
                res.Add(seria);
            }
            //
            return res;
        }

        #region merge
        private string FindReadKey(string wkey)
        {
            string loop = "abcd";
            string tteloop = "abcd";
            Match m = Regex.Match(wkey, @"(\w+?_LOOP\d[\w\W]+)");
            if (m.Success)
                loop = m.Groups[1].Value;
            m = Regex.Match(wkey, @"(\w+?_TTELOOP\d)");
            if (m.Success)
                tteloop = m.Groups[1].Value;
            foreach (string key in _dread_prop.Keys)
            {
                if (key == wkey)
                    return key;
                if (wkey == "" && Regex.IsMatch(key, @"_PANEL$"))
                    return key;
                if (Regex.IsMatch(wkey, @"PUTS$"))
                {
                    string s = wkey.Substring(0, wkey.Length - 1);
                    if (Regex.IsMatch(key, s + "$"))
                        return key;
                }
                if (wkey != "" && Regex.IsMatch(key, wkey + "$"))
                    return key;
                if (wkey != "" && Regex.IsMatch(key, "^" + wkey))
                    return key;
                if (Regex.IsMatch(wkey, @"EVAC_ZONES*_GROUPS*") && Regex.IsMatch(key, @"EVAC_ZONES*_GROUPS*"))
                    return key;
                if (Regex.IsMatch(key, "^" + loop))
                    return key;
                if (Regex.IsMatch(key, loop + "$"))
                    return key;
                if (Regex.IsMatch(key, "^" + tteloop))
                    return key;
                if (Regex.IsMatch(key, tteloop + "$"))
                    return key;
                //m = Regex.Match(wkey, @"[\w\W]+?[\\/]([\w_]+)$");
                //if (m.Success && Regex.IsMatch(key, m.Groups[1].Value))
                //    return key;
            }
            //
            return null;
        }

        private Dictionary<string, string> RWMergedPaths()
        {
            if (_dread_prop == null || _dwrite_prop == null/* || _dread_prop.Count != _dwrite_prop.Count*/)
            {
                Monitor.Exit(_cs_);
                return null;
            }
            //
            Dictionary<string, string> wrkeys = new Dictionary<string, string>();
            foreach (string wkey in _dwrite_prop.Keys)
            {
                string rkey = FindReadKey(wkey);
                if (rkey == null)
                {
                    Monitor.Exit(_cs_);
                    return null;
                }
                wrkeys.Add(wkey, rkey);
            }
            //
            return wrkeys;
        }

        internal override Dictionary<string, cRWPath> RWMerged()
        {
            Dictionary<string, cRWPath> res = new Dictionary<string, cRWPath>();
            Dictionary<string, string> wrkeys = RWMergedPaths();
            foreach (string wpath in wrkeys.Keys)
            {
                cRWPath rwpath = new cRWPath();
                rwpath.WritePath = wpath;
                rwpath.ReadPath = wrkeys[wpath];
                //read
                Dictionary<string, Dictionary<string, List<string>>> dr = _dread_prop[rwpath.ReadPath];
                string firstkey = dr.Keys.First();
                Dictionary<string, List<string>> dr1 = dr[firstkey];
                firstkey = dr1.Keys.First();
                List<string> lXml = dr1[firstkey];
                string xml = lXml[0];
                Match m = Regex.Match(xml, @"<COMMANDS([\w\W]+?)</COMMANDS");
                if (m.Success)
                    foreach (Match mc in Regex.Matches(m.Groups[1].Value, @"<COMMAND\W[\w\W]*?BYTES\s*?=\s*?""(\w+?)"""))
                    {
                        string cmd = mc.Groups[1].Value;
                        cRWCommand rwc = CommandObject(cmd);
                        if (cmd.Length < rwc.CommandLength())
                            continue;
                        rwpath.ReadCommands.Add(rwc);
                    }
                m = Regex.Match(xml, @"<PROPERTIES([\w\W]+?)</PROPERTIES");
                if (m.Success)
                    foreach (Match mp in Regex.Matches(m.Groups[1].Value, @"<PROPERTY[\w\W]+?/>"))
                    {
                        cRWPropertyIRIS p = new cRWPropertyIRIS();
                        p.xmltag = mp.Value;
                        p.id = p.PropertyValue(eIO.ioRead, "ID");
                        p.offset = Convert.ToInt32(p.PropertyValue(eIO.ioRead, "BYTE"));
                        p.bytescnt = Convert.ToByte(p.PropertyValue(eIO.ioRead, "LENGTH"));
                        rwpath.ReadProperties.Add(p.id, p);
                    }
                //
                Dictionary<string, Dictionary<string, List<string>>> dw = _dwrite_prop[wpath];
                StringBuilder sbwo = new StringBuilder();
                foreach (string dwkey in dw.Keys)
                {
                    firstkey = dw[dwkey].Keys.First();
                    List<string> lst = dw[dwkey][firstkey];
                    foreach (string ln in lst)
                        foreach (Match mm in Regex.Matches(ln, @"<BYTE\s[\w\W]+?/>"))
                            sbwo.Append(mm.Value + "\r\n");
                }
                string swo = sbwo.ToString();
                Dictionary<string, string> dwcmd = new Dictionary<string, string>();
                foreach (Match mo in Regex.Matches(swo, @"<BYTE\W[\w\W]+?/>"))
                {
                    string ln = mo.Value;
                    cWriteOperation op = new cWriteOperation();
                    m = Regex.Match(ln, @"<BYTE\s+?VALUE\s*?=\s*?""(\w+?)""");
                    if (m.Success)
                    {
                        op.operation = eWriteOperation.woBytes;
                        op.value = m.Groups[1].Value;
                        if (!dwcmd.ContainsKey(op.value))
                            dwcmd.Add(op.value, "");
                    }
                    else
                    {
                        op.operation = eWriteOperation.woProperty;
                        m = Regex.Match(ln, @"<BYTE\s+?XPATH\s*?=\s*?""([\w\W]+?)""");
                        if (m.Success)
                        {
                            string path = m.Groups[1].Value;
                            m = Regex.Match(path, @"PROPERTY\s*?\[\s*?@ID\s*?=\s*?'([\w\W]+?)'");
                            if (m.Success)
                            {
                                op.value = m.Groups[1].Value;
                                cRWPropertyIRIS p = new cRWPropertyIRIS();
                                p.xmltag = mo.Value;
                                p.id = m.Groups[1].Value;
                                p.bytescnt = Convert.ToByte(p.PropertyValue(eIO.ioWrite, "LENGTH"));
                                rwpath.WriteProperties.Add(p.id, p);
                            }
                        }
                    }
                    rwpath.WriteOperationOrder.Add(op);
                }
                foreach (string cmd in dwcmd.Keys)
                {
                    try
                    {
                        cRWCommand rwc = CommandObject(cmd);
                        if (cmd.Length < rwc.CommandLength())
                            continue;
                        rwpath.WriteCommands.Add(rwc);
                    }
                    catch { }
                }
                res.Add(wpath, rwpath);
            }
            //
            return res;
        }
        #endregion
    }
}
