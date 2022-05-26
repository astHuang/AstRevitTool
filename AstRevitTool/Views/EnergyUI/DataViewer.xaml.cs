using CarboLifeAPI;
using CarboLifeAPI.Data;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

namespace AstRevitTool.Views.EnergyUI
{
    /// <summary>
    /// Interaction logic for DataViewer.xaml
    /// </summary>
    public partial class DataViewer : UserControl
    {
        public CarboProject CarboLifeProject;

        public DataViewer()
        {
            InitializeComponent();
        }

        private void UserControl_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            try
            {
                DependencyObject parent = VisualTreeHelper.GetParent(this);
                Window parentWindow = Window.GetWindow(parent);
                EnergyMainWindow mainViewer = parentWindow as EnergyMainWindow;

                if (mainViewer != null)
                    CarboLifeProject = mainViewer.getCarbonLifeProject();

                if (CarboLifeProject != null)
                {
                    //A project Is loaded, Proceed to next
                    SetupInterFace();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void SetupInterFace()
        {
            try
            {
                dgv_Overview.ItemsSource = CarboLifeProject.getGroupList;
                SortData();

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void Btn_Calculate_Click(object sender, RoutedEventArgs e)
        {
            Calculate();
        }

        private void Calculate()
        {
            CarboLifeProject.CalculateProject();
            refreshData();
        }

        private void Btn_Material_Click(object sender, RoutedEventArgs e)
        {
            if (dgv_Overview.SelectedItems.Count > 0)
            {
                try
                {
                    //Select all the groups
                    var selectedItems = dgv_Overview.SelectedItems;
                    IList<CarboGroup> selectedGroups = new List<CarboGroup>();

                    // ... Add all Names to a List.
                    foreach (var item in selectedItems)
                    {
                        CarboGroup cg = item as CarboGroup;
                        selectedGroups.Add(cg);
                    }

                    if (selectedGroups.Count > 0)
                    {
                        CarboGroup carboGroup = selectedGroups[0];

                        MaterialSelector materialEditor = new MaterialSelector(carboGroup.Material.Name, CarboLifeProject.CarboDatabase);
                        materialEditor.ShowDialog();
                        //If okay change the materials and re-calculate project
                        if (materialEditor.isAccepted == true)
                        {
                            foreach (CarboGroup cg in selectedGroups)
                            {
                                CarboLifeProject.UpdateMaterial(cg, materialEditor.selectedMaterial);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }





            CarboLifeProject.CalculateProject();
            refreshData();

        }

        private void btn_OpenMaterialEditor_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                //Check if a group has been selected:
                CarboGroup PotentialSelectedCarboGroup = new CarboGroup();
                PotentialSelectedCarboGroup.MaterialName = "";
                DependencyObject parent = VisualTreeHelper.GetParent(this);
                Window parentWindow = Window.GetWindow(parent);
                EnergyMainWindow mainViewer = parentWindow as EnergyMainWindow;

                if (mainViewer != null)
                    CarboLifeProject = mainViewer.getCarbonLifeProject();

                if (dgv_Overview.SelectedItems.Count > 0)
                {

                    var selectedItems = dgv_Overview.SelectedItems;
                    IList<CarboGroup> selectedGroups = new List<CarboGroup>();

                    // ... Add all Names to a List.
                    foreach (var item in selectedItems)
                    {
                        CarboGroup cg = item as CarboGroup;
                        if (cg != null)
                            selectedGroups.Add(cg);
                    }

                    if (selectedGroups.Count > 0)
                    {
                        PotentialSelectedCarboGroup = selectedGroups[0];
                    }
                }

                if (CarboLifeProject.CarboDatabase.CarboMaterialList.Count > 0)
                {
                    CarboMaterial carbomat = CarboLifeProject.CarboDatabase.CarboMaterialList[0];

                    if (PotentialSelectedCarboGroup.MaterialName != "")
                    {
                        //A group with a valid material was selected
                        carbomat = PotentialSelectedCarboGroup.Material;
                    }

                    if (carbomat == null)
                        carbomat = new CarboMaterial();

                    MaterialEditor materialEditor = new MaterialEditor(carbomat.Name, CarboLifeProject.CarboDatabase);
                    materialEditor.ShowDialog();

                    if (materialEditor.acceptNew == true)
                    {
                        CarboLifeProject.CarboDatabase = materialEditor.returnedDatabase;

                        CarboLifeProject.UpdateAllMaterials();
                    }
                }
                else
                {
                    MessageBox.Show("There were no materials found in the project, please re-create your project");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            CarboLifeProject.CalculateProject();
            refreshData();
        }

        public void refreshData()
        {
            dgv_Overview.ItemsSource = null;
            dgv_Overview.ItemsSource = CarboLifeProject.getGroupList;

            //GetTotals
            double totals = 0;

            if (CarboLifeProject.getGroupList.Count > 0)
            {
                totals = CarboLifeProject.getTotalsGroup().EC;
            }
            else
            {
                totals = 0;
            }

            lbl_Total.Content = "TOTAL: " + Math.Round(totals, 2) + " tCO₂";


            SortData();
        }

        private void SortData()
        {
            if (cbb_Sort.Text == "Material")
            {
                SortByMaterial();
            }
            else
            {
                SortByCategoty();
            }
        }

        private void SortByMaterial()
        {
            if (CarboLifeProject.getGroupList != null)
            {
                ListCollectionView collectionView = new ListCollectionView(CarboLifeProject.getGroupList);
                collectionView.GroupDescriptions.Add(new PropertyGroupDescription("MaterialName"));
                dgv_Overview.ItemsSource = null;
                dgv_Overview.ItemsSource = collectionView;
            }
        }

        private void SortByCategoty()
        {
            if (CarboLifeProject.getGroupList != null)
            {
                ListCollectionView collectionView = new ListCollectionView(CarboLifeProject.getGroupList);
                collectionView.GroupDescriptions.Add(new PropertyGroupDescription("Category"));
                dgv_Overview.ItemsSource = null;
                dgv_Overview.ItemsSource = collectionView;
            }
        }

        private void Dgv_Overview_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            CarboGroup carboGroup = (CarboGroup)dgv_Overview.SelectedItem;
            if (carboGroup != null)
            {
                dgv_Elements.ItemsSource = carboGroup.AllElements;
            }
        }

        private void Dgv_Overview_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            if (dgv_Overview != null)
            {
                CarboGroup carboGroup = (CarboGroup)dgv_Overview.SelectedItem;

                if (carboGroup != null)
                {
                    TextBox t = e.EditingElement as TextBox;
                    DataGridColumn dgc = e.Column;

                    if (t != null)
                    {
                        //Corrections:
                        if (dgc.Header.ToString().StartsWith("Correction"))
                        {
                            string textExpression = t.Text;
                            if (CarboLifeAPI.Utils.isValidExpression(textExpression) == true)
                            {
                                carboGroup.Correction = textExpression;
                                carboGroup.CalculateTotals();

                                CarboLifeProject.UpdateGroup(carboGroup);

                            }
                            else
                            {
                                carboGroup.Correction = "";
                                carboGroup.CalculateTotals();

                                CarboLifeProject.UpdateGroup(carboGroup);
                            }
                        }
                        if (dgc.Header.ToString().StartsWith("Volume"))
                        {
                            if (carboGroup.AllElements.Count > 0)
                            {
                                MessageBox.Show("The volume of this group is calculated using the element volumes extracted from the 3D model," + Environment.NewLine + " you need to purge the elements before overriding the volume");
                                carboGroup.CalculateTotals();
                                CarboLifeProject.UpdateGroup(carboGroup);

                                //System.Threading.Thread.Sleep(500);
                                //Calculate();
                            }
                            else
                            {
                                double volumeEdit = CarboLifeAPI.Utils.ConvertMeToDouble(t.Text);
                                if (volumeEdit != 0)
                                {
                                    carboGroup.Volume = volumeEdit;

                                    carboGroup.CalculateTotals();
                                    CarboLifeProject.UpdateGroup(carboGroup);
                                    //carboGroup.CalculateTotals();
                                }
                            }
                        }
                        //Waste
                        //Corrections:
                        if (dgc.Header.ToString().StartsWith("Waste"))
                        {
                            double wastevalue = CarboLifeAPI.Utils.ConvertMeToDouble(t.Text);
                            if (wastevalue != 0)
                            {
                                carboGroup.Waste = wastevalue;

                                carboGroup.CalculateTotals();
                                CarboLifeProject.UpdateGroup(carboGroup);
                                //carboGroup.CalculateTotals();
                            }
                        }
                        //Additional:
                        if (dgc.Header.ToString().StartsWith("Additional"))
                        {
                            double additional = CarboLifeAPI.Utils.ConvertMeToDouble(t.Text);
                            if (additional != 0)
                            {
                                carboGroup.Additional = additional;

                                carboGroup.CalculateTotals();
                                CarboLifeProject.UpdateGroup(carboGroup);
                                //carboGroup.CalculateTotals();
                            }
                        }

                        //B4:
                        if (dgc.Header.ToString().StartsWith("Group"))
                        {
                            double b4 = CarboLifeAPI.Utils.ConvertMeToDouble(t.Text);
                            if (b4 != 0)
                            {
                                carboGroup.B4Factor = b4;

                                carboGroup.CalculateTotals();
                                CarboLifeProject.UpdateGroup(carboGroup);
                                //carboGroup.CalculateTotals();
                            }
                        }
                        //The below triggers an error when switching cells too fast, no idea why need to resolve.
                        //dgv_Overview.ItemsSource = null;
                        //dgv_Overview.ItemsSource = CarboLifeProject.getGroupList;
                        //SortData();
                    }
                }
            }
        }

        private void ComboBox_DropDownClosed(object sender, EventArgs e)
        {
            SortData();
        }

        private void RoundValue(object sender, RoutedEventArgs e)
        {
            TextBlock tb = ((TextBlock)sender);

            // do anything with textblock    
            if (tb.Text != null)
            {
                double value = CarboLifeAPI.Utils.ConvertMeToDouble(tb.Text);

                tb.Text = Math.Round(value, 2).ToString();
            }
        }

        private void PercentValue(object sender, RoutedEventArgs e)
        {
            TextBlock tb = ((TextBlock)sender);

            // do anything with textblock    
            if (tb.Text != null)
            {
                double value = CarboLifeAPI.Utils.ConvertMeToDouble(tb.Text);

                tb.Text = Math.Round(value, 2).ToString() + " % ";
            }
        }
    }
}
