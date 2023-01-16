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

        public string getJsonForElement(string elementType, int elementNumber)
        {

            // var e = elementType;
            //return @"{ ""pageName"": ""wb1"" }";
            string res = cComm.GetListElement(cJson.CurrentPanelID, elementType, elementNumber.ToString());
            res = cJson.GroupsWithValues(res).ToString();
            res = Regex.Replace(res, @",\s*?""~rw""[\w\W]+$", "") + "\r\n}";
            //File.WriteAllTextAsync("wb3.json", res);
            return res;
        }

        public string getJsonNodeForElement(string elementType, int elementNumber, string key)
        {
            string res = cComm.GetListElementNode(cJson.CurrentPanelID, elementType, elementNumber.ToString(), key);
            File.WriteAllTextAsync("wb3.json", res);
            return res;
        }

        public string getLoopDevices(string elementType/*NO_LOOP*/, int elementNumber)
        {
            return null;
        }

        public string getLoops(string elementType/*NO_LOOP*/)
        {
            return null;
        }

        public void setLoopType(string elementType/*NO_LOOP*/, int elementNumber, string type)
        {

        }

        public string getJsonNode(string elementName, string key)
        {
            JObject jnode = new JObject(cJson.GetNode(elementName));
            if (jnode != null)
            {
                JToken t = jnode[key];
                File.WriteAllTextAsync("wb3.json", t.ToString());
                if (t != null)
                    return t.ToString();
            }
            return null;
        }
    }
}
