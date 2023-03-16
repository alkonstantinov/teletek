using System;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Text.RegularExpressions;
using common;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using lcommunicate;
using System.Reflection.Metadata.Ecma335;
using System.Linq;

namespace ljson
{
    internal class cPanelField
    {
        #region static
        internal static void ReverseArray(byte[] val)
        {
            if (val.Length == 2)
            {
                byte _val1 = val[0];
                val[0] = val[1];
                val[1] = _val1;
            }
            else if (val.Length == 4)
            {
                ushort _val23 = (ushort)((ushort)(val[0] * 16) + val[1]);
                val[0] = val[2];
                val[1] = val[3];
                val[2] = (byte)(_val23 / 16);
                val[3] = (byte)(_val23 % 16);
            }
        }
        internal static void ProcessByteArrayByField(byte[] val, JObject _field)
        {
            string _sfield = _field.ToString();
            Match m = Regex.Match(_sfield, @"REVERSE""\s*?:\s*?""(\d+?)""", RegexOptions.IgnoreCase);
            bool reverse = m.Success && Convert.ToInt32(m.Groups[1].Value) != 0;
            if (!reverse)
            {
                m = Regex.Match(_sfield, @"REVERSE""\s*?:\s*?""(true|false)""", RegexOptions.IgnoreCase);
                reverse = m.Success && m.Groups[1].ToString().ToLower() == "true";
            }
            if (reverse)
                ReverseArray(val);
        }
        internal static void BytesByXmlTag(byte[] val, string _read_xml, JObject _field)
        {
            Match m = Regex.Match(_read_xml, @"INC\s*?=\s*?""(\d+?)""");
            //
            if (m.Success)
            {
                byte inc = Convert.ToByte(m.Groups[1].Value);
                for (int i = 0; i < val.Length; i++)
                    val[i] += inc;
            }
            ProcessByteArrayByField(val, _field);
        }
        internal static uint Array2UVal(byte[] val)
        {
            uint u = 0;
            for (int i = 0; i < val.Length; i++)
                u += ((uint)val[i] << i * 8);
            return u;
        }
        internal static string SValByArray(byte[] val, JObject _field)
        {
            ProcessByteArrayByField(val, _field);
            uint u = Array2UVal(val);
            return u.ToString();
        }
        internal static string SValByArray(JArray jval, JObject _field)
        {
            List<byte> lst = new List<byte>();
            foreach (JToken t in jval)
                lst.Add(Convert.ToByte(t.ToString()));
            byte[] val = lst.ToArray();
            return SValByArray(val, _field);
        }
        internal static byte[] BValByArray(JArray jval, JObject _field)
        {
            List<byte> lst = new List<byte>();
            foreach (JToken t in jval)
                lst.Add(Convert.ToByte(t.ToString()));
            byte[] val = lst.ToArray();
            ProcessByteArrayByField(val, _field);
            return val;
        }
        public static void SetPathValue(string panel_id, string path, byte[] bval, JToken _field)
        {
            if (_field.Type != JTokenType.Object)
            {
                //cComm.SetPathValue(panel_id, path, value, "");
                return;
            }
            JObject _ofield = (JObject)_field;
            //
            //cComm.SetPathValue(panel_id, path, value, "");
        }
        internal static JObject FieldByWrongPath(JObject panel, string wrong_path, string new_path, dFindObjectByProperty _find)
        {
            JToken t = panel.SelectToken(wrong_path);
            if (t == null || t.Type != JTokenType.Object || ((JObject)t)["@ID"] == null)
                return null;
            string fieldname = ((JObject)t)["@ID"].ToString();
            JToken new_token = panel.SelectToken(new_path);
            if (new_token == null)
                return null;
            JObject o = _find(new_token, "@ID", fieldname);
            return o;
        }
        internal static JObject FindGroup(JObject panel, string wrong_path)
        {

            JToken t = panel.SelectToken(wrong_path);
            if (t == null)
                return null;
            JObject ofield = (JObject)t;
            if (ofield["@ID"] == null)
                return null;
            string field = ofield["@ID"].ToString().ToLower();
            Match m = Regex.Match(wrong_path, @"^[\w\W]+?\.Groups");
            if (!m.Success)
                return null;
            JToken tgroups = panel.SelectToken(m.Value);
            if (tgroups == null)
                return null;
            JObject grp = null;
            foreach (JProperty p in tgroups)
                if (p.Name.ToLower() == field)
                {
                    grp = (JObject)p.Value;
                    if (grp["~type"] != null)
                        return grp;
                    else
                        return null;
                }
            return null;
        }
        internal static Dictionary<string, string> GroupPathValuesAND(JObject grp, byte[] val, string sidx)
        {
            if (!Regex.IsMatch(sidx, "~index"))
                sidx = ".~index~" + sidx;
            if (!Regex.IsMatch(sidx, @"^\."))
                sidx = "." + sidx;
            //
            Dictionary<string, string> res = new Dictionary<string, string>();
            string gpath = grp["~path"].ToString() + sidx;
            if (val != null)
                res.Add(gpath, SValByArray(val, grp));
            else
                return res;
            if (grp["fields"] != null)
                grp = (JObject)grp["fields"];
            ushort bval = (ushort)Array2UVal(val);// Convert.ToUInt16(val);
            foreach (JProperty p in grp.Properties())
            {
                if (p.Value.Type != JTokenType.Object)
                    continue;
                JObject o = (JObject)p.Value;
                if (o["@TYPE"] == null)
                    continue;
                if (o["@AND"] == null)
                    continue;
                string ftype = o["@TYPE"].ToString().ToUpper();
                ushort band = Convert.ToUInt16(o["@AND"].ToString(), 16);
                ushort bres = (ushort)(bval & band);
                string sval = null;
                if (ftype == "CHECK")
                {
                    sval = (bres != 0) ? "True" : "False";
                }
                else
                {
                    sval = bres.ToString();//ako ne e check
                }
                string path = o["~path"].ToString() + sidx;
                if (sval != null)
                    res.Add(path, sval);
            }
            //
            return res;
        }
        internal static Dictionary<string, string> GroupPathValues(JObject grp, byte[] val, string sidx)
        {
            if (grp == null || grp["~type"] == null)
                return null;
            string typ = grp["~type"].ToString().ToUpper();
            if (typ == "AND")
                return GroupPathValuesAND(grp, val, sidx);
            //
            return null;
        }
        internal static Dictionary<string, string> GroupPathValues(JObject grp, string sval, string sidx)
        {
            uint ival = Convert.ToUInt32(sval);
            int i = 1;
            while (Math.Pow(256, 1) < ival)
                i++;
            byte[] val = new byte[i];
            for (int j = 0; j < val.Length; j++)
                val[j] = (byte)(((255 << j * 8) & ival) >> j * 8);
            return GroupPathValues(grp, val, sidx);

        }
        private static string ANDValue(JObject o, string sval)
        {
            if (o["@AND"] == null)
                return sval;
            ushort uand = Convert.ToUInt16(o["@AND"].ToString(), 16);
            ushort uval = Convert.ToUInt16(sval);
            string fval = ((ushort)(uand & uval)).ToString();
            return fval;
        }
        private static void AddAndValues(Dictionary<string, string> values, JObject field, string sval, string sidx)
        {
            if (field["PROPERTIES"] == null || field["PROPERTIES"]["PROPERTY"] == null)
                return;
            if (field["PROPERTIES"]["PROPERTY"].Type == JTokenType.Object)
            {
                JObject o = (JObject)field["PROPERTIES"]["PROPERTY"];
                if (o["~path"] == null)
                    return;
                string fval = ANDValue(o, sval);
                string path = o["~path"].ToString() + sidx;
                if (!values.ContainsKey(path))
                    values.Add(path, fval);
                else
                    values[path] = fval;
            }
            else
            {
                JArray a = (JArray)field["PROPERTIES"]["PROPERTY"];
                foreach (JObject o in a)
                {
                    if (o["~path"] == null)
                        continue;
                    string fval = ANDValue(o, sval);
                    string path = o["~path"].ToString() + sidx;
                    if (!values.ContainsKey(path))
                        values.Add(path, fval);
                    else
                        values[path] = fval;
                    AddAndValues(values, o, fval, sidx);
                }
                //if (o["@TYPE"] != null && o["@TYPE"].ToString().ToUpper() == "AND")
            }
        }
        internal static Dictionary<string, string> ValuesDependedOnType(JObject panel, string path, string sval)
        {
            Dictionary<string, string> res = new Dictionary<string, string>();
            res.Add(path, sval);
            string path_noidx = Regex.Replace(path, @"\.~index~\d+$", "");
            string sidx = "";
            Match m = Regex.Match(path, @"([\w\W]+?)(\.~index~\d+)$");
            if (m.Success)
                sidx = m.Groups[2].Value;
            JToken field = panel.SelectToken(path_noidx);
            if (field == null || field.Type != JTokenType.Object)
                return res;
            JObject ofield = (JObject)field;
            if (ofield["@TYPE"] == null)
                return res;
            string jtype = ofield["@TYPE"].ToString().ToUpper();
            if (jtype == "CHECK")
                try
                {
                    res[path] = Convert.ToBoolean(sval).ToString();
                }
                catch
                {
                    res[path] = (sval == null || sval.Trim() == "" || sval.Trim() == "0") ? "False" : "True";
                }
            else if (jtype == "AND")
                AddAndValues(res, ofield, sval, sidx);
            else if (jtype == "LIST" || jtype == "INT" || jtype == "TEXT" || jtype == "INTLIST" || jtype == "HIDDEN" || jtype == "IP" || jtype == "SLIDER")
                return res;
            else
                return res;
            //
            return res;
        }
        private static string WriteValue(string _type, int _size, string sval, JObject _field, string _xmltag)
        {
            string res = null;
            if (Regex.IsMatch(_type, @"^(INT|LIST)$"))
            {
                res = Convert.ToInt32(sval).ToString("X" + (_size * 2).ToString());
                if (_size == 2 && Regex.IsMatch(_xmltag, @"OPERATION\s*?=\s*?""MAKEINT16"""))
                    res = res.Substring(res.Length - 2, 2) + res.Substring(0, 2);
                //while (_size * 2 > res.Length)
                //    res = "0" + res;
            }
            else if (Regex.IsMatch(_type, @"^(INTLIST|IP)$"))
            {
                string[] lst;
                char delimiter = ',';
                if (_type == "IP")
                    delimiter = '.';
                if (!Regex.IsMatch(sval, @"([,\.])"))
                {
                    while (sval.Length < _size) { sval = "0" + sval; }
                    lst = new string[_size];
                    for (int i = 0; i < sval.Length; i++) { lst[i] = sval[i].ToString(); }
                }
                else
                    lst = sval.Split(delimiter);
                sval = "";
                for (int i = 0; i < lst.Length; i++) { sval += Convert.ToInt32(lst[i]).ToString("X2"); }
                res = Convert.ToInt32(sval, 16).ToString("X" + (_size * 2).ToString());
            }
            else if (Regex.IsMatch(_type, @"(CHECK|SLIDER)"))
            {
                if (Regex.IsMatch(sval, "true", RegexOptions.IgnoreCase))
                    sval = "1";
                else if (Regex.IsMatch(sval, "false", RegexOptions.IgnoreCase))
                    sval = "0";
                res = Convert.ToInt32(sval).ToString("X" + (_size * 2).ToString());
            }
            else if (Regex.IsMatch(_type, @"(TEXT)"))
            {
                byte[] tbytes = Encoding.Unicode.GetBytes(sval);
                byte[] bytesadd = new byte[_size - tbytes.Length];
                Array.Clear(bytesadd, 0, bytesadd.Length);
                byte[] bytesres = new byte[_size];
                tbytes.CopyTo(bytesres, 0);
                bytesadd.CopyTo(bytesres, tbytes.Length);
                res = "";
                foreach (byte b in bytesres)
                    res += b.ToString("X2");
            }
            else if (Regex.IsMatch(_type, @"(HIDDEN|FROMGROUP)"))
            {
                if (!Regex.IsMatch(sval, @"\D"))
                    res = Convert.ToInt32(sval).ToString("X" + (_size * 2).ToString());
            }
            else if (_type == "IP")
            {
                string[] iparr = sval.Split('.');
                if (iparr.Length != 4)
                    return null;
                res = Convert.ToByte(iparr[0]).ToString("X2");
                res += Convert.ToByte(iparr[1]).ToString("X2");
                res += Convert.ToByte(iparr[2]).ToString("X2");
                res += Convert.ToByte(iparr[3]).ToString("X2");
            }
            else if (_type == "TAB")
            {
                Match m = Regex.Match(sval, @"^(\d+?)_");
                if (m.Success)
                    sval = m.Groups[1].Value;
                if (!Regex.IsMatch(sval, @"\D"))
                {
                    byte bval = Convert.ToByte(sval);
                    res = bval.ToString("X" + (_size * 2).ToString());
                }
                else
                {
                    byte[] tbytes = Encoding.Unicode.GetBytes(sval);
                    byte[] bytesadd = new byte[_size - tbytes.Length];
                    Array.Clear(bytesadd, 0, bytesadd.Length);
                    byte[] bytesres = new byte[_size];
                    tbytes.CopyTo(bytesres, 0);
                    bytesadd.CopyTo(bytesres, tbytes.Length);
                    res = "";
                    foreach (byte b in bytesres)
                        res += b.ToString("X2");
                }
            }
            else if (_type == "AND")
            {
                sval = Value(_field, _type);
                res = Convert.ToInt32(sval).ToString("X" + (_size * 2).ToString());
            }
            else if (_type == "WEEK")
            {
                if (sval.Length > 2)
                {
                    string[] valarr = sval.Split(' ');
                    res = "";
                    foreach (string tm in valarr)
                    {
                        string[] tmarr = tm.Split(':');
                        res += Convert.ToByte(tmarr[0]).ToString("X2");
                    }
                }
                else
                {
                    res = "".PadLeft(_size * 2, '0');
                }
            }
            else
            {
                return null;//breakpoint
            }
            //
            if (_size == 2 && Regex.IsMatch(_xmltag, @"OPERATION\s*?=\s*?""SWITCHBYTES""", RegexOptions.IgnoreCase))
                res = res.Substring(res.Length - 2, 2) + res.Substring(0, 2);
            //
            return res;
        }
        private static string Value(JObject _field, string _type)
        {
            string sval = null;
            if (_field["~value"] != null)
                sval = _field["~value"].ToString();
            else if (_field["@VALUE"] != null)
                sval = _field["@VALUE"].ToString();
            else if (_field["@FORCE"] != null)
                sval = _field["@FORCE"].ToString();
            else if (Regex.IsMatch(_type, "(CHECK|SLIDER)"))
            {
                string schecked = "0";
                if (_field["@CHECKED"] != null)
                    schecked = _field["@CHECKED"].ToString();
                bool _checked = Convert.ToInt32(schecked) != 0;
                if (_checked && _field["@YESVAL"] != null)
                    sval = _field["@YESVAL"].ToString();
                else if (!_checked && _field["@NOVAL"] != null)
                    sval = _field["@NOVAL"].ToString();
                else
                    sval = "0";
            }
            else if (_type == "LIST")
                sval = "0";
            else if (_type == "INTLIST")
            {
                if (_field["@MIN"] != null)
                    sval = _field["@MIN"].ToString();
                else
                    sval = "0";
            }
            else
                sval = "0";
            return sval;
        }
        public static string WriteValue(JObject _field, string xmltag)
        {
            int size = 1;
            Match m = Regex.Match(xmltag, @"LENGTH\s*?=\s*?""(\d+?)""");
            if (m.Success)
                size = Convert.ToInt32(m.Groups[1].Value);
            else if (_field["@SIZE"] != null)
                size = Convert.ToInt32(_field["@SIZE"].ToString());
            //
            string _type = null;
            if (_field["@TYPE"] != null)
                _type = _field["@TYPE"].ToString();
            if (_type == null)
                return null;
            //
            string sval = Value(_field, _type);
            int? inc = null;
            m = Regex.Match(xmltag, @"INC\s*?=\s*?""([\d\-]+?)""");
            if (m.Success)
                inc = Convert.ToInt32(m.Groups[1].Value);
            if (inc != null)
                sval = (Convert.ToInt32(sval) + inc).ToString();
            //
            string _writeval = WriteValue(_type, size, sval, _field, xmltag);
            //
            return _writeval;
        }
        public static JObject TypeGroupToField(JObject group)
        {
            JObject res = new JObject(group);
            if (res["~type"].ToString() == "AND")
            {
                JObject fields = new JObject((JObject)res["fields"]);
                res["@TYPE"] = "FROMGROUP";
                res["PROPERTIES"] = new JObject();
                res["PROPERTIES"]["PROPERTY"] = new JObject();
                int groupval = 0x0000;
                int size = 1;
                foreach (JProperty p in fields.Properties())
                {
                    if (p.Value.Type == JTokenType.Object)
                    {
                        JObject field = (JObject)p.Value;
                        string sval = Value(field, field["@TYPE"].ToString());
                        if (sval.ToLower() == "true")
                            sval = field["@AND"].ToString();
                        else if (sval.ToLower() == "false")
                            sval = "0";
                        int ival = Convert.ToInt32(sval);
                        groupval |=  ival;
                        field["~value"] = sval;
                        if (field["@SIZE"] != null)
                            size = Convert.ToInt32(field["@SIZE"].ToString());
                        res["PROPERTIES"]["PROPERTY"][p.Name] = field;
                    }
                }
                res["@SIZE"] = size.ToString();
                res["~value"] = groupval.ToString();
                res.Remove("fields");
                res.Remove("name");
            }
            //
            return res;
        }
        public static void TypeGroupsToNewGroup(JObject groups)
        {
            Dictionary<string, JObject> typegrp = new Dictionary<string, JObject>();
            List<string> todel = new List<string>();
            foreach (JProperty p in groups.Properties())
            {
                if (p.Value.Type != JTokenType.Object)
                    continue;
                JObject grp = (JObject)p.Value;
                if (grp["~type"] != null)
                {
                    JObject field = TypeGroupToField(grp);
                    typegrp.Add(p.Name, field);
                    todel.Add(p.Name);
                }
            }
            foreach (string del in todel)
                groups.Remove(del);
            groups["~typegrpfields"] = new JObject();
            groups["~typegrpfields"]["fields"] = new JObject();
            foreach (string name in typegrp.Keys)
                groups["~typegrpfields"]["fields"][name] = typegrp[name];
        }
        #endregion

