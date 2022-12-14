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
            if (Regex.IsMatch(name, @"ECLIPSE[\w\W]*?_codes$", RegexOptions.IgnoreCase))
                name = "eclipse_codes";
            else if (Regex.IsMatch(name, @"ECLIPSE[\w\W]*?_GENERALSETTINGS$", RegexOptions.IgnoreCase))
                name = "eclipse_gen_settings";
            else if (Regex.IsMatch(name, @"ECLIPSE[\w\W]*?_SYSTEMSETTINGS$", RegexOptions.IgnoreCase))
                name = "eclipse_sys_settings";
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
                            //ChangeContent(content, _pages);
                            ((JObject)fo)["CONTAINS"] = content;
                        }
                    }
                    //foreach (JToken et in )
                    //{

                    //}
                    if (elements.Count > 0)
                    {
                        TranslateObjectsKeys(elements);
                        elements["eclipse"]["title"] = _pages["eclipse"]["title"];// "Iris";
                        elements["eclipse"]["left"] = _pages["eclipse"]["left"];//"IRIS/divIRIS.html";
                        elements["eclipse"]["right"] = _pages["eclipse"]["right"];//"IRIS/IRISPANEL.html";
                        elements["eclipse"]["breadcrumbs"] = _pages["eclipse"]["breadcrumbs"];//JArray.Parse("[\"index\"]");
                        //cIRIS.CreateMainGroups(elements);
                        //
                        if (p.Name != "ELEMENT")
                            o1[p.Name] = elements;
                        else
                            o1["ELEMENTS"] = elements;
                    }
                }

            }
            //
            return o1.ToString();
        }
    }
}
