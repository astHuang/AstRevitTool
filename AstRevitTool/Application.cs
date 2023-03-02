using AstRevitTool.Commands;
using Autodesk.Revit.UI;
using AutoUpdaterDotNET;
using AstRevitTool.Masterclass.Dockable;
using Autodesk.Revit.DB.Events;
using AstRevitTool.Views;
using AstRevitTool.Core.UnitMatrix;

namespace AstRevitTool
{
    [UsedImplicitly]
    public class Application : IExternalApplication
    {
        private const string RibbonImageUri = "/AstRevitTool;component/Resources/Icons/icon-16.png";
        private const string RibbonLargeImageUri = "/AstRevitTool;component/Resources/Icons/icon-32.png";

        static private EventsReactor m_eventReactor;

        public static EventsReactor EventReactor
        {
            get
            {
                if (null == m_eventReactor)
                {
                    throw new ArgumentException("External application was not loaded yet, please make sure you register external application by correct full path of dll.", "EventReactor");
                }
                else
                {
                    return Application.m_eventReactor;
                }
            }
        }

        public static ASTRequestHandler ASTRequestHandler { get; set; }
        public static ExternalEvent ASTEvent { get; set; }
        public Result OnStartup(UIControlledApplication application)
        {
            AddMenu(application);
            string assemblyName = this.GetType().Assembly.Location;
            m_eventReactor = new EventsReactor(assemblyName.Replace(".dll", ".log"));
            //
            // subscribe events
            application.ControlledApplication.DocumentSaving += new EventHandler<Autodesk.Revit.DB.Events.DocumentSavingEventArgs>(EventReactor.DocumentSaving);
            application.ControlledApplication.DocumentSavingAs += new EventHandler<Autodesk.Revit.DB.Events.DocumentSavingAsEventArgs>(EventReactor.DocumentSavingAs);
            application.ControlledApplication.DocumentClosed += new EventHandler<Autodesk.Revit.DB.Events.DocumentClosedEventArgs>(EventReactor.DocumentClosed);
            return Result.Succeeded;
        }

        public Result OnShutdown(UIControlledApplication application)
        {
            m_eventReactor.Dispose();
            application.ControlledApplication.DocumentSaving -= new EventHandler<Autodesk.Revit.DB.Events.DocumentSavingEventArgs>(EventReactor.DocumentSaving);
            application.ControlledApplication.DocumentSavingAs -= new EventHandler<Autodesk.Revit.DB.Events.DocumentSavingAsEventArgs>(EventReactor.DocumentSavingAs);
            application.ControlledApplication.DocumentClosed -= new EventHandler<Autodesk.Revit.DB.Events.DocumentClosedEventArgs>(EventReactor.DocumentClosed);
            return Result.Succeeded;
        }

