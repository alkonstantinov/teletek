using Newtonsoft.Json.Linq;
using ProstePrototype.POCO;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Markup;
using ljson;
using lcommunicate;
using System.Windows.Controls;
using System.IO;
using System.Text.RegularExpressions;
using System.Linq;
using System.Windows.Controls.Primitives;

namespace ProstePrototype
{
    public class CallbackObjectForJs
    {
        //public void transformToTooltip(string spanContent)
        //{
        //    Application.Current.Dispatcher.Invoke(() =>
        //    {
        //        MainWindow mainWindow = Application.Current.Windows.OfType<MainWindow>().FirstOrDefault();
        //        ToolTip tooltip = new ToolTip();
        //        tooltip.Content = spanContent;
        //        tooltip.Placement = PlacementMode.Mouse;
        //        //tooltip.PlacementTarget = mainWindow.wb1;

        //        mainWindow.wb1.ToolTip = tooltip;
        //    });
        //}
        public string modifyElementAddress(string oldAddress, string elementType, string newAddress)
        {
            cJson.ChangeElementAddress(oldAddress, elementType, newAddress);
            return "{}";
            //return "Hi from " + elementType + " with new address at " + newAddress;
        }
        public string modifyDeviceLoopAddress(string oldAddress, string loopType, string newAddress)
        {
            cJson.ChangeDeviceAddress(oldAddress, loopType, newAddress);
            return "{}";
            //return "Hi from " + loopType + " with new address at " + newAddress;
        }
        public string getUsedInputGroups()
        {
            string sinput = cJson.InputsElementName();
            List<string> lst = cJson.SelectPathValuesDistinctIntSort(sinput + @"[\w\W]*?\.Group\.~index~\d+$");
            try
            {
                File.WriteAllTextAsync("wb3.json", JArray.FromObject(lst).ToString());
            }
            catch { }
            return JArray.FromObject(lst).ToString();
        }
        public string getElements(string elementType)
        {
            //string s = getUsedInputGroups();
            Dictionary<string, string> dres = cComm.GetElements(cJson.CurrentPanelID, elementType);
            if (dres == null)
                return null;
            JObject o = new JObject();
            foreach (string key in dres.Keys)
            {
                JObject ok = JObject.Parse(dres[key]);
                if (ok["~value"] != null)
                    ok["~strtype"] = cJson.DevName(ok["~value"].ToString());
                ok.Remove("~rw");
                o[key] = ok;
            }
            try
            {
                File.WriteAllTextAsync("wb3.json", o.ToString());

            }
              catch { }
            return o.ToString();
        }
        public string getElement(string elementName)
        {
            JObject el = cJson.GetNode(elementName);
            el.Remove("~rw");
            //File.WriteAllTextAsync("wb4.json", el.ToString());
            return el.ToString();
        }
        public string getJsonForElement(string elementType, int elementNumber, string fieldName = "")
        {
            // var e = elementType;
            //return @"{ ""pageName"": ""wb1"" }";
            //string res = cComm.GetListElement(cJson.CurrentPanelID, elementType, elementNumber.ToString());
            string res = cJson.GetListElement(elementType, elementNumber.ToString());
            //
            Application.Current.Dispatcher.Invoke(() =>
            {
                MainWindow mw = Application.Current.Windows.OfType<MainWindow>().FirstOrDefault();
                mw.AddingSegmentElements(elementType, fieldName);
            });
            

            res = cJson.GroupsWithValues(res).ToString();
            res = Regex.Replace(res, @",\s*?""~rw""[\w\W]+$", "") + "\r\n}";
            try
            {

                File.WriteAllText("wb3.json", res);
            } catch { }
            return res;
        }

        public void addingSegmentsToElement(string elementType, string fieldName)
        {
            //
            Application.Current.Dispatcher.Invoke(() =>
            {
                MainWindow mw = Application.Current.Windows.OfType<MainWindow>().FirstOrDefault();
                mw.AddingSegmentElements(elementType, fieldName);
            });

        }

        public string addingElementSync(string elementType, int elementNumber)
        {
            MainWindow.AddingElement(elementType, elementNumber.ToString());
            return "added";
        }

        public string getJsonNodeForElement(string elementType, int elementNumber, string key)
        {
            string res = cComm.GetListElementNode(cJson.CurrentPanelID, elementType, elementNumber.ToString(), key);
            //File.WriteAllTextAsync("wb3.json", res);
            return res;
        }

        public string getLoops(string elementType)
        {
            Dictionary<string, string> dres = cComm.GetPseudoElementsList(cJson.CurrentPanelID, elementType);
            if (dres == null)
                return null;
            JObject o = new JObject();
            foreach (string key in dres.Keys)
            {
                JObject ok = JObject.Parse(dres[key]);
                o[key] = ok["~loop_type"];
            }
            try
            {
            File.WriteAllTextAsync("wb3.json", o.ToString());

            } catch { }
            return o.ToString();
        }

