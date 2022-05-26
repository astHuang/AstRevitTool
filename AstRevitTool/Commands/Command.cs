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
using AstRevitTool.Energy;


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
                //string report = "";
                //ASTExportUtils.txtExport(doc, wwr, out report);

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
                //string report = "";
                //ASTExportUtils.txtExport(doc, mat, out report);

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
                //string report = "";
                //ASTExportUtils.txtExport(doc, mat, out report);

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
                //string report = "";
                //ASTExportUtils.txtExport(doc, mat, out report);

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
                //string report = "";
                //ASTExportUtils.txtExport(doc, mat, out report);

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
                //string report = "";
                //ASTExportUtils.txtExport(doc, mat, out report);

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

                /*
                DialogResult includeGeneric = MessageBox.Show("Do you want to include generic models?(Select yes if you have complex model-in-place geometries)", "Inclusion", MessageBoxButtons.YesNo);
                if (includeGeneric == DialogResult.Yes)
                {
                    analysis.IncludeGeneric = true;
                }*/

                analysis.Analyze();
                //string report = "";
                //ASTExportUtils.txtExport(doc, analysis, out report);

                //MessageBox.Show(report);

                Views.Form1 form = new Views.Form1(analysis, doc);
                //uidoc.Selection.SetElementIds(new List<ElementId>(analysis.AllAnalyzedElement().Select<Element, ElementId>(r => r.Id)));

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
            string version = Assembly.GetExecutingAssembly().GetName().Version.ToString();
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
                var v = Assembly.GetExecutingAssembly().GetName().Version;
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
                var v = Assembly.GetExecutingAssembly().GetName().Version;
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





    [Transaction(TransactionMode.Manual)]
    public class CmdLifeCycle : IExternalCommand
    {
        public Result Execute(
          ExternalCommandData commandData,
          ref string message,
          ElementSet elements)
        {
            UIApplication application = commandData.Application;
            UIDocument activeUIDocument = commandData.Application.ActiveUIDocument;
            var app = application.Application;
            var doc = activeUIDocument.Document;
            Result rc;
            try
            {
                RevitImportSettings settings = new RevitImportSettings();
                settings = settings.DeSerializeXML();
                RevitImport.ImportElements(application, settings, "");
                //UI2 ui2 = new UI2(doc,app,activeUIDocument);
                //ui2.Show();
                rc = Result.Succeeded;
            }
            catch (Exception ex)
            {
                TaskDialog.Show("Failed!" + "\r\n" + ex.Message, ex.Message + "\r\n" + ex.StackTrace);
                rc = Result.Failed;
            }

            return rc;
        }

    }



    [Transaction(TransactionMode.Manual)]
    public class CmdBOMA : IExternalCommand
    {
        public static DialogResult InputBox(string title, string promptText, ref string value)
        {
            System.Windows.Forms.Form form = new System.Windows.Forms.Form();
            Label label = new Label();
            System.Windows.Forms.TextBox textBox = new System.Windows.Forms.TextBox();
            Button buttonOk = new Button();
            Button buttonCancel = new Button();

            form.Text = title;
            label.Text = promptText;
            textBox.Text = value;

            buttonOk.Text = "OK";
            buttonCancel.Text = "Cancel";
            buttonOk.DialogResult = DialogResult.OK;
            buttonCancel.DialogResult = DialogResult.Cancel;

            label.SetBounds(9, 20, 372, 13);
            textBox.SetBounds(12, 36, 372, 20);
            buttonOk.SetBounds(228, 72, 75, 23);
            buttonCancel.SetBounds(309, 72, 75, 23);

            label.AutoSize = true;
            textBox.Anchor = textBox.Anchor | AnchorStyles.Right;
            buttonOk.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            buttonCancel.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;

            form.ClientSize = new Size(396, 107);
            form.Controls.AddRange(new System.Windows.Forms.Control[] { label, textBox, buttonOk, buttonCancel });
            form.ClientSize = new Size(Math.Max(300, label.Right + 10), form.ClientSize.Height);
            form.FormBorderStyle = FormBorderStyle.FixedDialog;
            form.StartPosition = FormStartPosition.CenterScreen;
            form.MinimizeBox = false;
            form.MaximizeBox = false;
            form.AcceptButton = buttonOk;
            form.CancelButton = buttonCancel;

            DialogResult dialogResult = form.ShowDialog();
            value = textBox.Text;
            return dialogResult;
        }
        public bool Check_Spec(ViewSchedule vs, Document doc) {
            bool checkclass = false;
            bool checkfar = false;
            bool checkrentable = false;
            int num = vs.Definition.GetFieldCount();
            for (int i = 0; i < num; i++) {
                string colhead = vs.Definition.GetField(i).ColumnHeading;
                if(colhead.IndexOf("FAR Exclusion", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    checkfar = true;
                    using (Transaction tr = new Transaction(doc))
                    {
                        tr.Start("Change Field Name for FAR Exclusion");
                        vs.Definition.GetField(i).ColumnHeading = "FAR Exclusion";
                        tr.Commit();
                    }
                    continue;
                }
                else if (colhead.IndexOf("Leasing Classification", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    checkclass = true;
                    using (Transaction tr = new Transaction(doc))
                    {
                        tr.Start("Change Field Name for Leasing Classification");
                        vs.Definition.GetField(i).ColumnHeading = "Leasing Classification";
                        tr.Commit();
                    }
                    continue;
                }
                else if (colhead.IndexOf("Rentable Exclusion", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    checkrentable = true;
                    using (Transaction tr = new Transaction(doc))
                    {
                        tr.Start("Change Field Name for Rentable Exclusion");
                        vs.Definition.GetField(i).ColumnHeading = "Rentable Exclusion";
                        tr.Commit();
                    }
                    continue;
                }
            }
            return checkclass && checkfar && checkrentable;
        }


        public string ReplaceInvalidChars(string filename)
        {
            return string.Join("_", filename.Split(Path.GetInvalidFileNameChars()));
        }
        public Result Execute(
          ExternalCommandData commandData,
          ref string message,
          ElementSet elements)
        {
            UIApplication application = commandData.Application;
            UIDocument activeUIDocument = commandData.Application.ActiveUIDocument;
            var app = application.Application;
            var doc = activeUIDocument.Document;
            Result rc;
            try
            {
                FilteredElementCollector col = new FilteredElementCollector(doc).OfClass(typeof(ViewSchedule));
                string extra = "";
                InputBox("Extra Keywords", "Separate different BOMA schedules using keywords, such as 'Single Tenant'", ref extra);
                ViewScheduleExportOptions opt
                  = new ViewScheduleExportOptions();
                string folder = Directory.GetParent(Assembly.GetExecutingAssembly().Location) + "\\BOMA";
                List<string> args = new List<string>();
                bool foundArea = false;
                bool foundSpec = false;
                foreach (ViewSchedule vs in col)
                {
                    //Directory.CreateDirectory(folder);
                    try
                    {
                        string title = vs.Name;
                        bool containkeyword = title.IndexOf(extra, StringComparison.OrdinalIgnoreCase) >= 0;
                        if (!containkeyword) { continue; }

                        string filename = title.Replace(" ", "_")+".txt";
                        filename = ReplaceInvalidChars(filename);
                        string datafolder = folder + "\\dist\\app\\data";
                        bool contains1 = foundArea == false && title.IndexOf("Boundary Area", StringComparison.OrdinalIgnoreCase) >= 0;                        
                        bool contains2 = foundSpec == false && title.IndexOf("Space Classification", StringComparison.OrdinalIgnoreCase) >= 0;
                        
                        if (contains1)
                        {
                            vs.Export(datafolder, filename, opt);
                            foundArea = true;
                            args.Add(filename);
                        }
                        else if (contains2) {
                            bool eligable = Check_Spec(vs, doc);
                            if (!eligable)
                            {
                                MessageBox.Show("Your SPACE CLASSIFICATION schedule must contain'FAR Exclusion','Rentable Exclusion' and 'Leasing Classification'!");
                                return Result.Failed;
                            }
                            vs.Export(datafolder, filename, opt);
                            foundSpec = true;
                            args.Add(filename);
                        }
                        continue;

                    }
                    catch
                    {
                        continue;
                    }
                    //args.Add(vs.Name+".txt");
                }
                if (args.Count != 2) {
                    MessageBox.Show("Can not find area schedule!");
                    return Result.Failed;
                }
                var psi = new ProcessStartInfo();
                //psi.FileName = @"C:\Users\huang\Anaconda3\envs\ghcpython\python.exe";
                psi.FileName = folder + "\\dist\\app\\app.exe";
                //bool test = File.Exists(psi.FileName);
                psi.WorkingDirectory = folder + "\\dist\\app";

                //var script = "RvTest.py";
                //test = File.Exists(script);
                string arg1 = args[0];
                string arg2 = args[1];
                //string arg1 = "BOMA Boundary Area.txt";
                //string arg2 = "BOMA Space Classifications.txt";
                //psi.Arguments = $"\"{script}\" \"{arg1}\" \"{arg2}\"";
                psi.Arguments = $"\"{arg1}\" \"{arg2}\"";

                //Process.Start(psi);
                psi.UseShellExecute = false;
                psi.RedirectStandardOutput = true;
                psi.CreateNoWindow = true;
                psi.RedirectStandardError = true;

                //var errors = "";
                var results = "";
                using(var process = Process.Start(psi))
                {
                    //errors = process.StandardError.ReadToEnd();
                    results = process.StandardOutput.ReadToEnd();
                }

                string finaloutput = "";
                finaloutput += "RESULTS:"  + "\n " + results;
                MessageBox.Show(finaloutput);

               rc = Result.Succeeded;
            }
            catch (Exception ex)
            {
                TaskDialog.Show("Failed!" + "\r\n" + ex.Message, ex.Message + "\r\n" + ex.StackTrace);
                rc = Result.Failed;
            }

            return rc;
        }

    }
}