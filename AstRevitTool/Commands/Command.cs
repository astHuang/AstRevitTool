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
using System.Text;
using System.Text.RegularExpressions;
using System.Net;


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
                if (byCategory == DialogResult.Yes)
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



    [Transaction(TransactionMode.Manual)]
    public class CmdUpdater : IExternalCommand
    {
        public Result Execute(
          ExternalCommandData commandData,
          ref string message,
          ElementSet elements)
        {
            string version = Assembly.GetCallingAssembly().GetName().Version.ToString();
            string latest = LatestVersion.ToString();
            if (!HasUpdate)
            {
                TaskDialog.Show("Update check", "This is the latest version! Version: "+ version);
            }
            else
            {
                TaskDialog.Show("Update check", "This tool can be updated!Current version is: "+ version+  "\r\n The latest version is: "+latest + "\r\n Opening download link...");
                string url = string.Concat("https://github.com", GitHubRepo, "/releases/latest");
                Process.Start(url);
            }
            return Result.Succeeded;
        }
        static readonly Lazy<IDictionary<Version, Uri>> _lazyVersionUrls = new Lazy<IDictionary<Version, Uri>>(() => _GetVersionUrls());
        public static string GitHubRepo = "astHuang/AstRevitTool";
        public static string GitHubRepoName
        {
            get
            {
                var si = GitHubRepo.LastIndexOf('/');
                return GitHubRepo.Substring(si + 1);
            }
        }
        static IDictionary<Version, Uri> _VersionUrls
        {
            get
            {
                return _lazyVersionUrls.Value;
            }
        }
        public static Version LatestVersion
        {
            get
            {
                var v = Assembly.GetCallingAssembly().GetName().Version;
                var va = new List<Version>(_VersionUrls.Keys);
                va.Add(v);
                va.Sort();
                return va[va.Count - 1];
            }
        }
        public static bool HasUpdate
        {
            get
            {
                var v = Assembly.GetCallingAssembly().GetName().Version;
                foreach (var e in _VersionUrls)
                    if (e.Key>v)
                        return true;
                return false;
            }
        }
        static IDictionary<Version, Uri> _GetVersionUrls()
        {

            string pattern =
                    string.Concat(
                        Regex.Escape(GitHubRepo),
                        @"\/releases\/download\/Refresh.v[0-9]+\.[0-9]+\.[0-9]+\.[0-9]+.*\.zip");

            Regex urlMatcher = new Regex(pattern, RegexOptions.CultureInvariant | RegexOptions.Compiled);
            var result = new Dictionary<Version, Uri>();
            WebRequest wrq = WebRequest.Create(string.Concat("https://github.com", GitHubRepo, "/releases/latest"));
            WebResponse wrs = null;
            try
            {
                wrs = wrq.GetResponse();
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Error fetching repo: "+ex.Message);
                return result;
            }
            using (var sr = new StreamReader(wrs.GetResponseStream()))
            {
                string line;
                while (null != (line = sr.ReadLine()))
                {
                    var match = urlMatcher.Match(line);
                    if (match.Success)
                    {
                        var uri = new Uri(string.Concat("https://github.com", match.Value));
                        var vs = match.Value.LastIndexOf("/Refresh.v");
                        var sa = match.Value.Substring(vs+10).Split('.', '/');
                        var v = new Version(int.Parse(sa[0]), int.Parse(sa[1]), int.Parse(sa[2]), int.Parse(sa[3]));
                        result.Add(v, uri);
                    }
                }
            }
            return result;
        }
    }
}