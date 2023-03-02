using AstRevitTool.Core.Analysis;
using Autodesk.Revit.DB;
using System.Collections.ObjectModel;
using AstRevitTool.Core;
using System.Windows.Media;
using System.IO;
using System.Windows.Forms;
using Autodesk.Revit.UI;

namespace AstRevitTool.Views
{
    public class ViewModelByCategory
    {
        public ViewModelByCategory()
        {
            this._dataDetails = new ObservableCollection<SourceDataTypes>();
        }

        public IAnalysis Analysis;

        private ObservableCollection<SourceDataTypes> _dataDetails;
        private ObservableCollection<SourceDataTypes> _dataByMaterial;
        private ObservableCollection<SourceDataTypes> _dataByTransparency;

        private ObservableCollection<SourceDataTypes> _hiddenDetails;
        private ObservableCollection<SourceDataTypes> _hiddenMaterial;
        private ObservableCollection<SourceDataTypes> _hiddenTransparency;

        public Dictionary<String, FilterData> DictFilters { get; set; }
        private ObservableCollection<SourceDataTypes> allHidden()
        {
            ObservableCollection<SourceDataTypes> result = new ObservableCollection<SourceDataTypes>();
            var allhidden = _hiddenDetails.Union(_hiddenMaterial).Union(_hiddenTransparency);
            foreach (SourceDataTypes data in allhidden)
            {
                result.Add(data);
            }
            return result;
        }

        public HashSet<Element> GetHiddenElements()
        {
            HashSet<Element> hidden = new HashSet<Element>();
            hidden.UnionWith(hiddenElementSetfromData(_hiddenDetails));
            hidden.UnionWith(hiddenElementSetfromData(_hiddenMaterial));
            hidden.UnionWith(hiddenElementSetfromData(_hiddenTransparency));
            return hidden;
        }



        public HashSet<Element> hiddenElementSetfromData(IEnumerable<SourceDataTypes> data)
        {
            HashSet<Element> hidden = new HashSet<Element>();
            foreach (var dataitem in data)
            {
                if (dataitem.Elements != null)
                {
                    hidden.UnionWith(dataitem.Elements);
                }
            }
            return hidden;
        }

        public List<string> SpandrelMaterialsKeyword = new List<string>();
        public ObservableCollection<SourceDataTypes> DataDetails { get { return _dataDetails; } }
        public ObservableCollection<SourceDataTypes> DataByMaterial { get { return _dataByMaterial; } }

        public ObservableCollection<SourceDataTypes> DataByTransparency { get { return _dataByTransparency; } }

        public ObservableCollection<SourceDataTypes> HiddenItems { get { return _hiddenDetails; } }

        public bool CheckContain(SourceDataTypes target, IEnumerable<SourceDataTypes> source)
        {
            //TODO: Recursively check if a target data exist in a list of source data
            foreach(SourceDataTypes data in source)
            {
                if(data.Children==null || data.Children.Count == 0)
                {
                    if(data.Equals(target)) return true;
                }
                else
                {
                    if(data.Equals(target) || CheckContain(target, data.Children)) return true;
                }
            }
            return false;
        }

        public void Remove(SourceDataTypes toRemove, ICollection<SourceDataTypes> source)
        {
            foreach(SourceDataTypes data in source)
            {
                if (data.Equals(toRemove))
                {
                    source.Remove(data);
                    return;
                }
                else
                {
                    if(data.Children==null || data.Children.Count == 0)
                    {
                        continue;
                    }
                    else
                    {
                        Remove(toRemove, data.Children);
                    }
                }
            }
        }

        public void InsertBack(SourceDataTypes toInsert, ICollection<SourceDataTypes> source)
        {
            if(toInsert.Parent == null)
            {
                source.Add(toInsert);
                return;
            }
            else
            {
                foreach (SourceDataTypes data in source)
                {
                    if (data.Equals(toInsert.Parent))
                    {
                        data.Children.Add(toInsert);
                        return;
                    }
                }
            }
            
        }
        public ViewModelByCategory(CustomAnalysis analysis)
        {
            this.Analysis = analysis;
            SpandrelMaterialsKeyword.Add("Spandrel");
            SpandrelMaterialsKeyword.Add("Shadowbox");
            SpandrelMaterialsKeyword.Add("Backpanel");
            this._dataDetails = this.CreateData(analysis);
            this._dataByMaterial = this.CreateDataByMaterial(analysis);
            this._dataByTransparency = this.ConvertData(this._dataDetails);
            this._hiddenDetails = new ObservableCollection<SourceDataTypes>();
            this._hiddenMaterial = new ObservableCollection<SourceDataTypes>();
            this._hiddenTransparency = new ObservableCollection<SourceDataTypes>();
            this.DictFilters = new Dictionary<string, FilterData>();
        }

