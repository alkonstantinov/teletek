using Newtonsoft.Json.Linq;
using ProstePrototype.POCO;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Markup;
using ljson;
using lcommunicate;
using System.Windows.Controls;
using System.IO;
using System.Text.RegularExpressions;

namespace ProstePrototype
{
    public class CallbackObjectForJs
    {
        public string showMessage(string msg)
        {//Read Note
            // MessageBox.Show(msg);
            return "Hi from " + msg;
        }
        public string getElements(string elementType)
        {
            Dictionary<string, string> dres = cComm.GetElements(cJson.CurrentPanelID, elementType);
            if (dres == null)
                return null;
            JObject o = new JObject();
            foreach (string key in dres.Keys)
            {
                JObject ok = JObject.Parse(dres[key]);
                ok.Remove("~rw");
                o[key] = ok;
            }
            File.WriteAllTextAsync("wb3.json", o.ToString());
            return o.ToString();
        }
        public string getJsonForElement(string elementType, int elementNumber)
        {
            // var e = elementType;
            //return @"{ ""pageName"": ""wb1"" }";
            string res = cComm.GetListElement(cJson.CurrentPanelID, elementType, elementNumber.ToString());
            res = cJson.GroupsWithValues(res).ToString();
            res = Regex.Replace(res, @",\s*?""~rw""[\w\W]+$", "") + "\r\n}";
            File.WriteAllTextAsync("wb3.json", res);
            return res;
        }

        public string addingElementSync(string elementType, int elementNumber)
        {
            MainWindow.AddingElement(elementType, elementNumber.ToString());
            return "added";
        }

        public string getJsonNodeForElement(string elementType, int elementNumber, string key)
        {
            string res = cComm.GetListElementNode(cJson.CurrentPanelID, elementType, elementNumber.ToString(), key);
            File.WriteAllTextAsync("wb3.json", res);
            return res;
        }

        public string getLoops(string elementType)
        {
            Dictionary<string, string> dres = cComm.GetPseudoElementsList(cJson.CurrentPanelID, elementType);
            if (dres == null)
                return null;
            JObject o = new JObject();
            foreach (string key in dres.Keys)
            {
                JObject ok = JObject.Parse(dres[key]);
                o[key] = ok["~loop_type"];
            }
            return o.ToString();
        }

        public string getLoopDevices(string elementType, int elementNumber)
        {
            Dictionary<string, string> d = cComm.GetPseudoElementDevices(cJson.CurrentPanelID, elementType, elementNumber.ToString());
            //string zdevs = zoneDevices(1);
            if (d == null)
                return null;
            //string[] akeys = new string[d.Count];
            //d.Keys.CopyTo(akeys);
            //akeys.so
            JArray res = new JArray();
            foreach (string addr in d.Keys)
            {
                JObject jdev = JObject.Parse(d[addr]);
                string devname = jdev["~device"].ToString();
                jdev.Remove("~rw");
                jdev.Remove("~device");
                jdev.Remove("~device_type");
                jdev = cJson.GroupsWithValues(jdev);
                JObject onew = new JObject();
                onew["Groups"] = jdev;
                onew["~device"] = devname;
                onew["~address"] = addr;
                res.Add(onew);
            }
            File.WriteAllTextAsync("wb3.json", res.ToString());
            return res.ToString();
        }

        public void setLoopType(string elementType/*NO_LOOP*/, int elementNumber, string typ)
        {
            JObject o = JObject.Parse(cComm.GetPseudoElement(cJson.CurrentPanelID, elementType, elementNumber.ToString()));
            o["~loop_type"] = typ;
            cComm.SetPseudoElement(cJson.CurrentPanelID, elementType, elementNumber.ToString(), o.ToString());
        }

        public string getJsonNode(string elementName, string key)
        {
            JObject jnode = new JObject(cJson.GetNode(elementName));
            if (jnode != null)
            {
                JToken t = jnode[key];
                //File.WriteAllTextAsync("wb3.json", t.ToString());
                if (t != null)
                    return t.ToString();
            }
            return null;
        }

        public string loopsInputs(string path)
        {
            JObject o = cJson.LoopsInputs(path);
            return o.ToString();
        }

        public string loopsOutputs(string path)
        {
            JObject o = cJson.LoopsOutputs(path);
            return o.ToString();
        }

        public string channelUses(string path)
        {
            JArray a = cJson.ChannelUses(path);
            return (a != null) ? a.ToString() : "[]";
        }

        public string zoneDevices(int elementNumber)
        {
            JArray zdevs = cJson.ZoneDevices(elementNumber.ToString());
            return (zdevs != null) ? zdevs.ToString() : "[]";
        }
    }
}
