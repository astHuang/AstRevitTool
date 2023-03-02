﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.DB;

namespace AstRevitTool.Core
{
    class LineDrawings2DExportContext:IExportContext2D
    {
        #region Data
        /// <summary>
        /// The list of (start, end) points for all tessellated lines
        /// </summary>
        private IList<XYZ> m_points = new List<XYZ>();

        /// <summary>
        /// The number of all processed elements, as well as breakdown by some element kinds
        /// </summary>
        private int m_numElements = 0;
        private int m_numTexts = 0;

        /// <summary>
        /// All text collected in the view, with a newline between each TextNode.
        /// </summary>
        private string m_texts;

        Element m_currentElem = null;

        public int NumElements
        {
            get
            {
                return m_numElements;
            }
        }

        public int NumTexts
        {
            get
            {
                return m_numTexts;
            }
        }

        public string Texts
        {
            get
            {
                return m_texts;
            }
        }
        #endregion

        #region IExportContext2DOverrides

        public LineDrawings2DExportContext(out IList<XYZ> points)
        {
            points = m_points;
        }

        public bool Start()
        {
            return true;
        }

        public void Finish()
        {
        }

        public bool IsCanceled()
        {
            return false;
        }

        public RenderNodeAction OnViewBegin(ViewNode node)
        {
            return RenderNodeAction.Proceed;
        }

        public void OnViewEnd(ElementId elementId)
        {
        }

        public RenderNodeAction OnInstanceBegin(InstanceNode node)
        {
            return RenderNodeAction.Proceed;
        }

        public void OnInstanceEnd(InstanceNode node)
        {
        }

        public RenderNodeAction OnLinkBegin(LinkNode node)
        {
            return RenderNodeAction.Proceed;
        }

        public void OnLinkEnd(LinkNode node)
        {
        }

        public RenderNodeAction OnElementBegin(ElementId elementId)
        {
            return RenderNodeAction.Skip;
        }

        public void OnElementEnd(ElementId elementId)
        {
        }

        public RenderNodeAction OnElementBegin2D(ElementNode node)
        {
            m_numElements++;

            m_currentElem = node.Document.GetElement(node.ElementId);

            return RenderNodeAction.Proceed;
        }

        public void OnElementEnd2D(ElementNode node)
        {
            m_currentElem = null;
        }

        public RenderNodeAction OnFaceBegin(FaceNode node)
        {
            return RenderNodeAction.Proceed;
        }

        public void OnFaceEnd(FaceNode node)
        {
        }

        public RenderNodeAction OnCurve(CurveNode node)
        {
            // Customize tessellation of annotation curves
            if (m_currentElem.Category.CategoryType == CategoryType.Annotation)
            {
                IList<XYZ> list = new List<XYZ>();

                Curve curve = node.GetCurve();
                if (curve is Line)
                {
                    Line l = curve as Line;
                    list.Add(l.GetEndPoint(0));
                    list.Add(l.GetEndPoint(1));
                }
                else
                {
                    list = curve.Tessellate();
                }

                Util.addTo(m_points, list);
                return RenderNodeAction.Skip;
            }

            return RenderNodeAction.Proceed;
        }

        public RenderNodeAction OnPolyline(PolylineNode node)
        {
            // Customize processing of annotation polylines
            if (m_currentElem.Category.CategoryType == CategoryType.Annotation)
            {
                PolyLine pLine = node.GetPolyline();
                IList<XYZ> list = pLine.GetCoordinates();
                Util.addTo(m_points, list);
                return RenderNodeAction.Skip;
            }

            return RenderNodeAction.Proceed;
        }

        public RenderNodeAction OnFaceEdge2D(FaceEdgeNode node)
        {
            // Customize tessellation of annotation face edges
            //if (m_currentElem.Category.CategoryType == CategoryType.Annotation)
            //{
            //   Curve curve = node.GetFaceEdge().AsCurve();
            //   if (curve != null)
            //   {
            //      curve = curve.CreateTransformed(node.GetInstanceTransform());
            //      IList<XYZ> list = curve.Tessellate();
            //      Utilities.addTo(m_points, list);
            //   }
            //}
            return RenderNodeAction.Proceed;
        }

        public RenderNodeAction OnFaceSilhouette2D(FaceSilhouetteNode node)
        {
            return RenderNodeAction.Proceed;
        }

        public void OnText(Autodesk.Revit.DB.TextNode node)
        {
            m_texts += "\n" + node.Text;
            ++m_numTexts;
        }

        public void OnLight(LightNode node)
        {
        }

        public void OnRPC(RPCNode node)
        {
        }

        public void OnMaterial(MaterialNode node)
        {
        }

        public void OnPolymesh(PolymeshTopology node)
        {
        }

        public void OnLineSegment(LineSegment segment)
        {
            XYZ segmentStart = segment.StartPoint;
            XYZ segmentEnd = segment.EndPoint;

            IList<XYZ> list = new List<XYZ>();
            list.Add(segmentStart);
            list.Add(segmentEnd);
            Util.addTo(m_points, list);
        }

        public void OnPolylineSegments(PolylineSegments segments)
        {
            IList<XYZ> segPoints = segments.GetVertices();
            Util.addTo(m_points, segPoints);
        }
        #endregion
    }
}