        private ObservableCollection<SourceDataTypes> CreateData(CustomAnalysis analysis)
        {
            var dataList = new ObservableCollection<SourceDataTypes>();
            foreach (CustomAnalysis.DataType dataType in analysis.DataTree)
            {
                dataList.Add(new SourceDataTypes(dataType));
            }
            return dataList;
        }

        private ObservableCollection<SourceDataTypes> CreateDataByMaterial(CustomAnalysis analysis)
        {
            var materialList = new ObservableCollection<SourceDataTypes>();
            foreach (var mat in analysis.AllMaterial)
            {
                if (mat != null)
                {
                    materialList.Add(new SourceDataTypes(mat));
                }
            }
            return materialList;
        }

        private ObservableCollection<SourceDataTypes> ConvertData(ObservableCollection<SourceDataTypes> data)
        {
            //Hierarchy: Transparency(2 items) ==> Category ==> Type ===> Material
            var dataList = new ObservableCollection<SourceDataTypes>();
            SourceDataTypes opaque = new SourceDataTypes("Solid/Facade", false);
            
            
            SourceDataTypes glazing = new SourceDataTypes("Fenestration", true);

            var windows = data.Where(x => x.Name == "Windows");
            var walls = data.Where(x => x.Name == "Walls");
            var doors = data.Where(x => x.Name == "Doors");
            var curtains = data.Where(x => x.Name == "Curtain Panels");

            //All windows are glazing area
            foreach (var x in windows)
            {
                glazing.Children.Add(new SourceDataTypes(x));

            }

            //All walls are opaque area
            foreach (var x in walls)
            {
                opaque.Children.Add(new SourceDataTypes(x));
            }


            foreach (var x in curtains)
            {
                SourceDataTypes curtainpanels = new SourceDataTypes("Curtain Panels", true);
                curtainpanels.BICs.Add(BuiltInCategory.OST_CurtainWallPanels);
                SourceDataTypes spandrelpanels = new SourceDataTypes("Curtain Panels - Spandrel", false);
                spandrelpanels.BICs.Add(BuiltInCategory.OST_CurtainWallPanels);
                foreach (SourceDataTypes paneltype in x.Children)
                {
                    SourceDataTypes glass;
                    SourceDataTypes spandrel;
                    if (splitCurtainPanel(paneltype, out glass, out spandrel))
                    {
                        if (glass != null)
                        {
                            curtainpanels.Children.Add(glass);
                        }
                        if (spandrel != null)
                        {
                            spandrelpanels.Children.Add(spandrel);
                        }
                    }
                }
                curtainpanels.updateArea();
                curtainpanels.updateElement();
                spandrelpanels.updateArea();
                spandrelpanels.updateElement();
                if (curtainpanels.Area > 0)
                {
                    glazing.Children.Add(curtainpanels);
                }
                if (spandrelpanels.Area > 0)
                {
                    opaque.Children.Add(spandrelpanels);
                }
            }

            //Doors need to seperate
            foreach (var x in doors)
            {
                SourceDataTypes door = new SourceDataTypes("Doors, Solid Parts", false);
                door.BICs.Add(BuiltInCategory.OST_Doors);
                SourceDataTypes door_transom = new SourceDataTypes("Doors, Glazing Parts", true);
                door_transom.BICs.Add(BuiltInCategory.OST_Doors);
                foreach (SourceDataTypes doorType in x.Children)
                {
                    if (doorType.Family != null)
                    {
                        string TName = doorType.Name;
                        string FName = doorType.FamilyName;
                        bool totalopening = AnalysisUtils.Categorize_Door(FName, TName);
                        if (totalopening)
                        {
                            if (FName.Contains("Storefront"))
                            {
                                //TODO
                            }
                            door_transom.Children.Add(doorType);
                        }
                        else
                        {
                            //Door with transoms
                            double totalLightArea = 0;
                            foreach (Element doorele in doorType.Elements)
                            {
                                double l_area = AnalysisUtils.lighting_area(AnalysisUtils._keys, doorele);
                                totalLightArea += l_area;
                            }
                            SourceDataTypes doorsNormal;
                            SourceDataTypes doorsWithTransom;
                            if (splitDoor(doorType, totalLightArea, out doorsNormal, out doorsWithTransom))
                            {
                                if (doorsNormal.Area > 0.5)
                                {
                                    door.Children.Add(doorsNormal);
                                }
                                if(doorsWithTransom.Area > 0.5)
                                {
                                    door_transom.Children.Add(doorsWithTransom);
                                }
                            }

                        }
                    }
                    else
                    {
                        door.Children.Add(doorType);
                    }
                }
                door.updateArea();
                door.updateElement();
                door_transom.updateArea();
                door_transom.updateElement();
                if (door_transom.Area > 0)
                {
                    glazing.Children.Add(door_transom);
                }
                if (door.Area > 0)
                {
                    opaque.Children.Add(door);
                }
            }

            opaque.updateArea();
            opaque.updateElement();
            opaque.updateBIC();
            glazing.updateArea();
            glazing.updateElement();
            glazing.updateBIC();
            dataList.Add(opaque);
            dataList.Add(glazing);
            return dataList;
        }

