using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace PD2ModelParser
{
    partial class Form1
    {

        private Label label2;

        private TextBox textBox2;

        private Label label3;

        private FolderBrowserDialog folderBrowserDialog1;

        private Button exportBttn;

        private Button button3;

        private Button button4;

        private CheckBox addNewObjects_checkbox;

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
            this.label2 = new System.Windows.Forms.Label();
            this.textBox2 = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.folderBrowserDialog1 = new System.Windows.Forms.FolderBrowserDialog();
            this.exportBttn = new System.Windows.Forms.Button();
            this.button3 = new System.Windows.Forms.Button();
            this.button4 = new System.Windows.Forms.Button();
            this.addNewObjects_checkbox = new System.Windows.Forms.CheckBox();
            this.importPanel1 = new PD2ModelParser.UI.ImportPanel();
            this.SuspendLayout();
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 35);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(61, 13);
            this.label2.TabIndex = 4;
            this.label2.Text = "Output File:";
            this.label2.Click += new System.EventHandler(this.label2_Click);
            // 
            // textBox2
            // 
            this.textBox2.Location = new System.Drawing.Point(79, 32);
            this.textBox2.Name = "textBox2";
            this.textBox2.Size = new System.Drawing.Size(129, 20);
            this.textBox2.TabIndex = 3;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(350, 124);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(222, 13);
            this.label3.TabIndex = 6;
            this.label3.Text = "Credit to ZNixian, PoueT and I am not a spy...";
            // 
            // exportBttn
            // 
            this.exportBttn.Location = new System.Drawing.Point(214, 30);
            this.exportBttn.Name = "exportBttn";
            this.exportBttn.Size = new System.Drawing.Size(75, 23);
            this.exportBttn.TabIndex = 7;
            this.exportBttn.Text = "Export";
            this.exportBttn.UseVisualStyleBackColor = true;
            this.exportBttn.Click += new System.EventHandler(this.button2_Click);
            // 
            // button3
            // 
            this.button3.Location = new System.Drawing.Point(159, 59);
            this.button3.Name = "button3";
            this.button3.Size = new System.Drawing.Size(130, 23);
            this.button3.TabIndex = 8;
            this.button3.Text = "Import Model (.obj)";
            this.button3.UseVisualStyleBackColor = true;
            this.button3.Click += new System.EventHandler(this.button3_Click);
            // 
            // button4
            // 
            this.button4.Location = new System.Drawing.Point(159, 88);
            this.button4.Name = "button4";
            this.button4.Size = new System.Drawing.Size(130, 23);
            this.button4.TabIndex = 9;
            this.button4.Text = "Import Model (2nd UV)";
            this.button4.UseVisualStyleBackColor = true;
            this.button4.Click += new System.EventHandler(this.button4_Click);
            // 
            // addNewObjects_checkbox
            // 
            this.addNewObjects_checkbox.AutoSize = true;
            this.addNewObjects_checkbox.Location = new System.Drawing.Point(15, 63);
            this.addNewObjects_checkbox.Name = "addNewObjects_checkbox";
            this.addNewObjects_checkbox.Size = new System.Drawing.Size(105, 17);
            this.addNewObjects_checkbox.TabIndex = 12;
            this.addNewObjects_checkbox.Text = "Add new objects";
            this.addNewObjects_checkbox.UseVisualStyleBackColor = true;
            // 
            // importPanel1
            // 
            this.importPanel1.Location = new System.Drawing.Point(353, 12);
            this.importPanel1.Name = "importPanel1";
            this.importPanel1.Size = new System.Drawing.Size(302, 109);
            this.importPanel1.TabIndex = 13;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(667, 172);
            this.Controls.Add(this.importPanel1);
            this.Controls.Add(this.addNewObjects_checkbox);
            this.Controls.Add(this.button4);
            this.Controls.Add(this.button3);
            this.Controls.Add(this.exportBttn);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.textBox2);
            this.Name = "Form1";
            this.ShowIcon = false;
            this.Text = "Diesel Model Tool v1.03";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private UI.ImportPanel importPanel1;
    }
}