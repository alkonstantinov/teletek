using System;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Text.RegularExpressions;
using common;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ljson
{
    internal class cPanelField
    {
        #region static
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
        internal static Dictionary<string, string> GroupPathValuesAND(JObject grp, string val, string sidx)
        {
            if (!Regex.IsMatch(sidx, "~index"))
                sidx = ".~index~" + sidx;
            if (!Regex.IsMatch(sidx, @"^\."))
                sidx = "." + sidx;
            //
            Dictionary<string, string> res = new Dictionary<string, string>();
            if (grp["fields"] != null)
                grp = (JObject)grp["fields"];
            byte bval = Convert.ToByte(val);
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
                byte band = Convert.ToByte(o["@AND"].ToString(), 16);
                byte bres = (byte)(bval & band);
                string sval = null;
                if (ftype == "CHECK")
                {
                    sval = (bres != 0) ? "1" : "0";
                }
                else
                {
                    sval = (bres != 0) ? "1" : "0";//ako ne e check
                }
                string path = o["~path"].ToString() + sidx;
                if (sval != null)
                    res.Add(path, sval);
            }
            //
            return res;
        }
        internal static Dictionary<string, string> GroupPathValues(JObject grp, string val, string sidx)
        {
            if (grp == null || grp["~type"] == null)
                return null;
            string typ = grp["~type"].ToString().ToUpper();
            if (typ == "AND")
                return GroupPathValuesAND(grp, val, sidx);
            //
            return null;
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
        #endregion

        private string _sidx = "";
        private string _panel_id;
        private JObject _field;

        internal cPanelField(JObject Field, string panelID, int? idx)
        {
            _field = Field;
            _panel_id = panelID;
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
