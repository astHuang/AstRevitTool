using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB;
using System.Windows.Forms;

namespace AstRevitTool.Core.Analysis
{
    public class Assembly_Analysis:IAnalysis
    {
        public Dictionary<string, HashSet<Element>> AnalyzedElements { get; set; } = new Dictionary<string, HashSet<Element>>();
        //MaterialCategoryBook - Material area List by category
        public Dictionary<string, Dictionary<string, double>> MaterialCategoryBook { get; set; } = new Dictionary<string, Dictionary<string, double>>();

        private ElementsVisibleInViewExportContext Context;
        private Autodesk.Revit.ApplicationServices.Application App;

        private Dictionary<string, double> subTotals = new Dictionary<string, double>();
        public virtual Dictionary<string, Tuple<string,double>> Quantities { get; set; } = new Dictionary<string, Tuple<string, double>>();
        public List<string> SpandrelMaterialsKeyword { get; set; } = new List<string>();
        public Autodesk.Revit.DB.View ActiveView { get; set; }

        private ICollection<Element> MyElements = new List<Element>();

        private List<FilteredInfo> MyInfo = new List<FilteredInfo>();
        public Assembly_Analysis(ElementsVisibleInViewExportContext context, Autodesk.Revit.ApplicationServices.Application app)
        {
            Context = context;
            App = app;
            ActiveView = context.MainDoc.ActiveView;
            AnalyzedElements.Add("Basic Walls", new HashSet<Element>());
            AnalyzedElements.Add("Curtain Walls", new HashSet<Element>());
            AnalyzedElements.Add("Curtain Panels", new HashSet<Element>());
            AnalyzedElements.Add("Floors", new HashSet<Element>());
            AnalyzedElements.Add("Roofs", new HashSet<Element>());
            AnalyzedElements.Add("Doors", new HashSet<Element>());
            AnalyzedElements.Add("Windows", new HashSet<Element>());
            AnalyzedElements.Add("Ceilings", new HashSet<Element>());
            SpandrelMaterialsKeyword.Add("Spandrel");
            SpandrelMaterialsKeyword.Add("ShadowBox");
        }
        public void Extraction() {
            List<Element> allwalls = new List<Element>();
            List<ElementId> hosts = new List<ElementId>();
            foreach (Document d in App.Documents)
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
                foreach (Element e in roofs)
                {
                    if (Context.get_ElementVisible(d, e.Id))
                    {
                        AnalyzedElements["Roofs"].Add(e);
                    }
                }
            }
        }

        public ICollection<Element> AllAnalyzedElement()
        {
            ICollection<Element> elements = new List<Element>();
            foreach (KeyValuePair<string, HashSet<Element>> es in this.AnalyzedElements)
            {
                foreach (Element e in es.Value)
                {
                    elements.Add(e);
                }
            }
            return elements;
        }

        #region Analyze Methods
        private void AnalyzeCompoundStructure(string keyword)
        {
            double total = 0.0;
            foreach (Element ele in this.AnalyzedElements[keyword])
            {
                Document doc = ele.Document;
                ElementId typeId = ele.GetTypeId();
                string TName = keyword + ": " + doc.GetElement(typeId)?.Name;
                string TMark = doc.GetElement(typeId)?.get_Parameter(BuiltInParameter.ALL_MODEL_TYPE_MARK)?.AsString();
                if (null == TMark || "" == TMark) TMark = "Type Mark Missing!";
                TName += " // " + TMark;
                try
                {
                    double area = 0.0;
                    if (keyword == "Windows" || keyword == "Doors")
                    {
                        FamilyInstance fInstance = ele as FamilyInstance;
                        area = AnalysisUtils.GetInstanceSurfaceAreaMetric(fInstance);
                    }
                    else 
                    {
                        //area = ele.LookupParameter("Area").AsDouble();
                        area = AnalysisUtils.ElementArea(ele);
                    }
                    total += area;
                    if (Quantities.Keys.Contains(TName))
                    {
                        double sumarea = Quantities[TName].Item2 + area;
                        FilteredInfo.matchInfoFromList(this.MyInfo, TName).Area += area;
                        FilteredInfo.matchInfoFromList(this.MyInfo, TName).FilteredElements.Add(ele);
                        Quantities[TName] = new Tuple<string, double>(TMark, sumarea);

                    }
                    else
                    {
                        Quantities.Add(TName, new Tuple<string, double>(TMark,area));
                        List<Element> init = new List<Element>();
                        init.Add(ele);
                        this.MyInfo.Add(new FilteredInfo(TName, area, init));
                    }

                }
                catch
                {
                    continue;
                }
            }
            if(total > 0)
            {
                this.subTotals.Add(keyword, total);
            }
        }