        public string getLoopDevices(string elementType, int elementNumber, string fieldName = "")
        {
            Dictionary<string, string> d = cComm.GetPseudoElementDevices(cJson.CurrentPanelID, elementType, elementNumber.ToString());
            
            if (!String.IsNullOrEmpty(fieldName))
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    MainWindow mw = Application.Current.Windows.OfType<MainWindow>().FirstOrDefault();
                    mw.AddingSegmentElements(elementType, fieldName);
                });
            }
            //string zdevs = zoneDevices(1);
            if (d == null)
                return null;
            //string[] akeys = new string[d.Count];
            //d.Keys.CopyTo(akeys);
            //akeys.so

            JArray res = new JArray();
            foreach (string addr in d.Keys)
            {
                JObject jdev = JObject.Parse(d[addr]);
                string devname = jdev["~device"].ToString();
                jdev.Remove("~rw");
                jdev.Remove("~device");
                jdev.Remove("~device_type");
                jdev = cJson.GroupsWithValues(jdev);
                JObject onew = new JObject();
                onew["Groups"] = jdev;
                onew["~device"] = devname;
                onew["~address"] = addr;
                res.Add(onew);
            }
            try
            {
            File.WriteAllTextAsync("wb3.json", res.ToString());

            } catch { }
            return res.ToString();
        }

        public void setLoopType(string elementType/*NO_LOOP*/, int elementNumber, string typ)
        {
            JObject o = JObject.Parse(cComm.GetPseudoElement(cJson.CurrentPanelID, elementType, elementNumber.ToString()));
            o["~loop_type"] = typ;
            cComm.SetPseudoElement(cJson.CurrentPanelID, elementType, elementNumber.ToString(), o.ToString());
        }

        public string getJsonNode(string elementName, string key)
        {
            JObject jnode = new JObject(cJson.GetNode(elementName));
            if (jnode != null)
            {
                JToken t = jnode[key];
                
                if (t != null)
                {
                    try
                    {
                    File.WriteAllTextAsync("wb3.json", t.ToString());

                    } catch { }
                    return t.ToString();
                }
                if (jnode["PROPERTIES"] != null)
                    t = jnode["PROPERTIES"][key];
                if (t != null)
                {
                    try
                    {
                        File.WriteAllTextAsync("wb3.json", t.ToString());

                    }
                    catch { }
                    return t.ToString();
                }
                if (jnode["CONTAINS"] != null)
                    t = jnode["CONTAINS"];
                if (t != null)
                {
                    try
                    {
                        File.WriteAllTextAsync("wb3.json", t.ToString());

                    }
                    catch { }
                    return t.ToString();
                }
            }
            return null;
        }

        public string loopsInputs(string path)
        {
            JObject o = cJson.LoopsInputs(path);
            try
            {
                File.WriteAllTextAsync("wb4.json", o.ToString());

            }
            catch { }
            return o.ToString();
        }

        public string loopsOutputs(string path)
        {
            JObject o = cJson.LoopsOutputs(path);
            try
            {
            File.WriteAllTextAsync("wb4.json", o.ToString());

            } catch { }
            return o.ToString();
        }
        public string setNodeFilters(string elementName)
        {
            JObject el = cJson.GetNode(elementName);
            cJson.SetNodeFilters(el);
            el.Remove("~rw");
            el["PROPERTIES"]["Groups"] = cJson.GroupsWithValues(el["PROPERTIES"]["Groups"].ToString());
            return el.ToString();
        }
        public string checkLoopConnection(string noLoop, int loopNumber)
        {
            JObject o = cJson.LoopRelations(noLoop, loopNumber);
            //JObject o = new JObject() { 
            //    ["1. IRIS_MIO22"] = new JArray { "IRIS_INPUT1", "IRIS_INPUT2" }, 
            //    ["2. IRIS_MIO04"] = new JArray { "IRIS_OUTPUT1", "IRIS_OUTPUT2" } 
            //};
            return o.ToString();
        }

        public string channelUses(string path)
        {
            JArray a = cJson.ChannelUses(path);
            return (a != null) ? a.ToString() : "[]";
        }

        public string zoneDevices(int elementNumber)
        {
            JArray zdevs = new JArray();
            try
            {
                zdevs = cJson.ZoneDevices(elementNumber.ToString());
            } catch { }
            return (zdevs != null) ? zdevs.ToString() : "[]";
        }
        public string setActivePanel(string panel_id)
        {
            cJson.CurrentPanelID = panel_id;
            return "setSuccess"; //Ако искаме да връщаме нещо, а ако не - ще е void => връщаме, за да действаме синхронно
        }
        public string panelsInLeftBrowser()
        {
            JArray res = cJson.PanelsInLeftBrowser();
            try
            {
            File.WriteAllTextAsync("wb3.json", res.ToString());

            } catch { }
            return res.ToString();
        }
        public string MIMICPanels()
        {
            return JArray.Parse("[]").ToString();
        }
    }
}
