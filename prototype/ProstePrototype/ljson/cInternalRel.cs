using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

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
        internal virtual void SetNodeFilters(string _panel_id, JObject _node) { }
    }
}
