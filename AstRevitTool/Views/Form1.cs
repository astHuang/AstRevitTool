using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Diagnostics;
using System.Collections;
using System.Windows.Forms;
using AstRevitTool.Core.Analysis;
using AstRevitTool.Core.Export;
using Autodesk.Revit.DB;
using System.Net;
using System.Net.Mail;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows.Media;



namespace AstRevitTool.Views
{

    public partial class Form1 : System.Windows.Forms.Form
    {
        public static string[] ColourValues = new string[] {
        "FF0000", "00FF00", "0000FF", "FFFF00", "FF00FF", "00FFFF", "000000",
        "800000", "008000", "000080", "808000", "800080", "008080", "808080",
        "C00000", "00C000", "0000C0", "C0C000", "C000C0", "00C0C0", "C0C0C0",
        "400000", "004000", "000040", "404000", "400040", "004040", "404040",
        "200000", "002000", "000020", "202000", "200020", "002020", "202020",
        "600000", "006000", "000060", "606000", "600060", "006060", "606060",
        "A00000", "00A000", "0000A0", "A0A000", "A000A0", "00A0A0", "A0A0A0",
        "E00000", "00E000", "0000E0", "E0E000", "E000E0", "00E0E0", "E0E0E0",
        };

        public static byte[] HexStringToColor(string hexColor)
        {
            var color = (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#"+hexColor);
            byte r = color.R;
            byte g = color.G;
            byte b = color.B;
            return new byte[] {r,g,b};
        }

        public static Dictionary<string,int> elementMap(List<Element> selected)
        {
            Dictionary<string,int> result = new Dictionary<string,int>();
            foreach(Element element in selected)
            {
                if (result.ContainsKey(element.Category.Name))
                {
                    result[element.Category.Name] += 1;
                }
                else
                {
                    result.Add(element.Category.Name, 1);
                }
            }
            return result;
        }

        public Form1()
        {
            InitializeComponent();

        }

        public Form1(IAnalysis analysis, Document document)
        {
            InitializeComponent();
            CommonInit(analysis,document);
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }


        private void button2_Click(object sender, EventArgs e)
        {
            try
            {
                //SmtpClient client = new SmtpClient("smtp.outlook.com", 25);
                //var message = new MailMessage();
                //string str = String.Format("Please email {0} for help.",
                //   "<a href=\"huang@arrowstreet.com\">here</a>");
                MessageBox.Show("Oops, we're sorry for the error! Jumping to email to ask the developer for help!");
                string url = "mailto:huang@arrowstreet.com?subject=Re:Error_Report&body=";
                string body = "Describe the errors you encountered...\n";
                System.Diagnostics.Process.Start(url+body);
            }
            catch { }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string filename = Core.Export.ASTExportUtils.filename(this.maindoc, this.my_analysis);
            SaveFileDialog savefile = new SaveFileDialog();
            savefile.FileName = filename;
            savefile.Filter = "CSV|*.csv";
            if (savefile.ShowDialog() == DialogResult.OK)
            {
                using (StreamWriter sw = new StreamWriter(savefile.FileName))
                {
                    Core.Export.ASTExportUtils.csvExport(sw, this.my_analysis);
                    sw.Close();
                }
                string msg = "CSV file is saved! File location: " + savefile.FileName;
                string title = "Successfully saved file!";

                MessageBox.Show(msg, title);
                this.label4.Text = msg;
            }
            //var m_writer = new StreamWriter(filename);
            //Export.ASTExportUtils.csvExport(m_writer, this.my_analysis);
            //m_writer.Close();

        }

        private void button3_Click(object sender, EventArgs e)
        {
            string filename = Core.Export.ASTExportUtils.filename(this.maindoc, this.my_analysis);
            SaveFileDialog savefile = new SaveFileDialog();
            savefile.FileName = filename;
            savefile.Filter = "Text File | *.txt";
            if (savefile.ShowDialog() == DialogResult.OK)
            {
                using (StreamWriter sw = new StreamWriter(savefile.FileName))
                {
                    Core.Export.ASTExportUtils.txtExport(sw,this.my_analysis);
                    sw.Close();
                }
                string msg = "Text file is saved! File location: " + savefile.FileName;
                string title = "Successfully saved file!";

                MessageBox.Show(msg, title);
                this.label4.Text = msg;
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {

            var selecteditems = this.m_lvData.SelectedItems;
            List<Element> allselected = new List<Element>();
            List<List<Element>> tree = new List<List<Element>>();
            foreach (ListViewItem item in selecteditems)
            {
                var elelist = item.Tag as List<Element>;
                allselected.AddRange(elelist);
                tree.Add(elelist);
            }

            Dictionary<string,int> map = elementMap(allselected);

            //Check whether element in linked file or main doc
            List<List<ElementId>> tohighlited = new List<List<ElementId>>();
            foreach (List<Element> elelist in tree) {
                List<ElementId> list = new List<ElementId>();
                foreach (Element el in elelist)
                {
                    if (el.Document.Equals(this.maindoc))
                    {
                        list.Add(el.Id);
                    }
                }
                tohighlited.Add(list);
            }
            string[] colors = ColourValues.Skip(0).Take(tree.Count).ToArray();
            List<Autodesk.Revit.DB.Color> rvtColors = new List<Autodesk.Revit.DB.Color>();
            List<byte[]> argbs = new List<byte[]>();
            foreach (string color in colors)
            {
                byte[] rgb = HexStringToColor(color);
                argbs.Add(rgb);
                rvtColors.Add(new Autodesk.Revit.DB.Color(rgb[0], rgb[1], rgb[2]));
            }

            List<ElementId> flattenedList = tohighlited.SelectMany(x => x).ToList();
            if (allselected.Count > flattenedList.Count)
            {
                MessageBox.Show("There are " + (allselected.Count - flattenedList.Count).ToString() + " elements in linked file that we cannot color! Please go into linked model to check");
            }

            using (Transaction tr = new Transaction(this.maindoc))
            {
                tr.Start("color elements");
                int count = 0;
                foreach (List<ElementId> sublist in tohighlited)
                {                   
                    OverrideGraphicSettings ogs = new OverrideGraphicSettings();
                    Autodesk.Revit.DB.Color color = rvtColors[count];
                    this.m_lvData.SelectedItems[count].BackColor = System.Drawing.Color.FromArgb(argbs[count][0],argbs[count][1],argbs[count][2]);
                    ogs.SetProjectionLineColor(color);
                    ogs.SetProjectionLineWeight(10);
                    ogs.SetSurfaceForegroundPatternColor(color);
                    var patternCollector = new FilteredElementCollector(this.maindoc.ActiveView.Document);
                    patternCollector.OfClass(typeof(Autodesk.Revit.DB.FillPatternElement));
                    Autodesk.Revit.DB.FillPatternElement solidFill = patternCollector.ToElements().Cast<Autodesk.Revit.DB.FillPatternElement>().First(x => x.GetFillPattern().IsSolidFill);
                    ogs.SetSurfaceForegroundPatternId(solidFill.Id);

                    foreach (ElementId eid in sublist) {
                        this.maindoc.ActiveView.SetElementOverrides(eid, ogs);
                    }

                    count++;
                }
                tr.Commit();
            }
            string text = this.my_analysis.Conclusion();
            text += "\n\n Selection Detail:";
            foreach(KeyValuePair<string, int> entry in map)
            {
                text += "\n Element of "+ entry.Key +": "+ entry.Value;
            }
            label3.Text = text;
            /*
            HashSet<Document> alldocs = new HashSet<Document>();

            foreach (Element el in this.my_analysis.AllAnalyzedElement())
            {
                Document doc = el.Document;
                alldocs.Add(doc);
                using (Transaction tr = new Transaction(doc)) { 
                    tr.Start("change color");
                    doc.ActiveView.SetElementOverrides(id, ogs);
                    tr.Commit();
                }
                    
            }/*
            /*
            var loadedExternalFilesRef = new List<RevitLinkType>();
            var collector = new FilteredElementCollector(this.maindoc);
            foreach (Element element in collector.OfClass(typeof(RevitLinkType)))
            {
                ExternalFileReference extFileRef = element.GetExternalFileReference();
                if (null == extFileRef || extFileRef.GetLinkedFileStatus() != LinkedFileStatus.Loaded)
                    continue;
                var revitLinkType = (RevitLinkType)element;
                loadedExternalFilesRef.Add(revitLinkType);
                revitLinkType.Unload(null);
            }*/
            /*
            foreach (Document d in alldocs)
            {
                using (Transaction tr = new Transaction(d))
                {
                    tr.Start("change color");
                    d.ActiveView.SetElementOverrides(id, ogs);
                    tr.Commit();
                }
            }*/
            
        }

        private void button5_Click(object sender, EventArgs e)
        {
            OverrideGraphicSettings ogs = new OverrideGraphicSettings();
            using(Transaction tr = new Transaction(this.maindoc))
            {
                tr.Start("resume color");
                foreach(Element el in this.my_analysis.AllAnalyzedElement())
                {
                    ElementId id = el.Id;
                    this.maindoc.ActiveView.SetElementOverrides(id, ogs);                  
                }
                tr.Commit();
            }
            foreach(ListViewItem item in this.m_lvData.Items)
            {
                item.BackColor = System.Drawing.Color.White;
            }
            string text = this.my_analysis.Conclusion();
            this.label3.Text = text;

        }

        private void m_lvData_SelectedIndexChanged(object sender, EventArgs e)
        {
            this.m_lvData.Sort() ;
        }
    }
}
