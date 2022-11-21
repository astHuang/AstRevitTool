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

        public string Method;
        public FilteredMaterial(string matName, double area) : base(matName, area)
        {
            this.typeArea = new Dictionary<string, double>();
            this.typeElement = new Dictionary<string, HashSet<Element>>();
        }

        public FilteredMaterial(string matName, double area, HashSet<Element> ele, Material rvtMat):base(matName, area, ele)
        {
            this.typeArea = new Dictionary<string, double>();
            this.typeElement = new Dictionary<string, HashSet<Element>>();
            this.Area = area;
            this.RevitMaterial = rvtMat;
        }
    }
}
