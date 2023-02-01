using System;
using System.Data.SqlTypes;
using System.IO;
using System.Runtime.InteropServices.ComTypes;
using System.Xml;
using NewTeletekSW.Utils;
using Newtonsoft.Json.Linq;
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
            if (!args.Contains("-f") && !args.Contains("-d"))
            {
                Console.WriteLine("Parameter -f or -d is required");
                return;
            }
            else
            {
                if (args.Contains("-f")) 
                { 
                    pathIndex = Array.IndexOf(args, "-f") + 1;
                } else 
                {
                    pathIndex = Array.IndexOf(args, "-d") + 1;
                };
                if (string.IsNullOrEmpty(args[pathIndex]) || args[pathIndex].StartsWith("-"))
                {
                    Console.WriteLine(args.Contains("-f") ? "Filename is required right after the parameter -f" : "Folder name is required right after the parameter -d");
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
            #region getFiles
            var fileOrDirectory = args[pathIndex];
            List<string> fileArgs = new List<string>();
            if (fileOrDirectory.Contains(","))
            {
                fileArgs = fileOrDirectory.Split(",").Select(f => f.Trim()).ToList();
            } else if (args.Contains("-f")) 
            {
                fileArgs.Append<string>(fileOrDirectory);                
            } else
            {
                string[] directoriesAtLevel1 = Directory.GetDirectories(fileOrDirectory); // Configuration
                foreach (string directory in directoriesAtLevel1)
                {
                    if (!directory.Contains(" - No")) 
                    {
                        FnDProcessors.ProcessDirectory(directory, fileArgs);
                    }
                }
            }
            #endregion

            switch (args[operationIndex])
            {
                case "r":
                case "read":
                    // open XML
                    Converter.ReadXML(fileArgs[0]);
                    break;
                case "w":
                case "write":
                    // write XML
                    //Converter.WriteXML(file);
                    break;
                case "log":
                    // write XML
                    break;
                case "time":
                    // write XML
                    break;
                case "t":
                case "transform":
                    JObject json = new JObject();

                    var mergeSettings = new JsonMergeSettings
                    {
                        MergeArrayHandling = MergeArrayHandling.Merge
                    };

                    if (fileArgs.Count > 0) 
                    {
                        foreach (string fileName in fileArgs)
                        {
                            string purFileName = (fileName.Split("."))[0];
                            string[] arr = purFileName.Split("_");
                            string key = "en";
                            if (arr.GetType() == typeof(string[]) && arr[arr.Length - 1].Length == 2)
                            {
                                key= arr[arr.Length - 1];
                            }

                            json.Merge(Converter.WriteXML(fileName, json, key), mergeSettings);
                        }
                        if (json.ToString() != null)
                        {
                            Console.WriteLine(json.ToString());
                        }
                    }
                    break;
                case "tr":
                case "translate":
                    json = new JObject();

                    mergeSettings = new JsonMergeSettings
                    {
                        MergeArrayHandling = MergeArrayHandling.Merge
                    };

                    if (fileArgs.Count > 0)
                    {
                        foreach (string fileName in fileArgs)
                        {
                            string purFileName = (fileName.Split("."))[0];
                            string[] arr = purFileName.Split("_");
                            string key = "en";
                            if (arr.GetType() == typeof(string[]) && arr[arr.Length - 1].Length == 2)
                            {
                                key = arr[arr.Length - 1];
                            }

                            json.Merge(Converter.TranslateXML(fileName, json, key), mergeSettings);
                        }
                        if (json.ToString() != null)
                        {
                            Console.WriteLine(json.ToString());
                        }
                    }
                    break;
                default:
                    // using LNGID
                    json = new JObject();

                    mergeSettings = new JsonMergeSettings
                    {
                        MergeArrayHandling = MergeArrayHandling.Merge
                    };

                    if (fileArgs.Count > 0)
                    {
                        foreach (string fileName in fileArgs)
                        {
                            string purFileName = (fileName.Split("."))[0];
                            string[] arr = purFileName.Split("_");
                            string key = "en";
                            if (arr.GetType() == typeof(string[]) && arr[arr.Length - 1].Length == 2)
                            {
                                key = arr[arr.Length - 1];
                            }

                            json.Merge(Converter.ToXML(fileName, json, key), mergeSettings);
                        }
                        if (json.ToString() != null)
                        {
                            Console.WriteLine(json.ToString());
                        }
                    }
                    break;
            }
        }
    }
}