using System.Text.RegularExpressions;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using NewTeletekSW.Utils;
using Newtonsoft.Json.Linq;

namespace XMLDocument
{
    partial class Program
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
            List<string> fileArgs = new();
            if (fileOrDirectory.Contains(','))
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
                    JObject json = new();

                    var mergeSettings = new JsonMergeSettings
                    {
                        MergeArrayHandling = MergeArrayHandling.Merge
                    };

                    if (fileArgs.Count > 0)
                    {
                        foreach (string fileName in fileArgs)
                        {
                            string purFileName = fileName.Split(".")[0];
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
                            string purFileName = fileName.Split(".")[0];
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
                    // -p toXLS'
                    // '-f "C:\Users\vbb12\GitHub\Teletek\teletek\prototype\ProstePrototype\ProstePrototype\html\imports\translations.js"
                    string jsonString = RemoveConstRegex().Replace(File.ReadAllText(fileArgs[0]), ""); // fileArgs[0] should be a JSON file
                    JObject jsonData = JObject.Parse(jsonString);

                    string intialLang = "";
                    if (jsonData.ContainsKey("initial") && jsonData["initial"] != null) 
                    {
                        intialLang = jsonData["initial"]!.ToString();
                    }
                    JObject? translationJson = jsonData["translations"] as JObject;
                    JArray? allLanguages = jsonData["languages"] as JArray;
                    // creating the header based on all languges found
                    List<string> h = new();
                    if (allLanguages != null)
                        h = allLanguages.Select(x => (JObject)x).Select(y => y["id"]!.ToString()).ToList();
                    List<string> propKeys = new();
                    if (translationJson != null)
                        propKeys = translationJson.Properties().Select(k => k.Name).ToList();

                    string? destinationPath = Path.GetDirectoryName(fileOrDirectory);
                    // create the file
                    new ExcelExporter(
                        propKeys, 
                        h, 
                        destinationPath + Path.DirectorySeparatorChar + "translations.xlsx" // @"C:\Users\vbb12\GitHub\Teletek\teletek\prototype\ProstePrototype\ProstePrototype\html\imports\translations.xlsx"
                    ).WriteFile(
                        intialLang.ToString(),
                        translationJson ?? new JObject(),
                        allLanguages ?? new JArray()
                    );
                    break;
                case "fromXLS":
                    // read .xlsx file and transform it to json
                    // Command line:
                    // -p fromXLS'
                    // '-f "C:\Users\vbb12\GitHub\Teletek\teletek\prototype\ProstePrototype\ProstePrototype\html\imports\translations.xlsx"

                    JObject translationObj = new();
                    using (SpreadsheetDocument document = SpreadsheetDocument.Open(fileArgs[0], false)) // open the document (false for read-only)
                    {
                        WorkbookPart wbPart = document.WorkbookPart!; // each spreadsheet has a workbook part, that contains sheets

                        // sheets contains list of sheet and each sheet has its own sheet definition (worksheet) which contains sheetData
                        List<string?> sheetsNames = wbPart.Workbook.Sheets!.Elements<Sheet>().Select(x => (string?)x.Name).ToList();
                        
                        for (int i = 0; i < sheetsNames.Count; i++)
                        {
                            string? sheetName = sheetsNames[i];
                            // get the sheet object based on the sheet names
                            Sheet? theSheet = wbPart.Workbook.Descendants<Sheet>().Where(s => s.Name == sheetName).FirstOrDefault() ?? throw new ArgumentException("sheetName");
                            // find the worksheet part based on the sheet id
                            WorksheetPart wsPart = (WorksheetPart)wbPart.GetPartById(theSheet.Id!); 
                            JObject jsonEl = JsonExporter.ReadExcelFileTranslations(wsPart, wbPart);
                            if (sheetName == "languages")
                            {
                                JArray lang = new();
                                foreach (var l in ((JObject)jsonEl["id"]!).Properties().Select(x => x.Name))
                                {
                                    JObject o = new();
                                    foreach (string s in jsonEl.Properties().Select(p => p.Name))
                                    {
                                        o.Add(s, jsonEl[s]![l]);
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
                            Console.WriteLine(dirNotFoundException.Message.ToString());
                        }
                        catch (UnauthorizedAccessException unauthorizedAccessException)
                        {
                            // Show a message to the user
                            Console.WriteLine(unauthorizedAccessException.Message.ToString());
                        }
                        catch (IOException ioException)
                        {
                            //logger.Error(ioException, "Error during file write");
                            // Show a message to the user
                            Console.WriteLine(ioException.Message.ToString());
                        }
                        catch (Exception exception)
                        {
                            //logger.Fatal(exception, "We need to handle this better");
                            // Show general message to the user
                            Console.WriteLine(exception.Message.ToString());
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
                            string purFileName = fileName.Split(".")[0];
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

        [GeneratedRegex("^const Translations = ", RegexOptions.IgnoreCase, "bg-BG")]
        private static partial Regex RemoveConstRegex();
    }
}