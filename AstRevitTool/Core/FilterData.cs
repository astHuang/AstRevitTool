﻿using System;
using System.Text;
using System.Windows.Forms;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using AstRevitTool.Core;
using System.Diagnostics;

using Autodesk.Revit;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.DB.Architecture;
using PFRF = Autodesk.Revit.DB.ParameterFilterRuleFactory;
using AstRevitTool.Views;
using System.Web.UI.WebControls;

namespace AstRevitTool.Core
{
    /// <summary>
    /// Sample custom immutable class used to represents Revit internal FilterRule.
    /// This class and its variables will help display the contents of UI controls.
    /// This class can build its data caches to Revit FilterRule object.
    /// </summary>
    public sealed class FilterRuleBuilder
    {
        #region Class Member Variables
        /// <summary>
        /// Parameter of filter rule
        /// </summary>
        public BuiltInParameter Parameter { get; private set; }

        /// <summary>
        /// Filter rule criteria(in String type)
        /// </summary>
        public String RuleCriteria { get; private set; }

        /// <summary>
        /// Rule values in string 
        /// </summary>
        public String RuleValue { get; private set; }

        /// <summary>
        /// Parameter storage type of current FilterRule.
        /// </summary>
        public StorageType ParamType { get; private set; }

        /// <summary>
        /// Tolerance of double comparison, valid only when ParamType is double
        /// </summary>
        public double Epsilon { get; private set; }

        #endregion

        #region Class Public Properties

        /// <summary>
        /// Get ElementId of current parameter 
        /// </summary>
        public ElementId ParamId
        {
            get { return new ElementId(Parameter); }
        }
        #endregion

        #region Class Public Methods
        /// <summary>
        /// Create FilterRuleBuilder for String FilterRule
        /// </summary>
        /// <param name="param">Parameter of FilterRule.</param>
        /// <param name="ruleCriteria">Rule criteria.</param>
        /// <param name="ruleValue">Rule value.</param>
        public FilterRuleBuilder(BuiltInParameter param, String ruleCriteria, String ruleValue)
        {
            InitializeMemebers();
            //
            // set data with specified values
            ParamType = StorageType.String;
            Parameter = param;
            RuleCriteria = ruleCriteria;
            RuleValue = ruleValue;
        }

        /// <summary>
        /// Create FilterRuleBuilder for double FilterRule
        /// </summary>
        /// <param name="param">Parameter of FilterRule.</param>
        /// <param name="ruleCriteria">Rule criteria.</param>
        /// <param name="ruleValue">Rule value.</param>
        /// <param name="tolerance">Epsilon for double values comparison.</param>
        public FilterRuleBuilder(BuiltInParameter param, String ruleCriteria, double ruleValue, double tolearance)
        {
            InitializeMemebers();
            //
            // set data with specified values
            ParamType = StorageType.Double;
            Parameter = param;
            RuleCriteria = ruleCriteria;
            RuleValue = ruleValue.ToString();
            Epsilon = tolearance;
        }

        /// <summary>
        /// Create FilterRuleBuilder for int FilterRule
        /// </summary>
        /// <param name="param">Parameter of FilterRule.</param>
        /// <param name="ruleCriteria">Rule criteria.</param>
        /// <param name="ruleValue">Rule value.</param>
        public FilterRuleBuilder(BuiltInParameter param, String ruleCriteria, int ruleValue)
        {
            InitializeMemebers();
            //
            // set data with specified values
            ParamType = StorageType.Integer;
            Parameter = param;
            RuleCriteria = ruleCriteria;
            RuleValue = ruleValue.ToString();
        }

        /// <summary>
        /// Create FilterRuleBuilder for ElementId FilterRule
        /// </summary>
        /// <param name="param">Parameter of FilterRule.</param>
        /// <param name="ruleCriteria">Rule criteria.</param>
        /// <param name="ruleValue">Rule value.</param>
        public FilterRuleBuilder(BuiltInParameter param, String ruleCriteria, ElementId ruleValue)
        {
            InitializeMemebers();
            //
            // set data with specified values
            ParamType = StorageType.ElementId;
            Parameter = param;
            RuleCriteria = ruleCriteria;
            RuleValue = ruleValue.ToString();
        }

