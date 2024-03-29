﻿using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using Autodesk.Revit.DB.Architecture;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows.Forms;
using System.IO;
using System.Linq;
using System.Text;
using AstRevitTool.Core.Analysis;
using BoundarySegment = Autodesk.Revit.DB.BoundarySegment;
using Newtonsoft.Json;
using AstRevitTool;
using Autodesk.Revit.DB.Electrical;
using AstRevitTool.Views;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static AstRevitTool.Core.Export.SvgExport;

namespace AstRevitTool.Core.Export
{
    public class SvgExport
    {
        private const int _target_height_size = 600;
        private const int _target_width_size = 900;
        private const string formatSpecification = "0=+9.3f";
        private const double curveScale = 1;
        private const double svgScale = 0.95;
        private XYZ PMID = 100 * XYZ.BasisX + 100 * XYZ.BasisY + 100 * XYZ.BasisZ;
        public const string boma = AstRevitTool.Constants.BOMA;
        static BoundingBoxXYZ MergeBoundingBoxXyz(
  BoundingBoxXYZ boundingBoxXyz0,
  BoundingBoxXYZ boundingBoxXyz1)
        {
            BoundingBoxXYZ mergedResult = new BoundingBoxXYZ();

            /*
            mergedResult.Min = new XYZ(
              Math.Min(boundingBoxXyz0.Min.X, boundingBoxXyz1.Min.X),
              Math.Min(boundingBoxXyz0.Min.Y, boundingBoxXyz1.Min.Y),
              0.0);

            mergedResult.Max = new XYZ(
              Math.Max(boundingBoxXyz0.Max.X, boundingBoxXyz1.Max.X),
              Math.Max(boundingBoxXyz0.Max.Y, boundingBoxXyz1.Max.Y),
              0.0);*/

            return mergedResult;
        }

        private static BoundingBoxXYZ MergeBoundingBoxXyz(List<Element> areas, ViewPlan vp)
        {
            FBXExportOptions options = new FBXExportOptions();
            
            if (areas.Count == 0) return null;
            else if(areas.Count == 1) return areas[0].get_BoundingBox(vp);
            BoundingBoxXYZ bb = new BoundingBoxXYZ();
            foreach(Element area in areas)
            {
                BoundingBoxXYZ bb1 = area.get_BoundingBox(vp);
                bb = MergeBoundingBoxXyz(bb, area.get_BoundingBox(vp));
            }
            return bb;
        }

        private static string styleFromClassification(string classification)
        {
            string fillColor = "#FDF2FF";//Default fill color is white
            string strokeColor = "#000000";
            switch (classification)
            {
                case "TENANT AREA":
                    fillColor = "lavender";
                    break;
                case "BUILDING AMENITY AREA":
                    fillColor = "#DDE5C0";
                    break;
                case "BUILDING SERVICE AREA":
                    fillColor = "#D9D9D9";
                    break;
                case "TENANT ANCILLARY AREA":
                    fillColor = "#F1DAE4";
                    break;
                case "FLOOR SERVICE AREA":
                    fillColor = "#DDE6B8";
                    break;
                case "RETAIL":
                    fillColor = "#E5CFFF";
                    break;
                case "MAJOR VERTICAL PENETRATION":
                    fillColor = "#FF7F7E";
                    break;  
                default:
                    break;
            }
            string style = string.Format("fill:{0};stroke:{1};stroke-width:1", fillColor, strokeColor);
            return style;
        }

        public class objectJson
        {
            public objectJson() { }
            public string toJsonString()
            {
                return JsonConvert.SerializeObject(this, Formatting.Indented);
            }
        }

        public class unitMatrixJson : objectJson
        {
            public Newtonsoft.Json.Linq.JRaw[] rooms;
            public unitMatrixJson() { }
            public unitMatrixJson(IEnumerable<roomJson> rooms)
            {
                this.rooms = rooms.Select(x => new Newtonsoft.Json.Linq.JRaw(x.toJsonString())).ToArray();
            }
        }

        public class roomJson: objectJson
        {
            public string roomName;
            public string roomId;
            public double roomArea;
            public double ajustedRoomArea { get; set; }
            public Newtonsoft.Json.Linq.JRaw[] walls;
            public roomJson() { }
            public roomJson(Room room, IEnumerable<wallJson> walls)
            {
                this.roomName = room.Name;
                this.roomId = room.Id.ToString();
                this.roomArea = room.Area;
                this.walls = walls.Select(x => new Newtonsoft.Json.Linq.JRaw(x.toJsonString())).ToArray();
            }
        }

