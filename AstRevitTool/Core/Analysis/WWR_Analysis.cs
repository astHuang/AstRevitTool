using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB;
using System.Windows.Forms;

namespace AstRevitTool.Core.Analysis
{
    public class WWR_Analysis : Facade_Analysis
    {
        public WWR_Analysis(ElementsVisibleInViewExportContext context, Autodesk.Revit.ApplicationServices.Application app) : base(context,app) {
            Metrics.Add("Solid Walls", 0.0);
            Metrics.Add("Total Curtain Walls", 0.0);
            Metrics.Add("::Curtain Wall, Vision Glazing", 0.0);
            Metrics.Add("::Curtain Wall, Spandrel", 0.0);
            Metrics.Add("Total Doors", 0.0);
            Metrics.Add("::Doors, Full Glazing", 0.0);
            Metrics.Add("::Doors, Solid", 0.0);
            Metrics.Add("::Doors, Transom and Sidelights", 0.0);
            Metrics.Add("Windows", 0.0);
            SpandrelMaterialsKeyword.Add("Spandrel");
            SpandrelMaterialsKeyword.Add("Shadow Box");
            this.wwr = 0.0;
            this.TotalFacade = 0.0;
            this.TotalOpening = 0.0;
        }

        public override void AnalyzeBasicWalls()
        {
            foreach (Element wall in this.AnalyzedElements["Basic Walls"])
            {
                Metrics["Solid Walls"] += wall.LookupParameter("Area").AsDouble();
            }
        }
        public double wwr { get; set; }
        public double TotalOpening { get; set; }
        public double TotalFacade { get; set; }
        public List<string> SpandrelMaterialsKeyword { get; set; } = new List<string>();
        public override void AnalyzeCurtainWalls()
        {
            foreach (Element cpanels in this.AnalyzedElements["Curtain Walls"])
            {
                double cw_area = 0.0;
                cw_area = cpanels.LookupParameter("Area").AsDouble();
                Metrics["Total Curtain Walls"] += cw_area;
            }

            foreach (Element cpanel in this.AnalyzedElements["Curtain Panels"])
            {
                //Autodesk.Revit.DB.Panel panel = cpanel as Autodesk.Revit.DB.Panel;
                Document doc = cpanel.Document;
                ElementId pTypeId = cpanel.GetTypeId();
                PanelType panelType = doc.GetElement(pTypeId) as PanelType;
                FamilySymbol panelSymbol = doc.GetElement(pTypeId) as FamilySymbol;
                if (null != panelType)
                {
                    ElementId id = panelType.get_Parameter(BuiltInParameter.MATERIAL_ID_PARAM).AsElementId();
                    if (id != null)
                    {
                        Material mat = doc.GetElement(id) as Material;
                        if (mat.Transparency < 5 || SpandrelMaterialsKeyword.Any(mat.Name.Contains))
                        {
                            Metrics["::Curtain Wall, Spandrel"] += cpanel.LookupParameter("Area").AsDouble();
                        }
                    }

                }
                else if(null != panelSymbol){
                    Options option = new Options();
                    option.View = doc.ActiveView;
                    option.ComputeReferences = true;
                    GeometryElement geoEl = cpanel.get_Geometry(option);
                    add_spandrel_area(geoEl, SpandrelMaterialsKeyword, doc);
                }
            }
        }

        public void add_spandrel_area(GeometryElement geo, List<string> cands, Document doc)
        {           
            foreach(GeometryObject o in geo)
            {
                if(o is Solid)
                {
                    double area = 0.0;
                    Solid solid = o as Solid;
                    foreach(Face face in solid.Faces)
                    {
                        if (null == doc.GetElement(face.MaterialElementId)) continue;
                        bool spandrel = cands.Any(doc.GetElement(face.MaterialElementId).Name.Contains) && face.Area > area;
                        area = spandrel? face.Area:area;
                    }
                    Metrics["::Curtain Wall, Spandrel"] += area;
                }
                else if(o is GeometryInstance)
                {
                    GeometryInstance i = o as GeometryInstance;
                    add_spandrel_area(i.GetInstanceGeometry(i.Transform), cands, doc);
                }
            }
        }

