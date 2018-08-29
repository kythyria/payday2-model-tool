namespace PD2ModelParser.UI
{
    partial class ExportPanel
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
            this.exportBttn = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.formatBox = new System.Windows.Forms.ComboBox();
            this.inputFileBox = new PD2ModelParser.UI.FileBrowserControl();
            this.SuspendLayout();
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
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(5, 39);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(42, 13);
            this.label2.TabIndex = 18;
            this.label2.Text = "Format:";
            // 
            // formatBox
            // 
            this.formatBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.formatBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.formatBox.FormattingEnabled = true;
            this.formatBox.Items.AddRange(new object[] {
            "Object (.obj)",
            "Collada (.dae)"});
            this.formatBox.Location = new System.Drawing.Point(70, 36);
            this.formatBox.Name = "formatBox";
            this.formatBox.Size = new System.Drawing.Size(353, 21);
            this.formatBox.TabIndex = 19;
            // 
            // inputFileBox
            // 
            this.inputFileBox.AllowDrop = true;
            this.inputFileBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.inputFileBox.Filter = "Diesel Model(*.model)|*.model";
            this.inputFileBox.Location = new System.Drawing.Point(70, 7);
            this.inputFileBox.Name = "inputFileBox";
            this.inputFileBox.SaveMode = false;
            this.inputFileBox.Size = new System.Drawing.Size(353, 23);
            this.inputFileBox.TabIndex = 20;
            this.inputFileBox.FileSelected += new System.EventHandler(this.inputFileBox_FileSelected);
            // 
            // ExportPanel
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.inputFileBox);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.formatBox);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.exportBttn);
            this.Name = "ExportPanel";
            this.Size = new System.Drawing.Size(426, 189);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.Button exportBttn;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ComboBox formatBox;
        private FileBrowserControl inputFileBox;
    }
}
