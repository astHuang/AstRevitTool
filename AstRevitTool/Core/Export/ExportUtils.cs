using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using Autodesk.Revit.DB.Architecture;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows.Forms;
using System.IO;
using System.Linq;
using System.Text;
using AstRevitTool.Core.Analysis;
using AstRevitTool.Views;
using BoundarySegment = Autodesk.Revit.DB.BoundarySegment;
using Microsoft.Office.Interop.Excel;
using OfficeOpenXml;
using OfficeOpenXml.Style;

namespace AstRevitTool.Core.Export
{
    class ASTExportUtils
    {
        //file path generator 
        private const int _target_square_size = 100;
        public static string filepath(Document maindoc, IAnalysis analysis, string format)
        {
            string time = DateTime.Now.ToString(@"MM\/dd\/yyyy h\:mm tt");
            string defaultName = Path.ChangeExtension(maindoc.Title + "_"+ analysis.Type(), format);
            string defaultFolder = System.Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            return Path.Combine(defaultFolder, defaultName);
        }

        public static string filename(Document maindoc, IAnalysis analysis)
        {
            return maindoc.Title + "_"+ analysis.Type();
        }

        /// <summary>
        /// Export all visible element in current 3D view. Output an instance of context which includes visibility boolean information for all elements.
        /// </summary>
        public static void ExportView3D(Document maindoc, Autodesk.Revit.DB.View view, UIApplication uiapp, out ElementsVisibleInViewExportContext context_)
        {
            ElementsVisibleInViewExportContext context = new ElementsVisibleInViewExportContext(maindoc);
            //context.UIApp= uiapp;
            CustomExporter exporter = new CustomExporter(maindoc, context);
            exporter.Export(view);
            context_ = context;
        }

        public static void ExportView3D(Document maindoc, Autodesk.Revit.DB.View view, out ElementsVisibleInViewExportContext context_)
        {
            ElementsVisibleInViewExportContext context = new ElementsVisibleInViewExportContext(maindoc);
            CustomExporter exporter = new CustomExporter(maindoc, context);
            exporter.Export(view);
            context_ = context;
        }
        public static void WriteExcelFromModel(string fileName, bool exportRvtData, ViewModelByCategory model)
        {
            if(Directory.Exists(fileName))
            {
                Directory.Delete(fileName, false) ;
            }
            using (ExcelPackage excelPackage = new ExcelPackage(new FileInfo(fileName)))
            {
                ExcelWorksheet worksheet1 = null;
                ExcelWorksheet worksheet2 = null;
                ExcelWorksheet worksheet3 = null;
                if (excelPackage.Workbook.Worksheets["DataByCategory"] != null)
                {
                    worksheet1 = excelPackage.Workbook.Worksheets["DataByCategory"];
                    worksheet1.Cells.Clear();
                }
                else
                {
                    worksheet1 = excelPackage.Workbook.Worksheets.Add("DataByCategory");
                }

                if (excelPackage.Workbook.Worksheets["DataByMaterial"] != null)
                {
                    worksheet2 = excelPackage.Workbook.Worksheets["DataByMaterial"];
                    worksheet2.Cells.Clear();
                }
                else
                {
                    worksheet2 = excelPackage.Workbook.Worksheets.Add("DataByMaterial");
                }

                if (excelPackage.Workbook.Worksheets["DataByTransparency"] != null)
                {
                    worksheet3 = excelPackage.Workbook.Worksheets["DataByTransparency"];
                    worksheet3.Cells.Clear();
                }
                else
                {
                    worksheet3 = excelPackage.Workbook.Worksheets.Add("DataByTransparency");
                }

                List<ExcelWorksheet> ws = new List<ExcelWorksheet>();
                ws.Add(worksheet1);
                ws.Add(worksheet2);
                ws.Add(worksheet3);
                foreach(ExcelWorksheet worksheet in ws)
                {
                    worksheet.Cells[1, 1].Value ="Name";
                    worksheet.Cells[1, 2].Value = "Area";
                    worksheet.Cells[1, 3].Value = "Notes/TypeMark";
                    if(exportRvtData)
                    {
                        worksheet.Cells[1, 4].Value = "RvtColor";
                        worksheet.Cells[1, 5].Value = "RvtElementId";
                        worksheet.Cells[1, 6].Value = "RvtDocument";
                    }
                }

                
                List<ICollection<SourceDataTypes>> listData = new List<ICollection<SourceDataTypes>>();
                listData.Add(model.DataDetails);
                listData.Add(model.DataByMaterial);
                listData.Add(model.DataByTransparency);
                for(int i=0; i<listData.Count;i++)
                {
                    int rowIndex = 2;
                    var dataList = listData[i];
                    var worksheet = ws[i];
                    foreach(var data in dataList)
                    {
                        rowIndex++;
                        int level = 0;
                        worksheet.Row(rowIndex).OutlineLevel= level;
                        worksheet.Cells[rowIndex, 1].Value = data.Name;
                        worksheet.Cells[rowIndex, 2].Value = data.Area;
                        worksheet.Cells[rowIndex, 3].Value = data.Notes;
                        if (exportRvtData)
                        {
                            worksheet.Cells[rowIndex, 4].Value = data.Color.ToString();
                            if (data.Rvt_ptr != null)
                            {
                                worksheet.Cells[rowIndex, 5].Value = data.Rvt_ptr.Id;
                                worksheet.Cells[rowIndex, 6].Value = data.Rvt_ptr.Document.Title;
                            }
                        }
                        WriteNodeData(data, exportRvtData,worksheet,level,ref rowIndex);
                    }
                }

                excelPackage.Save();
                excelPackage.Dispose();
            }

            MessageBox.Show("Excel file created!");
        }
        private static void WriteNodeData(SourceDataTypes dataNode, bool exportRvt, ExcelWorksheet worksheet, int level,ref int rowIndex)
        {
            if (dataNode.Children==null || dataNode.Children.Count==0)
            {
                return;
            }
            
            foreach (var childNode in dataNode.Children)
            {
                rowIndex++;
                worksheet.Row(rowIndex).OutlineLevel = level+1;
                SetBackgroundColor(worksheet.Row(rowIndex), level + 1);
                worksheet.Cells[rowIndex, 1].Value = childNode.Name;
                worksheet.Cells[rowIndex, 2].Value = childNode.Area;
                worksheet.Cells[rowIndex, 3].Value = childNode.Notes;
                if (exportRvt)
                {
                    worksheet.Cells[rowIndex, 4].Value = childNode.Color.ToString();
                    if (childNode.Rvt_ptr != null)
                    {
                        worksheet.Cells[rowIndex, 5].Value = childNode.Rvt_ptr.Id;
                        worksheet.Cells[rowIndex, 6].Value = childNode.Rvt_ptr.Document.Title;
                    }
                }
                WriteNodeData(childNode, exportRvt,worksheet, level+1,ref rowIndex);
            }
        }

