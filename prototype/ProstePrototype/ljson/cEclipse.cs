using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ljson
{
    public class cEclipse : cXml
    {
        #region common
        private static string TranslateKey(string name)
        {
            if (Regex.IsMatch(name, @"ECLIPSE[\w\W]*?_AREAS$", RegexOptions.IgnoreCase))
                name = "eclipse_areas";
            else if (Regex.IsMatch(name, @"ECLIPSE[\w\W]*?_GENERALSETTINGS$", RegexOptions.IgnoreCase))
                name = "eclipse_gen_settings";
            else if (Regex.IsMatch(name, @"ECLIPSE[\w\W]*?_OUTPUTS$", RegexOptions.IgnoreCase))
                name = "eclipse_outputs";
            else if (Regex.IsMatch(name, @"ECLIPSE[\w\W]*?_USERS$", RegexOptions.IgnoreCase))
                name = "eclipse_users";
            else if (Regex.IsMatch(name, @"ECLIPSE[\w\W]*?_ZONES$", RegexOptions.IgnoreCase))
                name = "eclipse_zones";
            else if (Regex.IsMatch(name, @"ECLIPSE[\w\W]*?_TIMESLOTS$", RegexOptions.IgnoreCase))
                name = "eclipse_timeslots";
            else if (Regex.IsMatch(name, @"ECLIPSE[\w\W]*?_COMMUNICATOR$", RegexOptions.IgnoreCase))
                name = "eclipse_communicator";
            else if (Regex.IsMatch(name, @"ECLIPSE[\w\W]*?_DEVICES$", RegexOptions.IgnoreCase))
                name = "eclipse_devices";
            else if (Regex.IsMatch(name, @"IRIS[\w\W]*?_LOOPDEVICES$", RegexOptions.IgnoreCase))
                name = "iris_loop_devices";
            else if (Regex.IsMatch(name, @"ECLIPSE[\w\W]*?_ECLIPSE$", RegexOptions.IgnoreCase))
                name = "eclipse";
            return name;
        }

        private static void TranslateObjectsKeys(JObject content)
        {
            List<JToken> from = new List<JToken>();
            List<JToken> to = new List<JToken>();
            foreach (JToken t in (JToken)content)
            {
                string name = TranslateKey(((JProperty)t).Name);
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
            content["eclipse_areas"]["title"] = _pages["eclipse_areas"]["title"];//"Access codes";
            content["eclipse_areas"]["left"] = _pages["eclipse_areas"]["left"]; //"IRIS/divIRIS.html";
            content["eclipse_areas"]["right"] = _pages["eclipse_areas"]["right"];// "IRIS/access.html";
            content["eclipse_areas"]["breadcrumbs"] = _pages["eclipse_areas"]["breadcrumbs"];// JArray.Parse("[\"index\", \"iris\"]");
            //
            content["eclipse_gen_settings"]["title"] = _pages["eclipse_gen_settings"]["title"];// "Network";
            content["eclipse_gen_settings"]["left"] = _pages["eclipse_gen_settings"]["left"];// "IRIS/divIRIS.html";
            content["eclipse_gen_settings"]["right"] = _pages["eclipse_gen_settings"]["right"];// "IRIS/network.html";
            content["eclipse_gen_settings"]["breadcrumbs"] = _pages["eclipse_gen_settings"]["breadcrumbs"];// JArray.Parse("[\"index\", \"iris\"]");
            //
            content["eclipse_outputs"]["title"] = _pages["eclipse_outputs"]["title"];// "Panels in network";
            content["eclipse_outputs"]["left"] = _pages["eclipse_outputs"]["left"];//"IRIS/divIRIS.html";
            content["eclipse_outputs"]["right"] = _pages["eclipse_outputs"]["right"];//"IRIS/panels_in_network.html";
            content["eclipse_outputs"]["breadcrumbs"] = _pages["eclipse_outputs"]["breadcrumbs"];//JArray.Parse("[\"index\", \"iris\"]");
            //
            content["eclipse_timeslots"]["title"] = _pages["eclipse_timeslots"]["title"];// "Inputs";
            content["eclipse_timeslots"]["left"] = _pages["eclipse_timeslots"]["left"];//"IRIS/divIRIS.html";
            content["eclipse_timeslots"]["right"] = _pages["eclipse_timeslots"]["right"];//"IRIS/input.html";
            content["eclipse_timeslots"]["breadcrumbs"] = _pages["eclipse_timeslots"]["breadcrumbs"];//JArray.Parse("[\"index\", \"iris\"]");
            //
            content["eclipse_users"]["title"] = _pages["eclipse_users"]["title"];// "Input Groups";
            content["eclipse_users"]["left"] = _pages["eclipse_users"]["left"];//"IRIS/divIRIS.html";
            content["eclipse_users"]["right"] = _pages["eclipse_users"]["right"];//"IRIS/inputs_group.html";
            content["eclipse_users"]["breadcrumbs"] = _pages["eclipse_users"]["breadcrumbs"];//JArray.Parse("[\"index\", \"iris\"]");
            //
            content["eclipse_zones"]["title"] = _pages["eclipse_zones"]["title"];// "Output";
            content["eclipse_zones"]["left"] = _pages["eclipse_zones"]["left"];//"IRIS/divIRIS.html";
            content["eclipse_zones"]["right"] = _pages["eclipse_zones"]["right"];//"IRIS/output.html";
            content["eclipse_zones"]["breadcrumbs"] = _pages["eclipse_zones"]["breadcrumbs"];//JArray.Parse("[\"index\", \"iris\"]");
            //
            content["eclipse_communicator"]["title"] = _pages["eclipse_communicator"]["title"];// "FAT FBF";
            content["eclipse_communicator"]["left"] = _pages["eclipse_communicator"]["left"];//"IRIS/divIRIS.html";
            content["eclipse_communicator"]["right"] = _pages["eclipse_communicator"]["right"];//"IRIS/fat-fbf.html";
            content["eclipse_communicator"]["breadcrumbs"] = _pages["eclipse_communicator"]["breadcrumbs"];//JArray.Parse("[\"index\", \"iris\"]");
            //
            content["eclipse_devices"]["title"] = _pages["eclipse_devices"]["title"];// "Zones";
            content["eclipse_devices"]["left"] = _pages["eclipse_devices"]["left"];//"IRIS/divIRIS.html";
            content["eclipse_devices"]["right"] = _pages["eclipse_devices"]["right"];//"IRIS/zone.html";
            content["eclipse_devices"]["breadcrumbs"] = _pages["eclipse_devices"]["breadcrumbs"];//JArray.Parse("[\"index\", \"iris\"]");
        }

        private static void CreateMainGroups(JObject json)
        {
            //JArray proparr = (JArray)json["iris"]["PROPERTIES"]["PROPERTY"];
            //JObject o = Array2Object(proparr);
            ////groups
            //json["iris"]["PROPERTIES"]["Groups"] = new JObject();
            ////
            //JObject grp1 = new JObject();
            //grp1["name"] = "Delay Mode";
            //JObject f1 = new JObject();
            //f1["SounderDelayMode"] = o["SounderDelayMode"];
            //f1["FireBrigadeDelayMode"] = o["FireBrigadeDelayMode"];
            //f1["FireProtectionDelayMode"] = o["FireProtectionDelayMode"];
            //f1["DayMode"] = o["DayMode"];
            //grp1["fields"] = f1;
            //json["iris"]["PROPERTIES"]["Groups"]["DelayMode"] = grp1;
            ////
            //JObject grp2 = new JObject();
            //grp2["name"] = "Parameters";
            //JObject f2 = new JObject();
            //f2["SoundersMode"] = o["SoundersMode"];
            //f2["CallPointMode"] = o["CallPointMode"];
            //f2["PRINTER"] = o["PRINTER"];
            //f2["T1DELAY"] = o["T1DELAY"];
            //f2["EVACUATION_TIMEOUT"] = o["EVACUATION_TIMEOUT"];
            //grp2["fields"] = f2;
            //json["iris"]["PROPERTIES"]["Groups"]["Parameters"] = grp2;
            ////
            //JObject grp3 = new JObject();
            //grp3["name"] = "Tone Settings";
            //JObject f3 = new JObject();
            //f3["TONEALARM"] = o["TONEALARM"];
            //f3["TONEEVACUATE"] = o["TONEEVACUATE"];
            //f3["TONECLASSCHANGE"] = o["TONECLASSCHANGE"];
            //grp3["fields"] = f3;
            //json["iris"]["PROPERTIES"]["Groups"]["ToneSettings"] = grp3;
            ////
            //JObject grp4 = new JObject();
            //grp4["name"] = "Auto Log Off";
            //JObject f4 = new JObject();
            //f4["AUTOLOGOFFENABLED"] = o["AUTOLOGOFFENABLED"];
            //f4["TIMEAUTOLOGOFFINSTALLER"] = o["TIMEAUTOLOGOFFINSTALLER"];
            //grp4["fields"] = f4;
            //json["iris"]["PROPERTIES"]["Groups"]["AutoLogOff"] = grp4;
            ////
            //JObject grp5 = new JObject();
            //grp5["name"] = "Settings";
            //JObject f5 = new JObject();
            //f5["ALARM_SETTINGS"] = o["ALARM_SETTINGS"];
            //f5["EVACUATE_SETTINGS"] = o["EVACUATE_SETTINGS"];
            //grp5["fields"] = f5;
            //json["iris"]["PROPERTIES"]["Groups"]["Settings"] = grp5;
            ////
            //JObject grp6 = new JObject();
            //grp6["name"] = "Alert/EVAC Voice Cycle";
            //JObject f6 = new JObject();
            //f6["EVACUATECYCLE_ON"] = o["EVACUATECYCLE_ON"];
            //f6["EVACUATECYCLE_OFF"] = o["EVACUATECYCLE_OFF"];
            //f6["EVACUATIONCYCLE INVERTED"] = o["EVACUATIONCYCLE INVERTED"];
            //f6["SOUNDERS_RESOUND"] = o["SOUNDERS_RESOUND"];
            //grp6["fields"] = f6;
            //json["iris"]["PROPERTIES"]["Groups"]["AlertEVACVoiceCycle"] = grp6;
            ////
            //JObject grp7 = new JObject();
            //grp7["name"] = "Company Info";
            //JObject f7 = new JObject();
            //f7["LOGO1"] = o["LOGO1"];
            //f7["LOGO2"] = o["LOGO2"];
            //f7["LOGO3"] = o["LOGO3"];
            //f7["LOGO4"] = o["LOGO4"];
            //grp7["fields"] = f7;
            //json["iris"]["PROPERTIES"]["Groups"]["CompanyInfo"] = grp7;
            ////
            //json["iris"]["PROPERTIES"]["OLD"] = o;
        }

        private static void ConvertMainArraysLeft(JObject json)
        {
            JObject o = cXml.Array2Object(json["ELEMENTS"]["eclipse"]["MEDIA"]["ITEMS"]);
            if (o != null)
                json["ELEMENTS"]["eclipse"]["MEDIA"] = o;
            o = cXml.Array2Object(json["ELEMENTS"]["eclipse"]["ADDITIONALPARAMS"]["PARAM"]);
            if (o != null)
                json["ELEMENTS"]["eclipse"]["ADDITIONALPARAMS"] = o;
        }
        #endregion

        #region general settings
        private static void CreateGenSettingsGroups(JObject json)
        {
            //JArray proparr = (JArray)json["eclipse_gen_settings"]["PROPERTIES"]["PROPERTY"];
            //JObject o = Array2Object(proparr);
            //
            //groups
            //json["iris_access_code"]["PROPERTIES"]["Groups"] = new JObject();
            ////
            //JObject grp1 = new JObject();
            //grp1["name"] = "Code 1";
            //JObject f1 = new JObject();
            //f1["Code1"] = o["Code1"];
            //f1["Level1"] = o["Level1"];
            //grp1["fields"] = f1;
            //json["iris_access_code"]["PROPERTIES"]["Groups"]["Code1"] = grp1;
            ////
            //JObject grp2 = new JObject();
            //grp2["name"] = "Code 2";
            //JObject f2 = new JObject();
            //f2["Code2"] = o["Code2"];
            //f2["Level2"] = o["Level2"];
            //grp2["fields"] = f2;
            //json["iris_access_code"]["PROPERTIES"]["Groups"]["Code2"] = grp2;
            ////
            //JObject grp3 = new JObject();
            //grp3["name"] = "Code 3";
            //JObject f3 = new JObject();
            //f3["Code3"] = o["Code3"];
            //f3["Level3"] = o["Level3"];
            //grp3["fields"] = f3;
            //json["iris_access_code"]["PROPERTIES"]["Groups"]["Code3"] = grp3;
            ////
            //JObject grp4 = new JObject();
            //grp4["name"] = "Code 4";
            //JObject f4 = new JObject();
            //f4["Code4"] = o["Code4"];
            //f4["Level4"] = o["Level4"];
            //grp4["fields"] = f4;
            //json["iris_access_code"]["PROPERTIES"]["Groups"]["Code4"] = grp4;
            ////
            //json["iris_access_code"]["PROPERTIES"]["OLD"] = o;
        }

        private static void ConvertGenSettingsCode(JObject json, JObject _pages)
        {
            CreateGenSettingsGroups((JObject)json["ELEMENTS"]);
            JObject ac = (JObject)json["ELEMENTS"]["eclipse_gen_settings"];
            ac["title"] = _pages["eclipse_gen_settings"]["title"];
            ac["left"] = _pages["eclipse_gen_settings"]["left"];
            ac["right"] = _pages["eclipse_gen_settings"]["right"];
            ac["breadcrumbs"] = _pages["eclipse_gen_settings"]["breadcrumbs"];
            //
            //JObject o = cXml.Array2Object(json["ELEMENTS"]["iris_access_code"]["RULES"]["RULE"]);
            //if (o != null)
            //    json["ELEMENTS"]["iris_access_code"]["RULES"] = o;
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
                        elements["eclipse"]["title"] = _pages["eclipse"]["title"];
                        elements["eclipse"]["left"] = _pages["eclipse"]["left"];
                        elements["eclipse"]["right"] = _pages["eclipse"]["right"];
                        elements["eclipse"]["breadcrumbs"] = _pages["eclipse"]["breadcrumbs"];
                        CreateMainGroups(elements);
                        //
                        if (p.Name != "ELEMENT")
                            o1[p.Name] = elements;
                        else
                            o1["ELEMENTS"] = elements;
                    }
                }

            }
            //
            ConvertMainArraysLeft(o1);
            ConvertGenSettingsCode(o1, _pages);
            //
            return o1.ToString();
        }
    }
}
