using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using AstRevitTool.Core.Analysis;
using AstRevitTool.Core;
using System.IO;
using System.Windows.Forms;
using System.Web.UI.WebControls;

namespace AstRevitTool.Views
{
    /// <summary>
    /// Interaction logic for Analysis_Main.xaml
    /// </summary>
    public partial class Analysis_Main : Window
    {

        

        public Analysis_Main()
        {
            InitializeComponent();
        }

        public Analysis_Main(Document doc,CustomAnalysis custom_Analysis)
        {
            InitializeComponent();
            this.Analysis = custom_Analysis;
            this.MainDoc = doc;

        }

        public Analysis_Main(ElementsVisibleInViewExportContext context,Document doc, Autodesk.Revit.ApplicationServices.Application app)
        {
            InitializeComponent();
            //contentArea.Children.Clear();
            Context = context;
            App = app;
            MainDoc = doc;
            contextRefreshed= true;
        }

        public CustomAnalysis Analysis;
        public Document MainDoc;
        public ElementsVisibleInViewExportContext Context { get; set; }
        public ViewModelByCategory Model;
        public Autodesk.Revit.ApplicationServices.Application App;
        public bool contextRefreshed;


        private void refreshContext()
        {
            if (this.Context == null || this.Model == null) return;
            Document doc = this.MainDoc;
            
            if (doc.ActiveView as View3D != null)
            {
                 
                this.Model.Update(this.Context,doc);
            }
                
            else
            {
                System.Windows.MessageBox.Show("You must be in 3D view to export.");
                return;
            }
        }

        private void btnBeginAnalysis_Click(object sender, RoutedEventArgs e)
        {
            //contentArea.Children.Clear();
            
            if (!contextRefreshed)
            {
                System.Windows.MessageBox.Show("Press 'Refresh' button first before clicking this!");
                return;
            }
            
            Analysis = new CustomAnalysis(Context, App);
            Analysis.Extraction();
            Analysis.Analyze();

            
            Model = new ViewModelByCategory(this.Analysis);           

            DataContext = Model;
            contextRefreshed = false;
            //contentArea.Children.Add(contentBorder);

        }

        private void btnHide_Click(object sender, RoutedEventArgs e)
        {
            var selectedItems = multiSelectTreeView.SelectedItems;

            if (TabCategory.IsSelected)
            {
                selectedItems = multiSelectTreeView.SelectedItems;
            }
            else if (TabMaterial.IsSelected)
            {
                selectedItems = multiSelectTreeView_byMaterial.SelectedItems;
            }
            else if (TabWWR.IsSelected)
            {
                selectedItems = multiSelectTreeView_byTransparency.SelectedItems;
            }
            if (selectedItems.Count == 0) { return; }

            var selectedData = selectedItems.Cast<SourceDataTypes>().Where(item => item != null).ToList();
            
            
            Model.Hide(selectedData);
            
     

            DataContext = Model;
        }

        private void btnHighlightSelected_Click(object sender, RoutedEventArgs e)
        {
            var selected = multiSelectTreeView.SelectedItems;
            var selectedData = selected.Cast<SourceDataTypes>().Where(item => item != null).ToList();
            Model.Hide(selectedData);
            DataContext = Model;
        }

        private void btnSelect_Click(object sender, RoutedEventArgs e)
        {
            UIDocument uiDoc = new UIDocument(this.MainDoc);
            var selectedItems = multiSelectTreeView.SelectedItems;

            if (TabCategory.IsSelected)
            {
                selectedItems = multiSelectTreeView.SelectedItems;
            }
            else if (TabMaterial.IsSelected)
            {
                selectedItems = multiSelectTreeView_byMaterial.SelectedItems;
            }
            else if (TabWWR.IsSelected)
            {
                selectedItems = multiSelectTreeView_byTransparency.SelectedItems;
            }
            if (selectedItems.Count == 0) { return; }

            HashSet<Element> uniqueSelection = new HashSet<Element>();
            
            foreach(var dataItem in selectedItems)
            {
                SourceDataTypes data = dataItem as SourceDataTypes;
                uniqueSelection.UnionWith(data.Elements);
            }
            
            var legalElementIds = uniqueSelection.Where(item => item.Document.Equals(this.MainDoc)).Select(element => element.Id).ToList();
            if(legalElementIds.Count == 0)
            {
                System.Windows.MessageBox.Show("No element in selection!");
                return;
            }
            if(legalElementIds.Count != uniqueSelection.Count)
            {
                System.Windows.MessageBox.Show("Elements in linked file cannot be selected! Please go into linked model to edit");
            }
            uiDoc.Selection.SetElementIds(legalElementIds);
            uiDoc.ShowElements(legalElementIds);
        }

