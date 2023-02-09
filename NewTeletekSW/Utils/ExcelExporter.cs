using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewTeletekSW.Utils
{
    public class ExcelExporter
    {
        private readonly IEnumerable<IEnumerable<string>> _rows;
        private readonly IEnumerable<string> _header;
        private readonly string _title;
        public ExcelExporter(IEnumerable<IEnumerable<string>> rows, IEnumerable<string> header, string title)
        {
            _rows = rows;
            _header = header;
            _title = title;
        }
        public byte[] WriteFile(JObject jsonData)
        {
            // Create a new XLSX file
            using (var spreadsheet = SpreadsheetDocument.Create("data.xlsx", SpreadsheetDocumentType.Workbook))
            {
                // Add a worksheet to the document
                var worksheetPart = spreadsheet.AddWorkbookPart().WorksheetParts.First(); // .AddWorksheetPart();
                worksheetPart.Worksheet = new Worksheet();

                // Create a header row
                var headerRow = new Row();
                headerRow.Append(new Cell
                {
                    DataType = CellValues.String,
                    CellValue = new CellValue("Property")
                });
                headerRow.Append(new Cell
                {
                    DataType = CellValues.String,
                    CellValue = new CellValue("Value")
                });

                // Add the header row to the worksheet
                worksheetPart.Worksheet.AppendChild(headerRow);

                // Loop over the properties
                var propertyNames = jsonData.Keys.OrderBy(k => k);
                foreach (var propertyName in propertyNames)
                {
                    var property = jsonData[propertyName];
                    var subPropertyNames = property.Keys.OrderBy(k => k);

                    // Loop over the sub-properties
                    foreach (var subPropertyName in subPropertyNames)
                    {
                        // Add a row for the sub-property
                        var subPropertyRow = new Row();
                        subPropertyRow.Append(new Cell
                        {
                            DataType = CellValues.String,
                            CellValue = new CellValue(propertyName + " - " + subPropertyName)
                        });
                        subPropertyRow.Append(new Cell
                        {
                            DataType = CellValues.String,
                            CellValue = new CellValue(property[subPropertyName].ToString())
                        });

                        // Add the subPropertyRow to the worksheet
                        worksheetPart.Worksheet.AppendChild(subPropertyRow);
                    }
                }

                // Save the changes to the worksheet
                worksheetPart.Worksheet.Save();

                // Close the document
                spreadsheet.Close();
            }
        }
    }
}


