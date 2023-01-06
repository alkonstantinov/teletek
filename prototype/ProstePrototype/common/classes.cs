using System;
using System.Threading;
using System.IO;
using System.Text.RegularExpressions;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Windows.Documents;
using System.Text.Encodings;
using System.Xml.Linq;
using System.Text.Json.Serialization;
using System.Linq;

namespace common
{
    public delegate string dObject2JSONString(object o);
    public delegate object dJSONString2Object(string s, System.Type typ);

    public enum eCommuncationType { IP = 1, COM = 2, USB = 3 };

    public class cIPParams
    {
        public string address;
        public int port;

        public cIPParams(string _ip, int _port) { address = _ip; port = _port; }
    }

    public class cWriteField
    {
        public int ord;
        public string lngid;
        public int bytes_cnt;
        public string path;
        public string cmd;
        public byte[] bcmd;
        public string path_org;
    }

    public class cPanelCommand
    {
        internal string _scmd;
        public string scmd { get { return _scmd; } set { _scmd = value; } }

        internal byte[] _bcmd;
        public string[] bcmd
        {
            get
            {
                if (_bcmd != null)
                {
                    string[] res = new string[_bcmd.Length];
                    for (int i = 0; i < _bcmd.Length; i++)
                        res[i] = Convert.ToString(_bcmd[i], 16);
                    return res;
                    //return (_bcmd != null) ? Encoding.UTF8.GetString(_bcmd).ToCharArray() : null;
                }
                else
                    return null;
            }
            set
            {
                _bcmd = new byte[value.Length];
                for (int i = 0; i < value.Length; i++)
                    _bcmd[i] = Convert.ToByte(value[i], 16);
            }
        }
        internal byte[] _bidxmask;
        public string[] bidxmask
        {
            get
            {
                if (_bidxmask != null)
                {
                    string[] res = new string[_bidxmask.Length];
                    for (int i = 0; i < _bidxmask.Length; i++)
                        res[i] = Convert.ToString(_bidxmask[i], 16);
                    return res;
                }
                else
                    return null;
            }
            set
            {
                if (value != null)
                {
                    _bidxmask = new byte[value.Length];
                    for (int i = 0; i < value.Length; i++)
                        _bidxmask[i] = Convert.ToByte(value[i], 16);
                }
                else
                    _bidxmask = null;
            }
        }

        internal byte[] _prefix;
        public string[] prefix
        {
            get
            {
                if (_prefix != null)
                {
                    string[] res = new string[_prefix.Length];
                    for (int i = 0; i < _prefix.Length; i++)
                        res[i] = Convert.ToString(_prefix[i], 16);
                    return res;
                }
                else
                    return null;
            }
            set
            {
                if (value != null)
                {
                    _prefix = new byte[value.Length];
                    for (int i = 0; i < value.Length; i++)
                        _prefix[i] = Convert.ToByte(value[i], 16);
                }
                else _prefix = null;
            }
        }

        internal byte _idxbytescnt;
        public char idxbytescnt
        {
            get
            {
                return _idxbytescnt.ToString("X")[0];
            }
            set
            {
                _idxbytescnt = Convert.ToByte(value);
            }
        }

        internal byte[] _sufix;
        public string[] sufix
        {
            get
            {
                if (_sufix != null)
                {
                    string[] res = new string[_sufix.Length];
                    for (int i = 0; i < _sufix.Length; i++)
                        res[i] = Convert.ToString(_sufix[i], 16);
                    return res;
                }
                else
                    return null;
            }
            set
            {
                if (value != null)
                {
                    _sufix = new byte[value.Length];
                    for (int i = 0; i < value.Length; i++)
                        _sufix[i] = Convert.ToByte(value[i], 16);
                }
                else
                    _sufix = null;
            }
        }

        internal List<byte[]> _idxbytes = new List<byte[]>();
        public List<string[]> idxbytes
        {
            get
            {
                List<string[]> res = new List<string[]>();
                foreach (byte[] bytes in _idxbytes)
                {
                    string[] b = new string[bytes.Length];
                    for (int i = 0; i < b.Length; i++)
                        b[i] = Convert.ToString(bytes[i], 16);
                    res.Add(b);
                }
                return res;
            }
            set
            {
                _idxbytes = new List<byte[]>();
                if (value == null)
                {
                    _idxbytes = null;
                    return;
                }
                foreach (string[] bytes in value)
                {
                    byte[] b = new byte[bytes.Length];
                    for (int i = 0; i < bytes.Length; i++)
                        b[i] = Convert.ToByte(bytes[i], 16);
                    _idxbytes.Add(b);
                }
            }
        }
    }

    public class cSeriaProperty
    {
        public string id;
        public int byteidx;
        public int len;
    }

    public class cPropertyGroup
    {
        public string id;
        public string txt;
        internal List<cPanelCommand> _commands = new List<cPanelCommand>();
        public List<cPanelCommand> commands { get { return _commands; } set { _commands = value; } }
        public Dictionary<string, cSeriaProperty> _properties;
    }
    public class cSeria
    {
        public string _txt = "";
        public string _txt_noelements = "";
        public string _element = "";
        public List<cPanelCommand> _commands = new List<cPanelCommand>();
        public string element_id = "";
        public Dictionary<string, cSeriaProperty> properties = new Dictionary<string, cSeriaProperty>();
    }

    public enum ePanelType { ptIRIS = 1, ptEclipse = 2 };

