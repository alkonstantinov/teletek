using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using DocumentFormat.OpenXml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using DocumentFormat.OpenXml.Wordprocessing;

namespace NewTeletekSW.Utils
{
    public static class JsonExporter
    {
        public static void ReadExcelFileLanguages(string fileName)
        {
            using SpreadsheetDocument spreadsheetDocument = SpreadsheetDocument.Open(fileName, false);
            WorkbookPart workbookPart = spreadsheetDocument.WorkbookPart!;
            WorksheetPart worksheetPart = workbookPart.WorksheetParts.First();
            OpenXmlReader reader = OpenXmlReader.Create(worksheetPart);
            string text;
            while (reader.Read())
            {
                if (reader.ElementType == typeof(CellValue))
                {
                    text = reader.GetText();
                    Console.Write(text + " ");
                }
            }
            Console.WriteLine();
            Console.ReadKey();
        }

        internal static string GetValueFromCell(Cell c, WorkbookPart wbPart)
        {
            string value = c.InnerText;
            if (c.DataType != null)
            {
                var stringTable = wbPart.GetPartsOfType<SharedStringTablePart>().FirstOrDefault();
                if (stringTable != null)
                {
                    value =
                        stringTable.SharedStringTable
                        .ElementAt(int.Parse(value)).InnerText;
                }
            }
            return value.Trim();
        }

        // The non-SAX approach.
        internal static JObject ReadExcelFileTranslations(WorksheetPart worksheetPart, WorkbookPart wbPart)
        {
            JObject result = new();
            // sheetData (cell table grouped by row) contains row, c (cell in row), v (value in cell)
            SheetData sheetData = worksheetPart.Worksheet.Elements<SheetData>().First();
            string text;

            // deal with header -> languages
            List<string> languages = new();

            // deal with translation per lang cells
            foreach (Row r in sheetData.Elements<Row>())
            {
                string key = GetValueFromCell(r.Elements<Cell>().First(), wbPart);
                JObject internalObj = new();
                int index = 0;
                foreach (Cell c in r.Elements<Cell>())
                {
                    text = GetValueFromCell(c, wbPart);                    
                    if (text == key) { continue; }
                    if (key == "KEY") languages.Add(text);
                    else {
                        if (index < languages.Count)
                        {
                            internalObj.Add(languages[index], text);
                            index++;
                        } else { continue; }
                    }
                }
                if (key != "KEY")
                    result.Add(key, internalObj);
            }
            
            return result;
        }
    }
}
