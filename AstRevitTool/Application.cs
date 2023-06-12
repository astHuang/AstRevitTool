using AstRevitTool.Commands;
using Autodesk.Revit.UI;
using AutoUpdaterDotNET;
using AstRevitTool.Masterclass.Dockable;
using Autodesk.Revit.DB.Events;
using AstRevitTool.Views;
using AstRevitTool.Core.UnitMatrix;
using System.Windows.Media.Imaging;
//using AstRevitTool.ChatRevit.Interfaces;
//using AstRevitTool.ChatRevit;
//using AstRevitTool.ChatRevit.Service;
//using CommunityToolkit.Mvvm.DependencyInjection;
//using Microsoft.Extensions.DependencyInjection;

namespace AstRevitTool
{
    [UsedImplicitly]
    public class Application : IExternalApplication
    {
        private const string RibbonImageUri = "/AstRevitTool;component/Resources/Icons/icon-16.png";
        private const string RibbonLargeImageUri = "/AstRevitTool;component/Resources/Icons/icon-32.png";
        private const string icon_export = "/AstRevitTool;component/Resources/Icons/icon_dataexport-16.png";
        private const string icon_export_large = "/AstRevitTool;component/Resources/Icons/icon_dataexport-32.png";
        private const string icon_takeoff = "/AstRevitTool;component/Resources/Icons/icon_takeoff-16.png";
        private const string icon_takeoff_large = "/AstRevitTool;component/Resources/Icons/icon_takeoff-32.png";
        private const string icon_takeoff_dep = "/AstRevitTool;component/Resources/Icons/icon_takeoff_dep-16.png";
        private const string icon_takeoff_dep_large = "/AstRevitTool;component/Resources/Icons/icon_takeoff_dep-32.png";
        private const string icon_excel = "/AstRevitTool;component/Resources/Icons/icon_excel-16.png";
        private const string icon_excel_large = "/AstRevitTool;component/Resources/Icons/icon_excel-32.png";
        private const string icon_area = "/AstRevitTool;component/Resources/Icons/icon_manager-16.png";
        private const string icon_area_large = "/AstRevitTool;component/Resources/Icons/icon_manager-32.png";
        static private EventsReactor m_eventReactor;
        //static private IServiceCollection Services { get; } = new ServiceCollection();

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

        /*
        public void RegisterTypes()
        {
            Services.AddSingleton<IDockablePaneService, ChatRevitService>();
            Services.AddSingleton<IApplicationUI, AstRevitTool.ChatRevit.GPTUIManager>();
            Services.AddSingleton(typeof(ChatRevitPanel));
            Services.AddSingleton(typeof(ChatRevitViewModel));
            Ioc.Default.ConfigureServices(
                Services
                .BuildServiceProvider());
        }*/

        public static ASTRequestHandler ASTRequestHandler { get; set; }

        public static ExternalEventService GPTRequestHandler { get; set; }
        public static ExternalEvent ASTEvent { get; set; }
        public Result OnStartup(UIControlledApplication application)
        {
            AddMenu(application);
            /*
            Services
                .AddSingleton(application)
                .AddSingleton<IUIProvider, UIProvider>()
                .AddScoped<IDataContext, DataContext>()
                .AddSingleton<IUIServiceProvider, UIServiceProvider>()
                .AddSingleton<IExternalEventService, ExternalEventService>();

            RegisterTypes();*/
            /*
            var events = Ioc.Default.GetService<IEventManager>();
            if (events != null)
            {
                events.Subscribe();
            }*/
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
            /*
            var events = Ioc.Default.GetService<IEventManager>();
            if (events != null)
            {
                events.Unsubscribe();
            }*/

            return Result.Succeeded;
        }