    /// <summary>
    /// Съдържа XML-конфигурации(template, read, write)
    /// </summary>
    public class cXmlConfigs
    {
        public ePanelType PanelType = ePanelType.ptIRIS;

        private string CommandIO(string _cmd)
        {
            if (PanelType == ePanelType.ptIRIS)
                return _cmd.Substring(0, 4);
            else
                return _cmd;
        }

        private string CommandDataType(string _cmd)
        {
            if (PanelType == ePanelType.ptIRIS)
                return (_cmd.Length >= 6) ? _cmd.Substring(4, 2) : null;
            else
                return _cmd;
        }

        private string CommandKey(string _cmd)
        {
            if (PanelType == ePanelType.ptIRIS)
                return CommandIO(_cmd) + CommandDataType(_cmd);
            else
                return _cmd;
        }

        private string CommandIndexS(string _cmd)
        {
            if (PanelType == ePanelType.ptIRIS)
                return (_cmd.Length >= 8) ? _cmd.Substring(6, 2) : null;
            else
                return _cmd;
        }

        private byte CommandIndexB(string _cmd)
        {
            if (PanelType == ePanelType.ptIRIS)
                return Convert.ToByte(CommandIndexS(_cmd), 16);
            else
                return 0;
        }

        private dObject2JSONString _serialyzer = null;
        private dJSONString2Object _deserialyzer = null;

        public cXmlConfigs(dObject2JSONString serialyzer, dJSONString2Object deserialyzer)
        {
            _serialyzer = serialyzer;
            _deserialyzer = deserialyzer;
        }

        private object _cs_config = new object();

        private string _config_path = null;
        /// <summary>
        /// файл, съдържащ основна конфигурация на устройство
        /// </summary>
        public string ConfigPath
        {
            get
            {
                string f = null;
                Monitor.Enter(_cs_config);
                f = _config_path;
                Monitor.Exit(_cs_config);
                return f;
            }
            set
            {
                Monitor.Enter(_cs_config);
                _config_path = value;
                Monitor.Exit(_cs_config);
            }
        }

        private object _config = null;
        /// <summary>
        /// Основна конфигурация(XmlDocument)
        /// </summary>
        public object Config
        {
            get
            {
                object cfg = null;
                Monitor.Enter(_cs_config);
                cfg = _config;
                Monitor.Exit(_cs_config);
                return cfg;
            }
            set
            {
                Monitor.Enter(_cs_config);
                _config = value;
                Monitor.Exit(_cs_config);
            }
        }

        private Dictionary<string, object> _read_structure = null;
        private List<cSeria> _serias = null;
        private string _read_xml_no_serias = null;
        private Dictionary<string, cPropertyGroup> _property_groups;
        private object _cs_read_structure = new object();

        private bool xmlFirstIsObject(string body)
        {
            string xml = Regex.Replace(body, @"^[^<]+", "");
            Match mstart = Regex.Match(xml, "<");
            if (mstart.Success)
                xml = xml.Substring(mstart.Groups[0].Index + 1, xml.Length - mstart.Groups[0].Index - 1);
            else
                return false;
            int posobjend = Int32.MaxValue;
            Match mobjend = Regex.Match(xml, "<");
            if (mobjend.Success)
                posobjend = mobjend.Groups[0].Index;
            int pospropend = Int32.MaxValue;
            Match mpropend = Regex.Match(xml, "/>");
            if (mpropend.Success)
                pospropend = mpropend.Groups[0].Index;
            return posobjend < pospropend;
        }
        private Dictionary<string, object> ReadNode(string xml)
        {
            xml = Regex.Replace(xml, @"<\?[\w\W]*?\?>", "");
            xml = xml.Trim();
            if (xml == null || !Regex.IsMatch(xml, @"^\s*?<[\w\W]+>\s*$"))
                return null;
            Dictionary<string, object> res = null;
            do
            {
                string regex = null;
                bool isObj = xmlFirstIsObject(xml);
                if (isObj)
                    regex = @"(<\w+?)([\s>])([\w\W]+)(</\w+>)";
                else
                    regex = @"(<\w+?)([\s>])([\w\W]+)(/>)";
                Match m = Regex.Match(xml, regex);
                if (!m.Success)
                    return res;
                string start = m.Groups[1].Value;
                string body = m.Groups[2].Value + m.Groups[3].Value;
                string end = m.Groups[4].Value;
                string n = start + body + end;
                body = Regex.Replace(body, @"^[^<]+", "");
                if (res == null)
                    res = new Dictionary<string, object>();
                string key = Regex.Replace(start, @"[<]", "").Trim();
                if (isObj)
                    res.Add(key, ReadNode(body));
                else
                    res.Add(key, n);
                int i = m.Groups[0].Index + m.Groups[0].Length + 1;
                if (i < xml.Length)
                {
                    int len = xml.Length - i;// + 1;
                    xml = xml.Substring(i, len).Trim();
                }
                else
                    xml = "";
                //foreach (Match m in Regex.Matches(xml, @"(<\w+?)([\s>])([\w\W]+)(/>|</\w+>)"))
                //{
                //    string start = m.Groups[1].Value;
                //    string body = m.Groups[2].Value + m.Groups[3].Value;
                //    string end = m.Groups[4].Value;
                //    if (end != "/>")//object
                //    {
                //        string n = start + body + end;
                //        string key = Regex.Replace(start, @"[</>]", "").Trim();
                //        body = Regex.Replace(body, @"^[^<]+", "");
                //        if (res == null)
                //            res = new Dictionary<string, object>();
                //        res.Add(key, ReadNode(body));
                //    }
                //    else
                //    {
                //        string prop = start + body + end;
                //    }
                //}
            } while (xml != "");
            //
            return res;
        }