        private static void SetBackgroundColor(ExcelRow cell, int level)
        {
            switch (level)
            {
                case 1:
                    cell.Style.Fill.PatternType = ExcelFillStyle.Solid;
                    cell.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightYellow);
                    break;
                case 2:
                    cell.Style.Fill.PatternType = ExcelFillStyle.Solid;
                    cell.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGreen);
                    break;
                case 3:
                    cell.Style.Fill.PatternType = ExcelFillStyle.Solid;
                    cell.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightSkyBlue);
                    break;
                case 4:
                    cell.Style.Fill.PatternType = ExcelFillStyle.Solid;
                    cell.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.Azure);
                    break;
                default:
                    break;
            }
        }
        /// <summary>
        /// General txt export using a streamwriter
        /// </summary>
        public static void txtExport(TextWriter writer, IAnalysis analysis)
        {
            if (analysis.ResultList().Count == 0)
                return;

            string time = DateTime.Now.ToString(@"MM\/dd\/yyyy h\:mm tt");
            string title = "Arrowstreet Revit Project Report " + time + " ";
            writer.WriteLine(title);

            string str = "";
            str += analysis.Type();
            str += "\n---Report Details--- ";
            str += analysis.Report();
            writer.Write(str);
        }


        public static void csvExport(TextWriter writer, IAnalysis analysis)
        {
            if (analysis.ResultList().Count == 0)
                return;
            String legendLine = "Area(sq ft)";
            writer.WriteLine();
            writer.WriteLine(String.Format("Detailed results for {0},{1}", analysis.Type(), legendLine));

            foreach(KeyValuePair<string,double> entry in analysis.ResultList())
            {
                string name = entry.Key;
                string number = entry.Value.ToString("0.##");
                writer.WriteLine(String.Format("{0},{1}",
                    name.Replace(',',':'),number));
            }
            if(analysis.Conclusion() != "")
            {
                writer.Write(analysis.Conclusion().Replace(':',','));
            }
            
        }

        private void ReportResultsAssembly(Document maindoc,Assembly_Analysis analysis, TextWriter writer)
        {
            foreach (string typename in analysis.Quantities.Keys)
            {
                double quantity = analysis.Quantities[typename].Item2;
                string typemark = analysis.Quantities[typename].Item1;

                //writer.WriteLine(String.Format("   {0} Net: [{1:F2} cubic ft {2:F2} sq. ft]  Gross: [{3:F2} cubic ft {4:F2} sq. ft]", material.Name, quantity.NetVolume, quantity.NetArea, quantity.GrossVolume, quantity.GrossArea));
                writer.WriteLine(String.Format("{0},{1:F2},{2:F2}",
                    typename.Replace(',', ':'),  // Element names may have ',' in them
                    typemark, quantity));
            }
        }

    }
}