        public class wallJson: objectJson
        {
            public string wallDescription;
            public string wallId;
            public double wallLength;
            public double wallWidth;
            public wallJson()
            {

            }
            public wallJson(Wall wall,double adjacentLength)
            {
                this.wallDescription = Util.ElementDescription(wall);
                this.wallId = wall.Id.ToString();
                this.wallLength = adjacentLength;
                this.wallWidth = wall.Width;
            }
        }

        public class extWallJson : wallJson
        {
            public double[] widthList;
            public string[] materialList;
            public MaterialFunctionAssignment[] functionList;
            public double effectiveWidth;
            public extWallJson(Wall wall, double adjLength, double[] widthList, string[] materialList, MaterialFunctionAssignment[] functionList, double effectiveWidth) : base(wall,adjLength)
            {
                this.widthList = widthList;
                this.materialList = materialList;
                this.functionList = functionList;
                this.effectiveWidth = effectiveWidth;
            }
        }
        public class detailJson : objectJson
        {
            public string title;
            public string pnumber;
            public string client;
            public geoJson location;
            public detailJson(Document doc)
            {
                this.title = doc.ProjectInformation.Name;
                this.pnumber = doc.ProjectInformation.Number;
                this.client = doc.ProjectInformation.ClientName;
                this.location = new geoJson(doc);
            }
        }
        public class metricJson : objectJson {
            public string grossSquareFootage;
            public string totalRentableArea;
            public string buildingAllocationRatio;
            public metricJson()
            {
                this.grossSquareFootage = "none";
                this.totalRentableArea = "none";
                this.buildingAllocationRatio = "none";
            }
        }

        public class versionJson: objectJson
        {
            public int projectVersion;
            public string modifiedDate;
            public versionJson(int version)
            {
                this.projectVersion = version;
                this.modifiedDate = DateTime.Now.ToString();
            }
        }

        public class geoJson:objectJson
        {
            public string neighborhood;
            public string street;
            public string city;
            public string state;
            public string zip;
            public geoJson(Document doc)
            {
                this.neighborhood = doc.ProjectInformation.BuildingName;
                this.street = doc.ProjectInformation.Address;
                this.city = "";
                this.state = "";
                this.zip = "00000";
            }
        }
        public class buildingJson: objectJson
        {
            public detailJson details;
            public metricJson totalMetrics;
            public versionJson versioning;

            public string title;
            public int totalArea;
            public string date;
            public Newtonsoft.Json.Linq.JRaw[] floors;
            public int totalLevel;

            public buildingJson(Document doc,IEnumerable<floorJson> floors)
            {
                this.details = new detailJson(doc);
                this.totalMetrics = new metricJson();
                this.versioning = new versionJson(2);

                this.title = doc.Title;
                this.totalArea = floors.Sum(x => x.Value[0].boundaryArea);
                this.date = DateTime.Today.ToString();
                this.totalLevel = floors.Count();
                this.floors = floors.Select(x => new Newtonsoft.Json.Linq.JRaw(x.toJsonString())).ToArray();
            }          
        }
        public class floorJson: objectJson
        {
            public string label = "";
            public List<valueItemJson> Value;
            public List<locationJson> locationJsons = new List<locationJson>();

            public floorJson(string label, string tenantType,IEnumerable<locationJson> locations, bool exportSvgData = false)
            {
                this.label = label;
                if (exportSvgData)
                {
                    this.locationJsons = locations.ToList();
                }
                //this.svgString = writeSvg(locations);
                valueItemJson value = new valueItemJson(tenantType, locations);
                Value = new List<valueItemJson>();
                Value.Add(value);
            }

            
           
        }

        public static void WriteSvg(floorJson fj, string filename)
        {
            StreamWriter sw = new StreamWriter(filename);
            //StringBuilder sb = new StringBuilder();
            string header = string.Format("<svg height=\"{0}\" width=\"{1}\" viewBox=\"0 0 {1} {0}\" fill=\"none\" xmlns=\"http://www.w3.org/2000/svg\">", _target_height_size, _target_width_size);
            sw.WriteLine(header);
            //sb.AppendLine(header);

            foreach (locationJson location in fj.locationJsons)
            {
                string d = location.path;
                string style = styleFromClassification(location.name.ToUpper());
                string areaGeo = string.Format("    <path d=\"{0}\"   style= \"{1}\" />", d, style);
                sw.WriteLine(areaGeo);
            }
            sw.WriteLine("</svg>");
            sw.Close();
            //this.svgString = sb.ToString();
            return;
        }

