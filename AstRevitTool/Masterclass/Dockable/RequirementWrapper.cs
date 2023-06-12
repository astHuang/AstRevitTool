using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;

namespace AstRevitTool.Masterclass.Dockable
{
    public class RequirementWrapper : INotifyPropertyChanged
    {
        public string input { get; set; }
        public string FamilyName { get; set; }

        public string FamilyType { get; set; }

        public int Count { get; set; }

        private int _placeCount;

        public event PropertyChangedEventHandler PropertyChanged;

        public int PlaceCount { get { return _placeCount; } set { _placeCount = value; } }
        public RequirementWrapper(string fn, string ft, int count)
        {
            FamilyName = fn;
            FamilyType = ft;
            Count = count;
        }

    }
}
