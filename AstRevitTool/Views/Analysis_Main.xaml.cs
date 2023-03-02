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
using Syncfusion.UI.Xaml.TreeGrid;
using System.IO;
using System.Windows.Forms;

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
            foreach(var item in selectedItems)
            {
                SourceDataTypes data = item as SourceDataTypes;
                if(!toRemove.Contains(data.Name))
                {
                    toRemove.Add(data.Name);
                }
            }
            

            Model.RemoveFilter(toRemove);
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
            Dictionary<String, FilterData> m_dictFilters = new Dictionary<String, FilterData>();
            foreach (var item in selectedItems)
            {               
                SourceDataTypes data = item as SourceDataTypes;
                FilterData currentData = createFilterFromData(data);
                if (currentData.RuleData == null || currentData.RuleData.Count == 0)
                {
                    continue;
                }
                process_info.Text = "Assigning color for:\n" + data.Name;
                var csd = new ColorSelectionDialog();
                if (csd.Show() == ItemSelectionDialogResult.Confirmed)
                {
                    Autodesk.Revit.DB.Color currentColor = csd.SelectedColor;
                    currentData.originalData._color = Util.FormColorFromRevit(currentColor);
                    currentData.originalData.Background = new SolidColorBrush(currentData.originalData._color);
                    m_dictFilters.Add(data.Name, currentData);
                }
                csd.Dispose();
            }
            if (m_dictFilters.Count == 0)
            {
                System.Windows.MessageBox.Show("No Available filters to color");
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
            if (data.Children == null || data.Children.Count == 0)
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
            m_currentFilterData.SetNewCategories(data.BICs.ToList());
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
