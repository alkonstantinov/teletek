using common;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace lcommunicate
{
    internal class cTDF : cTransport
    {
        private Dictionary<string, byte[]> _commands = new Dictionary<string, byte[]>();
        private string PropertySBytes(string propid, JObject tdf_props)
        {
            if (propid == null || tdf_props == null)
                return null;
            propid = propid.ToUpper();
            JToken tprops = tdf_props["PROPERTY"];
            if (tprops != null && tprops.Type == JTokenType.Object && tprops["@ID"] != null && tprops["@VALUE"] != null)
                return tprops["@VALUE"].ToString();
            else if (tprops != null && tprops.Type == JTokenType.Array)
            {
                JArray aprops = (JArray)tprops;
                foreach (JToken t in aprops)
                {
                    if (t.Type != JTokenType.Object || t["@ID"] == null || t["@ID"].ToString().ToUpper() != propid)
                        continue;
                    if (t["@VALUE"] == null)
                        return null;
                    else
                        return t["@VALUE"].ToString();
                }
            }
            else
                return null;
            return null;
        }
        private void ProcessElement(JObject relement, JObject felement)
        {
            JToken tprop = relement["PROPERTIES"]["PROPERTY"];
            if (tprop != null && tprop.Type == JTokenType.Object)
            {
                string sbytes = PropertySBytes(tprop["@ID"].ToString(), (JObject)felement["PROPERTIES"]);
                if (sbytes == null)
                {
                    int i = Convert.ToInt32(tprop["@LENGTH"].ToString()) * 2;
                    sbytes = "";
                    while (sbytes.Length < i)
                        sbytes += "0";
                }
                tprop["~values"] = sbytes;
            }
            else if (tprop != null && tprop.Type == JTokenType.Array)
            {
                JArray aprops = (JArray)tprop;
                foreach (JToken t in aprops)
                {
                    string sbytes = PropertySBytes(t["@ID"].ToString(), (JObject)felement["PROPERTIES"]);
                    if (sbytes == null)
                    {
                        int i = Convert.ToInt32(t["@LENGTH"].ToString()) * 2;
                        sbytes = "";
                        while (sbytes.Length < i)
                            sbytes += "0";
                    }
                    t["~values"] = sbytes;
                }
            }
        }
        private void FillZProperties(JArray props)
        {
            foreach (JToken p in props)
            {
                string val = "";
                int i = Convert.ToInt32(p["@LENGTH"].ToString()) * 2;
                while (val.Length < i)
                    val += "0";
                p["~values"] = val;
            }
        }
        private void FillZElements(JArray elements)
        {
            if (elements == null)
                return;
            foreach (JToken t in elements)
            {
                if (t["PROPERTIES"] != null)
                    FillZProperties((JArray)t["PROPERTIES"]["PROPERTY"]);
                else
                    FillZElements((JArray)t["ELEMENTS"]["ELEMENT"]);
            }
        }
        private void FillElements(JArray aread, JArray afile)
        {
            foreach (JToken t in aread)
            {
                string rid = t["@ID"].ToString();
                //if (Regex.IsMatch(rid, "INPUT$", RegexOptions.IgnoreCase))
                //{
                //    rid = rid;
                //}
                JObject of = null;
                foreach (JToken tf in afile)
                    if (tf["@ID"] != null && (tf["@ID"].ToString() == rid || (Regex.IsMatch(rid, "NONE$") && tf["~readed"] == null)))
                    {
                        if (tf["~readed"] != null)
                            continue;
                        tf["~readed"] = "true";
                        of = (JObject)tf;
                        break;
                    }
                if (of != null)
                {
                    if (t["PROPERTIES"] != null)
                    {
                        ProcessElement((JObject)t, of);
                    }
                    else
                    {
                        JArray af = null;
                        JToken telements = of["ELEMENTS"]["ELEMENT"];
                        if (telements.Type == JTokenType.Array)
                            af = (JArray)of["ELEMENTS"]["ELEMENT"];
                        else
                        {
                            af = new JArray();
                            af.Add(of["ELEMENTS"]["ELEMENT"]);
                        }
                        FillElements((JArray)t["ELEMENTS"]["ELEMENT"], af);
                    }
                }
                else
                    FillZElements((JArray)t["ELEMENTS"]["ELEMENT"]);
            }
        }
        private Dictionary<string, string> MakeCommands(Dictionary<string, string> res, JObject json)
        {
            if (res == null)
                res = new Dictionary<string, string>();
            //
            if (json["COMMANDS"] != null && json["PROPERTIES"] != null)
            {
                string prop_path = Regex.Replace(json["PROPERTIES"]["PROPERTY"].Path, @"^[\w\W]+?ELEMENTS\.ELEMENT\.", "");
                int prop_idx = 0;
                foreach (JToken tcmd in json["COMMANDS"]["COMMAND"])
                {
                    string cmd = null;
                    if (tcmd.Type == JTokenType.Object)
                        cmd = tcmd["@BYTES"].ToString();
                    else if (tcmd.Type == JTokenType.Property)
                        cmd = ((JProperty)tcmd).Value.ToString();
                    if (cmd.Length < 12)
                        continue;
                    string slen = cmd.Substring(cmd.Length - 2);
                    int len = Convert.ToInt32(slen, 16);
                    if (!res.ContainsKey(cmd))
                        res.Add(cmd, "");
                    byte next_start = 2;
                    while (true)
                    {
                        if (res[cmd].Length / 2 >= len)
                            break;
                        JToken tprop = json.SelectToken(prop_path + "[" + prop_idx.ToString() + "]");
                        if (tprop == null)
                        {
                            while (res[cmd].Length / 2 < len)
                                res[cmd] += "00";
                            break;
                        }
                        int prop_start = Convert.ToInt32(tprop["@BYTE"].ToString());
                        while (next_start < prop_start)
                        {
                            res[cmd] += "00";
                            next_start++;
                        }
                        next_start += Convert.ToByte(tprop["@LENGTH"].ToString());
                        if ((res[cmd].Length + tprop["~values"].ToString().Length) / 2 >= len)
                        {
                            while (res[cmd].Length / 2 < len)
                                res[cmd] += "00";
                            break;
                        }
                        prop_idx++;
                        res[cmd] += tprop["~values"].ToString();
                    }
                }
            }
            if (json["ELEMENTS"] != null)
            {
                JArray ae = (JArray)json["ELEMENTS"]["ELEMENT"];
                foreach (JToken t in ae)
                    if (t.Type == JTokenType.Object)
                        MakeCommands(res, new JObject((JObject)t));
            }
            //
            return res;
        }
        private void FillCommands(JObject readcfg, JObject tdf, JObject template)
        {
            JObject relement = (JObject)readcfg["OPERATIONS"]["ELEMENTS"]["ELEMENT"];
            JObject felement = (JObject)tdf["ELEMENT"]["ELEMENTS"]["ELEMENT"];
            ProcessElement(relement, felement);
            JArray ar = (JArray)relement["ELEMENTS"]["ELEMENT"];
            JArray af = (JArray)felement["ELEMENTS"]["ELEMENT"];
            FillElements(ar, af);
            Dictionary<string, string> scmd = MakeCommands(null, new JObject((JObject)readcfg["OPERATIONS"]["ELEMENTS"]["ELEMENT"]));
            if (_commands == null)
                _commands = new Dictionary<string, byte[]>();
            _commands.Clear();
            foreach (string cmd in scmd.Keys)
            {
                if (_commands.ContainsKey(cmd.ToLower()))
                    continue;
                byte[] cmdres = new byte[2 + scmd[cmd].Length / 2];
                for (int i = 2; i < cmdres.Length; i++)
                    cmdres[i] = Convert.ToByte(scmd[cmd].Substring((i - 2) * 2, 2), 16);
                _commands.Add(cmd.ToLower(), cmdres);
            }
        }
        internal override object Connect(object o)
        {
            if (!(o is cTDFParams))
                return null;
            cTDFParams _conn = (cTDFParams)o;
            if (_conn.tdf == null || _conn.readcfg == null)
                return null;
            FillCommands(new JObject(_conn.readcfg), new JObject(_conn.tdf), new JObject(_conn.template));
            return "Ok";
        }
        public override object GetCache()
        {
            return _commands;
        }
        internal override object ConnectCached(object o, object _cache)
        {
            if (!(o is cTDFParams))
                return null;
            cTDFParams _conn = (cTDFParams)o;
            if (_conn.tdf == null || _conn.readcfg == null)
                return null;
            if (_cache != null)
                _commands = (Dictionary<string, byte[]>)_cache;
            if (_commands == null || _commands.Count == 0)
                FillCommands(new JObject(_conn.readcfg), new JObject(_conn.tdf), new JObject(_conn.template));
            return "Ok";
        }
        private byte[] ZArray(string cmd)
        {
            byte[] res = new byte[Convert.ToByte(cmd.Substring(cmd.Length - 2), 16) + 2];
            //for (int i = 0; i < res.Length; i++)
            //    res[i] = 0;
            return res;
        }
        private byte[] ZArray(byte[] cmd)
        {
            byte[] res = new byte[cmd[cmd.Length - 1] + 2];
            //for (int i = 0; i < res.Length; i++)
            //    res[i] = 0;
            return res;
        }
        internal override byte[] SendCommand(object _connection, byte[] _command)
        {
            string scmd = "";
            for (int i = 0; i < _command.Length; i++)
                scmd += _command[i].ToString("X2");
            byte[] res = null;
            if (_commands.ContainsKey(scmd.ToLower()))
                res = _commands[scmd.ToLower()];
            else
                res = ZArray(_command);
            return res;
        }
        internal override byte[] SendCommand(object _connection, string _command)
        {
            byte[] res = null;
            if (_commands.ContainsKey(_command.ToLower()))
                res = _commands[_command.ToLower()];
            else
                res = ZArray(_command);
            return res;
        }
        internal override byte[] SendCommand(byte[] _command)
        {
            byte[] res = null;
            string scmd = "";
            for (int i = 0; i < _command.Length; i++)
                scmd += _command[i].ToString("X2");
            if (_commands.ContainsKey(scmd.ToLower()))
                res = _commands[scmd.ToLower()];
            else
                res = ZArray(_command);
            return res;
        }
        internal override byte[] SendCommand(string _command)
        {
            byte[] res = null;
            //if (_command == "035101A0005d")
            //{
            //    _command = _command;
            //}
            if (_commands.ContainsKey(_command.ToLower()))
                res = _commands[_command.ToLower()];
            else
                res = ZArray(_command);
            return res;
        }
    }
}
