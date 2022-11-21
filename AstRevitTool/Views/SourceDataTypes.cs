using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB;
using AstRevitTool.Core.Analysis;
using AstRevitTool.Core;
using System.Collections.ObjectModel;

namespace AstRevitTool.Views
{
    public class SourceDataTypes
    {
        private string _name;
        private double _area;
        private Color _color;
        private bool? _transparent;
        private string _notes;
        private ObservableCollection<SourceDataTypes> _children;

        public string Name { get { return _name; } }

        public double Area { get { return _area; } }

        public Color Color { get { return _color; } }

        public bool? Transparent { get { return _transparent; } }

        public string Notes { get { return _notes; } }  

        public ObservableCollection<SourceDataTypes> Children { get { return _children; } }

        public SourceDataTypes()
        {

        }

        public SourceDataTypes(CustomAnalysis.DataType dataType)
        {
            _name = dataType.CategoryName;
            _area = Math.Round(dataType.GetArea());
            _color = Color.InvalidColorValue;
            _transparent = null;
            _notes = "";
            _children = new ObservableCollection<SourceDataTypes>();
            foreach(FilteredData data in dataType.filteredData)
            {
                _children.Add(new SourceDataTypes(data));
            }
            
        }

        public SourceDataTypes(FilteredData data)
        {
            _name = data.TypeName;
            _area = Math.Round(data.Area, 1);
            _color = Color.InvalidColorValue;
            _transparent=null;
            _children = new ObservableCollection<SourceDataTypes>();
            _notes = data.TypeMark;
            foreach(KeyValuePair<string, FilteredMaterial> item in data.BreakDowns)
            {
                _children.Add(new SourceDataTypes(item, data));
            }
        }

        public SourceDataTypes(KeyValuePair<string, FilteredMaterial> item,FilteredData parent)
        {
            _name = item.Key;
            _area = Math.Round(item.Value.Area,2);
            _color = Color.InvalidColorValue;
            _children = null;
            _transparent = null;
            _notes = item.Value.Method;
        }

        
    }
}
