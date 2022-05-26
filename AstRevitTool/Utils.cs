using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Management;
using System.Net.NetworkInformation;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Windows.Media.Imaging;
using System.Windows.Forms;
using CarboLifeAPI;
using CarboLifeAPI.Data;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using AstRevitTool.Energy;


namespace AstRevitTool
{
    internal class Utils
    {
        internal static void selectElements(UIDocument uiDoc, List<Element> elems)
        {
            try
            {
                uiDoc.Selection.SetElementIds((ICollection<ElementId>)new List<ElementId>(elems.Select<Element, ElementId>((Func<Element, ElementId>)(e => e.Id))));
            }
            catch
            {

            }
        }

        internal static Parameter findParameter(Element e, string name)
        {
            try
            {
                return e.LookupParameter(name);
            }
            catch
            {
                return null;
            }
        }

        internal static string linkDoc(RevitLinkInstance inst)
        {
            try
            {
                return inst.GetLinkDocument().Title;
            }
            catch 
            {
                return "";
            }
        }

        internal static void isolate(Document doc,List<ElementId> ids)
        {
            try
            {
                ICollection<ElementId> elems = new List<ElementId>();
                foreach (ElementId id in ids)
                {
                    elems.Add(id);
                }
                doc.ActiveView.IsolateElementsTemporary(elems);
            }
            catch { }
        }

        internal static double area(Element elem,Material mat)
        {
            try
            {
                return elem.GetMaterialArea(((Element)mat).Id, false);
            }
            catch
            {
                return 0.0;
            }
        }

        internal static double polygonArea(CurveLoop c)
        {
            try
            {
                double value = 0.5 * Enumerable.Sum<Curve>((IEnumerable<Curve>)Enumerable.ToList<Curve>((IEnumerable<Curve>)c), (Func<Curve, double>)delegate (Curve curve)
                  {
                      XYZ end = curve.GetEndPoint(0);
                      XYZ start = curve.GetEndPoint(1);
                      return start.X * end.Y - end.X * start.Y;
                  });
                return value;
            }
            catch
            {
                return 0.0;
            }
        }

        internal static double volu(Element elem, Material mat)
        {
            try
            {
                return elem.GetMaterialVolume(((Element)mat).Id);
            }
            catch
            {
                return 0.0;
            }
        }

        internal static List<Material> mats(Element elem)
        {
            List<Material> list = new List<Material>();
            try
            {
                foreach (ElementId materialId in elem.GetMaterialIds(false))
                {
                    Element element = elem.Document.GetElement(materialId);
                    list.Add(element as Material);
                }
                return list;
            }
            catch
            {
                return list;
            }
        }

        internal static double pVal(FamilyInstance fi , BuiltInParameter[] bis)
        {
            foreach(BuiltInParameter b in bis)
            {
                try
                {
                    double num = ((Element)fi).get_Parameter(b).AsDouble();
                    if(num > 0) { return num; }

                }
                catch {  }

            }
            foreach(BuiltInParameter b2 in bis)
            {
                try
                {
                    double num2 = fi.Symbol.get_Parameter(b2).AsDouble();
                    if(num2 > 0) { return num2; }
                }
                catch { }
            }
            return 0.0;
        }

        internal static string getStringParam(Element elem, BuiltInParameter p)
        {
            if (elem != null)
            {
                try
                {
                    return elem.get_Parameter(p).AsString();
                }
                catch
                {
                }
            }
            return "";
        }

        internal static Element getElemParam(Element elem, BuiltInParameter p)
        {
            if (elem != null)
            {
                try
                {
                    return elem.Document.GetElement(elem.get_Parameter(p).AsElementId());
                }
                catch
                {
                }
            }
            return null;
        }

        internal static bool equalNames(string first, string second)
        {
            string text = first.Replace(".rvt", "").Replace("(Read-only)", "").Trim();
            string value = second.Replace(".rvt", "").Replace("(Read-only)", "").Trim();
            return text.Equals(value);
        }

        internal static List<Document> getLinks(Document doc)
        {
            List<Document> links = new List<Document>();
            try
            {
                List<RevitLinkType> list2 = Enumerable.ToList<RevitLinkType>(Enumerable.Select<Element, RevitLinkType>((IEnumerable<Element>)new FilteredElementCollector(doc).OfCategory((BuiltInCategory)(-2001352)).OfClass(typeof(RevitLinkType)).ToElements(), (Func<Element, RevitLinkType>)((Element e) => e as RevitLinkType)));
                foreach (Document document in doc.Application.Documents)
                {
                    Document linkedDoc = document;
                    try
                    {
                        if (Enumerable.Count<RevitLinkType>((IEnumerable<RevitLinkType>)list2, (Func<RevitLinkType, bool>)((RevitLinkType e) => equalNames(((Element)e).Name, linkedDoc.Title))) > 0)
                        {
                            links.Add(linkedDoc);
                        }
                    }
                    catch
                    {
                    }
                }
                return links;
            }
            catch
            {
                return links;
            }
        }

