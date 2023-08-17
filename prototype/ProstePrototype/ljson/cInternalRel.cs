using common;
using lcommunicate;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Input;
using System.Xaml.Schema;

namespace ljson
{
    internal class cInternalRel
    {
        internal JObject FindObjectByNAMEProp(JToken _node, string _name)
        {
            if (_node.Type == JTokenType.Object)
            {
                JObject o = (JObject)_node;
                JToken tn = o["@NAME"];
                if (tn != null && tn.ToString().ToLower() == _name.ToLower())
                    return o;
                foreach (JToken t in _node)
                {
                    JObject res = FindObjectByNAMEProp(t, _name);
                    if (res != null)
                        return res;
                }
            }
            else if (_node.Type == JTokenType.Array)
                foreach (JToken t in _node)
                {
                    JObject res = FindObjectByNAMEProp(t, _name);
                    if (res != null)
                        return res;
                }
            else if (_node.Type == JTokenType.Property)
            {
                return FindObjectByNAMEProp(((JProperty)_node).Value, _name);
            }
            return null;
        }
        internal JObject FindObjectByProperty(JToken _node, string prop_name, string val)
        {
            if (_node.Type == JTokenType.Object)
            {
                JObject o = (JObject)_node;
                JToken tn = o[prop_name];
                if (tn != null && tn.ToString().ToLower() == val.ToLower())
                    return o;
                foreach (JToken t in _node)
                {
                    JObject res = FindObjectByProperty(t, prop_name, val);
                    if (res != null)
                        return res;
                }
            }
            else if (_node.Type == JTokenType.Array)
                foreach (JToken t in _node)
                {
                    JObject res = FindObjectByProperty(t, prop_name, val);
                    if (res != null)
                        return res;
                }
            else if (_node.Type == JTokenType.Property)
            {
                return FindObjectByProperty(((JProperty)_node).Value, prop_name, val);
            }
            return null;
        }
        internal JObject FindObjectByProperty(JToken _node, string prop_name, string val, bool check_value_exists)
        {
            if (_node.Type == JTokenType.Object)
            {
                JObject o = (JObject)_node;
                JToken tn = o[prop_name];
                if (tn != null && tn.ToString().ToLower() == val.ToLower() && (!check_value_exists || o["~value"] != null))
                    return o;
                foreach (JToken t in _node)
                {
                    JObject res = FindObjectByProperty(t, prop_name, val, check_value_exists);
                    if (res != null)
                        return res;
                }
            }
            else if (_node.Type == JTokenType.Array)
                foreach (JToken t in _node)
                {
                    JObject res = FindObjectByProperty(t, prop_name, val, check_value_exists);
                    if (res != null)
                        return res;
                }
            else if (_node.Type == JTokenType.Property)
            {
                return FindObjectByProperty(((JProperty)_node).Value, prop_name, val, check_value_exists);
            }
            return null;
        }
        internal JObject GetDeviceGroupsNode(string json, string node_name)
        {
            JObject o = JObject.Parse(json);
            foreach (JProperty p in (JToken)o)
            {
                if (p.Value.Type == JTokenType.Object)
                {
                    JObject go = (JObject)p.Value;
                    if (go["fields"] != null)
                    {
                        JObject po = (JObject)go["fields"];
                        if (po[node_name] != null && po[node_name].Type == JTokenType.Object)
                        {
                            JObject res = (JObject)po[node_name];
                            return res;
                        }
                        else
                            foreach (JProperty fp in (JToken)po)
                                if (fp.Name.ToLower() == node_name.ToLower() && fp.Value.Type == JTokenType.Object)
                                {
                                    JObject res = (JObject)fp.Value;
                                    return res;
                                }
                    }
                    else if (go[node_name] != null)
                    {
                        JObject res = (JObject)go[node_name];
                        return res;
                    }
                    else
                        foreach (JProperty fp in (JToken)go)
                            if (fp.Name.ToLower() == node_name.ToLower() && fp.Value.Type == JTokenType.Object)
                            {
                                JObject res = (JObject)fp.Value;
                                return res;
                            }
                }
            }
            //if (node_name == "P1")
            return FindObjectByProperty(JObject.Parse(json), "@ID", node_name);
            //else
            //return null;
        }
        internal JObject GetDeviceGroupsNode(string json, string node_name, bool check_value_exists)
        {
            JObject o = JObject.Parse(json);
            foreach (JProperty p in (JToken)o)
            {
                if (p.Value.Type == JTokenType.Object)
                {
                    JObject go = (JObject)p.Value;
                    if (go["fields"] != null)
                    {
                        JObject po = (JObject)go["fields"];
                        if (po[node_name] != null && po[node_name].Type != JTokenType.Null && (!check_value_exists || po[node_name]["~value"] != null))
                        {
                            JObject res = (JObject)po[node_name];
                            return res;
                        }
                        else
                            foreach (JProperty fp in (JToken)po)
                                if (fp.Name.ToLower() == node_name.ToLower() && fp.Value.Type == JTokenType.Object)
                                {
                                    JObject res = (JObject)fp.Value;
                                    if (!check_value_exists || res["~value"] != null)
                                        return res;
                                }
                    }
                    else if (go[node_name] != null)
                    {
                        JObject res = (JObject)go[node_name];
                        if (!check_value_exists || res["~value"] != null)
                            return res;
                    }
                    else
                        foreach (JProperty fp in (JToken)go)
                            if (fp.Name.ToLower() == node_name.ToLower() && fp.Value.Type == JTokenType.Object)
                            {
                                JObject res = (JObject)fp.Value;
                                if (!check_value_exists || res["~value"] != null)
                                    return res;
                            }
                }
            }
            //if (node_name == "P1")
            return FindObjectByProperty(JObject.Parse(json), "@ID", node_name, check_value_exists);
            //else
            //return null;
        }
        internal string GetWeekValue(byte[] bval)
        {
            string res = "";
            for (int i = 0; i < 14; i++)
            {
                string sh = bval[i * 2].ToString().PadLeft(2, '0');
                string sm = bval[i * 2 + 1].ToString().PadLeft(2, '0');
                res += ((res != "") ? " " : "") + sh + ":" + sm;
            }
            return res;
        }
        internal JObject FindContentFromElement(JObject panel, string element_name)
        {
            string s = ElementNameFromRWPath(panel, element_name);
            if (s != null)
                element_name = s;
            JObject content_element = null;
            JObject elements = (JObject)panel["ELEMENTS"];
            foreach (JProperty pel in elements.Properties())
            {
                if (pel.Value == null || pel.Value.Type != JTokenType.Object)
                    continue;
                JObject oel = (JObject)pel.Value;
                if (oel["CONTAINS"] == null)
                    continue;
                JObject content = (JObject)oel["CONTAINS"];
                foreach (JProperty pc in content.Properties())
                {
                    if (pc.Name.ToUpper() == element_name.ToUpper() && pc.Value.Type == JTokenType.Object)
                    {
                        content_element = (JObject)pc.Value;
                        break;
                    }
                }
                if (content_element != null)
                    break;
            }
            return content_element;
        }
        internal string ElementNameFromRWPath(JObject panel, string rwkey)
        {
            string[] analyse_key = rwkey.Split('/');
            string res = analyse_key[analyse_key.Length - 1];
            JObject elements = (JObject)panel["ELEMENTS"];
            JToken element = elements[res];
            if (element == null && analyse_key.Length == 1)
            {
                res = Regex.Replace(analyse_key[analyse_key.Length - 1], "S$", "", RegexOptions.IgnoreCase);
                element = elements[res];
            }
            if (element == null && analyse_key.Length == 1)
            {
                res = Regex.Replace(analyse_key[analyse_key.Length - 1], "S_", "_", RegexOptions.IgnoreCase);
                element = elements[res];
            }
            if (element == null && analyse_key.Length == 1)
            {
                string key1 = Regex.Replace(analyse_key[analyse_key.Length - 1], "S$", "", RegexOptions.IgnoreCase);
                res = Regex.Replace(key1, "S_", "_", RegexOptions.IgnoreCase);
                element = elements[res];
            }
            if (element == null && analyse_key.Length == 1)
            {
                string key1 = Regex.Replace(analyse_key[analyse_key.Length - 1], "S$", "", RegexOptions.IgnoreCase);
                Match m = Regex.Match(key1, @"^(\w+?_)");
                if (m.Success)
                {
                    string panel_prefix = m.Groups[1].Value;
                    key1 = Regex.Replace(key1, "^" + panel_prefix, "");
                    res = panel_prefix + Regex.Replace(key1, "S_", "_", RegexOptions.IgnoreCase);
                    element = elements[res];
                }
            }
            if (element != null)
                return res;
            else
                return null;
        }