        private void AddMenu(UIControlledApplication app)
        {
            //RibbonPanel rvtRibbonPanel = app.CreateRibbonPanel("AST Revit");
            //PulldownButtonData data = new PulldownButtonData("Options", "AST Revit");
            

            var panel = app.CreatePanel("AST Revit Toolkit", "Arrowstreet");

            var showButton1 = panel.AddPushButton<CmdCustomAnalysis>("Take-off Window");
            showButton1.SetImage(icon_takeoff);
            showButton1.SetLargeImage(icon_takeoff_large);

            PulldownButtonData data1 = new PulldownButtonData("Take-off", "Take-off\n(Deprecated)");
            var item1 = panel.AddItem(data1);
            var takeoffBtn = item1 as PulldownButton;
            takeoffBtn.SetImage(icon_takeoff_dep);
            takeoffBtn.SetLargeImage(icon_takeoff_dep_large);
            //takeoffBtn.AddPushButton(typeof(CmdCustomAnalysis), "Integrated Analysis");
            takeoffBtn.AddPushButton(typeof(CmdWWRCalc), "Window-to-wall Ratio");
            takeoffBtn.AddPushButton(typeof(CmdAssembly), "Exterior Types Take-off");
            //takeoffBtn.AddPushButton(typeof(CmdAssemblyMaterial), "Assembly Decomposition");
            takeoffBtn.AddPushButton(typeof(CmdMatCalc), "Material Take-off");
            //takeoffBtn.AddPushButton(typeof(CmdCustomAnalysis), "Integrated Analysis");

            ASTRequestHandler = new ASTRequestHandler();
            ASTEvent = ExternalEvent.Create(ASTRequestHandler);

            //TODO: uncomment after finilizing GPT plugin
            //GPTRequestHandler = new ExternalEventService();

            PulldownButtonData data2 = new PulldownButtonData("Excel Tables", "Excel Automation");
            var item2 = panel.AddItem(data2);
            var exportBtn = item2 as PulldownButton;
            exportBtn.SetImage(icon_excel);
            exportBtn.SetLargeImage(icon_excel_large);
            exportBtn.AddPushButton(typeof(CmdBOMA), "Generate standadard BOMA sheet");
            exportBtn.AddPushButton(typeof(CmdUnitMatrix), "Generate Unit Matrix sheet");

            PulldownButtonData data3 = new PulldownButtonData("Export data of this Revit model", "Data Export");
            var item3 = panel.AddItem(data3);
            var exchangeBtn = item3 as PulldownButton;
            exchangeBtn.SetImage(icon_export);
            exchangeBtn.SetLargeImage(icon_export_large);
            exchangeBtn.AddPushButton(typeof(CmdColladaExport), "Export 3D model");
            //exportBtn.AddPushButton(typeof(CmdUSDExport), "Export 3D model as GLTF");
            exchangeBtn.AddPushButton(typeof(CmdSvgExport), "Export BOMA JSON");
            exchangeBtn.AddPushButton(typeof(CmdSvgBatchExport), "Export AreaPlan SVG");

            PulldownButtonData data4 = new PulldownButtonData("Room and Area assistant (under development)", "Room Manager");
            var item4 = panel.AddItem(data4);
            var areaBtn = item4 as PulldownButton;
            areaBtn.SetImage(icon_area);
            areaBtn.SetLargeImage(icon_area_large);
            //areaBtn.AddPushButton(typeof(CmdCustomAreaScheduleExport), "Export Area Scheme");
            areaBtn.AddPushButton(typeof(CmdRoomSchedule), "Room manager");
            var plan = areaBtn.AddPushButton<CmdAreaPlan>("AreaPlan helper");
            plan.ToolTip = "Help build an area plan from rooms";
            plan.LongDescription = "Using rooms either in the current model or a linked model to help create areas to meet your BOMA or other standard. ";
            //plan.SetImage(RibbonImageUri);
            //plan.SetLargeImage(RibbonLargeImageUri);


            var showButton = panel.AddPushButton<CmdUpdater>("Update Check");
            showButton.SetImage(RibbonImageUri);
            showButton.SetLargeImage(RibbonLargeImageUri);

            //var plan = panel.AddPushButton<CmdAreaPlan>("AreaPlan Helper");
            

            //var dockableButton = panel.AddPushButton<CmdDocakble>("Dockable panel");
            //dockableButton.SetImage(RibbonImageUri);
            //dockableButton.SetLargeImage(RibbonLargeImageUri);

            //DockablePanelUtils.RegisterDockablePanel(app);
            app.ControlledApplication.DocumentChanged += OnDocumentChanged;

        }

        private System.Windows.Media.ImageSource getImage(string imageFile)
        {
            try
            {
                System.IO.Stream stream = this.GetType().Assembly.GetManifestResourceStream(imageFile);
                if (stream == null) return null;
                PngBitmapDecoder pngDecoder = new PngBitmapDecoder(stream, BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.Default);
                return pngDecoder.Frames[0];

            }
            catch
            {
                return null; // no image


            }
        }

        private void OnDocumentChanged(object sender, DocumentChangedEventArgs e)
        {
            var Document = e.GetDocument();

        }
    }
}