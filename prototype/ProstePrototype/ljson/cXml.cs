using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ljson
{
    public class cXml
    {
        public static string GetID(JToken t)
        {
            if (t["ID"] != null)
                return t["ID"].ToString();
            else if (t["@ID"] != null)
                return t["@ID"].ToString();
            else if (t["@LNGID"] != null)
                return t["@LNGID"].ToString();
            return null;
        }

        public static void RemoveID(JToken t)
        {
            JObject o = (JObject)t;
            if (t["ID"] != null)
                o.Remove("ID");
            else if (t["@ID"] != null)
                o.Remove("@ID");
        }

        public static JObject Array2Object(JArray arr)
        {
            JObject res = new JObject();
            foreach (JToken t in arr)
            {
                //JObject o = new JObject();
                //o["ID"] = GetID(t);
                if (t.Type == JTokenType.Object)
                {
                    string id = GetID(t);
                    if (id != null)
                    {
                        RemoveID(t);
                        res[id] = t;
                    }
                    else
                        return null;
                }
                else
                    return null;
            }
            return res;
        }

        public static JObject Array2Object(JToken arr)
        {
            JArray jarr = null;
            if (arr.Type == JTokenType.Array)
                jarr = (JArray)arr;
            JProperty f = (JProperty)arr.First;
            if (f.Value.Type == JTokenType.Array)
                jarr = (JArray)f.Value;
            if (jarr != null)
                return Array2Object(jarr);
            string id = GetID(arr);
            if (id == null)
                return null;
            RemoveID(arr);
            JObject res = new JObject();
            res.Add(id, arr);
            return res;
        }

        public static JObject Contains2Object(JToken contains)
        {
            if (contains.Type == JTokenType.Array)
                return Array2Object((JArray)contains);
            JProperty f = (JProperty)contains.First;
            if (f.Value.Type == JTokenType.Array)
                return Array2Object((JArray)f.Value);
            string id = GetID(f.Value);
            RemoveID(f.Value);
            JObject res = new JObject();
            res[id] = f.Value;
            return res;
        }

        public static void Arrays2Objects(JObject root)
        {
            foreach (JToken t in root.SelectTokens("$.*"))
            {
                if (t.Type == JTokenType.Array)
                {
                    JObject o = Contains2Object(t);
                    if (o != null)
                    {
                        JTokenType typ = t.Parent.Type;
                        if (typ == JTokenType.Property)
                        {
                            JProperty prop = (JProperty)t.Parent;
                            string path = prop.Path;
                            string name = prop.Name;
                            if (name == "PROPERTY" || Regex.IsMatch(name, @"^\s*?#") ||
                                Regex.IsMatch(path, @"Groups\.[\w\W]*?\.fields\.[\w\W]*?.ITEM$", RegexOptions.IgnoreCase) ||
                                Regex.IsMatch(path, @"PROPERTIES\.OLD", RegexOptions.IgnoreCase)
                               )
                            {
                                foreach (JToken tarr in t)
                                    if (tarr.Type == JTokenType.Object)
                                        ((JObject)tarr)["~path"] = tarr.Path;
                                continue;
                            }
                            else if (Regex.IsMatch(path, @"TABS\.TAB$"))
                            {
                                JProperty parent = (JProperty)prop.Parent.Parent;
                                //JProperty tparent = (JProperty)parent;
                                JObject tpo = Array2Object((JToken)parent.Value);
                                parent.Value = tpo;
                                Arrays2Objects((JObject)(parent.Value));
                            }
                            else
                            {
                                foreach (JToken tarr in t)
                                    if (tarr.Type == JTokenType.Object)
                                        ((JObject)tarr)["~path"] = tarr.Path;
                                continue;
                            }
                        }
                    }
                }
                else if (t.Type == JTokenType.Object)
                {
                    JObject o = (JObject)t;
                    o["~path"] = t.Path;
                    Arrays2Objects(o);
                }
            }
        }
    }
}
