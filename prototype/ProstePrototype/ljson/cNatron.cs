using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.IO;
using common;
using System.ComponentModel;

namespace ljson
{
    public class cNatron : cXml
    {
        #region read/write

        #endregion

        #region common
        private static bool TypeGroupExists(JObject groups, JProperty prop)
        {
            string name = prop.Name;
            JObject field = (JObject)prop.Value;
            if (field["@TEXT"] != null) name = field["@TEXT"].ToString().ToLower();
            string ftype = "";
            if (field["@TYPE"] != null) ftype = field["@TYPE"].ToString().ToLower();
            foreach (JProperty p in groups.Properties())
                if (p.Name.ToLower() == name)
                {
                    JObject grp = (JObject)p.Value;
                    string gtype = null;
                    if (grp["~type"] != null) gtype = grp["~type"].ToString().ToLower();
                    if (gtype != null && gtype == ftype) return true;
                }
            return false;
        }
        private static void AddMissetFields(JObject groups, JObject old, string grp2add)
        {
            if (grp2add == null)
            {
                int i = 0;
                while (true)
                {
                    grp2add = "~noname" + ((i > 0) ? i.ToString() : "");
                    if (groups[grp2add] == null) break;
                    i++;
                }
                //grp2add = "~new";
                groups[grp2add] = new JObject();
                groups[grp2add]["name"] = "";
                groups[grp2add]["fields"] = new JObject();
            }
            Dictionary<string, string> found = new Dictionary<string, string>();
            Dictionary<string, string> foundbytext = new Dictionary<string, string>();
            if (groups[grp2add] == null)
                groups[grp2add] = new JObject();
            if (groups[grp2add]["fields"] == null)
                groups[grp2add]["fields"] = new JObject();
            JObject fields2add = (JObject)groups[grp2add]["fields"];
            foreach (JProperty pgrp in groups.Properties())
                if (pgrp.Value != null && pgrp.Value.Type == JTokenType.Object)
                {
                    JObject grp = (JObject)pgrp.Value;
                    if (grp["fields"] == null && !Regex.IsMatch(pgrp.Name, @"^~noname")) continue;
                    JObject fields = null;
                    if (grp["fields"] != null)
                        fields = (JObject)grp["fields"];
                    else if (Regex.IsMatch(pgrp.Name, @"^~noname"))
                        fields = grp;
                    if (fields == null) continue;
                    foreach (JProperty pfld in fields.Properties())
                    {
                        if (!found.ContainsKey(pfld.Name.ToLower()) && pfld.Value.Type == JTokenType.Object)
                            found.Add(pfld.Name.ToLower(), null);
                        JObject ofound = (JObject)pfld.Value;
                        if (ofound["@TEXT"] != null && !foundbytext.ContainsKey(ofound["@TEXT"].ToString()))
                            foundbytext.Add(ofound["@TEXT"].ToString().ToLower(), null);
                    }
                }
            foreach (JProperty pold in old.Properties())
            {
                JObject fo = (JObject)pold.Value;
                string txt = "~~~~~~~~~~~~~~~~~~~~~";
                if (fo["@TEXT"] != null) txt = fo["@TEXT"].ToString().ToLower();
                if (!found.ContainsKey(pold.Name.ToLower()) && !foundbytext.ContainsKey(txt) && !TypeGroupExists(groups, pold) && !Regex.IsMatch(pold.Name, @"^\s*?emacETH[\w\W]+?\d$"))
                    fields2add[pold.Name] = pold.Value;
            }
            if (groups[grp2add]["fields"].Count() == 0) groups.Remove(grp2add);
            else
                return;
        }
        #endregion

        #region main
        private static void ChangeContent(JObject content, JObject _pages)
        {
            TranslateObjectsKeys(content);
            //
            if (content["natron_none"] != null)
            {
                content["natron_none"]["breadcrumbs"] = _pages["natron_none"]["breadcrumbs"];
                content["natron_none"]["picture"] = _pages["natron_none"]["picture"];
                content["natron_none"]["icon"] = _pages["natron_none"]["icon"];
            }
        }
        private static void SetContentMinMax(JObject json)
        {
            JObject odev = (JObject)json["ELEMENTS"]["natron_device"];
            string min = odev["@AUTOCREATE"].ToString();
            string max = odev["@AUTOCREATECOUNT"].ToString();
            JObject ocontent = (JObject)json["ELEMENTS"]["Natron"]["CONTAINS"]["natron_device"];
            ocontent["@MIN"] = min;
            ocontent["@MAX"] = max;
        }
        #endregion

