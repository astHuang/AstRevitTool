using AstRevitTool.ViewModels;
using AstRevitTool.Views;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
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
using AstRevitTool.Masterclass.Dockable;
using System.Text;
using System.Text.RegularExpressions;
using System.Net;
using AstRevitTool.Energy;
using Octokit;


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
                    Core.Export.ASTExportUtils.ExportView3D(doc, doc.ActiveView, uiapp, out context);
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
                    Core.Export.ASTExportUtils.ExportView3D(doc, doc.ActiveView, uiapp, out context);
                else
                    MessageBox.Show("You must be in 3D view to export.");

                Material_Analysis mat = new Material_Analysis(context, app);
                mat.Analyze();
                //string report = "";
                //ASTExportUtils.txtExport(doc, mat, out report);

                //MessageBox.Show(report);
                Views.Form1 form = new Views.Form1(mat, doc);
                form.Show();
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
                    Core.Export.ASTExportUtils.ExportView3D(doc, doc.ActiveView, uiapp, out context);
                else
                    MessageBox.Show("You must be in 3D view to export.");

                Material_Analysis mat = new Material_Analysis(context, app);
                mat.byType = true;
                mat.Analyze();
                //string report = "";
                //ASTExportUtils.txtExport(doc, mat, out report);

                //MessageBox.Show(report);
                Views.Form1 form = new Views.Form1(mat, doc);
                form.Show();
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
                    Core.Export.ASTExportUtils.ExportView3D(doc, doc.ActiveView, uiapp, out context);
                else
                    MessageBox.Show("You must be in 3D view to export.");

                Material_Analysis mat = new Material_Analysis(context, app);
                mat.byFamily = true;
                mat.Analyze();
                //string report = "";
                //ASTExportUtils.txtExport(doc, mat, out report);

                //MessageBox.Show(report);
                Views.Form1 form = new Views.Form1(mat, doc);
                form.Show();

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
    public class CmdCustomAnalysis : IExternalCommand
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

            ElementsVisibleInViewExportContext context = new ElementsVisibleInViewExportContext(doc);
            if (doc.ActiveView as View3D != null)
                Core.Export.ASTExportUtils.ExportView3D(doc, doc.ActiveView, uiapp, out context);
            else
                MessageBox.Show("You must be in 3D view to export.");

            Analysis_Main main_window = new Analysis_Main(context,doc,app);
            main_window.Show();
            rc = Result.Succeeded;
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
                    Core.Export.ASTExportUtils.ExportView3D(doc, doc.ActiveView, uiapp, out context);
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
                    Core.Export.ASTExportUtils.ExportView3D(doc, doc.ActiveView, uiapp, out context);
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
                    Core.Export.ASTExportUtils.ExportView3D(doc, doc.ActiveView, uiapp, out context);
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
            Version localVersion = Assembly.GetExecutingAssembly().GetName().Version;
            Version latestGitHubVersion = LatestVersion;
            /*
            if (!HasUpdate)
            {
                TaskDialog.Show("Update check", "This is the latest version! Version: "+ version);
            }
            else
            {
                TaskDialog.Show("Update check", "This tool can be updated!Current version is: "+ version+  "\r\n The latest version is: "+latest + "\r\n Opening download link...");
                string url = string.Concat("https://github.com", GitHubRepo, "/releases/latest");
                Process.Start(url);
            }*/
            //CheckGitHubNewerVersion();
            //GitHubClient client = new GitHubClient(new ProductHeaderValue("AstRevitTool"));
            //client.Repository.Release.GetAll("astHuang", "AstRevitTool");
            int versionComparison = localVersion.CompareTo(latestGitHubVersion);
            if (versionComparison < 0)
            {
                TaskDialog.Show("Update check", "This tool can be updated!Current version is: " + localVersion.ToString() + "\r\n The latest version is: " + latestGitHubVersion.ToString() + "\r\n Opening download link...");
                string url = string.Concat("https://github.com", GitHubRepo, "/releases/latest");
                Process.Start(url);
                //The version on GitHub is more up to date than this local release.
            }
            else if (versionComparison > 0)
            {
                //This local version is greater than the release version on GitHub.
                TaskDialog.Show("Update check", "This is a test version that has not been released! Version: " + localVersion.ToString());
            }
            else
            {
                //This local Version and the Version on GitHub are equal.
                TaskDialog.Show("Update check", "This is the latest release version! Version: " + localVersion.ToString());
            }
            return Result.Succeeded;
        }
        static readonly Lazy<IDictionary<Version, Uri>> _lazyVersionUrls = new Lazy<IDictionary<Version, Uri>>(() => _GetVersionUrls());
        public static string GitHubRepo = "/astHuang/AstRevitTool";
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

        private async System.Threading.Tasks.Task CheckGitHubNewerVersion()
        {
            //Get all releases from GitHub
            //Source: https://octokitnet.readthedocs.io/en/latest/getting-started/
            GitHubClient client = new GitHubClient(new ProductHeaderValue("AstRevitTool"));
            IReadOnlyList<Release> releases = await client.Repository.Release.GetAll("astHuang", "AstRevitTool");

            //Setup the versions
            Version latestGitHubVersion = new Version(releases[0].TagName);
            Version localVersion = Assembly.GetExecutingAssembly().GetName().Version; //Replace this with your local version. 
                                                                                      //Only tested with numeric values.

            //Compare the Versions
            //Source: https://stackoverflow.com/questions/7568147/compare-version-numbers-without-using-split-function
            int versionComparison = localVersion.CompareTo(latestGitHubVersion);
            if (versionComparison < 0)
            {
                TaskDialog.Show("Update check", "This tool can be updated!Current version is: " + localVersion.ToString() + "\r\n The latest version is: " + latestGitHubVersion.ToString() + "\r\n Opening download link...");
                string url = string.Concat("https://github.com", GitHubRepo, "/releases/latest");
                Process.Start(url);
                //The version on GitHub is more up to date than this local release.
            }
            else if (versionComparison > 0)
            {
                //This local version is greater than the release version on GitHub.
                TaskDialog.Show("Update check", "This is a test version that has not been released! Version: " + localVersion.ToString());
            }
            else
            {
                //This local Version and the Version on GitHub are equal.
                TaskDialog.Show("Update check", "This is the latest release version! Version: " + localVersion.ToString());
            }
        }

        public static Version LatestVersion
        {
            get
            {

                var v = Assembly.GetExecutingAssembly().GetName().Version;
                var va = new List<Version>(_VersionUrls.Keys);
                //va.Add(v);
                //va.Sort();
                return va[0];
            }
        }
        public static bool HasUpdate
        {
            get
            {
                var v = Assembly.GetExecutingAssembly().GetName().Version;
                foreach (var e in _VersionUrls)
                    if (e.Key > v)
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
                var v = wrs.ResponseUri.ToString().Split('/');
                var vs = v[v.Length - 1].Split('.');
                var version = new Version(int.Parse(vs[0]), int.Parse(vs[1]), int.Parse(vs[2]), int.Parse(vs[3]));
                result.Add(version, wrs.ResponseUri);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Error fetching repo: " + ex.Message);
                return result;
            }
            /*
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
            }*/
            return result;
        }
    }

    [Transaction(TransactionMode.ReadOnly)]
    public class CmdUSDExport : IExternalCommand
    {
        
        public Result Execute(
          ExternalCommandData commandData,
          ref string message,
          ElementSet elements)
        {
            UIApplication uiapp = commandData.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            var app = uiapp.Application;
            Document doc = uidoc.Document;
            //var result = Result.Cancelled;
            var view = doc.ActiveView as Autodesk.Revit.DB.View;


            if (null == view as View3D)
            {
                message = "Please run this command in a 3D view.";
                return Result.Failed;
            }


            int lodValue = 8;
            var window = new AstRevitTool.Views.Window1();
            window.ShowDialog();
            lodValue = window.lod;
            

            SaveFileDialog sdial = new SaveFileDialog();
            
            sdial.Filter = "gltf|*.gltf|glb|*.glb";
            //MessageBox.Show("This function is under development and will be released with Cumulus 3.0!");
            if (sdial.ShowDialog() == DialogResult.OK)
            {
                string filename = sdial.FileName;
                string directory = Path.GetDirectoryName(filename) + "\\";

                
                
                //默认值减面为等级8
                int combobox_value = lodValue;
                //RevitExportObj2Gltf contextObj = new RevitExportObj2Gltf(doc, sdial.FileName, combobox_value);
                //MyGltfExportContext contextGltf = new MyGltfExportContext(doc, combobox_value);
                //拿到revit的doc  CustomExporter 用户自定义导出
                RevitExportGltfContext context = new RevitExportGltfContext(doc, sdial.FileName, combobox_value);
                
                using (CustomExporter exporter = new CustomExporter(doc, context))
                {
                    //是否包括Geom对象
                    exporter.IncludeGeometricObjects = false;
                    exporter.ShouldStopOnError = true;
                    //导出3D模型
                    exporter.Export(view);
                    MessageBox.Show("glTF exported successfully!");
                }
                /*
                using (CustomExporter exporterObj = new CustomExporter(doc, contextObj))
                {
                    //是否包括Geom对象
                    exporterObj.IncludeGeometricObjects = false;
                    exporterObj.ShouldStopOnError = true;
                    //导出3D模型                 
                    exporterObj.Export(view);
                }*/
                /*
                using (CustomExporter exporterGltf = new CustomExporter(doc, contextGltf))
                {
                    //是否包括Geom对象                    
                    exporterGltf.IncludeGeometricObjects = false;
                    exporterGltf.ShouldStopOnError = true;
                    //导出3D模型                   
                    exporterGltf.Export(view);
                    contextGltf._model.SaveGLB(sdial.FileName);
                    contextGltf._model.SaveGLTF(sdial.FileName);
                }*/
                System.Diagnostics.Process p = new System.Diagnostics.Process();
                p.StartInfo.FileName = "cmd.exe";
                p.StartInfo.UseShellExecute = false;        //是否使用操作系统shell启动
                p.StartInfo.RedirectStandardInput = true;   //接受来自调用程序的输入信息
                p.StartInfo.RedirectStandardOutput = true;  //由调用程序获取输出信息
                p.StartInfo.RedirectStandardError = true;   //重定向标准错误输出
                p.StartInfo.CreateNoWindow = true;//不显示程序窗口
                p.StartInfo.WorkingDirectory = Path.GetDirectoryName(filename);
                p.Start();//启动程序
                //使用gltf pipeline命令行工具
                //向cmd窗口发送输入信息  （node.js已经是配置好了系统环境变量）
                //string str = @"cd D:\cmder";
                //p.StandardInput.WriteLine(str);

                //将GLTF转换为glb二进制 压缩纹理与bin顶点
                /*
                string glbName = Path.GetFileNameWithoutExtension(sdial.FileName) + "(Draco)" + ".glb";
                string glbstr = string.Format("gltf-pipeline.cmd gltf-pipeline -i {0}", sdial.FileName);
                p.StandardInput.WriteLine(glbstr);*/


                //gltf-pipeline.c md gltf-pipeline -i model.gltf -o modelDraco.gltf -d
                //运用Draco算法将GLTF压缩  压缩纹理与bin顶点是json文件
                string gltfDracoName = Path.GetFileNameWithoutExtension(sdial.FileName) + "(Draco)" + ".gltf";
                string gltfDraco = string.Format("gltf-pipeline.cmd gltf-pipeline -i {0} -d", Path.GetFileName(sdial.FileName));
                p.StandardInput.WriteLine(gltfDraco);

                //gltf - pipeline - i model.gltf - t
                //压缩bin二进制为base64编码，但是保留纹理
                /*
                string gltfTextureName = Path.GetFileNameWithoutExtension(sdial.FileName) + "(Texture)" + ".gltf";
                string gltfTexture = string.Format("gltf-pipeline.cmd gltf-pipeline -i {0} -t", sdial.FileName);
                p.StandardInput.WriteLine(gltfTexture);*/

                p.StandardInput.AutoFlush = true;
                p.StandardInput.WriteLine("exit");

                //获取cmd窗口的输出信息
                string output = p.StandardOutput.ReadToEnd();
                System.Windows.MessageBox.Show(output);
            }

            return Result.Succeeded;
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

    [Transaction(TransactionMode.ReadOnly)]
    public class CmdSvgExport : IExternalCommand
    {
        public bool Check_Spec(ViewSchedule vs, Document doc)
        {
            bool checkclass = false;
            bool checkrentable = false;
            int num = vs.Definition.GetFieldCount();
            for (int i = 0; i < num; i++)
            {
                string colhead = vs.Definition.GetField(i).ColumnHeading;

                if (colhead.IndexOf("BOMA Space Classification", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    checkclass = true;
                }
                else if (colhead.IndexOf("BOMA Rentable Exclusion", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    checkrentable = true;
                }
            }
            return checkclass && checkrentable;
        }

        public bool CheckViewSchedule(ViewSchedule vs, Document doc)
        {
            bool r1 = SvgExport.getAreaSchemeFromSchedule(vs, doc) != null;
            if (!r1) return false;
            AreaScheme areaScheme = SvgExport.getAreaSchemeFromSchedule(vs, doc);
            ElementClassFilter filter = new ElementClassFilter(typeof(SpatialElement));
            IList<Element> allareaonLevel = areaScheme.GetDependentElements(filter).Select(x => doc.GetElement(x)).ToList();

            foreach (Element el in allareaonLevel)
            {
                Area area = el as Area;
                if (area != null) continue;
                if (area.LookupParameter(AstRevitTool.Constants.BOMA).AsString() != null)
                {
                    return true;
                }
            }
            //MessageBox.Show("Please check if your area schedule contains correct BOMA parameters");
            //return false;
            return true;
        }
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {

            UIApplication uiapp = commandData.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            Result rc = Result.Cancelled;
            var app = uiapp.Application;
            Document doc = uidoc.Document;
            ViewSchedule view = doc.ActiveView as ViewSchedule;
            if (view == null)
            {
                MessageBox.Show("Please export in a schedule view");
                return Result.Cancelled;
            }
            if (!CheckViewSchedule(view, doc)) return Result.Cancelled;

            List<ElementId> levelIds = SvgExport.uniqueLevelIdsInSchedule(view, doc);
            Dictionary<string, SvgExport.floorJson> exportList = new Dictionary<string, SvgExport.floorJson>();
            IList<SvgExport.floorJson> floors = new List<SvgExport.floorJson>();
            foreach (ElementId level in levelIds)
            {
                SvgExport.floorJson fj = SvgExport.levelFromSchedule(view, level, doc);
                string fileName = doc.Title + "__" + doc.GetElement(level).Name;
                exportList.Add(fileName, fj);
                floors.Add(fj);
            }
            SvgExport.buildingJson bj = new SvgExport.buildingJson(doc.Title, floors);
            SaveFileDialog dlg = new SaveFileDialog();
            dlg.FileName = "BomaViz__" + doc.Title;
            dlg.DefaultExt = "json";
            dlg.CheckFileExists = false;
            dlg.AddExtension = true;
            dlg.Filter = "JSON files(*.json) | *.json | All files(*.*) | *.* ";
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                string filename = dlg.FileName;
                StreamWriter sw = new StreamWriter(filename);
                sw.Write(bj.toJsonString());
                sw.Close();
                string msg = "JSON file is saved! File location: " + dlg.FileName;
                string title = "Successfully saved BOMA Json file!";
                MessageBox.Show(msg, title);
                rc = Result.Succeeded;
            }

            return rc;
        }
    }

    [Transaction(TransactionMode.ReadOnly)]
    public class CmdColladaExport : IExternalCommand
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
            if (doc.ActiveView as View3D != null)
            {

                var window = new AstRevitTool.Views.Window1();
                window.ShowDialog();
                
                SaveFileDialog dlg = new SaveFileDialog();
                dlg.FileName = doc.Title.Replace(" ", "_");
                dlg.DefaultExt = "dae";
                dlg.CheckFileExists = false;
                dlg.AddExtension = true;
                dlg.Filter = "COLLADA files(*.dae) | *.dae | All files(*.*) | *.* ";
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    string filename = dlg.FileName;
                    
                    //filename = filename.Replace(" ", "_");
                    OptionsExporter option = new OptionsExporter();
                    option.CollectTextures = window.useTexture;
                    option.OptimizeSolids = false;
                    option.ExportNodes = window.useNodes;
                    option.SkipInteriorDetails = window.skipInterior;
                    option.FilePath = filename;
                    option.MainView3D = doc.ActiveView as View3D;

                    ExportView3D(doc,option);
                    string msg = "COLLADA file is saved! Now optimizing for Cumulus...Press OK to continue ";
                    string title = "Successfully saved file!";
                    MessageBox.Show(msg, title);

                    string input = dlg.FileName;
                    string outputDir = Path.GetDirectoryName(input);
                    outputDir += "\\Cumulus";
                    if (!Directory.Exists(outputDir))
                    {
                        Directory.CreateDirectory(outputDir);
                    }
                    filename = Path.GetFileNameWithoutExtension(input).Replace(" ","_") + ".gltf";
                    string output = Path.Combine(outputDir, filename);

                    RunConverter(input, output);
                    //msg = "Model file for Cumulus is optimized and saved! File location: " + output;
                    //title = "Successful cumulus convertion!";
                    //MessageBox.Show(msg, title);
                    rc = Result.Succeeded;
                }
                else { rc = Result.Cancelled; }
            }
            else
                MessageBox.Show("You must be in 3D view to export.");

            rc = Result.Cancelled;
            return rc;
        }

        internal void ExportView3D(Document document, OptionsExporter exportingOptions)
        {
            ColladaExportContext exportContextExport = new ColladaExportContext(document, exportingOptions);
            CustomExporter customExporter1 = new CustomExporter(document, (IExportContext)exportContextExport);
            customExporter1.IncludeGeometricObjects = false;
            customExporter1.ShouldStopOnError = false;
            try
            {
                string str = "";
                try
                {
                    ElementId id = ((Element)exportingOptions.MainView3D).Id;
                    str = Path.GetTempFileName();
                    ImageExportOptions imageExportOptions1 = new ImageExportOptions()
                    {
                        ZoomType = (ZoomFitType)0,
                        PixelSize = 128,
                        ImageResolution = (ImageResolution)0,
                        FitDirection = (FitDirectionType)0,
                        ExportRange = (ExportRange)2,
                        HLRandWFViewsFileType = (ImageFileType)4,
                        ShadowViewsFileType = (ImageFileType)4,
                        FilePath = str
                    };
                    ImageExportOptions imageExportOptions2 = imageExportOptions1;
                    List<ElementId> elementIdList = new List<ElementId>();
                    elementIdList.Add(id);
                    imageExportOptions2.SetViewsAndSheets((IList<ElementId>)elementIdList);
                    document.ExportImage(imageExportOptions1);
                    str = Path.Combine(Path.GetDirectoryName(str), Path.GetFileNameWithoutExtension(str) + ImageExportOptions.GetFileName(document, id) + ".png");
                    if (!File.Exists(str))
                        str = "";
                }
                catch (Exception ex)
                {
                    if (File.Exists(str))
                        File.Delete(str);
                    str = "";
                }
                exportContextExport.IsSolidsPass = false;
                CustomExporter customExporter2 = customExporter1;
                List<ElementId> elementIdList1 = new List<ElementId>();
                elementIdList1.Add(((Element)exportingOptions.MainView3D).Id);
                customExporter2.Export((IList<ElementId>)elementIdList1);
                if (exportContextExport.HasSolids)
                {
                    exportContextExport.IsSolidsPass = true;
                    CustomExporter customExporter3 = customExporter1;
                    List<ElementId> elementIdList2 = new List<ElementId>();
                    elementIdList2.Add(((Element)exportingOptions.MainView3D).Id);
                    customExporter3.Export((IList<ElementId>)elementIdList2);
                }
                exportContextExport.WriteFile(str);
                if (str != "")
                {
                    if (File.Exists(str))
                        File.Delete(str);
                }
            }
            catch (Exception ex)
            {
                string str = exportContextExport != null ? exportContextExport.GetExceptionRaport() : "";
                int num = (int)MessageBox.Show(string.Format("Error exporting document \"{0}\"\nDescription: {1}{2}", (object)document.Title, (object)ex.Message, (object)str), "Lumion Collada exporter", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }

        internal void RunConverter(string input,string outputDir)
        {
            string folder = Constants.SCRIPT_FOLDER + "\\COLLADA2GLTF";
            System.Diagnostics.Process p = new System.Diagnostics.Process();
            p.StartInfo.FileName = "cmd.exe";
            p.StartInfo.UseShellExecute = false;        
            p.StartInfo.RedirectStandardInput = true;   
            p.StartInfo.RedirectStandardOutput = true;  
            p.StartInfo.RedirectStandardError = true;   
            p.StartInfo.CreateNoWindow = true;
            p.StartInfo.WorkingDirectory = folder;
            p.Start();

            
            string gltf = string.Format("COLLADA2GLTF-bin -i {0} -o {1}", input, outputDir);
            p.StandardInput.WriteLine(gltf);

            p.StandardInput.AutoFlush = true;
            p.StandardInput.WriteLine("exit");

            //获取cmd窗口的输出信息
            string output = p.StandardOutput.ReadToEnd();
            System.Windows.MessageBox.Show(output);

        }
    }
    [Transaction(TransactionMode.Manual)]
    public class CmdBOMA : IExternalCommand
    {
        
        public bool Check_Spec(ViewSchedule vs, Document doc)
        {
            bool checkclass = false;
            bool checkrentable = false;
            int num = vs.Definition.GetFieldCount();
            for (int i = 0; i < num; i++)
            {
                string colhead = vs.Definition.GetField(i).ColumnHeading;

                if (colhead.IndexOf("BOMA Space Classification", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    checkclass = true;
                }
                else if (colhead.IndexOf("BOMA Rentable Exclusion", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    checkrentable = true;
                }
            }
            return checkclass && checkrentable;
        }


        public void runPython(string name, string outDir, string project_name,string input_folder)
        {
            string folder = input_folder + "\\dist\\app";
            System.Diagnostics.Process p = new System.Diagnostics.Process();
            p.StartInfo.FileName = "cmd.exe";
            p.StartInfo.UseShellExecute = false;
            p.StartInfo.RedirectStandardInput = true;
            p.StartInfo.RedirectStandardOutput = true;
            p.StartInfo.RedirectStandardError = true;
            p.StartInfo.CreateNoWindow = true;
            p.StartInfo.WorkingDirectory = folder;
            p.Start();


            string gltf = string.Format("app.exe -i {0} -o {1} -n{2} -m{3}", '"'+name+'"', '"' + outDir + '"', '"' + project_name + '"', "BOMA");
            p.StandardInput.WriteLine(gltf);

            p.StandardInput.AutoFlush = true;
            p.StandardInput.WriteLine("exit");

            string output = p.StandardOutput.ReadToEnd();
            System.Windows.MessageBox.Show(output);


        }
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {

            UIApplication uiapp = commandData.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            Result rc = Result.Cancelled;
            var app = uiapp.Application;
            Document doc = uidoc.Document;
            ViewSchedule view = doc.ActiveView as ViewSchedule;
            string project = doc.ProjectInformation.Name;
            if (view == null)
            {
                MessageBox.Show("Please export in a schedule view");
                return Result.Cancelled;
            }
            string folder = Constants.SCRIPT_FOLDER + "\\PROGRAMMING";
            if (!Directory.Exists(folder))
            {
                MessageBox.Show("Unable export BOMA Excel Sheet because target directory cannot be found! \nNavigate mannually...");
                string testfolder = DialogUtils.SelectFolder();
                string process = testfolder + "\\dist\\app\\app.exe";
                if (Directory.Exists(process))
                {
                    folder = testfolder;
                }
                else
                {
                    MessageBox.Show("The folder is invalid. Try to force script running...");                   
                }
            }
            if (!Check_Spec(view, doc))
            {
                MessageBox.Show("Schedule does not contain BOMA Space Classification or Rentable Exclusion columns!");
                return Result.Cancelled;
            };
            SaveFileDialog dlg = new SaveFileDialog();
            dlg.FileName = "BOMA_" + doc.Title.Replace(" ", "_");
            dlg.DefaultExt = "xlsx";
            dlg.CheckFileExists = false;
            dlg.AddExtension = true;
            dlg.Filter = "Excel Files|*.xls;*.xlsx;*.xlsm"; ;
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                string filename = dlg.FileName;

                ViewScheduleExportOptions opt
                  = new ViewScheduleExportOptions();

                string datafolder = folder + "\\dist\\app\\data";
                string name = doc.Title.Replace(" ", "_") + ".txt";
                name = Utils.ReplaceInvalidChars(name);
                view.Export(datafolder, name, opt);
                runPython(name, dlg.FileName,project,folder);
                string logpath = folder + "\\dist\\app\\RESULT_FLAG.txt";
                if (File.ReadAllText(logpath) == "TRUE")
                {
                    rc = Result.Succeeded;
                    string finaloutput = "BOMA EXCEL EXPORTED SUCCESSFULLY!" + "\n " + dlg.FileName;
                    MessageBox.Show(finaloutput);
                }
                else
                {
                    rc = Result.Failed;
                    string reasons = "Possible Reasons: \nArea plans are not well placed or missing";
                    string finaloutput = "THERE ARE SOME PROBLEMS WHEN PROCESSING THE EXCEL SHEET!" + "\n " + reasons;
                    MessageBox.Show(finaloutput);
                }
            }
            return rc;
            
        }

    }

    [Transaction(TransactionMode.ReadOnly)]
    public class CmdUnitMatrix : IExternalCommand
    {

        public bool Check_Spec(ViewSchedule vs, Document doc)
        {
            bool checklevel = false;
            bool checkunit = false;
            bool checkarea = false;
            int num = vs.Definition.GetFieldCount();
            
            for (int i = 0; i < num; i++)
            {
                string colhead = vs.Definition.GetField(i).ColumnHeading;

                if (colhead=="Name")
                {

                    checkunit = true;
                }
                else if (colhead== "Level")
                {
                    checklevel = true;
                }
                else if (colhead == "Area")
                {
                    checkarea = true;
                }
            }
            bool result = checklevel && checkunit && checkarea;
            if(result == false)
            {
                MessageBox.Show("Illegal schedule header, Please check if you include Name, Area and Level!");
            }
            return result;
        }

        public bool checkName(string unitName)
        {
            string[] sub = unitName.Split(' ','(');
            string name = sub[0];
            char[] chars = new char[] { 'S', 'A', 'B', 'C' };
            char first = name[0];
            string substring = name.Substring(1);
            double index;
            bool isNum = Double.TryParse(substring, out index);
            return chars.Contains(first) && isNum;
        }

        public int indexFromArray(string[] a, string key)
        {
            for (int i = 0; i < a.Length; i++)
            {
                string column_name = a[i];
                if (column_name.IndexOf(key, StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    return i;
                }

            }
            return -1;
        }
        public bool ScheduleDataParser(string filename)
        {
            StreamReader stream = File.OpenText(filename);

            string line;
            string[] a;
            string _name = null;
            string[] _info = null;
            int id = -1;
            
            while (null != (line = stream.ReadLine()))
            {
                a = line
                  .Split('\t')
                  .Select<string, string>(s => s.Trim('"'))
                  .ToArray();

                
                // First line of text file contains 
                // schedule name

                if (null == _name)
                {
                    _name = a[0];
                    continue;
                }

                // Second line of text file contains 
                // schedule column names

                if (null == _info)
                {
                    _info = a;
                    if (indexFromArray(_info, "Name") == -1)
                    {
                        MessageBox.Show("No Name information");
                        return false;
                    }

                    id = indexFromArray(_info, "Name");
                    continue;
                }

                // Remaining lines define schedula data
                try
                {
                    string unitName = a[id];
                    bool legal = checkName(unitName);
                    if (!legal)
                    {
                        string message = "Illegal room name: " + unitName + "\n Room name must be like: S(A,B,C)1.1";
                        message += "\n " + line;
                        MessageBox.Show(message);
                        return false;
                    }
                }
                catch(Exception ex)
                {
                    string problem = "There are some issues when parsing the schedule data! Export process terminated.";
                    MessageBox.Show(problem);
                }
                
            }
            return true;
        }
        public bool CheckViewSchedule(ViewSchedule vs, Document doc)
        {
            FilteredElementCollector col = new FilteredElementCollector(doc, vs.Id);
            foreach(Element element in col)
            {
                Room room = element as Room;
                if (room == null) continue;
                else
                {
                    string name = room.Name;
                    bool current = checkName(name);
                    if (!current)
                    {
                        string message = "Illegal room name: " + name + "\n Room name must be like: S(A,B,C)1.1";
                        MessageBox.Show(message);
                        return false;
                    }
                }
            }
            return true;
        }
        public void runPython(string name, string outDir, string project_name)
        {
            string folder = Constants.SCRIPT_FOLDER + "\\PROGRAMMING\\dist\\app";
            System.Diagnostics.Process p = new System.Diagnostics.Process();
            p.StartInfo.FileName = "cmd.exe";
            p.StartInfo.UseShellExecute = false;
            p.StartInfo.RedirectStandardInput = true;
            p.StartInfo.RedirectStandardOutput = true;
            p.StartInfo.RedirectStandardError = true;
            p.StartInfo.CreateNoWindow = true;
            p.StartInfo.WorkingDirectory = folder;
            p.Start();


            string matrix = string.Format("app.exe -i {0} -o {1} -n{2} -m{3}", '"' + name + '"', '"' + outDir + '"', '"' + project_name + '"', "UNITMATRIX");
            p.StandardInput.WriteLine(matrix);

            p.StandardInput.AutoFlush = true;
            p.StandardInput.WriteLine("exit");

            string output = p.StandardOutput.ReadToEnd();
            System.Windows.MessageBox.Show(output);


        }
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {

            UIApplication uiapp = commandData.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            Result rc = Result.Cancelled;
            var app = uiapp.Application;
            Document doc = uidoc.Document;
            ViewSchedule view = doc.ActiveView as ViewSchedule;
            string project = doc.ProjectInformation.Name;
            if (view == null)
            {
                MessageBox.Show("Please export in a schedule view");
                return Result.Cancelled;
            }
            
            string folder = Constants.SCRIPT_FOLDER + "\\PROGRAMMING";
            string datafolder = folder + "\\dist\\app\\data";
            //string testfile = datafolder + "\\RoomSchedule---20021_WhitinAve_v2020_detached_waldron7YAGT.txt";
            //bool testtest = ScheduleDataParser(testfile);
            if (!Directory.Exists(folder))
            {
                MessageBox.Show("Unable export Unit Matrix Excel Sheet because target directory cannot be found! \nPlease make sure you connect to the Arrowstreet VPN!");
                return Result.Cancelled;
            }
            if (!Check_Spec(view, doc))
            {
                MessageBox.Show("Schedule does not contain necessary Unit Matrix infromation!");
                return Result.Cancelled;
            };
            
            SaveFileDialog dlg = new SaveFileDialog();
            dlg.FileName = "UnitMatrix_" + doc.Title.Replace(" ", "_");
            dlg.DefaultExt = "xlsx";
            dlg.CheckFileExists = false;
            dlg.AddExtension = true;
            dlg.Filter = "Excel Files|*.xls;*.xlsx;*.xlsm"; ;
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                string filename = dlg.FileName;

                ViewScheduleExportOptions opt
                  = new ViewScheduleExportOptions();
                string type = view.Name.Replace(" ", "");
                type = Utils.ReplaceInvalidChars(type);
                string name = doc.Title.Replace(" ", "") + ".txt";
                name = Utils.ReplaceInvalidChars(name);
                name = type + "---" + name;
                view.Export(datafolder, name, opt);
                //name = "RoomSchedule---20021_WhitinAve_v2020_detached_waldron7YAGT.txt";
                bool toContinue = ScheduleDataParser(datafolder + "\\" + name);
                if (!toContinue) { return Result.Cancelled; }
                runPython(name, dlg.FileName, project);
                string logpath = folder + "\\dist\\app\\RESULT_FLAG.txt";
                if (File.ReadAllText(logpath) == "TRUE")
                {
                    rc = Result.Succeeded;
                    string finaloutput = "UNIT MATRIX EXCEL EXPORTED SUCCESSFULLY!" + "\n " + dlg.FileName;
                    MessageBox.Show(finaloutput);
                }
                else
                {
                    rc = Result.Failed;
                    string reasons = "Possible Reasons: \nArea plans are not well placed or missing";
                    string finaloutput = "THERE ARE SOME PROBLEMS WHEN PROCESSING THE EXCEL SHEET!" + "\n " + reasons;
                    MessageBox.Show(finaloutput);
                }
            }
            return rc;
        }

    }
    
    [Transaction(TransactionMode.Manual)]
    public class CmdDocakble : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            try {
                var app = commandData.Application;
                DockablePanelUtils.ShowDockablePanel(app);
            }
            catch {
                return Result.Failed;
            }

            return Result.Succeeded;
        }

        
    }
}