        public override void AnalyzeDoors()
        {
            List<Tuple<Element, string>> error = new List<Tuple<Element, string>>();
            foreach (Element door in this.AnalyzedElements["Doors"])
            {
                Document doc = door.Document;
                ElementId dTypeId = door.GetTypeId();
                FamilyInstance fInstance = door as FamilyInstance;
                FamilySymbol FType = fInstance.Symbol;
                string FName = FType.FamilyName;
                ElementType et = doc.GetElement(dTypeId) as ElementType;
                String TName = et.Name.ToString();
                bool totalopening = AnalysisUtils.Categorize_Door(FName, TName);
                Wall hostwall = fInstance.Host as Wall;
                double d_area = 0.0;
                try
                {
                    d_area = AnalysisUtils.GetInstanceSurfaceAreaMetric(fInstance);
                }
                catch
                {
                    d_area = 0.0;
                    error.Add(new Tuple<Element, string>(door, TName));
                    continue;
                }

                Metrics["Total Doors"] += d_area;
                if (!totalopening)
                {
                    double l_area = AnalysisUtils.lighting_area(AnalysisUtils._keys, door);
                    Metrics["::Doors, Transom and Sidelights"] += l_area;
                    Metrics["::Doors, Solid"] += d_area - l_area;
                }
                else
                {
                    if (FName.Contains("Storefront"))
                    {
                        Metrics["Total Curtain Walls"] -= d_area;
                    }

                }

            }
            if (error.Any())
            {
                string errormsg = "Cannot calculate area of some doors. Please check whether they should be included in WWR calculation.";
                foreach (Tuple<Element, string> wall in error)
                {
                    errormsg += "\n" + wall.Item2 + ",Element Id: " + wall.Item1.Id.ToString();
                }
                MessageBox.Show(errormsg, "Door area calculation error!");
            }
        }

        public override void AnalyzeWindows()
        {
            List<Tuple<Element, string>> error = new List<Tuple<Element, string>>();
            foreach (Element window in this.AnalyzedElements["Windows"])
            {
                double w_area = 0.0;
                FamilyInstance fInstance = window as FamilyInstance;
                try
                {
                    w_area = AnalysisUtils.GetInstanceSurfaceAreaMetric(fInstance);

                }
                catch
                {
                    //MessageBox.Show("Cannot calculate windows area, Windows Id: " + window.Id, ex.Message + "\r\n" + ex.StackTrace);
                    error.Add(new Tuple<Element, string>(window, window.Name));
                    w_area = 0.0;
                    continue;
                }
                Metrics["Windows"] += w_area;
            }
            if (error.Any())
            {
                string errormsg = "Cannot calculate area of some windows. Please check whether they should be included in WWR calculation.";
                foreach (Tuple<Element, string> wall in error)
                {
                    errormsg += "\n" + wall.Item2 + ",Element Id: " + wall.Item1.Id.ToString();
                }
                MessageBox.Show(errormsg, "Window area calculation error!");
            }
        }

        private void SumUpWWR()
        {
            double TotalCurtainWallArea = Metrics["Total Curtain Walls"];
            double TotalSpandrelArea = Metrics["::Curtain Wall, Spandrel"];
            Metrics["::Curtain Wall, Vision Glazing"] = TotalCurtainWallArea - TotalSpandrelArea;
            double TotalWindowArea = Metrics["Windows"];
            double TotalDoorArea = Metrics["Total Doors"];
            double SolidDoorArea = Metrics["::Doors, Solid"];
            Metrics["::Doors, Full Glazing"] = TotalDoorArea - SolidDoorArea - Metrics["::Doors, Transom and Sidelights"];
            double TotalSolidWallArea = Metrics["Solid Walls"];

            this.TotalOpening = (TotalCurtainWallArea - TotalSpandrelArea) + TotalWindowArea + (TotalDoorArea - SolidDoorArea);
            this.TotalFacade = TotalCurtainWallArea + TotalSolidWallArea + TotalWindowArea + TotalDoorArea;
            this.wwr = this.TotalOpening / this.TotalFacade;
            //Metrics.Add("_Total Opening Area", TotalOpeningArea);
            //Metrics.Add("_Total Facade Area", TotalFacadeArea);
            //Metrics.Add("_Window-to-wall Ratio", TotalOpeningArea / TotalFacadeArea);
        }
        public override void Analyze()
        {
            base.Extraction();
            AnalyzeBasicWalls();
            AnalyzeCurtainWalls();
            AnalyzeDoors();
            AnalyzeWindows();
            SumUpWWR();
        }

        public override string Conclusion()
        {
            string lastline = "\n";
            lastline += "\n Total Opening Area: " + this.TotalOpening.ToString("0.##");
            lastline += "\n Total Facade Area: " + this.TotalFacade.ToString("0.##");
            lastline += "\n Window-to-Wall Ratio: " + this.wwr.ToString("0.###");
            return lastline;
        }

        public override string Report()
        {
            return base.Report() + this.Conclusion();
        }

        public override string Type()
        {
            return "Window-to-Wall Ratio Analysis";
        }

        public override Dictionary<string, double> ResultList()
        {
            return this.Metrics;
        }
    }
}
