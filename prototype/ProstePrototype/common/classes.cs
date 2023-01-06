using System;
using System.Threading;
using System.IO;
using System.Text.RegularExpressions;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Windows.Documents;
using System.Text.Encodings;

namespace common
{
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
    }

    /// <summary>
    /// Съдържа XML-конфигурации(template, read, write)
    /// </summary>
    public class cXmlConfigs
    {
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
                                        //
                                        //m = Regex.Match(xpath, @"ELEMENTS/ELEMENT[\w\W]*?\[\s*?@ID\s*?=\s*?[""']([\w\W]+?)[""']\s*?\]([\w\W]*?)(/ELEMENTS/ELEMENT[\w\W]*?)\[(\d+?)\D", RegexOptions.IgnoreCase);
                                        m = Regex.Match(xpath, @"ELEMENTS/ELEMENT[\w\W]+?(/ELEMENTS/ELEMENT[\w\W]*?)\[(\d+?)\D([\w\W]+$)", RegexOptions.IgnoreCase);
                                        if (m.Success)
                                        {
                                            string grp = m.Groups[1].Value + m.Groups[3].Value;
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
                List<cWriteField> dublicated = new List<cWriteField>();
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
                                        dublicated.Add(_xpaths[cmd][path]);
                                        string lngid = _xpaths[cmd][path].lngid += "/dublicate(" + dublcnt.ToString() + ")";
                                        _xpaths[cmd][path].lngid += "/dublicate";
                                        _write_fields_by_lngid.Add(lngid, _xpaths[cmd][path]);
                                        dublcnt++;
                                    }
                    }
                }
                Monitor.Exit(_cs_config);
                return _write_fields_by_lngid;
            }
        }
    }
    public class classes
    {
    }
}
