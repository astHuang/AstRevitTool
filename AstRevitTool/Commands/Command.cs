using AstRevitTool.ViewModels;
using AstRevitTool.Views;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.UI.Selection;
using System.Drawing;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows.Forms;
using System.Reflection;
using System.IO;
using AstRevitTool.Core.Analysis;
using AstRevitTool.Core.Export;
using AstRevitTool.Core;

namespace AstRevitTool.Commands
{
    [Transaction(TransactionMode.Manual)]
    public class CmdWWRCalc : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIApplication uiapp = commandData.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;

            Autodesk.Revit.ApplicationServices.Application app = uiapp.Application;
            Document doc = uidoc.Document;
            Result rc;
            try
            {
                ElementsVisibleInViewExportContext context = new ElementsVisibleInViewExportContext(doc);
                if (doc.ActiveView as View3D != null)
                    Core.Export.ASTExportUtils.ExportView3D(doc, doc.ActiveView as View3D, uiapp, out context);
                else
                    MessageBox.Show("You must be in 3D view to export.");

                WWR_Analysis wwr = new WWR_Analysis(context, app);
                wwr.Analyze();
                string report = "";
                ASTExportUtils.txtExport(doc, wwr, out report);

                Views.Form1 form = new Views.Form1(wwr, doc);

                form.ShowDialog();
                rc = Result.Succeeded;
            }
            catch (Exception ex)
            {
                TaskDialog.Show("Calculation Failed!" + "\r\n" + ex.Message, ex.Message + "\r\n" + ex.StackTrace);
                rc = Result.Failed;
            }

