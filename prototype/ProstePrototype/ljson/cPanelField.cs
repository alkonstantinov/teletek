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
        private static string WriteValue(string _type, int _size, string sval, JObject _field)
        {
            string res = null;
            if (Regex.IsMatch(_type, @"^(INT|LIST)$"))
            {
                res = Convert.ToInt32(sval).ToString("X" + (_size * 2).ToString());
                //while (_size * 2 > res.Length)
                //    res = "0" + res;
            }
            else if (Regex.IsMatch(_type, @"(CHECK)"))
            {
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
            else if (Regex.IsMatch(_type, @"(HIDDEN)"))
            {
                if (!Regex.IsMatch(sval, @"\D"))
                    res = Convert.ToInt32(sval).ToString("X" + (_size * 2).ToString());
            }
            //
            return res;
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
            string sval = null;
            if (_field["~value"] != null)
                sval = _field["~value"].ToString();
            else if (_field["@VALUE"] != null)
                sval = _field["@VALUE"].ToString();
            else if (_field["@FORCE"] != null)
                sval = _field["@FORCE"].ToString();
            else if (_type == "CHECK")
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
            else
                sval = "0";
            int? inc = null;
            m = Regex.Match(xmltag, @"INC\s*?=\s*?""(\d+?)""");
            if (m.Success)
                inc = Convert.ToInt32(m.Groups[1].Value);
            if (inc != null)
                sval = (Convert.ToInt32(sval) + inc).ToString();
            //
            string _writeval = WriteValue(_type, size, sval, _field);
            //
            return _writeval;
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
