using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using DocumentFormat.OpenXml.Wordprocessing;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Text = DocumentFormat.OpenXml.Spreadsheet.Text;

namespace NewTeletekSW.Utils
{
    public class ExcelExporter
    {
        private readonly IEnumerable<string> _rows;
        private readonly IEnumerable<string> _header;
        private readonly string _title;
        public ExcelExporter(IEnumerable<string> rows, IEnumerable<string> header, string title)
        {
            _rows = rows;
            _header = header;
            _title = title;
        }

        private void AddBoldToCell(Row row, string str)
        {
            var run1 = new DocumentFormat.OpenXml.Spreadsheet.Run();
            run1.Append(new Text(str));
            //create runproperties and append a "Bold" to them
            var run1Properties = new DocumentFormat.OpenXml.Spreadsheet.RunProperties();
            run1Properties.Append(new DocumentFormat.OpenXml.Spreadsheet.Bold());
            //set the first runs RunProperties to the RunProperties containing the bold
            run1.RunProperties = run1Properties;
            InlineString inlineString = new InlineString();
            inlineString.Append(run1);
            var cell = new DocumentFormat.OpenXml.Spreadsheet.Cell();
            cell.DataType = CellValues.InlineString;
            cell.AppendChild(inlineString);
            row.Append(cell);
        }
        public void WriteFile(string initial, JObject jsonData, JArray langObject)
        {
            // Create a new XLSX file
            using (SpreadsheetDocument spreadsheet = SpreadsheetDocument.Create(_title, SpreadsheetDocumentType.Workbook))
            {
                // Add a WorkbookPart to the document - The root element for the main document/spreadsheet part.
                WorkbookPart workbookpart = spreadsheet.AddWorkbookPart();
                workbookpart.Workbook = new Workbook(); // workbook contains sheets

                // Add Sheets to the Workbook - The container for the block-level structures such as sheet, fileVersion, and others specified in the ISO/IEC 29500 specification.
                Sheets sheets = spreadsheet.WorkbookPart.Workbook.AppendChild<Sheets>(new Sheets());

                #region Sheet0
                // Add also WorksheetPart to the WorkbookPart. Worksheet is a sheet definition file that contains the sheet data
                WorksheetPart worksheetPart0 = workbookpart.AddNewPart<WorksheetPart>();
                SheetData data0 = new SheetData(); // create the sheet data
                Row firstRow = new Row(); // create a row
                firstRow.Append(new Cell // and append to it the first cell
                {
                    DataType = CellValues.String,
                    CellValue = new CellValue(initial)
                });
                data0.Append(firstRow);
                worksheetPart0.Worksheet = new Worksheet(data0); // assotiation of sheet data 0 to the worksheet part 0
                Sheet sheet0 = new Sheet() 
                {
                    Id = spreadsheet.WorkbookPart.GetIdOfPart(worksheetPart0), // here we say that the data appended to the worksheetPart0 will become this new sheet
                    SheetId = 1,
                    Name = "initial"
                };
                sheets.Append(sheet0); // appending the sheet to the sheets data
                #endregion

                #region Sheet1
                // Add also WorksheetPart to the WorkbookPart. Worksheet is a sheet definition file that contains the sheet data
                WorksheetPart worksheetPart1 = workbookpart.AddNewPart<WorksheetPart>();

                // Add data for sheet1 (worksheetPart1)
                SheetData data1 = new SheetData();

                #region filling translation sheet with data
                // Create a header row
                var headerRow = new Row();
                headerRow.Append(new Cell
                {
                    DataType = CellValues.String,
                    CellValue = new CellValue(" KEY "),
                    StyleIndex = Convert.ToUInt32(0)
                });
                foreach (string head in _header)
                {
                    AddBoldToCell(headerRow, head);                   
                }

                // Add the header row to the worksheet
                data1.AppendChild(headerRow);

                // Loop over the properties
                foreach (string propertyName in _rows)
                {
                    var propertyRow = new Row();
                    AddBoldToCell(propertyRow, propertyName);

                    foreach (string head in _header)
                    {
                        string? val = (string?)jsonData[propertyName][head];
                        propertyRow.Append(new Cell
                        {
                            DataType = CellValues.String,
                            CellValue = new CellValue(String.IsNullOrEmpty(val) ? "" : val)
                        });
                    }

                    // Add the propertyRow to the worksheet
                    data1.AppendChild(propertyRow);
                }
                #endregion
                
                // adding data to the worksheetPart
                worksheetPart1.Worksheet = new Worksheet(data1);

                // Append a new sheet and associate it with the workbook's sheets - A sheet that points to a sheet definition (Worksheet).
                Sheet sheet1 = new Sheet()
                {
                    Id = spreadsheet.WorkbookPart.GetIdOfPart(worksheetPart1), // here we say that the data appended to the worksheetPart1 will become this new sheet
                    SheetId = 2,
                    Name = "translations"
                };

                sheets.Append(sheet1);
                #endregion

                #region Sheet2
                // creating space (sheet definition) for sheet2
                WorksheetPart worksheetPart2 = workbookpart.AddNewPart<WorksheetPart>();

                // Add data for sheet2 
                SheetData data2 = new SheetData();

                #region filling language sheet as sheet2 with data
                // using the same headerRow for sheet 2 as well
                Row sameHeaderRow = new Row();
                sameHeaderRow.Append(new Cell
                {
                    DataType = CellValues.String,
                    CellValue = new CellValue(" KEY ")
                });
                foreach (string head in _header)
                {
                    AddBoldToCell(sameHeaderRow, head);
                }
                data2.AppendChild(sameHeaderRow);

                string[] props = ((JObject)langObject[0]).Properties().Select(x => x.Name).ToArray();
                foreach(string prop in props)
                {
                    var propRow = new Row();
                    propRow.Append(new Cell
                    {
                        DataType = CellValues.String,
                        CellValue = new CellValue(prop)
                    });
                    foreach (string lang in _header)
                    {
                        string value = (string)((JObject)langObject.First(x => (string)x["id"] == lang))[prop];
                        propRow.Append(new Cell
                        {
                            DataType = CellValues.String,
                            CellValue = new CellValue(String.IsNullOrEmpty(value) ? "" : value)
                        });
                    }
                    data2.AppendChild(propRow);
                }
                #endregion
                // adding data to the worksheetPart2
                worksheetPart2.Worksheet = new Worksheet(data2);

                Sheet sheet2 = new Sheet()
                {
                    Id = spreadsheet.WorkbookPart.GetIdOfPart(worksheetPart2), // here we say that the data appended to the worksheetPart2 will become this new sheet
                    SheetId = 3,
                    Name = "languages"
                };

                sheets.Append(sheet2);
                #endregion region

                // Save the changes to the workbook
                workbookpart.Workbook.Save();

                // Close the document
                spreadsheet.Close();
            }
        }
    }

}