        public class valueItemJson : objectJson
        {
            public string TenantType = "";
            public string viewBox = "0 0 " + _target_width_size.ToString() + " " + _target_height_size.ToString();
            public int boundaryArea = 0;
            public Newtonsoft.Json.Linq.JRaw[] locations;
            private List<locationJson> _locations;
            public string viewName = "";
            public string[] floorPlanLines;

            public valueItemJson(string tenantType, IEnumerable<locationJson> locations)
            {
                this.TenantType = tenantType;
                this.boundaryArea = locations.Sum(x => x.area);
                this.locations = locations.Select(x => new Newtonsoft.Json.Linq.JRaw(x.toJsonString())).ToArray();
                this._locations = locations.ToList();
            }
        }

        public class locationJson: objectJson
        {
            public string name = "";
            public string id = "";
            public string path = "";
            public int area = 0;
            public string space_id = "";
            public string boma_exclusion = "False";
            public locationJson()
            {

            }
            public locationJson(string name,string id,string path,int area,string space_id,string exclusion)
            {
                this.name = name;
                this.id = id;  
                this.path = path;
                this.area = area;
                this.space_id = space_id;
                this.boma_exclusion= exclusion;
            }

        }
        private static readonly Dictionary<string, byte[]> keyValuePairs = new Dictionary<string, byte[]>()
        {
            {"BACK OF HOUSE",new byte[]{255,000,128} },
            {"BUILDING AMENITY AREA",new byte[]{255,239,225} },
            {"BUILDING SERVICE AREA",new byte[]{211,210,210} }
        };

        public class RoomSelectionFilter : ISelectionFilter
        {
            public bool AllowElement(Element e)
            {
                return e is Area;
            }

            public bool AllowReference(Reference r, XYZ p)
            {
                return true;
            }
        }

        public static List<ElementId> uniqueLevelIdsInSchedule(ViewSchedule vs, Document doc)
        {
            List<Element> levels = new List<Element>();
            List<ElementId> levelList = new List<ElementId>();
            List<Element> elems = new FilteredElementCollector(doc, vs.Id)
            .ToElements()
            .ToList();
            foreach(Element el in elems)
            {
                if(el is Area)
                {
                    Area area = (Area)el;
                    levelList.Add(area.Level.Id);
                }
            }
            List<ElementId> uniqueLevels = levelList.Distinct().ToList();
            var sortedLevels = uniqueLevels.OrderBy(uniqueLevels => ((Level)doc.GetElement(uniqueLevels)).Elevation).ToList();
            return sortedLevels;

        }
        
        public List<List<Curve>> roomsBounds(List<Area> areas)
        {
            List<List<Curve>> allcrvs = new List<List<Curve>>();
            SpatialElementBoundaryOptions option = new SpatialElementBoundaryOptions();
            option.SpatialElementBoundaryLocation = SpatialElementBoundaryLocation.Finish;
            foreach(Area area in areas)
            {
                List<Curve> crvs = new List<Curve>();
                foreach(var seg in area.GetBoundarySegments(option)[0])
                {
                    Curve crv = seg.GetCurve();
                    crvs.Add(crv);
                }
                allcrvs.Add(crvs);
            }
            return allcrvs;
        }
        /// Return an SVG representation of the
        /// given XYZ point scaled, offset and
        /// Y flipped to the target square size.
        /// </summary>
        public static string GetSvgPointFrom(
          XYZ p,
          XYZ pmid,
          double scale)
        {
            p -= pmid;
            p *= scale;
            int x = (int)(p.X + 0.5);
            int y = (int)(p.Y + 0.5);

            // The Revit Y coordinate points upwards,
            // the SVG one down, so flip the Y coord.

            y = -y;

            x += _target_width_size / 2;
            y += _target_height_size / 2;
            return x.ToString() + " " + y.ToString();
        }

        public static string GetSvgPoint2Decimal(XYZ p, XYZ pmid, double scale)
        {
            p -= pmid;
            p *= scale;
            double x = p.X;
            double y = -p.Y;
            x += _target_width_size / 2;
            y += _target_height_size / 2;
            return x.ToString("0.##") + " " + y.ToString("0.##");
        }

