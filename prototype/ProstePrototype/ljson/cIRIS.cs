﻿using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ljson
{
    public class cIRIS : cXml
    {
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
        private static void ChangeContent(JObject content)
        {
            TranslateObjectsKeys(content);
            //
            content["iris_access_code"]["title"] = "Access codes";
            content["iris_access_code"]["left"] = "IRIS/divIRIS.html";
            content["iris_access_code"]["right"] = "IRIS/access.html";
            content["iris_access_code"]["breadcrumbs"] = JArray.Parse("[\"index\", \"iris\"]");
            //
            content["iris_network"]["title"] = "Network";
            content["iris_network"]["left"] = "IRIS/divIRIS.html";
            content["iris_network"]["right"] = "IRIS/network.html";
            content["iris_network"]["breadcrumbs"] = JArray.Parse("[\"index\", \"iris\"]");
            //
            content["iris_panels_in_network"]["title"] = "Panels in network";
            content["iris_panels_in_network"]["left"] = "IRIS/divIRIS.html";
            content["iris_panels_in_network"]["right"] = "IRIS/panels_in_network.html";
            content["iris_panels_in_network"]["breadcrumbs"] = JArray.Parse("[\"index\", \"iris\"]");
            //
            content["iris_inputs"]["title"] = "Inputs";
            content["iris_inputs"]["left"] = "IRIS/divIRIS.html";
            content["iris_inputs"]["right"] = "IRIS/input.html";
            content["iris_inputs"]["breadcrumbs"] = JArray.Parse("[\"index\", \"iris\"]");
            //
            content["iris_inputs_group"]["title"] = "Input Groups";
            content["iris_inputs_group"]["left"] = "IRIS/divIRIS.html";
            content["iris_inputs_group"]["right"] = "IRIS/inputs_group.html";
            content["iris_inputs_group"]["breadcrumbs"] = JArray.Parse("[\"index\", \"iris\"]");
            //
            content["iris_outputs"]["title"] = "Output";
            content["iris_outputs"]["left"] = "IRIS/divIRIS.html";
            content["iris_outputs"]["right"] = "IRIS/output.html";
            content["iris_outputs"]["breadcrumbs"] = JArray.Parse("[\"index\", \"iris\"]");
            //
            content["iris_fat_fbf"]["title"] = "FAT FBF";
            content["iris_fat_fbf"]["left"] = "IRIS/divIRIS.html";
            content["iris_fat_fbf"]["right"] = "IRIS/fat-fbf.html";
            content["iris_fat_fbf"]["breadcrumbs"] = JArray.Parse("[\"index\", \"iris\"]");
            //
            content["iris_zones"]["title"] = "Zones";
            content["iris_zones"]["left"] = "IRIS/divIRIS.html";
            content["iris_zones"]["right"] = "IRIS/zone.html";
            content["iris_zones"]["breadcrumbs"] = JArray.Parse("[\"index\", \"iris\"]");
            //
            content["iris_evac_zones"]["title"] = "Evac zones";
            content["iris_evac_zones"]["left"] = "IRIS/divIRIS.html";
            content["iris_evac_zones"]["right"] = "IRIS/zone_evac.html";
            content["iris_evac_zones"]["breadcrumbs"] = JArray.Parse("[\"index\", \"iris\"]");
            //
            content["iris_peripheral_devices"]["title"] = "Peripheral devices";
            content["iris_peripheral_devices"]["left"] = "IRIS/divIRIS.html";
            content["iris_peripheral_devices"]["right"] = "IRIS/periph_devices.html";
            content["iris_peripheral_devices"]["breadcrumbs"] = JArray.Parse("[\"index\", \"iris\"]");
            //
            content["iris_loop_devices"]["title"] = "Loop devices";
            content["iris_loop_devices"]["left"] = "IRIS/divIRIS.html";
            content["iris_loop_devices"]["right"] = "IRIS/loop_devices.html";
            content["iris_loop_devices"]["breadcrumbs"] = JArray.Parse("[\"index\", \"iris\"]");
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
            f1["FireBrigadeDelayMode"] = o["FireBrigadeDelayMode"];
            f1["FireProtectionDelayMode"] = o["FireProtectionDelayMode"];
            f1["DayMode"] = o["DayMode"];
            grp1["fields"] = f1;
            json["iris"]["PROPERTIES"]["Groups"]["DelayMode"] = grp1;
            //
            JObject grp2 = new JObject();
            grp2["name"] = "Parameters";
            JObject f2 = new JObject();
            f2["SoundersMode"] = o["SoundersMode"];
            f2["CallPointMode"] = o["CallPointMode"];
            f2["Protocol"] = o["Protocol"];
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
            f5["AUTOLOGOFFENABLED"] = o["AUTOLOGOFFENABLED"];
            f5["TIMEAUTOLOGOFFINSTALLER"] = o["TIMEAUTOLOGOFFINSTALLER"];
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
            f7["TONEALARM"] = o["TONEALARM"];
            f7["TONEEVACUATE"] = o["TONEEVACUATE"];
            f7["TONECLASSCHANGE"] = o["TONECLASSCHANGE"];
            grp7["fields"] = f7;
            json["iris"]["PROPERTIES"]["Groups"]["CompanyInfo"] = grp7;
            //
            json["iris"]["PROPERTIES"]["OLD"] = o;
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

        private static void ConvertAccessCode(JObject json)
        {
            CreateAccessCodeGroups((JObject)json["ELEMENTS"]);
            JObject ac = (JObject)json["ELEMENTS"]["iris_access_code"];
            ac["title"] = "Access codes";
            ac["left"] = "IRIS/divIRIS.html";
            ac["right"] = "IRIS/access.html";
            ac["breadcrumbs"] = JArray.Parse("[\"index\", \"iris\"]");
        }
        #endregion

        #region panels in network
        private static void ConvertPanelsInNetwork(JObject json)
        {
            JObject ac = (JObject)json["ELEMENTS"]["iris_panels_in_network"];
            ac["title"] = "Panels in network";
            ac["left"] = "RIS/divIRIS.html";
            ac["right"] = "IRIS/panels_in_network.html";
            ac["breadcrumbs"] = JArray.Parse("[\"index\", \"iris\"]");
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
            f2["PanelEvacNumber"] = o["PanelEvacNumber"];
            f2["emacETHADDR0"] = o["emacETHADDR0"];
            f2["emacETHADDR1"] = o["emacETHADDR1"];
            f2["emacETHADDR2"] = o["emacETHADDR2"];
            f2["emacETHADDR3"] = o["emacETHADDR3"];
            f2["emacETHADDR4"] = o["emacETHADDR4"];
            f2["emacETHADDR5"] = o["emacETHADDR5"];
            grp2["fields"] = f2;
            json["iris_network"]["PROPERTIES"]["Groups"]["NetworkSettings"] = grp2;
            //
            json["iris_network"]["PROPERTIES"]["OLD"] = o;
        }
        private static void ConvertNetwork(JObject json)
        {
            CreateNetworkGroups((JObject)json["ELEMENTS"]);
            JObject ac = (JObject)json["ELEMENTS"]["iris_network"];
            ac["title"] = "Network";
            ac["left"] = "IRIS/divIRIS.html";
            ac["right"] = "IRIS/network.html";
            ac["breadcrumbs"] = JArray.Parse("[\"index\", \"iris\"]");
        }
        #endregion

        #region inputs
        private static void ConvertInputs(JObject json)
        {
            JObject ac = (JObject)json["ELEMENTS"]["iris_inputs"];
            ac["title"] = "Inputs";
            ac["left"] = "RIS/divIRIS.html";
            ac["right"] = "IRIS/input.html";
            ac["breadcrumbs"] = JArray.Parse("[\"index\", \"iris\"]");
            JObject contains = Contains2Object((JObject)json["ELEMENTS"]["iris_inputs"]["CONTAINS"]);
            json["ELEMENTS"]["iris_inputs"]["CONTAINS"] = contains;
        }
        #endregion

        #region FAT FBF
        private static void ConvertFATFBF(JObject json)
        {
            JObject ac = (JObject)json["ELEMENTS"]["iris_fat_fbf"];
            ac["title"] = "FAT FBF";
            ac["left"] = "RIS/divIRIS.html";
            ac["right"] = "IRIS/fat-fbf.html";
            ac["breadcrumbs"] = JArray.Parse("[\"index\", \"iris\"]");
            JObject contains = Contains2Object((JObject)json["ELEMENTS"]["iris_fat_fbf"]["CONTAINS"]);
            json["ELEMENTS"]["iris_fat_fbf"]["CONTAINS"] = contains;
        }
        #endregion

        #region input groups
        private static void ConvertInputGroups(JObject json)
        {
            JObject ac = (JObject)json["ELEMENTS"]["iris_inputs_group"];
            ac["title"] = "Input Groups";
            ac["left"] = "RIS/divIRIS.html";
            ac["right"] = "IRIS/inputs_group.html";
            ac["breadcrumbs"] = JArray.Parse("[\"index\", \"iris\"]");
            JObject contains = Contains2Object((JObject)json["ELEMENTS"]["iris_inputs_group"]["CONTAINS"]);
            json["ELEMENTS"]["iris_inputs_group"]["CONTAINS"] = contains;
        }
        #endregion

        #region outputs
        private static void ConvertOutputs(JObject json)
        {
            JObject ac = (JObject)json["ELEMENTS"]["iris_outputs"];
            ac["title"] = "Output";
            ac["left"] = "RIS/divIRIS.html";
            ac["right"] = "IRIS/output.html";
            ac["breadcrumbs"] = JArray.Parse("[\"index\", \"iris\"]");
            JObject contains = Contains2Object((JObject)json["ELEMENTS"]["iris_outputs"]["CONTAINS"]);
            json["ELEMENTS"]["iris_outputs"]["CONTAINS"] = contains;
        }
        #endregion

        #region zones
        private static void ConvertZones(JObject json)
        {
            JObject ac = (JObject)json["ELEMENTS"]["iris_zones"];
            ac["title"] = "Zones";
            ac["left"] = "RIS/divIRIS.html";
            ac["right"] = "IRIS/zone.html";
            ac["breadcrumbs"] = JArray.Parse("[\"index\", \"iris\"]");
            JObject contains = Contains2Object((JObject)json["ELEMENTS"]["iris_zones"]["CONTAINS"]);
            json["ELEMENTS"]["iris_zones"]["CONTAINS"] = contains;
        }
        #endregion

        #region evac zones
        private static void ConvertEvacZones(JObject json)
        {
            JObject ac = (JObject)json["ELEMENTS"]["iris_evac_zones"];
            ac["title"] = "Evac zones";
            ac["left"] = "RIS/divIRIS.html";
            ac["right"] = "IRIS/zone_evac.html";
            ac["breadcrumbs"] = JArray.Parse("[\"index\", \"iris\"]");
            JObject contains = Contains2Object((JObject)json["ELEMENTS"]["iris_evac_zones"]["CONTAINS"]);
            json["ELEMENTS"]["iris_evac_zones"]["CONTAINS"] = contains;
        }
        #endregion

        #region peripheral devices
        private static void ConvertPeripheralDevices(JObject json)
        {
            JObject ac = (JObject)json["ELEMENTS"]["iris_peripheral_devices"];
            ac["title"] = "Peripheral devices";
            ac["left"] = "RIS/divIRIS.html";
            ac["right"] = "IRIS/periph_devices.html";
            ac["breadcrumbs"] = JArray.Parse("[\"index\", \"iris\"]");
            JObject contains = Contains2Object((JObject)json["ELEMENTS"]["iris_peripheral_devices"]["CONTAINS"]);
            json["ELEMENTS"]["iris_peripheral_devices"]["CONTAINS"] = contains;
        }
        #endregion

        #region loop devices
        private static void ConvertLoopDevices(JObject json)
        {
            JObject ac = (JObject)json["ELEMENTS"]["iris_loop_devices"];
            ac["title"] = "Loop devices";
            ac["left"] = "RIS/divIRIS.html";
            ac["right"] = "IRIS/loop_devices.html";
            ac["breadcrumbs"] = JArray.Parse("[\"index\", \"iris\"]");
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
                json["ELEMENTS"][key]["PROPERTIES"] = prop;
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
            JObject props = Array2Object((JArray)o["PARAMETERS"]["PROPERTIES"]["PROPERTY"]);
            //f2["parameters"] = props;
            grp3["fields"] = props;
            json[key]["PROPERTIES"]["Groups"]["Parameters"] = grp3;
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
            JObject props = Array2Object((JArray)o["EVACPANEL0"]["PROPERTIES"]["PROPERTY"]);
            grp1["fields"] = props;
            json[key]["PROPERTIES"]["Groups"]["EvacPanel0"] = grp1;
            //
            JObject grp2 = new JObject();
            grp2["name"] = "Evacuation Panel 1";
            props = Array2Object((JArray)o["EVACPANEL1"]["PROPERTIES"]["PROPERTY"]);
            grp2["fields"] = props;
            json[key]["PROPERTIES"]["Groups"]["EvacPanel1"] = grp2;
            //
            JObject grp3 = new JObject();
            grp3["name"] = "Evacuation Panel 2";
            props = Array2Object((JArray)o["EVACPANEL2"]["PROPERTIES"]["PROPERTY"]);
            grp3["fields"] = props;
            json[key]["PROPERTIES"]["Groups"]["EvacPanel2"] = grp3;
            //
            JObject grp4 = new JObject();
            grp4["name"] = "Evacuation Panel 3";
            props = Array2Object((JArray)o["EVACPANEL3"]["PROPERTIES"]["PROPERTY"]);
            grp4["fields"] = props;
            json[key]["PROPERTIES"]["Groups"]["EvacPanel3"] = grp4;
            //
            JObject grp5 = new JObject();
            grp5["name"] = "Evacuation Panel 4";
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
                json["ELEMENTS"][key]["PROPERTIES"] = ConvertPreripherialDevicesContentNodeProps(dev);
            }
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
            json[key]["PROPERTIES"]["Groups"]["~noname"]["Panel_name"] = o["Panel_name"];
            //
            JObject grp1 = new JObject();
            grp1["name"] = "Parameters";
            JObject f1 = new JObject();
            f1["ReceiveMessages"] = o["panel"];
            f1["ReceiveCommands"] = o["repeater"];
            f1["SendCommands"] = o["SendCommand"];
            grp1["fields"] = f1;
            json[key]["PROPERTIES"]["Groups"]["Parameters"] = grp1;
            //
            JObject grp2 = new JObject();
            grp2["name"] = "Panel Outputs";
            JObject props = Array2Object((JArray)o["panel_flags"]["PROPERTIES"]["PROPERTY"]);
            grp2["fields"] = props;
            json[key]["PROPERTIES"]["Groups"]["Panel Outputs"] = grp2;
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

        public static string Convert(string json)
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
                            ChangeContent(content);
                            ((JObject)fo)["CONTAINS"] = content;
                        }
                    }
                    //foreach (JToken et in )
                    //{

                    //}
                    if (elements.Count > 0)
                    {
                        cIRIS.TranslateObjectsKeys(elements);
                        elements["iris"]["title"] = "Iris";
                        elements["iris"]["left"] = "IRIS/divIRIS.html";
                        elements["iris"]["right"] = "IRIS/IRISPANEL.html";
                        elements["iris"]["breadcrumbs"] = JArray.Parse("[\"index\"]");
                        cIRIS.CreateMainGroups(elements);
                        //
                        if (p.Name != "ELEMENT")
                            o1[p.Name] = elements;
                        else
                            o1["ELEMENTS"] = elements;
                    }
                }

            }
            ConvertAccessCode(o1);
            ConvertPanelsInNetwork(o1);
            ConvertNetwork(o1);
            ConvertInputs(o1);
            ConvertFATFBF(o1);
            ConvertInputGroups(o1);
            ConvertOutputs(o1);
            ConvertZones(o1);
            ConvertEvacZones(o1);
            ConvertPeripheralDevices(o1);
            ConvertLoopDevices(o1);
            ConvertInputsGroup(o1);
            ConvertInput(o1);
            ConvertOutput(o1);
            ConvertZone(o1);
            ConvertEvacZoneGroup(o1);
            ConvertPreripherialDevicesContentNodes(o1);
            ConvertPanelInNetwork(o1);
            return o1.ToString();
        }
    }
}
