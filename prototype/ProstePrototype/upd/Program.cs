using System;
using System.Text.RegularExpressions;
using common;
using lupd;

namespace upd
{
    internal class Program
    {
        private static void crcprocess(string filename)
        {
            Console.WriteLine(filename);
        }
        private static void http_progress(string filename, int counter, int cntall)
        {
            Console.WriteLine(filename + "  /" + counter.ToString() + " of " + cntall.ToString());
        }
        static void Main(string[] args)
        {
            string path = @".\app";
            if (args.Length > 0) path = args[0];
            if (!Regex.IsMatch(path, @"[\\/]$")) path += "\\";
            string err = "";
            eUPDResult res = cUpd.DoUpdate(path, crcprocess, http_progress, ref err);
            if (res != eUPDResult.Ok) Console.WriteLine(err);
            else Console.WriteLine("Ok");
        }
    }
}