        public static string svgPathFromArea(Area area, BoundingBoxXYZ bb)
        {
            SpatialElementBoundaryOptions option = new SpatialElementBoundaryOptions();
            option.SpatialElementBoundaryLocation = SpatialElementBoundaryLocation.Finish;
            var loops = area.GetBoundarySegments(option);
            string path = "";
            foreach(var loop in loops)
            {
                path += GetSvgPathFrom(bb, loop);
            }
            return path;
        }

        public static locationJson locationFromArea(Area area, BoundingBoxXYZ bb)
        {
            string name = area.LookupParameter(boma).AsString();
            string exclusion = area.LookupParameter(Constants.EXCLUSION).AsInteger().ToString();
            if(name == null) MessageBox.Show("Area without BOMA Space Classification! Area ID: " + area.Id.ToString());
            string id = area.Id.ToString();
            string path = svgPathFromArea(area, bb);
            int _area = (int)Math.Round(area.Area);
            string spaceID = area.Name;
            return new locationJson(name, id, path,_area,spaceID,exclusion);
        }
        public static AreaScheme getAreaSchemeFromSchedule(ViewSchedule vs, Document doc)
        {
            var areascheme = vs.Definition.AreaSchemeId;
            if (areascheme == null || doc.GetElement(areascheme) is not AreaScheme)
            {
                MessageBox.Show("Current Schedule does not contain an area scheme!");
                return null;
            }
            return doc.GetElement(areascheme) as AreaScheme;
        }

        public static ViewPlan viewFromLevel(Level l)
        {
            ElementId viewId = l.FindAssociatedPlanViewId();
            if (viewId == null) return null;
            
            return l.Document.GetElement(viewId) as ViewPlan;
        }
            
        public static floorJson levelFromSchedule(ViewSchedule vs,ElementId levelId,Document doc, string rentingType, bool writeLines = true, bool writeSvg = false)
        {
            Level l = doc.GetElement(levelId) as Level;
            ElementLevelFilter filter = new ElementLevelFilter(levelId);
            ElementClassFilter filter2= new ElementClassFilter(typeof(SpatialElement));
            LogicalAndFilter lf = new LogicalAndFilter(filter,filter2);
            AreaScheme areaScheme = getAreaSchemeFromSchedule(vs, doc);
            IList<Element> allareaonLevel = areaScheme.GetDependentElements(lf).Select(x => doc.GetElement(x)).ToList();           
            var viewplans = new FilteredElementCollector(doc).OfClass(typeof(ViewPlan)).Where(x=>x.LevelId==levelId);
            //IList<Element> vps = viewplans.ToElements();
            ViewPlan vp = viewFromLevel(l);
            if(vp == null)
            {
                /*int cand = 0;
                int ind = 0;
                foreach(ViewPlan v in viewplans)
                {
                    if (v.Name.Contains("Parent"))
                    {
                        ind = cand;
                    }
                    cand++;
                }*/
                vp = viewplans.ToList()[0] as ViewPlan;
                if(vp == null)
                {
                    MessageBox.Show("Cannot retrieve floorplan from Level " + l.Name);
                    return null;
                }
            }
            
            //BoundingBoxXYZ bb = vp.get_BoundingBox(vp);
            BoundingBoxXYZ bb = vp.CropBox == null ? new BoundingBoxXYZ() : vp.CropBox; ;
            string label = l.Name;
            bool hasTenant = false;
            List<locationJson> locations = new List<locationJson>();
            foreach(Element el in allareaonLevel)
            {
                if(el is Area)
                {
                    Area area = (Area)el;
                    
                    if(area.Area > 0.0 && area.LookupParameter(boma).AsString() != null)
                    {
                        string areaCategory = area.LookupParameter(boma).AsString().ToUpper();
                        if((!hasTenant) && areaCategory.Contains("TENANT AREA"))
                        {
                            hasTenant = true;
                        }
                        locationJson location = locationFromArea(area, bb);
                        locations.Add(location);
                    }
                }
            }
            string tenantOption = hasTenant ? rentingType : "N/A";
            floorJson fj = new floorJson(label,tenantOption, locations,writeSvg);
            
            fj.Value[0].viewName = vp.Title;
            //string[] floorPlanLines;
            //string planName = vp.Name;
            if (writeLines)
            {
                IList<XYZ> linePts;
                LineDrawings2DExportContext context = new LineDrawings2DExportContext(out linePts);
                CustomExporter exporter = new CustomExporter(doc, context);
                exporter.Export2DIncludingAnnotationObjects = false;
                exporter.Export2DGeometricObjectsIncludingPatternLines = false;
                exporter.ShouldStopOnError = true;
                exporter.Export(vp);
                string[] floorPlanLines = GetSvgLinesFromPoints(linePts, vp, doc.Application.ShortCurveTolerance);
                fj.Value[0].floorPlanLines = floorPlanLines;
            }

            //fj.Value[0].floorPlanLines = floorPlanLines;
            return fj;

        }
        /// <summary>
        /// Generate single lines in svg format
        /// Input points are separated in group of two
        /// </summary>
        public static string[] GetSvgLinesFromPoints(IList<XYZ> pts, Autodesk.Revit.DB.View view,double tolerance)
        {
            //StringBuilder s = new StringBuilder();
            Plane plane = Util.getAppropriatePlane(view);
            BoundingBoxXYZ bb = view.CropBox == null? new BoundingBoxXYZ():view.CropBox;           
            XYZ pmin = bb.Min;
            XYZ pmax = bb.Max;
            XYZ vsize = pmax - pmin;
            XYZ pmid = pmin + 0.5 * vsize;
            List<string> lines = new List<string>();
            double scale = curveScale;
            if (plane != null)
            {
                for(int i=0; i<pts.Count; i++)
                {
                    StringBuilder s = new StringBuilder();
                    UV uvStart, uvEnd;
                    double distance = double.MaxValue;
                    plane.Project(pts[i], out uvStart, out distance);
                    plane.Project(pts[i+1], out uvEnd, out distance);

                    XYZ projectionStart;
                    XYZ projectionEnd;

                    projectionStart = uvStart.U * plane.XVec + uvStart.V * plane.YVec + plane.Origin;
                    projectionEnd = uvEnd.U * plane.XVec + uvEnd.V * plane.YVec + plane.Origin;

                    
                    if (projectionStart.DistanceTo(projectionEnd) < 0.5)
                    {
                        i++;
                        continue;
                    }

                    string svgstart = GetSvgPoint2Decimal(projectionStart,pmid,scale);
                    string svgend = GetSvgPoint2Decimal(projectionEnd,pmid,scale);

                    s.Append(svgstart);
                    s.Append(' ');
                    s.Append(svgend);
                    lines.Add(s.ToString());
                    i++;
                }
            }
            return lines.Distinct().ToArray();
        }
        