        private void btnUnhideSelected_Click(object sender, RoutedEventArgs e)
        {
            Model.Unhide();
            DataContext = Model;
            /*
            UIDocument uiDoc = new UIDocument(this.MainDoc);
            var selectedItems = multiSelectTreeView.SelectedItems;
            if(selectedItems.Count == 0) { selectedItems = multiSelectTreeView_byMaterial.SelectedItems; }
            if(selectedItems.Count == 0) { selectedItems = multiSelectTreeView_byTransparency.SelectedItems; }
            if(selectedItems.Count == 0) { return; }
            List<HashSet<Element>> tree = new List<HashSet<Element>>();
            List<SourceDataTypes> selection = new List<SourceDataTypes>();
            List<ParameterFilterElement> elemFilterList = new List<ParameterFilterElement>();
            Dictionary<String, FilterData> m_dictFilters = new Dictionary<String, FilterData>();
            foreach ( var item in selectedItems) {
                SourceDataTypes data = item as SourceDataTypes;
                //Element ele = data.Rvt_ptr;
                
                var elelist = data.Elements;
                tree.Add(elelist);
                selection.Add(data);
            }



            List<FilterElement> filterList = new FilteredElementCollector(this.MainDoc).WherePasses(new ElementClassFilter(typeof(FilterElement))).ToElements().Cast<FilterElement>().ToList();

            string[] colors = Constants.ColourValues.Skip(0).Take(tree.Count).ToArray();
            Dictionary<string,HashSet<Element>> colorinfo = new Dictionary<string,HashSet<Element>>();
            for(int i=0;i<colors.Length;i++)
            {
                string color = colors[i];
                SourceDataTypes data = selectedItems[i] as SourceDataTypes;
                byte[] rgb = Util.HexStringToColor(color);
                data._color = System.Windows.Media.Color.FromRgb(rgb[0], rgb[1], rgb[2]);
                data.Background = new SolidColorBrush(data._color);
                
                HashSet<Element> elements = tree[i];
                colorinfo.Add(color, elements);
            }

            Model.Color(colorinfo);
            //TODO: Solve linked element
            */
        }

