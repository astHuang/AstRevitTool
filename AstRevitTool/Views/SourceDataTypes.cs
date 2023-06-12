using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB;
using AstRevitTool.Core.Analysis;
using AstRevitTool.Core;
using System.Collections.ObjectModel;
using System.Windows.Media;
using System.ComponentModel;
using Microsoft.Office.Interop.Excel;

namespace AstRevitTool.Views
{
    public enum SourceDataCategory
    {
        ElementType,
        ElementCategory,
        ElementSubMaterial,
        ElementMaterial,
        ElementSubType
    }
    public class SourceDataTypes:INotifyPropertyChanged
    {
        private string _name;
        private SourceDataCategory _datahierarchy;
        public double _area;
        public System.Windows.Media.Color _color;
        private bool _transparent;
        public string _notes;
        private string _familyName;
        private ObservableCollection<SourceDataTypes> _children;
        private string _hierarchy;
        private SourceDataTypes _parent;
        private HashSet<Element> _elements;
        private string _ruleName;
        private string _colorgroup;
        public bool isSelected;
        public string MaterialCategory = "";

        public string ColorGroup { 
            get { return _colorgroup; } 
            set { _colorgroup = value;OnPropertyChanged("ColorGroup");
                OnPropertyChanged("DisplayColorGroupName"); } 
        }

        public string DisplayColorGroupName
        {
            get { return (_colorgroup == _ruleName) || (_ruleName == null)? "" : _colorgroup;}
        }

        public SourceDataCategory Hierarchy { get { return _datahierarchy; }  }
        public Category b_category;
        public Element Rvt_ptr;
        public FamilySymbol Family { get; set; }

        public List<string> TypeNameCollection = new List<string>();
        public string Name { get { return _name; } set { _name = value; OnPropertyChanged("Name"); } }

        public HashSet<Element> Elements { get { return _elements; } }
        public double Area { get { return _area; } set { _area = value;OnPropertyChanged("Area"); } }

        public string RuleName { get { return _ruleName; } set { _ruleName = value;OnPropertyChanged("RuleName"); } }
        private Brush _background;

        public Brush Background { get { return _background; } set { _background = value;OnPropertyChanged("Background"); } }

        
        public string FamilyName { get { return _familyName; } }

        public System.Windows.Media.Color Color {  get { return _color; } }

        public bool Transparent { get { return _transparent; } }

        public string Notes { get { return _notes; } }

        public HashSet<BuiltInCategory> BICs = new HashSet<BuiltInCategory>();

        public ObservableCollection<SourceDataTypes> Children { get { return _children; } }

        public void ResetColorGroup()
        {
            this._colorgroup = this._ruleName;
        }

        

        public void SortChilden()
        {
            if(_children != null && _children.Count > 0)
            {
                _children = new ObservableCollection<SourceDataTypes>(
        Children.OrderBy(child => child.Notes)
                .ThenByDescending(child => child.Area)
                .ThenBy(child => child.Name)
    );

                // Notify any subscribers that the Children property has been updated
                OnPropertyChanged(nameof(Children));
            }
        }
        public SourceDataTypes Parent { get { return _parent; } }

        //create data type of fenestration or 
        public SourceDataTypes(string name, bool opaque)
        {
            _name = name;
            _datahierarchy = SourceDataCategory.ElementCategory;
            _colorgroup = name;
            _transparent = opaque;
            _children = new ObservableCollection<SourceDataTypes>();
            _parent = null;
            _elements = new HashSet<Element>();
            _color = Colors.Transparent;
            _background = new SolidColorBrush(_color);
            Rvt_ptr = null;
            _ruleName= null;
            BICs = new HashSet<BuiltInCategory>();

        }
        #region Categorized by Type
        public SourceDataTypes(CustomAnalysis.DataType dataType)
        {
            isSelected = false;
            _name = dataType.CategoryName;
            _colorgroup = dataType.CategoryName;
            b_category = dataType.Category;
            _datahierarchy = SourceDataCategory.ElementCategory;
            BICs.Clear();
            BICs.Add(dataType.BIC);         
            _area = Math.Round(dataType.GetArea(),1);
            _color = Colors.Transparent;
            _background = new SolidColorBrush(_color);
            _transparent = false;
            _notes = "";
            _ruleName= null;
            _children = new ObservableCollection<SourceDataTypes>();
            _elements = new HashSet<Element>();
            Rvt_ptr = dataType.Doc.GetElement(dataType.Category.Id);
            _parent = null;
            foreach(FilteredData data in dataType.filteredData)
            {
                _children.Add(new SourceDataTypes(data,this));
                _elements.UnionWith(data.FilteredElements);
            }
            SortChilden();
            
        }

        public void updateArea()
        {
            if(this.Children.Count == 0)
            {
                return;
            }
            this._area = this.Children.Sum(x => x.Area);
        }

        public void updateBIC()
        {
            if (this.Children.Count == 0)
            {
                return;
            }
            this.BICs.Clear();
            foreach(var child in this.Children)
            {
                this.BICs.UnionWith(child.BICs);
            }
        }

