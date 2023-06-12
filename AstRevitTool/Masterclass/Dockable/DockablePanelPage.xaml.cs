using Autodesk.Revit.UI;


namespace AstRevitTool.Masterclass.Dockable
{
    /// <summary>
    /// Interaction logic for DockablePanelPage.xaml
    /// </summary>
    public partial class DockablePanelPage : IDockablePaneProvider
    {
        public DockablePanelPage()
        {
            InitializeComponent();
        }

        public void SetupDockablePane(DockablePaneProviderData data)
        {
            data.FrameworkElement = this;
            data.InitialState = new DockablePaneState
            {
                DockPosition = DockPosition.Tabbed,
                TabBehind = DockablePanes.BuiltInDockablePanes.ProjectBrowser
            };
            data.VisibleByDefault = true;
        }

        private void Button_Click(object sender, System.Windows.RoutedEventArgs e)
        {

        }
    }
}
