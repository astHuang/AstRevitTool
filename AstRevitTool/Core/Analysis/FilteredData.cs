using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB;
using System.Windows.Forms;

namespace AstRevitTool.Core.Analysis
{
    public class FilteredData : FilteredInfo
    {
        public string UniqueTypeId;

        public string TypeName;
        public string TypeMark { get; set; }
        public Autodesk.Revit.DB.Family Family;
        public Category Category;
        public BuiltInCategory BIC;

        public Dictionary<string, FilteredMaterial> BreakDowns;
        //public Dictionary<string, Material> BreakDownMaterials;
        //public Dictionary<Material, double> BreakDownArea;
        //public Dictionary<Material, HashSet<Element>> MaterialElement;
        public HashSet<Element> UnassignedElement;
        public double UnassignedArea;
        public List<FilteredInfo> Materials;

        public List<FilteredInfo> materialPool;

        public Document PrimaryDoc;

        public static List<FilteredInfo> MaterialsFromCollection(IEnumerable<FilteredData> data)
        {
            List<FilteredInfo> uniqueList = new List<FilteredInfo>();
            foreach(FilteredData item in data)
            {
                foreach(FilteredInfo material in item.Materials)
                {
                    FilteredMaterial mat = material as FilteredMaterial;
                    if(mat != null)
                    {
                        uniqueList.Add(mat);
                    }
                }
            }
            return uniqueList;
        }
        
        public FilteredData(string typeid, double area) : base(typeid, area)
        {
            UniqueTypeId = typeid;
            TypeName = "";
            TypeMark = "";
            Family = null;
            Category = null;

            FilteredElements = new HashSet<Element>();
            BreakDowns = new Dictionary<string, FilteredMaterial>();
            //BreakDownArea = new Dictionary<Material, double>();
            //MaterialElement = new Dictionary<Material, HashSet<Element>>();
            UnassignedArea = 0;
            UnassignedElement = new HashSet<Element>();


        }

        public FilteredData(string typeid,double area, HashSet<Element> ele, List<FilteredInfo> basepool,BuiltInCategory bic) : base(typeid, area, ele)
        {
            this.UniqueTypeId = typeid;
            this.TypeMark = "";
            this.TypeName = "";
            this.Family = null;
            this.Category = null;
            BreakDowns = new Dictionary<string, FilteredMaterial>();
            //BreakDownArea = new Dictionary<Material, double>();
            //MaterialElement = new Dictionary<Material, HashSet<Element>>();
            UnassignedArea = 0;
            UnassignedElement = new HashSet<Element>();
            Materials = new List<FilteredInfo>();
            PrimaryDoc = ele.ToList()[0].Document;
           
            BIC = bic;

            materialPool = basepool;
            if(ele.Count > 0)
            {
                foreach(Element ele2 in ele)
                {
                    addMaterialFromElement(ele2);
                }
            }
            ConvertMaterial();
        }

        public double getMaterialTotal()
        {
            return BreakDowns.Sum(x => x.Value.Area);
        }

        public void addMaterialFromElement(Element ele)
        {
            List<BuiltInCategory> bdCategories = new List<BuiltInCategory>();
            bdCategories.Add(BuiltInCategory.OST_Floors);
            bdCategories.Add(BuiltInCategory.OST_Roofs);
            bdCategories.Add(BuiltInCategory.OST_Walls);
           
            if (ele == null) return;
            Options option = new Options();
            option.ComputeReferences = true;
            GeometryElement geoEl = ele.get_Geometry(option);
            if (geoEl == null) return;
            if(ele.Category.HasMaterialQuantities == true && bdCategories.Contains(this.BIC))
            {
                add_material_area(ele);
            }
            else
            {
                add_material_area(geoEl, ele);
            }
        }

        private string matName(Document doc,Face face)
        {
            if (null == doc.GetElement(face.MaterialElementId) || "" == doc.GetElement(face.MaterialElementId).Name) return "Unassigned_Material";
            return doc.GetElement(face.MaterialElementId).Name;
        }

