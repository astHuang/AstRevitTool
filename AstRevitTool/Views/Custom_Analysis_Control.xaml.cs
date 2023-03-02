using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using AstRevitTool.Core.Analysis;
using AstRevitTool.Core.Export;

namespace AstRevitTool.Views
{
    /// <summary>
    /// Interaction logic for Custom_Analysis_Control.xaml
    /// </summary>
    public partial class Custom_Analysis_Control : UserControl
    {
        public Custom_Analysis_Control()
        {
            InitializeComponent();
        }

        public Custom_Analysis_Control(CustomAnalysis analysis)
        {
            InitializeComponent();
            this.Collection = analysis.DataTree;
        }

        private void CreateTreeView()
        {
            
        }

        public CustomAnalysis.DataTypes Collection;
    }

}
