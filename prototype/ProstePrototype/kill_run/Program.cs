using System;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Xml.Linq;

namespace kill_run
{
    internal class Program
    {
        static void CopyFiles(string fromDir, string toDir)
        {
            string[] dirs = Directory.GetDirectories(fromDir);
            string[] files = Directory.GetFiles(fromDir);
            //
            foreach (string dir in dirs)
            {
                string dName = Path.GetFileName(dir);
                string reldir = Regex.Replace(toDir, @"[\\/]$", "") + Path.DirectorySeparatorChar + dName;
                CopyFiles(dir, reldir);
                //res[key] = files(crcprocess, dir, (JObject)res[key]);
                //res[key]["type"] = "dir";
                //res[key]["Filename"] = dName;
            }
            foreach (string file in files)
            {
                string fName = Path.GetFileName(file);
                byte[] content = File.ReadAllBytes(file);
                if (!Directory.Exists(toDir)) Directory.CreateDirectory(toDir);
                string fname = Regex.Replace(toDir, @"[\\/]$", "") + Path.DirectorySeparatorChar + Path.GetFileName(file);
                Console.WriteLine("copy " +  fname);
                File.WriteAllBytes(fname, content);
            }
        }
        static void DeleteDirectories(Dictionary<string, string> dirs)
        {
            Dictionary<string, string> delkeys = new Dictionary<string, string>();
            while (true)
            {
                int cnt = dirs.Count;
                foreach (string dir in dirs.Keys)
                {
                    bool direxists = false;
                    if (Directory.Exists(dir))
                    {
                        string[] darr = Directory.GetDirectories(dir);
                        foreach (string d in darr)
                            if (d != "." && d != "..")
                            {
                                direxists = true;
                                break;
                            }
                    }
                    Console.Write("directory \"" + dir + "\"");
                    if (Directory.Exists(dir) && Directory.GetFiles(dir).Length == 0 && !direxists)
                    {
                        Directory.Delete(dir, true);
                        Console.WriteLine(" - deleted");
                        if (!delkeys.ContainsKey(dir)) delkeys.Add(dir, null);
                    }
                    else if (!Directory.Exists(dir)) Console.WriteLine(" - not exists");
                    else if (Directory.GetFiles(dir).Length != 0 || direxists) Console.WriteLine(" - not empty");
                }
                foreach (string delkey in delkeys.Keys) dirs.Remove(delkey);
                if (dirs.Count == cnt) return;
            }
        }
        static void DeleteFiles(string updpath, string s)
        {
            Dictionary<string, string> dirs = new Dictionary<string, string>();
            string[] files = s.Split('\n');
            foreach (string fpath in files)
            {
                //string regmask = @"^\.[\\/]";
                string _fpath = Regex.Replace(fpath, @"^\.[\\/]", "");
                _fpath = Regex.Replace(_fpath, @"^[\\/]", "");
                _fpath = Regex.Replace(updpath, @"[\\/]$", "") + Path.DirectorySeparatorChar + _fpath;
                //
                string dir = Path.GetDirectoryName(_fpath);
                if (!dirs.ContainsKey(dir)) dirs.Add(dir, null);
                Console.Write("deleting " + _fpath/* + "(" + regmask + ")"*/);
                if (File.Exists(_fpath))
                {
                    File.Delete(_fpath);
                    Console.WriteLine(" - deleted");
                }
                else Console.WriteLine(" - not exists");
            }
            DeleteDirectories(dirs);
            //foreach (string dir in dirs.Keys) if (Directory.Exists(dir) && Directory.GetFiles(dir).Length == 0) Directory.Delete(dir, true);
        }
        static void Main(string[] args)
        {
            if (args.Length < 2) throw new Exception("parameter missed :\nparam1=executable path\nparam2=updpath");
            string exePath = args[0];
            string updPath = args[1];
            Console.WriteLine("exePath=" + exePath + ", updPath=" + updPath);
            Console.WriteLine("Get processes...");
            //
            Process[] procs = Process.GetProcesses();
            foreach (Process process in procs)
            {
                ProcessModule m = null;
                try
                {
                    m = process.MainModule;
                }
                catch { }
                if (m != null && string.Compare(m.FileName, exePath, StringComparison.InvariantCultureIgnoreCase) == 0)
                {
                    Console.WriteLine("Kill " + m.FileName);
                    process.Kill();
                    break;
                }
            }
            Console.WriteLine("Ok - Wait a second...");
            System.Threading.Thread.Sleep(1000);
            //
            Console.WriteLine("Delete unused files and directories...");
            string locks = Regex.Replace(updPath, @"[\\/]$", "") + Path.DirectorySeparatorChar + "~files4del.lock";
            if (File.Exists(locks))
            {
                string s = File.ReadAllText(locks);
                DeleteFiles(updPath, s);
                File.Delete(locks);
            }
            //
            Console.WriteLine("Ok");
            Console.WriteLine("Copy files...");
            locks = Regex.Replace(updPath, @"[\\/]$", "") + Path.DirectorySeparatorChar + "~locks" + Path.DirectorySeparatorChar;
            if (Directory.Exists(locks))
            {
                CopyFiles(locks, updPath);
                DirectoryInfo di = new DirectoryInfo(locks);
                di.Delete(true);
            }
            Console.WriteLine("Ok");
            //
            Console.WriteLine("Press Enter to continue!");
            Console.ReadLine();
            Process.Start(exePath);
        }
    }
}