        /// <summary>
        /// Create API FilterRule according to sample's FilterRuleBuilder        
        /// </summary>
        /// <returns>API FilterRule converted from current FilterRuleBuilder.</returns>
        public FilterRule AsFilterRule()
        {
            ElementId paramId = new ElementId(Parameter);
            if (ParamType == StorageType.String)
            {
                switch (RuleCriteria)
                {
                    case RuleCriteraNames.BeginWith:
                        return PFRF.CreateBeginsWithRule(paramId, RuleValue, false);
                    case RuleCriteraNames.Contains:
                        return PFRF.CreateContainsRule(paramId, RuleValue,false);
                    case RuleCriteraNames.EndsWith:
                        return PFRF.CreateEndsWithRule(paramId, RuleValue, false);
                    case RuleCriteraNames.Equals_:
                        return PFRF.CreateEqualsRule(paramId, RuleValue, false);
                    case RuleCriteraNames.Greater:
                        return PFRF.CreateGreaterRule(paramId, RuleValue, false);
                    case RuleCriteraNames.GreaterOrEqual:
                        return PFRF.CreateGreaterOrEqualRule(paramId, RuleValue, false);
                    case RuleCriteraNames.Less:
                        return PFRF.CreateLessRule(paramId, RuleValue, false);
                    case RuleCriteraNames.LessOrEqual:
                        return PFRF.CreateLessOrEqualRule(paramId, RuleValue, false);
                    case RuleCriteraNames.NotBeginWith:
                        return PFRF.CreateNotBeginsWithRule(paramId, RuleValue, false);
                    case RuleCriteraNames.NotContains:
                        return PFRF.CreateNotContainsRule(paramId, RuleValue, false);
                    case RuleCriteraNames.NotEndsWith:
                        return PFRF.CreateNotEndsWithRule(paramId, RuleValue, false);
                    case RuleCriteraNames.NotEquals:
                        return PFRF.CreateNotEqualsRule(paramId, RuleValue, false);
                }
            }
            else if (ParamType == StorageType.Double)
            {
                switch (RuleCriteria)
                {
                    case RuleCriteraNames.Equals_:
                        return PFRF.CreateEqualsRule(paramId, double.Parse(RuleValue), Epsilon);
                    case RuleCriteraNames.Greater:
                        return PFRF.CreateGreaterRule(paramId, double.Parse(RuleValue), Epsilon);
                    case RuleCriteraNames.GreaterOrEqual:
                        return PFRF.CreateGreaterOrEqualRule(paramId, double.Parse(RuleValue), Epsilon);
                    case RuleCriteraNames.Less:
                        return PFRF.CreateLessRule(paramId, double.Parse(RuleValue), Epsilon);
                    case RuleCriteraNames.LessOrEqual:
                        return PFRF.CreateLessOrEqualRule(paramId, double.Parse(RuleValue), Epsilon);
                    case RuleCriteraNames.NotEquals:
                        return PFRF.CreateNotEqualsRule(paramId, double.Parse(RuleValue), Epsilon);
                }
            }
            else if (ParamType == StorageType.Integer)
            {
                switch (RuleCriteria)
                {
                    case RuleCriteraNames.Equals_:
                        return PFRF.CreateEqualsRule(paramId, int.Parse(RuleValue));
                    case RuleCriteraNames.Greater:
                        return PFRF.CreateGreaterRule(paramId, int.Parse(RuleValue));
                    case RuleCriteraNames.GreaterOrEqual:
                        return PFRF.CreateGreaterOrEqualRule(paramId, int.Parse(RuleValue));
                    case RuleCriteraNames.Less:
                        return PFRF.CreateLessRule(paramId, int.Parse(RuleValue));
                    case RuleCriteraNames.LessOrEqual:
                        return PFRF.CreateLessOrEqualRule(paramId, int.Parse(RuleValue));
                    case RuleCriteraNames.NotEquals:
                        return PFRF.CreateNotEqualsRule(paramId, int.Parse(RuleValue));
                }
            }
            else if (ParamType == StorageType.ElementId)
            {
                switch (RuleCriteria)
                {
                    case RuleCriteraNames.Equals_:
                        return PFRF.CreateEqualsRule(paramId,new ElementId(int.Parse(RuleValue)));
                    case RuleCriteraNames.Greater:
                        return PFRF.CreateGreaterRule(paramId, new ElementId(int.Parse(RuleValue)));
                    case RuleCriteraNames.GreaterOrEqual:
                        return PFRF.CreateGreaterOrEqualRule(paramId, new ElementId(int.Parse(RuleValue)));
                    case RuleCriteraNames.Less:
                        return PFRF.CreateLessRule(paramId, new ElementId(int.Parse(RuleValue)));
                    case RuleCriteraNames.LessOrEqual:
                        return PFRF.CreateLessOrEqualRule(paramId, new ElementId(int.Parse(RuleValue)));
                    case RuleCriteraNames.NotEquals:
                        return PFRF.CreateNotEqualsRule(paramId, new ElementId(int.Parse(RuleValue)));
                }
            }
            //
            // Throw exception for others
            throw new System.NotImplementedException("This filter rule or criteria is not implemented yet.");
        }
        #endregion

