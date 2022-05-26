using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB;
using System.Windows.Forms;
using AstRevitTool.Core.Analysis;

namespace AstRevitTool.Core.Analysis
{
    public class Material_Analysis: Facade_Analysis
    {
        public Material_Analysis(ElementsVisibleInViewExportContext context, Autodesk.Revit.ApplicationServices.Application app) : base(context, app)
        {
            byType = false;
            byFamily = false;
            TotalCurtainArea = 0.0;
            TotalCurtainArea = 0.0;
            WallMaterialArea = 0.0;
            CurtainFamilyArea = 0.0;
        }
        public bool byType { get; set; }
        private double TotalWallArea { get; set; }
        private double TotalCurtainArea { get; set; }

        private double WallMaterialArea { get; set; }

        private double CurtainFamilyArea { get; set; }
        public bool byFamily { get; set; }
        public override void AnalyzeBasicWalls()
        {
            List<Tuple<Element,string>> error = new List<Tuple<Element,string>>();
            foreach (Element wall in this.AnalyzedElements["Basic Walls"])
            {
                if (AnalysisUtils.WallEligable(wall))
                {
                    try
                    {
                        Document doc = wall.Document;
                        this.TotalWallArea += AnalysisUtils.ElementArea(wall);
                        ElementId typeId = wall.GetTypeId();
                        WallType wtype = doc.GetElement(typeId) as WallType;
                        string TName = wtype?.Name;
                        string TMark = wtype?.get_Parameter(BuiltInParameter.ALL_MODEL_TYPE_MARK).AsString();
                        if (this.byType == true ||this.byFamily == true)
                        {
                            try
                            {
                                string fullname = TMark + " : " + TName;
                                double area = AnalysisUtils.ElementArea(wall);
                                this.WallMaterialArea += area;
                                if (Metrics.Keys.Contains(fullname))
                                {
                                    Metrics[fullname] += area;
                                }
                                else
                                {
                                    Metrics.Add(fullname, area);
                                }

                            }
                            catch
                            {
                                if (AnalysisUtils.ElementArea(wall)>0) { error.Add(new Tuple<Element, string>(wall, TName)); }
                                continue;
                            }
                        }
                        else
                        {
                            double area = AnalysisUtils.ElementArea(wall);
                            try
                            {
                                ElementId matId = wtype.GetCompoundStructure().GetLayers()[0].MaterialId;
                                string matName = doc.GetElement(matId)?.Name;
                                //double area = wall.LookupParameter("Area").AsDouble();
                                this.WallMaterialArea += area;
                                if (Metrics.Keys.Contains(matName))
                                {
                                    Metrics[matName] += area;
                                }
                                else
                                {
                                    Metrics.Add(matName, area);
                                }
                            }
                            catch
                            {
                                if (area>0) { error.Add(new Tuple<Element, string>(wall, TName)); }
                                continue;
                            }
                        }
                    }
                    catch { continue; }
                }
            }
            if (error.Any())
            {
                string errormsg = "Cannot find walls' material, Please check those walls: ";
                foreach(Tuple<Element,string> wall in error)
                {
                    errormsg += "\n" + wall.Item2 + ",Element Id: " + wall.Item1.Id.ToString();
                }
                MessageBox.Show(errormsg, "Wall Material not Found!");
            }
        }

        public override void AnalyzeCurtainWalls()
        {
            List<Tuple<Element, string>> error = new List<Tuple<Element, string>>();
            foreach (Element cpanel in this.AnalyzedElements["Curtain Panels"])
            {

                this.TotalCurtainArea += AnalysisUtils.ElementArea(cpanel); 
                Document doc = cpanel.Document;
                double area = AnalysisUtils.ElementArea(cpanel);
                if (AnalysisUtils.PanelEligable(cpanel))
                {
                    try
                    {
                        ElementId typeId = cpanel.GetTypeId();
                        string TName = doc.GetElement(typeId).Name;
                        FamilyInstance fInstance = cpanel as FamilyInstance;
                        FamilySymbol FType = fInstance?.Symbol;
                        string FName = FType?.FamilyName;
                        string fullname = byFamily == true ? FName : FName + " : " + TName;
                        //double area = cpanel.LookupParameter("Area").AsDouble();
                        this.CurtainFamilyArea += area;
                        if (Metrics.Keys.Contains(fullname))
                        {
                            Metrics[fullname] += area;
                        }
                        else
                        {
                            Metrics.Add(fullname, area);
                        }

                    }
                    catch
                    {
                        if (area > 0) { error.Add(new Tuple<Element, string>(cpanel, cpanel.Name)); }
                        //error.Add(new Tuple<Element, string>(cpanel, cpanel.Name));
                        continue;
                    }
                }
            }
            if (error.Any())
            {
                string errormsg = "Cannot extraction information in curtain panel, Please check those panels: ";
                foreach (Tuple<Element, string> wall in error)
                {
                    errormsg += "\n" + wall.Item2 ;
                }
                MessageBox.Show(errormsg, "Curtain Panel material information not Found!");
            }
        }

        public override void AnalyzeDoors()
        {
            return;
        }

        public override void AnalyzeWindows()
        {
            return;
        }

        public override void Analyze()
        {
            base.Extraction();
            AnalyzeBasicWalls();
            if(byType == true || byFamily == true) {
                AnalyzeCurtainWalls();
            }
        }

        public override string Conclusion()
        {
            string lastline = "\n";
            lastline += "\n Total Wall Area: " + this.TotalWallArea.ToString("0.##");
            double uncounted = this.TotalWallArea - this.WallMaterialArea;
            if(uncounted > 0.01)
            {
                lastline += "\n Counted Wall Area: " + this.WallMaterialArea.ToString("0.##");
                lastline += "\n Uncounted Wall Area: " + uncounted.ToString("0.##");

            }
            if (byType == true || byFamily == true) {
                lastline += "\n Total Curtain Area: " + this.TotalCurtainArea.ToString("0.##");
                double uncountedcurtain = this.TotalCurtainArea - this.CurtainFamilyArea;
                if(uncountedcurtain > 0.01)
                {
                    lastline += "\n Counted Curtain Area: " + this.TotalCurtainArea.ToString("0.##");
                    lastline += "\n Uncounted Curtain Area: " + uncountedcurtain.ToString("0.##");
                }
            }
            return lastline;
        }

        public override string Report()
        {
            return base.Report();
        }

        public override string Type()
        {
            string type = "Wall Material Take-offs";
            if(byType == true)
            {
                type = "Wall Material Take-offs by Type";
            }

            if (byFamily == true)
            {
                type = "Wall Family Take-offs";
            }
            return type;
        }

        public override Dictionary<string, double> ResultList()
        {
            return this.Metrics;
        }

        public override List<FilteredInfo> InfoList()
        {
            return new List<FilteredInfo>();
        }
    }
}
