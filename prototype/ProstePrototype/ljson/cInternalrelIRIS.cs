using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using lcommunicate;

namespace ljson
{
    internal class cInternalrelIRIS : cInternalRel
    {
        private enum eIO { Input = 1, Output = 2 };
        private void SetInputFilters(string _panel_id, JObject _node)
        {

        }
        private void SetOutputFilters(string _panel_id, JObject _node)
        {

        }
        private void SetIOFilters(string _panel_id, JObject _node, eIO io)
        {
            JObject looptab = FindObjectByNAMEProp((JToken)_node, "LOOP_DEVICE");
            if (looptab != null)
            {
                bool enabled = cComm.PseudoElementExists(_panel_id);
                looptab["~enabled"] = enabled;
                JObject sloop = FindObjectByNAMEProp((JObject)looptab, "SYSTEM SENSOR LOOP");
                if (sloop != null)
                {
                    enabled = cComm.PseudoElementExists(_panel_id, "NO_LOOP", "_LOOP");
                    sloop["~enabled"] = enabled;
                }
                JObject tteloop = FindObjectByNAMEProp((JObject)looptab, "TELETEK LOOP");
                if (tteloop != null)
                {
                    enabled = cComm.PseudoElementExists(_panel_id, "NO_LOOP", "_TTELOOP");
                    tteloop["~enabled"] = enabled;
                }
            }
        }
        
        internal override void SetNodeFilters(string _panel_id, JObject _node)
        {
            string path = _node["~path"].ToString();
            if (Regex.IsMatch(path, @"INPUT", RegexOptions.IgnoreCase))
                SetIOFilters(_panel_id, _node, eIO.Input);
            else if (Regex.IsMatch(path, @"OUTPUT", RegexOptions.IgnoreCase))
                SetIOFilters(_panel_id, _node, eIO.Output);
        }
    }
}
