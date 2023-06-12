using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AstRevitTool.Core.Analysis;
using AstRevitTool.Core;
using AstRevitTool.Views;
using Autodesk.Revit.UI;
using Autodesk.Revit.DB;
using System.Windows;
using System.Windows.Media;
using System.Collections.ObjectModel;
using System.Windows.Controls.Primitives;

namespace AstRevitTool.Views
{
    public enum RequestId
    {
        None,
        Color,
        Clear,
        Select,
        Hide,
        Update,
        Filter,
        Unhide,
        Uncolor
    }
    public class ASTRequestHandler : IExternalEventHandler
    {
        public RequestId Request { get; set; }
        public object Arg1 { get; set; }
        public object Arg2 { get; set; }

        
        public void Execute(UIApplication app)
        {
            try
            {
                switch (Request)
                {
                    case RequestId.None:
                        return;
                    case RequestId.Color:
                        Color(app);
                        break;
                    case RequestId.Clear:
                        Clear(app);
                        break;
                    case RequestId.Select:
                        break;
                    case RequestId.Hide:
                        Hide(app);
                        break;
                    case RequestId.Update:
                        Update(app);
                        break;
                    case RequestId.Filter:
                        Filter(app);
                        break;
                    case RequestId.Unhide:
                        Unhide(app);
                        break;
                    case RequestId.Uncolor:
                        Uncolor(app);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();

                }
            }
            catch (Exception ex)
            {
                //ignore
            }
        }

        public string GetName()
        {
            return "AST Color Element Request Handler";
        }

        public void Unhide(UIApplication app) {
            if (!(Arg1 is IEnumerable<SourceDataTypes> hidden))
                return;


            var doc = app.ActiveUIDocument.Document;
            var uidoc = new UIDocument(doc);

            HashSet<Element> uniqueSelection = new HashSet<Element>();
            if (hidden.Count() > 0)
            {
                var selectedData = hidden.Cast<SourceDataTypes>().Where(item => item != null);
            }
            foreach (var dataItem in hidden)
            {
                SourceDataTypes data = dataItem as SourceDataTypes;
                uniqueSelection.UnionWith(data.Elements);
            }
            var legalElementIds = uniqueSelection.Where(item => item.Document.Equals(doc)).Select(element => element.Id).ToList();
            using (var trans = new Transaction(doc, "Unhide Elements"))
            {
                trans.Start();
                doc.ActiveView.UnhideElements(legalElementIds);
                trans.Commit();
            }
        }

        public void Uncolor(UIApplication app) {
            if (!(Arg1 is Dictionary<String, FilterData> m_dictFilters))
                return;
            var doc = app.ActiveUIDocument.Document;


            ICollection<String> updatedFilters = new List<String>();
            ICollection<ElementId> deleteElemIds = new List<ElementId>();
            FilteredElementCollector collector = new FilteredElementCollector(doc);
            ICollection<Element> oldFilters = collector.OfClass(typeof(ParameterFilterElement)).ToElements();
            Transaction tran = new Transaction(doc, "Remove View Filter");
            tran.Start();
            try
            {
                // 1. Update existing filters
                foreach (ParameterFilterElement filter in oldFilters)
                {
                    FilterData filterData;
                    bool bExist = m_dictFilters.TryGetValue(filter.Name, out filterData);
                    if (bExist && filterData.m_originalDataList.Count == 0)
                    {
                        deleteElemIds.Add(filter.Id);

                        try
                        {
                            doc.ActiveView.RemoveFilter(filter.Id);
                        }
                        catch
                        {

                        }
                        //continue;
                    }
                    
                }
                //
                // 2. Delete some filters
                if (deleteElemIds.Count > 0)
                    doc.Delete(deleteElemIds);
                //
                // 3. Create new filters(if have)
                foreach (KeyValuePair<String, FilterData> myFilter in m_dictFilters)
                {
                    FilterData currentData = myFilter.Value;
                    
                    if (currentData != null)
                    {
                        if(currentData.m_originalDataList.Count == 0)
                        {

                        }
                        //currentData.originalData._color = Colors.Transparent;
                        //currentData.originalData.Background = new SolidColorBrush(currentData.originalData._color);
                        //currentData.originalData.ColorGroup = currentData.originalData.Name;
                        currentData.m_originalDataList.ForEach(data =>
                        {
                            data._color = Colors.Transparent;
                            data.Background = new SolidColorBrush(data._color);
                            data.ColorGroup = data.RuleName;
                        });
                    }
                }
                
                // 
                // Commit change now
                tran.Commit();
            }
            catch (Exception ex)
            {
                String failMsg = String.Format("Revit filters update failed and was aborted: " + ex.ToString());
                MessageBox.Show(failMsg);
                tran.RollBack();
            }
        }
        public void Color(UIApplication app)
        {
            if (!(Arg1 is Dictionary<string, HashSet<Element>> colorinfo))
                return;

            var doc = app.ActiveUIDocument.Document;
            using (Transaction tr = new Transaction(doc))
            {
                tr.Start("color elements");
                foreach(var kvp in colorinfo)
                {
                    byte[] rgb = Util.HexStringToColor(kvp.Key);
                    Autodesk.Revit.DB.Color rvtColor = new Autodesk.Revit.DB.Color(rgb[0], rgb[1], rgb[2]);
                    OverrideGraphicSettings ogs = new OverrideGraphicSettings();
                    ogs.SetProjectionLineColor(rvtColor);
                    ogs.SetProjectionLineWeight(10);
                    ogs.SetSurfaceForegroundPatternColor(rvtColor);
                    var patternCollector = new FilteredElementCollector(doc.ActiveView.Document);
                    patternCollector.OfClass(typeof(Autodesk.Revit.DB.FillPatternElement));
                    Autodesk.Revit.DB.FillPatternElement solidFill = patternCollector.ToElements().Cast<Autodesk.Revit.DB.FillPatternElement>().First(x => x.GetFillPattern().IsSolidFill);
                    ogs.SetSurfaceForegroundPatternId(solidFill.Id);

                    foreach (Element e in kvp.Value)
                    {
                        if (e.Document.Equals(doc))
                        {
                            doc.ActiveView.SetElementOverrides(e.Id, ogs);
                        }
                    }
                    
                }
                
                tr.Commit();
            }
        }