        private void AddMenu(UIControlledApplication app)
        {
            //RibbonPanel rvtRibbonPanel = app.CreateRibbonPanel("AST Revit");
            //PulldownButtonData data = new PulldownButtonData("Options", "AST Revit");


            var panel = app.CreatePanel("AST Revit Toolkit", "Arrowstreet");
            PulldownButtonData data1 = new PulldownButtonData("Take-off", "Take-off Tools");
            var item1 = panel.AddItem(data1);
            var takeoffBtn = item1 as PulldownButton;
            takeoffBtn.SetImage(RibbonImageUri);
            takeoffBtn.SetLargeImage(RibbonLargeImageUri);
            takeoffBtn.AddPushButton(typeof(CmdWWRCalc), "Window-to-wall Ratio");
            takeoffBtn.AddPushButton(typeof(CmdAssembly), "Exterior Types Take-off");
            takeoffBtn.AddPushButton(typeof(CmdAssemblyMaterial), "Assembly Decomposition");
            takeoffBtn.AddPushButton(typeof(CmdMatCalc), "Material Take-off");
            takeoffBtn.AddPushButton(typeof(CmdCustomAnalysis), "Custom Analysis");

            ASTRequestHandler = new ASTRequestHandler();
            ASTEvent = ExternalEvent.Create(ASTRequestHandler);

            PulldownButtonData data2 = new PulldownButtonData("Export", "Exporting Tools");
            var item2 = panel.AddItem(data2);
            var exportBtn = item2 as PulldownButton;
            exportBtn.SetImage(RibbonImageUri);
            exportBtn.SetLargeImage(RibbonLargeImageUri);
            exportBtn.AddPushButton(typeof(CmdColladaExport), "Export 3D model for Cumulus");
            //exportBtn.AddPushButton(typeof(CmdUSDExport), "Export 3D model as GLTF");
            exportBtn.AddPushButton(typeof(CmdSvgExport), "Export BOMA schedule with floorplans");
            exportBtn.AddPushButton(typeof(CmdBOMA), "Export BOMA standadard Excel Sheet");
            //exportBtn.AddPushButton(typeof(CmdUnitMatrix), "Export Unit Matrix Excel Sheet");
            exportBtn.AddPushButton(typeof(CmdRoomSchedule), "Room and unix matrix manager");
            var showButton = panel.AddPushButton<CmdUpdater>("Update Check");
            showButton.SetImage(RibbonImageUri);
            showButton.SetLargeImage(RibbonLargeImageUri);

            //var dockableButton = panel.AddPushButton<CmdDocakble>("Dockable panel");
            //showButton.SetImage(RibbonImageUri);
            //showButton.SetLargeImage(RibbonLargeImageUri);

            //DockablePanelUtils.RegisterDockablePanel(app);
            //app.ControlledApplication.DocumentChanged += OnDocumentChanged;

            /*
            RibbonItem item = rvtRibbonPanel.AddItem(data);
            PulldownButton optionsBtn = item as PulldownButton;
            // Add Icons to main Menu
            optionsBtn.SetImage(RibbonImageUri);
            optionsBtn.SetLargeImage(RibbonLargeImageUri);

            //optionsBtn.AddPushButton(new PushButtonData("AST Welcome", "Welcome to AST Revit Toolkits!", ExecutingAssemblyPath, "AST_Revit_Toolkit.HelloWorld"));
            optionsBtn.AddPushButton(typeof(CmdWWRCalc), "Window-to-wall Ratio Calculation");
            //optionsBtn.AddPushButton(typeof(CmdWallMat), "Basic Wall Material Take-off");
            //optionsBtn.AddPushButton(new PushButtonData("Wall Types", "Summarize Facade by Types", ExecutingAssemblyPath, "AST_Revit_Toolkit.CmdWallType"));
            //optionsBtn.AddPushButton(typeof(CmdWallFamily), "Facade Family Take-off");
            optionsBtn.AddPushButton(typeof(CmdAssembly), "Exterior Types Take-off");
            optionsBtn.AddPushButton(typeof(CmdAssemblyMaterial), "Assembly Decomposition");
            optionsBtn.AddPushButton(typeof(CmdMatCalc), "Material Take-off");
            optionsBtn.AddPushButton(typeof(CmdSvgExport), "Export room plan Svg");
            optionsBtn.AddPushButton(typeof(CmdColladaExport), "Export model as COLLADA");
            optionsBtn.AddPushButton(typeof(CmdUpdater), "Check Latest Update");
            //optionsBtn.AddPushButton(typeof(CmdLifeCycle), "Beta UI of Life Cycle Analysis");
            //optionsBtn.AddPushButton(typeof(CmdBOMA), "BOMA tool");
            //optionsBtn.AddPushButton(new PushButtonData("Table Display", "Testing UI Form...", ExecutingAssemblyPath, "AST_Revit_Toolkit.CmdUIDisplay"));*/
        }

        private void OnDocumentChanged(object sender, DocumentChangedEventArgs e)
        {
            var Document = e.GetDocument();

        }
    }
}