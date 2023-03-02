using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB;

namespace AstRevitTool.Masterclass.Dockable
{
    public class ElementWrapper
    {
        public string FamilyName { get; set; }

        public string FamilyType { get; set; }

        public ElementWrapper(Element e)
        {
            var doc = e.Document;
            if(!(doc.GetElement(e.GetTypeId()) is FamilySymbol type))return;

            FamilyName = type.FamilyName;
        }
    }
}
