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

        public static void RemoveProp(JObject o, string name)
        {
            if (o[name] != null)
                o.Remove(name);
            else if (o["@" + name] != null)
                o.Remove("@" + name);
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
            else
            {
                JProperty f = (JProperty)arr.First;
                if (f.Value.Type == JTokenType.Array)
                    jarr = (JArray)f.Value;
            }
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

        private static List<string> ObjectTypes(JObject o)
        {
            List<string> res = new List<string>();
            foreach (JToken t in o.SelectTokens("$.*"))
            {
                if (t.Type == JTokenType.Object)
                {
                    List<string> subs = ObjectTypes((JObject)t);
                    foreach (string typ in subs)
                        res.Add(typ);
                }
                else if (t.Parent.Type == JTokenType.Property)
                {
                    JProperty p = (JProperty)t.Parent;
                    if (p.Value.Type == JTokenType.String && p.Name == "@TYPE")
                        res.Add(p.Value.ToString());
                    else if (p.Value.Type == JTokenType.Object)
                    {
                        List<string> subs = ObjectTypes((JObject)p.Value);
                        foreach (string typ in subs)
                            res.Add(typ);
                    }
                }
            }
            //
            return res;
        }

        private static bool OnlyChecksInGroup(JObject grp)
        {
            List<string> _object_types = ObjectTypes(grp);
            if (_object_types == null || _object_types.Count == 0)
                return false;
            foreach (string val in _object_types)
                if (val != "CHECK")
                    return false;
            return true;
        }

        private static JObject FieldsParent(JObject o)
        {
            JContainer res = o;
            while (res != null && (res.Type != JTokenType.Object || ((JObject)res)["fields"] == null))
                res = res.Parent;
            return (res != null) ? (JObject)res["fields"] : null;
        }

        public static void Arrays2Objects(JObject root, bool _convert_checks)
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
                                if (Regex.IsMatch(prop.Path, @"^ELEMENTS\.IRIS8_INPUT\.PROPERTIES\.Groups\.InputType\.fields\.Type\.TABS\.d076749b-3d30-4cb2-8785-fd82145fd7dd\.PROPERTIES"))
                                {
                                    JToken ttt = t;
                                }
                                foreach (JToken tarr in t)
                                    if (tarr.Type == JTokenType.Object)
                                    {
                                        ((JObject)tarr)["~path"] = tarr.Path;
                                        Arrays2Objects((JObject)(tarr), _convert_checks);
                                    }

                                //continue;
                            }
                            else if (Regex.IsMatch(path, @"TABS\.TAB$"))
                            {
                                JProperty parent = (JProperty)prop.Parent.Parent;
                                //JProperty tparent = (JProperty)parent;
                                JObject tpo = Array2Object((JToken)parent.Value);
                                parent.Value = tpo;
                                Arrays2Objects((JObject)(parent.Value), _convert_checks);
                            }
                            else
                            {
                                foreach (JToken tarr in t)
                                    if (tarr.Type == JTokenType.Object)
                                        ((JObject)tarr)["~path"] = tarr.Path;
                                continue;
                            }
                        }
                        else
                        {
                            string hhh = "";
                            continue;
                        }
                    }
                }
                else if (t.Type == JTokenType.Object)
                {
                    JObject o = (JObject)t;
                    o["~path"] = t.Path;
                    if (_convert_checks && o["@TYPE"] != null && o["@TYPE"].ToString() == "CHECK")
                    {
                        JObject fo = FieldsParent(o);
                        if (fo != null)
                        {
                            bool _checks_only = OnlyChecksInGroup(fo);
                            if (!_checks_only)
                                o["@TYPE"] = "SLIDER";
                        }
                    }
                    Arrays2Objects(o, _convert_checks);
                }
            }
            //root["~path"] = root.Path;
        }
    }
}
