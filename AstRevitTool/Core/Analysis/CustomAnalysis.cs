using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Microsoft.Office.Interop.Excel;

namespace AstRevitTool.Core.Analysis
{
    public class CustomAnalysis : IAnalysis
    {
        public Dictionary<BuiltInCategory, HashSet<Element>> AnalyzedElements { get; set; } = new Dictionary<BuiltInCategory, HashSet<Element>>();
        public Dictionary<BuiltInCategory, List<FilteredData>> AnalyzedData { get; set; } = new Dictionary<BuiltInCategory, List<FilteredData>>();

        public List<FilteredInfo> AnalyzedMaterial { get; set; } = new List<FilteredInfo>();

        public List<FilteredMaterial> AllMaterial { get; set; } = new List<FilteredMaterial>();
        public class DataTypes : List<DataType>
        {
            public DataTypes(Dictionary<BuiltInCategory, List<FilteredData>> analyzedData)
            {
                foreach(KeyValuePair<BuiltInCategory, List<FilteredData>> item in analyzedData)
                {
                    if (item.Value.Count == 0) continue;
                    DataType type = new DataType(item);
                    this.Add(type);
                }

            }
            public string Name { get; set; }
        }

        public class DataType
        {
            public DataType(KeyValuePair<BuiltInCategory, List<FilteredData>> dataItem)
            {
                if (dataItem.Value.Count == 0) return;
                Category = dataItem.Value[0].Category;
                CategoryName = Category.Name;
                filteredData = dataItem.Value;
                Doc = dataItem.Value[0].PrimaryDoc;
                BIC = dataItem.Key;
            }
            public Document Doc { get; set; }
            public Category Category { get; set; }
            public string CategoryName { get; set; }

            public BuiltInCategory BIC { get; set; }
            public List<FilteredData> filteredData { get; set; }
            public double GetArea()
            {
                return this.filteredData.Sum(c => c.Area);
            }
        }
        public HashSet<BuiltInCategory> IncludedCategories { get; set; } = new HashSet<BuiltInCategory>();
        public DataTypes DataTree;
        private ElementsVisibleInViewExportContext Context;
        private Autodesk.Revit.ApplicationServices.Application App;
        private BuiltInCategory[] DefaultCategories = new BuiltInCategory[] { 
            BuiltInCategory.OST_Floors,
            BuiltInCategory.OST_Roofs,
            BuiltInCategory.OST_Ceilings,
            //BuiltInCategory.OST_Railings,
            BuiltInCategory.OST_CurtainWallMullions,
            BuiltInCategory.OST_Windows,
            BuiltInCategory.OST_Doors
        };

        private ICollection<Element> MyElements = new List<Element>();
        private List<FilteredInfo> MyInfo = new List<FilteredInfo>();
        private Material defaultMat = null;
        

        public CustomAnalysis(ElementsVisibleInViewExportContext context, Autodesk.Revit.ApplicationServices.Application app)
        {
            Context = context;
            App = app;

            //default categories to analyze
            foreach (BuiltInCategory category in DefaultCategories)
            {
                IncludedCategories.Add(category);
            }

            AnalyzedMaterial = new List<FilteredInfo>();
        }

        public CustomAnalysis(ElementsVisibleInViewExportContext context, Autodesk.Revit.ApplicationServices.Application app,List<BuiltInCategory> selected_categories)
        {
            Context = context;
            App = app;

            //User selected categories to analyze
            foreach(BuiltInCategory category in selected_categories)
            {
                IncludedCategories.Add(category);
            }
        }
        
        private bool StackedWallEligible(Wall wall, IList<Element> allwalls)
        {
            bool result = true;
            if(!wall.IsStackedWall) return false;
            foreach (var eid in wall.GetStackedWallMemberIds())
            {
                if (!allwalls.Select(x => x.Id).Contains(eid))
                {
                    return false;
                }

            }
            return result;
        }
        public void Extraction()
        {
            //Must-include categories
            IncludedCategories.Add(BuiltInCategory.OST_Walls);
            IncludedCategories.Add(BuiltInCategory.OST_CurtainWallPanels);

            //Initialize AnalyzedElements
            foreach(BuiltInCategory category in this.IncludedCategories)
            {
                AnalyzedElements.Add(category, new HashSet<Element>());
                AnalyzedData.Add(category, new List<FilteredData>());
            }
            
            List<Element> allwalls = new List<Element>();
            List<ElementId> hosts = new List<ElementId>();
            foreach(Document d in App.Documents)
            {
                //Store for walls and curtain panels
                IList<Element> walls = new FilteredElementCollector(d).OfCategory(BuiltInCategory.OST_Walls).WhereElementIsNotElementType().ToElements();
                IList<Element> cpanels = new FilteredElementCollector(d).OfCategory(BuiltInCategory.OST_CurtainWallPanels).WhereElementIsNotElementType().ToElements();
                foreach (Element e in walls)
                {
                    if (Context.get_ElementVisible(d, e.Id))
                    {
                        allwalls.Add(e);
                    }
                }
                foreach(var ele in walls)
                {
                    Wall wall = ele as Wall;
                    if (wall !=null && wall.IsStackedWall && StackedWallEligible(wall,walls))
                    {
                        allwalls.Remove(wall);
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
                        AnalyzedElements[BuiltInCategory.OST_CurtainWallPanels].Add(e);
                    }
                }
                foreach (Element wall in allwalls)
                {
                    if (!hosts.Contains(wall.Id))
                    {
                        AnalyzedElements[BuiltInCategory.OST_Walls].Add(wall);
                    }
                }

                //Store visible elements in other categories to AnalyzedElements
                foreach (BuiltInCategory c in this.IncludedCategories)
                {
                    if(c != BuiltInCategory.OST_CurtainWallPanels && c!= BuiltInCategory.OST_Walls)
                    {
                        IList<Element> elements = new FilteredElementCollector(d).OfCategory(c).WhereElementIsNotElementType().ToElements();
                        foreach(Element e in elements)
                        {
                            if(Context.get_ElementVisible(d, e.Id))
                            {
                                this.AnalyzedElements[c].Add(e);
                            }
                        }
                    }
                }
            }
        }

