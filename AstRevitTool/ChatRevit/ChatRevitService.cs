using Autodesk.Revit.UI;
using CommunityToolkit.Mvvm.DependencyInjection;
using AstRevitTool.ChatRevit.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace AstRevitTool.ChatRevit
{
    public class ChatRevitService : IDockablePaneService
    {
        public static string PageGuid = "7A8F5162-67F6-4DFB-A3C7-E397292A7B38";

        public ChatRevitService()
        {
        }

        public FrameworkElement GetDockablePane()
        {
            var mainPage = Ioc.Default.GetService<ChatRevitPanel>();
            mainPage.DataContext = Ioc.Default.GetService<ChatRevitViewModel>();
            return mainPage;
        }

        public DockablePaneId GetDockablePaneId()
        {
            return new DockablePaneId(new Guid(PageGuid));
        }

        public string GetDockablePaneTitle()
        {
            return "ChatRevit";
        }

        public Type GetServiceType()
        {
            return typeof(ChatRevitService);
        }

        public void SetupDockablePane(DockablePaneProviderData data)
        {
            data.FrameworkElement = this.GetDockablePane();
            data.InitialState = new DockablePaneState
            {
                DockPosition = DockPosition.Bottom
            };
            data.VisibleByDefault = true;
            data.EditorInteraction = new EditorInteraction(EditorInteractionType.KeepAlive);
        }
    }
}
