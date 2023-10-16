using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Net;
using common;
using System.Text.RegularExpressions;
using System.Security.Policy;
//using System.IO.Hashing;
//using System.Runtime.Intrinsics.Arm;

namespace lupd
{
    public static class cUpd
    {
        public static JObject FilesMap(dFileCRCProcessing crcprocess, string path = ".")
        {
            JObject res = crc32.files(crcprocess, path);
            return res;
        }
        public static string FilesMapStr(dFileCRCProcessing crcprocess, string path = ".")
        {
            return FilesMap(crcprocess, path).ToString();
        }
        private static void SetAction(JObject o, string action)
        {
            foreach (JProperty p in o.Properties())
                if (p.Value.Type == JTokenType.Object)
                {
                    JObject v = (JObject)p.Value;
                    v["action"] = action;
                    SetAction(v, action);
                }
        }
        private static void filesDiff(JObject srvFiles, JObject oFiles, JObject res)
        {
            if (res == null) res = new JObject();
            //
            foreach (JProperty p in srvFiles.Properties())
            {
                if (p.Value.Type != JTokenType.Object) continue;
                JObject srvObj = (JObject)p.Value;
                res[p.Name] = new JObject(srvObj);
                if (oFiles[p.Name] == null)
                {
                    res[p.Name] = new JObject(srvObj);
                    res[p.Name]["action"] = "add";
                    SetAction((JObject)res[p.Name], "add");
                    continue;
                }
                JObject lObj = (JObject)oFiles[p.Name];
                if (srvObj["type"].ToString() == "file" && srvObj["crc"].ToString() != lObj["crc"].ToString())
                {
                    res[p.Name] = new JObject(srvObj);
                    res[p.Name]["action"] = "new";
                    continue;
                }
                res[p.Name]["action"] = "none";
                if (srvObj["type"].ToString() == "dir")
                    filesDiff(srvObj, lObj, (JObject)res[p.Name]);
            }
        }
        private static void files_with_action(JObject diff, JObject res, string relpath)
        {
            if (res == null) res = new JObject();
            foreach (JProperty p in diff.Properties())
            {
                if (p.Value.Type != JTokenType.Object) continue;
                JObject o = (JObject)p.Value;
                string fname = relpath;
                if (!Regex.IsMatch(fname, @"[\\/]$")) fname += "/";
                fname += o["Filename"].ToString();
                if (o["action"].ToString() == "none" && o["type"].ToString() != "dir") continue;
                else if (o["type"].ToString() == "dir")
                {
                    files_with_action(o, res, fname);
                    continue;
                }
                string key = fname.ToLower();
                res[key] = new JObject();
                foreach (JProperty pp in o.Properties()) if (pp.Value.Type != JTokenType.Object) res[key][pp.Name] = pp.Value;
                res[key]["relpath"] = fname;
                if (o["type"].ToString() == "dir") files_with_action(o, res, fname);
            }
        }
        public static JObject files4download(dFileCRCProcessing crcprocess, JObject oFiles, JObject srvFiles, string path = ".")
        {
            JObject diff = new JObject();
            filesDiff(srvFiles, oFiles, diff);
            JObject res = new JObject();
            files_with_action(diff, res, "");
            //
            return res;
        }
        public static JObject files4download(dFileCRCProcessing crcprocess, JObject srvFiles, string path = ".")
        {
            JObject oFiles = crc32.files(crcprocess, path, null);
            //
            return files4download(crcprocess, oFiles, srvFiles, path);
        }
        public static string files4download(string srvFiles, dFileCRCProcessing crcprocess, string path = ".")
        {
            return files4download(crcprocess, JObject.Parse(srvFiles), path).ToString();
        }
        private static byte[] HTTPGet(string uri, ref eUPDResult res, ref string err, int counter, int cntall, dFileDownloadProgress downloading)
        {
            WebResponse oResp = null;
            try
            {
                oResp = WebRequest.Create(uri).GetResponse();
            }
            catch (System.Net.WebException e)
            {
                HttpWebResponse resp = (HttpWebResponse)e.Response;
                if (resp != null && resp.StatusCode == HttpStatusCode.NotFound)
                {
                    res = eUPDResult.FilesMapNotExists;
                    err = uri + "\r\n" + resp.StatusDescription;
                }
                else if (resp == null) res = eUPDResult.NoInternet;
                else res = eUPDResult.Other;
                return null;
            }
            byte[] content = new byte[oResp.ContentLength];
            BinaryReader reader = new BinaryReader(oResp.GetResponseStream());
            bool f = true;
            int total = 0;
            while (f)
            {
                int readed = reader.Read(content, total, content.Length - total);
                total += readed;
                if (downloading != null) downloading(uri, counter, cntall, total, content.Length);
                f = total < content.Length;
            }
            return content;
        }
        public static JObject fMap(ref eUPDResult res, ref string err)
        {
            string updhttppath = settings.updhttppath;
            if (!Regex.IsMatch(updhttppath, @"/$")) updhttppath += "/";
            string updmapfilename = settings.updmapfilename;
            string uri = updhttppath + updmapfilename;
            byte[] content = HTTPGet(uri, ref res, ref err, 1, 1, null);
            if (res != eUPDResult.Ok) return null;
            string fmap = Encoding.UTF8.GetString(content);
            return JObject.Parse(fmap);
        }
        private static JObject updPathMap(string updpath, dFileCRCProcessing crcprocess)
        {
            if (!Regex.IsMatch(updpath, @"[\\/]$")) updpath += @"\";
            string fmappath = updpath + settings.updmapfilename;
            if (System.IO.File.Exists(fmappath))
            {
                string smap = System.IO.File.ReadAllText(fmappath);
                return JObject.Parse(smap);
            }
            return FilesMap(crcprocess, updpath);
        }
        private static void Save2Locks(string updpath, string _relpath, byte[] content)
        {
            if (!Regex.IsMatch(updpath, @"[\\/]$")) updpath += Path.DirectorySeparatorChar.ToString();
            string locks = updpath + "~locks" + Path.DirectorySeparatorChar.ToString() + Regex.Replace(_relpath, @"^[\\/]", "");
            string dir = Path.GetDirectoryName(locks);
            if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
            File.WriteAllBytes(locks, content);
        }
        private static void SaveDelFiles(string updpath, JObject files4del)
        {
            if (files4del == null || files4del.First == null) return;
            if (!Regex.IsMatch(updpath, @"[\\/]$")) updpath += Path.DirectorySeparatorChar.ToString();
            string delfile = updpath + "~files4del.lock";
            string s = "";
            foreach (JProperty p in files4del.Properties())
            {
                JObject o = (JObject)p.Value;
                if (o["action"].ToString().ToLower() != "add") continue;
                string sf = Regex.Replace(Regex.Replace(o["relpath"].ToString(), @"^[\\/]", ""), @"[\\/]", Path.DirectorySeparatorChar.ToString());
                string fpath = Regex.Replace(updpath, @"[\\/]$", "") + Path.DirectorySeparatorChar + sf;
                s += ((s != "") ? "\n" : "") + fpath;
            }
            File.WriteAllText(delfile, s);
        }
        private static void Download(string updpath, JObject downloads, dFileDownloadProgress downloading, ref eUPDResult res, ref string err)
        {
            string updhttppath = Regex.Replace(settings.updhttppath, @"[\\/]$", "");
            int cntall = downloads.Properties().Count();
            int counter = 0;
            foreach (JProperty p in downloads.Properties())
            {
                counter++;
                JObject of = (JObject)p.Value;
                string fpath = updhttppath + "/" + Regex.Replace(of["relpath"].ToString(), @"^[\\/]", "");
                //if (downloading != null) downloading(fpath, counter, cntall);
                byte[] content = HTTPGet(fpath, ref res, ref err, counter, cntall, downloading);
                if (res != eUPDResult.Ok) return;
                //
                string savepath = Regex.Replace(Regex.Replace(updpath, @"[\\/]", Path.DirectorySeparatorChar.ToString()), @"[\\/]$", "") + Path.DirectorySeparatorChar;
                string _relpath = Regex.Replace(Regex.Replace(of["relpath"].ToString(), @"^[\\/]", ""), @"[\\/]", Path.DirectorySeparatorChar.ToString());
                savepath += _relpath;
                string dir = Path.GetDirectoryName(savepath);
                if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
                try
                {
                    File.WriteAllBytes(savepath, content);
                }
                catch
                {
                    Save2Locks(updpath, _relpath, content);
                }
            }
        }
        private static void DeleteUnusedFiles(string updpath, JObject files4del)
        {
            Dictionary<string, string> dir4del = new Dictionary<string, string>();
            foreach (JProperty p in files4del.Properties())
            {
                JObject o = (JObject)p.Value;
                if (o["action"].ToString().ToLower() != "add") continue;
                string s = Regex.Replace(Regex.Replace(o["relpath"].ToString(), @"^[\\/]", ""), @"[\\/]", Path.DirectorySeparatorChar.ToString());
                string fpath = Regex.Replace(updpath, @"[\\/]$", "") + Path.DirectorySeparatorChar + s;
                string dir = Path.GetDirectoryName(fpath);
                if (!dir4del.ContainsKey(dir)) dir4del.Add(dir, null);
                File.Delete(fpath);
            }
            foreach (string dir in dir4del.Keys) if (Directory.Exists(dir) && Directory.GetFiles(dir).Length == 0) Directory.Delete(dir, true);
        }
        public static eUPDResult DoUpdate(string updpath, dFileCRCProcessing crcProcessing, dFileDownloadProgress downloading, ref string err)
        {
            eUPDResult res = eUPDResult.Ok;
            err = "";
            if (res != eUPDResult.Ok) return res;
            JObject fmap = fMap(ref res, ref err);
            if (res != eUPDResult.Ok) return res;
            JObject localmap = updPathMap(updpath, crcProcessing);
            JObject downloads = files4download(crcProcessing, localmap, fmap);
            Download(updpath, downloads, downloading, ref res, ref err);
            JObject files4del = files4download(crcProcessing, fmap, localmap);
            try
            {
                DeleteUnusedFiles(updpath, files4del);
            }
            catch
            {
                SaveDelFiles(updpath, files4del);
            }
            //
            string fmapfile = Regex.Replace(Regex.Replace(updpath, @"[\\/]$", ""), @"[\\/]", Path.DirectorySeparatorChar.ToString());
            fmapfile += Path.DirectorySeparatorChar + settings.updmapfilename;
            File.WriteAllText(fmapfile, fmap.ToString());
            //
            return res;
        }
        public static bool Check4Updates(string updpath)
        {
            eUPDResult res = eUPDResult.Ok;
            string err = "";
            JObject fmap = fMap(ref res, ref err);
            if (res != eUPDResult.Ok) return false;
            JObject localmap = updPathMap(updpath, null);
            JObject downloads = files4download(null, localmap, fmap);
            return downloads != null && downloads.Properties().Count() > 0;
        }
    }
}