        private void btnClearSelected_Click(object sender, RoutedEventArgs e)
        {
            var selectedItems = multiSelectTreeView.SelectedItems;

            if (TabCategory.IsSelected)
            {
                selectedItems = multiSelectTreeView.SelectedItems;
            }
            else if (TabMaterial.IsSelected)
            {
                selectedItems = multiSelectTreeView_byMaterial.SelectedItems;
            }
            else if (TabWWR.IsSelected)
            {
                selectedItems = multiSelectTreeView_byTransparency.SelectedItems;
            }
            if (selectedItems.Count == 0) { return; }
            List<string> toRemove = new List<string>();
            Dictionary<string,string> ItemGroupPair = new Dictionary<string,string>();
            var orig_filter = Model.DictFilters;

            foreach(var item in selectedItems)
            {
                SourceDataTypes data = item as SourceDataTypes;

                
                string groupName = data.ColorGroup;
                

                orig_filter.TryGetValue(groupName, out FilterData currentData);
                if(currentData != null)
                {
                    currentData.RemoveDataElement(data);
                }
                /*
                string dataName = data.RuleName;
                ItemGroupPair.Add(dataName, groupName);
                if(!toRemove.Contains(data.ColorGroup))
                {
                    toRemove.Add(data.ColorGroup);
                }
                */
                data._color = Colors.Transparent;
                data.Background = new SolidColorBrush(data._color);
                data.ColorGroup = data.RuleName;
            }

            Model.UpdateFilter(orig_filter);
            Model.ViewFilter();
            //Model.RemoveFilter(ItemGroupPair);
            //Model.RemoveFilter(toRemove);
            DataContext= Model;
        }


       
        private void TreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            var item = (e.NewValue as TreeViewItem);
            if (item.Items.Count > 0)
            {
                (item.Items[0] as TreeViewItem).IsSelected = true;
            }
        }

        
        private void btnFilter_Click(object sender, RoutedEventArgs e)
        {
            var selectedItems = multiSelectTreeView.SelectedItems;
            
            if (TabCategory.IsSelected)
            {
                selectedItems = multiSelectTreeView.SelectedItems;
            }
            else if(TabMaterial.IsSelected)
            {
                selectedItems = multiSelectTreeView_byMaterial.SelectedItems;
            }
            else if (TabWWR.IsSelected)
            {
                selectedItems = multiSelectTreeView_byTransparency.SelectedItems;
            }
            if (selectedItems.Count == 0) { return; }
            Dictionary<String, FilterData> m_dictFilters = new Dictionary<String, FilterData>(this.Model.DictFilters);
            //Dictionary<String, List<FilterData>> m_dictFilters = new Dictionary<String, List<FilterData>>();

            process_info.Text = "Assigning color for selection";
            var csd = new ColorSelectionDialog();
            if (csd.Show() == ItemSelectionDialogResult.Confirmed)
            {
                Autodesk.Revit.DB.Color currentColor = csd.SelectedColor;

                //new version: color by data group
                List<SourceDataTypes> dataList = selectedItems.Cast<SourceDataTypes>().ToList();

                if(dataList.Count > 0) {
                    var groupedData = dataList.GroupBy(data => data.Hierarchy)
                              .ToDictionary(group => group.Key, group => group.ToList());

                    foreach (KeyValuePair<SourceDataCategory, List<SourceDataTypes>> kvp in groupedData)
                    {
                        var sourceDataCategory = kvp.Key;
                        var sourceDataList = kvp.Value;
                        string content = "Filter name for group at hierarchy " + kvp.Key.ToString();
                        string value = kvp.Value.First().Notes;
                        var dg = AstRevitTool.Commands.CmdSvgExport.ShowInputDialog(content, ref value);

                        foreach(SourceDataTypes data in  sourceDataList)
                        {
                            string groupName = data.ColorGroup;

                            if (groupName != null)
                            {
                                this.Model.DictFilters.TryGetValue(groupName, out FilterData currentData);
                                if (currentData != null)
                                {
                                    currentData.RemoveDataElement(data);
                                }
                            }

                            //this.Model.DictFilters.TryGetValue(groupName, out FilterData currentData);
                            /*if (currentData != null)
                            {
                                currentData.RemoveDataElement(data);
                            }*/
                        }
                        if (value == null || value == "" || dg == System.Windows.Forms.DialogResult.Cancel)
                        {
                            System.Windows.MessageBox.Show("No filter group name, will color individually");
                            foreach(SourceDataTypes data in sourceDataList)
                            {
                                
                                data._color = Util.FormColorFromRevit(currentColor);
                                data.Background = new SolidColorBrush(data._color);
                                FilterData currentData = createFilterFromDataList(new List<SourceDataTypes>() { data});
                                if (currentData.m_originalDataList == null || currentData.m_originalDataList.Count == 0)
                                {
                                    continue;
                                }

                                //process_info.Text = "Assigning color for:\n" + data.Name;

                                //currentData.originalData._color = Util.FormColorFromRevit(currentColor);
                                //currentData.originalData.Background = new SolidColorBrush(currentData.originalData._color);
                                if (m_dictFilters.Keys.Contains(data.Name))
                                {
                                    m_dictFilters[data.Name] = currentData;
                                }
                                else
                                {
                                    m_dictFilters.Add(data.Name, currentData);
                                }
                                
                            }
                        }

                        else if (m_dictFilters.Keys.Contains(value))
                        {
                            System.Windows.MessageBox.Show("Same filter detected, will add the items to the filter");
                            sourceDataList.ForEach(data => {
                                data._color = Util.FormColorFromRevit(currentColor);
                                data.Background = new SolidColorBrush(data._color);
                                data.ColorGroup = value;//assign unique value to data's color group
                            });
                            sourceDataList.ForEach(x =>
                            m_dictFilters[value].AddDataElement(x)                        
                            ) ;
                        }
                        else
                        {
                            FilterData currentData = createFilterFromDataList(sourceDataList);
                            sourceDataList.ForEach(data => {
                                data._color = Util.FormColorFromRevit(currentColor);
                                data.Background = new SolidColorBrush(data._color);
                                data.ColorGroup = value;
                            });
                            m_dictFilters.Add(value, currentData);
                        }

                        /*
                        foreach(var data in sourceDataList)
                        {
                            if (this.Model.DictFilters.Keys.Contains(data.ColorGroup))
                            {
                                this.Model.DictFilters[data.ColorGroup].RemoveDataElement(data);
                            }
                        }*/
                    }
                }

                /*
                else
                {
                    SourceDataTypes data = selectedItems[0] as SourceDataTypes;
                    FilterData currentData = createFilterFromData(data);
                    if (currentData.RuleData == null || currentData.RuleData.Count == 0)
                    {
                        System.Windows.MessageBox.Show("The item you select is unable to assign color");
                        return;
                    }

                    process_info.Text = "Assigning color for:\n" + data.Name;

                    currentData.originalData._color = Util.FormColorFromRevit(currentColor);
                    currentData.originalData.Background = new SolidColorBrush(currentData.originalData._color);
                    m_dictFilters.Add(data.Name, currentData);

                }*/
                /*
                //old version color by individual data
                    foreach (var item in selectedItems)
                    {
                        SourceDataTypes data = item as SourceDataTypes;
                        FilterData currentData = createFilterFromData(data);
                        if (currentData.RuleData == null || currentData.RuleData.Count == 0)
                        {
                            continue;
                        }

                        process_info.Text = "Assigning color for:\n" + data.Name;

                        currentData.originalData._color = Util.FormColorFromRevit(currentColor);
                        currentData.originalData.Background = new SolidColorBrush(currentData.originalData._color);
                        m_dictFilters.Add(data.Name, currentData);

                }*/
                
                
            }
            csd.Dispose();
            
            if (m_dictFilters.Count == 0)
            {
                System.Windows.MessageBox.Show("Unable to color selection. \n Possible reasons: " +
                    "\n You select items with different hierarchies at the same time" +
                    "\n You select the bottom hierarchy which is meaningless to color");
                return;
            }
            
            process_info.Text = "";
            //create filter list
            Model.UpdateFilter(m_dictFilters);
            Model.ViewFilter();
            DataContext = Model;
        }