        internal static void explain(object obj)
        {
            try
            {
                if (obj == null)
                {
                    return;
                }
                string name = obj.GetType().Name;
                string text = "";
                PropertyInfo[] properties = obj.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public);
                PropertyInfo[] properties2 = obj.GetType().GetProperties(BindingFlags.Instance | BindingFlags.NonPublic);
                int num = 3;
                try
                {
                    num = Math.Max(num, Enumerable.Max(Enumerable.Select<PropertyInfo, int>((IEnumerable<PropertyInfo>)properties, (Func<PropertyInfo, int>)((PropertyInfo p) => p.Name.Length))) + 3);
                }
                catch
                {
                }
                try
                {
                    num = Math.Max(num, Enumerable.Max(Enumerable.Select<PropertyInfo, int>((IEnumerable<PropertyInfo>)properties2, (Func<PropertyInfo, int>)((PropertyInfo p) => p.Name.Length))) + 3);
                }
                catch
                {
                }
                PropertyInfo[] array = properties;
                foreach (PropertyInfo propertyInfo in array)
                {
                    string text2 = new string(' ', 2 * (num - propertyInfo.Name.Length));
                    text = text + "\n" + propertyInfo.Name + text2 + "\t";
                    object value = propertyInfo.GetValue(obj);
                    if (value == null)
                    {
                        text += "null";
                        continue;
                    }
                    string text3 = value.ToString();
                    text = ((text3.Length > 40) ? (text + "\n" + text3 + "\n") : (text + text3));
                }
                PropertyInfo[] array2 = properties2;
                foreach (PropertyInfo propertyInfo2 in array2)
                {
                    string text4 = new string(' ', 2 * (num - propertyInfo2.Name.Length));
                    text = text + "\n" + propertyInfo2.Name + text4 + "\t";
                    object value2 = propertyInfo2.GetValue(obj);
                    if (value2 == null)
                    {
                        text += "null";
                        continue;
                    }
                    string text5 = value2.ToString();
                    text = ((text5.Length > 40) ? (text + "\n" + text5 + "\n") : (text + text5));
                }
                MessageBox.Show(text, name);
            }
            catch
            {
            }
        }

        internal static string hash(string content)
        {
            KeyedHashAlgorithm val = KeyedHashAlgorithm.Create();
            string s = "AST air";
            byte[] bytes = Encoding.ASCII.GetBytes(s);
            byte[] bytes2 = Encoding.ASCII.GetBytes(content);
            byte[] bytes3 = Encoding.ASCII.GetBytes(content.ToLower());
            HMACSHA512 val2 = new HMACSHA512(bytes);
            byte[] bytes4 = ((HashAlgorithm)val2).ComputeHash(bytes2);
            byte[] bytes5 = ((HashAlgorithm)val2).ComputeHash(bytes3);
            string @string = Encoding.ASCII.GetString(bytes4);
            string string2 = Encoding.ASCII.GetString(bytes5);
            return $"{@string.GetHashCode():X}";
        }

        internal static string mailTo(string subject)
        {
            return "mailto:" + Constants.emailSupport + "?subject=" + subject;
        }

        internal static BitmapImage bitmapToImageSource(Bitmap bitmap)
        {
            //IL_0021: Unknown result type (might be due to invalid IL or missing references)
            //IL_0027: Expected O, but got Unknown
            try
            {
                if (bitmap == null)
                {
                    return null;
                }
                using MemoryStream memoryStream = new MemoryStream();
                ((Image)bitmap).Save((Stream)memoryStream, ImageFormat.Bmp);
                memoryStream.Position = 0L;
                BitmapImage val = new BitmapImage();
                val.BeginInit();
                val.StreamSource = ((Stream)memoryStream);
                val.CacheOption = ((BitmapCacheOption)1);
                val.EndInit();
                return val;
            }
            catch
            {
                return null;
            }
        }




        ///Carbo methods defination
        ///Implementing CarboRevitUtils