        private string _sidx = "";
        private string _panel_id = null;
        private JObject _field;

        internal cPanelField(JObject Field, string panelID, int? idx)
        {
            _field = Field;
            _panel_id = panelID;
            _idx = idx;
        }
        internal cPanelField(JObject Field, int? idx)
        {
            _field = Field;
            _idx = idx;
        }
        private int? _idx
        {
            get
            {
                if (_sidx == null || _sidx == "")
                    return null;
                Match m = Regex.Match(_sidx, @"[\w\W]+?(\d+)$");
                if (m.Success)
                    return Convert.ToInt32(m.Groups[1].Value);
                else
                    return null;
            }
            set
            {
                if (value == null || value < 0)
                {
                    _sidx = "";
                    return;
                }
                _sidx = ".~index~" + value.ToString();
            }
        }
        private string Path
        {
            get
            {
                if (_field == null || _field["~path"] == null)
                    return null;
                return _field["~path"].ToString();
            }
        }
        private string Type
        {
            get
            {
                if (_field == null || _field["@TYPE"] == null)
                    return null;
                return _field["@TYPE"].ToString().ToUpper();
            }
        }
        private string ListDefaultValue()
        {
            if (_field["ITEMS"] == null || _field["ITEMS"]["ITEM"] == null)
                return null;
            JArray jitems = (JArray)_field["ITEMS"]["ITEM"];
            int aidx = -1;
            foreach (JObject item in jitems)
            {
                aidx++;
                if (item["@DEFAULT"] == null)
                    continue;
                if (item["@DEFAULT"].ToString() == aidx.ToString())
                {
                    if (item["@VALUE"] == null)
                        return null;
                    return item["@VALUE"].ToString();
                }
            }
            return null;
        }
        private string jValue
        {
            get
            {
                if (_field == null || _field["@VALUE"] == null)
                {
                    string stype = Type;
                    if (stype == null)
                        return null;
                    if (stype == "LIST")
                        return ListDefaultValue();
                    return null;
                }
                return _field["@VALUE"].ToString();
            }
        }
    }
}