        public void AnalyzeCurtainWalls()
        {
            double totalarea = 0.0;
            Quantities.Add("Curtain Wall: Spandrel", new Tuple<string, double>("Spandrel Curtainwall", 0.0));
            Quantities.Add("Curtain Wall: Glazing", new Tuple<string, double>("Vision Glazing", 0.0));
            this.MyInfo.Add(new FilteredInfo("Curtain Wall: Spandrel", 0.0));
            this.MyInfo.Add(new FilteredInfo("Curtain Wall: Glazing", 0.0));
            foreach (Element cpanels in this.AnalyzedElements["Curtain Walls"])
            {
                try
                {
                    double cw_area = 0.0;
                    cw_area = cpanels.LookupParameter("Area").AsDouble();
                    totalarea += cw_area;                   
                }
                catch
                {
                    continue;
                }
            }

            foreach (Element cpanel in this.AnalyzedElements["Curtain Panels"])
            {
                try {
                    Document doc = cpanel.Document;
                    ElementId pTypeId = cpanel.GetTypeId();
                    PanelType panelType = doc.GetElement(pTypeId) as PanelType;
                    FamilySymbol panelSymbol = doc.GetElement(pTypeId) as FamilySymbol;
                    if (null != panelType)
                    {
                        ElementId id = panelType.get_Parameter(BuiltInParameter.MATERIAL_ID_PARAM)?.AsElementId();
                        if (id != null)
                        {
                            Material mat = doc.GetElement(id) as Material;
                            if (mat.Transparency < 5 || SpandrelMaterialsKeyword.Any(mat.Name.Contains))
                            {
                                double sumarea = cpanel.LookupParameter("Area").AsDouble() + Quantities["Curtain Wall: Spandrel"].Item2;
                                Quantities["Curtain Wall: Spandrel"] = new Tuple<string, double>("Spandrel Curtainwall", sumarea);
                                FilteredInfo.matchInfoFromList(this.MyInfo, "Curtain Wall: Spandrel").Area += cpanel.LookupParameter("Area").AsDouble();
                                FilteredInfo.matchInfoFromList(this.MyInfo, "Curtain Wall: Spandrel").FilteredElements.Add(cpanel);
                            }
                            else {
                                FilteredInfo.matchInfoFromList(this.MyInfo, "Curtain Wall: Glazing").Area += cpanel.LookupParameter("Area").AsDouble();
                                FilteredInfo.matchInfoFromList(this.MyInfo, "Curtain Wall: Glazing").FilteredElements.Add(cpanel);
                            }
                        }

                    }
                    else if (null != panelSymbol)
                    {
                        Options option = new Options();
                        option.View = this.ActiveView;
                        option.ComputeReferences = true;
                        GeometryElement geoEl = cpanel.get_Geometry(option);
                        add_spandrel_area(cpanel,geoEl, SpandrelMaterialsKeyword, doc);
                    }
                }
                catch
                {
                    continue;
                }
            }

            foreach (Document d in App.Documents)
            {
                FilteredElementCollector doors = new FilteredElementCollector(d).OfCategory(BuiltInCategory.OST_Doors);
                foreach (Element door in doors)
                {
                    if (Context.get_ElementVisible(d, door.Id))
                    {
                        try
                        {
                            ElementId dTypeId = door.GetTypeId();
                            FamilyInstance fInstance = door as FamilyInstance;
                            FamilySymbol FType = fInstance?.Symbol;
                            string FName = FType?.FamilyName;
                            if (FName.Contains("Storefront"))
                            {
                                totalarea -= AnalysisUtils.GetInstanceSurfaceAreaMetric(fInstance);
                            }
                        }
                        catch
                        {
                            continue;
                        }

                    }

                }
            }

            Quantities["Curtain Wall: Glazing"] = new Tuple<string, double>("Vision Glazing", totalarea - Quantities["Curtain Wall: Spandrel"].Item2);
            if (totalarea > 0)
            {
                this.subTotals.Add("Curtain Wall", totalarea);
            }
        }