        /// <summary>
        /// Generate and return an SVG path definition to
        /// represent the given room boundary loop, scaled 
        /// from the given bounding box to fit into a 
        /// 100 x 100 canvas. Actually, the size is 
        /// determined by _target_square_size.
        /// </summary>
        public static string GetSvgPathFrom(
          BoundingBoxXYZ bb,
          IList<BoundarySegment> loop)
        {
            
            XYZ pmin = bb.Min;
            XYZ pmax = bb.Max;
            XYZ vsize = pmax - pmin;
            XYZ pmid = pmin + 0.5 * vsize;
            //XYZ pmid = new XYZ(0, 0, 0);

            double scale = curveScale;

            StringBuilder s = new StringBuilder();

            int nSegments = loop.Count;

            XYZ p0 = null; // loop start point
            XYZ p; // segment start point
            XYZ q = null; // segment end point

            foreach (BoundarySegment seg in loop)
            {
                Curve curve = seg.GetCurve();
                IList<XYZ> pts = curve.Tessellate();
              

                p = curve.GetEndPoint(0);
                q = curve.GetEndPoint(1);

                if (null == p0)
                {
                    p0 = p; // save loop start point

                    s.Append("M"
                      + GetSvgPointFrom(p, pmid, scale));
                }
                for(int i = 1; i < pts.Count; i++)
                {
                    s.Append("L"
                  + GetSvgPointFrom(pts[i], pmid, scale));
                }
                //s.Append("L"+ GetSvgPointFrom(q, pmid, scale));
            }
            s.Append("Z");
            return s.ToString();
        }

        /// <summary>
        /// Invoke the SVG node.js web server.
        /// Use a local or global base URL and append
        /// the SVG path definition as a query string.
        /// Compare this with the JavaScript version used in
        /// http://the3dwebcoder.typepad.com/blog/2015/04/displaying-2d-graphics-via-a-node-server.html
        /// </summary>
        public static void DisplaySvg(string path_data)
        {
            var local = false;

            var base_url = local
              ? "http://127.0.0.1:5000"
              : "https://shielded-hamlet-1585.herokuapp.com";

            var d = path_data.Replace(' ', '+');

            var query_string = "d=" + d;

            string url = base_url + '?' + query_string;

            System.Diagnostics.Process.Start(url);
        }
    }


}
