using common;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
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
                        if (po[node_name] != null)
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
        internal virtual void SetNodeFilters(string _panel_id, JObject _node) { }
        public virtual Dictionary<string, Dictionary<string, Dictionary<string, string>>> UnionInOuts(string _panel_id, string io) { return null; }
        public virtual void FilterValueChanged(string path, string _new_val, ref bool remove_value) { }
        public virtual List<string> ChannelUsedIn(string channel_path, string myIOPath) { return null; }
        public virtual List<string> ChannelUsedIn(string channel_path) { return null; }
        public virtual Tuple<string, string> GroupPropertyVal(JObject groups, string PropertyName, byte[] val, string _xmltag) { return null; }
        public virtual bool AddSerialDevice(string key, JObject node, byte[] val, byte address, Dictionary<string, cRWProperty> read_props) { return true; }
        public virtual void AfterRead(string _panel_id, JObject panel, dGetNode _get_node, dFilterValueChanged _filter_func) { }
        public virtual void AfterDevicesChanged(string _panel_id, JObject panel, dGetNode _get_node) { }
        public virtual void ClearCache() { }
    }
}
