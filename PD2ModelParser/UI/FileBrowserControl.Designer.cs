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
            inputFileBox = new System.Windows.Forms.TextBox();
            browseBttn = new System.Windows.Forms.Button();
            clearBttn = new System.Windows.Forms.Button();
            SuspendLayout();
            // 
            // inputFileBox
            // 
            inputFileBox.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            inputFileBox.Enabled = false;
            inputFileBox.Location = new System.Drawing.Point(0, 2);
            inputFileBox.Margin = new System.Windows.Forms.Padding(0);
            inputFileBox.Name = "inputFileBox";
            inputFileBox.Size = new System.Drawing.Size(251, 23);
            inputFileBox.TabIndex = 14;
            // 
            // browseBttn
            // 
            browseBttn.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            browseBttn.Location = new System.Drawing.Point(251, 0);
            browseBttn.Margin = new System.Windows.Forms.Padding(0);
            browseBttn.Name = "browseBttn";
            browseBttn.Size = new System.Drawing.Size(88, 27);
            browseBttn.TabIndex = 15;
            browseBttn.Text = "Browse...";
            browseBttn.UseVisualStyleBackColor = true;
            browseBttn.Click += browseBttn_Click;
            // 
            // clearBttn
            // 
            clearBttn.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            clearBttn.Location = new System.Drawing.Point(340, 0);
            clearBttn.Margin = new System.Windows.Forms.Padding(0);
            clearBttn.Name = "clearBttn";
            clearBttn.Size = new System.Drawing.Size(44, 27);
            clearBttn.TabIndex = 15;
            clearBttn.Text = "Clear";
            clearBttn.UseVisualStyleBackColor = true;
            clearBttn.Click += ClearFileSelected;
            // 
            // FileBrowserControl
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            Controls.Add(inputFileBox);
            Controls.Add(browseBttn);
            Controls.Add(clearBttn);
            Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            Name = "FileBrowserControl";
            Size = new System.Drawing.Size(384, 27);
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private System.Windows.Forms.TextBox inputFileBox;
        private System.Windows.Forms.Button browseBttn;
        private System.Windows.Forms.Button clearBttn;

    }
}
