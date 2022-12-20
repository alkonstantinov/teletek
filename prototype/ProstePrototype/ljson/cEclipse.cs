using Microsoft.VisualBasic;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Input;

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

        private static JObject UniqueGroups(JObject node)
        {
            JObject g = (JObject)node["UNIQUEGROUPS"]["UNIQUEGROUP"];
            JObject g1 = new JObject();
            foreach (JProperty p in (JToken)g)
                if (p.Name != "IGNOREVALUES")
                    g1[p.Name] = p.Value;
            JObject ugi = new JObject();
            foreach (JObject o in (JArray)g1["UNIQUEGROUPITEM"])
            {
                string prop = o["@PROPERTYNAME"].ToString();
                string el = o["@ELEMENTNAME"].ToString();
                cXml.RemoveProp(o, "@PROPERTYNAME");
                ugi[el + "/" + prop] = o;
            }
            g1["Properties"] = ugi;
            cXml.RemoveProp(g1, "UNIQUEGROUPITEM");
            return g1;
        }

        private static JObject CombineUniqueGroups(JObject ug, JObject json)
        {
            JObject res = new JObject();
            foreach (JToken t in (JToken)ug)
            {
                if (t.Type == JTokenType.Property && ((JProperty)t).Value.Type != JTokenType.Object)
                    res[((JProperty)t).Name] = ((JProperty)t).Value;
            }
            Dictionary<string, JObject> keys = new Dictionary<string, JObject>();
            JObject props = (JObject)ug["Properties"];
            res["Properties"] = new JObject();
            foreach (JToken t in (JToken)props)
            {
                JProperty p = (JProperty)t;
                string pname = Regex.Split(p.Name, "/")[1];
                string key = ((JObject)p.Value)["@ELEMENTNAME"].ToString();
                JObject node = null;
                if (keys.ContainsKey(key))
                    node = keys[key];
                else
                {
                    node = new JObject((JObject)json[key]["PROPERTIES"]);
                    node = cXml.Array2Object(node);
                    keys.Add(key, node);
                }
                if (node == null)
                    continue;
                JObject attr = (JObject)node[pname];
                attr["@ELEMENTNAME"] = key;
                res["Properties"][key + "/" + pname] = attr;
            }
            return res;
        }

        private static JObject GroupsFromContent(JObject json, string key)
        {
            JObject groups = new JObject();
            JObject content = new JObject((JObject)json[key]["CONTAINS"]);
            if (content["ELEMENT"] != null)
                content = Array2Object(content["ELEMENT"]);
            foreach (JProperty c in (JToken)content)
            {
                string ckey = c.Name;
                groups[ckey] = new JObject();
                JObject cnode = new JObject((JObject)json[ckey]);
                groups[ckey]["name"] = cnode["@PRODUCTNAME"].ToString();
                groups[ckey]["fields"] = Array2Object(cnode["PROPERTIES"]["PROPERTY"]);
            }
            return groups;
        }

        private static JObject GetPropertiesByContent(JObject elements, string key)
        {
            JObject o = (JObject)elements[key];
            o = Array2Object(o["CONTAINS"]["ELEMENT"]);
            o["CONTAINS"] = o;
            JProperty p = (JProperty)o.First;
            o = new JObject((JObject)elements[p.Name]);
            if (o["PROPERTIES"] != null)
                return Array2Object(o["PROPERTIES"]["PROPERTY"]);
            //
            return null;
        }
        #endregion

        #region main
        private static void ChangeContent(JObject content, JObject _pages)
        {
            TranslateObjectsKeys(content);
            //
            content["eclipse_areas"]["title"] = _pages["eclipse_areas"]["title"];
            content["eclipse_areas"]["left"] = _pages["eclipse_areas"]["left"];
            content["eclipse_areas"]["right"] = _pages["eclipse_areas"]["right"];
            content["eclipse_areas"]["breadcrumbs"] = _pages["eclipse_areas"]["breadcrumbs"];
            //
            content["eclipse_gen_settings"]["title"] = _pages["eclipse_gen_settings"]["title"];
            content["eclipse_gen_settings"]["left"] = _pages["eclipse_gen_settings"]["left"];
            content["eclipse_gen_settings"]["right"] = _pages["eclipse_gen_settings"]["right"];
            content["eclipse_gen_settings"]["breadcrumbs"] = _pages["eclipse_gen_settings"]["breadcrumbs"];
            //
            content["eclipse_outputs"]["title"] = _pages["eclipse_outputs"]["title"];
            content["eclipse_outputs"]["left"] = _pages["eclipse_outputs"]["left"];
            content["eclipse_outputs"]["right"] = _pages["eclipse_outputs"]["right"];
            content["eclipse_outputs"]["breadcrumbs"] = _pages["eclipse_outputs"]["breadcrumbs"];
            //
            content["eclipse_timeslots"]["title"] = _pages["eclipse_timeslots"]["title"];
            content["eclipse_timeslots"]["left"] = _pages["eclipse_timeslots"]["left"];
            content["eclipse_timeslots"]["right"] = _pages["eclipse_timeslots"]["right"];
            content["eclipse_timeslots"]["breadcrumbs"] = _pages["eclipse_timeslots"]["breadcrumbs"];
            //
            content["eclipse_users"]["title"] = _pages["eclipse_users"]["title"];
            content["eclipse_users"]["left"] = _pages["eclipse_users"]["left"];
            content["eclipse_users"]["right"] = _pages["eclipse_users"]["right"];
            content["eclipse_users"]["breadcrumbs"] = _pages["eclipse_users"]["breadcrumbs"];
            //
            content["eclipse_zones"]["title"] = _pages["eclipse_zones"]["title"];
            content["eclipse_zones"]["left"] = _pages["eclipse_zones"]["left"];
            content["eclipse_zones"]["right"] = _pages["eclipse_zones"]["right"];
            content["eclipse_zones"]["breadcrumbs"] = _pages["eclipse_zones"]["breadcrumbs"];
            //
            content["eclipse_communicator"]["title"] = _pages["eclipse_communicator"]["title"];
            content["eclipse_communicator"]["left"] = _pages["eclipse_communicator"]["left"];
            content["eclipse_communicator"]["right"] = _pages["eclipse_communicator"]["right"];
            content["eclipse_communicator"]["breadcrumbs"] = _pages["eclipse_communicator"]["breadcrumbs"];
            //
            content["eclipse_devices"]["title"] = _pages["eclipse_devices"]["title"];
            content["eclipse_devices"]["left"] = _pages["eclipse_devices"]["left"];
            content["eclipse_devices"]["right"] = _pages["eclipse_devices"]["right"];
            content["eclipse_devices"]["breadcrumbs"] = _pages["eclipse_devices"]["breadcrumbs"];
        }

        private static void CreateMainGroups(JObject json)
        {
            JObject groups = GroupsFromContent(json, "eclipse_gen_settings");
            if (json["eclipse"]["PROPERTIES"] == null)
                json["eclipse"]["PROPERTIES"] = new JObject();
            json["eclipse"]["PROPERTIES"]["Groups"] = groups;
            //JObject ug = UniqueGroups((JObject)json["eclipse"]);
            //JObject combined = CombineUniqueGroups(ug, json);
            //json["eclipse"]["PROPERTIES"] = new JObject();
            //json["eclipse"]["PROPERTIES"]["CombinedFields"] = combined["Properties"];
            //json["eclipse"]["PROPERTIES"]["Groups"] = new JObject();
            //json["eclipse"]["PROPERTIES"]["Groups"]["Codes"] = new JObject();
            //json["eclipse"]["PROPERTIES"]["Groups"]["Codes"]["Name"] = "Codes";
            //json["eclipse"]["PROPERTIES"]["Groups"]["Codes"]["Fields"] = json["eclipse"]["PROPERTIES"]["CombinedFields"];
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
            JObject groups = GroupsFromContent(json, "eclipse_gen_settings");
            if (json["eclipse_gen_settings"]["PROPERTIES"] == null)
                json["eclipse_gen_settings"]["PROPERTIES"] = new JObject();
            json["eclipse_gen_settings"]["PROPERTIES"]["Groups"] = groups;
        }

        private static void ConvertGenSettings(JObject json, JObject _pages)
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

        #region users
        private static void ConvertUsers(JObject json, JObject _pages)
        {
            JObject ac = (JObject)json["ELEMENTS"]["eclipse_users"];
            ac["title"] = _pages["eclipse_users"]["title"];
            ac["left"] = _pages["eclipse_users"]["left"];
            ac["right"] = _pages["eclipse_users"]["right"];
            ac["breadcrumbs"] = _pages["eclipse_users"]["breadcrumbs"];
            JObject contains = Contains2Object((JObject)json["ELEMENTS"]["eclipse_users"]["CONTAINS"]);
            json["ELEMENTS"]["eclipse_users"]["CONTAINS"] = contains;
            //
            //JObject o = cXml.Array2Object(json["ELEMENTS"]["iris_access_code"]["RULES"]["RULE"]);
            //if (o != null)
            //    json["ELEMENTS"]["iris_access_code"]["RULES"] = o;
        }
        #endregion

        #region user
        private static void CreateUserGroups(JObject json, string key)
        {
            JObject groups = GroupsFromContent(json, "eclipse_users");
            JObject fields = (JObject)groups[key]["fields"];
            groups["~noname"] = new JObject();
            groups["~noname"]["name"] = "";
            groups["~noname"]["fields"] = new JObject();
            groups["~noname"]["fields"]["NAME"] = fields["NAME"];
            groups["Codes"] = new JObject();
            groups["Codes"]["name"] = "Codes";
            groups["Codes"]["fields"] = new JObject();
            groups["Codes"]["fields"]["CODE"] = fields["CODE"];
            groups["Codes"]["fields"]["PROXYCODE"] = fields["PROXYCODE"];
            groups["Attributes"] = new JObject();
            groups["Attributes"]["name"] = "Attributes";
            groups["Attributes"]["fields"] = new JObject();
            groups["Attributes"]["fields"]["ATTRIBUTES"] = fields["ATTRIBUTES"];
            groups["ProxyAttribs"] = new JObject();
            groups["ProxyAttribs"]["name"] = "Proxy Attributes";
            groups["ProxyAttribs"]["fields"] = new JObject();
            groups["ProxyAttribs"]["fields"]["PROXYATTR"] = fields["PROXYATTR"];
            groups["RC"] = new JObject();
            groups["RC"]["name"] = "RC";
            groups["RC"]["fields"] = new JObject();
            groups["RC"]["fields"]["RCFUNCTIONBUTTON"] = fields["RCFUNCTIONBUTTON"];
            groups["RC"]["fields"]["RCFUNCTIONBUTTON2"] = fields["RCFUNCTIONBUTTON2"];
            groups["Areas"] = new JObject();
            groups["Areas"]["name"] = "Areas";
            groups["Areas"]["fields"] = new JObject();
            groups["Areas"]["fields"]["AREAS1"] = fields["AREAS1"];
            groups["Areas"]["fields"]["AREAS2"] = fields["AREAS2"];
            groups["~noname2"] = new JObject();
            groups["~noname2"]["name"] = "";
            groups["~noname2"]["fields"] = new JObject();
            groups["~noname2"]["fields"]["TIMESLOT"] = fields["TIMESLOT"];
            if (json[key]["PROPERTIES"] == null)
                json[key]["PROPERTIES"] = new JObject();
            json[key]["PROPERTIES"][key] = new JObject((JObject)groups[key]);
            groups.Remove(key);
            json[key]["PROPERTIES"]["Groups"] = groups;
        }

        private static void ConvertUser(JObject json, JObject _pages)
        {
            foreach (JProperty p in json["ELEMENTS"]["eclipse_users"]["CONTAINS"])
            {
                string key = p.Name.ToString();
                CreateUserGroups((JObject)json["ELEMENTS"], key);
            }
        }
        #endregion

        #region zones        
        private static void CreateZonesGroups(JObject json)
        {
            JObject o = new JObject((JObject)json["eclipse_zones"]["PROPERTIES"]);
            o = Array2Object(o["PROPERTY"]);
            JObject groups = new JObject();
            groups["Settings"] = new JObject();
            groups["Settings"]["name"] = "Settings";
            groups["Settings"]["fields"] = new JObject();
            JObject f = (JObject)groups["Settings"]["fields"];
            f["CONNTYPE"] = o["CONNTYPE"];
            f["INSTANTZONES"] = o["INSTANTZONES"];
            f["BPSCOUNTER"] = o["BPSCOUNTER"];
            f["PULSECOUNT"] = o["PULSECOUNT"];
            f["PULSECOUNTTIMER"] = o["PULSECOUNTTIMER"];
            json["eclipse_zones"]["PROPERTIES"]["Groups"] = groups;
        }

        private static void ConvertZones(JObject json, JObject _pages)
        {
            CreateZonesGroups((JObject)json["ELEMENTS"]);
            JObject ac = (JObject)json["ELEMENTS"]["eclipse_zones"];
            ac["title"] = _pages["eclipse_zones"]["title"];
            ac["left"] = _pages["eclipse_zones"]["left"];
            ac["right"] = _pages["eclipse_zones"]["right"];
            ac["breadcrumbs"] = _pages["eclipse_zones"]["breadcrumbs"];
            JObject contains = Contains2Object((JObject)json["ELEMENTS"]["eclipse_zones"]["CONTAINS"]);
            json["ELEMENTS"]["eclipse_zones"]["CONTAINS"] = contains;
        }
        #endregion

        #region zone
        private static void CreateZoneGroups(JObject json, string key)
        {
            JObject o = new JObject((JObject)json[key]["PROPERTIES"]);
            o = Array2Object(o["PROPERTY"]);
            JObject groups = new JObject();
            groups["~noname"] = new JObject();
            groups["~noname"]["name"] = "";
            groups["~noname"]["fields"] = new JObject();
            JObject f = (JObject)groups["~noname"]["fields"];
            f["NAME"] = o["NAME"];
            f["TYPE"] = o["TYPE"];
            //
            groups["Attachment"] = new JObject();
            groups["Attachment"]["name"] = "Attachment";
            groups["Attachment"]["fields"] = new JObject();
            f = (JObject)groups["Attachment"]["fields"];
            f["ATTDEVICENUMBER"] = o["ATTDEVICENUMBER"];
            f["ATTTERMINALNUMBER"] = o["ATTTERMINALNMBER"];
            //
            groups["Attributes"] = new JObject();
            groups["Attributes"]["name"] = "Attributes";
            groups["Attributes"]["fields"] = new JObject();
            f = (JObject)groups["Attributes"]["fields"];
            f["ATTRIBUTES1"] = o["ATTRIBUTES1"];
            f["ATTRIBUTES2"] = o["ATTRIBUTES2"];
            //
            groups["AttributesAux"] = new JObject();
            groups["AttributesAux"]["name"] = "Attributes Aux";
            groups["AttributesAux"]["fields"] = new JObject();
            f = (JObject)groups["AttributesAux"]["fields"];
            f["ATTRAUX"] = o["ATTRAUX"];
            f["ATTRAUX"]["@TEXT"] = "";
            //
            groups["AttributesKeySwitch"] = new JObject();
            groups["AttributesKeySwitch"]["name"] = "Attributes Key Switch";
            groups["AttributesKeySwitch"]["fields"] = new JObject();
            f = (JObject)groups["AttributesKeySwitch"]["fields"];
            f["ATTRIBUTESKEYSWITCH"] = o["ATTRIBUTESKEYSWITCH"];
            //
            groups["Areas"] = new JObject();
            groups["Areas"]["name"] = "Areas";
            groups["Areas"]["fields"] = new JObject();
            f = (JObject)groups["Areas"]["fields"];
            f["AREAS1"] = o["AREAS1"];
            f["AREAS2"] = o["AREAS2"];
            //
            json[key]["PROPERTIES"]["Groups"] = groups;
        }

        private static void ConvertZone(JObject json)
        {
            JObject zc = (JObject)json["ELEMENTS"]["eclipse_zones"]["CONTAINS"];
            JProperty f = (JProperty)zc.First;
            string zonekey = f.Name;
            CreateZoneGroups((JObject)json["ELEMENTS"], zonekey);
        }
        #endregion

        #region outputs
        private static void ConvertOutputs(JObject json, JObject _pages)
        {
            JObject ac = (JObject)json["ELEMENTS"]["eclipse_outputs"];
            ac["title"] = _pages["eclipse_outputs"]["title"];
            ac["left"] = _pages["eclipse_outputs"]["left"];
            ac["right"] = _pages["eclipse_outputs"]["right"];
            ac["breadcrumbs"] = _pages["eclipse_outputs"]["breadcrumbs"];
            JObject contains = Contains2Object((JObject)json["ELEMENTS"]["eclipse_outputs"]["CONTAINS"]);
            json["ELEMENTS"]["eclipse_outputs"]["CONTAINS"] = contains;
        }
        #endregion

        #region output
        private static void CreateOutputGroups(JObject json, string key)
        {
            JObject o = new JObject((JObject)json[key]["PROPERTIES"]);
            o = Array2Object(o["PROPERTY"]);
            JObject groups = new JObject();
            groups["Attachment"] = new JObject();
            groups["Attachment"]["name"] = "Attachment";
            groups["Attachment"]["fields"] = new JObject();
            JObject f = (JObject)groups["Attachment"]["fields"];
            f["ATTDEVICENUMBER"] = o["ATTDEVICENUMBER"];
            f["ATTTERMINALNUMBER"] = o["ATTTERMINALNUMBER"];
            //
            groups["ActivationEvent"] = new JObject();
            groups["ActivationEvent"]["name"] = "Activation Event";
            groups["ActivationEvent"]["fields"] = new JObject();
            f = (JObject)groups["ActivationEvent"]["fields"];
            f["ATTDEVICENUMBER"] = o["ATTDEVICENUMBER"];
            f["ATTTERMINALNUMBER"] = o["ATTTERMINALNMBER"];
            //
            groups["Options"] = new JObject();
            groups["Options"]["name"] = "Options";
            groups["Options"]["fields"] = new JObject();
            f = (JObject)groups["Options"]["fields"];
            f["ATTRIBUTES1"] = o["ATTRIBUTES1"];
            //
            groups["Areas"] = new JObject();
            groups["Areas"]["name"] = "Areas";
            groups["Areas"]["fields"] = new JObject();
            f = (JObject)groups["Areas"]["fields"];
            f["AREAS1"] = o["AREAS1"];
            f["AREAS2"] = o["AREAS2"];
            //
            groups["Tabs&etc"] = new JObject();
            groups["Tabs&etc"]["name"] = "PROPERTY ACTEVENT/type TAB, DEACTIVATION_TIMER/type INT, DELAY/type INT: какво ги правим? Определено са стойности към \"Options\"";
            groups["Tabs&etc"]["fields"] = new JObject();
            f = (JObject)groups["Tabs&etc"]["fields"];
            f["ACTEVENT"] = o["ACTEVENT"];
            f["DEACTIVATION_TIMER"] = o["DEACTIVATION_TIMER"];
            f["DELAY"] = o["DELAY"];
            json[key]["PROPERTIES"]["Groups"] = groups;
        }

        private static void ConvertOutput(JObject json)
        {
            JObject oc = (JObject)json["ELEMENTS"]["eclipse_outputs"]["CONTAINS"];
            JProperty f = (JProperty)oc.First;
            string outputkey = f.Name;
            CreateOutputGroups((JObject)json["ELEMENTS"], outputkey);
        }
        #endregion

        #region areas
        private static void CreateAreasGroups(JObject json)
        {
            JObject o = new JObject((JObject)json["eclipse_areas"]["PROPERTIES"]);
            o = Array2Object(o["PROPERTY"]);
            JObject groups = new JObject();
            groups["Settings"] = new JObject();
            groups["Settings"]["name"] = "Settings";
            groups["Settings"]["fields"] = new JObject();
            JObject f = (JObject)groups["Settings"]["fields"];
            f["ACCLEN"] = o["ACCLEN"];
            f["DBLKNOCKTIME"] = o["DBLKNOCKTIME"];
            f["AUTOARMNOMOV"] = o["AUTOARMNOMOV"];
            f["POSTPONEALARM"] = o["POSTPONEALARM"];
            json["eclipse_areas"]["PROPERTIES"]["Groups"] = groups;
        }

        private static void ConvertAreas(JObject json, JObject _pages)
        {
            CreateAreasGroups((JObject)json["ELEMENTS"]);
            JObject ac = (JObject)json["ELEMENTS"]["eclipse_areas"];
            ac["title"] = _pages["eclipse_areas"]["title"];
            ac["left"] = _pages["eclipse_areas"]["left"];
            ac["right"] = _pages["eclipse_areas"]["right"];
            ac["breadcrumbs"] = _pages["eclipse_areas"]["breadcrumbs"];
            JObject contains = Contains2Object((JObject)json["ELEMENTS"]["eclipse_areas"]["CONTAINS"]);
            json["ELEMENTS"]["eclipse_areas"]["CONTAINS"] = contains;
        }
        #endregion

        #region area
        private static void CreateAreaGroups(JObject json, string key)
        {
            JObject o = new JObject((JObject)json[key]["PROPERTIES"]);
            o = Array2Object(o["PROPERTY"]);
            JObject groups = new JObject();
            groups["~noname"] = new JObject();
            groups["~noname"]["name"] = "";
            groups["~noname"]["fields"] = new JObject();
            JObject f = (JObject)groups["~noname"]["fields"];
            f["NAME"] = o["NAME"];
            f["TIMESLOT"] = o["TIMESLOT"];
            f["ACCCODE"] = o["ACCCODE"];
            //
            groups["OnOffSettings"] = new JObject();
            groups["OnOffSettings"]["name"] = "On / Off Settings";
            groups["OnOffSettings"]["fields"] = new JObject();
            f = (JObject)groups["OnOffSettings"]["fields"];
            f["ENTRYTIME"] = o["ENTRYTIME"];
            f["ENTRYTIME2"] = o["ENTRYTIME2"];
            f["EXITTIME"] = o["EXITTIME"];
            f["ATTR2"] = o["ATTR2"];
            f["ATTR2"] = o["ATTR2"];
            //
            groups["BellSettings"] = new JObject();
            groups["BellSettings"]["name"] = "Bell Settings";
            groups["BellSettings"]["fields"] = new JObject();
            f = (JObject)groups["BellSettings"]["fields"];
            f["BELLTIME"] = o["BELLTIME"];
            f["BELLDELAY"] = o["BELLDELAY"];
            f["ATTR1"] = o["ATTR1"];
            //
            groups["PanicAlarmTypes"] = new JObject();
            groups["PanicAlarmTypes"]["name"] = "Panic Alarm Types";
            groups["PanicAlarmTypes"]["fields"] = new JObject();
            f = (JObject)groups["PanicAlarmTypes"]["fields"];
            f["PANICS"] = o["PANICS"];
            //
            json[key]["PROPERTIES"]["Groups"] = groups;
        }

        private static void ConvertArea(JObject json)
        {
            JObject oc = (JObject)json["ELEMENTS"]["eclipse_areas"]["CONTAINS"];
            JProperty f = (JProperty)oc.First;
            string areakey = f.Name;
            CreateAreaGroups((JObject)json["ELEMENTS"], areakey);
        }
        #endregion

        #region timeslots
        private static void CreateTimeslotsGroups(JObject json)
        {
            if (json["eclipse_timeslots"]["PROPERTIES"] == null)
                json["eclipse_timeslots"]["PROPERTIES"] = new JObject();
            JObject o = (JObject)json["eclipse_timeslots"]["CONTAINS"];
            string slotsettings_key = null;
            string holidays_key = null;
            foreach (JProperty p in (JToken)o)
            {
                string s = p.Name;
                if (Regex.IsMatch(s, @"timeslot", RegexOptions.IgnoreCase))
                    slotsettings_key = s;
                else if (Regex.IsMatch(s, @"holiday", RegexOptions.IgnoreCase))
                    holidays_key = s;
            }
            //JObject o = new JObject((JObject)json["eclipse_timeslots"]["PROPERTIES"]);
            //o = Array2Object(o["PROPERTY"]);
            JObject groups = new JObject();
            groups["TimeslotSettings"] = new JObject();
            groups["TimeslotSettings"]["name"] = "Timeslot Settings";
            groups["TimeslotSettings"]["fields"] = new JObject();
            JObject f = (JObject)groups["TimeslotSettings"]["fields"];
            f[slotsettings_key] = new JObject((JObject)o[slotsettings_key]);
            f[slotsettings_key]["@TYPE"] = "CONTENT_KEY";
            f["ADDBUTTON"] = new JObject();
            f["ADDBUTTON"]["@TYPE"] = "ADDCONTENTITEMBUTTON";
            f["ADDBUTTON"]["@TEXT"] = "+ Add New Timeslot";
            f["ADDBUTTON"]["@ALIGN"] = "BOTTOM";
            //
            groups["HolidaysSettings"] = new JObject();
            groups["HolidaysSettings"]["name"] = "Holidays Settings";
            groups["HolidaysSettings"]["fields"] = new JObject();
            f = (JObject)groups["HolidaysSettings"]["fields"];
            f[holidays_key] = new JObject((JObject)o[holidays_key]);
            f[holidays_key]["@TYPE"] = "CONTENT_KEY";
            f["ADDBUTTON"] = new JObject();
            f["ADDBUTTON"]["@TYPE"] = "ADDCONTENTITEMBUTTON";
            f["ADDBUTTON"]["@TEXT"] = "+ Add New Holidays";
            f["ADDBUTTON"]["@ALIGN"] = "BOTTOM";
            json["eclipse_timeslots"]["PROPERTIES"]["Groups"] = groups;
        }
        private static void ConvertTimeslots(JObject json, JObject _pages)
        {
            JObject ac = (JObject)json["ELEMENTS"]["eclipse_timeslots"];
            ac["title"] = _pages["eclipse_timeslots"]["title"];
            ac["left"] = _pages["eclipse_timeslots"]["left"];
            ac["right"] = _pages["eclipse_timeslots"]["right"];
            ac["breadcrumbs"] = _pages["eclipse_timeslots"]["breadcrumbs"];
            JObject contains = Contains2Object((JObject)json["ELEMENTS"]["eclipse_timeslots"]["CONTAINS"]);
            json["ELEMENTS"]["eclipse_timeslots"]["CONTAINS"] = contains;
            CreateTimeslotsGroups((JObject)json["ELEMENTS"]);
        }
        #endregion

        #region timeslot
        private static void CreateTimeslotGroups(JObject json, string key)
        {
            JObject c = Contains2Object((JObject)json[key]["CONTAINS"]);
            json[key]["CONTAINS"] = c;
            JProperty first = (JProperty)c.First;
            string skey = first.Name;
            //
            if (json[key]["PROPERTIES"] == null)
                json[key]["PROPERTIES"] = new JObject();
            JObject o = new JObject((JObject)json[skey]["PROPERTIES"]);
            if (o["PROPERTY"] != null)
                o = Array2Object(o["PROPERTY"]);
            JObject groups = new JObject();
            groups["~noname"] = new JObject();
            groups["~noname"]["name"] = "";
            groups["~noname"]["fields"] = new JObject();
            JObject f = (JObject)groups["~noname"]["fields"];
            f["BEGINTIME"] = o["BEGINTIME"];
            f["ENDTIME"] = o["ENDTIME"];
            f["WEEKDAYS"] = o["WEEKDAYS"];
            f["ENABLEHOLIDAYS"] = o["ENABLEHOLIDAYS"];
            //
            json[key]["PROPERTIES"]["Groups"] = groups;
        }

        private static void CreateHolidaysGroups(JObject json, string key)
        {
            //JObject c = Contains2Object((JObject)json[skey]["CONTAINS"]);
            //json[skey]["CONTAINS"] = c;
            //JProperty first = (JProperty)c.First;
            //string key = first.Name;
            //
            if (json[key]["PROPERTIES"] == null)
                json[key]["PROPERTIES"] = new JObject();
            JObject o = new JObject((JObject)json[key]["PROPERTIES"]);
            if (o["PROPERTY"] != null)
                o = Array2Object(o["PROPERTY"]);
            JObject groups = new JObject();
            groups["~noname"] = new JObject();
            groups["~noname"]["name"] = "";
            groups["~noname"]["fields"] = new JObject();
            JObject f = (JObject)groups["~noname"]["fields"];
            f["From"] = new JObject();
            f["From"]["@TYPE"] = "DATE";
            f["From"]["@TEXT"] = "from";
            f["To"] = new JObject();
            f["To"]["@TYPE"] = "DATE";
            f["To"]["@TEXT"] = "to";
            //
            json[key]["PROPERTIES"]["Groups"] = groups;
        }
        private static void ConvertTimeslot(JObject json)
        {
            JObject o = (JObject)json["ELEMENTS"]["eclipse_timeslots"]["CONTAINS"];
            string skey = null;
            string hkey = null;
            foreach (JProperty p in (JToken)o)
            {
                string s = p.Name;
                if (Regex.IsMatch(s, @"timeslot", RegexOptions.IgnoreCase))
                    skey = s;
                else if (Regex.IsMatch(s, @"holiday", RegexOptions.IgnoreCase))
                    hkey = s;
            }
            if (skey != null)
                CreateTimeslotGroups((JObject)json["ELEMENTS"], skey);
            if (hkey != null)
                CreateHolidaysGroups((JObject)json["ELEMENTS"], hkey);
        }
        #endregion

        #region communicator
        private static void CreateCommunicatorGroups(JObject json)
        {
            JObject o = new JObject((JObject)json["eclipse_communicator"]["PROPERTIES"]);
            o = Array2Object(o["PROPERTY"]);
            //
            JObject groups = new JObject();
            groups["DialerOptions"] = new JObject();
            groups["DialerOptions"]["name"] = "Dialer options";
            groups["DialerOptions"]["fields"] = new JObject();
            JObject f = (JObject)groups["DialerOptions"]["fields"];
            f["DIALEROPT"] = o["DIALEROPT"];
            //
            groups["CommunicatorSettings"] = new JObject();
            groups["CommunicatorSettings"]["name"] = "Communicator Settings";
            groups["CommunicatorSettings"]["fields"] = new JObject();
            f = (JObject)groups["CommunicatorSettings"]["fields"];
            f["COMMATT"] = o["COMMATT"];
            f["TMPERIOD"] = o["TMPERIOD"];
            f["TMSTARTTIME"] = o["TMSTARTTIME"];
            f["ALARMDELAY"] = o["ALARMDELAY"];
            //
            JObject content = (JObject)json["eclipse_communicator"]["CONTAINS"];
            string pn = null;
            string vd = null;
            string udl = null;
            foreach (JProperty cp in (JToken)content)
            {
                if (Regex.IsMatch(cp.Name, @"phonenumber", RegexOptions.IgnoreCase))
                    pn = cp.Name;
                if (Regex.IsMatch(cp.Name, @"voicedialer", RegexOptions.IgnoreCase))
                    vd = cp.Name;
                if (Regex.IsMatch(cp.Name, @"udlsettings", RegexOptions.IgnoreCase))
                    udl = cp.Name;
            }
            //
            groups["PhoneNumberSettings"] = new JObject();
            groups["PhoneNumberSettings"]["name"] = "Phone Number Settings";
            groups["PhoneNumberSettings"]["fields"] = new JObject();
            f = (JObject)groups["PhoneNumberSettings"]["fields"];
            f[pn] = new JObject((JObject)content[pn]);
            f[pn]["@TYPE"] = "CONTENT_KEY";
            f["ADDBUTTON"] = new JObject();
            f["ADDBUTTON"]["@TYPE"] = "ADDCONTENTITEMBUTTON";
            f["ADDBUTTON"]["@TEXT"] = "+ Add New Phone";
            f["ADDBUTTON"]["@ALIGN"] = "BOTTOM";
            //phone number groups
            JObject props = GetPropertiesByContent(json, pn);
            if (json[pn]["PROPERTIES"] == null)
                json[pn]["PROPERTIES"] = new JObject();
            JObject cgroups = new JObject();
            cgroups["~noname"] = new JObject();
            cgroups["~noname"]["name"] = "";
            cgroups["~noname"]["fields"] = new JObject();
            f = (JObject)cgroups["~noname"]["fields"];
            f["NUMBER"] = props["NUMBER"];
            f["PROTOCOL"] = props["PROTOCOL"];
            f["MSGTYPES"] = props["MSGTYPES"];
            f["CAREAS1"] = props["CAREAS1"];
            f["CAREAS2"] = props["CAREAS2"];
            json[pn]["PROPERTIES"]["Groups"] = cgroups;
            //
            //
            groups["VoiceDialerSettings"] = new JObject();
            groups["VoiceDialerSettings"]["name"] = "Phone Dialer Settings";
            groups["VoiceDialerSettings"]["fields"] = new JObject();
            f = (JObject)groups["VoiceDialerSettings"]["fields"];
            f[vd] = new JObject((JObject)content[vd]);
            f[vd]["@TYPE"] = "CONTENT_KEY";
            f["ADDBUTTON"] = new JObject();
            f["ADDBUTTON"]["@TYPE"] = "ADDCONTENTITEMBUTTON";
            f["ADDBUTTON"]["@TEXT"] = "+ Add New Phone Number";
            f["ADDBUTTON"]["@ALIGN"] = "BOTTOM";
            //voice dialer groups
            props = Array2Object(json[vd]["PROPERTIES"]["PROPERTY"]);//GetPropertiesByContent(json, vd);
            //if (json[vd]["PROPERTIES"] == null)
            //    json[vd]["PROPERTIES"] = new JObject();
            cgroups = new JObject();
            cgroups["~noname"] = new JObject();
            cgroups["~noname"]["name"] = "";
            cgroups["~noname"]["fields"] = props;
            json[vd]["PROPERTIES"]["Groups"] = cgroups;
            //
            //
            groups["UDLSettings"] = new JObject();
            groups["UDLSettings"]["name"] = "UDL Settings";
            groups["UDLSettings"]["fields"] = new JObject();
            f = (JObject)groups["UDLSettings"]["fields"];
            f[udl] = new JObject((JObject)content[udl]);
            f[udl]["@TYPE"] = "CONTENT_KEY";
            //UDL groups
            props = Array2Object(json[udl]["PROPERTIES"]["PROPERTY"]);//GetPropertiesByContent(json, udl);
            //if (json[udl]["PROPERTIES"] == null)
            //    json[udl]["PROPERTIES"] = new JObject();
            cgroups = new JObject();
            cgroups["~noname"] = new JObject();
            cgroups["~noname"]["name"] = "";
            cgroups["~noname"]["fields"] = props;
            json[udl]["PROPERTIES"]["Groups"] = cgroups;
            //
            //
            json["eclipse_communicator"]["PROPERTIES"]["Groups"] = groups;
        }

        private static void ConvertCommunicator(JObject json, JObject _pages)
        {
            JObject ac = (JObject)json["ELEMENTS"]["eclipse_communicator"];
            ac["title"] = _pages["eclipse_communicator"]["title"];
            ac["left"] = _pages["eclipse_communicator"]["left"];
            ac["right"] = _pages["eclipse_communicator"]["right"];
            ac["breadcrumbs"] = _pages["eclipse_communicator"]["breadcrumbs"];
            JObject contains = Contains2Object((JObject)json["ELEMENTS"]["eclipse_communicator"]["CONTAINS"]);
            json["ELEMENTS"]["eclipse_communicator"]["CONTAINS"] = contains;
            CreateCommunicatorGroups((JObject)json["ELEMENTS"]);
        }
        #endregion

        #region devices
        private static void CreateDeviceGroups(JObject json)
        {
            JObject c = (JObject)json["eclipse_devices"]["CONTAINS"];
            JToken t = c.First;
            string devkey = ((JProperty)t).Name;
            JObject d = new JObject((JObject)json[devkey]["PROPERTIES"]);
            JObject o = Array2Object(d["PROPERTY"]);
            //
            d["Groups"] = new JObject();
        }

        private static void ConvertDevices(JObject json, JObject _pages)
        {
            JObject ac = (JObject)json["ELEMENTS"]["eclipse_devices"];
            ac["title"] = _pages["eclipse_devices"]["title"];
            ac["left"] = _pages["eclipse_devices"]["left"];
            ac["right"] = _pages["eclipse_devices"]["right"];
            ac["breadcrumbs"] = _pages["eclipse_devices"]["breadcrumbs"];
            JObject contains = Contains2Object((JObject)json["ELEMENTS"]["eclipse_devices"]["CONTAINS"]);
            json["ELEMENTS"]["eclipse_devices"]["CONTAINS"] = contains;
            CreateDeviceGroups((JObject)json["ELEMENTS"]);
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
            ConvertGenSettings(o1, _pages);
            ConvertUsers(o1, _pages);
            ConvertUser(o1, _pages);
            ConvertZones(o1, _pages);
            ConvertZone(o1);
            ConvertOutputs(o1, _pages);
            ConvertOutput(o1);
            ConvertAreas(o1, _pages);
            ConvertArea(o1);
            ConvertTimeslots(o1, _pages);
            ConvertTimeslot(o1);
            ConvertCommunicator(o1, _pages);
            ConvertDevices(o1, _pages);
            //
            return o1.ToString();
        }
    }
}
