using Autodesk.Revit.ApplicationServices;
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
        public class buildingJson
        {
            public string title;
            public int totalArea;
            public Newtonsoft.Json.Linq.JRaw[] levels;
            public int totalLevel;

            public buildingJson(string title,IEnumerable<floorJson> floors)
            {
                this.title = title;
                this.totalArea = floors.Sum(x => x.boundaryArea);
                this.totalLevel = floors.Count();
                this.levels = floors.Select(x => new Newtonsoft.Json.Linq.JRaw(x.toJsonString())).ToArray();
            }

            public string toJsonString()
            {
                return JsonConvert.SerializeObject(this, Formatting.Indented);
            }
        }
        public class floorJson
        {
            public string label = "";
            public string viewBox = "0 0 " + _target_width_size.ToString() + " " + _target_height_size.ToString();
            public int boundaryArea = 0;
            public Newtonsoft.Json.Linq.JRaw[] locations;
            private List<locationJson> _locations;
            public string viewName = "";
            public string[] floorPlanLines;

            public floorJson(string label, IEnumerable<locationJson> locations)
            {
                this.label = label;
                this.boundaryArea = locations.Sum(x => x.area);
                this.locations = locations.Select(x => new Newtonsoft.Json.Linq.JRaw(x.toJsonString())).ToArray();
                this._locations = locations.ToList();
            }

            public string toJsonString()
            {
                return JsonConvert.SerializeObject(this,Formatting.Indented);
            }
        }

        public class locationJson
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


            public string toJsonString()
            {
                return JsonConvert.SerializeObject(this,Formatting.Indented);
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
            return levelList.Distinct().ToList();

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
            int _area = ((int)area.Area);
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
            
        public static floorJson levelFromSchedule(ViewSchedule vs,ElementId levelId,Document doc)
        {
            Level l = doc.GetElement(levelId) as Level;
            ElementLevelFilter filter = new ElementLevelFilter(levelId);
            ElementClassFilter filter2= new ElementClassFilter(typeof(SpatialElement));
            LogicalAndFilter lf = new LogicalAndFilter(filter,filter2);
            AreaScheme areaScheme = getAreaSchemeFromSchedule(vs, doc);
            IList<Element> allareaonLevel = areaScheme.GetDependentElements(lf).Select(x => doc.GetElement(x)).ToList();           
            FilteredElementCollector viewplans = new FilteredElementCollector(doc).OfClass(typeof(ViewPlan));
            IList<Element> vps = viewplans.ToElements();
            ViewPlan vp = viewFromLevel(l);
            if(vp == null)
            {
                vp = viewplans.ToList()[0] as ViewPlan;
                if(vp == null)
                {
                    MessageBox.Show("Cannot retrieve floorplan from Level " + l.Name);
                    return null;
                }
            }
            IList<XYZ> linePts;
            LineDrawings2DExportContext context = new LineDrawings2DExportContext(out linePts);
            CustomExporter exporter = new CustomExporter(doc, context);
            exporter.Export2DIncludingAnnotationObjects = false;
            exporter.Export2DGeometricObjectsIncludingPatternLines = false;
            exporter.ShouldStopOnError = true;
            exporter.Export(vp);
            string[] floorPlanLines = GetSvgLinesFromPoints(linePts, vp,doc.Application.ShortCurveTolerance);
            //BoundingBoxXYZ bb = vp.get_BoundingBox(vp);
            BoundingBoxXYZ bb = vp.CropBox == null ? new BoundingBoxXYZ() : vp.CropBox; ;
            string label = l.Name;
            List<locationJson> locations = new List<locationJson>();
            foreach(Element el in allareaonLevel)
            {
                if(el is Area)
                {
                    Area area = (Area)el;
                    if(area.Area > 0.0 && area.LookupParameter(boma).AsString() != null)
                    {
                        locationJson location = locationFromArea(area, bb);
                        locations.Add(location);
                    }
                }
            }
            floorJson fj = new floorJson(label, locations);
            fj.viewName = vp.Name;
            fj.floorPlanLines = floorPlanLines;
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

                // Todo: handle non-linear curve.
                // Especially: if two long lines have a 
                // short arc in between them, skip the arc
                // and extend both lines.

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
