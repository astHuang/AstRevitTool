﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB;
using System.Windows.Forms;

namespace AstRevitTool.Core.Analysis
{
    public class DetailedMaterial_Analysis : Facade_Analysis
    {
        public DetailedMaterial_Analysis(ElementsVisibleInViewExportContext context, Autodesk.Revit.ApplicationServices.Application app) : base(context, app)
        {
            this.SortByFamily = false;
            this.SortByType = false;
            this.AnalyzingVolume = false;
            this.SortByCategory = false;
        }
        public bool SortByCategory { get; set; }
        public bool SortByFamily { get; set; }
        public bool SortByType { get; set; }

        public bool AnalyzingVolume { get; set; }

        private string MatName(Document doc,Face face,string mat_extra)
        {
            string name = doc.GetElement(face.MaterialElementId).Name;
            if (mat_extra == "") return name;
            else return mat_extra + " :: " + name;
        }
        public void add_material_area(GeometryElement geo, Document doc,string mat_extra)
        {
            foreach (GeometryObject o in geo)
            {
                if (o is Solid)
                {
                    double area = 0.0;
                    Solid solid = o as Solid;
                    string materialname = "";
                    foreach (Face face in solid.Faces)
                    {
                        if (null == doc.GetElement(face.MaterialElementId)) continue;
                        if (face.Area > area) {
                            area = face.Area;
                            materialname = MatName(doc,face,mat_extra);
                        };
                    }
                    if (Metrics.ContainsKey(materialname))
                    {
                        Metrics[materialname] += area;
                    }
                    else if(materialname != "" && !Metrics.ContainsKey(materialname))
                    {
                        Metrics.Add(materialname,area);
                    }                   
                }
                else if (o is GeometryInstance)
                {
                    GeometryInstance i = o as GeometryInstance;
                    add_material_area(i.GetInstanceGeometry(i.Transform), doc,mat_extra);
                }
            }
        }

        public void add_material_volume(GeometryElement geo, Document doc, string mat_extra)
        {
            foreach (GeometryObject o in geo)
            {
                if (o is Solid)
                {
                    double area = 0.0;
                    Solid solid = o as Solid;
                    string materialname = "";
                    double volume = solid.Volume;
                    foreach (Face face in solid.Faces)
                    {
                        if (null == doc.GetElement(face.MaterialElementId)) continue;
                        if (face.Area > area)
                        {
                            materialname = MatName(doc, face, mat_extra);
                        };
                    }
                    if (Metrics.ContainsKey(materialname))
                    {
                        Metrics[materialname] += volume;
                    }
                    else if (materialname != "" && !Metrics.ContainsKey(materialname))
                    {
                        Metrics.Add(materialname, volume);
                    }
                }
                else if (o is GeometryInstance)
                {
                    GeometryInstance i = o as GeometryInstance;
                    add_material_volume(i.GetInstanceGeometry(i.Transform), doc, mat_extra);
                }
            }
        }
        public void process_element(Element el)
        {
            Document doc = el.Document;
            Options option = new Options();
            option.View = doc.ActiveView;
            option.ComputeReferences = true;
            GeometryElement geoEl = el.get_Geometry(option);
            string MatNameExtra = "";
            if(this.SortByCategory == true)
            {
                string CName = el.Category?.Name;
                MatNameExtra += CName;
            }
            if(this.SortByFamily == true)
            {
                ElementId typeId = el.GetTypeId();
                string TName = doc.GetElement(typeId)?.Name;
                FamilyInstance fInstance = el as FamilyInstance;
                FamilySymbol FType = fInstance?.Symbol;
                string FName = FType?.FamilyName;
                if (TName != null)
                {
                    if (FName != null)
                    {
                        if(this.SortByType == true)
                        {
                            MatNameExtra += FName + " :: " + TName;
                        }
                        else
                        {
                            MatNameExtra += FName;
                        }
                    }
                    else
                    {
                        MatNameExtra += TName;
                    }
                }
            }
            if(geoEl == null) { return; }
            if(this.AnalyzingVolume == true)
            {
                add_material_volume(geoEl, doc, MatNameExtra);
            }
            else{
                add_material_area(geoEl, doc, MatNameExtra);
            }
            
        }
        public override void AnalyzeBasicWalls()
        {
            foreach (Element wall in this.AnalyzedElements["Basic Walls"])
            {
                process_element(wall);
            }
        }

        public override void AnalyzeCurtainWalls()
        {
            foreach (Element cpanel in this.AnalyzedElements["Curtain Panels"])
            {
                process_element(cpanel);
            }
        }

        public override void AnalyzeDoors()
        {
            foreach (Element door in this.AnalyzedElements["Doors"])
            {
                process_element(door);
            }
        }

        public override void AnalyzeWindows()
        {
            foreach (Element window in this.AnalyzedElements["Windows"])
            {
                process_element(window);
            }
        }

        public void AnalyzeRoofs()
        {
            foreach (Element roof in this.AnalyzedElements["Roofs"])
            {
                process_element(roof);
            }
        }

        public void AnalyzeFloors()
        {
            foreach (Element floor in this.AnalyzedElements["Floors"])
            {
                process_element(floor);
            }
        }

        public override void Analyze()
        {
            base.Extraction();
            AnalyzeBasicWalls();
            AnalyzeCurtainWalls();
            AnalyzeDoors();
            AnalyzeWindows();
            AnalyzeRoofs();
            AnalyzeFloors();
        }

        public override string Conclusion()
        {
            string lastline = "\n";
            lastline += "\n Total Material Count: " + this.Metrics.Count;
            lastline += "\n Total Material Area: " + this.Metrics.Values.Sum().ToString("0.##");
            return lastline;
        }

        public override string Report()
        {
            return base.Report() + this.Conclusion();
        }

        public override string Type()
        {
            return "Material Categories Analysis";
        }

        public override Dictionary<string, double> ResultList()
        {
            return this.Metrics;
        }
    }
}
