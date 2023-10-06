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
    public class cIRIS : cXml
    {
        #region read/write

        #endregion

        #region common
        private static string TranslateKey(string name)
        {
            if (Regex.IsMatch(name, @"iris[\w\W]*?_accesscode$", RegexOptions.IgnoreCase))
                name = "iris_access_code";
            else if (Regex.IsMatch(name, @"tftr[\w\W]*?_accesscode$", RegexOptions.IgnoreCase))
                name = "iris_access_code";
            else if (Regex.IsMatch(name, @"IRIS[\w\W]*?_NETWORK$", RegexOptions.IgnoreCase))
                name = "iris_network";
            else if (Regex.IsMatch(name, @"SIMPO[\w\W]*?_NETWORK_R$", RegexOptions.IgnoreCase))
                name = "iris_network";
            else if (Regex.IsMatch(name, @"SIMPO[\w\W]*?_NETWORK$", RegexOptions.IgnoreCase))
                name = "iris_network";
            else if (Regex.IsMatch(name, @"TFTR[\w\W]*?_NETWORK$", RegexOptions.IgnoreCase))
                name = "iris_network";
            else if (Regex.IsMatch(name, @"IRIS[\w\W]*?_PANELSINNETWORK$", RegexOptions.IgnoreCase))
                name = "iris_panels_in_network";
            else if (Regex.IsMatch(name, @"SIMPO[\w\W]*?_PANELS_R$", RegexOptions.IgnoreCase))
                name = "iris_panels_in_network";
            //else if (Regex.IsMatch(name, @"SIMPO[\w\W]*?_PANELS$", RegexOptions.IgnoreCase))
            //    name = "iris_panels_in_network";
            else if (Regex.IsMatch(name, @"TFTR[\w\W]*?_PANELSINNETWORK$", RegexOptions.IgnoreCase))
                name = "iris_panels_in_network";
            else if (Regex.IsMatch(name, @"IRIS[\w\W]*?_INPUTS$", RegexOptions.IgnoreCase))
                name = "iris_inputs";
            else if (Regex.IsMatch(name, @"INPUTS[\w\W]*?_GROUP$", RegexOptions.IgnoreCase))
                name = "iris_inputs_group";
            else if (Regex.IsMatch(name, @"IRIS[\w\W]*?_OUTPUTS$", RegexOptions.IgnoreCase))
                name = "iris_outputs";
            else if (Regex.IsMatch(name, @"FAT[\w\W]*?_FBF$", RegexOptions.IgnoreCase))
                name = "iris_fat_fbf";
            else if (Regex.IsMatch(name, @"IRIS[\w\W]*?_ZONES$", RegexOptions.IgnoreCase))
                name = "iris_zones";
            else if (Regex.IsMatch(name, @"SIMPO[\w\W]*?_ZONES$", RegexOptions.IgnoreCase))
                name = "iris_zones";
            else if (Regex.IsMatch(name, @"IRIS[\w\W]*?_EVAC_ZONES_GROUPS$", RegexOptions.IgnoreCase))
                name = "iris_evac_zones";
            else if (Regex.IsMatch(name, @"SIMPO[\w\W]*?_EVAC_ZONES_GROUPS$", RegexOptions.IgnoreCase))
                name = "iris_evac_zones";
            else if (Regex.IsMatch(name, @"IRIS[\w\W]*?_PERIPHERIALDEVICES$", RegexOptions.IgnoreCase))
                name = "iris_peripheral_devices";
            else if (Regex.IsMatch(name, @"IRIS[\w\W]*?_LOOPDEVICES$", RegexOptions.IgnoreCase))
                name = "iris_loop_devices";
            else if (Regex.IsMatch(name, @"SIMPO[\w\W]*?_LOOPDEVICES$", RegexOptions.IgnoreCase))
                name = "iris_loop_devices";
            else if (Regex.IsMatch(name, @"IRIS[\w\W]*?_PANEL$", RegexOptions.IgnoreCase))
                name = "iris";
            else if (Regex.IsMatch(name, @"^R_PANEL$", RegexOptions.IgnoreCase))
                name = "iris";
            else if (Regex.IsMatch(name, @"^SIMPO_PANEL$", RegexOptions.IgnoreCase))
                name = "iris";
            else if (Regex.IsMatch(name, @"^TFTR_PANEL$", RegexOptions.IgnoreCase))
                name = "iris";
            return name;
        }

        private static void TranslateObjectsKeys(JObject content)
        {
            bool isRepeater = Regex.IsMatch(content.ToString(), @"""@PRODUCTNAME""\s*?:\s*?""REPEATER\s+?Iris/Simpo", RegexOptions.IgnoreCase);
            List<JToken> from = new List<JToken>();
            List<JToken> to = new List<JToken>();
            foreach (JToken t in (JToken)content)
            {
                string name = cIRIS.TranslateKey(((JProperty)t).Name);
                if (isRepeater && Regex.IsMatch(name, @"panels[\w\W]+?in[\w\W]+?network$", RegexOptions.IgnoreCase))
                    name = ((JProperty)t).Name;
                JProperty p = new JProperty(name, ((JProperty)t).Value);
                from.Add(t);
                to.Add((JToken)p);
            }
            for (int i = 0; i < from.Count; i++)
                from[i].Replace(to[i]);
        }
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
                        if (pfld.Value.Type != JTokenType.Object) continue;
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
            if (content["iris_access_code"] != null)
            {
                content["iris_access_code"]["title"] = _pages["iris_access_code"]["title"];//"Access codes";
                content["iris_access_code"]["left"] = _pages["iris_access_code"]["left"]; //"IRIS/divIRIS.html";
                content["iris_access_code"]["right"] = _pages["iris_access_code"]["right"];// "IRIS/access.html";
                content["iris_access_code"]["breadcrumbs"] = _pages["iris_access_code"]["breadcrumbs"];// JArray.Parse("[\"index\", \"iris\"]");
            }
            //
            content["iris_network"]["title"] = _pages["iris_network"]["title"];// "Network";
            content["iris_network"]["left"] = _pages["iris_network"]["left"];// "IRIS/divIRIS.html";
            content["iris_network"]["right"] = _pages["iris_network"]["right"];// "IRIS/network.html";
            content["iris_network"]["breadcrumbs"] = _pages["iris_network"]["breadcrumbs"];// JArray.Parse("[\"index\", \"iris\"]");
            //
            if (content["iris_panels_in_network"] != null)
            {
                content["iris_panels_in_network"]["@MIN"] = "1";
                content["iris_panels_in_network"]["@MAX"] = "1";
                content["iris_panels_in_network"]["title"] = _pages["iris_panels_in_network"]["title"];// "Panels in network";
                content["iris_panels_in_network"]["left"] = _pages["iris_panels_in_network"]["left"];//"IRIS/divIRIS.html";
                content["iris_panels_in_network"]["right"] = _pages["iris_panels_in_network"]["right"];//"IRIS/panels_in_network.html";
                content["iris_panels_in_network"]["breadcrumbs"] = _pages["iris_panels_in_network"]["breadcrumbs"];//JArray.Parse("[\"index\", \"iris\"]");
            }
            //
            if (content["iris_inputs"] != null)
            {
                content["iris_inputs"]["title"] = _pages["iris_inputs"]["title"];// "Inputs";
                content["iris_inputs"]["left"] = _pages["iris_inputs"]["left"];//"IRIS/divIRIS.html";
                content["iris_inputs"]["right"] = _pages["iris_inputs"]["right"];//"IRIS/input.html";
                content["iris_inputs"]["breadcrumbs"] = _pages["iris_inputs"]["breadcrumbs"];//JArray.Parse("[\"index\", \"iris\"]");
            }
            //
            if (content["iris_inputs_group"] != null)
            {
                content["iris_inputs_group"]["title"] = _pages["iris_inputs_group"]["title"];// "Input Groups";
                content["iris_inputs_group"]["left"] = _pages["iris_inputs_group"]["left"];//"IRIS/divIRIS.html";
                content["iris_inputs_group"]["right"] = _pages["iris_inputs_group"]["right"];//"IRIS/inputs_group.html";
                content["iris_inputs_group"]["breadcrumbs"] = _pages["iris_inputs_group"]["breadcrumbs"];//JArray.Parse("[\"index\", \"iris\"]");
            }
            //
            if (content["iris_outputs"] != null)
            {
                content["iris_outputs"]["title"] = _pages["iris_outputs"]["title"];// "Output";
                content["iris_outputs"]["left"] = _pages["iris_outputs"]["left"];//"IRIS/divIRIS.html";
                content["iris_outputs"]["right"] = _pages["iris_outputs"]["right"];//"IRIS/output.html";
                content["iris_outputs"]["breadcrumbs"] = _pages["iris_outputs"]["breadcrumbs"];//JArray.Parse("[\"index\", \"iris\"]");
            }
            //
            if (content["iris_fat_fbf"] != null)
            {
                content["iris_fat_fbf"]["title"] = _pages["iris_fat_fbf"]["title"];// "FAT FBF";
                content["iris_fat_fbf"]["left"] = _pages["iris_fat_fbf"]["left"];//"IRIS/divIRIS.html";
                content["iris_fat_fbf"]["right"] = _pages["iris_fat_fbf"]["right"];//"IRIS/fat-fbf.html";
                content["iris_fat_fbf"]["breadcrumbs"] = _pages["iris_fat_fbf"]["breadcrumbs"];//JArray.Parse("[\"index\", \"iris\"]");
            }
            //
            if (content["iris_zones"] != null)
            {
                content["iris_zones"]["title"] = _pages["iris_zones"]["title"];// "Zones";
                content["iris_zones"]["left"] = _pages["iris_zones"]["left"];//"IRIS/divIRIS.html";
                content["iris_zones"]["right"] = _pages["iris_zones"]["right"];//"IRIS/zone.html";
                content["iris_zones"]["breadcrumbs"] = _pages["iris_zones"]["breadcrumbs"];//JArray.Parse("[\"index\", \"iris\"]");
            }
            //
            if (content["iris_evac_zones"] != null)
            {
                content["iris_evac_zones"]["title"] = _pages["iris_evac_zones"]["title"];// "Evac zones";
                content["iris_evac_zones"]["left"] = _pages["iris_evac_zones"]["left"];//"IRIS/divIRIS.html";
                content["iris_evac_zones"]["right"] = _pages["iris_evac_zones"]["right"];//"IRIS/zone_evac.html";
                content["iris_evac_zones"]["breadcrumbs"] = _pages["iris_evac_zones"]["breadcrumbs"];//JArray.Parse("[\"index\", \"iris\"]");
            }
            //
            if (content["iris_peripheral_devices"] != null)
            {
                content["iris_peripheral_devices"]["title"] = _pages["iris_peripheral_devices"]["title"];// "Peripheral devices";
                content["iris_peripheral_devices"]["left"] = _pages["iris_peripheral_devices"]["left"];//"IRIS/divIRIS.html";
                content["iris_peripheral_devices"]["right"] = _pages["iris_peripheral_devices"]["right"];//"IRIS/periph_devices.html";
                content["iris_peripheral_devices"]["breadcrumbs"] = _pages["iris_peripheral_devices"]["breadcrumbs"];//JArray.Parse("[\"index\", \"iris\"]");
            }
            //
            if (content["iris_loop_devices"] != null)
            {
                content["iris_loop_devices"]["title"] = _pages["iris_loop_devices"]["title"];// "Loop devices";
                content["iris_loop_devices"]["left"] = _pages["iris_loop_devices"]["left"];//"IRIS/divIRIS.html";
                content["iris_loop_devices"]["right"] = _pages["iris_loop_devices"]["right"];//"IRIS/loop_devices.html";
                content["iris_loop_devices"]["breadcrumbs"] = _pages["iris_loop_devices"]["breadcrumbs"];//JArray.Parse("[\"index\", \"iris\"]");
            }
            if (content["SIMPO_PANELOUTPUTS"] != null)
            {
                content["SIMPO_PANELOUTPUTS"]["title"] = _pages["simpo_paneloutputs"]["title"];
                content["SIMPO_PANELOUTPUTS"]["left"] = _pages["simpo_paneloutputs"]["left"];
                content["SIMPO_PANELOUTPUTS"]["right"] = _pages["simpo_paneloutputs"]["right"];
                content["SIMPO_PANELOUTPUTS"]["breadcrumbs"] = _pages["simpo_paneloutputs"]["breadcrumbs"];
            }
            if (content["SIMPO_MIMICPANELS"] != null)
            {
                content["SIMPO_MIMICPANELS"]["title"] = _pages["simpo_mimicpanels"]["title"];
                content["SIMPO_MIMICPANELS"]["left"] = _pages["simpo_mimicpanels"]["left"];
                content["SIMPO_MIMICPANELS"]["right"] = _pages["simpo_mimicpanels"]["right"];
                content["SIMPO_MIMICPANELS"]["breadcrumbs"] = _pages["simpo_mimicpanels"]["breadcrumbs"];
            }
        }
        private static void CreateMainGroupsSimpoRepeater(JObject json)
        {
            JArray proparr = (JArray)json["iris"]["PROPERTIES"]["PROPERTY"];
            JObject o = Array2Object(proparr);
            //groups
            json["iris"]["PROPERTIES"]["Groups"] = new JObject();
            //
            JObject grp1 = new JObject();
            grp1["name"] = "Panel Settings";
            JObject f1 = new JObject();
            f1["Language"] = o["Language"];
            f1["BRIGHTNESS_R"] = o["BRIGHTNESS_R"];
            f1["AUTOLOGOFF"] = o["AUTOLOGOFF"];
            f1["LOGOFFENABLED"] = o["LOGOFFENABLED"];
            grp1["fields"] = f1;
            json["iris"]["PROPERTIES"]["Groups"]["PanelSettings"] = grp1;
            //
            JObject grp2 = new JObject();
            grp2["name"] = "Access Codes";
            JObject f2 = new JObject();
            f2["Code1"] = o["Code1"];
            f2["Code2"] = o["Code2"];
            grp2["fields"] = f2;
            json["iris"]["PROPERTIES"]["Groups"]["AccessCodes"] = grp2;
            //
            JObject grp3 = new JObject();
            grp3["name"] = "Company Logo";
            JObject f3 = new JObject();
            f3["LOGO1"] = o["LOGO1"];
            f3["LOGO2"] = o["LOGO2"];
            grp3["fields"] = f3;
            json["iris"]["PROPERTIES"]["Groups"]["CompanyLogo"] = grp3;
            //
            json["iris"]["PROPERTIES"]["OLD"] = o;
        }
        private static void CreateMainGroupsSimpoPanel(JObject json)
        {
            JArray proparr = (JArray)json["iris"]["PROPERTIES"]["PROPERTY"];
            JObject o = Array2Object(proparr);
            //groups
            json["iris"]["PROPERTIES"]["Groups"] = new JObject();
            //
            JObject grp1 = new JObject();
            grp1["name"] = "Panel Settings";
            JObject f1 = new JObject();
            f1["Language"] = o["Language"];
            f1["BRIGHTNESS"] = o["BRIGHTNESS"];
            f1["AUTOLOGOFF"] = o["AUTOLOGOFF"];
            f1["LOGOFFENABLED"] = o["LOGOFFENABLED"];
            grp1["fields"] = f1;
            json["iris"]["PROPERTIES"]["Groups"]["PanelSettings"] = grp1;
            //
            JObject grp2 = new JObject();
            grp2["name"] = "Access Codes";
            JObject f2 = new JObject();
            f2["Code1"] = o["Code1"];
            f2["Code2"] = o["Code2"];
            grp2["fields"] = f2;
            json["iris"]["PROPERTIES"]["Groups"]["AccessCodes"] = grp2;
            //
            JObject grp3 = new JObject();
            grp3["name"] = "Day/Night";
            JObject f3 = new JObject();
            f3["DayNightSchedule"] = o["DayNightSchedule"];
            f3["DayMode"] = o["DayMode"];
            grp3["fields"] = f3;
            json["iris"]["PROPERTIES"]["Groups"]["DayNight"] = grp3;
            //
            JObject grp4 = new JObject();
            grp4["name"] = "Delay(T1)";
            JObject f4 = new JObject();
            f4["T1DELAY"] = o["T1DELAY"];
            grp4["fields"] = f4;
            json["iris"]["PROPERTIES"]["Groups"]["DelayT1"] = grp4;
            //
            JObject grp5 = new JObject();
            grp5["name"] = "Sounders mode";
            JObject f5 = new JObject();
            f5["SoundersMode"] = o["SoundersMode"];
            f5["ClassChangeTone"] = o["ClassChangeTone"];
            f5["ALARMTONE"] = o["ALARMTONE"];
            f5["ALARMTONESETTINGS"] = o["ALARMTONESETTINGS"];
            f5["EVACUATIONTONE"] = o["EVACUATIONTONE"];
            f5["EVACUATETIMEOUT"] = o["EVACUATETIMEOUT"];
            f5["VECInverted"] = o["VECInverted"];
            f5["EVACUATECYCLE_OFF"] = o["EVACUATECYCLE_OFF"];
            f5["EVACUATECYCLE_ON"] = o["EVACUATECYCLE_ON"];
            f5["EVACUATESETTINGS"] = o["EVACUATESETTINGS"];
            grp5["fields"] = f5;
            json["iris"]["PROPERTIES"]["Groups"]["SoundersMode"] = grp5;
            //
            JObject grp6 = new JObject();
            grp6["name"] = "Company Logo";
            JObject f6 = new JObject();
            f6["LOGO1"] = o["LOGO1"];
            f6["LOGO2"] = o["LOGO2"];
            grp6["fields"] = f6;
            json["iris"]["PROPERTIES"]["Groups"]["CompanyLogo"] = grp6;
            //
            json["iris"]["PROPERTIES"]["OLD"] = o;
        }
        private static void CreateMainGroupsTFTRepeater(JObject json)
        {
            JArray proparr = (JArray)json["iris"]["PROPERTIES"]["PROPERTY"];
            JObject o = Array2Object(proparr);
            //groups
            json["iris"]["PROPERTIES"]["Groups"] = new JObject();
            //
            JObject grp1 = new JObject();
            grp1["name"] = "Auto Log Off";
            JObject f1 = new JObject();
            f1["AUTOLOGOFFENABLED"] = o["AUTOLOGOFFENABLED"];
            f1["TIMEAUTOLOGOFFINSTALLER"] = o["TIMEAUTOLOGOFFINSTALLER"];
            grp1["fields"] = f1;
            json["iris"]["PROPERTIES"]["Groups"]["AutoLogOff"] = grp1;
            //
            JObject grp2 = new JObject();
            grp2["name"] = "Company Info";
            JObject f2 = new JObject();
            f2["LOGO1"] = o["LOGO1"];
            f2["LOGO2"] = o["LOGO2"];
            f2["LOGO3"] = o["LOGO3"];
            f2["LOGO4"] = o["LOGO4"];
            grp2["fields"] = f2;
            json["iris"]["PROPERTIES"]["Groups"]["CompanyInfo"] = grp2;
            //
            json["iris"]["PROPERTIES"]["OLD"] = o;
            AddMissetFields((JObject)json["iris"]["PROPERTIES"]["Groups"], o, "Parameters");
        }
        private static void CreateMainGroups(JObject json)
        {
            if (Regex.IsMatch(json["iris"]["@PRODUCTNAME"].ToString(), @"REPEATER\s+?Iris[\w\W]+?Simpo$", RegexOptions.IgnoreCase))
            {
                CreateMainGroupsSimpoRepeater(json);
                return;
            }
            if (Regex.IsMatch(json["iris"]["@PRODUCTNAME"].ToString(), @"Simpo[\w\W]+?panel\s*$", RegexOptions.IgnoreCase))
            {
                CreateMainGroupsSimpoPanel(json);
                return;
            }
            if (Regex.IsMatch(json["iris"]["@PRODUCTNAME"].ToString(), @"TFT[\w\W]+?Repeater[\w\W]+?panel\s*$", RegexOptions.IgnoreCase))
            {
                CreateMainGroupsTFTRepeater(json);
                return;
            }
            JArray proparr = (JArray)json["iris"]["PROPERTIES"]["PROPERTY"];
            JObject o = Array2Object(proparr);
            //groups
            json["iris"]["PROPERTIES"]["Groups"] = new JObject();
            //
            JObject grp1 = new JObject();
            grp1["name"] = "Delay Mode";
            JObject f1 = new JObject();
            f1["SounderDelayMode"] = o["SounderDelayMode"];
            f1["SounderDelaySchedule"] = o["SounderDelaySchedule"];
            JArray jarr = (JArray)f1["SounderDelayMode"]["ITEMS"]["ITEM"];
            foreach (JToken t in jarr)
                if (t.Type == JTokenType.Object && ((JObject)t)["@NAME"].ToString() == "SCHEDULE")
                    ((JObject)t)["ScheduleKey"] = "SounderDelaySchedule";
            f1["FireBrigadeDelayMode"] = o["FireBrigadeDelayMode"];
            f1["FireBrigadeDelaySchedule"] = o["FireBrigadeDelaySchedule"];
            jarr = (JArray)f1["FireBrigadeDelayMode"]["ITEMS"]["ITEM"];
            foreach (JToken t in jarr)
                if (t.Type == JTokenType.Object && ((JObject)t)["@NAME"].ToString() == "SCHEDULE")
                    ((JObject)t)["ScheduleKey"] = "FireBrigadeDelaySchedule";
            f1["FireProtectionDelayMode"] = o["FireProtectionDelayMode"];
            f1["FireProtectionDelaySchedule"] = o["FireProtectionDelaySchedule"];
            jarr = (JArray)f1["FireProtectionDelayMode"]["ITEMS"]["ITEM"];
            foreach (JToken t in jarr)
                if (t.Type == JTokenType.Object && ((JObject)t)["@NAME"].ToString() == "SCHEDULE")
                    ((JObject)t)["ScheduleKey"] = "FireProtectionDelaySchedule";
            f1["DayMode"] = o["DayMode"];
            f1["DayNightSchedule"] = o["DayNightSchedule"];
            jarr = (JArray)f1["DayMode"]["ITEMS"]["ITEM"];
            foreach (JToken t in jarr)
                if (t.Type == JTokenType.Object && ((JObject)t)["@NAME"].ToString() == "SCHEDULE")
                    ((JObject)t)["ScheduleKey"] = "DayNightSchedule";
            grp1["fields"] = f1;
            json["iris"]["PROPERTIES"]["Groups"]["DelayMode"] = grp1;
            //
            JObject grp2 = new JObject();
            grp2["name"] = "Parameters";
            JObject f2 = new JObject();
            f2["SoundersMode"] = o["SoundersMode"];
            f2["CallPointMode"] = o["CallPointMode"];
            f2["PRINTER"] = o["Protocol"];
            if (f2["PRINTER"] == null || f2["PRINTER"].Type == JTokenType.Null)
                f2["PRINTER"] = o["PRINTER"];
            f2["T1DELAY"] = o["T1DELAY"];
            f2["EVACUATION_TIMEOUT"] = o["EVACUATION_TIMEOUT"];
            grp2["fields"] = f2;
            json["iris"]["PROPERTIES"]["Groups"]["Parameters"] = grp2;
            //
            JObject grp3 = new JObject();
            grp3["name"] = "Tone Settings";
            JObject f3 = new JObject();
            f3["TONEALARM"] = o["TONEALARM"];
            f3["TONEEVACUATE"] = o["TONEEVACUATE"];
            f3["TONECLASSCHANGE"] = o["TONECLASSCHANGE"];
            grp3["fields"] = f3;
            json["iris"]["PROPERTIES"]["Groups"]["ToneSettings"] = grp3;
            //
            JObject grp4 = new JObject();
            grp4["name"] = "Auto Log Off";
            JObject f4 = new JObject();
            f4["AUTOLOGOFFENABLED"] = o["AUTOLOGOFFENABLED"];
            f4["TIMEAUTOLOGOFFINSTALLER"] = o["TIMEAUTOLOGOFFINSTALLER"];
            grp4["fields"] = f4;
            json["iris"]["PROPERTIES"]["Groups"]["AutoLogOff"] = grp4;
            //
            JObject grp5 = new JObject();
            grp5["name"] = "Settings";
            JObject f5 = new JObject();
            f5["ALARM_SETTINGS"] = o["ALARM_SETTINGS"];
            f5["EVACUATE_SETTINGS"] = o["EVACUATE_SETTINGS"];
            grp5["fields"] = f5;
            json["iris"]["PROPERTIES"]["Groups"]["Settings"] = grp5;
            //
            JObject grp6 = new JObject();
            grp6["name"] = "Alert/EVAC Voice Cycle";
            JObject f6 = new JObject();
            f6["EVACUATECYCLE_ON"] = o["EVACUATECYCLE_ON"];
            f6["EVACUATECYCLE_OFF"] = o["EVACUATECYCLE_OFF"];
            f6["EVACUATIONCYCLE INVERTED"] = o["EVACUATIONCYCLE INVERTED"];
            f6["SOUNDERS_RESOUND"] = o["SOUNDERS_RESOUND"];
            grp6["fields"] = f6;
            json["iris"]["PROPERTIES"]["Groups"]["AlertEVACVoiceCycle"] = grp6;
            //
            JObject grp7 = new JObject();
            grp7["name"] = "Company Info";
            JObject f7 = new JObject();
            f7["LOGO1"] = o["LOGO1"];
            f7["LOGO2"] = o["LOGO2"];
            f7["LOGO3"] = o["LOGO3"];
            f7["LOGO4"] = o["LOGO4"];
            grp7["fields"] = f7;
            json["iris"]["PROPERTIES"]["Groups"]["CompanyInfo"] = grp7;
            //
            json["iris"]["PROPERTIES"]["OLD"] = o;
            AddMissetFields((JObject)json["iris"]["PROPERTIES"]["Groups"], o, "Parameters");
        }

        private static void ConvertMainArraysLeft(JObject json)
        {
            JObject o = cXml.Array2Object(json["ELEMENTS"]["iris"]["MEDIA"]["ITEMS"]);
            if (o != null)
                json["ELEMENTS"]["iris"]["MEDIA"] = o;
            o = cXml.Array2Object(json["ELEMENTS"]["iris"]["ADDITIONALPARAMS"]["PARAM"]);
            if (o != null)
                json["ELEMENTS"]["iris"]["ADDITIONALPARAMS"] = o;
        }
        #endregion

        #region accesscode
        private static void CreateAccessCodeGroups(JObject json)
        {
            JArray proparr = (JArray)json["iris_access_code"]["PROPERTIES"]["PROPERTY"];
            JObject o = Array2Object(proparr);
            //groups
            json["iris_access_code"]["PROPERTIES"]["Groups"] = new JObject();
            //
            JObject grp1 = new JObject();
            grp1["name"] = "Code 1";
            JObject f1 = new JObject();
            f1["Code1"] = o["Code1"];
            f1["Level1"] = o["Level1"];
            grp1["fields"] = f1;
            json["iris_access_code"]["PROPERTIES"]["Groups"]["Code1"] = grp1;
            //
            JObject grp2 = new JObject();
            grp2["name"] = "Code 2";
            JObject f2 = new JObject();
            f2["Code2"] = o["Code2"];
            f2["Level2"] = o["Level2"];
            grp2["fields"] = f2;
            json["iris_access_code"]["PROPERTIES"]["Groups"]["Code2"] = grp2;
            //
            JObject grp3 = new JObject();
            grp3["name"] = "Code 3";
            JObject f3 = new JObject();
            f3["Code3"] = o["Code3"];
            f3["Level3"] = o["Level3"];
            grp3["fields"] = f3;
            json["iris_access_code"]["PROPERTIES"]["Groups"]["Code3"] = grp3;
            //
            JObject grp4 = new JObject();
            grp4["name"] = "Code 4";
            JObject f4 = new JObject();
            f4["Code4"] = o["Code4"];
            f4["Level4"] = o["Level4"];
            grp4["fields"] = f4;
            json["iris_access_code"]["PROPERTIES"]["Groups"]["Code4"] = grp4;
            //
            json["iris_access_code"]["PROPERTIES"]["OLD"] = o;
            //
            AddMissetFields((JObject)json["iris_access_code"]["PROPERTIES"]["Groups"], o, null);
        }

        private static void ConvertAccessCode(JObject json, JObject _pages)
        {
            if (Regex.IsMatch(json["ELEMENTS"]["iris"]["@PRODUCTNAME"].ToString(), @"REPEATER\s+?Iris[\w\W]+?Simpo$", RegexOptions.IgnoreCase))
                return;
            if (Regex.IsMatch(json["ELEMENTS"]["iris"]["@PRODUCTNAME"].ToString(), @"^SIMPO[\w\W]*?PANEL$", RegexOptions.IgnoreCase))
                return;
            CreateAccessCodeGroups((JObject)json["ELEMENTS"]);
            JObject ac = (JObject)json["ELEMENTS"]["iris_access_code"];
            ac["title"] = _pages["iris_access_code"]["title"];
            ac["left"] = _pages["iris_access_code"]["left"];
            ac["right"] = _pages["iris_access_code"]["right"];
            ac["breadcrumbs"] = _pages["iris_access_code"]["breadcrumbs"];
            //
            JObject o = cXml.Array2Object(json["ELEMENTS"]["iris_access_code"]["RULES"]["RULE"]);
            if (o != null)
                json["ELEMENTS"]["iris_access_code"]["RULES"] = o;
        }
        #endregion

        #region panels in network
        private static void ConvertPanelsInNetwork(JObject json, JObject _pages)
        {
            if (Regex.IsMatch(json["ELEMENTS"]["iris"]["@PRODUCTNAME"].ToString(), @"REPEATER\s+?Iris[\w\W]+?Simpo$", RegexOptions.IgnoreCase))
            {
                JObject pcontent = new JObject((JObject)json["ELEMENTS"]["iris_network"]["CONTAINS"]["ELEMENT"]);
                if (json["ELEMENTS"]["iris_panels_in_network"] == null) json["ELEMENTS"]["iris_panels_in_network"] = new JObject();
                json["ELEMENTS"]["iris_panels_in_network"]["CONTAINS"] = new JObject();
                json["ELEMENTS"]["iris_panels_in_network"]["CONTAINS"]["ELEMENT"] = pcontent;
                //pcontent["@ID"] = TranslateKey(pcontent["@ID"].ToString());
                JObject netobj = (JObject)json["ELEMENTS"]["iris_network"];
                netobj.Remove("CONTAINS");
            }
            JObject ac = (JObject)json["ELEMENTS"]["iris_panels_in_network"];
            ac["title"] = _pages["iris_panels_in_network"]["title"];
            ac["left"] = _pages["iris_panels_in_network"]["left"];
            ac["right"] = _pages["iris_panels_in_network"]["right"];
            ac["breadcrumbs"] = _pages["iris_panels_in_network"]["breadcrumbs"];
            JObject contains = Contains2Object((JObject)json["ELEMENTS"]["iris_panels_in_network"]["CONTAINS"]);
            json["ELEMENTS"]["iris_panels_in_network"]["CONTAINS"] = contains;
        }
        #endregion

        #region network
        private static void CreateRepeaterNetworkGroups(JObject json)
        {
            JArray proparr = (JArray)json["iris_network"]["PROPERTIES"]["PROPERTY"];
            JObject o = Array2Object(proparr);
            //groups
            json["iris_network"]["PROPERTIES"]["Groups"] = new JObject();
            //not grouped params
            json["iris_network"]["PROPERTIES"]["Groups"]["~noname"] = new JObject();
            //json["iris_network"]["PROPERTIES"]["Groups"]["~noname"]["Name"] = "";
            json["iris_network"]["PROPERTIES"]["Groups"]["~noname"]["PANELNUMBER"] = o["PANELNUMBER"];
            json["iris_network"]["PROPERTIES"]["Groups"]["~noname"]["PANELNAME"] = o["PANELNAME"];
            json["iris_network"]["PROPERTIES"]["Groups"]["~noname"]["NETWORKENABLED"] = o["NETWORKENABLED"];
            //
            //
            json["iris_network"]["PROPERTIES"]["OLD"] = o;
        }
        private static void CreateNetworkGroups(JObject json)
        {
            if (Regex.IsMatch(json["iris"]["@PRODUCTNAME"].ToString(), @"REPEATER\s+?Iris[\w\W]+?Simpo$", RegexOptions.IgnoreCase) ||
                Regex.IsMatch(json["iris"]["@PRODUCTNAME"].ToString(), @"^SIMPO[\w\W]+?Panel$", RegexOptions.IgnoreCase))
            {
                CreateRepeaterNetworkGroups(json);
                return;
            }
            JArray proparr = (JArray)json["iris_network"]["PROPERTIES"]["PROPERTY"];
            JObject o = Array2Object(proparr);
            //groups
            json["iris_network"]["PROPERTIES"]["Groups"] = new JObject();
            //not grouped params
            json["iris_network"]["PROPERTIES"]["Groups"]["~noname"] = new JObject();
            json["iris_network"]["PROPERTIES"]["Groups"]["~noname"]["Name"] = o["Name"];
            json["iris_network"]["PROPERTIES"]["Groups"]["~noname"]["PanelNumber"] = o["PanelNumber"];
            //
            JObject grp1 = new JObject();
            grp1["name"] = "Parameters";
            JObject f1 = new JObject();
            f1["IsNetworkEnabled"] = o["IsNetworkEnabled"];
            JObject neprops = Array2Object((JArray)f1["IsNetworkEnabled"]["PROPERTIES"]["PROPERTY"]);
            if (Regex.IsMatch(json["iris"]["@PRODUCTNAME"].ToString(), @"^TFT[\w\W]+?Panel$", RegexOptions.IgnoreCase))
            {
                JObject ontype = (JObject)neprops["NetworkType"];
                if (ontype != null) ontype["@AND"] = "2";
            }
            f1["IsNetworkEnabled"]["PROPERTIES"] = neprops;
            //f1["NetworkType"] = o["NetworkType"];
            //f1["Protocol"] = o["Protocol"];
            //f1["Redundancy_En"] = o["Redundancy_En"];
            grp1["fields"] = f1;
            json["iris_network"]["PROPERTIES"]["Groups"]["Parameters"] = grp1;
            //
            JObject grp2 = new JObject();
            grp2["name"] = "Network Settings";
            JObject f2 = new JObject();
            f2["HostIP"] = o["HostIP"];
            f2["NetMask"] = o["NetMask"];
            f2["Router"] = o["Router"];
            f2["Port"] = o["Port"];
            //f2["PanelEvacNumber"] = o["PanelEvacNumber"];
            //f2["emacETHADDR0"] = o["emacETHADDR0"];
            //f2["emacETHADDR1"] = o["emacETHADDR1"];
            //f2["emacETHADDR2"] = o["emacETHADDR2"];
            //f2["emacETHADDR3"] = o["emacETHADDR3"];
            //f2["emacETHADDR4"] = o["emacETHADDR4"];
            //f2["emacETHADDR5"] = o["emacETHADDR5"];
            grp2["fields"] = f2;
            json["iris_network"]["PROPERTIES"]["Groups"]["NetworkSettings"] = grp2;
            //not grouped params
            json["iris_network"]["PROPERTIES"]["Groups"]["~noname1"] = new JObject();
            if (o["PanelEvacNumber"] != null) json["iris_network"]["PROPERTIES"]["Groups"]["~noname1"]["PanelEvacNumber"] = o["PanelEvacNumber"];
            json["iris_network"]["PROPERTIES"]["Groups"]["~noname1"]["emacETHADDR"] = o["emacETHADDR0"];
            json["iris_network"]["PROPERTIES"]["Groups"]["~noname1"]["emacETHADDR"]["@TYPE"] = "EMAC";
            json["iris_network"]["PROPERTIES"]["Groups"]["~noname1"]["emacETHADDR"]["@TEXT"] = "EMAC";
            //
            json["iris_network"]["PROPERTIES"]["OLD"] = o;
            //
            AddMissetFields((JObject)json["iris_network"]["PROPERTIES"]["Groups"], o, null);
        }
        private static void ConvertNetwork(JObject json, JObject _pages)
        {
            CreateNetworkGroups((JObject)json["ELEMENTS"]);
            JObject ac = (JObject)json["ELEMENTS"]["iris_network"];
            ac["title"] = _pages["iris_network"]["title"];
            ac["left"] = _pages["iris_network"]["left"];
            ac["right"] = _pages["iris_network"]["right"];
            ac["breadcrumbs"] = _pages["iris_network"]["breadcrumbs"];
        }
        #endregion

        #region inputs
        private static void ConvertInputs(JObject json, JObject _pages)
        {
            if (Regex.IsMatch(json["ELEMENTS"]["iris"]["@PRODUCTNAME"].ToString(), @"REPEATER\s+?Iris[\w\W]+?Simpo$", RegexOptions.IgnoreCase) ||
                Regex.IsMatch(json["ELEMENTS"]["iris"]["@PRODUCTNAME"].ToString(), @"^SIMPO[\w\W]+?Panel$", RegexOptions.IgnoreCase) ||
                Regex.IsMatch(json["ELEMENTS"]["iris"]["@PRODUCTNAME"].ToString(), @"^TFT[\w\W]+?Panel$", RegexOptions.IgnoreCase))
            {
                return;
            }
            JObject ac = (JObject)json["ELEMENTS"]["iris_inputs"];
            ac["title"] = _pages["iris_inputs"]["title"];
            ac["left"] = _pages["iris_inputs"]["left"];
            ac["right"] = _pages["iris_inputs"]["right"];
            ac["breadcrumbs"] = _pages["iris_inputs"]["breadcrumbs"];
            JObject contains = Contains2Object((JObject)json["ELEMENTS"]["iris_inputs"]["CONTAINS"]);
            json["ELEMENTS"]["iris_inputs"]["CONTAINS"] = contains;
        }
        #endregion

        #region FAT FBF
        private static void CreateFATFBFGroups(JObject json)
        {
            JObject content = (JObject)json["ELEMENTS"]["iris_fat_fbf"]["CONTAINS"];
            foreach (JProperty pfat in content.Properties())
            {
                string fat_name = pfat.Name;
                JObject ofat = (JObject)json["ELEMENTS"][fat_name];
                JObject fatprops_old = new JObject((JObject)ofat["PROPERTIES"]["PROPERTY"]);
                JObject fatprops = new JObject((JObject)ofat["PROPERTIES"]["PROPERTY"]);
                //
                JObject groups = new JObject();
                groups["~noname"] = new JObject();
                groups["~noname"]["name"] = "";
                groups["~noname"]["fields"] = new JObject();
                groups["~noname"]["fields"]["Type"] = fatprops;
                json["ELEMENTS"][fat_name]["PROPERTIES"]["Groups"] = ofat["PROPERTIES"]["Groups"];
                //
                fatprops = (JObject)ofat["PROPERTIES"];
                fatprops.Remove("PROPERTY");
                fatprops["Groups"] = groups;
                fatprops["PROPERTY"] = fatprops_old;
            }
            //
            //JArray proparr = (JArray)json["iris_fat_fbf"]["PROPERTIES"]["PROPERTY"];
            //JObject o = Array2Object(proparr);
            ////groups
            //json["iris_network"]["PROPERTIES"]["Groups"] = new JObject();
            ////not grouped params
            //json["iris_network"]["PROPERTIES"]["Groups"]["~noname"] = new JObject();
            //json["iris_network"]["PROPERTIES"]["Groups"]["~noname"]["Name"] = o["Name"];
            //json["iris_network"]["PROPERTIES"]["Groups"]["~noname"]["PanelNumber"] = o["PanelNumber"];
            ////
            //JObject grp1 = new JObject();
            //grp1["name"] = "Parameters";
            //JObject f1 = new JObject();
            //f1["IsNetworkEnabled"] = o["IsNetworkEnabled"];
            //JObject neprops = Array2Object((JArray)f1["IsNetworkEnabled"]["PROPERTIES"]["PROPERTY"]);
            //f1["IsNetworkEnabled"]["PROPERTIES"] = neprops;
            ////f1["NetworkType"] = o["NetworkType"];
            ////f1["Protocol"] = o["Protocol"];
            ////f1["Redundancy_En"] = o["Redundancy_En"];
            //grp1["fields"] = f1;
            //json["iris_network"]["PROPERTIES"]["Groups"]["Parameters"] = grp1;
            ////
            //JObject grp2 = new JObject();
            //grp2["name"] = "Network Settings";
            //JObject f2 = new JObject();
            //f2["HostIP"] = o["HostIP"];
            //f2["NetMask"] = o["NetMask"];
            //f2["Gateway"] = o["Router"];
            //f2["Port"] = o["Port"];
            ////f2["PanelEvacNumber"] = o["PanelEvacNumber"];
            ////f2["emacETHADDR0"] = o["emacETHADDR0"];
            ////f2["emacETHADDR1"] = o["emacETHADDR1"];
            ////f2["emacETHADDR2"] = o["emacETHADDR2"];
            ////f2["emacETHADDR3"] = o["emacETHADDR3"];
            ////f2["emacETHADDR4"] = o["emacETHADDR4"];
            ////f2["emacETHADDR5"] = o["emacETHADDR5"];
            //grp2["fields"] = f2;
            //json["iris_network"]["PROPERTIES"]["Groups"]["NetworkSettings"] = grp2;
            ////not grouped params
            //json["iris_network"]["PROPERTIES"]["Groups"]["~noname1"] = new JObject();
            //json["iris_network"]["PROPERTIES"]["Groups"]["~noname1"]["PanelEvacNumber"] = o["PanelEvacNumber"];
            //json["iris_network"]["PROPERTIES"]["Groups"]["~noname1"]["emacETHADDR"] = o["emacETHADDR0"];
            //json["iris_network"]["PROPERTIES"]["Groups"]["~noname1"]["emacETHADDR"]["@TYPE"] = "EMAC";
            //json["iris_network"]["PROPERTIES"]["Groups"]["~noname1"]["emacETHADDR"]["@TEXT"] = "EMAC";
            ////
            //json["iris_network"]["PROPERTIES"]["OLD"] = o;
        }
        private static void ConvertFATFBF(JObject json, JObject _pages)
        {
            if (Regex.IsMatch(json["ELEMENTS"]["iris"]["@PRODUCTNAME"].ToString(), @"REPEATER\s+?Iris[\w\W]+?Simpo$", RegexOptions.IgnoreCase) ||
                Regex.IsMatch(json["ELEMENTS"]["iris"]["@PRODUCTNAME"].ToString(), @"^SIMPO[\w\W]+?Panel$", RegexOptions.IgnoreCase)||
                Regex.IsMatch(json["ELEMENTS"]["iris"]["@PRODUCTNAME"].ToString(), @"^TFT[\w\W]+?Panel$", RegexOptions.IgnoreCase))
            {
                return;
            }
            JObject ac = (JObject)json["ELEMENTS"]["iris_fat_fbf"];
            ac["title"] = _pages["iris_fat_fbf"]["title"];
            ac["left"] = _pages["iris_fat_fbf"]["left"];
            ac["right"] = _pages["iris_fat_fbf"]["right"];
            ac["breadcrumbs"] = _pages["iris_fat_fbf"]["breadcrumbs"];
            JObject contains = Contains2Object((JObject)json["ELEMENTS"]["iris_fat_fbf"]["CONTAINS"]);
            json["ELEMENTS"]["iris_fat_fbf"]["CONTAINS"] = contains;
            CreateFATFBFGroups(json);
        }
        #endregion

        #region input groups
        private static void ConvertInputGroups(JObject json, JObject _pages)
        {
            if (Regex.IsMatch(json["ELEMENTS"]["iris"]["@PRODUCTNAME"].ToString(), @"REPEATER\s+?Iris[\w\W]+?Simpo$", RegexOptions.IgnoreCase) ||
                Regex.IsMatch(json["ELEMENTS"]["iris"]["@PRODUCTNAME"].ToString(), @"^SIMPO[\w\W]+?Panel$", RegexOptions.IgnoreCase) ||
                Regex.IsMatch(json["ELEMENTS"]["iris"]["@PRODUCTNAME"].ToString(), @"^TFT[\w\W]+?Panel$", RegexOptions.IgnoreCase))
            {
                return;
            }
            JObject ac = (JObject)json["ELEMENTS"]["iris_inputs_group"];
            ac["title"] = _pages["iris_inputs_group"]["title"];
            ac["left"] = _pages["iris_inputs_group"]["left"];
            ac["right"] = _pages["iris_inputs_group"]["right"];
            ac["breadcrumbs"] = _pages["iris_inputs_group"]["breadcrumbs"];
            JObject contains = Contains2Object((JObject)json["ELEMENTS"]["iris_inputs_group"]["CONTAINS"]);
            json["ELEMENTS"]["iris_inputs_group"]["CONTAINS"] = contains;
        }
        #endregion

        #region outputs
        private static void ConvertOutputs(JObject json, JObject _pages)
        {
            if (Regex.IsMatch(json["ELEMENTS"]["iris"]["@PRODUCTNAME"].ToString(), @"REPEATER\s+?Iris[\w\W]+?Simpo$", RegexOptions.IgnoreCase) ||
                Regex.IsMatch(json["ELEMENTS"]["iris"]["@PRODUCTNAME"].ToString(), @"^SIMPO[\w\W]+?Panel$", RegexOptions.IgnoreCase) ||
                Regex.IsMatch(json["ELEMENTS"]["iris"]["@PRODUCTNAME"].ToString(), @"^TFT[\w\W]+?Panel$", RegexOptions.IgnoreCase))
            {
                return;
            }
            JObject ac = (JObject)json["ELEMENTS"]["iris_outputs"];
            ac["title"] = _pages["iris_outputs"]["title"];
            ac["left"] = _pages["iris_outputs"]["left"];
            ac["right"] = _pages["iris_outputs"]["right"];
            ac["breadcrumbs"] = _pages["iris_outputs"]["breadcrumbs"];
            JObject contains = Contains2Object((JObject)json["ELEMENTS"]["iris_outputs"]["CONTAINS"]);
            json["ELEMENTS"]["iris_outputs"]["CONTAINS"] = contains;
        }
        #endregion

        #region zones
        private static void ConvertZones(JObject json, JObject _pages)
        {
            if (Regex.IsMatch(json["ELEMENTS"]["iris"]["@PRODUCTNAME"].ToString(), @"REPEATER\s+?Iris[\w\W]+?Simpo$", RegexOptions.IgnoreCase) ||
                Regex.IsMatch(json["ELEMENTS"]["iris"]["@PRODUCTNAME"].ToString(), @"^TFT[\w\W]+?Panel$", RegexOptions.IgnoreCase))
            {
                return;
            }
            JObject ac = (JObject)json["ELEMENTS"]["iris_zones"];
            ac["title"] = _pages["iris_zones"]["title"];
            ac["left"] = _pages["iris_zones"]["left"];
            ac["right"] = _pages["iris_zones"]["right"];
            ac["breadcrumbs"] = _pages["iris_zones"]["breadcrumbs"];
            JObject contains = Contains2Object((JObject)json["ELEMENTS"]["iris_zones"]["CONTAINS"]);
            json["ELEMENTS"]["iris_zones"]["CONTAINS"] = contains;
        }
        #endregion

        #region evac zones
        private static void ConvertEvacZones(JObject json, JObject _pages)
        {
            if (Regex.IsMatch(json["ELEMENTS"]["iris"]["@PRODUCTNAME"].ToString(), @"REPEATER\s+?Iris[\w\W]+?Simpo$", RegexOptions.IgnoreCase) ||
                Regex.IsMatch(json["ELEMENTS"]["iris"]["@PRODUCTNAME"].ToString(), @"^TFT[\w\W]+?Panel$", RegexOptions.IgnoreCase))
            {
                return;
            }
            JObject ac = (JObject)json["ELEMENTS"]["iris_evac_zones"];
            ac["title"] = _pages["iris_evac_zones"]["title"];
            ac["left"] = _pages["iris_evac_zones"]["left"];
            ac["right"] = _pages["iris_evac_zones"]["right"];
            ac["breadcrumbs"] = _pages["iris_evac_zones"]["breadcrumbs"];
            JObject contains = Contains2Object((JObject)json["ELEMENTS"]["iris_evac_zones"]["CONTAINS"]);
            json["ELEMENTS"]["iris_evac_zones"]["CONTAINS"] = contains;
        }
        #endregion

        #region peripheral devices
        private static void ConvertPeripheralDevices(JObject json, JObject _pages)
        {
            if (Regex.IsMatch(json["ELEMENTS"]["iris"]["@PRODUCTNAME"].ToString(), @"REPEATER\s+?Iris[\w\W]+?Simpo$", RegexOptions.IgnoreCase) ||
                Regex.IsMatch(json["ELEMENTS"]["iris"]["@PRODUCTNAME"].ToString(), @"^SIMPO[\w\W]+?Panel$", RegexOptions.IgnoreCase) ||
                Regex.IsMatch(json["ELEMENTS"]["iris"]["@PRODUCTNAME"].ToString(), @"^TFT[\w\W]+?Panel$", RegexOptions.IgnoreCase))
            {
                return;
            }
            JObject ac = (JObject)json["ELEMENTS"]["iris_peripheral_devices"];
            ac["title"] = _pages["iris_peripheral_devices"]["title"];
            ac["left"] = _pages["iris_peripheral_devices"]["left"];
            ac["right"] = _pages["iris_peripheral_devices"]["right"];
            ac["breadcrumbs"] = _pages["iris_peripheral_devices"]["breadcrumbs"];
            JObject contains = Contains2Object((JObject)json["ELEMENTS"]["iris_peripheral_devices"]["CONTAINS"]);
            json["ELEMENTS"]["iris_peripheral_devices"]["CONTAINS"] = contains;
        }
        #endregion

        #region loop devices
        private static void ConvertLoopDevices(JObject json, JObject _pages)
        {
            if (Regex.IsMatch(json["ELEMENTS"]["iris"]["@PRODUCTNAME"].ToString(), @"REPEATER\s+?Iris[\w\W]+?Simpo$", RegexOptions.IgnoreCase) ||
                Regex.IsMatch(json["ELEMENTS"]["iris"]["@PRODUCTNAME"].ToString(), @"^TFT[\w\W]+?Panel$", RegexOptions.IgnoreCase))
            {
                return;
            }
            JObject ac = (JObject)json["ELEMENTS"]["iris_loop_devices"];
            ac["title"] = _pages["iris_loop_devices"]["title"];
            ac["left"] = _pages["iris_loop_devices"]["left"];
            ac["right"] = _pages["iris_loop_devices"]["right"];
            ac["breadcrumbs"] = _pages["iris_loop_devices"]["breadcrumbs"];
            JObject contains = Contains2Object((JObject)json["ELEMENTS"]["iris_loop_devices"]["CONTAINS"]);
            json["ELEMENTS"]["iris_loop_devices"]["CONTAINS"] = contains;
            //
            if (Regex.IsMatch(json["ELEMENTS"]["iris"]["@PRODUCTNAME"].ToString(), @"SIMPO\s+?panel$", RegexOptions.IgnoreCase))
            {
                JObject elements = (JObject)json["ELEMENTS"];
                JObject props = (JObject)elements["SIMPO_TTELOOP1"]["PROPERTIES"];
                props["Groups"] = new JObject();
                props["Groups"]["~noname"] = new JObject();
                props["Groups"]["~noname"]["name"] = "";
                props["Groups"]["~noname"]["fields"] = new JObject();
                JObject f = new JObject((JObject)props["PROPERTY"]);
                props["Groups"]["~noname"]["fields"]["Ver"] = f;
                //
                props = (JObject)elements["SIMPO_TTELOOP2"]["PROPERTIES"];
                props["Groups"] = new JObject();
                props["Groups"]["~noname"] = new JObject();
                props["Groups"]["~noname"]["name"] = "";
                props["Groups"]["~noname"]["fields"] = new JObject();
                f = new JObject((JObject)props["PROPERTY"]);
                props["Groups"]["~noname"]["fields"]["Ver"] = f;
            }
        }
        #endregion

        #region input group
        private static void ConvertInputsGroup(JObject json)
        {
            if (Regex.IsMatch(json["ELEMENTS"]["iris"]["@PRODUCTNAME"].ToString(), @"REPEATER\s+?Iris[\w\W]+?Simpo$", RegexOptions.IgnoreCase) ||
                Regex.IsMatch(json["ELEMENTS"]["iris"]["@PRODUCTNAME"].ToString(), @"^SIMPO[\w\W]+?Panel$", RegexOptions.IgnoreCase) ||
                Regex.IsMatch(json["ELEMENTS"]["iris"]["@PRODUCTNAME"].ToString(), @"^TFT[\w\W]+?Panel$", RegexOptions.IgnoreCase))
            {
                return;
            }
            foreach (JProperty p in json["ELEMENTS"]["iris_inputs_group"]["CONTAINS"])
            {
                string key = p.Name.ToString();
                JObject prop = Contains2Object((JObject)json["ELEMENTS"][key]["PROPERTIES"]);
                if (json["ELEMENTS"][key]["PROPERTIES"] == null)
                    json["ELEMENTS"][key]["PROPERTIES"] = new JObject();
                if (json["ELEMENTS"][key]["PROPERTIES"]["Groups"] == null)
                    json["ELEMENTS"][key]["PROPERTIES"]["Groups"] = new JObject();
                if (json["ELEMENTS"][key]["PROPERTIES"]["Groups"]["~noname"] == null)
                    json["ELEMENTS"][key]["PROPERTIES"]["Groups"]["~noname"] = new JObject();
                json["ELEMENTS"][key]["PROPERTIES"]["Groups"]["~noname"]["name"] = "";
                if (prop["Input_Logic"] != null && prop["Input_Logic"]["ITEMS"] != null && prop["Input_Logic"]["ITEMS"]["ITEM"] != null)
                {
                    JArray item = (JArray)prop["Input_Logic"]["ITEMS"]["ITEM"];
                    foreach (JObject o in item)
                        if (o["@DEFAULT"] != null)
                        {
                            o["@DEFAULT"] = "0";
                            break;
                        }
                }
                json["ELEMENTS"][key]["PROPERTIES"]["Groups"]["~noname"]["fields"] = prop;
            }
        }
        #endregion

        #region input
        private static void CreateInputGroups(JObject json, string key)
        {
            JArray proparr = (JArray)json[key]["PROPERTIES"]["PROPERTY"];
            JObject o = Array2Object(proparr);
            //groups
            json[key]["PROPERTIES"]["Groups"] = new JObject();
            //not grouped params
            json[key]["PROPERTIES"]["Groups"]["~noname"] = new JObject();
            json[key]["PROPERTIES"]["Groups"]["~noname"]["MESS_IN"] = o["MESS_IN"];
            //
            JObject grp1 = new JObject();
            grp1["name"] = "Settings";
            JObject f1 = new JObject();
            f1["DELAY"] = o["DELAY"];
            f1["Group"] = o["Group"];
            grp1["fields"] = f1;
            json[key]["PROPERTIES"]["Groups"]["Settings"] = grp1;
            //
            JObject grp2 = new JObject();
            grp2["name"] = "Parameters";
            JObject f2 = new JObject();
            JObject props = Array2Object((JArray)o["PARAMETERS"]["PROPERTIES"]["PROPERTY"]);
            //f2["parameters"] = props;
            grp2["fields"] = props;
            grp2["~type"] = "AND";
            json[key]["PROPERTIES"]["Groups"]["Parameters"] = grp2;
            //
            JObject grp3 = new JObject();
            grp3["name"] = "Input Type";
            JObject f3 = new JObject();
            f3["Type"] = o["Type"];
            grp3["fields"] = f3;
            json[key]["PROPERTIES"]["Groups"]["InputType"] = grp3;
            //
            json[key]["PROPERTIES"]["OLD"] = o;
            //
            AddMissetFields((JObject)json[key]["PROPERTIES"]["Groups"], o, null);
        }
        private static void ConvertInput(JObject json)
        {
            if (Regex.IsMatch(json["ELEMENTS"]["iris"]["@PRODUCTNAME"].ToString(), @"REPEATER\s+?Iris[\w\W]+?Simpo$", RegexOptions.IgnoreCase) ||
                Regex.IsMatch(json["ELEMENTS"]["iris"]["@PRODUCTNAME"].ToString(), @"^SIMPO[\w\W]+?Panel$", RegexOptions.IgnoreCase) ||
                Regex.IsMatch(json["ELEMENTS"]["iris"]["@PRODUCTNAME"].ToString(), @"^TFT[\w\W]+?Panel$", RegexOptions.IgnoreCase))
            {
                return;
            }
            foreach (JProperty p in json["ELEMENTS"]["iris_inputs"]["CONTAINS"])
            {
                string key = p.Name.ToString();
                CreateInputGroups((JObject)json["ELEMENTS"], key);
            }

        }
        #endregion

        #region output
        private static void CreateOutputGroups(JObject json, string key)
        {
            JArray proparr = (JArray)json[key]["PROPERTIES"]["PROPERTY"];
            JObject o = Array2Object(proparr);
            //groups
            json[key]["PROPERTIES"]["Groups"] = new JObject();
            //not grouped params
            json[key]["PROPERTIES"]["Groups"]["~noname"] = new JObject();
            json[key]["PROPERTIES"]["Groups"]["~noname"]["MESS"] = o["MESS"];
            //
            JObject grp1 = new JObject();
            grp1["name"] = "Settings";
            JObject f1 = new JObject();
            f1["DELAY"] = o["DELAY"];
            f1["PULSE_DURATION"] = o["PULSE_DURATION"];
            grp1["fields"] = f1;
            json[key]["PROPERTIES"]["Groups"]["Settings"] = grp1;
            //
            JObject grp2 = new JObject();
            grp2["name"] = "Output Type";
            JObject f2 = new JObject();
            f2["Type"] = o["Type"];
            grp2["fields"] = f2;
            json[key]["PROPERTIES"]["Groups"]["OutputType"] = grp2;
            //
            JObject grp3 = new JObject();
            grp3["name"] = "Parameters";
            JObject f3 = new JObject();
            //JObject props = Array2Object((JArray)o["PARAMETERS"]["PROPERTIES"]["PROPERTY"]);
            ////f2["parameters"] = props;
            //grp3["fields"] = props;
            grp3["fields"] = new JObject();
            grp3["fields"]["Parameters"] = o["PARAMETERS"];
            grp3["fields"]["ORINPUTS"] = o["ORINPUTS"];
            grp3["fields"]["ANDINPUTS"] = o["ANDINPUTS"];
            json[key]["PROPERTIES"]["Groups"]["Parameters"] = grp3;
            //
            json[key]["PROPERTIES"]["Groups"]["~hidden"] = new JObject();
            json[key]["PROPERTIES"]["Groups"]["~hidden"]["name"] = "Hidden properties";
            json[key]["PROPERTIES"]["Groups"]["~hidden"]["fields"] = new JObject();
            json[key]["PROPERTIES"]["Groups"]["~hidden"]["fields"]["FUNCTION"] = o["FUNCTION"];
            //
            //JObject grp4 = new JObject();
            //grp4["name"] = "OR/AND inputs";
            //grp4["fields"] = new JObject();
            //grp4["fields"]["ORINPUTS"] = o["ORINPUTS"];
            //grp4["fields"]["ANDINPUTS"] = o["ANDINPUTS"];
            //json[key]["PROPERTIES"]["Groups"]["ANDORInputs"] = grp4;
            //
            json[key]["PROPERTIES"]["OLD"] = o;
            //
            AddMissetFields((JObject)json[key]["PROPERTIES"]["Groups"], o, null);
        }
        private static void ConvertOutput(JObject json)
        {
            if (Regex.IsMatch(json["ELEMENTS"]["iris"]["@PRODUCTNAME"].ToString(), @"REPEATER\s+?Iris[\w\W]+?Simpo$", RegexOptions.IgnoreCase) ||
                Regex.IsMatch(json["ELEMENTS"]["iris"]["@PRODUCTNAME"].ToString(), @"^SIMPO[\w\W]+?Panel$", RegexOptions.IgnoreCase) ||
                Regex.IsMatch(json["ELEMENTS"]["iris"]["@PRODUCTNAME"].ToString(), @"^TFT[\w\W]+?Panel$", RegexOptions.IgnoreCase))
            {
                return;
            }
            foreach (JProperty p in json["ELEMENTS"]["iris_outputs"]["CONTAINS"])
            {
                string key = p.Name.ToString();
                CreateOutputGroups((JObject)json["ELEMENTS"], key);
            }
        }
        #endregion

        #region zone
        private static void CreateZoneGroups(JObject json, string key)
        {
            JArray proparr = (JArray)json[key]["PROPERTIES"]["PROPERTY"];
            JObject o = Array2Object(proparr);
            //groups
            json[key]["PROPERTIES"]["Groups"] = new JObject();
            //not grouped params
            json[key]["PROPERTIES"]["Groups"]["~noname"] = new JObject();
            json[key]["PROPERTIES"]["Groups"]["~noname"]["ZONENAME"] = o["ZONENAME"];
            json[key]["PROPERTIES"]["Groups"]["~noname"]["ZONEMODE"] = o["ZONEMODE"];
            //
            JObject grp1 = new JObject();
            grp1["name"] = "Delays";
            JObject f1 = new JObject();
            f1["SOUNDERDELAY"] = o["SOUNDERDELAY"];
            f1["FIREBRIGADEDELAY"] = o["FIREBRIGADEDELAY"];
            if (o["FIREPROTECTIONDELAY"] != null) f1["FIREPROTECTIONDELAY"] = o["FIREPROTECTIONDELAY"];
            grp1["fields"] = f1;
            json[key]["PROPERTIES"]["Groups"]["Delays"] = grp1;
            //
            JObject grp2 = new JObject();
            grp2["name"] = "Zone";
            JObject f2 = new JObject();
            if (o["ZONEGROUP"] != null) f2["ZoneGroupA"] = o["ZONEGROUP"]; else f2["GROUP"] = o["GROUP"];
            if (o["ZONEGROUP2"] != null) f2["ZoneGroupB"] = o["ZONEGROUP2"]; else f2["GROUP2"] = o["GROUP2"];
            if (o["ZONEGROUP3"] != null) f2["ZoneGroupC"] = o["ZONEGROUP3"]; else f2["GROUP3"] = o["GROUP3"];
            if (o["ZONESOUNDERS"] != null) f2["ZoneSounders"] = o["ZONESOUNDERS"]; else f2["ZSOUNDERS"] = o["ZSOUNDERS"];
            grp2["fields"] = f2;
            json[key]["PROPERTIES"]["Groups"]["Zone"] = grp2;
            //
            JObject grp3 = new JObject();
            grp3["name"] = "Sounder";
            JObject f3 = new JObject();
            if (o["SOUNDERGROUP"] != null) f3["SounderGroupA"] = o["SOUNDERGROUP"]; else f3["SGROUP_A"] = o["SGROUP_A"];
            if (o["SOUNDERGROUP2"] != null) f3["SounderGroupB"] = o["SOUNDERGROUP2"]; else f3["SGROUP_B"] = o["SGROUP_B"];
            if (o["SOUNDERGROUP3"] != null) f3["SounderGroupC"] = o["SOUNDERGROUP3"]; else f3["SGROUP_C"] = o["SGROUP_C"];
            grp3["fields"] = f3;
            json[key]["PROPERTIES"]["Groups"]["Sounder"] = grp3;
            //
            json[key]["PROPERTIES"]["OLD"] = o;
            //
            AddMissetFields((JObject)json[key]["PROPERTIES"]["Groups"], o, null);
        }
        private static void ConvertZone(JObject json)
        {
            if (Regex.IsMatch(json["ELEMENTS"]["iris"]["@PRODUCTNAME"].ToString(), @"REPEATER\s+?Iris[\w\W]+?Simpo$", RegexOptions.IgnoreCase) ||
                Regex.IsMatch(json["ELEMENTS"]["iris"]["@PRODUCTNAME"].ToString(), @"^TFT[\w\W]+?Panel$", RegexOptions.IgnoreCase))
            {
                return;
            }
            foreach (JProperty p in json["ELEMENTS"]["iris_zones"]["CONTAINS"])
            {
                string key = p.Name.ToString();
                CreateZoneGroups((JObject)json["ELEMENTS"], key);
            }
        }
        #endregion

        #region evac zone group
        private static void CreateEvacZoneGroupGroups(JObject json, string key)
        {
            JArray proparr = (JArray)json[key]["PROPERTIES"]["PROPERTY"];
            JObject o = Array2Object(proparr);
            //groups
            json[key]["PROPERTIES"]["Groups"] = new JObject();
            //
            foreach (JProperty evacp in o.Properties())
            {
                if (evacp.Value.Type != JTokenType.Object) continue;
                JObject evaco = (JObject)evacp.Value;
                //
                Match m = Regex.Match(evacp.Name, @"(\d+)$");
                if (!m.Success) continue;
                string evacnom = m.Groups[1].Value;
                JObject grp = new JObject();
                grp["name"] = "Evacuation Panel " + evacnom;
                grp["~type"] = "AND";
                JObject props = Array2Object((JArray)o["EVACPANEL" + evacnom]["PROPERTIES"]["PROPERTY"]);
                grp["fields"] = props;
                json[key]["PROPERTIES"]["Groups"]["EvacPanel" + evacnom] = grp;
            }
            //JObject grp1 = new JObject();
            //grp1["name"] = "Evacuation Panel 0";
            //grp1["~type"] = "AND";
            //JObject props = Array2Object((JArray)o["EVACPANEL0"]["PROPERTIES"]["PROPERTY"]);
            //grp1["fields"] = props;
            //json[key]["PROPERTIES"]["Groups"]["EvacPanel0"] = grp1;
            ////
            //JObject grp2 = new JObject();
            //grp2["name"] = "Evacuation Panel 1";
            //grp2["~type"] = "AND";
            //props = Array2Object((JArray)o["EVACPANEL1"]["PROPERTIES"]["PROPERTY"]);
            //grp2["fields"] = props;
            //json[key]["PROPERTIES"]["Groups"]["EvacPanel1"] = grp2;
            ////
            //JObject grp3 = new JObject();
            //grp3["name"] = "Evacuation Panel 2";
            //grp3["~type"] = "AND";
            //props = Array2Object((JArray)o["EVACPANEL2"]["PROPERTIES"]["PROPERTY"]);
            //grp3["fields"] = props;
            //json[key]["PROPERTIES"]["Groups"]["EvacPanel2"] = grp3;
            ////
            //JObject grp4 = new JObject();
            //grp4["name"] = "Evacuation Panel 3";
            //grp4["~type"] = "AND";
            //props = Array2Object((JArray)o["EVACPANEL3"]["PROPERTIES"]["PROPERTY"]);
            //grp4["fields"] = props;
            //json[key]["PROPERTIES"]["Groups"]["EvacPanel3"] = grp4;
            ////
            //JObject grp5 = new JObject();
            //grp5["name"] = "Evacuation Panel 4";
            //grp5["~type"] = "AND";
            //props = Array2Object((JArray)o["EVACPANEL4"]["PROPERTIES"]["PROPERTY"]);
            //grp5["fields"] = props;
            //json[key]["PROPERTIES"]["Groups"]["EvacPanel4"] = grp5;
            //
            json[key]["PROPERTIES"]["OLD"] = o;
        }
        private static void ConvertEvacZoneGroup(JObject json)
        {
            if (Regex.IsMatch(json["ELEMENTS"]["iris"]["@PRODUCTNAME"].ToString(), @"REPEATER\s+?Iris[\w\W]+?Simpo$", RegexOptions.IgnoreCase) ||
                Regex.IsMatch(json["ELEMENTS"]["iris"]["@PRODUCTNAME"].ToString(), @"TFT[\w\W]+?Panel$", RegexOptions.IgnoreCase))
            {
                return;
            }
            foreach (JProperty p in json["ELEMENTS"]["iris_evac_zones"]["CONTAINS"])
            {
                string key = p.Name.ToString();
                CreateEvacZoneGroupGroups((JObject)json["ELEMENTS"], key);
            }
        }
        #endregion

        #region peripherial devices content nodes
        private static JObject ConvertPreripherialDevicesContentNodeChange(JObject json)
        {
            JObject res = json;
            JToken t = res["CHANGE"]["ELEMENT"];
            JArray chg = null;
            if (t.Type != JTokenType.Array)
            {
                chg = new JArray();
                chg.Add(t);
            }
            else
                chg = (JArray)res["CHANGE"]["ELEMENT"];
            JObject elements = Array2Object(chg);
            //
            return elements;
        }

        private static JObject ConvertPreripherialDevicesContentNodeProps(JObject json)
        {
            JObject res = json;
            JObject props = Array2Object((JArray)res["PROPERTIES"]["PROPERTY"]);
            //
            return props;
        }

        private static void ConvertPreripherialDevicesContentNodes(JObject json)
        {
            if (Regex.IsMatch(json["ELEMENTS"]["iris"]["@PRODUCTNAME"].ToString(), @"REPEATER\s+?Iris[\w\W]+?Simpo$", RegexOptions.IgnoreCase) ||
                Regex.IsMatch(json["ELEMENTS"]["iris"]["@PRODUCTNAME"].ToString(), @"^SIMPO[\w\W]+?Panel$", RegexOptions.IgnoreCase) ||
                Regex.IsMatch(json["ELEMENTS"]["iris"]["@PRODUCTNAME"].ToString(), @"^TFT[\w\W]+?Panel$", RegexOptions.IgnoreCase))
            {
                return;
            }
            JObject pdtypes = new JObject();
            foreach (JProperty p in json["ELEMENTS"]["iris_peripheral_devices"]["CONTAINS"])
            {
                string key = p.Name.ToString();
                JObject dev = (JObject)json["ELEMENTS"][key];
                JObject oldchange = (JObject)json["ELEMENTS"][key]["CHANGE"];
                JObject oldprops = (JObject)json["ELEMENTS"][key]["PROPERTIES"];
                json["ELEMENTS"][key]["OLD"] = new JObject();
                json["ELEMENTS"][key]["OLD"]["CHANGE"] = oldchange;
                json["ELEMENTS"][key]["OLD"]["PROPERTIES"] = oldprops;
                json["ELEMENTS"][key]["CHANGE"] = ConvertPreripherialDevicesContentNodeChange(dev);
                if (json["ELEMENTS"][key]["PROPERTIES"] == null)
                    json["ELEMENTS"][key]["PROPERTIES"] = new JObject();
                if (json["ELEMENTS"][key]["PROPERTIES"]["Groups"] == null)
                    json["ELEMENTS"][key]["PROPERTIES"]["Groups"] = new JObject();
                json["ELEMENTS"][key]["PROPERTIES"]["Groups"]["~noname"] = ConvertPreripherialDevicesContentNodeProps(dev);
                //if (json["ELEMENTS"][key]["PROPERTIES"]["Groups"]["~noname"] == null)
                //    json["ELEMENTS"][key]["PROPERTIES"]["Groups"]["~noname"] = new JObject();
                //json["ELEMENTS"][key]["PROPERTIES"]["Groups"]["~noname"]["name"] = "";
                //json["ELEMENTS"][key]["PROPERTIES"]["Groups"]["~noname"]["fields"] = ConvertPreripherialDevicesContentNodeProps(dev);
                //
                //JObject fields = (JObject)json["ELEMENTS"][key]["PROPERTIES"]["Groups"]["~noname"]["fields"];
                JObject fields = (JObject)json["ELEMENTS"][key]["PROPERTIES"]["Groups"]["~noname"];
                string val = fields["TYPE"]["@VALUE"].ToString();
                pdtypes[val] = key;
            }
            json["~pdtypes"] = pdtypes;
        }
        #endregion

        #region panel in network
        private static void CreateRepeaterPanelInNetworkGroups(JObject json, string key)
        {
            JArray proparr = (JArray)json[key]["PROPERTIES"]["PROPERTY"];
            JObject o = Array2Object(proparr);
            //groups
            json[key]["PROPERTIES"]["Groups"] = new JObject();
            //not grouped params
            json[key]["PROPERTIES"]["Groups"]["~noname"] = new JObject();
            json[key]["PROPERTIES"]["Groups"]["~noname"]["status"] = o["status"];
            json[key]["PROPERTIES"]["Groups"]["~noname"]["PANELFLAGS"] = o["PANELFLAGS"];
            //
            json[key]["PROPERTIES"]["OLD"] = o;
            //
            AddMissetFields((JObject)json[key]["PROPERTIES"]["Groups"], o, "~noname");
        }
        private static void CreateSimpoPanelPanelInNetworkGroups(JObject json, string key)
        {
            if (Regex.IsMatch(key, @"^SIMPO_PANELS$") && json[key] == null)
                key = "iris_panels_in_network";
            JArray proparr = (JArray)json[key]["PROPERTIES"]["PROPERTY"];
            JObject o = Array2Object(proparr);
            //groups
            json[key]["PROPERTIES"]["Groups"] = new JObject();
            //not grouped params
            json[key]["PROPERTIES"]["Groups"]["~noname"] = new JObject();
            json[key]["PROPERTIES"]["Groups"]["~noname"]["fields"] = new JObject();
            json[key]["PROPERTIES"]["Groups"]["~noname"]["fields"]["status"] = o["status"];
            json[key]["PROPERTIES"]["Groups"]["~noname"]["fields"]["panel_flags"] = o["panel_flags"];
            json[key]["PROPERTIES"]["Groups"]["~noname"]["fields"]["PANELFLAGS"] = o["PANELFLAGS"];
            //
            json[key]["PROPERTIES"]["OLD"] = o;
            //
            AddMissetFields((JObject)json[key]["PROPERTIES"]["Groups"], o, "~noname");
        }
        private static void CreatePanelInNetworkGroups(JObject json, string key)
        {
            JArray proparr = (JArray)json[key]["PROPERTIES"]["PROPERTY"];
            JObject o = Array2Object(proparr);
            //groups
            json[key]["PROPERTIES"]["Groups"] = new JObject();
            //not grouped params
            json[key]["PROPERTIES"]["Groups"]["~noname"] = new JObject();
            json[key]["PROPERTIES"]["Groups"]["~noname"]["IP"] = o["IP"];
            json[key]["PROPERTIES"]["Groups"]["~noname"]["Panel_Name"] = o["Panel_Name"];
            //
            JObject grp1 = new JObject();
            grp1["name"] = "Parameters";
            JObject f1 = new JObject();
            f1["ReceiveMessages"] = o["panel"];
            f1["ReceiveCommands"] = o["repeater"];
            f1["SendCommand"] = o["SendCommand"];
            grp1["fields"] = f1;
            json[key]["PROPERTIES"]["Groups"]["Parameters"] = grp1;
            //
            JObject grp2 = new JObject();
            grp2["name"] = "Panel Outputs";
            grp2["~type"] = "AND";
            JObject props = Array2Object((JArray)o["panel_flags"]["PROPERTIES"]["PROPERTY"]);
            grp2["fields"] = props;
            json[key]["PROPERTIES"]["Groups"]["Panel Outputs"] = grp2;
            //json[key]["PROPERTIES"]["Groups"]["panel_flags"] = grp2;
            //
            JObject grp3 = new JObject();
            grp3["name"] = "Missed";
            JObject f3 = new JObject();
            if (o["status"] != null) f3["status"] = o["status"];
            if (o["state"] != null) f3["state"] = o["state"];
            grp3["fields"] = f3;
            if (f3.Count > 0) json[key]["PROPERTIES"]["Groups"]["~noname1"] = grp3;
            //
            json[key]["PROPERTIES"]["OLD"] = o;
            //
            AddMissetFields((JObject)json[key]["PROPERTIES"]["Groups"], o, "~noname1");
        }
        private static void ConvertPanelInNetwork(JObject json)
        {
            foreach (JProperty p in json["ELEMENTS"]["iris_panels_in_network"]["CONTAINS"])
            {
                string key = p.Name.ToString();
                if (Regex.IsMatch(json["ELEMENTS"]["iris"]["@PRODUCTNAME"].ToString(), @"REPEATER\s+?Iris[\w\W]+?Simpo$", RegexOptions.IgnoreCase))
                {
                    CreateRepeaterPanelInNetworkGroups((JObject)json["ELEMENTS"], key);
                    return;
                }
                if (Regex.IsMatch(json["ELEMENTS"]["iris"]["@PRODUCTNAME"].ToString(), @"^SIMPO[\w\W]+?Panel$", RegexOptions.IgnoreCase))
                {
                    CreateSimpoPanelPanelInNetworkGroups((JObject)json["ELEMENTS"], key);
                    return;
                }
                CreatePanelInNetworkGroups((JObject)json["ELEMENTS"], key);
            }
        }
        #endregion

        #region change loops
        private static void SetLooDevsContent(JObject json)
        {
            JObject otypes = new JObject();
            JObject otypes_byloop = new JObject();
            foreach (JProperty p in json.SelectToken("ELEMENTS").Children())
            {
                string s = "";
                if (Regex.IsMatch(p.Name, @"(MODULES|SENSORS|TTENONE)$"))
                {
                    if (otypes_byloop[p.Name] == null)
                        otypes_byloop[p.Name] = new JObject();
                    JObject c = (JObject)p.Value;
                    if (c["CONTAINS"] != null)
                    {
                        c = new JObject((JObject)c["CONTAINS"]);
                        c = Array2Object(c["ELEMENT"]);
                        json["ELEMENTS"][p.Name]["CONTAINS"] = c;
                    }
                    else
                        c = new JObject((JObject)c["CHANGE"]);
                    if (((JProperty)c.First).Value.Type == JTokenType.Array)
                        c = Array2Object(((JProperty)c.First).Value);
                    foreach (JProperty pp in c.Children())
                    {
                        JObject oprops = (JObject)json["ELEMENTS"][pp.Name]["PROPERTIES"];
                        oprops = Array2Object(oprops["PROPERTY"]);
                        JObject grp = new JObject();
                        grp["name"] = "";
                        grp["fields"] = oprops;
                        JObject otype = (JObject)oprops["TYPE"];
                        byte btype = 0;
                        if (otype != null && otype["@VALUE"] != null)
                            btype = System.Convert.ToByte(otype["@VALUE"].ToString());
                        if (btype != 0)
                        {
                            otypes[btype.ToString()] = pp.Name;
                            ((JObject)otypes_byloop[p.Name])[btype.ToString()] = pp.Name;
                        }
                        json["ELEMENTS"][pp.Name]["PROPERTIES"]["Groups"] = new JObject();
                        json["ELEMENTS"][pp.Name]["PROPERTIES"]["Groups"]["~noname"] = grp;
                        JObject grpnew = (JObject)json["ELEMENTS"][pp.Name]["PROPERTIES"]["Groups"]["~noname"];
                        grpnew["fields"]["~path"] = grpnew["fields"].Path;
                        foreach (JProperty field in grpnew["fields"].Children())
                            if (field.Value.Type == JTokenType.Object)
                            {
                                JObject ofield = (JObject)field.Value;
                                ofield["~path"] = ofield.Path;
                            }
                    }
                    c = (JObject)p.Value;
                    if (c["PROPERTIES"] != null && c["PROPERTIES"]["Groups"] == null)
                    {
                        JObject cprops = (JObject)c["PROPERTIES"];
                        cprops = Array2Object(cprops["PROPERTY"]);
                        JObject grp = new JObject();
                        grp["name"] = "";
                        grp["fields"] = cprops;
                        c["PROPERTIES"]["Groups"] = new JObject();
                        c["PROPERTIES"]["Groups"]["~noname"] = grp;
                    }
                }
            }
            json["~devtypes"] = otypes;
            json["~devtypes_bynone"] = otypes_byloop;
        }
        private static void ChangeLoops(JObject json)
        {
            if (Regex.IsMatch(json["ELEMENTS"]["iris"]["@PRODUCTNAME"].ToString(), @"REPEATER\s+?Iris[\w\W]+?Simpo$", RegexOptions.IgnoreCase) ||
                Regex.IsMatch(json["ELEMENTS"]["iris"]["@PRODUCTNAME"].ToString(), @"^TFT[\w\W]+?Panel$", RegexOptions.IgnoreCase))
            {
                return;
            }
            JToken c = json.SelectToken("ELEMENTS.iris_loop_devices.CONTAINS");
            List<string> loop = new List<string>();
            foreach (JToken t in c.Children())
            {
                JProperty p = (JProperty)t;
                if (!Regex.IsMatch(p.Name, "^~"))
                    loop.Add(p.Name);
            }
            string key = Regex.Replace(loop[0], @"\d+$", "");
            JObject co = new JObject((JObject)c[loop[0]]);
            foreach (string pname in loop)
                ((JObject)c).Remove(pname);
            ((JObject)c)[key] = co;
            if (Regex.IsMatch(key, "8"))
                ((JObject)c)[key]["@MAX"] = 8;
            else if (!Regex.IsMatch(key, @"^SIMPO_", RegexOptions.IgnoreCase))
                ((JObject)c)[key]["@MAX"] = 4;
            else
                ((JObject)c)[key]["@MAX"] = 2;
            if (((JObject)c)[key]["~path"] == null)
                ((JObject)c)[key]["~path"] = ((JObject)c)[key].Path;
            ((JObject)c)[key]["~path"] = Regex.Replace(((JObject)c)[key]["~path"].ToString(), @"\d+$", "");
            //
            SetLooDevsContent(json);
        }
        #endregion

        #region Repeater IRIS Simpo
        private static Dictionary<string, JObject> ElementProperties(JArray elements, string ekey)
        {
            Dictionary<string, JObject> res = new Dictionary<string, JObject>();
            JObject el = null;
            foreach (JToken t in elements)
            {
                el = (JObject)t;
                if (el["@ID"].ToString() == ekey) break;
            }
            if (el != null)
            {
                if (el["PROPERTIES"]["PROPERTY"].Type == JTokenType.Array)
                {
                    JArray props = (JArray)el["PROPERTIES"]["PROPERTY"];
                    foreach (JToken t in props)
                    {
                        JObject p = (JObject)t;
                        string pkey = p["@ID"].ToString();
                        if (!res.ContainsKey(pkey)) res.Add(pkey, p);
                    }
                }
                else if (el["PROPERTIES"]["PROPERTY"].Type == JTokenType.Object)
                {
                    JObject p = (JObject)el["PROPERTIES"]["PROPERTY"];
                    string pkey = p["@ID"].ToString();
                    if (!res.ContainsKey(pkey)) res.Add(pkey, p);
                }
            }
            //
            return res;
        }
        private static JArray PropertiesFromContentItems(JArray elements, string ckey)
        {
            JArray res = new JArray();
            //
            JObject el = null;
            foreach (JToken t in elements)
            {
                el = (JObject)t;
                if (el["@ID"].ToString() == ckey) break;
            }
            if (el == null) return res;
            JArray content = (JArray)el["CONTAINS"]["ELEMENT"];
            Dictionary<string, JObject> props = new Dictionary<string, JObject>();
            foreach (JToken t in content)
            {
                Dictionary<string, JObject> eprops = ElementProperties(elements, t["@ID"].ToString());
                foreach (string k in eprops.Keys)
                    if (!props.ContainsKey(k))
                        props.Add(k, eprops[k]);
            }
            foreach (string k in props.Keys)
                res.Add(props[k]);
            //
            return res;
        }
        private static Dictionary<string, JObject> ItemContent(JArray elements, string itemid)
        {
            Dictionary<string, JObject> res = new Dictionary<string, JObject>();
            JObject el = null;
            foreach (JToken t in elements)
            {
                el = (JObject)t;
                if (el["@ID"].ToString() == itemid) break;
            }
            if (el == null) return res;
            JArray content;
            if (el["CONTAINS"]["ELEMENT"].Type == JTokenType.Array)
                content = (JArray)el["CONTAINS"]["ELEMENT"];
            else
            {
                content = new JArray();
                content.Add(el["CONTAINS"]["ELEMENT"]);
            }
            foreach (JToken t in content)
            {
                el = (JObject)t;
                res.Add(el["@ID"].ToString(), el);
            }
            return res;
        }
        private static string ReorderRepeaterTemplate(string json)
        {
            JObject o = JObject.Parse(json);
            JArray elements = (JArray)o["ELEMENTS"]["ELEMENT"];
            JArray content = (JArray)elements[0]["CONTAINS"]["ELEMENT"];
            Dictionary<string, JObject> dcontent = new Dictionary<string, JObject>();
            foreach (JToken t in content)
            {
                JObject el = (JObject)t;
                string ekey = el["@ID"].ToString();
                if (Regex.IsMatch(ekey, @"simpo[\w\W]+?general[\w\W]+?settings[\w\W]*?_r$", RegexOptions.IgnoreCase))
                {
                    JArray props = PropertiesFromContentItems(elements, ekey);
                    JObject el0 = (JObject)elements[0];
                    el0["PROPERTIES"] = new JObject();
                    el0["PROPERTIES"]["PROPERTY"] = props;
                }
                else
                {
                    dcontent.Add(ekey, el);
                    Dictionary<string, JObject> c = ItemContent(elements, el["@ID"].ToString());
                    foreach (string ckey in c.Keys) dcontent.Add(ckey, c[ckey]);
                }
            }
            JArray content_new = new JArray();
            foreach (string ckey in dcontent.Keys) content_new.Add(dcontent[ckey]);
            elements[0]["CONTAINS"]["ELEMENT"] = content_new;
            //
            return o.ToString();
        }
        private static void RemoveRepeaterSimpoElements(JObject json)
        {
            JObject elements = (JObject)json["ELEMENTS"];
            if (elements["SIMPO_PANELS_R"] == null) return;
            if (elements["SIMPO_GENERAL_SETTINGS_R"] != null) elements.Remove("SIMPO_GENERAL_SETTINGS_R");
            if (elements["SIMPO_ACCESSCODE_R"] != null) elements.Remove("SIMPO_ACCESSCODE_R");
            if (elements["SIMPO_PANELSETTINGS_R"] != null) elements.Remove("SIMPO_PANELSETTINGS_R");
            if (elements["SIMPO_LOGO_R"] != null) elements.Remove("SIMPO_LOGO_R");
            JObject panels = new JObject((JObject)elements["SIMPO_PANELS_R"]);
            elements.Remove("SIMPO_PANELS_R");
            elements["SIMPO_PANELS_R"] = panels;
        }
        #endregion

        #region Simpo panel
        private static string ReorderSimpoPanelTemplate(string json)
        {
            JObject o = JObject.Parse(json);
            JArray elements = (JArray)o["ELEMENTS"]["ELEMENT"];
            JArray content = (JArray)elements[0]["CONTAINS"]["ELEMENT"];
            Dictionary<string, JObject> dcontent = new Dictionary<string, JObject>();
            foreach (JToken t in content)
            {
                JObject el = (JObject)t;
                string ekey = el["@ID"].ToString();
                if (Regex.IsMatch(ekey, @"simpo[\w\W]+?general[\w\W]+?settings$", RegexOptions.IgnoreCase))
                {
                    JArray props = PropertiesFromContentItems(elements, ekey);
                    JObject el0 = (JObject)elements[0];
                    el0["PROPERTIES"] = new JObject();
                    el0["PROPERTIES"]["PROPERTY"] = props;
                }
                else if (Regex.IsMatch(ekey, @"simpo[\w\W]+?paneloutputs$", RegexOptions.IgnoreCase))
                {
                    JArray props = PropertiesFromContentItems(elements, ekey);
                    JObject elout = null;
                    foreach (JObject oel in elements)
                        if (oel["@ID"] != null && oel["@ID"].ToString() == "SIMPO_PANELOUTPUTS")
                        {
                            elout = oel;
                            break;
                        }
                    if (elout != null)
                    {
                        elout["PROPERTIES"] = new JObject();
                        elout["PROPERTIES"]["PROPERTY"] = props;

                    }
                }
                else
                {
                    dcontent.Add(ekey, el);
                    Dictionary<string, JObject> c = ItemContent(elements, el["@ID"].ToString());
                    foreach (string ckey in c.Keys) dcontent.Add(ckey, c[ckey]);
                }
            }
            JArray content_new = new JArray();
            foreach (string ckey in dcontent.Keys) content_new.Add(dcontent[ckey]);
            //elements[0]["CONTAINS"]["ELEMENT"] = content_new;
            //
            return o.ToString();
        }
        private static void ChangeSimpoPanelComtent(JObject json)
        {
            JObject c = (JObject)json["ELEMENTS"]["iris"]["CONTAINS"];
            JObject content = new JObject();
            content["SIMPO_GENERAL_SETTINGS"] = c["SIMPO_GENERAL_SETTINGS"];
            content["SIMPO_PANELOUTPUTS"] = c["SIMPO_PANELOUTPUTS"];
            content["iris_zones"] = c["iris_zones"];
            content["iris_evac_zones"] = c["iris_evac_zones"];
            content["iris_network"] = c["iris_network"];
            //content["iris_panels_in_network"] = c["iris_network"];
        }
        private static void CreatePaneloutputsGroups(JObject elements)
        {
            if (elements["SIMPO_PANELOUTPUTS"] == null)
                return;
            JArray proparr = (JArray)elements["SIMPO_PANELOUTPUTS"]["PROPERTIES"]["PROPERTY"];
            JObject o = Array2Object(proparr);
            //groups
            elements["SIMPO_PANELOUTPUTS"]["PROPERTIES"]["Groups"] = new JObject();
            //
            JObject grp1 = new JObject();
            grp1["name"] = "Sounders";
            JObject f1 = new JObject();
            f1["SounderDelayMode"] = o["SounderDelayMode"];
            grp1["fields"] = f1;
            elements["SIMPO_PANELOUTPUTS"]["PROPERTIES"]["Groups"]["Sounders"] = grp1;
            //
            JObject grp2 = new JObject();
            grp2["name"] = "Fire brigade";
            JObject f2 = new JObject();
            f2["FireBrigadeDelayMode"] = o["FireBrigadeDelayMode"];
            grp2["fields"] = f2;
            elements["SIMPO_PANELOUTPUTS"]["PROPERTIES"]["Groups"]["FireBrigade"] = grp2;
            //
            JObject grp3 = new JObject();
            grp3["name"] = "Relay 1";
            JObject f3 = new JObject();
            f3["ACTIVATION1r"] = o["ACTIVATION1r"];
            f3["NAME_OUT1r"] = o["NAME_OUT1r"];
            grp3["fields"] = f3;
            elements["SIMPO_PANELOUTPUTS"]["PROPERTIES"]["Groups"]["Relay1"] = grp3;
            //
            JObject grp4 = new JObject();
            grp4["name"] = "Relay 2";
            JObject f4 = new JObject();
            f4["ACTIVATION2r"] = o["ACTIVATION2r"];
            f4["NAME_OUT2r"] = o["NAME_OUT2r"];
            grp4["fields"] = f4;
            elements["SIMPO_PANELOUTPUTS"]["PROPERTIES"]["Groups"]["Relay2"] = grp4;
            //
            JObject grp5 = new JObject();
            grp5["name"] = "Relay 3";
            JObject f5 = new JObject();
            f5["ACTIVATION3r"] = o["ACTIVATION3r"];
            f5["NAME_OUT3r"] = o["NAME_OUT3r"];
            grp5["fields"] = f5;
            elements["SIMPO_PANELOUTPUTS"]["PROPERTIES"]["Groups"]["Relay3"] = grp5;
            //
            JObject grp6 = new JObject();
            grp6["name"] = "Relay 4";
            JObject f6 = new JObject();
            f6["ACTIVATION4r"] = o["ACTIVATION4r"];
            f6["NAME_OUT4r"] = o["NAME_OUT4r"];
            grp6["fields"] = f6;
            elements["SIMPO_PANELOUTPUTS"]["PROPERTIES"]["Groups"]["Relay4"] = grp6;
            //
            elements["SIMPO_PANELOUTPUTS"]["PROPERTIES"]["OLD"] = o;
        }
        private static void ConvertSimpoPaneloutputs(JObject json, JObject _pages)
        {
            CreatePaneloutputsGroups((JObject)json["ELEMENTS"]);
            JObject ac = (JObject)json["ELEMENTS"]["SIMPO_PANELOUTPUTS"];
            if (ac == null) return;
            ac["title"] = _pages["simpo_paneloutputs"]["title"];
            ac["left"] = _pages["simpo_paneloutputs"]["left"];
            ac["right"] = _pages["simpo_paneloutputs"]["right"];
            ac["breadcrumbs"] = _pages["simpo_paneloutputs"]["breadcrumbs"];
        }
        private static void SimpoPanelExtractPanelsFromNetwork(JObject json, JObject _pages)
        {
            JObject elements = (JObject)json["ELEMENTS"];
            if (elements["iris"] == null || elements["iris"]["@PRODUCTNAME"] == null) return;
            string product = elements["iris"]["@PRODUCTNAME"].ToString();
            if (!Regex.IsMatch(product, @"^SIMPO\s+?panel$", RegexOptions.IgnoreCase)) return;
            //
            JObject content = (JObject)elements["iris"]["CONTAINS"];
            JObject content_new = new JObject();
            foreach (JProperty p in content.Properties())
            {
                content_new[p.Name] = p.Value;
                if (p.Name == "iris_network")
                {
                    JObject o = new JObject();
                    o["@MIN"] = "1";
                    o["@MAX"] = "1";
                    o["title"] = _pages["iris_panels_in_network"]["title"];
                    o["left"] = _pages["iris_panels_in_network"]["left"];
                    o["right"] = _pages["iris_panels_in_network"]["right"];
                    o["breadcrumbs"] = _pages["iris_panels_in_network"]["breadcrumbs"];
                    content_new["iris_panels_in_network"] = o;
                    JObject onet = (JObject)elements["iris_network"];
                    elements["iris_panels_in_network"] = new JObject(onet);
                    ((JObject)elements["iris_panels_in_network"]).Remove("PROPERTIES");
                    elements["iris_panels_in_network"]["@PRODUCTNAME"] = "Panels";
                    onet.Remove("CONTAINS");
                }
            }
            elements["iris"]["CONTAINS"] = content_new;
        }
        private static void ConvertMIMICPanelGroups(JObject _panel)
        {
            if (_panel == null) return;
            //
            JArray proparr = (JArray)_panel["PROPERTIES"]["PROPERTY"];
            JObject o = Array2Object(proparr);
            //groups
            _panel["PROPERTIES"]["Groups"] = new JObject();
            //
            JObject grp1 = new JObject();
            grp1["name"] = "";
            JObject f1 = new JObject();
            f1["NAME"] = o["NAME"];
            f1["LOOP"] = o["LOOP"];
            f1["ADDRESS"] = o["ADDRESS"];
            grp1["fields"] = f1;
            _panel["PROPERTIES"]["Groups"]["~noname"] = grp1;
            //
            _panel["PROPERTIES"]["OLD"] = o;
        }
        private static void ConvertMIMICPanelsGroups(JObject json)
        {
            JObject content = (JObject)json["ELEMENTS"]["iris"];
            if (content == null) return;
            content = (JObject)content["CONTAINS"];
            if (content == null) return;
            JProperty pmimic = null;
            foreach (JProperty p in content.Properties())
                if (Regex.IsMatch(p.Name, @"^SIMPO_MIMIC"))
                {
                    pmimic = p;
                    break;
                }
            if (pmimic == null) return;
            JObject omimicpanels = (JObject)json["ELEMENTS"][pmimic.Name];
            omimicpanels["title"] = pmimic.Value["title"].ToString();
            omimicpanels["left"] = pmimic.Value["left"].ToString();
            omimicpanels["right"] = pmimic.Value["right"].ToString();
            omimicpanels["breadcrumbs"] = JArray.Parse(pmimic.Value["breadcrumbs"].ToString());
            content = (JObject)omimicpanels["CONTAINS"];
            if (content == null) return;
            if (content["ELEMENT"] != null && content["ELEMENT"].Type == JTokenType.Array)
            {
                JObject o = Array2Object((JArray)content["ELEMENT"]);
                omimicpanels["CONTAINS"] = o;
                content = o;
            }
            foreach (JProperty p in content.Properties())
            {
                JObject opanel = (JObject)json["ELEMENTS"][p.Name];
                ConvertMIMICPanelGroups(opanel);
            }
        }
        private static void ConvertMIMICOut(JObject json)
        {
            JObject element = (JObject)json["ELEMENTS"]["SIMPO_MIMICOUT"];
            if (element == null) return;
            //
            JArray proparr = null;
            if (element["PROPERTIES"]["PROPERTY"].Type == JTokenType.Array)
                proparr = (JArray)element["PROPERTIES"]["PROPERTY"];
            else
            {
                proparr = new JArray();
                proparr.Add(element["PROPERTIES"]["PROPERTY"]);
            }
            JObject o = Array2Object(proparr);
            //groups
            element["PROPERTIES"]["Groups"] = new JObject();
            //
            JObject grp1 = new JObject();
            grp1["name"] = "";
            JObject f1 = new JObject();
            f1["ACTIVATION1"] = o["ACTIVATION1"];
            grp1["fields"] = f1;
            element["PROPERTIES"]["Groups"]["~noname"] = grp1;
            //
            element["PROPERTIES"]["OLD"] = o;
        }
        private static void RemoveSimpoElements(JObject json)
        {
            JObject elements = (JObject)json["ELEMENTS"];
            if (elements["SIMPO_GENERAL_SETTINGS"] != null) elements.Remove("SIMPO_GENERAL_SETTINGS");
            if (elements["SIMPO_PANELSETTINGS"] != null) elements.Remove("SIMPO_PANELSETTINGS");
            if (elements["SIMPO_ACCESSCODE"] != null) elements.Remove("SIMPO_ACCESSCODE");
            if (elements["SIMPO_DAYNIGHT"] != null) elements.Remove("SIMPO_DAYNIGHT");
            if (elements["SIMPO_DELAYT1"] != null) elements.Remove("SIMPO_DELAYT1");
            if (elements["SIMPO_SOUNDERSMODE"] != null) elements.Remove("SIMPO_SOUNDERSMODE");
            if (elements["SIMPO_LOGO"] != null) elements.Remove("SIMPO_LOGO");
            if (elements["SIMPO_SOUNDERS"] != null) elements.Remove("SIMPO_SOUNDERS");
            if (elements["SIMPO_FIREBRIGADE"] != null) elements.Remove("SIMPO_FIREBRIGADE");
            if (elements["SIMPO_RELAY1"] != null) elements.Remove("SIMPO_RELAY1");
            if (elements["SIMPO_RELAY2"] != null) elements.Remove("SIMPO_RELAY2");
            if (elements["SIMPO_RELAY3"] != null) elements.Remove("SIMPO_RELAY3");
            if (elements["SIMPO_RELAY4"] != null) elements.Remove("SIMPO_RELAY4");
        }
        private static void ChangeSIMPOPanelTTENONE(JObject json)
        {
            if (Regex.IsMatch(json["ELEMENTS"]["iris"]["@PRODUCTNAME"].ToString(), @"SIMPO\s+?panel$", RegexOptions.IgnoreCase))
            {
                JObject elements = (JObject)json["ELEMENTS"];
                JObject ttenone1 = new JObject((JObject)elements["SIMPO_TTENONE"]);
                JObject ttenone2 = new JObject(ttenone1);
                //elements.Remove("SIMPO_TTENONE");
                elements["SIMPO_TTENONE1"] = ttenone1;
                elements["SIMPO_TTENONE2"] = ttenone2;
            }
        }
        #endregion
        public static string Convert(string json, JObject _pages)
        {
            if (Regex.IsMatch(json, @"""@ID""\s*?:\s*?""R_PANEL""", RegexOptions.IgnoreCase))
                json = ReorderRepeaterTemplate(json);
            else if (Regex.IsMatch(json, @"""@ID""\s*?:\s*?""SIMPO_PANEL""", RegexOptions.IgnoreCase))
                json = ReorderSimpoPanelTemplate(json);
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
                            JObject content = Array2Object((JArray)first.Value);
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
                        elements["iris"]["title"] = _pages["iris"]["title"];// "Iris";
                        elements["iris"]["left"] = _pages["iris"]["left"];//"IRIS/divIRIS.html";
                        elements["iris"]["right"] = _pages["iris"]["right"];//"IRIS/IRISPANEL.html";
                        elements["iris"]["breadcrumbs"] = _pages["iris"]["breadcrumbs"];//JArray.Parse("[\"index\"]");
                        CreateMainGroups(elements);
                        //
                        if (p.Name != "ELEMENT")
                            o1[p.Name] = elements;
                        else
                            o1["ELEMENTS"] = elements;
                    }
                }

            }
            ConvertMainArraysLeft(o1);
            ConvertAccessCode(o1, _pages);
            SimpoPanelExtractPanelsFromNetwork(o1, _pages);
            ConvertPanelsInNetwork(o1, _pages);
            ConvertNetwork(o1, _pages);
            ConvertInputs(o1, _pages);
            ConvertFATFBF(o1, _pages);
            ConvertInputGroups(o1, _pages);
            ConvertOutputs(o1, _pages);
            ConvertZones(o1, _pages);
            ConvertEvacZones(o1, _pages);
            ConvertPeripheralDevices(o1, _pages);
            ConvertLoopDevices(o1, _pages);
            ConvertInputsGroup(o1);
            ConvertInput(o1);
            ConvertOutput(o1);
            ConvertZone(o1);
            ConvertEvacZoneGroup(o1);
            ConvertPreripherialDevicesContentNodes(o1);
            ConvertPanelInNetwork(o1);
            RemoveRepeaterSimpoElements(o1);
            RemoveSimpoElements(o1);
            ConvertSimpoPaneloutputs(o1, _pages);
            ConvertMIMICPanelsGroups(o1);
            ConvertMIMICOut(o1);
            //ChangeSIMPOPanelTTENONE(o1);
            //
            cXml.Arrays2Objects(o1, true);
            //
            ChangeLoops(o1);
            doINC(o1);
            //
            return o1.ToString();
        }
    }
}