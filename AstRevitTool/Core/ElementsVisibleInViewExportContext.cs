using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB;

namespace AstRevitTool.Core
{
    public class ElementsVisibleInViewExportContext : IExportContext
    {
        private Stack<Document> Documents = new Stack<Document>();

        public Dictionary<string, HashSet<ElementId>> Elements { get; set; } = new Dictionary<string, HashSet<ElementId>>();

        public bool get_ElementVisible(Document doc, ElementId id)
        {
            var ids = default(HashSet<ElementId>);
            if (Elements.TryGetValue(doc.PathName, out ids))
            {
                if (ids.Contains(id))
                {
                    return true;
                }
            }

            return false;
        }

        public ElementsVisibleInViewExportContext(Document mainDocument)
        {
            Documents.Push(mainDocument);
            Elements.Add(mainDocument.PathName, new HashSet<ElementId>());
        }

        public bool Start()
        {
            return true;
        }

        public void Finish()
        {

            // Nothing.

        }

        public Autodesk.Revit.DB.RenderNodeAction OnViewBegin(Autodesk.Revit.DB.ViewNode node)
        {
            return RenderNodeAction.Proceed;
        }

        public void OnViewEnd(Autodesk.Revit.DB.ElementId elementId)
        {

            // Nothing.

        }

        public Autodesk.Revit.DB.RenderNodeAction OnLinkBegin(Autodesk.Revit.DB.LinkNode node)
        {
            var doc = node.GetDocument();
            Documents.Push(doc);
            if (!Elements.ContainsKey(doc.PathName))
                Elements.Add(doc.PathName, new HashSet<ElementId>());
            return RenderNodeAction.Proceed;
        }

        public void OnLinkEnd(Autodesk.Revit.DB.LinkNode node)
        {
            var doc = Documents.Pop();
        }

        public Autodesk.Revit.DB.RenderNodeAction OnElementBegin(Autodesk.Revit.DB.ElementId elementId)
        {
            this.Elements[Documents.Peek().PathName].Add(elementId);
            return RenderNodeAction.Proceed;
        }

        public void OnElementEnd(Autodesk.Revit.DB.ElementId elementId)
        {

            // Nothing.

        }

        public Autodesk.Revit.DB.RenderNodeAction OnInstanceBegin(Autodesk.Revit.DB.InstanceNode node)
        {
            return RenderNodeAction.Skip;
        }

        public void OnInstanceEnd(Autodesk.Revit.DB.InstanceNode node)
        {

            // Nothing.

        }

        public Autodesk.Revit.DB.RenderNodeAction OnFaceBegin(Autodesk.Revit.DB.FaceNode node)
        {
            return RenderNodeAction.Skip;
        }

        public void OnFaceEnd(Autodesk.Revit.DB.FaceNode node)
        {

            // Nothing.

        }

        public void OnMaterial(Autodesk.Revit.DB.MaterialNode node)
        {

            // Nothing.

        }

        public void OnPolymesh(Autodesk.Revit.DB.PolymeshTopology node)
        {

            // Nothing.

        }

        public void OnRPC(Autodesk.Revit.DB.RPCNode node)
        {

            // Nothing.

        }

        public void OnLight(Autodesk.Revit.DB.LightNode node)
        {

            // Nothing.

        }

        public bool IsCanceled()
        {
            return false;
        }
    }
}

