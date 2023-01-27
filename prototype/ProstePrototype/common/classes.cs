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
using System.Threading.Channels;

namespace common
{
    public delegate string dObject2JSONString(object o);
    public delegate object dJSONString2Object(string s, System.Type typ);
    public delegate void dFilterValueChanged(string path, string _new_val);

    public static class constants
    {
        public static string NO_LOOP = "NO_LOOP";
    }

    public class cLoopChannelInfo
    {
        public string loop;
        public string device;
        public string uname;
        public string channel;
        public string Name
        {
            get { return ((uname != null) ? uname : "") + "/" + channel; }
        }
    }

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
    public enum eIO { ioNull = 0, ioRead = 1, ioWrite = 2 };
    public enum eWriteOperation { woBytes = 1, woProperty = 2 };

    #region read/write
    public class cRWProperty
    {
        public string id = null;
        public string xmltag = "";
        public int offset;
        public byte bytescnt;

        internal string xml_tag_quote()
        {
            Match m = Regex.Match(xmltag, @"\w\s*?=\s*?(['""])");
            if (m.Success)
                return m.Groups[1].Value;
            return "\"";
        }
        public virtual string PropertyValue(eIO rw, string PropertyName) { return null; }
    }

    public class cRWCommand
    {
        public eIO io;
        public byte? DataType = null;
        public byte? index = null;
        public byte? buffoffset = null;
        public byte? len = null;
        //
        public string sio = null;
        public string sDataType = null;
        public string sindex = null;
        public string sbuffoffset = null;
        public string slen = null;

        public virtual int CommandLength() { return 0; }
        public virtual string CommandString() { return null; }
    }

    public class cWriteOperation
    {
        public eWriteOperation operation;
        public string value;
    }

    public class cRWPath
    {
        public string ReadPath = null;
        public List<cRWCommand> ReadCommands = new List<cRWCommand>();
        public Dictionary<string, cRWProperty> ReadProperties = new Dictionary<string, cRWProperty>();
        public string WritePath = null;
        public List<cRWCommand> WriteCommands = new List<cRWCommand>();
        public Dictionary<string, cRWProperty> WriteProperties = new Dictionary<string, cRWProperty>();
        public List<cWriteOperation> WriteOperationOrder = new List<cWriteOperation>();
    }

    internal class cRW
    {
        #region command analysis
        internal virtual string CommandIO(string _cmd) { return null; }
        internal virtual string CommandDataType(string _cmd) { return null; }
        internal virtual string CommandKey(string _cmd) { return null; }
        internal virtual string CommandIndexS(string _cmd) { return null; }
        internal virtual byte CommandIndexB(string _cmd) { return 0; }
        internal virtual string CommandBytesOffset(string _cmd) { return null; }
        internal virtual string CommandBytesCnt(string _cmd) { return null; }
        internal virtual cRWCommand CommandObject(string _cmd) { return null; }
        #endregion
        #region command synthesis
        internal virtual string SynCMD(string io, string datatype, string idx, string bytesoffset, string bytescnt) { return null; }
        internal virtual string SynCMD(cRWCommand _cmd) { return null; }
        #endregion

        //
        internal Dictionary<string, Dictionary<string, Dictionary<string, List<string>>>> _dread_prop = new Dictionary<string, Dictionary<string, Dictionary<string, List<string>>>>();
        internal string _read_xml_no_serias = null;
        internal object _cs_ = new object();
        internal List<cSeria> _wserias = null;
        internal Dictionary<string, Dictionary<string, List<cSeriaProperty>>> _write_property_groups = new Dictionary<string, Dictionary<string, List<cSeriaProperty>>>();
        internal Dictionary<string, Dictionary<string, Dictionary<string, List<string>>>> _dwrite_prop = new Dictionary<string, Dictionary<string, Dictionary<string, List<string>>>>();
        //
        internal virtual void ParseReadSeriaIRIS(string elements, ref string last_path) { }
        internal virtual List<cSeria> cfgReadSeries(string xml) { return null; }
        internal virtual void ParseWriteProperties(string write_operation) { }
        internal virtual List<cSeria> cfgWriteSerias(string xml) { return null; }
        #region merge
        internal virtual Dictionary<string, cRWPath> RWMerged() { return null; }
        #endregion
    }
    #endregion

