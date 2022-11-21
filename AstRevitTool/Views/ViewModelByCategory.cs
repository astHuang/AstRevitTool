using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AstRevitTool.Core.Analysis;
using System.Collections.ObjectModel;

namespace AstRevitTool.Views
{
    public class ViewModelByCategory
    {
        public ViewModelByCategory()
        {
            this._dataDetails = new ObservableCollection<SourceDataTypes>();
        }

        private ObservableCollection<SourceDataTypes> _dataDetails;

        public ObservableCollection<SourceDataTypes> DataDetails { get { return _dataDetails; } }

        public ViewModelByCategory(CustomAnalysis analysis)
        {
            this._dataDetails = this.CreateData(analysis);
        }

        private ObservableCollection<SourceDataTypes> CreateData(CustomAnalysis analysis)
        {
            var dataList = new ObservableCollection<SourceDataTypes>();
            foreach (CustomAnalysis.DataType dataType in analysis.DataTree)
            {
                dataList.Add(new SourceDataTypes(dataType));
            }
            return dataList;
        }
    }
}