        private Dictionary<string, Dictionary<string, Dictionary<string, List<string>>>> _dread_prop = new Dictionary<string, Dictionary<string, Dictionary<string, List<string>>>>();
        public Dictionary<string, Dictionary<string, Dictionary<string, List<string>>>> ReadProperties
        {
            get
            {
                Dictionary<string, Dictionary<string, Dictionary<string, List<string>>>> res = null;
                Monitor.Enter(_cs_read_structure);
                res = _dread_prop;
                Monitor.Exit(_cs_read_structure);
                return res;
            }
            set
            {
                Monitor.Enter(_cs_read_structure);
                _dread_prop = value;
                Monitor.Exit(_cs_read_structure);
            }
        }

        private void ParseReadSeriaIRIS(string elements, ref string last_path)
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
            //
            elements = Regex.Replace(elements, @"^\s*?<ELEMENT[\w\W]*?>\s*?<ELEMENTS>", "");
            elements = Regex.Replace(elements, @"</ELEMENTS>\s*?</ELEMENT>\s*$", "").Trim();
            foreach (Match mel in Regex.Matches(elements, @"<ELEMENT[\w\W]*?</ELEMENT>"))
            {
                string el = mel.Value;
                m = Regex.Match(el, @"ID\s*?=\s*?""([\w\W]+?)""");
                if (m.Success)
                {
                    //string element = ((element_root != "") ? element_root + "/" : "") + m.Groups[1].Value;
                    string element = path;
                    if (!Regex.IsMatch(path, "/" + m.Groups[1].Value + "/"))
                    {
                        string[] sarr = path.Split('/');
                        sarr[sarr.Length - 1] = m.Groups[1].Value;
                        element = String.Join('/', sarr);
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

        private List<cSeria> cfgReadSeries(string xml)
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
            {
                if (PanelType == ePanelType.ptIRIS)
                {
                    ParseReadSeriaIRIS(m.Groups[1].Value, ref last_path);
                    continue;
                }
                //
                cSeria seria = new cSeria();
                seria._txt = m.Groups[1].Value;
                List<List<cPanelCommand>> clst = new List<List<cPanelCommand>>();
                foreach (Match cm in Regex.Matches(seria._txt, @"<COMMANDS([\w\W]+?)</COMMANDS", RegexOptions.IgnoreCase))
                {
                    List<cPanelCommand> cmdl = new List<cPanelCommand>();
                    string cmds = cm.Groups[1].Value;
                    foreach (Match mcmd in Regex.Matches(cmds, @"BYTES\s*?=\s*?""([\w\W]*?)""", RegexOptions.IgnoreCase))
                    {
                        cPanelCommand pc = new cPanelCommand();
                        pc.scmd = mcmd.Groups[1].Value;
                        pc._bcmd = new byte[pc.scmd.Length / 2];
                        for (int i = 0; i < pc.scmd.Length / 2; i++)
                            pc._bcmd[i] = byte.Parse(pc.scmd.Substring(i * 2, 2), System.Globalization.NumberStyles.HexNumber);
                        cmdl.Add(pc);
                    }
                    clst.Add(cmdl);
                }
                if (clst.Count > 0)
                {
                    List<cPanelCommand> zlst = clst[0];
                    for (int j = 0; j < clst[0].Count; j++)
                    {
                        List<byte[]> lstidx = new List<byte[]>();
                        byte[] bbase = null;
                        byte[] bmask = null;
                        for (int i = 0; i < clst.Count; i++)
                        {
                            if (i == 0)
                            {
                                bbase = new byte[clst[i][j]._bcmd.Length];
                                clst[i][j]._bcmd.CopyTo(bbase, 0);
                                bmask = new byte[clst[i][j]._bcmd.Length];
                                for (int k = 0; k < bmask.Length; k++)
                                    bmask[k] = 0;
                            }
                            byte[] b = new byte[bbase.Length];
                            clst[i][j]._bcmd.CopyTo(b, 0);
                            lstidx.Add(b);
                            byte[] bxor = new byte[bbase.Length];
                            for (int k = 0; k < bbase.Length; k++)
                            {
                                bxor[k] = (byte)(bbase[k] ^ b[k]);
                                bmask[k] = (byte)(bmask[k] | bxor[k]);
                            }
                        }
                        cPanelCommand _cmd = new cPanelCommand();
                        _cmd.scmd = clst[0][j].scmd;
                        _cmd._bcmd = clst[0][j]._bcmd;
                        _cmd._bidxmask = new byte[bmask.Length];
                        bmask.CopyTo(_cmd._bidxmask, 0);
                        List<byte> lprefix = new List<byte>();
                        _cmd._idxbytescnt = 0;
                        List<byte> lsufix = new List<byte>();
                        bool idx = false;
                        for (int k = 0; k < bmask.Length; k++)
                        {
                            if (bmask[k] == 0)
                                if (!idx)
                                    lprefix.Add(_cmd._bcmd[k]);
                                else lsufix.Add(_cmd._bcmd[k]);
                            else
                            {
                                idx = true;
                                _cmd._idxbytescnt++;
                            }
                        }
                        _cmd._prefix = new byte[lprefix.Count];
                        lprefix.CopyTo(_cmd._prefix);
                        _cmd._sufix = new byte[lsufix.Count];
                        lsufix.CopyTo(_cmd._sufix);
                        //
                        for (int k = 0; k < lstidx.Count; k++)
                            for (int l = 0; l < lstidx[k].Length; l++)
                                lstidx[k][l] = (byte)(lstidx[k][l] & bmask[l]);
                        if (_cmd._idxbytes == null)
                            _cmd._idxbytes = new List<byte[]>();
                        _cmd._idxbytes.Clear();
                        for (int k = 0; k < lstidx.Count; k++)
                        {
                            byte[] b = new byte[_cmd._idxbytescnt];
                            int iidx = 0;
                            for (int l = 0; l < lstidx[k].Length; l++)
                                if (lstidx[k][l] != 0)
                                {
                                    b[iidx] = lstidx[k][l];
                                    iidx++;
                                }
                            _cmd._idxbytes.Add(b);
                        }
                        lstidx.Clear();
                        seria._commands.Add(_cmd);
                    }
                }
                //
                string firstelement = Regex.Replace(seria._txt, @"^\s*?<ELEMENT[\w\W]+?>", "", RegexOptions.IgnoreCase);
                firstelement = Regex.Replace(firstelement, @"^\s*?<ELEMENTS[\w\W]*?>", "", RegexOptions.IgnoreCase);
                Match mf = Regex.Match(firstelement, @"^[\w\W]*?</ELEMENT[\w\W]*?>", RegexOptions.IgnoreCase);
                if (mf.Success)
                {
                    seria._element = Regex.Replace(mf.Value, @"^[\r\n]+", "");
                    Match em = Regex.Match(seria._element, @"^\s*?<ELEMENT[\w\W]*?>", RegexOptions.IgnoreCase);
                    if (em.Success)
                    {
                        em = Regex.Match(em.Value, @"ID\s*?=\s*?""([\w\W]+?)""", RegexOptions.IgnoreCase);
                        if (em.Success)
                            seria.element_id = em.Groups[1].Value;
                    }
                    seria.properties = new Dictionary<string, cSeriaProperty>();
                    foreach (Match mp in Regex.Matches(seria._element, @"<PROPERTY[\w\W]+?/>"))
                    {
                        string sp = mp.Value;
                        string pid = null;
                        string pbyte = null;
                        string plen = null;
                        Match pvm = Regex.Match(sp, @"ID\s*?=\s*?""([\w\W]+?)""", RegexOptions.IgnoreCase);
                        if (pvm.Success)
                            pid = pvm.Groups[1].Value;
                        pvm = Regex.Match(sp, @"BYTE\s*?=\s*?""([\w\W]+?)""", RegexOptions.IgnoreCase);
                        if (pvm.Success)
                            pbyte = pvm.Groups[1].Value;
                        pvm = Regex.Match(sp, @"LENGTH\s*?=\s*?""([\w\W]+?)""", RegexOptions.IgnoreCase);
                        if (pvm.Success)
                            plen = pvm.Groups[1].Value;
                        if (pid != null && pbyte != null && plen != null)
                        {
                            cSeriaProperty prop = new cSeriaProperty();
                            prop.id = pid;
                            prop.byteidx = Convert.ToInt32(pbyte);
                            prop.len = Convert.ToInt32(plen);
                            seria.properties.Add(prop.id, prop);
                        }
                    }
                    string start = "";
                    //MatchCollection mc = Regex.Matches(seria._txt, @"^[\w\W]*?<ELEMENT[\w\W]+?<ELEMENTS[\w\W]+?\s*");
                    //Match mstart = Regex.Match(seria._txt, @"^[\w\W]*?<ELEMENT[\w\W]+?<ELEMENTS[\w\W]+?\s*");
                    //if (false/* && mstart.Success*/)
                    //{
                    //    //start = mstart.Value;
                    //}
                    //else
                    //{
                    //    Match mstart = Regex.Match(seria._txt, @"(^[\w\W]+?)<ELEMENT\W[\w\W]+?ID\s*?=\s*?""" + seria.element_id + @"""");
                    //    //Match mstart = Regex.Match(seria._txt, @"<ELEMENT\W[\w\W]+?ID\s*?=\s*?""" + seria.element_id + @"""");
                    //    if (mstart.Success)
                    //    {
                    //        start = mstart.Groups[1].Value;
                    //        //start = mstart.Value.Substring(0, mstart.Index);
                    //    }
                    //    else
                    //    {
                    //        start = "";
                    //    }
                    //}
                    //seria._txt_noelements = Regex.Replace(seria._txt, @"<ELEMENT[\w\W]+?ID\s*?=\s*?""" + seria.element_id + @"""[\w\W]*?</ELEMENT[\w\W]*?>", "", RegexOptions.IgnoreCase);
                    //seria._txt_noelements = start + "\r\n" + seria._txt_noelements.Trim();
                }
                //
                res.Add(seria);
            }
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

        private string RemoveSerias(string xml)
        {
            string res = Regex.Replace(xml, @"<!\-+?[\w\W]*?\-+?>", "");
            foreach (cSeria seria in _serias)
                res = Regex.Replace(res, seria._txt, "");
            return res;
        }

        private string ParsePropertyGroup(string s, cPropertyGroup grp)
        {
            string key = "";
            Match m = Regex.Match(s, @"<ELEMENT\W[\w\W]*?ID\s*?=\s*?""([\w\W]*?)""[\w\W]*?>", RegexOptions.IgnoreCase);
            if (m.Success)
                key = m.Groups[1].Value;
            //commands
            grp._commands = new List<cPanelCommand>();
            m = Regex.Match(s, @"<COMMANDS[\w\W]*?</COMMANDS>", RegexOptions.IgnoreCase);
            if (m.Success)
                foreach (Match mc in Regex.Matches(m.Value, @"<COMMAND\W[\w\W]*?BYTES\s*?=\s*?""([\w\W]+?)""", RegexOptions.IgnoreCase))
                {
                    cPanelCommand cmd = new cPanelCommand();
                    cmd.scmd = mc.Groups[1].Value;
                    cmd._bcmd = new byte[cmd.scmd.Length / 2];
                    for (int i = 0; i < cmd.scmd.Length / 2; i++)
                        cmd._bcmd[i] = byte.Parse(cmd.scmd.Substring(i * 2, 2), System.Globalization.NumberStyles.HexNumber);
                    grp._commands.Add(cmd);
                }
            //properties
            grp._properties = new Dictionary<string, cSeriaProperty>();
            m = Regex.Match(s, @"<PROPERTIES[\w\W]*?</PROPERTIES>", RegexOptions.IgnoreCase);
            foreach (Match mp in Regex.Matches(m.Value, @"<PROPERTY\W[\w\W]*?/>", RegexOptions.IgnoreCase))
            {
                cSeriaProperty prop = new cSeriaProperty();
                string p = mp.Value;
                m = Regex.Match(p, @"ID\s*?=\s*?""([\w\W]+?)""", RegexOptions.IgnoreCase);
                if (m.Success)
                    prop.id = m.Groups[1].Value;
                m = Regex.Match(p, @"BYTE\s*?=\s*?""([\w\W]+?)""", RegexOptions.IgnoreCase);
                if (m.Success)
                    prop.byteidx = Convert.ToInt32(m.Groups[1].Value);
                m = Regex.Match(p, @"LENGTH\s*?=\s*?""([\w\W]+?)""", RegexOptions.IgnoreCase);
                if (m.Success)
                    prop.len = Convert.ToInt32(m.Groups[1].Value);
                grp._properties.Add(prop.id, prop);
            }
            //
            return key;
        }

        private Dictionary<string, cPropertyGroup> PropertyGroups(string xml)
        {
            Dictionary<string, cPropertyGroup> res = new Dictionary<string, cPropertyGroup>();
            List<string> lgrp = new List<string>();
            foreach (Match m in Regex.Matches(xml, @"<ELEMENT\W[\w\W]+?</ELEMENT[\w\W]*?>", RegexOptions.IgnoreCase))
            {
                string s = m.Value;
                Match mm = Regex.Match(s, @"(<ELEMENT\W[\w\W]*?ID\s*?=[\w\W]*?)(<ELEMENT\W[\w\W]+$)", RegexOptions.IgnoreCase);
                if (mm.Success)
                {
                    lgrp.Add(mm.Groups[1].Value);
                    lgrp.Add(mm.Groups[2].Value);
                }
                else
                    lgrp.Add(m.Value);
            }
            foreach (string s in lgrp)
            {
                cPropertyGroup grp = new cPropertyGroup();
                string key = ParsePropertyGroup(s, grp);
                res.Add(key, grp);
            }
            //
            return res;
        }

        private Dictionary<string, string> ReadStructuresJSONFiles()
        {
            Dictionary<string, string> res = new Dictionary<string, string>();
            string dir = Path.GetDirectoryName(_read_path);
            if (!Regex.IsMatch(dir, "\\$"))
                dir += "\\";
            //res.Add("serias", dir + "_serias.json");
            //res.Add("read_xml_no_serias", dir + "_read_xml_no_serias.txt");
            //res.Add("property_groups", dir + "_property_groups.json");
            res.Add("_dread_prop", dir + "_dread_prop.json");
            //
            return res;
        }

        private bool JSONStructReadFilesExists(Dictionary<string, string> files)
        {
            foreach (string key in files.Keys)
                if (!File.Exists(files[key]))
                    return false;
            return true;
        }

        private void SerializeJSONReadStructs(Dictionary<string, string> files)
        {
            string f = null;
            string s = null;
            //
            //f = files["serias"];
            //s = _serialyzer(_serias);
            //File.WriteAllText(f, s);
            ////
            //f = files["read_xml_no_serias"];
            //File.WriteAllText(f, _read_xml_no_serias);
            ////
            //f = files["property_groups"];
            //s = _serialyzer(_property_groups);
            //File.WriteAllText(f, s);
            //
            f = files["_dread_prop"];
            s = _serialyzer(_dread_prop);
            File.WriteAllText(f, s);
        }

        private void DeSerializeJSONReadStructs(Dictionary<string, string> files)
        {
            string f = null;
            string s = null;
            //
            //f = files["serias"];
            //s = File.ReadAllText(f);
            //if (_serias == null)
            //    _serias = new List<cSeria>();
            //_serias = (List<cSeria>)_deserialyzer(s, _serias.GetType());
            ////
            //f = files["read_xml_no_serias"];
            //_read_xml_no_serias = File.ReadAllText(f);
            ////
            //f = files["property_groups"];
            //s = File.ReadAllText(f);
            //if (_property_groups == null)
            //    _property_groups = new Dictionary<string, cPropertyGroup>();
            //_property_groups = (Dictionary<string, cPropertyGroup>)_deserialyzer(s, _property_groups.GetType());
            //
            f = files["_dread_prop"];
            s = File.ReadAllText(f);
            if (_dread_prop == null)
                _dread_prop = new Dictionary<string, Dictionary<string, Dictionary<string, List<string>>>>();
            _dread_prop = (Dictionary<string, Dictionary<string, Dictionary<string, List<string>>>>)_deserialyzer(s, _dread_prop.GetType());
        }

        private void CreateReadStructure()
        {
            Monitor.Enter(_cs_config);
            string _rp = _read_path;
            string rxml = null;
            if (_rp != null && File.Exists(_rp))
                rxml = File.ReadAllText(_rp);
            string _wp = _write_path;
            string wxml = null;
            if (_wp != null && File.Exists(_wp))
                wxml = File.ReadAllText(_wp);
            Monitor.Exit(_cs_config);
            if (_rp == null || rxml == null || !Regex.IsMatch(rxml, @"^\s*?<[\w\W]+>\s*$"))
                return;
            Dictionary<string, string> _read_json_files = ReadStructuresJSONFiles();
            Monitor.Enter(_cs_read_structure);
            if (!JSONStructReadFilesExists(_read_json_files))
            {
                _serias = cfgReadSeries(rxml);
                //_read_xml_no_serias = RemoveSerias(rxml);
                //_property_groups = PropertyGroups(_read_xml_no_serias);
                ////_read_structure = ReadNode(rxml);
                ////cfgReadSeries(rxml);
                SerializeJSONReadStructs(_read_json_files);
            }
            else
                DeSerializeJSONReadStructs(_read_json_files);
            Monitor.Exit(_cs_read_structure);
        }

        private string _read_path = null;
        /// <summary>
        /// файл, съдържащ read-конфигурация на устройство
        /// </summary>
        public string ReadConfigPath
        {
            get
            {
                string f = null;
                Monitor.Enter(_cs_config);
                f = _read_path;
                Monitor.Exit(_cs_config);
                return f;
            }
            set
            {
                Monitor.Enter(_cs_config);
                _read_path = value;
                CreateReadStructure();
                Monitor.Exit(_cs_config);

            }
        }

        private object _read_config = null;
        /// <summary>
        /// read-конфигурация(XmlDocument)
        /// </summary>
        public object ReadConfig
        {
            get
            {
                object cfg = null;
                Monitor.Enter(_cs_config);
                cfg = _read_config;
                Monitor.Exit(_cs_config);
                return cfg;
            }
            set
            {
                Monitor.Enter(_cs_config);
                _read_config = value;
                Monitor.Exit(_cs_config);

            }
        }

        private object _cs_write_structure = new object();
        private List<cSeria> _wserias = null;
        private Dictionary<string, Dictionary<string, List<cSeriaProperty>>> _write_property_groups = new Dictionary<string, Dictionary<string, List<cSeriaProperty>>>();

        private Dictionary<string, Dictionary<string, Dictionary<string, List<string>>>> _dwrite_prop = new Dictionary<string, Dictionary<string, Dictionary<string, List<string>>>>();

        public Dictionary<string, Dictionary<string, Dictionary<string, List<string>>>> WriteProperties
        {
            get
            {
                Dictionary<string, Dictionary<string, Dictionary<string, List<string>>>> res = null;
                Monitor.Enter(_cs_write_structure);
                res = _dwrite_prop;
                Monitor.Exit(_cs_write_structure);
                return res;
            }
            set
            {
                Monitor.Enter(_cs_write_structure);
                _dwrite_prop = value;
                Monitor.Exit(_cs_write_structure);
            }
        }

        private void ParseWriteProperties(string write_operation)
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
            return "";
        }

        private List<cSeria> cfgWriteSerias(string xml)
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
                    if (PanelType == ePanelType.ptIRIS)
                    {
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

        private Dictionary<string, string> WriteStructuresJSONFiles()
        {
            Dictionary<string, string> res = new Dictionary<string, string>();
            string dir = Path.GetDirectoryName(_write_path);
            if (!Regex.IsMatch(dir, "\\$"))
                dir += "\\";
            //res.Add("wserias", dir + "_wserias.json");
            //res.Add("write_property_groups", dir + "_write_property_groups.json");
            res.Add("write_properties", dir + "_dwrite_prop.json");
            //
            return res;
        }

        private bool JSONStructWriteFilesExists(Dictionary<string, string> files)
        {
            foreach (string key in files.Keys)
                if (!File.Exists(files[key]))
                    return false;
            return true;
        }

        private void SerializeJSONWriteStructs(Dictionary<string, string> files)
        {
            string f = null;
            string s = null;
            //f = files["wserias"];
            //s = _serialyzer(_wserias);
            //File.WriteAllText(f, s);
            ////
            //f = files["write_property_groups"];
            //s = _serialyzer(_write_property_groups);
            //File.WriteAllText(f, s);
            //
            f = files["write_properties"];
            s = _serialyzer(_dwrite_prop);
            File.WriteAllText(f, s);
        }

        private void DeSerializeJSONWriteStructs(Dictionary<string, string> files)
        {
            string f = null;
            string s = null;
            //f = files["wserias"];
            //s = File.ReadAllText(f);
            //if (_wserias == null)
            //    _wserias = new List<cSeria>();
            //_wserias = (List<cSeria>)_deserialyzer(s, _wserias.GetType());
            ////
            //f = files["write_property_groups"];
            //s = File.ReadAllText(f);
            //if (_write_property_groups == null)
            //    _write_property_groups = new Dictionary<string, Dictionary<string, List<cSeriaProperty>>>();
            //_write_property_groups = (Dictionary<string, Dictionary<string, List<cSeriaProperty>>>)_deserialyzer(s, _write_property_groups.GetType());
            //
            //
            f = files["write_properties"];
            s = File.ReadAllText(f);
            if (_dwrite_prop == null)
                _dwrite_prop = new Dictionary<string, Dictionary<string, Dictionary<string, List<string>>>>();
            _dwrite_prop = (Dictionary<string, Dictionary<string, Dictionary<string, List<string>>>>)_deserialyzer(s, _dwrite_prop.GetType());
        }

        private void CreateWriteStructure()
        {
            Monitor.Enter(_cs_config);
            string _wp = _write_path;
            string wxml = null;
            if (_wp != null && File.Exists(_wp))
                wxml = File.ReadAllText(_wp);
            Monitor.Exit(_cs_config);
            if (_wp == null || wxml == null || !Regex.IsMatch(wxml, @"^\s*?<[\w\W]+>\s*$"))
                return;
            Monitor.Enter(_cs_write_structure);
            Dictionary<string, string> files = WriteStructuresJSONFiles();
            if (!JSONStructWriteFilesExists(files))
            {
                _wserias = cfgWriteSerias(wxml);
                SerializeJSONWriteStructs(files);
            }
            else
                DeSerializeJSONWriteStructs(files);
            Monitor.Exit(_cs_write_structure);
        }

        private string FindReadKeyIRIS(string wkey)
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
                if (Regex.IsMatch(key, wkey + "$"))
                    return key;
                if (Regex.IsMatch(key, "^" + wkey))
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

        public Dictionary<string, object> RWMerged()
        {
            //Dictionary<string, Dictionary<string, Dictionary<string, List<string>>>> dread = null;
            //Dictionary<string, Dictionary<string, Dictionary<string, List<string>>>> dwrite = null;
            Monitor.Enter(_cs_read_structure);
            Monitor.Enter(_cs_write_structure);
            if (_dread_prop == null || _dwrite_prop == null/* || _dread_prop.Count != _dwrite_prop.Count*/)
            {
                Monitor.Exit(_cs_write_structure);
                Monitor.Exit(_cs_read_structure);
                return null;
            }
            Dictionary<string, object> res = new Dictionary<string, object>();
            //
            Dictionary<string, string> wrkeys = new Dictionary<string, string>();
            foreach (string wkey in _dwrite_prop.Keys)
            {
                string rkey = null;
                if (PanelType == ePanelType.ptIRIS)
                    rkey = FindReadKeyIRIS(wkey);
                if (rkey == null)
                {
                    Monitor.Exit(_cs_write_structure);
                    Monitor.Exit(_cs_read_structure);
                    return null;
                }
                wrkeys.Add(wkey, rkey);
            }
            //
            Monitor.Exit(_cs_write_structure);
            Monitor.Exit(_cs_read_structure);
            return res;
        }

        private Dictionary<string, Dictionary<string, cWriteField>> _xpaths = null;
        private string _write_path = null;
        /// <summary>
        /// файл, съдържащ write-конфигурация на устройство
        /// </summary>
        public string WriteConfigPath
        {
            get
            {
                string f = null;
                Monitor.Enter(_cs_config);
                f = _write_path;
                Monitor.Exit(_cs_config);
                return f;
            }
            set
            {
                Monitor.Enter(_cs_config);
                _write_path = value;
                CreateWriteStructure();
                Monitor.Exit(_cs_config);
            }
        }

        private object _write_config = null;
        /// <summary>
        /// write-конфигурация(XmlDocument)
        /// </summary>
        public object WriteConfig
        {
            get
            {
                object cfg = null;
                Monitor.Enter(_cs_config);
                cfg = _write_config;
                Monitor.Exit(_cs_config);
                return cfg;
            }
            set
            {
                Monitor.Enter(_cs_config);
                _write_config = value;
                Monitor.Exit(_cs_config);

            }
        }

        public Dictionary<string, Dictionary<string, cWriteField>> WriteXpaths
        {
            get
            {
                Dictionary<string, Dictionary<string, cWriteField>> res = null;
                Monitor.Enter(_cs_config);
                res = _xpaths;
                if (res == null)
                {
                    string f = _write_path;
                    string xml = File.ReadAllText(f);
                    _xpaths = new Dictionary<string, Dictionary<string, cWriteField>>();
                    foreach (Match mop in Regex.Matches(xml, @"<WRITEOPERATION>[\w\W]+?</WRITEOPERATION>", RegexOptions.IgnoreCase))
                    {
                        string op = mop.Groups[0].Value;
                        Match mbytes = Regex.Match(op, @"<BYTES>[\w\W]+?</BYTES>", RegexOptions.IgnoreCase);
                        if (mbytes.Success)
                        {
                            string bytes = mbytes.Groups[0].Value;
                            Match mcmd = Regex.Match(bytes, @"<BYTE[^<]*?VALUE\s*?=\s*?""([\dA-Fa-f]+?)""", RegexOptions.IgnoreCase);
                            if (mcmd.Success)
                            {
                                string cmd = mcmd.Groups[1].Value;
                                if (!_xpaths.ContainsKey(cmd))
                                    _xpaths.Add(cmd, new Dictionary<string, cWriteField>());
                                else
                                    _xpaths[cmd].Clear();
                                int ord = 1;
                                foreach (Match mbyte in Regex.Matches(bytes, @"<BYTE[\w\W]*?XPATH\s*?=[\w\W]+?/>"))
                                {
                                    string ln = mbyte.Groups[0].Value;
                                    string path = null;
                                    string xpath = null;
                                    int len = -1;
                                    Match m = Regex.Match(ln, @"XPATH\s*?=\s*?""([\w\W]+?)""", RegexOptions.IgnoreCase);
                                    if (m.Success)
                                    {
                                        path = m.Groups[1].Value;
                                        xpath = path;
                                        //xpath = "//ELEMENTS / ELEMENT[@ID = 'IRIS8_FAT_FBF'] / ELEMENTS / ELEMENT[1] / PROPERTIES / PROPERTY[@ID = 'Type']";
                                        //if (xpath == "ELEMENTS/ELEMENT[@ID='IRIS8_FAT_FBF']/ELEMENTS/ELEMENT[1]/PROPERTIES/PROPERTY[@ID='Type']")
                                        //{
                                        //    string sdebug = "";
                                        //}
                                        //m = Regex.Match(xpath, @"ELEMENTS/ELEMENT[\w\W]*?\[\s*?@ID\s*?=\s*?[""']([\w\W]+?)[""']\s*?\]([\w\W]*?)(/ELEMENTS/ELEMENT[\w\W]*?)\[(\d+?)\D", RegexOptions.IgnoreCase);
                                        m = Regex.Match(xpath, @"/*?ELEMENTS\s*?/\s*?ELEMENT\s*?\[\D[\w\W]+?(/\s*?ELEMENTS\s*?/\s*?ELEMENT[\w\W]*?)\[(\d+?)\D([\w\W]+$)", RegexOptions.IgnoreCase);
                                        if (m.Success)
                                        {
                                            string grp = Regex.Replace(m.Groups[1].Value, @"\s+", "") + Regex.Replace(m.Groups[3].Value, @"\s+", "");
                                            int idx = Convert.ToInt32(m.Groups[2].Value);
                                            if (idx > 1)
                                                continue;
                                            xpath = grp;
                                            //if (idx <= 1)
                                            //{
                                            //    if (!lists.ContainsKey(grp))
                                            //        lists.Add(grp, new StringBuilder());
                                            //    lists[grp].Append(xpath + "\n");
                                            //}
                                            //continue;
                                        }
                                        //
                                        if (Regex.IsMatch(xpath, @"^properties", RegexOptions.IgnoreCase))
                                            xpath = "ELEMENTS/ELEMENT[1]/" + xpath;
                                        if (Regex.IsMatch(xpath, @"ELEMENTS/ELEMENT[\w\W]*?/ELEMENTS/ELEMENT", RegexOptions.IgnoreCase))
                                            xpath = Regex.Replace(xpath, @"^ELEMENTS/ELEMENT[\w\W]*?/", "", RegexOptions.IgnoreCase);
                                        xpath = Regex.Replace(xpath, @"^([\w\W]+?/ELEMENT\[[\w\W]+?=[\w\W]+?\])\[\d+?\](/[\w\W]+$)", "$1$2", RegexOptions.IgnoreCase);
                                        //
                                    }
                                    m = Regex.Match(ln, @"LENGTH\s*?=""(\d+?)""", RegexOptions.IgnoreCase);
                                    if (m.Success)
                                        len = Convert.ToInt32(m.Groups[1].Value);
                                    if (path != null)
                                    {
                                        cWriteField field = new cWriteField();
                                        field.lngid = null;
                                        field.ord = ord;
                                        field.bytes_cnt = len;
                                        field.path = xpath;
                                        field.cmd = cmd;
                                        field.bcmd = Encoding.ASCII.GetBytes(cmd);
                                        field.path_org = path;
                                        if (!_xpaths[cmd].ContainsKey(path))
                                            _xpaths[cmd].Add(path, field);
                                        else
                                            _xpaths[cmd][path] = field;
                                        ord++;
                                    }
                                }
                            }
                        }
                    }
                    res = _xpaths;
                }
                Monitor.Exit(_cs_config);
                return res;
            }
        }

        private Dictionary<string, cWriteField> _write_fields_by_lngid = null;

        public Dictionary<string, cWriteField> WriteFieldsByLNGID
        {
            get
            {
                Dictionary<string, cWriteField> dublicated = new Dictionary<string, cWriteField>();
                int dublcnt = 1;
                Monitor.Enter(_cs_config);
                if (_write_fields_by_lngid == null)
                {
                    if (_xpaths != null)
                    {
                        _write_fields_by_lngid = new Dictionary<string, cWriteField>();
                        foreach (string cmd in _xpaths.Keys)
                            foreach (string path in _xpaths[cmd].Keys)
                                if (_xpaths[cmd][path] != null && _xpaths[cmd][path].lngid != null && _xpaths[cmd][path].lngid != "")
                                    if (!_write_fields_by_lngid.ContainsKey(_xpaths[cmd][path].lngid))
                                        _write_fields_by_lngid.Add(_xpaths[cmd][path].lngid, _xpaths[cmd][path]);
                                    else
                                    {
                                        if (!dublicated.ContainsKey(_xpaths[cmd][path].lngid))
                                            dublicated.Add(_xpaths[cmd][path].lngid, _xpaths[cmd][path]);
                                        string lngid = _xpaths[cmd][path].lngid += "/dublicate(" + dublcnt.ToString() + ")";
                                        _write_fields_by_lngid.Add(lngid, _xpaths[cmd][path]);
                                        dublicated.Add(lngid, _xpaths[cmd][path]);
                                        dublcnt++;
                                    }
                    }
                }
                //
                //
                Monitor.Exit(_cs_config);
                return _write_fields_by_lngid;
            }
        }
    }
    public class classes
    {
    }
}
