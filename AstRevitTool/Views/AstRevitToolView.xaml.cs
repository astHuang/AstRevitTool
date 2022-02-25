using AstRevitTool.ViewModels;

namespace AstRevitTool.Views
{
    public partial class AstRevitToolView
    {
        public AstRevitToolView(AstRevitToolViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
        }
    }
}