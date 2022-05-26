using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB;
using System.Windows.Forms;
using Autodesk.Revit;
using Autodesk.Revit.UI;

namespace AstRevitTool.Core.Analysis
{
    public class AssemblyMaterials_Analysis:Assembly_Analysis
    {
        public AssemblyMaterials_Analysis(ElementsVisibleInViewExportContext context, Autodesk.Revit.ApplicationServices.Application app) : base(context, app)
        {
            m_totalQuantities = new Dictionary<string, MaterialQuantities>();
            m_quantitiesPerElement = new Dictionary<ElementId, Dictionary<string, MaterialQuantities>>();
        }

        private Dictionary<string, MaterialQuantities> m_totalQuantities = new Dictionary<string, MaterialQuantities>();
        private Dictionary<ElementId, Dictionary<string, MaterialQuantities>> m_quantitiesPerElement = new Dictionary<ElementId, Dictionary<string, MaterialQuantities>>();

        class MaterialQuantities
        {
            public double Volume { get; set; }

            /// <summary>
            /// Net area (sq. ft)
            /// </summary>
            public double Area { get; set; }
        }
        private void CalculateMaterialQuantitiesOfElement(Element e)
        {
            ElementId id = e.Id;
            Document doc = e.Document;
            foreach(ElementId matid in e.GetMaterialIds(false))
            {
                double area = e.GetMaterialArea(matid, false);
                double volume = e.GetMaterialVolume(matid);
                if(volume > 0.0 || area > 0.0)
                {
                    string cat = e.Category.Name;
                    string mat = cat + ": " + doc.GetElement(matid).Name;
                    StoreMaterialQuantities(mat, area, volume, m_totalQuantities);
                    Dictionary<string, MaterialQuantities> quantityperelement;
                    bool found = m_quantitiesPerElement.TryGetValue(id, out quantityperelement);
                    if (found)
                    {
                        StoreMaterialQuantities(mat, area, volume, quantityperelement);
                    }
                    else
                    {
                        quantityperelement = new Dictionary<string, MaterialQuantities>();
                        StoreMaterialQuantities(mat, area, volume, quantityperelement);
                        m_quantitiesPerElement.Add(id, quantityperelement);
                    }
                }
            }
        }

        private void StoreMaterialQuantities(string mat,double area,double volume, Dictionary<string, MaterialQuantities> quantities)
        {
            MaterialQuantities quantityperelement;
            bool found = quantities.TryGetValue(mat, out quantityperelement);
            if (found)
            {
                quantityperelement.Area += area;
                quantityperelement.Volume += volume;
            }
            else
            {
                quantityperelement = new MaterialQuantities();
                quantityperelement.Area = area;
                quantityperelement.Volume = volume;
                quantities.Add(mat, quantityperelement);
            }
        }

        public override void Analyze()
        {
            base.Extraction();
            foreach(Element e in this.AnalyzedElements["Basic Walls"])
            {
                CalculateMaterialQuantitiesOfElement(e);
            }
            foreach (Element e in this.AnalyzedElements["Floors"])
            {
                CalculateMaterialQuantitiesOfElement(e);
            }
            foreach (Element e in this.AnalyzedElements["Roofs"])
            {
                CalculateMaterialQuantitiesOfElement(e);
            }
        }

        public override string Report()
        {
            string str = "";
            foreach (KeyValuePair<string, MaterialQuantities> entry in this.m_totalQuantities)
            {
                if (entry.Value.Area!= 0.0)
                {
                    str += "\n" + entry.Key + "/ " + entry.Value.Area.ToString("0.##");
                }
            }
            return str;
        }
        public override string Type()
        {
            return "Compound Structures Material Analysis";
        }

        public override Dictionary<string, double> ResultList()
        {
            Dictionary<string, double> result = new Dictionary<string, double>();
            foreach (KeyValuePair<string, MaterialQuantities> entry in this.m_totalQuantities)
            {
                result.Add(entry.Key, entry.Value.Area);
            }
            return result;
        }

        public override List<FilteredInfo> InfoList()
        {
            return new List<FilteredInfo>();
        }
    }
}