        #region virtual
        internal virtual void SetNodeFilters(string _panel_id, JObject _node) { }
        public virtual Dictionary<string, Dictionary<string, Dictionary<string, string>>> UnionInOuts(string _panel_id, string io) { return null; }
        public virtual void FilterValueChanged(string path, string _new_val, ref bool remove_value) { }
        public virtual List<string> ChannelUsedIn(string channel_path, string myIOPath) { return null; }
        public virtual List<string> ChannelUsedIn(string channel_path) { return null; }
        public virtual Tuple<string, string> GroupPropertyVal(string _panel_id, JObject groups, string PropertyName, byte[] val, string _xmltag) { return null; }
        public virtual string WritePropertyVal(JObject groups, string PropertyName, string _xmltag) { return null; }
        public virtual bool AddSerialDevice(string key, JObject node, byte[] val, byte address, Dictionary<string, cRWProperty> read_props) { return true; }
        public virtual void AfterRead(string _panel_id, JObject panel, dGetNode _get_node, dFilterValueChanged _filter_func) { }
        public virtual void AfterDevicesChanged(string _panel_id, JObject panel, dGetNode _get_node) { }
        public virtual void AfterDeviceRemoved(string _panel_id, JObject panel, string loop_type, string dev_addr) { }
        public virtual void AfterInputRemoved(string _panel_id) { }
        public virtual void ClearCache() { }
        public virtual JObject Data2Save() { return null; }
        public virtual void Load(JObject o) { }
        public virtual void RemoveTABCache(string tab, string idx) { }
        public virtual void OnElementAddressChanged(string oldAddress, string elementType, string newAddress) { }
        public virtual void OnDeviceAddressChanged(string oldAddress, string loopType, string newAddress) { }
        public virtual void RemoveUnusedTabs(JObject json) { }
        public virtual string CurrentPanelID { get; set; }
        public virtual string FindElementKey(string _searching, JObject _panel) { return _searching; }
        #endregion
    }
}