        private void process_category(HashSet<Element> set,BuiltInCategory category)
        {
            Dictionary<ElementId,HashSet<Element>> typesubset = new Dictionary<ElementId, HashSet<Element>>();
            Dictionary<ElementId, Element> types = new Dictionary<ElementId, Element>();
            //grouping subset by typeid
            foreach (Element e in set)
            {
                //avoid stacked wall
                
                ElementId typeid = e.GetTypeId();
                if (typesubset.ContainsKey(typeid))
                {
                    typesubset[typeid].Add(e);
                    types[typeid] = e;
                }
                else
                {
                    typesubset.Add(typeid, new HashSet<Element>());
                    types.Add(typeid, e);
                    typesubset[typeid].Add(e);
                }
            }

            //for each subset with unique typeId, construct an instance of FilteredData
            foreach(KeyValuePair<ElementId,HashSet<Element>> kvp in typesubset)
            {
                ElementId typeid = kvp.Key;
                Element currentType = types[typeid];
                HashSet<Element> elements = kvp.Value;
                double area = 0.0;

                

                //Calculate area for doors or windows
                foreach (Element e in elements)
                {
                    try
                    {
                        if (category == BuiltInCategory.OST_Windows || category == BuiltInCategory.OST_Doors)
                        {
                            FamilyInstance fInstance = e as FamilyInstance;
                            area += AnalysisUtils.GetInstanceSurfaceAreaMetric(fInstance);
                        }
                        else if(category == BuiltInCategory.OST_Railings)
                        {

                        }
                        else
                        {
                            area += AnalysisUtils.ElementArea(e);
                        }
                    }
                    catch { continue; }
                }
                if(area < 0.1) { continue; }

                FilteredData data = new FilteredData(typeid.ToString(), area, elements,this.AnalyzedMaterial,category);
                this.AnalyzedMaterial = data.materialPool;

                data.Type = data.PrimaryDoc.GetElement(typeid);
                FamilySymbol panelSymbol = data.PrimaryDoc.GetElement(typeid) as FamilySymbol;
                if(panelSymbol != null)
                {
                    data.Family = panelSymbol;
                }
                data.TypeMark = data.PrimaryDoc.GetElement(typeid)?.get_Parameter(BuiltInParameter.ALL_MODEL_TYPE_COMMENTS)?.AsString();
                data.Category = data.PrimaryDoc.GetElement(typeid)?.Category;
                data.TypeName = data.PrimaryDoc.GetElement(typeid)?.Name;



                AnalyzedData[category].Add(data);
                MyInfo.Add(data);
            }
        }

        public void Analyze()
        {
            foreach(KeyValuePair<BuiltInCategory, HashSet<Element>> kvp in AnalyzedElements)
            {
                process_category(kvp.Value, kvp.Key);
            }

            foreach(var material in this.AnalyzedMaterial)
            {
                FilteredMaterial mat = material as FilteredMaterial;
                if(mat.RevitMaterial == null)
                {
                    continue;
                }
                mat.Area = 0;

                
                mat.FilteredElements = new HashSet<Element>();
                foreach(var uniqueType in mat.typeArea.Keys)
                {
                    mat.Area += mat.typeArea[uniqueType];
                    mat.FilteredElements.UnionWith(mat.typeElement[uniqueType]);

                }
                AllMaterial.Add(mat);              
            }

            DataTypes types = new DataTypes(this.AnalyzedData);
            types.Name = this.Context.MainDoc.Title;
            this.DataTree = types;

        }
        public virtual ICollection<Element> AllAnalyzedElement()
        {
            ICollection<Element> elements = new List<Element>();
            foreach (KeyValuePair<BuiltInCategory, HashSet<Element>> es in this.AnalyzedElements)
            {
                foreach (Element e in es.Value)
                {
                    elements.Add(e);
                }
            }
            return elements;
        }
        public virtual Dictionary<string, double> ResultList()
        {
            Dictionary<string, double> result = new Dictionary<string, double>();
            foreach (var data in this.MyInfo)
            {
                if(data.Area != 0.0)
                {
                    result.Add(data.UniqueName, data.Area);
                }              
            }
            return result;
        }
        public virtual List<FilteredInfo> InfoList()
        {
            return this.MyInfo;
        }

        public virtual string Type()
        {
            return "Custom Analysis";
        }

        public virtual string Report() { return ""; }

        public virtual string Conclusion() { return ""; }
    }

}
