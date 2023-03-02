using Microsoft.Win32;
using System;
using System.Globalization;

namespace AstRevitTool.Core.Export
{
    public class OptionsGlobal
    {
        public const string REG_KEY_NAME = "Software\\Arrowstreet\\CumulusRevitExporter";
        public const double DEFAULT_SOLIDS_FACTOR = 1.6;
        public const int SETTINGS_VERSION = 352;

        public static int MAX_LOD { get; private set; }

        public static double ManualTessellatorDivider { get; private set; }

        public static void LoadDefaults()
        {
            RegistryKey subKey = Registry.CurrentUser.CreateSubKey("Software\\Arrowstreet\\CumulusRevitExporter", true);
            if (Convert.ToInt32(subKey.GetValue("SettingsVersion", (object)0), (IFormatProvider)CultureInfo.InvariantCulture) < 352)
            {
                subKey.SetValue("SolidsLODFactor", (object)1.6.ToString("0.##", (IFormatProvider)CultureInfo.InvariantCulture), RegistryValueKind.String);
                subKey.SetValue("SettingsVersion", (object)352);
            }
            OptionsGlobal.MAX_LOD = Math.Max(10, Math.Min(15, Convert.ToInt32(subKey.GetValue("SettingsLOD", (object)10), (IFormatProvider)CultureInfo.InvariantCulture)));
            OptionsGlobal.ManualTessellatorDivider = Math.Min((double)(OptionsGlobal.MAX_LOD * 2), Math.Max((double)OptionsGlobal.MAX_LOD, Convert.ToDouble(((string)subKey.GetValue("SettingsMTD", (object)"11.0")).Replace(",", "."), (IFormatProvider)CultureInfo.InvariantCulture)));
            subKey.Close();
        }
    }
}
