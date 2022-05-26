using AstRevitTool.Commands;
using Autodesk.Revit.UI;
using AutoUpdaterDotNET;

namespace AstRevitTool
{
    [UsedImplicitly]
    public class Application : IExternalApplication
    {
        private const string RibbonImageUri = "/AstRevitTool;component/Resources/Icons/icon-16.png";
        private const string RibbonLargeImageUri = "/AstRevitTool;component/Resources/Icons/icon-32.png";

        public Result OnStartup(UIControlledApplication application)
        {
            AddMenu(application);

            return Result.Succeeded;
        }

        public Result OnShutdown(UIControlledApplication application)
        {
            return Result.Succeeded;
        }

        private void AddMenu(UIControlledApplication app)
        {
            RibbonPanel rvtRibbonPanel = app.CreateRibbonPanel("AST Revit");
            PulldownButtonData data = new PulldownButtonData("Options", "AST Revit");

            RibbonItem item = rvtRibbonPanel.AddItem(data);
            PulldownButton optionsBtn = item as PulldownButton;
            // Add Icons to main Menu
            optionsBtn.SetImage(RibbonImageUri);
            optionsBtn.SetLargeImage(RibbonLargeImageUri);

            //optionsBtn.AddPushButton(new PushButtonData("AST Welcome", "Welcome to AST Revit Toolkits!", ExecutingAssemblyPath, "AST_Revit_Toolkit.HelloWorld"));
            optionsBtn.AddPushButton(typeof(CmdWWRCalc), "Window-to-wall Ratio Calculation");
            //optionsBtn.AddPushButton(typeof(CmdWallMat), "Basic Wall Material Take-off");
            //optionsBtn.AddPushButton(new PushButtonData("Wall Types", "Summarize Facade by Types", ExecutingAssemblyPath, "AST_Revit_Toolkit.CmdWallType"));
            optionsBtn.AddPushButton(typeof(CmdWallFamily), "Facade Family Take-off");
            optionsBtn.AddPushButton(typeof(CmdAssembly), "Exterior Types Take-off");
            optionsBtn.AddPushButton(typeof(CmdAssemblyMaterial), "Assembly Material Decomposition");
            optionsBtn.AddPushButton(typeof(CmdMatCalc), "Detailed Material Take-off");
            optionsBtn.AddPushButton(typeof(CmdUpdater), "Check Latest Update");
            optionsBtn.AddPushButton(typeof(CmdLifeCycle), "Beta UI of Life Cycle Analysis");
            //optionsBtn.AddPushButton(typeof(CmdBOMA), "BOMA tool");
            //optionsBtn.AddPushButton(new PushButtonData("Table Display", "Testing UI Form...", ExecutingAssemblyPath, "AST_Revit_Toolkit.CmdUIDisplay"));
        }
    }
}