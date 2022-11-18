using Newtonsoft.Json.Linq;
// using System.Xml;
using System.Xml.Linq;

namespace NewTeletekSW.Utils
{
    public static class Converter
    {
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
        public static void WriteXML(string folder)
        {


        }
    }
}