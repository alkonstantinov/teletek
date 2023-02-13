using System;
using System.Data.SqlTypes;
using System.IO;
using System.Runtime.InteropServices.ComTypes;
using System.Text.RegularExpressions;
using System.Web;
using System.Xml;
using DocumentFormat.OpenXml.Bibliography;
using DocumentFormat.OpenXml.Drawing;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using NewTeletekSW.Utils;
using Newtonsoft.Json;
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
                }
                else
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
            }
            else if (args.Contains("-f"))
            {
                fileArgs.Add(fileOrDirectory);
            }
            else
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
                    // open XML and read it as json
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
                case "transform": // read xml and transform it to json based on @ID
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
                                key = arr[arr.Length - 1];
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
                case "toXLS":
                    // read json file and transform it to Excel
                    // Command line:
                    // '-f "C:\Users\vbb12\GitHub\Teletek\teletek\prototype\ProstePrototype\ProstePrototype\html\imports\translations.js"
                    // -p toXLS'
                    var jsonString = Regex.Replace(File.ReadAllText(fileArgs[0]), @"^const Translations = ", "", RegexOptions.IgnoreCase); // fileArgs[0] should be a JSON file
                    var jsonData = JObject.Parse(jsonString);

                    string intialLang = (string)jsonData["initial"];
                    JObject translationJson = (JObject)jsonData["translations"];
                    JArray allLanguages = (JArray)jsonData["languages"];
                    // creating the header based on all languges found
                    List<string> h = new List<string>();
                    h = allLanguages.Select(x => (JObject)x).Select(y => y["id"].ToString()).ToList();
                    List<string> propKeys = new List<string>();
                    propKeys = translationJson.Properties().Select(k => k.Name).ToList();

                    // create the file
                    new ExcelExporter(propKeys, h, @"C:\Users\vbb12\GitHub\Teletek\teletek\prototype\ProstePrototype\ProstePrototype\html\imports\translations.xlsx").WriteFile(intialLang.ToString(), translationJson, allLanguages);
                    break;
                case "fromXLS":
                    // read .xlsx file and transform it to json
                    // Command line:
                    // '-f "C:\Users\vbb12\GitHub\Teletek\teletek\prototype\ProstePrototype\ProstePrototype\html\imports\translations.xlsx"
                    // -p fromXLS'

                    JObject translationObj = new JObject();
                    using (SpreadsheetDocument document = SpreadsheetDocument.Open(fileArgs[0], false)) // open the document (false for read-only)
                    {
                        WorkbookPart wbPart = document.WorkbookPart; // each spreadsheet has a workbook part, that contains sheets

                        // sheets contains list of sheet and each sheet has its own sheet definition (worksheet) which contains sheetData
                        List<string> sheetsNames = wbPart.Workbook.Sheets.Elements<Sheet>().Select(x => (string)x.Name).ToList();
                        
                        for (int i = 0; i < sheetsNames.Count(); i++)
                        {
                            string sheetName = sheetsNames[i];
                            // get the sheet object based on the sheet names
                            Sheet theSheet = wbPart.Workbook.Descendants<Sheet>().Where(s => s.Name == sheetName).FirstOrDefault(); 
                            // Throw an exception if there is no sheet.
                            if (theSheet == null)
                            {
                                throw new ArgumentException("sheetName");
                            }
                            // find the worksheet part based on the sheet id
                            WorksheetPart wsPart = (WorksheetPart)(wbPart.GetPartById(theSheet.Id)); 
                            JObject jsonEl = (JObject)(JsonExporter.ReadExcelFileTranslations(wsPart, wbPart));
                            if (sheetName == "languages")
                            {
                                JArray lang = new JArray();
                                foreach (var l in ((JObject)jsonEl["id"]).Properties().Select(x => x.Name))
                                {
                                    JObject o = new JObject();
                                    foreach (string s in jsonEl.Properties().Select(p => p.Name))
                                    {
                                        o.Add(s, jsonEl[s][l]);
                                    }
                                    lang.Add(o);
                                }
                                translationObj.Add(sheetName, lang);
                            }
                            else if (sheetName == "initial")
                            {
                                translationObj.Add(sheetName, jsonEl.Properties().First().Name);
                            }
                            else if (sheetName != null)
                            {
                                translationObj.Add(sheetName, jsonEl);
                            }
                        }

                        string js = translationObj.ToString(); 
                        try
                        {
                            File.WriteAllText(@"C:\Users\vbb12\GitHub\Teletek\teletek\prototype\ProstePrototype\ProstePrototype\html\imports\output.js", string.Format("const Translations={0};", js));
                        }
                        catch (DirectoryNotFoundException dirNotFoundException)
                        {
                            // Create and try again
                        }
                        catch (UnauthorizedAccessException unauthorizedAccessException)
                        {
                            // Show a message to the user
                        }
                        catch (IOException ioException)
                        {
                            //logger.Error(ioException, "Error during file write");
                            // Show a message to the user
                        }
                        catch (Exception exception)
                        {
                            //logger.Fatal(exception, "We need to handle this better");
                            // Show general message to the user
                        }
                    }
                    break;
                default:
                    // read xml and translate it to JSON using LNGID
                    // Command line:
                    // '-d "C:\Users\vbb12\GitHub\Teletek\Programming Software Teletek Electronics\Configuration"
                    // -p dowhatever'
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