namespace PD2ModelParser.UI
{
    partial class ObjectsPanel
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
            System.Windows.Forms.Label lblModel;
            System.Windows.Forms.Label lblScript;
            this.showScriptChanges = new System.Windows.Forms.CheckBox();
            this.btnReload = new System.Windows.Forms.Button();
            this.treeView = new System.Windows.Forms.TreeView();
            this.propertyGrid1 = new System.Windows.Forms.PropertyGrid();
            this.scriptFile = new PD2ModelParser.UI.FileBrowserControl();
            this.modelFile = new PD2ModelParser.UI.FileBrowserControl();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            lblModel = new System.Windows.Forms.Label();
            lblScript = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.SuspendLayout();
            // 
            // lblModel
            // 
            lblModel.AutoSize = true;
            lblModel.Location = new System.Drawing.Point(23, 8);
            lblModel.Name = "lblModel";
            lblModel.Size = new System.Drawing.Size(69, 13);
            lblModel.TabIndex = 2;
            lblModel.Text = "Select Model";
            // 
            // lblScript
            // 
            lblScript.AutoSize = true;
            lblScript.Location = new System.Drawing.Point(25, 37);
            lblScript.Name = "lblScript";
            lblScript.Size = new System.Drawing.Size(67, 13);
            lblScript.TabIndex = 3;
            lblScript.Text = "Select Script";
            // 
            // showScriptChanges
            // 
            this.showScriptChanges.AutoSize = true;
            this.showScriptChanges.Checked = true;
            this.showScriptChanges.CheckState = System.Windows.Forms.CheckState.Checked;
            this.showScriptChanges.Location = new System.Drawing.Point(98, 61);
            this.showScriptChanges.Name = "showScriptChanges";
            this.showScriptChanges.Size = new System.Drawing.Size(128, 17);
            this.showScriptChanges.TabIndex = 4;
            this.showScriptChanges.Text = "Show Script Changes";
            this.showScriptChanges.UseVisualStyleBackColor = true;
            this.showScriptChanges.CheckedChanged += new System.EventHandler(this.showScriptChanges_CheckedChanged);
            // 
            // btnReload
            // 
            this.btnReload.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnReload.Location = new System.Drawing.Point(508, 61);
            this.btnReload.Name = "btnReload";
            this.btnReload.Size = new System.Drawing.Size(108, 23);
            this.btnReload.TabIndex = 5;
            this.btnReload.Text = "Reload";
            this.btnReload.UseVisualStyleBackColor = true;
            this.btnReload.Click += new System.EventHandler(this.btnReload_Click);
            // 
            // treeView
            // 
            this.treeView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.treeView.Location = new System.Drawing.Point(0, 0);
            this.treeView.Name = "treeView";
            this.treeView.Size = new System.Drawing.Size(434, 163);
            this.treeView.TabIndex = 6;
            this.treeView.NodeMouseClick += new System.Windows.Forms.TreeNodeMouseClickEventHandler(this.treeView_NodeMouseClick);
            // 
            // propertyGrid1
            // 
            this.propertyGrid1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.propertyGrid1.Location = new System.Drawing.Point(0, 0);
            this.propertyGrid1.Name = "propertyGrid1";
            this.propertyGrid1.SelectedObject = this;
            this.propertyGrid1.Size = new System.Drawing.Size(175, 163);
            this.propertyGrid1.TabIndex = 7;
            // 
            // scriptFile
            // 
            this.scriptFile.AllowDrop = true;
            this.scriptFile.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.scriptFile.Filter = "Model Script (*.mscript)|*.mscript";
            this.scriptFile.Location = new System.Drawing.Point(98, 32);
            this.scriptFile.Name = "scriptFile";
            this.scriptFile.SaveMode = false;
            this.scriptFile.Size = new System.Drawing.Size(518, 23);
            this.scriptFile.TabIndex = 1;
            this.scriptFile.FileSelected += new System.EventHandler(this.fileBrowserControl2_FileSelected);
            // 
            // modelFile
            // 
            this.modelFile.AllowDrop = true;
            this.modelFile.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.modelFile.Filter = "Diesel Model (*.model)|*.model";
            this.modelFile.Location = new System.Drawing.Point(98, 3);
            this.modelFile.Name = "modelFile";
            this.modelFile.SaveMode = false;
            this.modelFile.Size = new System.Drawing.Size(518, 23);
            this.modelFile.TabIndex = 0;
            this.modelFile.FileSelected += new System.EventHandler(this.fileBrowserControl1_FileSelected);
            // 
            // splitContainer1
            // 
            this.splitContainer1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.splitContainer1.Location = new System.Drawing.Point(3, 90);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.treeView);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.propertyGrid1);
            this.splitContainer1.Size = new System.Drawing.Size(613, 163);
            this.splitContainer1.SplitterDistance = 434;
            this.splitContainer1.TabIndex = 8;
            // 
            // ObjectsPanel
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.splitContainer1);
            this.Controls.Add(this.btnReload);
            this.Controls.Add(this.showScriptChanges);
            this.Controls.Add(lblScript);
            this.Controls.Add(lblModel);
            this.Controls.Add(this.scriptFile);
            this.Controls.Add(this.modelFile);
            this.Name = "ObjectsPanel";
            this.Size = new System.Drawing.Size(619, 256);
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private FileBrowserControl modelFile;
        private FileBrowserControl scriptFile;
        private System.Windows.Forms.CheckBox showScriptChanges;
        private System.Windows.Forms.Button btnReload;
        private System.Windows.Forms.TreeView treeView;
        private System.Windows.Forms.PropertyGrid propertyGrid1;
        private System.Windows.Forms.SplitContainer splitContainer1;
    }
}
