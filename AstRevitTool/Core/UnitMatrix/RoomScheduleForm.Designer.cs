//
// (C) Copyright 2003-2019 by Autodesk, Inc.
//
// Permission to use, copy, modify, and distribute this software in
// object code form for any purpose and without fee is hereby granted,
// provided that the above copyright notice appears in all copies and
// that both that copyright notice and the limited warranty and
// restricted rights notice below appear in all supporting
// documentation.
//
// AUTODESK PROVIDES THIS PROGRAM "AS IS" AND WITH ALL FAULTS.
// AUTODESK SPECIFICALLY DISCLAIMS ANY IMPLIED WARRANTY OF
// MERCHANTABILITY OR FITNESS FOR A PARTICULAR USE. AUTODESK, INC.
// DOES NOT WARRANT THAT THE OPERATION OF THE PROGRAM WILL BE
// UNINTERRUPTED OR ERROR FREE.
//
// Use, duplication, or disclosure by the U.S. Government is subject to
// restrictions set forth in FAR 52.227-19 (Commercial Computer
// Software - Restricted Rights) and DFAR 252.227-7013(c)(1)(ii)
// (Rights in Technical Data and Computer Software), as applicable.
//
namespace AstRevitTool.Core.UnitMatrix
{
    /// <summary>
    /// Room Schedule form, used to retrieve data from .xls data source and create new rooms.
    /// </summary>
    partial class RoomScheduleForm
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

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.levelLabel = new System.Windows.Forms.Label();
            this.revitRoomDataGridView = new System.Windows.Forms.DataGridView();
            this.levelComboBox = new System.Windows.Forms.ComboBox();
            this.roomsGroupBox = new System.Windows.Forms.GroupBox();
            this.button2 = new System.Windows.Forms.Button();
            this.button1 = new System.Windows.Forms.Button();
            this.showAllRoomsCheckBox = new System.Windows.Forms.CheckBox();
            this.closeButton = new System.Windows.Forms.Button();
            this.button3 = new System.Windows.Forms.Button();
            this.comboBox1 = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.revitRoomDataGridView)).BeginInit();
            this.roomsGroupBox.SuspendLayout();
            this.SuspendLayout();
            // 
            // levelLabel
            // 
            this.levelLabel.AutoSize = true;
            this.levelLabel.Location = new System.Drawing.Point(65, 20);
            this.levelLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.levelLabel.Name = "levelLabel";
            this.levelLabel.Size = new System.Drawing.Size(40, 16);
            this.levelLabel.TabIndex = 2;
            this.levelLabel.Text = "Level";
            // 
            // revitRoomDataGridView
            // 
            this.revitRoomDataGridView.AllowUserToAddRows = false;
            this.revitRoomDataGridView.AllowUserToDeleteRows = false;
            this.revitRoomDataGridView.AllowUserToResizeRows = false;
            this.revitRoomDataGridView.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.revitRoomDataGridView.Location = new System.Drawing.Point(8, 52);
            this.revitRoomDataGridView.Margin = new System.Windows.Forms.Padding(4);
            this.revitRoomDataGridView.Name = "revitRoomDataGridView";
            this.revitRoomDataGridView.RowHeadersVisible = false;
            this.revitRoomDataGridView.RowHeadersWidth = 51;
            this.revitRoomDataGridView.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.revitRoomDataGridView.Size = new System.Drawing.Size(1059, 601);
            this.revitRoomDataGridView.TabIndex = 1;
            // 
            // levelComboBox
            // 
            this.levelComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.levelComboBox.FormattingEnabled = true;
            this.levelComboBox.Location = new System.Drawing.Point(113, 17);
            this.levelComboBox.Margin = new System.Windows.Forms.Padding(4);
            this.levelComboBox.Name = "levelComboBox";
            this.levelComboBox.Size = new System.Drawing.Size(158, 24);
            this.levelComboBox.Sorted = true;
            this.levelComboBox.TabIndex = 0;
            this.levelComboBox.SelectedIndexChanged += new System.EventHandler(this.levelComboBox_SelectedIndexChanged);
            // 
            // roomsGroupBox
            // 
            this.roomsGroupBox.Controls.Add(this.label1);
            this.roomsGroupBox.Controls.Add(this.comboBox1);
            this.roomsGroupBox.Controls.Add(this.button2);
            this.roomsGroupBox.Controls.Add(this.button1);
            this.roomsGroupBox.Controls.Add(this.showAllRoomsCheckBox);
            this.roomsGroupBox.Controls.Add(this.revitRoomDataGridView);
            this.roomsGroupBox.Controls.Add(this.levelComboBox);
            this.roomsGroupBox.Controls.Add(this.levelLabel);
            this.roomsGroupBox.Location = new System.Drawing.Point(16, 13);
            this.roomsGroupBox.Margin = new System.Windows.Forms.Padding(4);
            this.roomsGroupBox.Name = "roomsGroupBox";
            this.roomsGroupBox.Padding = new System.Windows.Forms.Padding(4);
            this.roomsGroupBox.Size = new System.Drawing.Size(1075, 661);
            this.roomsGroupBox.TabIndex = 1;
            this.roomsGroupBox.TabStop = false;
            this.roomsGroupBox.Text = "Revit Rooms";
            // 
            // button2
            // 
            this.button2.Location = new System.Drawing.Point(773, 14);
            this.button2.Margin = new System.Windows.Forms.Padding(4);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(132, 28);
            this.button2.TabIndex = 5;
            this.button2.Text = "Clear Checked";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(932, 14);
            this.button1.Margin = new System.Windows.Forms.Padding(4);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(132, 28);
            this.button1.TabIndex = 4;
            this.button1.Text = "Check Selection";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click_1);
            // 
            // showAllRoomsCheckBox
            // 
            this.showAllRoomsCheckBox.AutoSize = true;
            this.showAllRoomsCheckBox.Location = new System.Drawing.Point(279, 19);
            this.showAllRoomsCheckBox.Margin = new System.Windows.Forms.Padding(4);
            this.showAllRoomsCheckBox.Name = "showAllRoomsCheckBox";
            this.showAllRoomsCheckBox.Size = new System.Drawing.Size(127, 20);
            this.showAllRoomsCheckBox.TabIndex = 2;
            this.showAllRoomsCheckBox.Text = "&Show All Rooms";
            this.showAllRoomsCheckBox.UseVisualStyleBackColor = true;
            this.showAllRoomsCheckBox.CheckedChanged += new System.EventHandler(this.showAllRoomsCheckBox_CheckedChanged);
            // 
            // closeButton
            // 
            this.closeButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.closeButton.Location = new System.Drawing.Point(983, 682);
            this.closeButton.Margin = new System.Windows.Forms.Padding(4);
            this.closeButton.Name = "closeButton";
            this.closeButton.Size = new System.Drawing.Size(100, 28);
            this.closeButton.TabIndex = 2;
            this.closeButton.Text = "&Close";
            this.closeButton.UseVisualStyleBackColor = true;
            this.closeButton.Click += new System.EventHandler(this.closeButton_Click);
            // 
            // button3
            // 
            this.button3.Location = new System.Drawing.Point(850, 682);
            this.button3.Margin = new System.Windows.Forms.Padding(4);
            this.button3.Name = "button3";
            this.button3.Size = new System.Drawing.Size(98, 28);
            this.button3.TabIndex = 6;
            this.button3.Text = "&Export";
            this.button3.UseVisualStyleBackColor = true;
            this.button3.Click += new System.EventHandler(this.button3_Click);
            // 
            // comboBox1
            // 
            this.comboBox1.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBox1.FormattingEnabled = true;
            this.comboBox1.Location = new System.Drawing.Point(508, 17);
            this.comboBox1.Margin = new System.Windows.Forms.Padding(4);
            this.comboBox1.Name = "comboBox1";
            this.comboBox1.Size = new System.Drawing.Size(158, 24);
            this.comboBox1.Sorted = true;
            this.comboBox1.TabIndex = 6;
            this.comboBox1.SelectedIndexChanged += new System.EventHandler(this.comboBox1_SelectedIndexChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(414, 20);
            this.label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(86, 16);
            this.label1.TabIndex = 7;
            this.label1.Text = "AreaScheme";
            // 
            // RoomScheduleForm
            // 
            this.AcceptButton = this.closeButton;
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.closeButton;
            this.ClientSize = new System.Drawing.Size(1119, 725);
            this.Controls.Add(this.button3);
            this.Controls.Add(this.closeButton);
            this.Controls.Add(this.roomsGroupBox);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Margin = new System.Windows.Forms.Padding(4);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "RoomScheduleForm";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.Text = "Room Schedule";
            ((System.ComponentModel.ISupportInitialize)(this.revitRoomDataGridView)).EndInit();
            this.roomsGroupBox.ResumeLayout(false);
            this.roomsGroupBox.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.Label levelLabel;
        private System.Windows.Forms.DataGridView revitRoomDataGridView;
        private System.Windows.Forms.ComboBox levelComboBox;
        private System.Windows.Forms.GroupBox roomsGroupBox;
        private System.Windows.Forms.Button closeButton;
        private System.Windows.Forms.CheckBox showAllRoomsCheckBox;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.Button button3;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ComboBox comboBox1;
    }
}

