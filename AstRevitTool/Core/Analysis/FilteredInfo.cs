using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB;
using System.Windows.Forms;

namespace AstRevitTool.Core.Analysis
{
    public class FilteredInfo
    {
        public double Area { get; set; }

        public double Volume { get; set; }

        public string Name { get; set; }

        public List<DirectShape> Shapes { get; set; }
        public static FilteredInfo matchInfoFromList(List<FilteredInfo> listinfo, string key)
        {
            foreach(FilteredInfo item in listinfo)
            {
                if(item.Name == key) return item;
            }
            return null;
        }
        public List<Element> FilteredElements { get; set; }

        public FilteredInfo(string name,double area,double volume, List<Element> ele)
        {
            Name = name;
            Area = area;
            Volume = volume;
            FilteredElements = ele;
        }

        public FilteredInfo(string name,double area, List<Element> ele)
        {   
            Name = name;
            Area = area;
            Volume = 0.0;
            FilteredElements = ele;
        }

        public FilteredInfo(string name,double area)
        {
            Name=name;
            Area = area;
            Volume=0.0;
            FilteredElements = new List<Element>();
        }

    }
}
