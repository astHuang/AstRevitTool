using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using AstRevitTool.ChatRevit.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.DependencyInjection;
using System.Reflection;

namespace AstRevitTool.ChatRevit.Service
{
    public class DataContext : IDataContext
    {
        private IUIServiceProvider _uiServiceProvider;

        public DataContext(IUIServiceProvider uiServiceProvider)
        {
            this._uiServiceProvider = uiServiceProvider;
        }
        public Document GetDocument()
        {
            UIDocument activeUIDocument = this.GetUIApplication().ActiveUIDocument;
            return (activeUIDocument != null) ? activeUIDocument.Document : null;
        }

        public UIApplication GetUIApplication()
        {
            UIControlledApplication uiapp = this._uiServiceProvider.GetUIApplication();
            MethodInfo method = uiapp.GetType().GetMethod("getUIApplication", BindingFlags.Instance | BindingFlags.NonPublic);
            return ((method != null) ? method.Invoke(uiapp, null) : null) as UIApplication;
        }

        public UIDocument GetUIDocument()
        {
            return new UIDocument(GetDocument());
        }

        public void Regenerate()
        {

        }
    }
}