        public static CarboElement getNewCarboElement(Document doc, Element el, ElementId materialIds, RevitImportSettings settings)
        {

            CarboElement newCarboElement = new CarboElement();
            try
            {
                if(settings == null)
                {
                    settings = new RevitImportSettings();
                }
                int setId;
                string setName;
                string setImportedMaterialName;
                string setCategory;
                string setSubCategory;
                double setVolume;
                double setLevel;
                bool setIsDemolished;
                bool setIsSubstructure;
                bool setIsExisting;
                //int layernr;

                // Material material = doc.GetElement(materialIds) as Material;
                //Id:
                setId = el.Id.IntegerValue;

                //Name (Type)
                ElementId elId = el.GetTypeId();
                ElementType type = doc.GetElement(elId) as ElementType;
                setName = type.Name;

                //MaterialName
                setImportedMaterialName = doc.GetElement(materialIds).Name.ToString();

                //CarboMaterial carboMaterial = new CarboMaterial(setMaterialName);

                //GetDensity
                Parameter paramMaterial = el.get_Parameter(BuiltInParameter.STRUCTURAL_MATERIAL_PARAM);
                if (paramMaterial != null)
                {
                    Material material = doc.GetElement(paramMaterial.AsElementId()) as Material;
                    if (material != null)
                    {
                        PropertySetElement property = doc.GetElement(material.StructuralAssetId) as PropertySetElement;
                        if (property != null)
                        {
                            Parameter paramDensity = property.get_Parameter(BuiltInParameter.PHY_MATERIAL_PARAM_STRUCTURAL_DENSITY);
                            if (paramDensity != null)
                            {
                                double density = paramDensity.AsDouble();
                                newCarboElement.Density = density;
                            }
                        }
                    }
                }


                //Category
                setCategory = getValueFromList(el, type, settings.MainCategory, doc);

                //SubCategory
                setSubCategory = getValueFromList(el, type, settings.SubCategory, doc);

                //Volume

                double volumeCubicFt = el.GetMaterialVolume(materialIds);
                setVolume = CarboLifeAPI.Utils.convertToCubicMtrs(volumeCubicFt);

                newCarboElement.isDemolished = false;

                Level lvl = doc.GetElement(el.LevelId) as Level;
                if (lvl != null)
                {
                    setLevel = Convert.ToDouble((lvl.Elevation) * 304.8);
                }
                else
                {
                    setLevel = 0;
                }

                if (setLevel <= settings.CutoffLevelValue)
                    setIsSubstructure = true;
                else
                    setIsSubstructure = false;

                //Get Phasing;
                Phase elCreatedPhase = doc.GetElement(el.CreatedPhaseId) as Phase;
                Phase elDemoPhase = doc.GetElement(el.DemolishedPhaseId) as Phase;


                if (elDemoPhase != null)
                {
                    setIsDemolished = true;
                }
                else
                {
                    setIsDemolished = false;
                }

                if (elCreatedPhase.Name == "Existing")
                {
                    setIsExisting = true;
                }
                else
                {
                    setIsExisting = false;
                }

                //Makepass;

                //Is existing and retained
                if (setIsExisting == true && setIsDemolished == false)
                {
                    if (settings.IncludeExisting == false)
                        return null;
                }

                //Is demolished
                if (setIsDemolished == true)
                {
                    if (settings.IncludeDemo == false)
                        return null;
                }

                //If it passed it is either proposed, or demolished and retained.

                newCarboElement.Id = setId;
                newCarboElement.Name = setName;
                newCarboElement.MaterialName = setImportedMaterialName;
                newCarboElement.Category = setCategory;
                newCarboElement.SubCategory = setSubCategory;
                newCarboElement.Volume = Math.Round(setVolume, 4);
                //newCarboElement.Material = carboMaterial; //Material removed
                newCarboElement.Level = Math.Round(setLevel, 3);
                newCarboElement.isDemolished = setIsDemolished;
                newCarboElement.isExisting = setIsExisting;
                newCarboElement.isSubstructure = setIsSubstructure;

                if (newCarboElement.Volume != 0)
                {
                    return newCarboElement;
                }
                else
                {
                    return null;
                }
            }
            catch
            {
                //TaskDialog.Show("Error", ex.Message);
                return null;
            }

        }
        private static string getValueFromList(Element el, ElementType type, string searchString, Document doc)
        {
            string result = "";

            if (searchString == "Type Comment")
            {
                Parameter commentpar = type.LookupParameter("Type Comments");
                if (commentpar != null)
                    result = commentpar.AsString();
            }
            else if (searchString == "Family Name")
            {
                result = type.FamilyName;
            }
            else if (searchString == "")
            {
                result = "";
            }
            else if (searchString == "CarboLifeCategory")
            {
                Parameter carbonpar = type.LookupParameter("CarboLifeCategory");
                if (carbonpar != null)
                    result = carbonpar.AsString();
            }
            else if (searchString == "Level")
            {
                Element lvlEl = doc.GetElement(el.LevelId);
                if (lvlEl != null)
                {
                    Level lvl = doc.GetElement(el.LevelId) as Level;
                    result = lvl.Name;
                }
                else
                {
                    result = "";
                }
            }
            else
            {
                result = el.Category.Name;
            }

            return result;

        }
        /// <summary>
        /// Validates the elements and it's class;
        /// </summary>
        /// <param name="el"></param>
        /// <returns></returns>
        public static bool isElementReal(Element el)
        {
            bool result = false;

            if (!(el is FamilySymbol || el is Family))
            {
                if (!(el.Category == null))
                {
                    if (el.get_Geometry(new Options()) != null)
                    {
                        //Check if not of any forbidden categories such as runs:
                        string Typename = el.Category.Name;
                        if (Typename != "Run")
                            result = true;
                    }
                }
            }

            return result;
        }
    }
}