        #region Class Implementations
        /// <summary>
        /// Make sure all members are initialized with expected values.
        /// </summary>
        private void InitializeMemebers()
        {
            Parameter = BuiltInParameter.INVALID;
            RuleCriteria = String.Empty;
            RuleValue = String.Empty;
            ParamType = StorageType.None;
            Epsilon = 0.0f;
        }
        #endregion
    }

    /// <summary>
    /// This class used to represents data for one API filter.
    /// It consists of BuiltInCategory and filter rules
    /// </summary>
    public sealed class FilterData
    {
        #region Class Members
        /// <summary>
        /// Reserves current active document
        /// </summary>
        Autodesk.Revit.DB.Document m_doc;

        /// <summary>
        /// BuiltInCategories of filter
        /// </summary>
        List<BuiltInCategory> m_filterCategories;

        /// <summary>
        /// Filer rules of filter
        /// </summary>
        List<FilterRuleBuilder> m_filterRules;
        public SourceDataTypes originalData { get; set; }

        public List<SourceDataTypes> m_originalDataList { get; set; }
        #endregion

        #region Public Class Methods
        /// <summary>
        /// Get BuiltInCategories of filter
        /// </summary>
        public List<BuiltInCategory> FilterCategories
        {
            get { return m_filterCategories; }
        }

        /// <summary>
        /// Get BuiltInCategory Ids of filter
        /// </summary>
        public IList<ElementId> GetCategoryIds()
        {
            List<ElementId> catIds = new List<ElementId>();
            foreach (BuiltInCategory cat in m_filterCategories)
                catIds.Add(new ElementId(cat));
            return catIds;
        }

        public static FilterRule[] RetrieveFilterRulesFromData(SourceDataTypes data)
        {

            //Basic operation in 'By-Category'
            if(data.RuleName != null)
            {
                if(data.Family != null)
                {
                    FilterRule[] rules = new FilterRule[2];
                    FilterRuleBuilder familyRule = new FilterRuleBuilder(BuiltInParameter.ALL_MODEL_FAMILY_NAME, RuleCriteraNames.Equals_, data.FamilyName);
                    rules[0] = familyRule.AsFilterRule();
                    FilterRuleBuilder typeRule = new FilterRuleBuilder(BuiltInParameter.ALL_MODEL_TYPE_NAME, RuleCriteraNames.Equals_, data.RuleName);
                    rules[1] = typeRule.AsFilterRule();
                    return rules;
                }
                else
                {
                    FilterRule[] rules = new FilterRule[1];
                    FilterRuleBuilder typeRule = new FilterRuleBuilder(BuiltInParameter.ALL_MODEL_TYPE_NAME, RuleCriteraNames.Equals_, data.RuleName);
                    rules[0] = typeRule.AsFilterRule();
                    return rules;
                }
            }

            if(data.Children == null || data.Children.Count == 0)
            {
                FilterRule[] rules = new FilterRule[0];
                return rules;
            }

            else
            {
                throw new Exception("Can't retrieve rules of hierarchy in Category or Material level");
            }
        }

