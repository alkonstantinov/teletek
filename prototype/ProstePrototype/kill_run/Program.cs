using System;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using System.Threading;

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
                File.WriteAllBytes(fname, content);
            }
        }
        static void Main(string[] args)
        {
            if (args.Length < 2) throw new Exception("parameter missed :\nparam1=executable path\nparam2=updpath");
            string exePath = args[0];
            string updPath = args[1];
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
                    process.Kill();
                    Thread.Sleep(1000);
                    break;
                }
            }
            //Console.WriteLine(exePath + " and " + updPath);
            string locks = Regex.Replace(updPath, @"[\\/]$", "") + Path.DirectorySeparatorChar + "~locks" + Path.DirectorySeparatorChar;
            if (!Directory.Exists(locks)) return;

            CopyFiles(locks, updPath);

            DirectoryInfo di = new DirectoryInfo(locks);
            di.Delete(true);
            //
            Process.Start(exePath);
        }
    }
}