        private FilterData createFilterFromData(SourceDataTypes data)
        {
            String filterName = data.Name;
            FilterData m_currentFilterData = new FilterData(this.MainDoc, new List<BuiltInCategory>(), new List<FilterRuleBuilder>());

            //case 1 no child == return empty dictionary: material in by category, type in by material, material in by wwr
            if (data.Hierarchy != SourceDataCategory.ElementSubMaterial && data.Hierarchy !=SourceDataCategory.ElementSubType)
            {
                return m_currentFilterData;
            }

            //case 2: core data == create one filterdata with one rule(type name)
            if (data.RuleName!=null) {
                m_currentFilterData.originalData = data;
                BuiltInParameter curParam = BuiltInParameter.ALL_MODEL_TYPE_NAME;
                FilterRuleBuilder newRule = new FilterRuleBuilder(curParam, RuleCriteraNames.Equals_, data.RuleName);
                m_currentFilterData.RuleData.Add(newRule);
                m_currentFilterData.SetNewCategories(data.BICs.ToList());
                return m_currentFilterData;
            }

            //case 3: has children but not core data == create one filterdata with multiple rules
            modifyFilterRulesFromData(data, m_currentFilterData);
            m_currentFilterData.originalData = data;
            m_currentFilterData.m_originalDataList = new List<SourceDataTypes> { data };
            m_currentFilterData.SetNewCategories(data.BICs.ToList());
            return m_currentFilterData;
        }

        private FilterData createFilterFromDataList(IEnumerable<SourceDataTypes> dataList)
        {
            HashSet<BuiltInCategory> bics = new HashSet<BuiltInCategory>();
            FilterData m_currentFilterData = new FilterData(this.MainDoc, new List<BuiltInCategory>(), new List<FilterRuleBuilder>());
            SourceDataCategory hrc = dataList.First().Hierarchy;
            bool assign = dataList.All(data => data.Hierarchy == hrc);
            if (!assign)
            {
                return m_currentFilterData;
            }
            //m_currentFilterData.m_originalDataList.AddRange(dataList);
            foreach(SourceDataTypes data in dataList)
            {
                m_currentFilterData.AddDataElement(data);
                bics.UnionWith(data.BICs);
            }

            m_currentFilterData.SetNewCategories(bics.ToList());

            return m_currentFilterData;
        }

        private void modifyFilterRulesFromData(SourceDataTypes data,FilterData currentFilterData)
        {

            if (data.RuleName!=null)
            {
                BuiltInParameter curParam = BuiltInParameter.ALL_MODEL_TYPE_NAME;
                FilterRuleBuilder newRule = new FilterRuleBuilder(curParam, RuleCriteraNames.Equals_, data.RuleName) ;
                currentFilterData.RuleData.Add(newRule);
                return;
            }
            if (data.Children == null || data.Children.Count == 0)
            {
                return;
            }
            else
            {
                foreach(SourceDataTypes child in data.Children)
                {
                    modifyFilterRulesFromData(child, currentFilterData);
                }
            }

        }

        public void removeFilterRulesFromData(string ruleName, FilterData currentFilterData)
        {

        }
        private void btnPrint_Click(object sender, RoutedEventArgs e)
        {

            SaveFileDialog dlg = new SaveFileDialog();
            dlg.FileName = "MaterialTakeOff__" + this.MainDoc.Title;
            dlg.DefaultExt = "xlsx";
            dlg.AddExtension = true;
            dlg.Filter = "Excel Workbook (.xlsx)|.xlsx| All files(*.*) | *.* ";
            if (dlg.ShowDialog()==System.Windows.Forms.DialogResult.OK)
            {
                string filename = dlg.FileName;
                AstRevitTool.Core.Export.ASTExportUtils.WriteExcelFromModel(filename, true, this.Model);
            }
        }

        private void btnClearAll_Click(object sender, RoutedEventArgs e)
        {
            Model.Clear();
            
            DataContext = Model;
        }

        private void btnRefresh_Click(object sender, RoutedEventArgs e)
        {
            refreshContext();
            this.contextRefreshed = true;
            System.Windows.MessageBox.Show("Context synced to active view! You can click 'Analysis' to get refreshed result!");
        }
    }


}
