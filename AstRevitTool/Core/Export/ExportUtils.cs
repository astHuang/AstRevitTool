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
using BoundarySegment = Autodesk.Revit.DB.BoundarySegment;


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
            context.UIApp= uiapp;
            CustomExporter exporter = new CustomExporter(maindoc, context);
            exporter.Export(view);
            context_ = context;
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