        private void add_spandrel_area(Element ele,GeometryElement geo, List<string> cands, Document doc)
        {
            foreach (GeometryObject o in geo)
            {
                if (o is Solid)
                {
                    double area = 0.0;
                    Solid solid = o as Solid;
                    foreach (Face face in solid.Faces)
                    {
                        if (null == doc.GetElement(face.MaterialElementId)) continue;
                        bool spandrel = cands.Any(doc.GetElement(face.MaterialElementId).Name.Contains) && face.Area > area;
                        area = spandrel ? face.Area : area;
                    }
                    double sumarea = Quantities["Curtain Wall: Spandrel"].Item2 + area;
                    if (area > 0)
                    {
                        FilteredInfo.matchInfoFromList(this.MyInfo, "Curtain Wall: Spandrel").Area += area;
                        FilteredInfo.matchInfoFromList(this.MyInfo, "Curtain Wall: Spandrel").FilteredElements.Add(ele);
                    }
                    else
                    {
                        FilteredInfo.matchInfoFromList(this.MyInfo, "Curtain Wall: Glazing").Area += area;
                        FilteredInfo.matchInfoFromList(this.MyInfo, "Curtain Wall: Glazing").FilteredElements.Add(ele);
                    }
                    Quantities["Curtain Wall: Spandrel"] = new Tuple<string, double>("Spandrel Curtainwall", sumarea);
                }
                else if (o is GeometryInstance)
                {
                    GeometryInstance i = o as GeometryInstance;
                    add_spandrel_area(ele,i.GetInstanceGeometry(i.Transform), cands, doc);
                }
            }
        }
        #endregion

        public virtual void Analyze() {
            Extraction();
            AnalyzeCompoundStructure("Basic Walls");
            AnalyzeCompoundStructure("Roofs");
            AnalyzeCompoundStructure("Floors");
            AnalyzeCompoundStructure("Ceilings");
            AnalyzeCompoundStructure("Windows");
            AnalyzeCompoundStructure("Doors");
            AnalyzeCurtainWalls();
        }

        public virtual string Report() {
            string str = "";
            foreach (KeyValuePair<string, Tuple<string,double>> entry in this.Quantities)
            {
                if (entry.Value.Item2 != 0.0)
                {
                    str += "\n" + entry.Key + "/ " + entry.Value.Item1 + "/ " + entry.Value.Item2.ToString("0.##");
                }
            }
            return str;
        }

        public virtual string Type()
        {
            return "Assembly Type Analysis";
        }

        public virtual string Conclusion()
        {
            string con = "";
            con += "Exterior type take-off\n\n";
            foreach(KeyValuePair<string, double> entry in this.subTotals)
            {
                con+="\n - Total " + entry.Key + " Area: " + entry.Value.ToString("0.###");
            }
            return con;
        }

        public virtual Dictionary<string,double> ResultList()
        {
            Dictionary<string, double> result = new Dictionary<string, double>();
            foreach(KeyValuePair<string, Tuple<string, double>> entry in this.Quantities)
            {
                if(entry.Value.Item2 != 0.0)
                {
                    result.Add(entry.Key, entry.Value.Item2);
                }
            }
            return result;
        }

        public virtual List<FilteredInfo> InfoList()
        {
            return this.MyInfo;
        }
    }
}
