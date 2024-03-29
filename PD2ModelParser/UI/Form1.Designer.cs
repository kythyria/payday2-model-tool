﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace PD2ModelParser
{
    partial class Form1
    {

        private Label label3;

        private FolderBrowserDialog folderBrowserDialog1;

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
            this.label3 = new System.Windows.Forms.Label();
            this.folderBrowserDialog1 = new System.Windows.Forms.FolderBrowserDialog();
            this.objectsTab = new System.Windows.Forms.TabPage();
            this.objectsPanel1 = new PD2ModelParser.UI.ObjectsPanel();
            this.exportTab = new System.Windows.Forms.TabPage();
            this.exportPanel1 = new PD2ModelParser.UI.ExportPanel();
            this.importTab = new System.Windows.Forms.TabPage();
            this.importPanel = new PD2ModelParser.UI.ImportPanel();
            this.mainTabs = new System.Windows.Forms.TabControl();
            this.objectsTab.SuspendLayout();
            this.exportTab.SuspendLayout();
            this.importTab.SuspendLayout();
            this.mainTabs.SuspendLayout();
            this.SuspendLayout();
            // 
            // label3
            // 
            this.label3.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(458, 366);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(222, 13);
            this.label3.TabIndex = 6;
            this.label3.Text = "Credit to ZNixian, PoueT and I am not a spy...";
            // 
            // objectsTab
            // 
            this.objectsTab.Controls.Add(this.objectsPanel1);
            this.objectsTab.Location = new System.Drawing.Point(4, 22);
            this.objectsTab.Name = "objectsTab";
            this.objectsTab.Size = new System.Drawing.Size(664, 303);
            this.objectsTab.TabIndex = 3;
            this.objectsTab.Text = "Objects";
            this.objectsTab.UseVisualStyleBackColor = true;
            // 
            // objectsPanel1
            // 
            this.objectsPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.objectsPanel1.Location = new System.Drawing.Point(0, 0);
            this.objectsPanel1.Name = "objectsPanel1";
            this.objectsPanel1.Size = new System.Drawing.Size(664, 303);
            this.objectsPanel1.TabIndex = 0;
            // 
            // exportTab
            // 
            this.exportTab.Controls.Add(this.exportPanel1);
            this.exportTab.Location = new System.Drawing.Point(4, 22);
            this.exportTab.Name = "exportTab";
            this.exportTab.Padding = new System.Windows.Forms.Padding(3);
            this.exportTab.Size = new System.Drawing.Size(664, 303);
            this.exportTab.TabIndex = 1;
            this.exportTab.Text = "Export";
            this.exportTab.UseVisualStyleBackColor = true;
            // 
            // exportPanel1
            // 
            this.exportPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.exportPanel1.Location = new System.Drawing.Point(3, 3);
            this.exportPanel1.MinimumSize = new System.Drawing.Size(0, 141);
            this.exportPanel1.Name = "exportPanel1";
            this.exportPanel1.Size = new System.Drawing.Size(658, 297);
            this.exportPanel1.TabIndex = 14;
            // 
            // importTab
            // 
            this.importTab.Controls.Add(this.importPanel);
            this.importTab.Location = new System.Drawing.Point(4, 22);
            this.importTab.Name = "importTab";
            this.importTab.Padding = new System.Windows.Forms.Padding(3);
            this.importTab.Size = new System.Drawing.Size(664, 303);
            this.importTab.TabIndex = 0;
            this.importTab.Text = "Import";
            this.importTab.UseVisualStyleBackColor = true;
            // 
            // importPanel
            // 
            this.importPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.importPanel.Location = new System.Drawing.Point(3, 3);
            this.importPanel.MinimumSize = new System.Drawing.Size(0, 549);
            this.importPanel.Name = "importPanel";
            this.importPanel.Size = new System.Drawing.Size(658, 549);
            this.importPanel.TabIndex = 0;
            // 
            // mainTabs
            // 
            this.mainTabs.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.mainTabs.Controls.Add(this.importTab);
            this.mainTabs.Controls.Add(this.exportTab);
            this.mainTabs.Controls.Add(this.objectsTab);
            this.mainTabs.Location = new System.Drawing.Point(12, 12);
            this.mainTabs.Name = "mainTabs";
            this.mainTabs.SelectedIndex = 0;
            this.mainTabs.Size = new System.Drawing.Size(672, 351);
            this.mainTabs.TabIndex = 14;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(696, 388);
            this.MinimumSize = this.SizeFromClientSize(this.ClientSize);
            this.Controls.Add(this.mainTabs);
            this.Controls.Add(this.label3);
            this.Name = "Form1";
            this.ShowIcon = false;
            this.Text = "Diesel Model Tool v1.03";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.objectsTab.ResumeLayout(false);
            this.exportTab.ResumeLayout(false);
            this.importTab.ResumeLayout(false);
            this.mainTabs.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private TabPage objectsTab;
        private UI.ObjectsPanel objectsPanel1;
        private TabPage exportTab;
        private UI.ExportPanel exportPanel1;
        private TabPage importTab;
        private UI.ImportPanel importPanel;
        private TabControl mainTabs;
    }
}