        public void AddDataElement(SourceDataTypes data)
        {
           
            if (data.RuleName != null)
            {
                BuiltInParameter curParam = BuiltInParameter.ALL_MODEL_TYPE_NAME;
                this.m_originalDataList.RemoveAll(S => S.Name == data.Name);
                this.RuleData.RemoveAll(rule => rule.RuleValue == data.RuleName);

                FilterRuleBuilder newRule = new FilterRuleBuilder(curParam, RuleCriteraNames.Equals_, data.RuleName);
                this.m_originalDataList.Add(data);
                this.RuleData.Add(newRule);
                return;
            }
            if (data.Children == null || data.Children.Count == 0)
            {
                return;
            }

            //Recursively add element data and rules: For category and material 
            else
            {
                this.m_originalDataList.RemoveAll(s => s.Name == data.Name);
                this.m_originalDataList.Add(data);

                foreach (SourceDataTypes child in data.Children)
                {
                    AddDataElement(child);
                }
            }
        }

        public void RemoveDataElement(SourceDataTypes data)
        {
            if (data.RuleName != null)
            {
                this.m_originalDataList.RemoveAll(S => S.Name == data.Name);
                //this.RuleData.RemoveAll(rule => rule.RuleValue == data.RuleName);
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
                    RemoveDataElement(child);
                }
            }
        }

        /// <summary>
        /// Set new categories, this method will possibly update existing criteria 
        /// </summary>
        /// <param name="newCatIds">New categories for current filter.</param>
        /// <returns>true if categories or criteria are changed; otherwise false.</returns>
        /// <remarks>
        /// If someone parameter of criteria cannot be supported by new categories, 
        /// the old criteria will be cleaned and set to empty
        /// </remarks>
        public bool SetNewCategories(List<BuiltInCategory> newCats)
        {
            // do nothing if new categories are equals to old categories
            if (ListCompareUtility<BuiltInCategory>.Equals(newCats, m_filterCategories))
                return false;
            m_filterCategories = newCats; // update categories

            List<ElementId> newCatIds = new List<ElementId>();
            foreach (BuiltInCategory cat in newCats)
            {
                newCatIds.Add(new ElementId(cat));
            }
            //
            // Check if need to update file rules:
            // . if filer rule is empty, do nothing
            // . if some parameters of rules cannot be supported by new categories, clean all old rules
            ICollection<ElementId> supportParams =
                ParameterFilterUtilities.GetFilterableParametersInCommon(m_doc, newCatIds);
            foreach (FilterRuleBuilder rule in m_filterRules)
            {
                if (!supportParams.Contains(new ElementId(rule.Parameter)))
                {
                    m_filterRules.Clear();
                    break;
                }
            }
            return true;
        }

        /// <summary>
        /// Get FilterRuleBuilder of API filter's rules
        /// </summary>
        public List<FilterRuleBuilder> RuleData
        {
            get { return m_filterRules; }
        }

        /// <summary>
        /// Create sample custom FilterData with specified categories and FilterRuleBuilder
        /// </summary>
        /// <param name="doc">Revit active document.</param>
        /// <param name="categories">BuilInCategories of filter.</param>
        /// <param name="filterRules">FilterRuleBuilder set of filter.</param>
        public FilterData(Autodesk.Revit.DB.Document doc,
            ICollection<BuiltInCategory> categories, ICollection<FilterRuleBuilder> filterRules)
        {
            m_doc = doc;
            m_originalDataList = new List<SourceDataTypes>();
            m_filterCategories = new List<BuiltInCategory>();
            m_filterCategories.AddRange(categories);
            m_filterRules = new List<FilterRuleBuilder>();
            m_filterRules.AddRange(filterRules);
        }

