using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
// using System.Xml;
using System.Xml.Linq;

namespace NewTeletekSW.Utils
{
    public static class Converter
    {
        public static string keyH = "";
        public static int count = 0;

        #region Read
        public static void ReadXML(string filename)
        {
            XDocument xml = XDocument.Load(filename);
            JObject json = new JObject();

            foreach (var xmlEl in xml.Elements())
            {
                JObject jsonTemp = new JObject();
                ConvertElements(xmlEl, jsonTemp);
                Checker(xmlEl, json, jsonTemp);
                //json.Add(xmlEl.Name.ToString(), json);
                string jsonString = json.ToString();
                System.IO.File.WriteAllText(Directory.GetCurrentDirectory() + "path.txt", jsonString);
                //Console.WriteLine(json);
            }
        }

        private static void ConvertElements(XElement node, JObject jsonObject)
        {
            var childs = new JObject();
            if (node.HasElements)
            {
                if (node.HasAttributes)
                {
                    AddAttributes(node, childs);
                }
                foreach (var element in node.Elements())
                {
                    //Console.WriteLine(childs);
                    ConvertElements(element, childs);
                }
                Checker(node, jsonObject, childs);
                //jsonObject.Add(node.Name.ToString(), childs);
            }
            else if (node.HasAttributes)
            {
                AddAttributes(node, childs);
                Checker(node, jsonObject, childs);
            }
        }

        private static void AddAttributes(XElement node, JObject childs)
        {
            foreach (var attribute in node.Attributes())
            {
                var value = attribute.Value;
                if (childs.ContainsKey(attribute.Name.ToString()))
                {
                    var prevValue = childs.GetValue(attribute.Name.ToString());
                    JArray series = new JArray();
                    if (prevValue != null)
                    {
                        if (prevValue.Type == JTokenType.Array)
                        {
                            ((JArray)prevValue).Add(value);
                            series = (JArray)prevValue;
                        }
                        else
                        {
                            series.Add(prevValue);
                            series.Add(value);
                        }
                    }
                    childs[attribute.Name.ToString()] = series;
                }
                else
                {
                    childs.Add(attribute.Name.ToString(), value);
                }
            }
        }

        private static void Checker(XElement node, JObject jsonObject, JObject childs)
        {
            if (jsonObject.ContainsKey(node.Name.ToString()))
            {
                var prevValue = jsonObject.GetValue(node.Name.ToString());
                JArray series = new JArray();
                if (prevValue != null)
                {
                    if (prevValue.Type == JTokenType.Array)
                    {
                        ((JArray)prevValue).Add(childs);
                        series = (JArray)prevValue;
                    }
                    else
                    {
                        series.Add(prevValue);
                        series.Add(childs);
                    }
                }
                jsonObject[node.Name.ToString()] = series;
            }
            else
            {
                jsonObject.Add(node.Name.ToString(), childs);
            }
        }
        #endregion
        public static JObject WriteXML(string filename, JObject json, string key = "en")
        {
            keyH = key.ToLower();
            count = 0;
            XDocument xml = XDocument.Load(filename);

            foreach (var xmlEl in xml.Elements())
            {
                ConvertElementsText(xmlEl, json);
                
                //JObject jsonTemp = new JObject();
                //CheckerText(xmlEl, json, jsonTemp);
                //json.Add(xmlEl.Name.ToString(), json);
            }

            //Console.WriteLine(json);
            //System.IO.File.WriteAllText(Directory.GetCurrentDirectory() + "path.txt", json.ToString());
            return json;
        }

        private static void ConvertElementsText(XElement node, JObject jsonObject)
        {
            if (node.HasElements)
            {
                if (node.HasAttributes)
                {
                    AddAttributesText(node, jsonObject);
                }
                foreach (var element in node.Elements())
                {
                    //Console.WriteLine(childs);
                    ConvertElementsText(element, jsonObject);
                }
                // CheckerText(node, jsonObject, jsonObject);
                //jsonObject.Add(node.Name.ToString(), childs);
            }
            else if (node.HasAttributes)
            {
                AddAttributesText(node, jsonObject);
                //CheckerText(node, jsonObject, jsonObject);
            }
        }

        private static void AddAttributesText(XElement node, JObject childs)
        {
            JObject idMemo = new JObject();
            string currentId = "";
            string keyJ = "";
            foreach (var attribute in node.Attributes())
            {
                /*
                 * { 
                 * keyJ1: { en: "dscsdcsd", bg:"dsfsdf", id: "weewwe"}, 
                 * keyJ2: { en: "csdcdsc", bg:"brgb", id: "revg"},
                 * ...
                 * }
                 */
                if (attribute.Name == "TEXT")
                {
                    count += 1;
                    string value = attribute.Value;
                    if (keyH == "en")
                    {
                        keyJ = value.ToLower().Replace(" ", "_").Trim(new Char[] { ' ', '*', '.', '?', '!' });
                        if (childs.ContainsKey(keyJ))
                        {                            
                            var prevValue = childs[keyJ];
                            if (prevValue != null)
                            {
                                ((JObject)prevValue).Merge(new JObject { { keyH, value } });
                                prevValue["countLst"]?.Append(count);
                            }

                        }
                        else
                        {
                            var countLst = new int[] { count };
                            JObject newValue = new()
                            {
                                [ keyH ] = value, 
                                ["countLst"] = new JArray { countLst }
                            };
                            newValue.Merge(JsonConvert.DeserializeObject<JObject>(JsonConvert.SerializeObject(idMemo)));
                            childs.Add(keyJ, newValue);
                            // childs[keyJ]?.Concat(JsonConvert.DeserializeObject<JObject>(JsonConvert.SerializeObject(idMemo))); // deep copy of idMemo-rized)
                            idMemo = new JObject();
                        }
                    } else
                    {
                        var allKeys = childs.Properties();
                        foreach (var kk in allKeys)
                        {
                            if (childs[kk.Name]?["id"] != null && childs[kk.Name]["id"].ToString() == new JArray { currentId }.ToString())
                            {
                                keyJ = kk.Name;
                            }
                            else if(childs[kk.Name]?["countLst"] != null && ((JArray)childs[kk.Name]?["countLst"]).Contains(count))
                            {
                                keyJ = kk.Name;
                            }
                        }
                        if (keyJ != "") 
                        { 
                            var prevValue = childs[keyJ];
                            if (prevValue != null)
                                ((JObject)prevValue).TryAdd(keyH, value);
                        }
                    }
                    
                } else if (attribute.Name == "ID")
                {
                    string idValue = attribute.Value;
                    if (keyH == "en")
                    {
                        idMemo.Add("id", new JArray { idValue });
                    } else
                    {
                        currentId = idValue;
                    }
                }
            }
        }
    }
}