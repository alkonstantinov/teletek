using System;
using System.Data.SqlTypes;
using System.IO;
using System.Xml;
using NewTeletekSW.Utils;
using static System.Net.Mime.MediaTypeNames;

namespace XMLDocument
{
    class Program
    {
        static void Main(string[] args)
        {
            #region get argument indexes
            if (args.Length == 0)
            {
                Console.WriteLine("You must provide parameters");
                return;
            }
            int pathIndex;
            if (!args.Contains("-f"))
            {
                Console.WriteLine("Filename parameter -f is required");
                return;
            }
            else
            {
                pathIndex = Array.IndexOf(args, "-f") + 1;
                if (string.IsNullOrEmpty(args[pathIndex]) || args[pathIndex].StartsWith("-"))
                {
                    Console.WriteLine("Filename is required right after the parameter -f");
                    return;
                }
            }
            int operationIndex;
            if (!args.Contains("-p"))
            {
                Console.WriteLine("Procedure parameter -p is required");
                return;
            }
            else
            {
                operationIndex = Array.IndexOf(args, "-p") + 1;
                if (string.IsNullOrEmpty(args[operationIndex]) || args[operationIndex].StartsWith("-"))
                {
                    Console.WriteLine("Operation name is required right after the parameter -p");
                    return;
                }
            }
            #endregion
            var file = args[pathIndex];
            switch (args[operationIndex])
            {
                case "r":
                case "read":
                    // open XML
                    Converter.ReadXML(file);
                    break;
                case "w":
                case "write":
                    // write XML
                    Converter.WriteXML(file);
                    break;
                case "log":
                    // write XML
                    break;
                case "time":
                    // write XML
                    break;
                default:
                    // do not know
                    break;
            }
        }
    }
}