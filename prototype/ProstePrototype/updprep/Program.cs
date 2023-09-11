using System;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using common;
using lupd;

namespace updprep
{
    internal class Program
    {
        private static void crcprocess(string filename)
        {
            Console.WriteLine(filename);
        }
        static void Main(string[] args)
        {
            string path = null;
            if (args.Length > 0) { path = args[0]; if (!Regex.IsMatch(path, @"[\\/]$")) path += "\\"; }
            if (System.IO.File.Exists(path + settings.updmapfilename)) System.IO.File.Delete(path + settings.updmapfilename);
            string upd = cUpd.FilesMapStr(crcprocess, path/*"c:\\work\\tmp\\1\\"*/);
            System.IO.File.WriteAllText(path + settings.updmapfilename, upd.ToString());
            //string f4download = cUpd.files4download(upd, "c:\\work\\tmp\\2\\");
        }
    }
}
