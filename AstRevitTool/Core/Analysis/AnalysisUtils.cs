using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB;
using System.Windows.Forms;
using I = Autodesk.Revit.DB.IFC;

namespace AstRevitTool.Core.Analysis
{
    public static class AnalysisUtils
    {
        //Return true if full opening; else false
        public static bool Categorize_Door(string FName, string TName)
        {
            string t_fg = "FG";
            string f_storefront = "Storefront";
            string f_allglass = "All-Glass";
            string glass = "Glass";
            List<string> listOfStrings = new List<string>() { "D", "F", "FL", "G2", "G", "L", "N" };
            if (TName.Contains(t_fg) | FName.Contains(f_allglass) | FName.Contains(f_storefront) | FName.Contains(glass))
            {
                return true;
            }
            else if (listOfStrings.Any(TName.Contains))
            {
                return false;
            }
            return true;
        }

        public static bool WallEligable(Element W_door)
        {
            //Document doc = W_door.Document;
            string name = W_door.Name;
            if(name.Contains("Curtain Wall Door Host")) { return false; }
            /*
            IEnumerable<Element> associate = new FilteredElementCollector(doc).OfClass(typeof(FamilyInstance)).Where(m => (m as FamilyInstance).Host.Id == W_door.Id);
            if (associate != null && associate.Count() >0)
            {
                foreach (Element element in associate)
                {
                    if(element.Name.IndexOf("Storefront", StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        try {
                            double w_area = W_door.LookupParameter("Area").AsDouble();
                            double d_area = element.LookupParameter("Area").AsDouble();
                            if (w_area < 1.01 * d_area)
                            {
                                return false;
                            }
                        }
                        catch { continue; }
                    }
                }
            
            }*/

            return true;
        }

        public static bool PanelEligable(Element panel)
        {
            string name = panel.Name;
            return !name.Contains("Curtain Wall Door Host");
        }

        //string key = "Double Sidelight", "Single Sidelight", or "Transom"

        public static Dictionary<string, string[]> _parameterMapping = new Dictionary<string, string[]>
        {
            {"Transom", new[] { "Transom Height", "Rough Width"} },
            {"Single Sidelight", new[] { "Sidelight Width", "Height"} },
            {"Double Sidelight", new[]{"Right Sidelight Width","Left Sidelight Width","Height" } }
        };

        public static string[] _keys = _parameterMapping.Keys.ToArray<string>();
        public static double lighting_area(string[] keys, Element door)
        {
            try
            {
                double t_area = 0.0;
                FamilyInstance fInstance = door as FamilyInstance;
                FamilySymbol FType = fInstance.Symbol;
                string FName = FType.FamilyName;
                Document doc = door.Document;
                foreach (string key in keys)
                {
                    if (FName.Contains(key))
                    {
                        string[] paraName = _parameterMapping[key];
                        var subset = paraName.Take(paraName.Length - 1);
                        double multiplier = doc.GetElement(door.GetTypeId()).LookupParameter(paraName.Last()).AsDouble();
                        foreach (string para in subset)
                        {
                            t_area += doc.GetElement(door.GetTypeId()).LookupParameter(para).AsDouble() * multiplier;
                            //+= "Rough Width" * "Rough Height" - "Width" * "Height"
                        }
                    }
                    else
                    {
                        t_area += 0.0;
                    }
                }
                return t_area;
            }
            catch
            {
                return 0.0;
            }
        }

        public static double GetInstanceSurfaceAreaMetric(
    FamilyInstance familyInstance)
        {
            double area_sq_ft = 0;

            Wall wall = familyInstance.Host as Wall;

            double width = familyInstance.Symbol.get_Parameter(BuiltInParameter.FAMILY_WIDTH_PARAM).AsDouble();

            double height = familyInstance.Symbol.get_Parameter(BuiltInParameter.FAMILY_HEIGHT_PARAM).AsDouble();

            if (null != wall)
            {
                if (wall.WallType.Kind == WallKind.Curtain)
                {
                    area_sq_ft = familyInstance.get_Parameter(
                      BuiltInParameter.HOST_AREA_COMPUTED)
                        .AsDouble();
                }
                else
                {
                    Document doc = familyInstance.Document;
                    XYZ basisY = XYZ.BasisY;
                    try {
                        CurveLoop curveLoop = I.ExporterIFCUtils
                      .GetInstanceCutoutFromWall(doc, wall,
                        familyInstance, out basisY);
                        IList<CurveLoop> loops = new List<CurveLoop>(1);
                        loops.Add(curveLoop);
                        area_sq_ft = I.ExporterIFCUtils
                              .ComputeAreaOfCurveLoops(loops);
                    }
                    catch
                    {
                        area_sq_ft = width * height;
                    }
                }
            }
            else
            {
                area_sq_ft = width * height;
            }
            return area_sq_ft;
        }