        public void Update(UIApplication app)
        {
            if (!(Arg1 is ElementsVisibleInViewExportContext context))
                return;
            var doc = app.ActiveUIDocument.Document;
            
            var newcontext = new ElementsVisibleInViewExportContext(doc);
            if (doc.ActiveView as View3D != null)
            {
                using (var trans = new Transaction(doc, "Update model context"))
                {
                    trans.Start();
                    Core.Export.ASTExportUtils.ExportView3D(doc, doc.ActiveView, app, out newcontext);
                    context.Elements = newcontext.Elements;
                    trans.Commit();
                }
            }
                
            else
                return;
            
        }
        public void Hide(UIApplication app)
        {
            if (!(Arg1 is IEnumerable<SourceDataTypes> selected))
                return;

            
            var doc = app.ActiveUIDocument.Document;
            var uidoc = new UIDocument(doc);

            HashSet<Element> uniqueSelection = new HashSet<Element>();
            if (selected.Count() > 0)
            {
                var selectedData = selected.Cast<SourceDataTypes>().Where(item => item != null);
            }
            foreach (var dataItem in selected)
            {
                SourceDataTypes data = dataItem as SourceDataTypes;
                uniqueSelection.UnionWith(data.Elements);
            }
            var legalElementIds = uniqueSelection.Where(item => item.Document.Equals(doc)).Select(element => element.Id).ToList();
            using (var trans = new Transaction(doc, "Hide Elements"))
            {
                trans.Start();
                doc.ActiveView.HideElements(legalElementIds);
                trans.Commit();
            }

        }
        public void Clear(UIApplication app)
        {
            if (!(Arg1 is IEnumerable<Element> allelements))
                return;
            var doc = app.ActiveUIDocument.Document;
            View activeView = doc.ActiveView;
            ICollection<ElementId> collector = activeView.GetFilters();
            //collector.OfClass(typeof(FilterElement));
            //ICollection<Element> oldFilters = collector.OfClass(typeof(ParameterFilterElement)).ToElements();
            OverrideGraphicSettings ogs = new OverrideGraphicSettings();
            using (Transaction tr = new Transaction(doc))
            {
                tr.Start("resume color");
                foreach(ElementId element in collector)
                {
                    try { doc.ActiveView.SetFilterOverrides(element,ogs);
                        doc.ActiveView.RemoveFilter(element);
                    }
                    catch { continue; }
                    //doc.Delete(element.Id);
                }
                foreach (Element el in allelements)
                {
                    ElementId id = el.Id;
                    if (el.Document.Equals(doc))
                    {
                        doc.ActiveView.SetElementOverrides(id, ogs);
                    }
                }
                tr.Commit();
            }
        }

