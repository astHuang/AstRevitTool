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
using System.Windows.Shapes;
using Autodesk.Revit.DB;
using AstRevitTool.Core.Analysis;
using AstRevitTool.Core;
using Syncfusion.UI.Xaml.TreeGrid;

namespace AstRevitTool.Views
{
    /// <summary>
    /// Interaction logic for Analysis_Main.xaml
    /// </summary>
    public partial class Analysis_Main : Window
    {
        public Analysis_Main()
        {
            InitializeComponent();
        }

        public Analysis_Main(Document doc,CustomAnalysis custom_Analysis)
        {
            InitializeComponent();
            this.Analysis = custom_Analysis;
            this.MainDoc = doc;

        }

        public Analysis_Main(ElementsVisibleInViewExportContext context,Document doc, Autodesk.Revit.ApplicationServices.Application app)
        {
            InitializeComponent();
            //contentArea.Children.Clear();
            Context = context;
            App = app;
        }

        public CustomAnalysis Analysis;
        public Document MainDoc;
        public ElementsVisibleInViewExportContext Context;
        public Autodesk.Revit.ApplicationServices.Application App;

        private void btnBeginAnalysis_Click(object sender, RoutedEventArgs e)
        {
            //contentArea.Children.Clear();
            Analysis = new CustomAnalysis(Context, App);
            Analysis.Extraction();
            Analysis.Analyze();

            
            ViewModelByCategory viewModel = new ViewModelByCategory(this.Analysis);

            DataContext = viewModel;

            //contentArea.Children.Add(contentBorder);

        }

        private void btnHide_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btnHighlightSelected_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btnSelect_Click(object sender, RoutedEventArgs e)
        {

        }
    }


}
