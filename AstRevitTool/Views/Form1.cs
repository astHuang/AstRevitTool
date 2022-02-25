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



namespace AstRevitTool.Views
{
    public partial class Form1 : System.Windows.Forms.Form
    {
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
            string filename = Core.Export.ASTExportUtils.filepath(this.maindoc, this.my_analysis, ".csv");
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
            }
            //var m_writer = new StreamWriter(filename);
            //Export.ASTExportUtils.csvExport(m_writer, this.my_analysis);
            //m_writer.Close();
            string msg = "CSV file is saved! File location: " + savefile.FileName;
            string title = "Successfully saved file!";
            MessageBox.Show(msg, title);
            this.label4.Text = msg;

        }
    }
}
