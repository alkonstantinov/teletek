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
            else if (Regex.IsMatch(name, @"IRIS[\w\W]*?_NETWORK$", RegexOptions.IgnoreCase))
                name = "iris_network";
            else if (Regex.IsMatch(name, @"IRIS[\w\W]*?_PANELSINNETWORK$", RegexOptions.IgnoreCase))
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
            else if (Regex.IsMatch(name, @"IRIS[\w\W]*?_EVAC_ZONES_GROUPS$", RegexOptions.IgnoreCase))
                name = "iris_evac_zones";
            else if (Regex.IsMatch(name, @"IRIS[\w\W]*?_PERIPHERIALDEVICES$", RegexOptions.IgnoreCase))
                name = "iris_peripheral_devices";
            else if (Regex.IsMatch(name, @"IRIS[\w\W]*?_LOOPDEVICES$", RegexOptions.IgnoreCase))
                name = "iris_loop_devices";
            else if (Regex.IsMatch(name, @"IRIS[\w\W]*?_PANEL$", RegexOptions.IgnoreCase))
                name = "iris";
            return name;
        }

        private static void TranslateObjectsKeys(JObject content)
        {
            List<JToken> from = new List<JToken>();
            List<JToken> to = new List<JToken>();
            foreach (JToken t in (JToken)content)
            {
                string name = cIRIS.TranslateKey(((JProperty)t).Name);
                JProperty p = new JProperty(name, ((JProperty)t).Value);
                from.Add(t);
                to.Add((JToken)p);
            }
            for (int i = 0; i < from.Count; i++)
                from[i].Replace(to[i]);
        }
        #endregion

        #region main
        private static void ChangeContent(JObject content, JObject _pages)
        {
            TranslateObjectsKeys(content);
            //
            content["iris_access_code"]["title"] = _pages["iris_access_code"]["title"];//"Access codes";
            content["iris_access_code"]["left"] = _pages["iris_access_code"]["left"]; //"IRIS/divIRIS.html";
            content["iris_access_code"]["right"] = _pages["iris_access_code"]["right"];// "IRIS/access.html";
            content["iris_access_code"]["breadcrumbs"] = _pages["iris_access_code"]["breadcrumbs"];// JArray.Parse("[\"index\", \"iris\"]");
            //
            content["iris_network"]["title"] = _pages["iris_network"]["title"];// "Network";
            content["iris_network"]["left"] = _pages["iris_network"]["left"];// "IRIS/divIRIS.html";
            content["iris_network"]["right"] = _pages["iris_network"]["right"];// "IRIS/network.html";
            content["iris_network"]["breadcrumbs"] = _pages["iris_network"]["breadcrumbs"];// JArray.Parse("[\"index\", \"iris\"]");
            //
            content["iris_panels_in_network"]["title"] = _pages["iris_panels_in_network"]["title"];// "Panels in network";
            content["iris_panels_in_network"]["left"] = _pages["iris_panels_in_network"]["left"];//"IRIS/divIRIS.html";
            content["iris_panels_in_network"]["right"] = _pages["iris_panels_in_network"]["right"];//"IRIS/panels_in_network.html";
            content["iris_panels_in_network"]["breadcrumbs"] = _pages["iris_panels_in_network"]["breadcrumbs"];//JArray.Parse("[\"index\", \"iris\"]");
            //
            content["iris_inputs"]["title"] = _pages["iris_inputs"]["title"];// "Inputs";
            content["iris_inputs"]["left"] = _pages["iris_inputs"]["left"];//"IRIS/divIRIS.html";
            content["iris_inputs"]["right"] = _pages["iris_inputs"]["right"];//"IRIS/input.html";
            content["iris_inputs"]["breadcrumbs"] = _pages["iris_inputs"]["breadcrumbs"];//JArray.Parse("[\"index\", \"iris\"]");
            //
            content["iris_inputs_group"]["title"] = _pages["iris_inputs_group"]["title"];// "Input Groups";
            content["iris_inputs_group"]["left"] = _pages["iris_inputs_group"]["left"];//"IRIS/divIRIS.html";
            content["iris_inputs_group"]["right"] = _pages["iris_inputs_group"]["right"];//"IRIS/inputs_group.html";
            content["iris_inputs_group"]["breadcrumbs"] = _pages["iris_inputs_group"]["breadcrumbs"];//JArray.Parse("[\"index\", \"iris\"]");
            //
            content["iris_outputs"]["title"] = _pages["iris_outputs"]["title"];// "Output";
            content["iris_outputs"]["left"] = _pages["iris_outputs"]["left"];//"IRIS/divIRIS.html";
            content["iris_outputs"]["right"] = _pages["iris_outputs"]["right"];//"IRIS/output.html";
            content["iris_outputs"]["breadcrumbs"] = _pages["iris_outputs"]["breadcrumbs"];//JArray.Parse("[\"index\", \"iris\"]");
            //
            content["iris_fat_fbf"]["title"] = _pages["iris_fat_fbf"]["title"];// "FAT FBF";
            content["iris_fat_fbf"]["left"] = _pages["iris_fat_fbf"]["left"];//"IRIS/divIRIS.html";
            content["iris_fat_fbf"]["right"] = _pages["iris_fat_fbf"]["right"];//"IRIS/fat-fbf.html";
            content["iris_fat_fbf"]["breadcrumbs"] = _pages["iris_fat_fbf"]["breadcrumbs"];//JArray.Parse("[\"index\", \"iris\"]");
            //
            content["iris_zones"]["title"] = _pages["iris_zones"]["title"];// "Zones";
            content["iris_zones"]["left"] = _pages["iris_zones"]["left"];//"IRIS/divIRIS.html";
            content["iris_zones"]["right"] = _pages["iris_zones"]["right"];//"IRIS/zone.html";
            content["iris_zones"]["breadcrumbs"] = _pages["iris_zones"]["breadcrumbs"];//JArray.Parse("[\"index\", \"iris\"]");
            //
            content["iris_evac_zones"]["title"] = _pages["iris_evac_zones"]["title"];// "Evac zones";
            content["iris_evac_zones"]["left"] = _pages["iris_evac_zones"]["left"];//"IRIS/divIRIS.html";
            content["iris_evac_zones"]["right"] = _pages["iris_evac_zones"]["right"];//"IRIS/zone_evac.html";
            content["iris_evac_zones"]["breadcrumbs"] = _pages["iris_evac_zones"]["breadcrumbs"];//JArray.Parse("[\"index\", \"iris\"]");
            //
            content["iris_peripheral_devices"]["title"] = _pages["iris_peripheral_devices"]["title"];// "Peripheral devices";
            content["iris_peripheral_devices"]["left"] = _pages["iris_peripheral_devices"]["left"];//"IRIS/divIRIS.html";
            content["iris_peripheral_devices"]["right"] = _pages["iris_peripheral_devices"]["right"];//"IRIS/periph_devices.html";
            content["iris_peripheral_devices"]["breadcrumbs"] = _pages["iris_peripheral_devices"]["breadcrumbs"];//JArray.Parse("[\"index\", \"iris\"]");
            //
            content["iris_loop_devices"]["title"] = _pages["iris_loop_devices"]["title"];// "Loop devices";
            content["iris_loop_devices"]["left"] = _pages["iris_loop_devices"]["left"];//"IRIS/divIRIS.html";
            content["iris_loop_devices"]["right"] = _pages["iris_loop_devices"]["right"];//"IRIS/loop_devices.html";
            content["iris_loop_devices"]["breadcrumbs"] = _pages["iris_loop_devices"]["breadcrumbs"];//JArray.Parse("[\"index\", \"iris\"]");
        }

        private static void CreateMainGroups(JObject json)
        {
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
        }

        private static void ConvertAccessCode(JObject json, JObject _pages)
        {
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
        private static void CreateNetworkGroups(JObject json)
        {
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
            f2["Gateway"] = o["Router"];
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
            json["iris_network"]["PROPERTIES"]["Groups"]["~noname1"]["PanelEvacNumber"] = o["PanelEvacNumber"];
            json["iris_network"]["PROPERTIES"]["Groups"]["~noname1"]["emacETHADDR"] = o["emacETHADDR0"];
            json["iris_network"]["PROPERTIES"]["Groups"]["~noname1"]["emacETHADDR"]["@TYPE"] = "EMAC";
            json["iris_network"]["PROPERTIES"]["Groups"]["~noname1"]["emacETHADDR"]["@TEXT"] = "EMAC";
            //
            json["iris_network"]["PROPERTIES"]["OLD"] = o;
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
        private static void ConvertFATFBF(JObject json, JObject _pages)
        {
            JObject ac = (JObject)json["ELEMENTS"]["iris_fat_fbf"];
            ac["title"] = _pages["iris_fat_fbf"]["title"];
            ac["left"] = _pages["iris_fat_fbf"]["left"];
            ac["right"] = _pages["iris_fat_fbf"]["right"];
            ac["breadcrumbs"] = _pages["iris_fat_fbf"]["breadcrumbs"];
            JObject contains = Contains2Object((JObject)json["ELEMENTS"]["iris_fat_fbf"]["CONTAINS"]);
            json["ELEMENTS"]["iris_fat_fbf"]["CONTAINS"] = contains;
        }
        #endregion

        #region input groups
        private static void ConvertInputGroups(JObject json, JObject _pages)
        {
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
            JObject ac = (JObject)json["ELEMENTS"]["iris_loop_devices"];
            ac["title"] = _pages["iris_loop_devices"]["title"];
            ac["left"] = _pages["iris_loop_devices"]["left"];
            ac["right"] = _pages["iris_loop_devices"]["right"];
            ac["breadcrumbs"] = _pages["iris_loop_devices"]["breadcrumbs"];
            JObject contains = Contains2Object((JObject)json["ELEMENTS"]["iris_loop_devices"]["CONTAINS"]);
            json["ELEMENTS"]["iris_loop_devices"]["CONTAINS"] = contains;
        }
        #endregion

        #region input group
        private static void ConvertInputsGroup(JObject json)
        {
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
        }
        private static void ConvertInput(JObject json)
        {
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
            //JObject grp4 = new JObject();
            //grp4["name"] = "OR/AND inputs";
            //grp4["fields"] = new JObject();
            //grp4["fields"]["ORINPUTS"] = o["ORINPUTS"];
            //grp4["fields"]["ANDINPUTS"] = o["ANDINPUTS"];
            //json[key]["PROPERTIES"]["Groups"]["ANDORInputs"] = grp4;
            //
            json[key]["PROPERTIES"]["OLD"] = o;
        }
        private static void ConvertOutput(JObject json)
        {
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
            f1["FIREPROTECTIONDELAY"] = o["FIREPROTECTIONDELAY"];
            grp1["fields"] = f1;
            json[key]["PROPERTIES"]["Groups"]["Delays"] = grp1;
            //
            JObject grp2 = new JObject();
            grp2["name"] = "Zone";
            JObject f2 = new JObject();
            f2["ZoneGroupA"] = o["ZONEGROUP"];
            f2["ZoneGroupB"] = o["ZONEGROUP2"];
            f2["ZoneGroupC"] = o["ZONEGROUP3"];
            f2["ZoneSounders"] = o["ZONESOUNDERS"];
            grp2["fields"] = f2;
            json[key]["PROPERTIES"]["Groups"]["Zone"] = grp2;
            //
            JObject grp3 = new JObject();
            grp3["name"] = "Sounder";
            JObject f3 = new JObject();
            f3["SounderGroupA"] = o["SOUNDERGROUP"];
            f3["SounderGroupB"] = o["SOUNDERGROUP2"];
            f3["SounderGroupC"] = o["SOUNDERGROUP3"];
            grp3["fields"] = f3;
            json[key]["PROPERTIES"]["Groups"]["Sounder"] = grp3;
            //
            json[key]["PROPERTIES"]["OLD"] = o;
        }
        private static void ConvertZone(JObject json)
        {
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
            JObject grp1 = new JObject();
            grp1["name"] = "Evacuation Panel 0";
            grp1["~type"] = "AND";
            JObject props = Array2Object((JArray)o["EVACPANEL0"]["PROPERTIES"]["PROPERTY"]);
            grp1["fields"] = props;
            json[key]["PROPERTIES"]["Groups"]["EvacPanel0"] = grp1;
            //
            JObject grp2 = new JObject();
            grp2["name"] = "Evacuation Panel 1";
            grp2["~type"] = "AND";
            props = Array2Object((JArray)o["EVACPANEL1"]["PROPERTIES"]["PROPERTY"]);
            grp2["fields"] = props;
            json[key]["PROPERTIES"]["Groups"]["EvacPanel1"] = grp2;
            //
            JObject grp3 = new JObject();
            grp3["name"] = "Evacuation Panel 2";
            grp3["~type"] = "AND";
            props = Array2Object((JArray)o["EVACPANEL2"]["PROPERTIES"]["PROPERTY"]);
            grp3["fields"] = props;
            json[key]["PROPERTIES"]["Groups"]["EvacPanel2"] = grp3;
            //
            JObject grp4 = new JObject();
            grp4["name"] = "Evacuation Panel 3";
            grp4["~type"] = "AND";
            props = Array2Object((JArray)o["EVACPANEL3"]["PROPERTIES"]["PROPERTY"]);
            grp4["fields"] = props;
            json[key]["PROPERTIES"]["Groups"]["EvacPanel3"] = grp4;
            //
            JObject grp5 = new JObject();
            grp5["name"] = "Evacuation Panel 4";
            grp5["~type"] = "AND";
            props = Array2Object((JArray)o["EVACPANEL4"]["PROPERTIES"]["PROPERTY"]);
            grp5["fields"] = props;
            json[key]["PROPERTIES"]["Groups"]["EvacPanel4"] = grp5;
            //
            json[key]["PROPERTIES"]["OLD"] = o;
        }
        private static void ConvertEvacZoneGroup(JObject json)
        {
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
            JObject elements = Array2Object((JArray)res["CHANGE"]["ELEMENT"]);
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
            JObject props = Array2Object((JArray)o["panel_flags"]["PROPERTIES"]["PROPERTY"]);
            grp2["fields"] = props;
            json[key]["PROPERTIES"]["Groups"]["Panel Outputs"] = grp2;
            //
            JObject grp3 = new JObject();
            grp3["name"] = "Missed";
            JObject f3 = new JObject();
            f3["status"] = o["status"];
            f3["state"] = o["state"];
            grp3["fields"] = f3;
            json[key]["PROPERTIES"]["Groups"]["~noname1"] = grp3;
            //
            json[key]["PROPERTIES"]["OLD"] = o;
        }
        private static void ConvertPanelInNetwork(JObject json)
        {
            foreach (JProperty p in json["ELEMENTS"]["iris_panels_in_network"]["CONTAINS"])
            {
                string key = p.Name.ToString();
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
            else
                ((JObject)c)[key]["@MAX"] = 4;
            if (((JObject)c)[key]["~path"] == null)
                ((JObject)c)[key]["~path"] = ((JObject)c)[key].Path;
            ((JObject)c)[key]["~path"] = Regex.Replace(((JObject)c)[key]["~path"].ToString(), @"\d+$", "");
            //
            SetLooDevsContent(json);
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
            //
            cXml.Arrays2Objects(o1, true);
            //
            ChangeLoops(o1);
            //
            return o1.ToString();
        }
    }
}
