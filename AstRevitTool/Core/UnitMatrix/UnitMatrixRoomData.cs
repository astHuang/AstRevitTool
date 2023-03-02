using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AstRevitTool.Core.UnitMatrix
{
    public enum UnitType
    {
        None,
        Studio,
        OneBed,
        TwoBed,
        ThreeBed,       
    }
    public class UnitMatrixRoomData
    {
        public UnitType uType;
        public string Unit { get; private set; } 
        public int TotalCount { get; private set; }

        public int RSFperUnit { get; set; }
        public int TotalRSF { get; private set; }

        public string Variation { get; private set; }

        public Dictionary<Level,int> levelCountPair { get; private set; }

    }

    public class UnitMatrix
    {
        public ICollection<UnitMatrixRoomData> roomData;
        public List<DataGridViewRow> selectedRows;
        public UnitMatrix(List<DataGridViewRow> selectedRows)
        {
            this.selectedRows = selectedRows;
            GenerateRoomData(selectedRows);
        }

        private void GenerateRoomData(List<DataGridViewRow> selectedRows)
        {
            throw new NotImplementedException();
        }
    }

    
}
