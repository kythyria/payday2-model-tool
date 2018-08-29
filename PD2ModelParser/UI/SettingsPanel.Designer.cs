namespace PD2ModelParser.UI
{
    partial class SettingsPanel
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
            this.enableAutomaticUpdates = new System.Windows.Forms.CheckBox();
            this.checkForUpdates = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // enableAutomaticUpdates
            // 
            this.enableAutomaticUpdates.AutoSize = true;
            this.enableAutomaticUpdates.Location = new System.Drawing.Point(3, 3);
            this.enableAutomaticUpdates.Name = "enableAutomaticUpdates";
            this.enableAutomaticUpdates.Size = new System.Drawing.Size(152, 17);
            this.enableAutomaticUpdates.TabIndex = 0;
            this.enableAutomaticUpdates.Text = "Enable Automatic Updates";
            this.enableAutomaticUpdates.UseVisualStyleBackColor = true;
            this.enableAutomaticUpdates.CheckedChanged += new System.EventHandler(this.enableAutomaticUpdates_CheckedChanged);
            // 
            // checkForUpdates
            // 
            this.checkForUpdates.Location = new System.Drawing.Point(3, 26);
            this.checkForUpdates.Name = "checkForUpdates";
            this.checkForUpdates.Size = new System.Drawing.Size(152, 23);
            this.checkForUpdates.TabIndex = 1;
            this.checkForUpdates.Text = "Check for updates now";
            this.checkForUpdates.UseVisualStyleBackColor = true;
            this.checkForUpdates.Click += new System.EventHandler(this.checkForUpdates_Click);
            // 
            // SettingsPanel
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.checkForUpdates);
            this.Controls.Add(this.enableAutomaticUpdates);
            this.Name = "SettingsPanel";
            this.Size = new System.Drawing.Size(336, 179);
            this.Load += new System.EventHandler(this.SettingsPanel_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.CheckBox enableAutomaticUpdates;
        private System.Windows.Forms.Button checkForUpdates;
    }
}