    /// <summary>
    /// Съдържа XML-конфигурации(template, read, write)
    /// </summary>
    public class cXmlConfigs
    {
        public ePanelType PanelType = ePanelType.ptIRIS;

        private cRW _readwriter = null;
        private dObject2JSONString _serialyzer = null;
        private dJSONString2Object _deserialyzer = null;

        public cXmlConfigs(string _s_panel_type, dObject2JSONString serialyzer, dJSONString2Object deserialyzer)
        {
            if (Regex.IsMatch(_s_panel_type, "iris", RegexOptions.IgnoreCase))
            {
                _readwriter = new cRWIRIS();
                PanelType = ePanelType.ptIRIS;
            }
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

        //private Dictionary<string, Dictionary<string, Dictionary<string, List<string>>>> _dread_prop = new Dictionary<string, Dictionary<string, Dictionary<string, List<string>>>>();
        public Dictionary<string, Dictionary<string, Dictionary<string, List<string>>>> ReadProperties
        {
            get
            {
                Dictionary<string, Dictionary<string, Dictionary<string, List<string>>>> res = null;
                Monitor.Enter(_cs_read_structure);
                res = _readwriter._dread_prop;
                Monitor.Exit(_cs_read_structure);
                return res;
            }
            set
            {
                Monitor.Enter(_cs_read_structure);
                _readwriter._dread_prop = value;
                Monitor.Exit(_cs_read_structure);
            }
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
            string fn = Path.GetFileNameWithoutExtension(_read_path);
            //res.Add("serias", dir + "_serias.json");
            //res.Add("read_xml_no_serias", dir + "_read_xml_no_serias.txt");
            //res.Add("property_groups", dir + "_property_groups.json");
            res.Add("_dread_prop", dir + fn + "_dread_prop.json");
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
            s = _serialyzer(_readwriter._dread_prop);
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
            if (_readwriter._dread_prop == null)
                _readwriter._dread_prop = new Dictionary<string, Dictionary<string, Dictionary<string, List<string>>>>();
            _readwriter._dread_prop = (Dictionary<string, Dictionary<string, Dictionary<string, List<string>>>>)_deserialyzer(s, _readwriter._dread_prop.GetType());
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
                _serias = _readwriter.cfgReadSeries(rxml);
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

        //public Dictionary<string, Dictionary<string, Dictionary<string, List<string>>>> WriteProperties
        //{
        //    get
        //    {
        //        Dictionary<string, Dictionary<string, Dictionary<string, List<string>>>> res = null;
        //        Monitor.Enter(_cs_write_structure);
        //        res = _readwriter._dwrite_prop;
        //        Monitor.Exit(_cs_write_structure);
        //        return res;
        //    }
        //    set
        //    {
        //        Monitor.Enter(_cs_write_structure);
        //        _readwriter._dwrite_prop = value;
        //        Monitor.Exit(_cs_write_structure);
        //    }
        //}

        private Dictionary<string, string> WriteStructuresJSONFiles()
        {
            Dictionary<string, string> res = new Dictionary<string, string>();
            string dir = Path.GetDirectoryName(_write_path);
            if (!Regex.IsMatch(dir, "\\$"))
                dir += "\\";
            string fn = Path.GetFileNameWithoutExtension(_write_path);
            //res.Add("wserias", dir + "_wserias.json");
            //res.Add("write_property_groups", dir + "_write_property_groups.json");
            res.Add("write_properties", dir + fn + "_dwrite_prop.json");
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
            s = _serialyzer(_readwriter._dwrite_prop);
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
            if (_readwriter._dwrite_prop == null)
                _readwriter._dwrite_prop = new Dictionary<string, Dictionary<string, Dictionary<string, List<string>>>>();
            _readwriter._dwrite_prop = (Dictionary<string, Dictionary<string, Dictionary<string, List<string>>>>)_deserialyzer(s, _readwriter._dwrite_prop.GetType());
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
                _readwriter._wserias = _readwriter.cfgWriteSerias(wxml);
                SerializeJSONWriteStructs(files);
            }
            else
                DeSerializeJSONWriteStructs(files);
            Monitor.Exit(_cs_write_structure);
        }

        public Dictionary<string, cRWPath> RWMerged()
        {
            return _readwriter.RWMerged();
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