        public void Filter(UIApplication app)
        {
            if (!(Arg1 is Dictionary<String, FilterData> m_dictFilters))
                return;
            var doc = app.ActiveUIDocument.Document;
            bool autoassign = false;

            Dictionary<string,Element> updatedFilters = new Dictionary<string, Element>();
            ICollection<ElementId> deleteElemIds = new List<ElementId>();
            FilteredElementCollector collector = new FilteredElementCollector(doc);
            ICollection<Element> oldFilters = collector.OfClass(typeof(ParameterFilterElement)).ToElements();
            ICollection<ElementId> CurrentFiltercollector = doc.ActiveView.GetFilters();
            Transaction tran = new Transaction(doc, "Update View Filter");
            tran.Start();
            try
            {
                // 1. Update existing filters
                foreach (ParameterFilterElement filter in oldFilters)
                {
                    FilterData filterData;
                    bool bExist = m_dictFilters.TryGetValue(filter.Name, out filterData);
                    if (!bExist) { continue; }
                    if (filterData.m_originalDataList.Count == 0)
                    {
                        deleteElemIds.Add(filter.Id);
                        m_dictFilters.Remove(filter.Name);

                        continue;
                    }
                    //
                    // Update Filter categories for this filter
                    ICollection<ElementId> newCatIds = filterData.GetCategoryIds();
                    if (!ListCompareUtility<ElementId>.Equals(filter.GetCategories(), newCatIds))
                    {
                        filter.SetCategories(newCatIds);
                    }

                    // Update filter rules for this filter
                    //IList<FilterRule> newRules = new List<FilterRule>();

                    // Updated version: update nested filter rules
                    IList<FilterRule[]> newNestedRules = new List<FilterRule[]>();
                    foreach (var s in filterData.m_originalDataList)
                    {
                        if (s.Hierarchy == SourceDataCategory.ElementCategory || s.Hierarchy == SourceDataCategory.ElementMaterial)
                        {
                            continue;
                        }
                        else {
                            var rules = FilterData.RetrieveFilterRulesFromData(s);
                            newNestedRules.Add(rules);
                        }
                    }
                    /*
                    foreach (FilterRuleBuilder ruleData in filterData.RuleData)
                    {
                        newRules.Add(ruleData.AsFilterRule());
                    }*/

                    //  old version
                    //ElementFilter elemFilter = AstRevitTool.Core.FilterUtil.CreateElementORFilterFromFilterRules(newRules);
                    
                    // new version
                    ElementFilter elemFilter = AstRevitTool.Core.FilterUtil.CreateNestedORFilterFromFilterRules(newNestedRules);
                    // Set this filter's list of rules.
                    filter.SetElementFilter(elemFilter);

                    //Update the filter's color
                    Autodesk.Revit.DB.Color rvtc;
                    var clr = filterData.m_originalDataList.First().Color;
                    rvtc = Util.RevitColorFromForm(clr);
                    OverrideGraphicSettings ogs = new OverrideGraphicSettings();
                    ogs.SetProjectionLineColor(rvtc);
                    ogs.SetProjectionLineWeight(10);
                    ogs.SetSurfaceForegroundPatternColor(rvtc);
                    var patternCollector = new FilteredElementCollector(doc.ActiveView.Document);
                    patternCollector.OfClass(typeof(Autodesk.Revit.DB.FillPatternElement));
                    Autodesk.Revit.DB.FillPatternElement solidFill = patternCollector.ToElements().Cast<Autodesk.Revit.DB.FillPatternElement>().First(x => x.GetFillPattern().IsSolidFill);
                    ogs.SetSurfaceForegroundPatternId(solidFill.Id);
                    doc.ActiveView.SetFilterOverrides(filter.Id, ogs);

                    // Remember that we updated this filter so that we do not try to create it again below.

                    updatedFilters.Add(filter.Name,filter);
                }
                //
                // 2. Delete some filters
                
                // 2.a Disable active filter

                foreach (ElementId element in CurrentFiltercollector)
                {
                    if (deleteElemIds.Contains(element)) {
                        doc.ActiveView.RemoveFilter(element);
                    }
                }

                if (deleteElemIds.Count > 0)
                    doc.Delete(deleteElemIds);
                // 3. Create new filters(if have)
                ICollection<Element> newFilters = new Collection<Element>();
                foreach (KeyValuePair<String, FilterData> myFilter in m_dictFilters)
                {
                    // If this filter was updated in the previous step, do nothing.
                    if (updatedFilters.Keys.Contains(myFilter.Key))
                    {
                        //newFilters.Add(updatedFilters[myFilter.Key]);
                        continue;
                    }

                    // Create a new filter. OLD VERSION
                    // Collect the FilterRules, create an ElementFilter representing the
                    // conjunction ("ANDing together") of the FilterRules, and use the ElementFilter
                    // to create a ParameterFilterElement
                    //IList<FilterRule> rules = new List<FilterRule>();

                    IList<FilterRule[]> nestedRules = new List<FilterRule[]>();
                    /*foreach (FilterRuleBuilder ruleData in myFilter.Value.RuleData)
                    {
                        rules.Add(ruleData.AsFilterRule());
                    }*/

                    foreach(var data in myFilter.Value.m_originalDataList) {
                        if (data.Hierarchy == SourceDataCategory.ElementCategory || data.Hierarchy == SourceDataCategory.ElementMaterial)
                        {
                            continue;
                        }
                        else
                        {
                            nestedRules.Add(FilterData.RetrieveFilterRulesFromData(data));
                        }
                    }

                    //OLD VERSION OF FILTERS
                    //ElementFilter elemFilter = FilterUtil.CreateElementORFilterFromFilterRules(rules);

                    ElementFilter elemFilter = FilterUtil.CreateNestedORFilterFromFilterRules(nestedRules);
                    
                    // Check that the ElementFilter is valid for use by a ParameterFilterElement.
                    IList<ElementId> categoryIdList = myFilter.Value.GetCategoryIds();
                    ISet<ElementId> categoryIdSet = new HashSet<ElementId>(categoryIdList);
                    if (!ParameterFilterElement.ElementFilterIsAcceptableForParameterFilterElement(
                       doc, categoryIdSet, elemFilter))
                    {
                        // In case the UI allowed invalid rules, issue a warning to the user.
                        MessageBox.Show("The combination of filter rules is not acceptable for a View Filter.");
                    }
                    else
                    {
                        var ele = ParameterFilterElement.Create(doc, myFilter.Key, categoryIdSet, elemFilter);
                        newFilters.Add(ele);
                        
                    }
                }
                //ICollection<Element> newFilters = collector.OfClass(typeof(ParameterFilterElement)).ToElements();
                Autodesk.Revit.DB.Color rvtColor;
                int count = 0;
                foreach(var filter in newFilters)
                {
                    string key = filter.Name;
                    FilterData currentData;
                    m_dictFilters.TryGetValue(key,out currentData);
                    if (currentData == null || currentData.m_originalDataList.Count == 0)
                    {
                        continue;
                    }
                    
                    doc.ActiveView.AddFilter(filter.Id);
                    doc.ActiveView.SetFilterVisibility(filter.Id, true);
                    if (autoassign && count < Constants.ColourValues.Length)
                    {
                        string color = Constants.ColourValues[count];
                        byte[] rgb = Util.HexStringToColor(color);
                        if (currentData != null)
                        {
                            if(currentData.m_originalDataList.Count > 0) { 

                            }
                            else
                            {
                                //currentData.originalData._color = System.Windows.Media.Color.FromRgb(rgb[0], rgb[1], rgb[2]);
                                //currentData.originalData.Background = new SolidColorBrush(currentData.originalData._color);
                            }
                            /*
                            currentData.originalData._color = System.Windows.Media.Color.FromRgb(rgb[0], rgb[1], rgb[2]);
                            currentData.originalData.Background = new SolidColorBrush(currentData.originalData._color);*/
                        }
                        rvtColor = new Autodesk.Revit.DB.Color(rgb[0], rgb[1], rgb[2]);
                    }
                    else
                    {
                        var clr = currentData.m_originalDataList.First().Color;
                        rvtColor = Util.RevitColorFromForm(clr);
                        //rvtColor = Util.RevitColorFromForm(currentData.originalData.Color);
                        //currentData.originalData.Background = new SolidColorBrush(currentData.originalData._color);
                    }
                    OverrideGraphicSettings ogs = new OverrideGraphicSettings();
                    ogs.SetProjectionLineColor(rvtColor);
                    ogs.SetProjectionLineWeight(10);
                    ogs.SetSurfaceForegroundPatternColor(rvtColor);
                    var patternCollector = new FilteredElementCollector(doc.ActiveView.Document);
                    patternCollector.OfClass(typeof(Autodesk.Revit.DB.FillPatternElement));
                    Autodesk.Revit.DB.FillPatternElement solidFill = patternCollector.ToElements().Cast<Autodesk.Revit.DB.FillPatternElement>().First(x => x.GetFillPattern().IsSolidFill);
                    ogs.SetSurfaceForegroundPatternId(solidFill.Id);
                    doc.ActiveView.SetFilterOverrides(filter.Id, ogs);


                    count++;
                }
                // 
                // Commit change now
                tran.Commit();
            }
            catch (Exception ex)
            {
                String failMsg = String.Format("Revit filters update failed and was aborted: " + ex.ToString());
                MessageBox.Show(failMsg);
                tran.RollBack();
            }
        }
    }
}
