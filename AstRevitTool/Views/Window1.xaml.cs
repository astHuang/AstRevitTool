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

namespace AstRevitTool.Views
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class Window1 : Window
    {
        public int lod = 8;
        public bool useTexture;
        public bool blackAndWhite;

        public bool useCentimeter;
        public bool skipInterior;
        public bool optimizeSolid = false;

        public bool exportBinary;
        public Window1()
        {
            InitializeComponent();
            this.lod = 4;
            this.useTexture = true;
            this.useCentimeter = (bool)this.UseCentimeter.IsChecked;
            this.blackAndWhite = (bool)this.UseBlackWhite.IsChecked;

            this.skipInterior = (bool)this.SkipInterior.IsChecked;
            this.exportBinary = (bool)this.BinaryExport.IsChecked;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.lod = (int)lodValueSlider.Value;
            this.useTexture = (bool)this.UseTexture.IsChecked;
            this.useCentimeter = (bool)this.UseCentimeter.IsChecked;
            this.blackAndWhite = (bool)this.UseBlackWhite.IsChecked;
            this.skipInterior= (bool)this.SkipInterior.IsChecked;
            this.exportBinary = (bool)this.BinaryExport.IsChecked;
            this.Close();
        }

        private void lodValueSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            this.lod = (int)lodValueSlider.Value;
            if(this.lodText != null) { this.lodText.Text = "Current LoD: " + lodValueSlider.Value.ToString(); }          
        }

        private void UseTexture_Checked(object sender, RoutedEventArgs e)
        {
            /*
            if(this.UseBlackWhite != null && this.UseBlackWhite.IsChecked!=null)
            {
                this.UseBlackWhite.IsChecked = false;
            }*/
        }
    }
}