        #region device
        private static void CreateDeviceGroups(JObject json, string key)
        {
            JArray proparr = (JArray)json[key]["PROPERTIES"]["PROPERTY"];
            JObject o = Array2Object(proparr);
            //groups
            json[key]["PROPERTIES"]["Groups"] = new JObject();
            //not grouped params
            json[key]["PROPERTIES"]["Groups"]["~noname"] = new JObject();
            JObject grp = (JObject)json[key]["PROPERTIES"]["Groups"]["~noname"];
            grp["name"] = "";
            grp["fields"] = new JObject();
            foreach (JProperty p in o.Properties())
            {
                if (p.Value.Type != JTokenType.Object) continue;
                grp["fields"][p.Name] = p.Value;
            }
        }
        private static void ConvertDevice(JObject json)
        {
            foreach (JProperty p in json["ELEMENTS"]["Natron"]["CONTAINS"])
            {
                string key = p.Name.ToString();
                CreateDeviceGroups((JObject)json["ELEMENTS"], key);
            }

        }
        private static void SetDevicesList(JObject json)
        {
            JObject elements = (JObject)json["ELEMENTS"];
            json["~devtypes"] = new JObject();
            foreach (JProperty p in elements.Properties())
            {
                if (p.Value.Type != JTokenType.Object) continue;
                if (p.Name.ToLower() == "natron") continue;
                if (p.Value["PROPERTIES"] == null) continue;
                JArray props = (JArray)p.Value["PROPERTIES"]["PROPERTY"];
                JObject otype = null;
                foreach (JObject o in props)
                    if (o["@ID"] != null && o["@ID"].ToString().ToLower() == "type")
                    {
                        otype = o;
                        break;
                    }
                if (otype == null) continue;
                string key = System.Convert.ToInt32(otype["@VALUE"].ToString()).ToString();
                json["~devtypes"][key] = p.Name + ((Regex.IsMatch(p.Name, "natron_device"))? "_NONE":"");
            }
            json["~pdtypes"] = new JObject((JObject)json["~devtypes"]);
        }
        #endregion
        public static string Convert(string json, JObject _pages)
        {
            JObject o = JObject.Parse(json);
            JObject o1 = new JObject();
            foreach (JProperty p in o["ELEMENTS"])
            {
                if (p.Value.Type != JTokenType.Array)
                {
                    o1[p.Name] = p.Value;
                }
                else
                {
                    JObject elements = Array2Object((JArray)p.Value);
                    JProperty first = (JProperty)elements.First;
                    if (first != null)
                    {
                        string firstkey = first.Name;
                        JToken fo = elements[firstkey];
                        JObject ct = (JObject)fo["CONTAINS"];
                        if (ct != null)
                        {
                            first = (JProperty)ct.First;
                            JObject content = null;
                            if (first.Value.Type == JTokenType.Array)
                                content = Array2Object((JArray)first.Value);
                            else
                            {
                                JObject oc = (JObject)first.Value;
                                content = new JObject();
                                string sid = oc["@ID"].ToString();
                                oc.Remove("@ID");
                                content[sid] = oc;
                            }
                            ChangeContent(content, _pages);
                            ((JObject)fo)["CONTAINS"] = content;
                        }
                    }
                    //foreach (JToken et in )
                    //{

                    //}
                    if (elements.Count > 0)
                    {
                        TranslateObjectsKeys(elements);
                        elements["Natron"]["title"] = _pages["natron"]["title"];
                        elements["Natron"]["left"] = _pages["natron"]["left"];
                        elements["Natron"]["right"] = _pages["natron"]["right"];
                        elements["Natron"]["breadcrumbs"] = _pages["natron"]["breadcrumbs"];
                        elements["Natron"]["picture"] = _pages["natron"]["picture"];
                        elements["Natron"]["icon"] = _pages["natron"]["icon"];
                        //
                        if (p.Name != "ELEMENT")
                            o1[p.Name] = elements;
                        else
                            o1["ELEMENTS"] = elements;
                    }
                }

            }
            o1["ELEMENTS"]["natron"] = new JObject((JObject)o1["ELEMENTS"]["Natron"]);
            o1.Remove("Natron");
            ConvertDevice(o1);
            //
            Arrays2Objects(o1, true);
            SetDevicesList(o1);
            SetContentMinMax(o1);
            //
            doINC(o1);
            //
            return o1.ToString();
        }
    }
}
