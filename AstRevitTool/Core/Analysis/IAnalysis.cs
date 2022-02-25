using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using AstRevitTool;
using AstRevitTool.Core.Export;

namespace AstRevitTool.Core.Analysis
{
    //
    // Summary:
    //     An interface that is used to analyze custom building performance metrics for visible elements in a Revit model.
    //
    // Remarks:
    //     An instance of a class that implements this interface is passed in as a parameter
    //     of the AST_Revit_Toolkit.Export.MetricExporter. The methods of the context
    //     are then called at times of exporting entities of the model. This is a base class
    //     for two other interfaces derived from it: Autodesk.Revit.DB.IPhotoRenderContext
    //     and Autodesk.Revit.DB.IModelExportContext. This base class contains methods that
    //     are common to both the leaf interfaces. Although it is still possible to use
    //     classes deriving directly from this base interface (for backward compatibility),
    //     future applications should implement the new leaf interfaces only.
    public interface IAnalysis
    {
        void Extraction();

        void Analyze();
        string Report();

        string Type();

        string Conclusion();

        Dictionary<string, double> ResultList();
    }
}
