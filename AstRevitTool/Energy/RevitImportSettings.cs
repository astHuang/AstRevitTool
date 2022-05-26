using CarboLifeAPI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace AstRevitTool.Energy
{
    public class RevitImportSettings
    {
        public string MainCategory { get; set; }
        public string SubCategory { get; set; }
        public string CutoffLevel { get; set; }
        public double CutoffLevelValue { get; set; }
        public bool IncludeDemo { get; set; }
        public bool IncludeExisting { get; set; }

        public RevitImportSettings()
        {
            MainCategory = "(Revit) Category";
            SubCategory = "";
            CutoffLevel = "Ground Floor";
            CutoffLevelValue = 0;
            IncludeDemo = false;
            IncludeExisting = false;
        }

        public RevitImportSettings DeSerializeXML()
        {
            string importSettingsPath = PathUtils.getRevitImportSettingspath();

            if (File.Exists(importSettingsPath))
            {
                try
                {
                    XmlSerializer ser = new XmlSerializer(typeof(RevitImportSettings));
                    RevitImportSettings bufferproject;

                    using (FileStream fs = new FileStream(importSettingsPath, FileMode.Open))
                    {
                        bufferproject = ser.Deserialize(fs) as RevitImportSettings;
                    }

                    return bufferproject;
                }
                catch (Exception ex)
                {
                    System.Windows.MessageBox.Show(ex.Message);
                    return null;
                }
            }
            else
            {
                RevitImportSettings newsettings = new RevitImportSettings();
                newsettings.SerializeXML();
                return newsettings;
            }
        }
        public bool SerializeXML()
        {
            string importSettingsPath = PathUtils.getRevitImportSettingspath();

            bool result = false;
            try
            {
                XmlSerializer ser = new XmlSerializer(typeof(RevitImportSettings));

                using (FileStream fs = new FileStream(importSettingsPath, FileMode.Create))
                {
                    ser.Serialize(fs, this);
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(ex.Message);
                return false;
            }

            return result;
        }

    }
}