        /// <summary>
        /// Create sample custom FilterData with specified category id and FilterRuleBuilder
        /// </summary>
        /// <param name="doc">Revit active document.</param>
        /// <param name="categories">BuilInCategory ids of filter.</param>
        /// <param name="filterRules">FilterRuleBuilder set of filter.</param>
        public FilterData(Autodesk.Revit.DB.Document doc,
            ICollection<ElementId> categories, ICollection<FilterRuleBuilder> filterRules)
        {
            m_doc = doc;
            m_filterCategories = new List<BuiltInCategory>();
            foreach (ElementId catId in categories)
                m_filterCategories.Add((BuiltInCategory)catId.IntegerValue);
            m_filterRules = new List<FilterRuleBuilder>();
            m_filterRules.AddRange(filterRules);
        }

        
        #endregion
    }

    /// <summary>
    /// This class define constant strings to map rule criteria
    /// </summary>
    public sealed class RuleCriteraNames
    {
        #region Public Class Members
        /// <summary>
        /// String represents BeginWith criteria
        /// </summary>
        public const String BeginWith = "begins with";

        /// <summary>
        /// String represents Contains criteria
        /// </summary>
        public const String Contains = "contains";

        /// <summary>
        /// String represents EndWith criteria
        /// </summary>
        public const String EndsWith = "ends with";

        /// <summary>
        /// String represents Equals criteria
        /// </summary>
        public const String Equals_ = "equals";

        /// <summary>
        /// String represents GreaterThan criteria
        /// </summary>
        public const String Greater = "is greater than";

        /// <summary>
        /// String represents GreaterOrEqual criteria
        /// </summary>
        public const String GreaterOrEqual = "is greater than or equal to";

        /// <summary>
        /// String represents LessOrEqual criteria
        /// </summary>
        public const String LessOrEqual = "is less than or equal to";

        /// <summary>
        /// String represents Less criteria
        /// </summary>
        public const String Less = "is less than";

        /// <summary>
        /// String represents NotBeginWith criteria
        /// </summary>
        public const String NotBeginWith = "does not begin with";

        /// <summary>
        /// String represents NotContains criteria
        /// </summary>
        public const String NotContains = "does not contain";

        /// <summary>
        /// String represents NotEndsWith criteria
        /// </summary>
        public const String NotEndsWith = "does not end with";

        /// <summary>
        /// String represents NotEquals criteria
        /// </summary>
        public const String NotEquals = "does not equal";

        /// <summary>
        /// Invalid criteria 
        /// </summary>
        public const String Invalid = "n/a";
        #endregion

        /// <summary>
        /// Hide ctor, this class defines only static members, no need to be created
        /// </summary>
        private RuleCriteraNames() { }

        /// <summary>
        /// Get all supported criteria(in string) according to StorageType of parameter
        /// </summary>
        /// <param name="paramType">Parameter type.</param>
        /// <returns>String list of criteria supported for specified parameter type.</returns>
        public static ICollection<String> Criterions(StorageType paramType)
        {
            ICollection<String> returns = new List<String>();
            //
            // all parameter supports following criteria
            returns.Add(Equals_);
            returns.Add(Greater);
            returns.Add(GreaterOrEqual);
            returns.Add(LessOrEqual);
            returns.Add(Less);
            returns.Add(NotEquals);
            // 
            // Only string parameter support criteria below
            if (paramType == StorageType.String)
            {
                returns.Add(BeginWith);
                returns.Add(Contains);
                returns.Add(EndsWith);
                returns.Add(NotBeginWith);
                returns.Add(NotContains);
                returns.Add(NotEndsWith);
            }
            return returns;
        }
    }
}