        private bool splitCurtainPanel(SourceDataTypes panelType, out SourceDataTypes _glasspanel, out SourceDataTypes _spandrelpanel)
        {
            SourceDataTypes panel_glass = new SourceDataTypes(panelType);
            SourceDataTypes spandrel_glass = new SourceDataTypes(panelType);
            Autodesk.Revit.DB.ElementType panel = panelType.Rvt_ptr as Autodesk.Revit.DB.ElementType;
            if (panel == null) { _glasspanel = null; _spandrelpanel = null; return false; }
            if (panelType.Children.Count == 0) { _glasspanel = null; _spandrelpanel = null; return false; }
            else if (panelType.Children.Count == 1)
            {
                var mat = panelType.Children[0];
                if (mat.Transparent)
                {
                    _glasspanel = panel_glass;
                    _glasspanel._notes = "Single transparent material";
                    _spandrelpanel = null;
                    return true;
                }
                else
                {
                    _glasspanel = null;
                    _spandrelpanel = spandrel_glass;
                    _spandrelpanel._notes = "Single opaque material";
                    return true;
                }
            }
            List<int> glassChildern = new List<int>();
            List<int> spandrelChildern = new List<int>();
            for (int i = 0; i < panelType.Children.Count; i++)
            {
                var materialData = panelType.Children[i];
                Autodesk.Revit.DB.Material material = materialData.Rvt_ptr as Autodesk.Revit.DB.Material;
                string matCat = materialData.MaterialCategory;
                if (material == null) { continue; }
                var matType = getPanelMaterialType(material, matCat);
                if (matType == CurtainPanelMaterialType.Glass)
                {
                    glassChildern.Add(i);
                }
                else if (matType == CurtainPanelMaterialType.Spandrel)
                {
                    spandrelChildern.Add(i);
                }
            }
            if (glassChildern.Count == 0)
            {
                _glasspanel = null;
                _spandrelpanel = spandrel_glass;
                _spandrelpanel._notes = "Multiple material with no glass";
                return true;
            }
            panel_glass.Children.Clear();
            double glass_area = 0;
            for (int i = 0; i < glassChildern.Count; i++)
            {
                var glass = panelType.Children[glassChildern[i]];
                panel_glass.Children.Add(glass);
                glass_area += glass.Area;
            }
            if (spandrelChildern.Count == 0 && glass_area > 0.95 * panelType.Area)
            {
                _glasspanel = panel_glass;
                _glasspanel._notes = "Multiple material with no shading";
                _spandrelpanel = null;
                return true;
            }
            spandrel_glass._area = 0;
            spandrel_glass.Children.Clear();
            for (int i = 0; i < spandrelChildern.Count; i++)
            {
                var spandrel = panelType.Children[spandrelChildern[i]];
                double area = spandrel.Area;
                panel_glass._area -= area;
                spandrel_glass.Children.Add(spandrel);
                spandrel_glass._area += area;
            }


            _glasspanel = panel_glass;
            _glasspanel._notes = "Panel area minus shading portion";
            _spandrelpanel = spandrel_glass;
            _spandrelpanel._notes = "Shading portion within a panel";
            return true;

        }

        private bool splitDoor(SourceDataTypes doorType, double l_area, out SourceDataTypes _doors, out SourceDataTypes _doorsWithTransom)
        {
            SourceDataTypes doors = new SourceDataTypes(doorType);
            SourceDataTypes doors_transom = new SourceDataTypes(doorType);
            doors_transom.Children.Clear();
            doors._area = doors._area - l_area;
            doors_transom._area = l_area;
            int count = 0;
            foreach (SourceDataTypes child in doorType.Children)
            {
                if (child.Transparent)
                {
                    doors.Children.RemoveAt(count);
                    doors_transom.Children.Add(child);
                }
                count++;
            }
            
            _doors = doors;
            _doorsWithTransom = doors_transom;
            return true;
        }

