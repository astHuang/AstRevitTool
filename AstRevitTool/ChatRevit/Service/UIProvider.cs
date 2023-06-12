using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using AstRevitTool.ChatRevit.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace AstRevitTool.ChatRevit.Service
{
    public class UIProvider : IUIProvider
    {
        private UIControlledApplication _application;

        public UIProvider(UIControlledApplication application)
        {
            _application = application;
        }

        public AddInId GetAddInId()
        {
            return GetUIApplication().ActiveAddInId;
        }

        public ControlledApplication GetApplication()
        {
            return GetUIApplication().ControlledApplication;
        }

        public UIControlledApplication GetUIApplication()
        {
            return _application;
        }

        public IntPtr GetWindowHandle()
        {
            return GetUIApplication().MainWindowHandle;
        }

    }
}
