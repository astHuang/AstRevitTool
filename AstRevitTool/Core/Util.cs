using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using AstRevitTool.Core.Export;
using Microsoft.Office.Interop.Excel;

namespace AstRevitTool.Core
{
    static class Util
    {
        /// <summary>
        /// c# 的扩展方法
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        public static string Point(this XYZ p)
        {
            return string.Format("{0}", "{1}", "{2}", Math.Round(p.X, 2), Math.Round(p.Y, 2), Math.Round(p.Z, 2));
        }

        public static System.Windows.Media.Color FormColorFromRevit(Autodesk.Revit.DB.Color rvtColor)
        {
            byte r = rvtColor.Red;
            byte g = rvtColor.Green;
            byte b = rvtColor.Blue;
            return System.Windows.Media.Color.FromRgb(r, g, b);
        }

        public static void runPython(string appFolder, string name, string outDir, string project_name, string mode)
        {
            string folder = appFolder;
            System.Diagnostics.Process p = new System.Diagnostics.Process();
            p.StartInfo.FileName = "cmd.exe";
            p.StartInfo.UseShellExecute = false;
            p.StartInfo.RedirectStandardInput = true;
            p.StartInfo.RedirectStandardOutput = true;
            p.StartInfo.RedirectStandardError = true;
            p.StartInfo.CreateNoWindow = true;
            p.StartInfo.WorkingDirectory = folder;
            p.Start();


            string matrix = string.Format("app.exe -i {0} -o {1} -n{2} -m{3}", '"' + name + '"', '"' + outDir + '"', '"' + project_name + '"', mode);
            p.StandardInput.WriteLine(matrix);

            p.StandardInput.AutoFlush = true;
            p.StandardInput.WriteLine("exit");

            string output = p.StandardOutput.ReadToEnd();
            System.Windows.MessageBox.Show(output);
            

        }
        public static Autodesk.Revit.DB.Color RevitColorFromForm(System.Windows.Media.Color color)
        {
            return new Autodesk.Revit.DB.Color(color.R, color.G, color.B);
        }
        public static byte[] HexStringToColor(string hexColor)
        {
            var color = (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#" + hexColor);
            byte r = color.R;
            byte g = color.G;
            byte b = color.B;
            return new byte[] { r, g, b };
        }
        public static Plane getAppropriatePlane(View view)
        {
            Plane plane = Plane.CreateByNormalAndOrigin(view.ViewDirection, view.Origin);
            return plane;
        }

        public static void DetermineAdjacentElementLengthsAndWallAreas(Room room, out SvgExport.roomJson rjson)
        {
            SpatialElementBoundaryOptions option = new SpatialElementBoundaryOptions();
            IList<IList<BoundarySegment>> boundaries= room.GetBoundarySegments(option);
            List<SvgExport.wallJson> allwalls = new List<SvgExport.wallJson>();
            int n = boundaries.Count;

            Document doc = room.Document;

            int iBoundary = 0, iSegment;
            double adjustedArea = room.Area;
            foreach (var b in boundaries)
            {
                ++iBoundary;
                iSegment = 0;
                foreach (BoundarySegment s in b)
                {
                    ++iSegment;
                    ElementId neighbourid = s.ElementId;
                    Element neighbour = doc.GetElement(neighbourid);
                    if(neighbour is RevitLinkInstance)
                    {
                        neighbourid = s.LinkElementId;
                        RevitLinkInstance link = neighbour as RevitLinkInstance;
                        neighbour = link.GetLinkDocument().GetElement(neighbourid);
                    }
                    Curve curve = s.GetCurve();
                    double length = curve.Length;

                    Debug.Print(
                      "  Neighbour {0}:{1} {2} has {3}"
                      + " feet adjacent to room.",
                      iBoundary, iSegment,
                      Util.ElementDescription(neighbour),
                      Util.RealString(length));

                    if (neighbour is Wall)
                    {
                        Wall wall = neighbour as Wall;
                        SvgExport.wallJson wjson = new SvgExport.wallJson(wall,length);
                        if(wall.Name.Contains("EWA") || wall.Name.Contains("EXT"))
                        {
                            if (wall.WallType.GetCompoundStructure() == null)
                            {
                                continue;
                            }
                            SvgExport.extWallJson ewj;
                            AnalyzeExteriorWall(wall, length, out ewj);
                            allwalls.Add(ewj);
                            adjustedArea += ewj.wallWidth * ewj.effectiveWidth;
                        }
                        else if(wall.Name.Contains("Corridor"))
                        {
                            allwalls.Add(wjson);
                            adjustedArea += wjson.wallWidth * wjson.wallLength;
                        }
                        else if (wall.Name.Contains("Demising"))
                        {
                            allwalls.Add(wjson);
                            adjustedArea += 0.5 * wjson.wallWidth * wjson.wallLength;
                        }
                        //TODO: Save all neighbour wall names as a Json file and print them out
                        Autodesk.Revit.DB.Parameter p = wall.get_Parameter(
                          BuiltInParameter.HOST_AREA_COMPUTED);

                        double area = p.AsDouble();

                        /*LocationCurve lc
                          //= wall.Location as LocationCurve;

                        //double wallLength = lc.Curve.Length;

                        Debug.Print(
                          "    This wall has a total length"
                          + " and area of {0} feet and {1}"
                          + " square feet.",
                          Util.RealString(wallLength),
                          Util.RealString(area));*/
                    }
                }
            }
            rjson = new SvgExport.roomJson(room, allwalls);
            rjson.ajustedRoomArea = adjustedArea;
        }

        public static void AnalyzeExteriorWall(Wall wall, double adjLength, out SvgExport.extWallJson ewj)
        {
            int i, n;
            double halfThickness, layerOffset;
            XYZ lcstart, lcend, v, w, p, q;

            LocationCurve curve
      = wall.Location as LocationCurve;

            lcstart = curve.Curve.GetEndPoint(0);
            lcend = curve.Curve.GetEndPoint(1);
            halfThickness = 0.5 * wall.WallType.Width;
            v = lcend - lcstart;
            v = v.Normalize(); // one foot long
            w = XYZ.BasisZ.CrossProduct(v).Normalize();
            if (wall.Flipped) { w = -w; }

            CompoundStructure structure = wall.WallType.GetCompoundStructure();
            halfThickness = 0.5 * wall.WallType.Width;
            var layers = structure.GetLayers();
            n = layers.Count;
            i = 0;
            layerOffset = halfThickness;
            double[] thickness= new double[n];
            string[] materials = new string[n];
            MaterialFunctionAssignment[] functions = new MaterialFunctionAssignment[n];
            double effectiveWidth = 0.0;
            foreach(var layer in layers)
            {
                MaterialFunctionAssignment function = layer.Function;
                double width = layer.Width;
                string material = wall.Document.GetElement(layer.MaterialId)?.Name;
                if(material == null) { material = "Empty"; }
                //FLIPPED LAYER SEQUENCE: FROM INTERIOR TO EXTERIOR
                if (!wall.Flipped)
                {
                    thickness[n-i-1]= width;
                    materials[n - i - 1] = material;
                    functions[n - i - 1] = function;
                }
                else
                {
                    thickness[i] = width;
                    materials[i] = material;
                    functions[i] = function;
                }
                i++;
                
            }
            int k = 0;
            while (k < n && functions[k]!= MaterialFunctionAssignment.Substrate)
            {
                effectiveWidth += thickness[k];
                k++;
            }
            ewj = new SvgExport.extWallJson(wall,adjLength,thickness, materials, functions,effectiveWidth);
        }

        public static void addTo(IList<XYZ> to, IList<XYZ> from)
        {
            int cnt = from.Count;
            for (int ii = 0; ii < cnt; ii++)
            {
                if (ii < cnt - 1)
                {
                    XYZ p0 = from[ii];
                    XYZ p1 = from[ii + 1];
                    to.Add(p0);
                    to.Add(p1);
                }
            }
        }

        public static bool checkLocalPath(string path)
        {
            return false; 
        }

        public static int[] GetVec3MinMax(List<int> vec3)
        {
            int minVertexX = int.MaxValue;
            int minVertexY = int.MaxValue;
            int minVertexZ = int.MaxValue;
            int maxVertexX = int.MinValue;
            int maxVertexY = int.MinValue;
            int maxVertexZ = int.MinValue;
            for (int i = 0; i < vec3.Count; i += 3)
            {
                if (vec3[i] < minVertexX) minVertexX = vec3[i];
                if (vec3[i] > maxVertexX) maxVertexX = vec3[i];

                if (vec3[i + 1] < minVertexY) minVertexY = vec3[i + 1];
                if (vec3[i + 1] > maxVertexY) maxVertexY = vec3[i + 1];

                if (vec3[i + 2] < minVertexZ) minVertexZ = vec3[i + 2];
                if (vec3[i + 2] > maxVertexZ) maxVertexZ = vec3[i + 2];
            }
            return new int[] { minVertexX, maxVertexX, minVertexY, maxVertexY, minVertexZ, maxVertexZ };
        }

        public static long[] GetVec3MinMax(List<long> vec3)
        {
            long minVertexX = long.MaxValue;
            long minVertexY = long.MaxValue;
            long minVertexZ = long.MaxValue;
            long maxVertexX = long.MinValue;
            long maxVertexY = long.MinValue;
            long maxVertexZ = long.MinValue;
            for (int i = 0; i < (vec3.Count / 3); i += 3)
            {
                if (vec3[i] < minVertexX) minVertexX = vec3[i];
                if (vec3[i] > maxVertexX) maxVertexX = vec3[i];

                if (vec3[i + 1] < minVertexY) minVertexY = vec3[i + 1];
                if (vec3[i + 1] > maxVertexY) maxVertexY = vec3[i + 1];

                if (vec3[i + 2] < minVertexZ) minVertexZ = vec3[i + 2];
                if (vec3[i + 2] > maxVertexZ) maxVertexZ = vec3[i + 2];
            }
            return new long[] { minVertexX, maxVertexX, minVertexY, maxVertexY, minVertexZ, maxVertexZ };
        }

        public static float[] GetVec3MinMax(List<float> vec3)
        {

            List<float> xValues = new List<float>();
            List<float> yValues = new List<float>();
            List<float> zValues = new List<float>();
            for (int i = 0; i < vec3.Count; i++)
            {
                if ((i % 3) == 0) xValues.Add(vec3[i]);
                if ((i % 3) == 1) yValues.Add(vec3[i]);
                if ((i % 3) == 2) zValues.Add(vec3[i]);
            }

            float maxX = xValues.Max();
            float minX = xValues.Min();
            float maxY = yValues.Max();
            float minY = yValues.Min();
            float maxZ = zValues.Max();
            float minZ = zValues.Min();

            return new float[] { minX, maxX, minY, maxY, minZ, maxZ };
        }

        public static int[] GetScalarMinMax(List<int> scalars)
        {
            int minFaceIndex = int.MaxValue;
            int maxFaceIndex = int.MinValue;
            for (int i = 0; i < scalars.Count; i++)
            {
                int currentMin = Math.Min(minFaceIndex, scalars[i]);
                if (currentMin < minFaceIndex) minFaceIndex = currentMin;

                int currentMax = Math.Max(maxFaceIndex, scalars[i]);
                if (currentMax > maxFaceIndex) maxFaceIndex = currentMax;
            }
            return new int[] { minFaceIndex, maxFaceIndex };
        }

        public static float[] GetUVMinMax(List<float> vec3)
        {

            List<float> xValues = new List<float>();
            List<float> yValues = new List<float>();
            for (int i = 0; i < vec3.Count; i++)
            {
                if ((i % 2) == 0) xValues.Add(vec3[i]);
                if ((i % 2) == 1) yValues.Add(vec3[i]);
            }
            float maxX = xValues.Max();
            float minX = xValues.Min();
            float maxY = yValues.Max();
            float minY = yValues.Min();
            return new float[] { minX, maxX, minY, maxY };
        }

        /// <summary>
        /// From Jeremy Tammik's RvtVa3c exporter:
        /// https://github.com/va3c/RvtVa3c
        /// Return a string for a real number
        /// formatted to two decimal places.
        /// </summary>
        public static string RealString(double a)
        {
            return a.ToString("0.##");
        }

        /// <summary>
        /// From Jeremy Tammik's RvtVa3c exporter:
        /// https://github.com/va3c/RvtVa3c
        /// Return a string for an XYZ point
        /// or vector with its coordinates
        /// formatted to two decimal places.
        /// </summary>
        public static string PointString(XYZ p)
        {
            return string.Format("({0},{1},{2})",
              RealString(p.X),
              RealString(p.Y),
              RealString(p.Z));
        }

        /// <summary>
        /// From Jeremy Tammik's RvtVa3c exporter:
        /// https://github.com/va3c/RvtVa3c
        /// Return an integer value for a Revit Color.
        /// </summary>
        public static int ColorToInt(Color color)
        {
            return ((int)color.Red) << 16
              | ((int)color.Green) << 8
              | (int)color.Blue;
        }

        /// <summary>
        /// From Jeremy Tammik's RvtVa3c exporter:
        /// https://github.com/va3c/RvtVa3c
        /// Extract a true or false value from the given
        /// string, accepting yes/no, Y/N, true/false, T/F
        /// and 1/0. We are extremely tolerant, i.e., any
        /// value starting with one of the characters y, n,
        /// t or f is also accepted. Return false if no 
        /// valid Boolean value can be extracted.
        /// </summary>
        public static bool GetTrueOrFalse(string s, out bool val)
        {
            val = false;

            if (s.Equals(Boolean.TrueString,
              StringComparison.OrdinalIgnoreCase))
            {
                val = true;
                return true;
            }
            if (s.Equals(Boolean.FalseString,
              StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
            if (s.Equals("1"))
            {
                val = true;
                return true;
            }
            if (s.Equals("0"))
            {
                return true;
            }
            s = s.ToLower();

            if ('t' == s[0] || 'y' == s[0])
            {
                val = true;
                return true;
            }
            if ('f' == s[0] || 'n' == s[0])
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// From Jeremy Tammik's RvtVa3c exporter:
        /// https://github.com/va3c/RvtVa3c
        /// Return a string describing the given element:
        /// .NET type name,
        /// category name,
        /// family and symbol name for a family instance,
        /// element id and element name.
        /// </summary>
        public static string ElementDescription(Element e)
        {
            if (null == e)
            {
                return "<null>";
            }

            // For a wall, the element name equals the
            // wall type name, which is equivalent to the
            // family name ...

            FamilyInstance fi = e as FamilyInstance;

            string typeName = e.GetType().Name;

            string categoryName = (null == e.Category)
              ? string.Empty
              : e.Category.Name + " ";

            string familyName = (null == fi)
              ? string.Empty
              : fi.Symbol.Family.Name + " ";

            string symbolName = (null == fi
              || e.Name.Equals(fi.Symbol.Name))
                ? string.Empty
                : fi.Symbol.Name + " ";

            return string.Format("{0} {1}{2}{3}<{4} {5}>",
              typeName, categoryName, familyName,
              symbolName, e.Id.IntegerValue, e.Name);
        }

        /// <summary>
        /// From Jeremy Tammik's RvtVa3c exporter:
        /// https://github.com/va3c/RvtVa3c
        /// Return a dictionary of all the given 
        /// element parameter names and values.
        /// </summary>
        public static Dictionary<string, string> GetElementProperties(Element e, bool includeType)
        {
            IList<Autodesk.Revit.DB.Parameter> parameters
              = e.GetOrderedParameters();

            Dictionary<string, string> a = new Dictionary<string, string>(parameters.Count);

            // Add element category
            a.Add("Element Category", e.Category.Name);

            string key;
            string val;

            foreach (Autodesk.Revit.DB.Parameter p in parameters)
            {
                key = p.Definition.Name;

                if (!a.ContainsKey(key))
                {
                    if (StorageType.String == p.StorageType)
                    {
                        val = p.AsString();
                    }
                    else
                    {
                        val = p.AsValueString();
                    }
                    if (!string.IsNullOrEmpty(val))
                    {
                        a.Add(key, val);
                    }
                }
            }

            if (includeType)
            {
                ElementId idType = e.GetTypeId();

                if (ElementId.InvalidElementId != idType)
                {
                    Document doc = e.Document;
                    Element typ = doc.GetElement(idType);
                    parameters = typ.GetOrderedParameters();
                    foreach (Autodesk.Revit.DB.Parameter p in parameters)
                    {
                        key = "Type " + p.Definition.Name;

                        if (!a.ContainsKey(key))
                        {
                            if (StorageType.String == p.StorageType)
                            {
                                val = p.AsString();
                            }
                            else
                            {
                                val = p.AsValueString();
                            }
                            if (!string.IsNullOrEmpty(val))
                            {
                                a.Add(key, val);
                            }
                        }
                    }
                }
            }
            return a;
        }
    }
}
