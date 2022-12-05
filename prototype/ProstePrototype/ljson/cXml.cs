using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ljson
{
    public class cXml
    {
        public static string GetID(JToken t)
        {
            return (t["ID"] != null) ? t["ID"].ToString() : t["@ID"].ToString();
        }

        public static void RemoveID(JToken t)
        {
            JObject o = (JObject)t;
            if (t["ID"] != null)
                o.Remove("ID");
            else
                o.Remove("@ID");
        }

        public static JObject Array2Object(JArray arr)
        {
            JObject res = new JObject();
            foreach (JToken t in arr)
            {
                //JObject o = new JObject();
                //o["ID"] = GetID(t);
                string id = GetID(t);
                RemoveID(t);
                res[id] = t;
            }
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
    }
}
