namespace PD2ModelParser.UI
{
    partial class ImportPanel
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.label4 = new System.Windows.Forms.Label();
            this.browseBttn = new System.Windows.Forms.Button();
            this.rootPoint_combobox = new System.Windows.Forms.ComboBox();
            this.inputFileBox = new System.Windows.Forms.TextBox();
            this.exportBttn = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(5, 39);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(59, 13);
            this.label4.TabIndex = 15;
            this.label4.Text = "Root point:";
            // 
            // browseBttn
            // 
            this.browseBttn.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.browseBttn.Location = new System.Drawing.Point(348, 7);
            this.browseBttn.Name = "browseBttn";
            this.browseBttn.Size = new System.Drawing.Size(75, 23);
            this.browseBttn.TabIndex = 13;
            this.browseBttn.Text = "Browse...";
            this.browseBttn.UseVisualStyleBackColor = true;
            this.browseBttn.Click += new System.EventHandler(this.browseBttn_Click);
            // 
            // rootPoint_combobox
            // 
            this.rootPoint_combobox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.rootPoint_combobox.FormattingEnabled = true;
            this.rootPoint_combobox.Location = new System.Drawing.Point(70, 36);
            this.rootPoint_combobox.Name = "rootPoint_combobox";
            this.rootPoint_combobox.Size = new System.Drawing.Size(353, 21);
            this.rootPoint_combobox.TabIndex = 16;
            this.rootPoint_combobox.SelectedIndexChanged += new System.EventHandler(this.rootPoint_combobox_SelectedIndexChanged);
            // 
            // inputFileBox
            // 
            this.inputFileBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.inputFileBox.Location = new System.Drawing.Point(70, 9);
            this.inputFileBox.Name = "inputFileBox";
            this.inputFileBox.Size = new System.Drawing.Size(272, 20);
            this.inputFileBox.TabIndex = 12;
            // 
            // exportBttn
            // 
            this.exportBttn.Enabled = false;
            this.exportBttn.Location = new System.Drawing.Point(6, 63);
            this.exportBttn.Name = "exportBttn";
            this.exportBttn.Size = new System.Drawing.Size(274, 23);
            this.exportBttn.TabIndex = 17;
            this.exportBttn.Text = "Convert";
            this.exportBttn.UseVisualStyleBackColor = true;
            this.exportBttn.Click += new System.EventHandler(this.exportBttn_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(3, 12);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(53, 13);
            this.label1.TabIndex = 14;
            this.label1.Text = "Input File:";
            // 
            // ImportPanel
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.label1);
            this.Controls.Add(this.exportBttn);
            this.Controls.Add(this.inputFileBox);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.rootPoint_combobox);
            this.Controls.Add(this.browseBttn);
            this.Name = "ImportPanel";
            this.Size = new System.Drawing.Size(426, 189);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Button browseBttn;
        private System.Windows.Forms.ComboBox rootPoint_combobox;
        private System.Windows.Forms.TextBox inputFileBox;
        private System.Windows.Forms.Button exportBttn;
        private System.Windows.Forms.Label label1;
    }
}
