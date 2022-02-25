using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB;
using System.Windows.Forms;


namespace AstRevitTool.Core.Analysis
{
    public abstract class Facade_Analysis:IAnalysis
    {
        public Dictionary<string, HashSet<Element>> AnalyzedElements { get; set; } = new Dictionary<string, HashSet<Element>>();

        public Dictionary<string, double> Metrics { get; set; } = new Dictionary<string, double>();

        private ElementsVisibleInViewExportContext Context;
        private Autodesk.Revit.ApplicationServices.Application App;

        public Facade_Analysis(ElementsVisibleInViewExportContext context, Autodesk.Revit.ApplicationServices.Application app)
        {
            Context = context;
            App = app;
            AnalyzedElements.Add("Doors", new HashSet<Element>());
            AnalyzedElements.Add("Windows", new HashSet<Element>());
            AnalyzedElements.Add("Basic Walls", new HashSet<Element>());
            AnalyzedElements.Add("Curtain Walls", new HashSet<Element>());
            AnalyzedElements.Add("Curtain Panels", new HashSet<Element>());
            AnalyzedElements.Add("Floors", new HashSet<Element>());
            AnalyzedElements.Add("Roofs", new HashSet<Element>());

        }
        public void Extraction() {
            List<Element> allwalls = new List<Element>();
            List<ElementId> hosts = new List<ElementId>();
            foreach(Document d in App.Documents)
            {
                IList<Element> walls = new FilteredElementCollector(d).OfClass(typeof(Wall)).WhereElementIsNotElementType().ToElements();
                IList<Element> cpanels = new FilteredElementCollector(d).OfCategory(BuiltInCategory.OST_CurtainWallPanels).WhereElementIsNotElementType().ToElements();
                FilteredElementCollector windows = new FilteredElementCollector(d).OfCategory(BuiltInCategory.OST_Windows);
                FilteredElementCollector doors = new FilteredElementCollector(d).OfCategory(BuiltInCategory.OST_Doors);
                IList<Element> floors = new FilteredElementCollector(d).OfClass(typeof(Floor)).WhereElementIsNotElementType().ToElements();
                IList<Element> roofs = new FilteredElementCollector(d).OfClass(typeof(RoofBase)).WhereElementIsNotElementType().ToElements();
                foreach (Element e in walls)
                {
                    if (Context.get_ElementVisible(d, e.Id))
                    {
                        allwalls.Add(e);
                    }
                }

                foreach (Element e in cpanels)
                {
                    if (Context.get_ElementVisible(d, e.Id))
                    {
                        FamilyInstance fi = e as FamilyInstance;
                        if (null != fi)
                        {
                            hosts.Add(fi.Host.Id);
                        }


                        AnalyzedElements["Curtain Panels"].Add(e);
                    }
                }

                foreach (Element e in windows)
                {
                    if (Context.get_ElementVisible(d, e.Id))
                    {
                        AnalyzedElements["Windows"].Add(e);
                    }

                }

                foreach (Element e in doors)
                {
                    if (Context.get_ElementVisible(d, e.Id))
                    {
                        AnalyzedElements["Doors"].Add(e);
                    }

                }

                //Distinguish between curtain walls and solid walls
                foreach (Element wall in allwalls)
                {
                    if (hosts.Contains(wall.Id))
                    {
                        AnalyzedElements["Curtain Walls"].Add(wall);
                    }
                    else
                    {
                        AnalyzedElements["Basic Walls"].Add(wall);
                    }
                }

                foreach (Element e in floors)
                {
                    if (Context.get_ElementVisible(d, e.Id))
                    {
                        AnalyzedElements["Floors"].Add(e);
                    }
                }

                foreach (Element e in roofs)
                {
                    if (Context.get_ElementVisible(d, e.Id))
                    {
                        AnalyzedElements["Roofs"].Add(e);
                    }
                }
            }
        }

        public abstract void AnalyzeBasicWalls();

        public abstract void AnalyzeCurtainWalls();

        public abstract void AnalyzeDoors();

        public abstract void AnalyzeWindows();

        public virtual void Analyze() {
            Extraction();
        }

        public virtual string Report() {
            string str = "";
            foreach (KeyValuePair<string, double> entry in this.Metrics)
            {
                str += "\n" + entry.Key + ": " + entry.Value.ToString("0.##");
            }
            return str;
        }

        public virtual string Type()
        {
            return "Facade Analysis";
        }

        public virtual string Conclusion()
        {
            return "";
        }
        public virtual Dictionary<string, double> ResultList()
        {
            return new Dictionary<string, double>();
        }
    }
}
