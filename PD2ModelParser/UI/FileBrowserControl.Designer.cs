namespace PD2ModelParser.UI
{
    partial class FileBrowserControl
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
            this.inputFileBox = new System.Windows.Forms.TextBox();
            this.browseBttn = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // inputFileBox
            // 
            this.inputFileBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.inputFileBox.Enabled = false;
            this.inputFileBox.Location = new System.Drawing.Point(3, 3);
            this.inputFileBox.Name = "inputFileBox";
            this.inputFileBox.Size = new System.Drawing.Size(191, 20);
            this.inputFileBox.TabIndex = 14;
            // 
            // browseBttn
            // 
            this.browseBttn.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.browseBttn.Location = new System.Drawing.Point(200, 1);
            this.browseBttn.Name = "browseBttn";
            this.browseBttn.Size = new System.Drawing.Size(75, 23);
            this.browseBttn.TabIndex = 15;
            this.browseBttn.Text = "Browse...";
            this.browseBttn.UseVisualStyleBackColor = true;
            this.browseBttn.Click += new System.EventHandler(this.browseBttn_Click);
            // 
            // FileBrowserControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.inputFileBox);
            this.Controls.Add(this.browseBttn);
            this.Name = "FileBrowserControl";
            this.Size = new System.Drawing.Size(278, 27);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox inputFileBox;
        private System.Windows.Forms.Button browseBttn;
    }
}
