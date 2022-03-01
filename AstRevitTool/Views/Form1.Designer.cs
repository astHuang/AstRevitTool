using System;
using System.Diagnostics;
using System.Drawing;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.ComponentModel;
using System.Windows.Forms;

using Autodesk.Revit.DB;
using AstRevitTool.Core.Analysis;
using AstRevitTool.Core.Export;

namespace AstRevitTool.Views
{
    partial class Form1
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        protected void CommonInit(IAnalysis analysis, Document doc)
        {
            
            this.maindoc = doc;
            this.my_analysis = analysis;
            this.Text = analysis.Type() + " @" + doc.Title;
            this.label1.Text = analysis.Type();
            string time = DateTime.Now.ToString(@"MM\/dd\/yyyy h\:mm tt");
            string title = "Arrowstreet Revit Project Report" + "\nAnalyze time: " + time;
            this.label2.Text = title + "\nAnalyze view: " + doc.ActiveView.Name;
            m_lvData.Items.Clear();
            foreach (KeyValuePair<string, double> entry in analysis.ResultList())
            {
                string number = entry.Value.ToString("0.##");
                var row = new string[] { entry.Key, number };
                var lvi = new ListViewItem(row);
                lvi.Tag = entry;
                m_lvData.Items.Add(lvi);
            }

            this.label3.Text = analysis.Conclusion();
            this.label3.AutoSize = true;

            string path = Core.Export.ASTExportUtils.filepath(doc, analysis, ".txt");
            this.label4.Text = "Document file is saved! File location: " + path;
        }


        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            this.m_bnOK = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.m_lvData = new System.Windows.Forms.ListView();
            this.m_lvCol_label = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.m_lvCol_value = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.button2 = new System.Windows.Forms.Button();
            this.button1 = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // m_bnOK
            // 
            this.m_bnOK.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.m_bnOK.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.m_bnOK.Location = new System.Drawing.Point(14, 458);
            this.m_bnOK.Name = "m_bnOK";
            this.m_bnOK.Size = new System.Drawing.Size(90, 27);
            this.m_bnOK.TabIndex = 3;
            this.m_bnOK.Text = "OK";
            // 
            // label1
            // 
            this.label1.AutoEllipsis = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 13.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(12, 14);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(288, 82);
            this.label1.TabIndex = 4;
            this.label1.Text = "label1";
            // 
            // label2
            // 
            this.label2.AutoEllipsis = true;
            this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.Location = new System.Drawing.Point(14, 105);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(288, 57);
            this.label2.TabIndex = 5;
            this.label2.Text = "label2";
            // 
            // m_lvData
            // 
            this.m_lvData.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.m_lvData.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.m_lvCol_label,
            this.m_lvCol_value});
            this.m_lvData.FullRowSelect = true;
            this.m_lvData.GridLines = true;
            this.m_lvData.HideSelection = false;
            this.m_lvData.Location = new System.Drawing.Point(325, 14);
            this.m_lvData.Name = "m_lvData";
            this.m_lvData.ShowItemToolTips = true;
            this.m_lvData.Size = new System.Drawing.Size(450, 471);
            this.m_lvData.TabIndex = 6;
            this.m_lvData.UseCompatibleStateImageBehavior = false;
            this.m_lvData.View = System.Windows.Forms.View.Details;
            // 
            // m_lvCol_label
            // 
            this.m_lvCol_label.Text = "Item";
            this.m_lvCol_label.Width = 346;
            // 
            // m_lvCol_value
            // 
            this.m_lvCol_value.Text = "Area";
            this.m_lvCol_value.Width = 100;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(14, 218);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(0, 16);
            this.label3.TabIndex = 7;
            // 
            // label4
            // 
            this.label4.AutoEllipsis = true;
            this.label4.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label4.Location = new System.Drawing.Point(14, 374);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(288, 81);
            this.label4.TabIndex = 8;
            this.label4.Text = "label4";
            // 
            // button2
            // 
            this.button2.Location = new System.Drawing.Point(163, 458);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(137, 27);
            this.button2.TabIndex = 10;
            this.button2.Text = "Error Report";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(12, 335);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(124, 27);
            this.button1.TabIndex = 11;
            this.button1.Text = "Save as csv...";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.HighlightText;
            this.ClientSize = new System.Drawing.Size(800, 497);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.m_lvData);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.m_bnOK);
            this.Name = "Form1";
            this.Text = "Form1";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        protected System.Windows.Forms.Button m_bnOK;
        private Label label1;
        private Label label2;
        protected ListView m_lvData;
        protected ColumnHeader m_lvCol_label;
        protected ColumnHeader m_lvCol_value;
        private Label label3;
        private Label label4;
        private Button button2;
        private Button button1;
        private Document maindoc;
        private IAnalysis my_analysis;
    }
}