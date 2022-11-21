using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.UI;

namespace AstRevitTool.Views
{
    public class ASTRequestHandler : IExternalEventHandler
    {
        public RequestId Request { get; set; }
        public enum RequestId
        {
            None,
            Coloring
        }
        public void Execute(UIApplication app)
        {
            
        }

        public string GetName()
        {
            return "AST Color Element Request Handler";
        }
    }
}