        public void updateElement()
        {
            //Only update the element at one level
            var new_elements = new HashSet<Element>();
            foreach(var child in this.Children)
            {
                new_elements.UnionWith(child.Elements);
            }
            this._elements = new_elements;
        }

        //Create a copy from sourcedatatype
        public SourceDataTypes(SourceDataTypes data)
        {
            _name=data.Name;
            _parent=data.Parent;
            _transparent = data.Transparent;
            _colorgroup = data._colorgroup;
            _area=data.Area;
            _datahierarchy = data._datahierarchy;
            _color = data.Color;
            _background = new SolidColorBrush(_color);
            _notes =data.Notes;
            _elements = data.Elements;
            _children = new ObservableCollection<SourceDataTypes>();
            Rvt_ptr = data.Rvt_ptr;
            _ruleName = data._ruleName;
            b_category = data.b_category;
            BICs = data.BICs;
            if(data.Children != null && data.Children.Count >0)
            {
                foreach(SourceDataTypes child in data.Children)
                {
                    _children.Add(child);
                }
                SortChilden();
            }
        }

        //Create a data from type data
        public SourceDataTypes(FilteredData data,SourceDataTypes parent)
        {
            _parent = parent;
            isSelected = false;
            _name = data.TypeName;
            _datahierarchy = SourceDataCategory.ElementType;
            _area = Math.Round(data.Area, 1);
            _color = Colors.Transparent;
            _colorgroup = _name;
            _background = new SolidColorBrush(_color);
            _transparent =false;
            _children = new ObservableCollection<SourceDataTypes>();
            _elements = data.FilteredElements;
            _notes = data.TypeMark;
            BICs.Clear();
            BICs.Add(data.BIC);
            Rvt_ptr = data.Type;
            _ruleName = Rvt_ptr.Name;
            if(data.Family != null)
            {
                this.Family = data.Family;
                this._familyName = data.Family.FamilyName;
                this._name = data.Family.FamilyName + "(" +data.TypeName +")";
            }
                       
            foreach(KeyValuePair<string, FilteredMaterial> item in data.BreakDowns)
            {
                _children.Add(new SourceDataTypes(item, data,this));
            }
            SortChilden();
        }

        public SourceDataTypes(KeyValuePair<string, FilteredMaterial> item,FilteredData parent,SourceDataTypes parents)
        {
            isSelected=false;
            _name = item.Key;
            _area = Math.Round(item.Value.Area,1);
            _color = Colors.Transparent;
            _background = new SolidColorBrush(_color);
            _parent = parents;
            _children = null;
            _datahierarchy = SourceDataCategory.ElementSubMaterial;
            _transparent = item.Value.isTransparent;
            _ruleName = null;
            if (item.Value.RevitMaterial != null)
            {
                Rvt_ptr = item.Value.RevitMaterial;
                
            }
            BICs.Clear();
            BICs.Add(parent.BIC);
            MaterialCategory = item.Value.subCategory;
            _notes = item.Value.subCategory;
            _elements = item.Value.FilteredElements;
        }
        #endregion

        public SourceDataTypes(FilteredMaterial material)
        {
            isSelected = false;
            MaterialCategory = material.subCategory;
            _name = material.RevitMaterial.Name;
            _datahierarchy = SourceDataCategory.ElementMaterial;
            Rvt_ptr= material.RevitMaterial;
            _ruleName = null;
            _area = Math.Round(material.Area,1);
            _color = Colors.Transparent;
            _colorgroup = _name;
            _background = new SolidColorBrush(_color);
            _transparent = material.isTransparent;
            _parent = null;
            _children = new ObservableCollection<SourceDataTypes>();
            _elements= material.FilteredElements;
            _notes = material.subCategory;
            TypeNameCollection = new List<string>();
            BICs.Clear();
            foreach(string uniqueName in material.typeNames.Keys)
            {
                string name = material.typeNames[uniqueName];
                string rulename = material.typeRules[uniqueName];
                double area = Math.Round(material.typeArea[uniqueName],1);
                HashSet<Element> elements = material.typeElement[uniqueName];
                HashSet<BuiltInCategory> cats = material.typeCat[uniqueName];
                BICs.UnionWith(cats);
                this.TypeNameCollection.Add(name);
                
                _children.Add(new SourceDataTypes(name, rulename,area, elements,cats,this));
            }
            SortChilden();
        }

        public SourceDataTypes(string name, string rule, double area,HashSet<Element> elements,HashSet<BuiltInCategory> bics,SourceDataTypes parent)
        {
            _parent = parent;
            BICs = bics;
            _datahierarchy = SourceDataCategory.ElementSubType;
            _name = name;
            _ruleName = rule;
            _area = Math.Round(area,1);
            _elements= elements;
            _color = Colors.Transparent;
            _colorgroup = rule;
            _background = new SolidColorBrush(_color);
            
        }

        public void UpdateBackground()
        {
            this._background = new SolidColorBrush(this._color);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;

            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