        private void add_material_area(Element e)
        {
            Document doc = e.Document;

            foreach (ElementId matid in e.GetMaterialIds(false))
            {
                double area = e.GetMaterialArea(matid, false);
                if (area > 0)
                {
                    Material mat = doc.GetElement(matid) as Material;
                    AddMaterial(mat, area, e);
                    if (mat != null)
                    {
                        FilteredMaterial outputMaterial;
                        this.BreakDowns.TryGetValue(mat.Name, out outputMaterial);
                        if (outputMaterial != null)
                        {
                            outputMaterial.Method = "Layer's Area";
                        }
                    }                  
                }

            }
        }

        private void add_material_area(GeometryElement geo,Element el)
        {
            Document doc = el.Document;
            foreach (GeometryObject o in geo)
            {
                if (o is Solid)
                {
                    double area = 0.0;
                    Solid solid = o as Solid;
                    string materialname = "";
                    Material mat = null;
                    foreach (Face face in solid.Faces)
                    {
                        if (face.Area > area)
                        {
                            area = face.Area;
                            materialname = matName(doc, face);
                            if (doc.GetElement(face.MaterialElementId) != null) {
                                mat = doc.GetElement(face.MaterialElementId) as Material;
                            }
                        };
                    }
                    if(area < 0.01) { continue; }
                    AddMaterial(mat, area, el);
                    FilteredMaterial outputMaterial;
                    if (mat != null)
                    {
                        this.BreakDowns.TryGetValue(mat.Name, out outputMaterial);
                        if (outputMaterial != null)
                        {
                            outputMaterial.Method = "Face's Area";
                        }
                    }
                    
                }
                else if (o is GeometryInstance)
                {
                    GeometryInstance i = o as GeometryInstance;
                    add_material_area(i.GetInstanceGeometry(i.Transform), el);
                }
            }
        }

        private void AddMaterial(Material mat, double area, Element e)
        {
            HashSet<Element> elements = new HashSet<Element>();
            elements.Add(e);
            if (mat != null)
            {
                string matName = mat.Name;
                if (BreakDowns.ContainsKey(matName))
                {
                    BreakDowns[matName].Area += area;
                    
                    BreakDowns[matName].FilteredElements.UnionWith(elements);
                }
                else
                {
                    FilteredMaterial newMat = new FilteredMaterial(matName, area, elements, mat);   
                    BreakDowns.Add(matName, newMat);
                }
            }
            else
            {
                UnassignedElement.Add(e);
                UnassignedArea += area;
            }
        }

        private void ConvertMaterial()
        {
            if(UnassignedArea >= 0.1)
            {
                FilteredMaterial unassgined = new FilteredMaterial("Unassigned", this.UnassignedArea);
                unassgined.FilteredElements = this.UnassignedElement;
                this.BreakDowns.Add("Unassigned", unassgined);
            }
            
            foreach(var kvp in this.BreakDowns)
            {
                
                string matName = kvp.Key;
                double area = kvp.Value.Area;
                FilteredMaterial currentMat;
                HashSet<Element> elements = kvp.Value.FilteredElements;
                bool needUpdate = false;
                if (matchInfoFromList(materialPool, matName) != null)
                {
                    currentMat = matchInfoFromList(materialPool, matName) as FilteredMaterial;
                }
                else
                {
                    currentMat = new FilteredMaterial(matName,area,elements,kvp.Value.RevitMaterial);
                    needUpdate = true;
                }
                if (!currentMat.typeArea.ContainsKey(this.UniqueName))
                {
                    currentMat.typeArea.Add(this.UniqueName, area);
                    currentMat.typeElement.Add(this.UniqueName, elements);
                }
                else
                {
                    currentMat.typeArea[UniqueName] += area;
                    currentMat.typeElement[UniqueName].UnionWith(elements);
                }

                currentMat.FilteredElements.UnionWith(elements);
                if (needUpdate) UpdateMaterialPool(currentMat);
            }
        }
        
        private void UpdateMaterialPool(FilteredMaterial currentMat)
        {
            this.materialPool.Add(currentMat);
        }
    }

}
