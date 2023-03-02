using System;

using Autodesk.Revit.DB;

using System.Runtime.InteropServices;

namespace AstRevitTool.Core.Analysis
{
    public class FilteredMaterial:FilteredInfo
    {
        public Material RevitMaterial;

        public bool isTransparent = false;

        /// <summary>
        /// collection of Type Id
        /// </summary>
        public Dictionary<string, double> typeArea;
        public Dictionary<string, HashSet<Element>> typeElement;
        public Dictionary<string, string> typeNames { get; set; }

        public Dictionary<string, string> typeRules { get; set; }
        public Dictionary<string, HashSet<BuiltInCategory>> typeCat { get; set; }
        public Dictionary<string,Element> typePointers { get; set; }


        // for the use of window-to-wall ratio
        public string subCategory = "";

        public string Method;
        public FilteredMaterial(string matName, double area) : base(matName, area)
        {
            this.typeArea = new Dictionary<string, double>();
            this.typeElement = new Dictionary<string, HashSet<Element>>();
            this.typeNames = new Dictionary<string, string>();
            this.typeRules = new Dictionary<string, string>();
            this.typeCat = new Dictionary<string, HashSet<BuiltInCategory>>();
            this.typePointers= new Dictionary<string, Element>();
        }

        public FilteredMaterial(string matName, double area, HashSet<Element> ele, Material rvtMat):base(matName, area, ele)
        {
            this.typeArea = new Dictionary<string, double>();
            this.typeElement = new Dictionary<string, HashSet<Element>>();
            this.typeNames=new Dictionary<string, string>();
            this.typeRules = new Dictionary<string, string>();
            this.typeCat = new Dictionary<string, HashSet<BuiltInCategory>>();
            this.typePointers = new Dictionary<string, Element>();
            this.Area = area;
            this.RevitMaterial = rvtMat;
        }
    }
}