        private void getPanelTypebyCategory(FamilyInstance fi)
        {

        }
        private CurtainPanelMaterialType getPanelMaterialType(Autodesk.Revit.DB.Material material, string category)
        {
            if (category != "" && category != null)
            {
                switch (category)
                {
                    case "Glass":
                        return CurtainPanelMaterialType.Glass;
                    case "Shadowbox":
                        return CurtainPanelMaterialType.Spandrel;

                    case "Spandrel":
                        return CurtainPanelMaterialType.Spandrel;
                    case "Frame":
                        return CurtainPanelMaterialType.Mullion;

                    default:
                        break;
                }
            }
            if (material.Transparency > 10)
            {
                if (this.SpandrelMaterialsKeyword.Any(material.Name.Contains))
                {
                    return CurtainPanelMaterialType.Spandrel;
                }
                else
                {
                    return CurtainPanelMaterialType.Glass;
                }
            }
            else
            {
                if (this.SpandrelMaterialsKeyword.Any(material.Name.Contains))
                {
                    return CurtainPanelMaterialType.Spandrel;
                }
                else
                {
                    return CurtainPanelMaterialType.Mullion;
                }
            }
        }

        public void Hide(IEnumerable<SourceDataTypes> sel)
        {
            foreach (SourceDataTypes data in sel)
            {
                Remove(data, this._dataDetails);
                this._hiddenDetails.Add(data);
            }
            this._dataByTransparency = this.ConvertData(this._dataDetails);

            Application.ASTRequestHandler.Arg1 = sel;
            Application.ASTRequestHandler.Request = RequestId.Hide;
            Application.ASTEvent.Raise();
        }

        public void Unhide()
        {
            IEnumerable<SourceDataTypes> hidden = this.allHidden();
            foreach(var data in this._hiddenDetails)
            {
                InsertBack(data, this._dataDetails);
            }
            this._hiddenDetails.Clear();
            
            this._dataByTransparency = this.ConvertData(this._dataDetails);
            Application.ASTRequestHandler.Arg1 = hidden;
            Application.ASTRequestHandler.Request = RequestId.Unhide;
            Application.ASTEvent.Raise();
        }

        public void Update(ElementsVisibleInViewExportContext context, Document doc)
        {
            Application.ASTRequestHandler.Arg1 = context;
            Application.ASTRequestHandler.Request = RequestId.Update;
            Application.ASTEvent.Raise();
        }

        public void Color(Dictionary<string, HashSet<Element>> colorinfo)
        {
            Application.ASTRequestHandler.Arg1 = colorinfo;
            Application.ASTRequestHandler.Request = RequestId.Color;
            Application.ASTEvent.Raise();
        }

        public void UpdateFilter(Dictionary<String, FilterData> m_dictFilters)
        {
            foreach(KeyValuePair<String,FilterData> kvp in m_dictFilters)
            {
                FilterData currentData;
                bool Exist = this.DictFilters.TryGetValue(kvp.Key, out currentData);
                if(Exist)
                {
                    currentData = kvp.Value;
                }
                else
                {
                    this.DictFilters.Add(kvp.Key, kvp.Value);
                }
            }
        }

        public void RemoveFilter(List<string> filterNamesToRemove)
        {
            Dictionary<String, FilterData> r_dictFilters = new Dictionary<string, FilterData>();
            foreach (string filterName in filterNamesToRemove) {
                FilterData currentData;
                bool Exist = this.DictFilters.TryGetValue(filterName, out currentData);
                if(Exist)
                {
                    this.DictFilters.Remove(filterName);
                    r_dictFilters.Add(filterName, currentData);
                }
            }
            if(r_dictFilters.Count > 0)
            {
                Application.ASTRequestHandler.Arg1 = r_dictFilters;
                Application.ASTRequestHandler.Request = RequestId.Uncolor;
                Application.ASTEvent.Raise();
            }

        }
        public void ViewFilter()
        {
            Application.ASTRequestHandler.Arg1 = this.DictFilters;
            Application.ASTRequestHandler.Request = RequestId.Filter;
            Application.ASTEvent.Raise();

        }

        private void ClearItemBackgroundColor(SourceDataTypes parent)
        {
            parent._color = Colors.Transparent;
            parent.Background = new SolidColorBrush(parent._color);
            if (parent.Children != null && parent.Children.Count != 0)
            {
                foreach (SourceDataTypes child in parent.Children)
                {
                    ClearItemBackgroundColor(child);
                }
            }
            
        }
        public void Clear()
        {
            this.DictFilters.Clear();
            IEnumerable<Element> allelements = this.Analysis.AllAnalyzedElement();
            foreach(SourceDataTypes data in this._dataByMaterial)
            {
                ClearItemBackgroundColor(data);
            }
            foreach (SourceDataTypes data in this._dataByTransparency)
            {
                ClearItemBackgroundColor(data);
            }
            foreach (SourceDataTypes data in this._dataDetails)
            {
                ClearItemBackgroundColor(data);
            }
            Application.ASTRequestHandler.Arg1 = allelements;
            Application.ASTRequestHandler.Request = RequestId.Clear;
            Application.ASTEvent.Raise();
            
        }

        public void Print(string fileName)
        {
            AstRevitTool.Core.Export.ASTExportUtils.WriteExcelFromModel(fileName, false, this);
        }

    }
}
