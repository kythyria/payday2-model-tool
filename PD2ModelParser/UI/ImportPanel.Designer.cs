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
			this.lblScript = new System.Windows.Forms.Label();
			this.labelSelBaseModel = new System.Windows.Forms.Label();
			this.createNewModel = new System.Windows.Forms.CheckBox();
			this.labelSaveTo = new System.Windows.Forms.Label();
			this.labelObj = new System.Windows.Forms.Label();
			this.labelPatternUV = new System.Windows.Forms.Label();
			this.labelAnimations = new System.Windows.Forms.Label();
			this.convert = new System.Windows.Forms.Button();
			this.createNewObjectsBox = new System.Windows.Forms.CheckBox();
			this.rootPoints = new System.Windows.Forms.ComboBox();
			this.labelRootPoint = new System.Windows.Forms.Label();
			this.labelRootPointHint = new System.Windows.Forms.Label();
			this.scriptFile = new PD2ModelParser.UI.FileBrowserControl();
			this.patternUVFile = new PD2ModelParser.UI.FileBrowserControl();
			this.animationFiles = new PD2ModelParser.UI.FileBrowserControl();
			this.objectFile = new PD2ModelParser.UI.FileBrowserControl();
			this.outputBox = new PD2ModelParser.UI.FileBrowserControl();
			this.baseModelFileBrowser = new PD2ModelParser.UI.FileBrowserControl();
			this.SuspendLayout();
			// 
			// lblScript
			// 
			this.lblScript.AutoSize = true;
			this.lblScript.Location = new System.Drawing.Point(65, 60);
			this.lblScript.Name = "lblScript";
			this.lblScript.Size = new System.Drawing.Size(37, 13);
			this.lblScript.TabIndex = 15;
			this.lblScript.Text = "Script:";
			this.lblScript.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// labelSelBaseModel
			// 
			this.labelSelBaseModel.AutoSize = true;
			this.labelSelBaseModel.Location = new System.Drawing.Point(3, 8);
			this.labelSelBaseModel.Name = "labelSelBaseModel";
			this.labelSelBaseModel.Size = new System.Drawing.Size(99, 13);
			this.labelSelBaseModel.TabIndex = 1;
			this.labelSelBaseModel.Text = "Select Base Model:";
			this.labelSelBaseModel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// createNewModel
			// 
			this.createNewModel.AutoSize = true;
			this.createNewModel.Location = new System.Drawing.Point(108, 32);
			this.createNewModel.Name = "createNewModel";
			this.createNewModel.Size = new System.Drawing.Size(133, 17);
			this.createNewModel.TabIndex = 2;
			this.createNewModel.Text = "Or create a new model";
			this.createNewModel.UseVisualStyleBackColor = true;
			this.createNewModel.CheckedChanged += new System.EventHandler(this.createNewModel_CheckedChanged);
			// 
			// labelSaveTo
			// 
			this.labelSaveTo.AutoSize = true;
			this.labelSaveTo.Location = new System.Drawing.Point(3, 275);
			this.labelSaveTo.Name = "labelSaveTo";
			this.labelSaveTo.Size = new System.Drawing.Size(51, 13);
			this.labelSaveTo.TabIndex = 3;
			this.labelSaveTo.Text = "Save To:";
			this.labelSaveTo.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// labelObj
			// 
			this.labelObj.AutoSize = true;
			this.labelObj.Location = new System.Drawing.Point(61, 89);
			this.labelObj.Name = "labelObj";
			this.labelObj.Size = new System.Drawing.Size(41, 13);
			this.labelObj.TabIndex = 7;
			this.labelObj.Text = "Object:";
			this.labelObj.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// labelPatternUV
			// 
			this.labelPatternUV.AutoSize = true;
			this.labelPatternUV.Location = new System.Drawing.Point(42, 118);
			this.labelPatternUV.Name = "labelPatternUV";
			this.labelPatternUV.Size = new System.Drawing.Size(62, 13);
			this.labelPatternUV.TabIndex = 8;
			this.labelPatternUV.Text = "Pattern UV:";
			this.labelPatternUV.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// labelAnimations
			// 
			this.labelAnimations.AutoSize = true;
			this.labelAnimations.Location = new System.Drawing.Point(42, 147);
			this.labelAnimations.Name = "labelAnimations";
			this.labelAnimations.Size = new System.Drawing.Size(62, 13);
			this.labelAnimations.TabIndex = 9;
			this.labelAnimations.Text = "Animations:";
			this.labelAnimations.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// convert
			// 
			this.convert.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.convert.Location = new System.Drawing.Point(6, 299);
			this.convert.Name = "convert";
			this.convert.Size = new System.Drawing.Size(604, 23);
			this.convert.TabIndex = 9;
			this.convert.Text = "Convert";
			this.convert.UseVisualStyleBackColor = true;
			this.convert.Click += new System.EventHandler(this.convert_Click);
			// 
			// createNewObjectsBox
			// 
			this.createNewObjectsBox.AutoSize = true;
			this.createNewObjectsBox.Location = new System.Drawing.Point(108, 170);
			this.createNewObjectsBox.Name = "createNewObjectsBox";
			this.createNewObjectsBox.Size = new System.Drawing.Size(163, 17);
			this.createNewObjectsBox.TabIndex = 10;
			this.createNewObjectsBox.Text = "Import objects not in base file";
			this.createNewObjectsBox.UseVisualStyleBackColor = true;
			// 
			// rootPoints
			// 
			this.rootPoints.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.rootPoints.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.rootPoints.FormattingEnabled = true;
			this.rootPoints.Location = new System.Drawing.Point(108, 193);
			this.rootPoints.Name = "rootPoints";
			this.rootPoints.Size = new System.Drawing.Size(502, 21);
			this.rootPoints.TabIndex = 11;
			// 
			// labelRootPoint
			// 
			this.labelRootPoint.AutoSize = true;
			this.labelRootPoint.Location = new System.Drawing.Point(44, 196);
			this.labelRootPoint.Name = "labelRootPoint";
			this.labelRootPoint.Size = new System.Drawing.Size(60, 13);
			this.labelRootPoint.TabIndex = 12;
			this.labelRootPoint.Text = "Root Point:";
			this.labelRootPoint.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// labelRootPointHint
			// 
			this.labelRootPointHint.AutoSize = true;
			this.labelRootPointHint.Location = new System.Drawing.Point(44, 217);
			this.labelRootPointHint.Name = "labelRootPointHint";
			this.labelRootPointHint.Size = new System.Drawing.Size(234, 13);
			this.labelRootPointHint.TabIndex = 13;
			this.labelRootPointHint.Text = "(which bone new objects should be attached to)";
			// 
			// scriptFile
			// 
			this.scriptFile.AllowDrop = true;
			this.scriptFile.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.scriptFile.Filter = "Model Script (*.mscript)|*.mscript";
			this.scriptFile.Location = new System.Drawing.Point(108, 55);
			this.scriptFile.MinimumSize = new System.Drawing.Size(0, 23);
			this.scriptFile.MultiFile = false;
			this.scriptFile.Name = "scriptFile";
			this.scriptFile.SaveMode = false;
			this.scriptFile.Selected = null;
			this.scriptFile.Size = new System.Drawing.Size(502, 23);
			this.scriptFile.TabIndex = 14;
			this.scriptFile.FileSelected += new System.EventHandler(this.scriptFile_FileSelected);
			// 
			// patternUVFile
			// 
			this.patternUVFile.AllowDrop = true;
			this.patternUVFile.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.patternUVFile.Filter = "Object File (*.obj)|*.obj";
			this.patternUVFile.Location = new System.Drawing.Point(108, 113);
			this.patternUVFile.MinimumSize = new System.Drawing.Size(0, 23);
			this.patternUVFile.MultiFile = false;
			this.patternUVFile.Name = "patternUVFile";
			this.patternUVFile.SaveMode = false;
			this.patternUVFile.Selected = null;
			this.patternUVFile.Size = new System.Drawing.Size(502, 23);
			this.patternUVFile.TabIndex = 6;
			// 
			// animationFiles
			// 
			this.animationFiles.AllowDrop = true;
			this.animationFiles.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.animationFiles.Filter = "Animation Files (*.animation)|*.animation";
			this.animationFiles.Location = new System.Drawing.Point(108, 142);
			this.animationFiles.MinimumSize = new System.Drawing.Size(0, 23);
			this.animationFiles.MultiFile = true;
			this.animationFiles.Name = "animationFiles";
			this.animationFiles.SaveMode = false;
			this.animationFiles.Selected = null;
			this.animationFiles.Size = new System.Drawing.Size(502, 23);
			this.animationFiles.TabIndex = 10;
			// 
			// objectFile
			// 
			this.objectFile.AllowDrop = true;
			this.objectFile.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.objectFile.Filter = "Model files (*.gltf, *.glb, *.obj)|*.gltf;*.glb;*.obj;|glTF files (*.gltf, *.glb)" +
    "|*.gltf;*.glb|Wavefront OBJ files (*.obj)|*.obj";
			this.objectFile.Location = new System.Drawing.Point(108, 84);
			this.objectFile.MinimumSize = new System.Drawing.Size(0, 23);
			this.objectFile.MultiFile = false;
			this.objectFile.Name = "objectFile";
			this.objectFile.SaveMode = false;
			this.objectFile.Selected = null;
			this.objectFile.Size = new System.Drawing.Size(502, 23);
			this.objectFile.TabIndex = 5;
			// 
			// outputBox
			// 
			this.outputBox.AllowDrop = true;
			this.outputBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.outputBox.Filter = "Diesel Model (*.model)|*.model";
			this.outputBox.Location = new System.Drawing.Point(108, 270);
			this.outputBox.MinimumSize = new System.Drawing.Size(0, 23);
			this.outputBox.MultiFile = false;
			this.outputBox.Name = "outputBox";
			this.outputBox.SaveMode = true;
			this.outputBox.Selected = null;
			this.outputBox.Size = new System.Drawing.Size(502, 23);
			this.outputBox.TabIndex = 4;
			// 
			// baseModelFileBrowser
			// 
			this.baseModelFileBrowser.AllowDrop = true;
			this.baseModelFileBrowser.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.baseModelFileBrowser.Filter = "Diesel Model (*.model)|*.model";
			this.baseModelFileBrowser.Location = new System.Drawing.Point(108, 3);
			this.baseModelFileBrowser.MinimumSize = new System.Drawing.Size(0, 23);
			this.baseModelFileBrowser.MultiFile = false;
			this.baseModelFileBrowser.Name = "baseModelFileBrowser";
			this.baseModelFileBrowser.SaveMode = false;
			this.baseModelFileBrowser.Selected = null;
			this.baseModelFileBrowser.Size = new System.Drawing.Size(502, 23);
			this.baseModelFileBrowser.TabIndex = 0;
			this.baseModelFileBrowser.FileSelected += new System.EventHandler(this.baseModelFileBrowser_FileSelected);
			// 
			// ImportPanel
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this.lblScript);
			this.Controls.Add(this.scriptFile);
			this.Controls.Add(this.labelRootPointHint);
			this.Controls.Add(this.labelRootPoint);
			this.Controls.Add(this.rootPoints);
			this.Controls.Add(this.createNewObjectsBox);
			this.Controls.Add(this.convert);
			this.Controls.Add(this.labelPatternUV);
			this.Controls.Add(this.labelAnimations);
			this.Controls.Add(this.labelObj);
			this.Controls.Add(this.patternUVFile);
			this.Controls.Add(this.animationFiles);
			this.Controls.Add(this.objectFile);
			this.Controls.Add(this.outputBox);
			this.Controls.Add(this.labelSaveTo);
			this.Controls.Add(this.createNewModel);
			this.Controls.Add(this.labelSelBaseModel);
			this.Controls.Add(this.baseModelFileBrowser);
			this.Name = "ImportPanel";
			this.Size = new System.Drawing.Size(613, 325);
			this.Load += new System.EventHandler(this.ImportPanel_Load);
			this.ResumeLayout(false);
			this.PerformLayout();

        }

        #endregion

        private FileBrowserControl baseModelFileBrowser;
        private System.Windows.Forms.Label labelSelBaseModel;
        private System.Windows.Forms.CheckBox createNewModel;
        private System.Windows.Forms.Label labelSaveTo;
        private FileBrowserControl outputBox;
        private FileBrowserControl objectFile;
        private FileBrowserControl patternUVFile;
        private FileBrowserControl animationFiles;
        private System.Windows.Forms.Label labelObj;
        private System.Windows.Forms.Label labelPatternUV;
		private System.Windows.Forms.Label labelAnimations;
		private System.Windows.Forms.Button convert;
        private System.Windows.Forms.CheckBox createNewObjectsBox;
        private System.Windows.Forms.ComboBox rootPoints;
        private System.Windows.Forms.Label labelRootPoint;
        private System.Windows.Forms.Label labelRootPointHint;
        private FileBrowserControl scriptFile;
        private System.Windows.Forms.Label lblScript;
	}
}
