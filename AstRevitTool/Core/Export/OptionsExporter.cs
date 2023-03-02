using Autodesk.Revit.DB;

using System;
using System.Globalization;

namespace AstRevitTool.Core.Export
{
    public class OptionsExporter
    {
        private double _solidsFactor;

        public View3D MainView3D { get; set; }

        public string FilePath { get; set; }

        public int InsertionPoint { get; set; }

        public bool SkipInteriorDetails { get; set; }

        public bool CollectTextures { get; set; }

        public bool UnicodeSupport { get; set; }

        public bool ExportNodes { get; set; }

        public int LevelOfDetail { get; set; }

        public bool OptimizeSolids { get; set; }

        public bool MergeIfcMaterials { get; set; }

        public bool MergeLinkedMaterials { get; set; }

        public int SolidsLOD => Math.Max(1, Math.Min(OptionsGlobal.MAX_LOD, (int)Math.Round((double)this.LevelOfDetail / this._solidsFactor)));

        public double ManualTessellatorLOD => (double)this.LevelOfDetail / OptionsGlobal.ManualTessellatorDivider;

        public OptionsExporter() => this.LoadDefaults();

        public void LoadDefaults()
        {
            this.InsertionPoint = 0;
            this.SkipInteriorDetails = false;
            this.CollectTextures = true;
            this.UnicodeSupport = true;
            this.ExportNodes = false;
            this.LevelOfDetail = 4;
            this._solidsFactor = 1.6;
            this.OptimizeSolids = false;
            this.MergeIfcMaterials = true;
            this.MergeLinkedMaterials = true;
        }
        /*
        public void LoadFromRegistry()
        {
            RegistryKey subKey = Registry.CurrentUser.CreateSubKey("Software\\Act-3D\\PluginForRevit");
            this.InsertionPoint = Convert.ToInt32(subKey.GetValue("InsertionPoint", (object)0), (IFormatProvider)CultureInfo.InvariantCulture);
            this.SkipInteriorDetails = Convert.ToBoolean(subKey.GetValue("SkipInteriorDetails", (object)0), (IFormatProvider)CultureInfo.InvariantCulture);
            this.CollectTextures = Convert.ToBoolean(subKey.GetValue("CollectTextures", (object)1), (IFormatProvider)CultureInfo.InvariantCulture);
            this.UnicodeSupport = Convert.ToBoolean(subKey.GetValue("UnicodeSupport", (object)1), (IFormatProvider)CultureInfo.InvariantCulture);
            this.ExportNodes = Convert.ToBoolean(subKey.GetValue("ExportNodes", (object)1), (IFormatProvider)CultureInfo.InvariantCulture);
            this.LevelOfDetail = Math.Min(Convert.ToInt32(subKey.GetValue("LevelOfDetail", (object)4), (IFormatProvider)CultureInfo.InvariantCulture), OptionsGlobal.MAX_LOD);
            this.OptimizeSolids = Convert.ToBoolean(subKey.GetValue("OptimizeSolids", (object)0), (IFormatProvider)CultureInfo.InvariantCulture);
            this.MergeIfcMaterials = Convert.ToBoolean(subKey.GetValue("MergeIfcMaterials", (object)1), (IFormatProvider)CultureInfo.InvariantCulture);
            this.MergeLinkedMaterials = Convert.ToBoolean(subKey.GetValue("MergeLinkedMaterials", (object)1), (IFormatProvider)CultureInfo.InvariantCulture);
            this._solidsFactor = Math.Max(1.0, Math.Min(Convert.ToDouble(((string)subKey.GetValue("SolidsLODFactor", (object)1.6.ToString())).Replace(",", "."), (IFormatProvider)CultureInfo.InvariantCulture), 5.0));
            subKey.Close();
        }

        public void SaveToRegistry()
        {
            RegistryKey subKey = Registry.CurrentUser.CreateSubKey("Software\\Act-3D\\PluginForRevit", true);
            subKey.SetValue("InsertionPoint", (object)this.InsertionPoint);
            subKey.SetValue("SkipInteriorDetails", (object)this.SkipInteriorDetails);
            subKey.SetValue("CollectTextures", (object)this.CollectTextures);
            subKey.SetValue("UnicodeSupport", (object)this.UnicodeSupport);
            subKey.SetValue("ExportNodes", (object)this.ExportNodes);
            subKey.SetValue("LevelOfDetail", (object)this.LevelOfDetail);
            subKey.SetValue("OptimizeSolids", (object)this.OptimizeSolids);
            subKey.SetValue("MergeIfcMaterials", (object)this.MergeIfcMaterials);
            subKey.SetValue("MergeLinkedMaterials", (object)this.MergeLinkedMaterials);
            subKey.Close();
        }*/
    }
}