        public static List<string> GetInstanceMaterials(GeometryElement geo, Document doc)
        {
            List<string> materials = new List<string>();
            foreach(GeometryObject o in geo)
            {
                if(o is Solid)
                {
                    Solid solid = o as Solid;
                    foreach(Face face in solid.Faces)
                    {
                        string s = doc.GetElement(face.MaterialElementId).Name;
                        materials.Add(s);
                    }
                }
                else if(o is GeometryInstance)
                {
                    GeometryInstance i = o as GeometryInstance;
                    materials.AddRange(GetInstanceMaterials(i.SymbolGeometry, doc));
                }
            }
            return materials;
        }

        public static Dictionary<string,double> GetInstanceMaterialsAreas(GeometryElement geo, Document doc)
        {
            List<string> materials = new List<string>();
            Dictionary<string, double> instance_mat_areas = new Dictionary<string, double>();
            foreach (GeometryObject o in geo)
            {
                if (o is Solid)
                {
                    
                    Solid solid = o as Solid;
                    string mat_name;
                    Dictionary<string, double> this_dic = new Dictionary<string, double>();
                    foreach (Face face in solid.Faces)
                    {
                        if (null == doc.GetElement(face.MaterialElementId)) continue;
                        mat_name = doc.GetElement(face.MaterialElementId).Name;
                        if (this_dic.Keys.Contains(mat_name)){
                            if (face.Area > this_dic[mat_name])
                            {
                                this_dic[mat_name] = face.Area;
                            }
                            else continue;
                        }
                        else
                        {
                            this_dic.Add(mat_name, face.Area);
                        }
                    }
                    foreach(KeyValuePair<string,double> entry in this_dic)
                    {
                        if (instance_mat_areas.Keys.Contains(entry.Key))
                        {
                            instance_mat_areas[entry.Key] += entry.Value;
                        }
                        else
                        {
                            instance_mat_areas.Add(entry.Key, entry.Value);
                        }
                    }
                }
                else if (o is GeometryInstance)
                {
                    GeometryInstance i = o as GeometryInstance;
                    materials.AddRange(GetInstanceMaterials(i.SymbolGeometry, doc));
                    Dictionary<string, double> solid_dic = GetInstanceMaterialsAreas(i.SymbolGeometry, doc);
                    foreach (KeyValuePair<string, double> entry in solid_dic)
                    {
                        if (instance_mat_areas.Keys.Contains(entry.Key))
                        {
                            instance_mat_areas[entry.Key] += entry.Value;
                        }
                        else
                        {
                            instance_mat_areas.Add(entry.Key, entry.Value);
                        }
                    }
                }
            }
            return instance_mat_areas;
        }

        private static double geoarea(GeometryElement geo)
        {
            double total= 0.0;
            foreach (GeometryObject o in geo)
            {
                if (o is Solid)
                {
                    double area = 0.0;
                    Solid solid = o as Solid;
                    foreach (Face face in solid.Faces)
                    {
                        if (face.Area > area)
                        {
                            area = face.Area;
                        };
                    }
                    total += area;
                    
                }
                else if (o is GeometryInstance)
                {
                    GeometryInstance i = o as GeometryInstance;
                    total += geoarea(i.GetInstanceGeometry(i.Transform));
                }
            }
            return total;
        }
        public static double ElementArea(Element el)
        {
            double area = 0.0;
            if (el.LookupParameter("Area")!=null)
            {
                area = el.LookupParameter("Area").AsDouble();
            }
            else
            {
                Options option = new Options();
                GeometryElement geo = el.get_Geometry(option);
                area = geoarea(geo);

            }
            return area;
        }
    }
}