            return rc;
        }
    }




    [Transaction(TransactionMode.Manual)]
    public class CmdWallMat : IExternalCommand
    {
        public Result Execute(
          ExternalCommandData commandData,
          ref string message,
          ElementSet elements)
        {
            UIApplication uiapp = commandData.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;

            Autodesk.Revit.ApplicationServices.Application app = uiapp.Application;
            Document doc = uidoc.Document;
            Result rc;
            try
            {
                ElementsVisibleInViewExportContext context = new ElementsVisibleInViewExportContext(doc);
                if (doc.ActiveView as View3D != null)
                    Core.Export.ASTExportUtils.ExportView3D(doc, doc.ActiveView as View3D, uiapp, out context);
                else
                    MessageBox.Show("You must be in 3D view to export.");

                Material_Analysis mat = new Material_Analysis(context, app);
                mat.Analyze();
                string report = "";
                ASTExportUtils.txtExport(doc, mat, out report);

                //MessageBox.Show(report);
                Views.Form1 form = new Views.Form1(mat, doc);
                form.ShowDialog();
                rc = Result.Succeeded;
            }
            catch (Exception ex)
            {
                TaskDialog.Show("Calculation Failed!" + "\r\n" + ex.Message, ex.Message + "\r\n" + ex.StackTrace);
                rc = Result.Failed;
            }

            return rc;
        }

    }




    [Transaction(TransactionMode.Manual)]
    public class CmdWallType : IExternalCommand
    {
        public Result Execute(
          ExternalCommandData commandData,
          ref string message,
          ElementSet elements)
        {
            UIApplication uiapp = commandData.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;

            Autodesk.Revit.ApplicationServices.Application app = uiapp.Application;
            Document doc = uidoc.Document;
            Result rc;
            try
            {
                ElementsVisibleInViewExportContext context = new ElementsVisibleInViewExportContext(doc);
                if (doc.ActiveView as View3D != null)
                    Core.Export.ASTExportUtils.ExportView3D(doc, doc.ActiveView as View3D, uiapp, out context);
                else
                    MessageBox.Show("You must be in 3D view to export.");

                Material_Analysis mat = new Material_Analysis(context, app);
                mat.byType = true;
                mat.Analyze();
                string report = "";
                ASTExportUtils.txtExport(doc, mat, out report);

                //MessageBox.Show(report);
                Views.Form1 form = new Views.Form1(mat, doc);
                form.ShowDialog();
                rc = Result.Succeeded;
            }
            catch (Exception ex)
            {
                TaskDialog.Show("Calculation Failed!" + "\r\n" + ex.Message, ex.Message + "\r\n" + ex.StackTrace);
                rc = Result.Failed;
            }

            return rc;
        }

    }




    [Transaction(TransactionMode.Manual)]
    public class CmdWallFamily : IExternalCommand
    {
        public Result Execute(
          ExternalCommandData commandData,
          ref string message,
          ElementSet elements)
        {
            UIApplication uiapp = commandData.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;

            Autodesk.Revit.ApplicationServices.Application app = uiapp.Application;
            Document doc = uidoc.Document;
            Result rc;
            try
            {
                ElementsVisibleInViewExportContext context = new ElementsVisibleInViewExportContext(doc);
                if (doc.ActiveView as View3D != null)
                    Core.Export.ASTExportUtils.ExportView3D(doc, doc.ActiveView as View3D, uiapp, out context);
                else
                    MessageBox.Show("You must be in 3D view to export.");

                Material_Analysis mat = new Material_Analysis(context, app);
                mat.byFamily = true;
                mat.Analyze();
                string report = "";
                ASTExportUtils.txtExport(doc, mat, out report);

                //MessageBox.Show(report);
                Views.Form1 form = new Views.Form1(mat, doc);
                form.ShowDialog();

                rc = Result.Succeeded;
            }
            catch (Exception ex)
            {
                TaskDialog.Show("Calculation Failed!" + "\r\n" + ex.Message, ex.Message + "\r\n" + ex.StackTrace);
                rc = Result.Failed;
            }

            return rc;
        }

    }




    [Transaction(TransactionMode.Manual)]
    public class CmdAssembly : IExternalCommand
    {
        public Result Execute(
          ExternalCommandData commandData,
          ref string message,
          ElementSet elements)
        {
            UIApplication uiapp = commandData.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;

            Autodesk.Revit.ApplicationServices.Application app = uiapp.Application;
            Document doc = uidoc.Document;
            Result rc;
            try
            {
                ElementsVisibleInViewExportContext context = new ElementsVisibleInViewExportContext(doc);
                if (doc.ActiveView as View3D != null)
                    Core.Export.ASTExportUtils.ExportView3D(doc, doc.ActiveView as View3D, uiapp, out context);
                else
                    MessageBox.Show("You must be in 3D view to export.");

                Assembly_Analysis mat = new Assembly_Analysis(context, app);
                mat.Analyze();
                string report = "";
                ASTExportUtils.txtExport(doc, mat, out report);

                //MessageBox.Show(report);
                Views.Form1 form = new Views.Form1(mat, doc);
                form.ShowDialog();
                rc = Result.Succeeded;
            }
            catch (Exception ex)
            {
                TaskDialog.Show("Calculation Failed!" + "\r\n" + ex.Message, ex.Message + "\r\n" + ex.StackTrace);
                rc = Result.Failed;
            }

            return rc;
        }

    }




    [Transaction(TransactionMode.Manual)]
    public class CmdAssemblyMaterial : IExternalCommand
    {
        public Result Execute(
          ExternalCommandData commandData,
          ref string message,
          ElementSet elements)
        {
            UIApplication uiapp = commandData.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;

            Autodesk.Revit.ApplicationServices.Application app = uiapp.Application;
            Document doc = uidoc.Document;
            Result rc;
            try
            {
                ElementsVisibleInViewExportContext context = new ElementsVisibleInViewExportContext(doc);
                if (doc.ActiveView as View3D != null)
                    Core.Export.ASTExportUtils.ExportView3D(doc, doc.ActiveView as View3D, uiapp, out context);
                else
                    MessageBox.Show("You must be in 3D view to export.");

                Assembly_Analysis mat = new AssemblyMaterials_Analysis(context, app);
                mat.Analyze();
                string report = "";
                ASTExportUtils.txtExport(doc, mat, out report);

                //MessageBox.Show(report);
                Views.Form1 form = new Views.Form1(mat, doc);
                form.ShowDialog();
                rc = Result.Succeeded;
            }
            catch (Exception ex)
            {
                TaskDialog.Show("Calculation Failed!" + "\r\n" + ex.Message, ex.Message + "\r\n" + ex.StackTrace);
                rc = Result.Failed;
            }

            return rc;
        }

    }




    [Transaction(TransactionMode.Manual)]
    public class CmdMatCalc : IExternalCommand
    {
        public Result Execute(
          ExternalCommandData commandData,
          ref string message,
          ElementSet elements)
        {
            UIApplication uiapp = commandData.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;

            Autodesk.Revit.ApplicationServices.Application app = uiapp.Application;
            Document doc = uidoc.Document;
            Result rc;
            try
            {
                ElementsVisibleInViewExportContext context = new ElementsVisibleInViewExportContext(doc);
                if (doc.ActiveView as View3D != null)
                    Core.Export.ASTExportUtils.ExportView3D(doc, doc.ActiveView as View3D, uiapp, out context);
                else
                    MessageBox.Show("You must be in 3D view to export.");

                DetailedMaterial_Analysis analysis = new DetailedMaterial_Analysis(context, app);
                DialogResult byCategory = MessageBox.Show("Do you want material to be sorted by Category?", "Category Sorting", MessageBoxButtons.YesNo);
                if(byCategory == DialogResult.Yes)
                {
                    analysis.SortByCategory = true;
                }
                DialogResult dialogResult = MessageBox.Show("Do you want material to be sorted by Family?", "Family Sorting", MessageBoxButtons.YesNo);
                if (dialogResult == DialogResult.Yes)
                {
                    analysis.SortByFamily = true;
                    DialogResult dialogResult2 = MessageBox.Show("Do you want material to be sorted by Type?", "Type Sorting", MessageBoxButtons.YesNo);
                    if (dialogResult2 == DialogResult.Yes)
                    {
                        analysis.SortByType = true;
                    }
                }

                analysis.Analyze();
                string report = "";
                ASTExportUtils.txtExport(doc, analysis, out report);

                //MessageBox.Show(report);

                Views.Form1 form = new Views.Form1(analysis, doc);

                form.ShowDialog();
                rc = Result.Succeeded;
            }
            catch (Exception ex)
            {
                TaskDialog.Show("Calculation Failed!" + "\r\n" + ex.Message, ex.Message + "\r\n" + ex.StackTrace);
                rc = Result.Failed;
            }

            return rc;
        }

    }
}