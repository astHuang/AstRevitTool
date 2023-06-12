using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using Autodesk.Revit.DB.Architecture;
using Quadrant = System.Int32;

namespace AstRevitTool.Core
{
    public class UVArray
    {
        List<UV> arrayPoints;
        public UVArray(List<XYZ> XYZArray)
        {
            arrayPoints = new List<UV>();
            foreach (var p in XYZArray)
            {
                arrayPoints.Add(p.TOUV());
            }
        }

        public UV get_Item(int i)
        {
            return arrayPoints[i];
        }

        public int Size
        {
            get
            {
                return arrayPoints.Count;
            }
        }
    }

    public class PointInPoly
    {

        /// <summary>
        /// Determine the quadrant of a polygon vertex 
        /// relative to the test point.
        /// </summary>
        Quadrant GetQuadrant(UV vertex, UV p)
        {
            return (vertex.U > p.U)
              ? ((vertex.V > p.V) ? 0 : 3)
              : ((vertex.V > p.V) ? 1 : 2);
        }

        /// <summary>
        /// Determine the X intercept of a polygon edge 
        /// with a horizontal line at the Y value of the 
        /// test point.
        /// </summary>
        double X_intercept(UV p, UV q, double y)
        {
            Debug.Assert(0 != (p.V - q.V),
              "unexpected horizontal segment");

            return q.U
              - ((q.V - y)
                * ((p.U - q.U) / (p.V - q.V)));
        }

        void AdjustDelta(
          ref int delta,
          UV vertex,
          UV next_vertex,
          UV p)
        {
            switch (delta)
            {
                // make quadrant deltas wrap around:
                case 3: delta = -1; break;
                case -3: delta = 1; break;
                // check if went around point cw or ccw:
                case 2:
                case -2:
                    if (X_intercept(vertex, next_vertex, p.V)
                      > p.U)
                    {
                        delta = -delta;
                    }
                    break;
            }
        }

        public bool PolyGonContains(List<XYZ> xyZArray, XYZ p1)
        {
            UVArray uva = new UVArray(xyZArray);
            return PolygonContains(uva, p1.TOUV());
        }

        /// <summary>
        /// Determine whether given 2D point lies within 
        /// the polygon.
        /// 
        /// Written by Jeremy Tammik, Autodesk, 2009-09-23, 
        /// based on code that I wrote back in 1996 in C++, 
        /// which in turn was based on C code from the 
        /// article "An Incremental Angle Point in Polygon 
        /// Test" by Kevin Weiler, Autodesk, in "Graphics 
        /// Gems IV", Academic Press, 1994.
        /// 
        /// Copyright (C) 2009 by Jeremy Tammik. All 
        /// rights reserved.
        /// 
        /// This code may be freely used. Please preserve 
        /// this comment.
        /// </summary>
        public bool PolygonContains(
          UVArray polygon,
          UV point)
        {
            // initialize
            Quadrant quad = GetQuadrant(
              polygon.get_Item(0), point);

            Quadrant angle = 0;

            // loop on all vertices of polygon
            Quadrant next_quad, delta;
            int n = polygon.Size;
            for (int i = 0; i < n; ++i)
            {
                UV vertex = polygon.get_Item(i);

                UV next_vertex = polygon.get_Item(
                  (i + 1 < n) ? i + 1 : 0);

                // calculate quadrant and delta from last quadrant

                next_quad = GetQuadrant(next_vertex, point);
                delta = next_quad - quad;

                AdjustDelta(
                  ref delta, vertex, next_vertex, point);

                // add delta to total angle sum
                angle = angle + delta;

                // increment for next step
                quad = next_quad;
            }

            // complete 360 degrees (angle of + 4 or -4 ) 
            // means inside

            return (angle == +4) || (angle == -4);

            // odd number of windings rule:
            // if (angle & 4) return INSIDE; else return OUTSIDE;
            // non-zero winding rule:
            // if (angle != 0) return INSIDE; else return OUTSIDE;
        }
    }
    public static class PointInPolyExtension
    {
        /// <summary>
        /// Add new point to list, unless already present.
        /// </summary>
        private static void AddToPunten(
          List<XYZ> XYZarray,
          XYZ p1)
        {
            var p = XYZarray.Where(
              c => Math.Abs(c.X - p1.X) < 0.001
                && Math.Abs(c.Y - p1.Y) < 0.001)
              .FirstOrDefault();

            if (p == null)
            {
                XYZarray.Add(p1);
            }
        }

        /// <summary>
        /// Return a list of boundary 
        /// points for the given room.
        /// </summary>
        private static List<XYZ> MaakPuntArray(
          Room room)
        {
            SpatialElementBoundaryOptions opt
              = new SpatialElementBoundaryOptions();

            opt.SpatialElementBoundaryLocation
              = SpatialElementBoundaryLocation.Center;

            var boundaries = room.GetBoundarySegments(
              opt);

            return MaakPuntArray(boundaries);
        }

        /// <summary>
        /// Return a list of boundary points 
        /// for the given boundary segments.
        /// </summary>
        private static List<XYZ> MaakPuntArray(
          IList<IList<BoundarySegment>> boundaries)
        {
            List<XYZ> puntArray = new List<XYZ>();
            foreach (var bl in boundaries)
            {
                foreach (var s in bl)
                {
                    Curve c = s.GetCurve();
                    AddToPunten(puntArray, c.GetEndPoint(0));
                    AddToPunten(puntArray, c.GetEndPoint(1));
                }
            }
            puntArray.Add(puntArray.First());
            return puntArray;
        }

        /// <summary>
        /// Return a list of boundary 
        /// points for the given area.
        /// </summary>
        private static List<XYZ> MaakPuntArray(
          Area area)
        {
            SpatialElementBoundaryOptions opt
              = new SpatialElementBoundaryOptions();

            opt.SpatialElementBoundaryLocation
              = SpatialElementBoundaryLocation.Center;

            var boundaries = area.GetBoundarySegments(
              opt);

            return MaakPuntArray(boundaries);
        }

        /// <summary>
        /// Check whether this area contains a given point.
        /// </summary>
        /// <summary>
        /// Check whether this area contains a given point.
        /// </summary>
        public static bool AreaContains(this Area a, XYZ p1)
        {
            bool ret = false;
            var p = MaakPuntArray(a);
            PointInPoly pp = new PointInPoly();
            ret = pp.PolyGonContains(p, p1);
            return ret;
        }

        /// <summary>
        /// Check whether this room contains a given point.
        /// </summary>
        public static bool RoomContains(this Room r, XYZ p1)
        {
            bool ret = false;
            var p = MaakPuntArray(r);
            PointInPoly pp = new PointInPoly();
            ret = pp.PolyGonContains(p, p1);
            return ret;
        }

        /// <summary>
        /// Project an XYZ point to a UV one in the 
        /// XY plane by simply dropping the Z coordinate.
        /// </summary>
        public static UV TOUV(this XYZ point)
        {
            UV ret = new UV(point.X, point.Y);
            return ret;
        }
    }
}
