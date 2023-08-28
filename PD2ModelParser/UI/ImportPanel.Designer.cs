namespace PD2ModelParser.UI {
	partial class ImportPanel {
		/// <summary> 
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary> 
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing) {
			if (disposing && (components != null)) {
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent() {
			components = new System.ComponentModel.Container();
			lblScript = new System.Windows.Forms.Label();
			labelSelBaseModel = new System.Windows.Forms.Label();
			createNewModel = new System.Windows.Forms.CheckBox();
			labelSaveTo = new System.Windows.Forms.Label();
			labelObj = new System.Windows.Forms.Label();
			labelPatternUV = new System.Windows.Forms.Label();
			labelAnimations = new System.Windows.Forms.Label();
			convert = new System.Windows.Forms.Button();
			createNewObjectsBox = new System.Windows.Forms.CheckBox();
			rootPoints = new System.Windows.Forms.ComboBox();
			labelRootPoint = new System.Windows.Forms.Label();
			labelRootPointHint = new System.Windows.Forms.Label();
			scriptFile = new FileBrowserControl();
			patternUVFile = new FileBrowserControl();
			animationFiles = new FileBrowserControl();
			objectFile = new FileBrowserControl();
			outputBox = new FileBrowserControl();
			baseModelFileBrowser = new FileBrowserControl();
			importTransformsBox = new System.Windows.Forms.CheckBox();
			SuspendLayout();
			// 
			// lblScript
			// 
			lblScript.AutoSize = true;
			lblScript.Location = new System.Drawing.Point(76, 69);
			lblScript.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			lblScript.Name = "lblScript";
			lblScript.Size = new System.Drawing.Size(40, 15);
			lblScript.TabIndex = 15;
			lblScript.Text = "Script:";
			lblScript.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// labelSelBaseModel
			// 
			labelSelBaseModel.AutoSize = true;
			labelSelBaseModel.Location = new System.Drawing.Point(4, 9);
			labelSelBaseModel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			labelSelBaseModel.Name = "labelSelBaseModel";
			labelSelBaseModel.Size = new System.Drawing.Size(105, 15);
			labelSelBaseModel.TabIndex = 1;
			labelSelBaseModel.Text = "Select Base Model:";
			labelSelBaseModel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// createNewModel
			// 
			createNewModel.AutoSize = true;
			createNewModel.Location = new System.Drawing.Point(126, 37);
			createNewModel.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
			createNewModel.Name = "createNewModel";
			createNewModel.Size = new System.Drawing.Size(145, 19);
			createNewModel.TabIndex = 2;
			createNewModel.Text = "Or create a new model";
			createNewModel.UseVisualStyleBackColor = true;
			createNewModel.CheckedChanged += createNewModel_CheckedChanged;
			// 
			// labelSaveTo
			// 
			labelSaveTo.AutoSize = true;
			labelSaveTo.Location = new System.Drawing.Point(4, 317);
			labelSaveTo.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			labelSaveTo.Name = "labelSaveTo";
			labelSaveTo.Size = new System.Drawing.Size(49, 15);
			labelSaveTo.TabIndex = 3;
			labelSaveTo.Text = "Save To:";
			labelSaveTo.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// labelObj
			// 
			labelObj.AutoSize = true;
			labelObj.Location = new System.Drawing.Point(71, 103);
			labelObj.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			labelObj.Name = "labelObj";
			labelObj.Size = new System.Drawing.Size(45, 15);
			labelObj.TabIndex = 7;
			labelObj.Text = "Object:";
			labelObj.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// labelPatternUV
			// 
			labelPatternUV.AutoSize = true;
			labelPatternUV.Location = new System.Drawing.Point(49, 136);
			labelPatternUV.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			labelPatternUV.Name = "labelPatternUV";
			labelPatternUV.Size = new System.Drawing.Size(66, 15);
			labelPatternUV.TabIndex = 8;
			labelPatternUV.Text = "Pattern UV:";
			labelPatternUV.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// labelAnimations
			// 
			labelAnimations.AutoSize = true;
			labelAnimations.Location = new System.Drawing.Point(49, 170);
			labelAnimations.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			labelAnimations.Name = "labelAnimations";
			labelAnimations.Size = new System.Drawing.Size(71, 15);
			labelAnimations.TabIndex = 9;
			labelAnimations.Text = "Animations:";
			labelAnimations.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// convert
			// 
			convert.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
			convert.Location = new System.Drawing.Point(7, 345);
			convert.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
			convert.Name = "convert";
			convert.Size = new System.Drawing.Size(705, 27);
			convert.TabIndex = 9;
			convert.Text = "Convert";
			convert.UseVisualStyleBackColor = true;
			convert.Click += convert_Click;
			// 
			// createNewObjectsBox
			// 
			createNewObjectsBox.AutoSize = true;
			createNewObjectsBox.Location = new System.Drawing.Point(126, 196);
			createNewObjectsBox.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
			createNewObjectsBox.Name = "createNewObjectsBox";
			createNewObjectsBox.Size = new System.Drawing.Size(183, 19);
			createNewObjectsBox.TabIndex = 10;
			createNewObjectsBox.Text = "Import objects not in base file";
			createNewObjectsBox.UseVisualStyleBackColor = true;
			// 
			// rootPoints
			// 
			rootPoints.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
			rootPoints.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			rootPoints.FormattingEnabled = true;
			rootPoints.Location = new System.Drawing.Point(126, 246);
			rootPoints.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
			rootPoints.Name = "rootPoints";
			rootPoints.Size = new System.Drawing.Size(585, 23);
			rootPoints.TabIndex = 11;
			// 
			// labelRootPoint
			// 
			labelRootPoint.AutoSize = true;
			labelRootPoint.Location = new System.Drawing.Point(51, 249);
			labelRootPoint.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			labelRootPoint.Name = "labelRootPoint";
			labelRootPoint.Size = new System.Drawing.Size(66, 15);
			labelRootPoint.TabIndex = 12;
			labelRootPoint.Text = "Root Point:";
			labelRootPoint.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// labelRootPointHint
			// 
			labelRootPointHint.AutoSize = true;
			labelRootPointHint.Location = new System.Drawing.Point(51, 273);
			labelRootPointHint.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			labelRootPointHint.Name = "labelRootPointHint";
			labelRootPointHint.Size = new System.Drawing.Size(261, 15);
			labelRootPointHint.TabIndex = 13;
			labelRootPointHint.Text = "(which bone new objects should be attached to)";
			labelRootPointHint.AutoEllipsis = true;
			// 
			// scriptFile
			// 
			scriptFile.AllowDrop = true;
			scriptFile.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
			scriptFile.Filter = "Model Script (*.mscript)|*.mscript";
			scriptFile.Location = new System.Drawing.Point(126, 63);
			scriptFile.Margin = new System.Windows.Forms.Padding(5, 3, 5, 3);
			scriptFile.MinimumSize = new System.Drawing.Size(0, 27);
			scriptFile.MultiFile = false;
			scriptFile.Name = "scriptFile";
			scriptFile.SaveMode = false;
			scriptFile.Selected = null;
			scriptFile.Size = new System.Drawing.Size(586, 27);
			scriptFile.TabIndex = 14;
			scriptFile.FileSelected += scriptFile_FileSelected;
			// 
			// patternUVFile
			// 
			patternUVFile.AllowDrop = true;
			patternUVFile.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
			patternUVFile.Filter = "Object File (*.obj)|*.obj";
			patternUVFile.Location = new System.Drawing.Point(126, 130);
			patternUVFile.Margin = new System.Windows.Forms.Padding(5, 3, 5, 3);
			patternUVFile.MinimumSize = new System.Drawing.Size(0, 27);
			patternUVFile.MultiFile = false;
			patternUVFile.Name = "patternUVFile";
			patternUVFile.SaveMode = false;
			patternUVFile.Selected = null;
			patternUVFile.Size = new System.Drawing.Size(586, 27);
			patternUVFile.TabIndex = 6;
			// 
			// animationFiles
			// 
			animationFiles.AllowDrop = true;
			animationFiles.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
			animationFiles.Filter = "Animation Files (*.animation)|*.animation";
			animationFiles.Location = new System.Drawing.Point(126, 164);
			animationFiles.Margin = new System.Windows.Forms.Padding(5, 3, 5, 3);
			animationFiles.MinimumSize = new System.Drawing.Size(0, 27);
			animationFiles.MultiFile = true;
			animationFiles.Name = "animationFiles";
			animationFiles.SaveMode = false;
			animationFiles.Selected = null;
			animationFiles.Size = new System.Drawing.Size(586, 27);
			animationFiles.TabIndex = 10;
			// 
			// objectFile
			// 
			objectFile.AllowDrop = true;
			objectFile.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
			objectFile.Filter = "Model files (*.gltf, *.glb, *.obj)|*.gltf;*.glb;*.obj;|glTF files (*.gltf, *.glb)|*.gltf;*.glb|Wavefront OBJ files (*.obj)|*.obj";
			objectFile.Location = new System.Drawing.Point(126, 97);
			objectFile.Margin = new System.Windows.Forms.Padding(5, 3, 5, 3);
			objectFile.MinimumSize = new System.Drawing.Size(0, 27);
			objectFile.MultiFile = false;
			objectFile.Name = "objectFile";
			objectFile.SaveMode = false;
			objectFile.Selected = null;
			objectFile.Size = new System.Drawing.Size(586, 27);
			objectFile.TabIndex = 5;
			// 
			// outputBox
			// 
			outputBox.AllowDrop = true;
			outputBox.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
			outputBox.Filter = "Diesel Model (*.model)|*.model";
			outputBox.Location = new System.Drawing.Point(126, 312);
			outputBox.Margin = new System.Windows.Forms.Padding(5, 3, 5, 3);
			outputBox.MinimumSize = new System.Drawing.Size(0, 27);
			outputBox.MultiFile = false;
			outputBox.Name = "outputBox";
			outputBox.SaveMode = true;
			outputBox.Selected = null;
			outputBox.Size = new System.Drawing.Size(586, 27);
			outputBox.TabIndex = 4;
			// 
			// baseModelFileBrowser
			// 
			baseModelFileBrowser.AllowDrop = true;
			baseModelFileBrowser.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
			baseModelFileBrowser.Filter = "Diesel Model (*.model)|*.model";
			baseModelFileBrowser.Location = new System.Drawing.Point(126, 3);
			baseModelFileBrowser.Margin = new System.Windows.Forms.Padding(5, 3, 5, 3);
			baseModelFileBrowser.MinimumSize = new System.Drawing.Size(0, 27);
			baseModelFileBrowser.MultiFile = false;
			baseModelFileBrowser.Name = "baseModelFileBrowser";
			baseModelFileBrowser.SaveMode = false;
			baseModelFileBrowser.Selected = null;
			baseModelFileBrowser.Size = new System.Drawing.Size(586, 27);
			baseModelFileBrowser.TabIndex = 0;
			baseModelFileBrowser.FileSelected += baseModelFileBrowser_FileSelected;
			// 
			// importTransformsBox
			// 
			importTransformsBox.AutoSize = true;
			importTransformsBox.Checked = true;
			importTransformsBox.CheckState = System.Windows.Forms.CheckState.Checked;
			importTransformsBox.Location = new System.Drawing.Point(126, 221);
			importTransformsBox.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
			importTransformsBox.Name = "importTransformsBox";
			importTransformsBox.Size = new System.Drawing.Size(122, 19);
			importTransformsBox.TabIndex = 16;
			importTransformsBox.Text = "Import transforms";
			importTransformsBox.UseVisualStyleBackColor = true;
			// 
			// ImportPanel
			// 
			AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
			AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowOnly;
			Controls.Add(importTransformsBox);
			Controls.Add(lblScript);
			Controls.Add(scriptFile);
			Controls.Add(labelRootPointHint);
			Controls.Add(labelRootPoint);
			Controls.Add(rootPoints);
			Controls.Add(createNewObjectsBox);
			Controls.Add(convert);
			Controls.Add(labelPatternUV);
			Controls.Add(labelAnimations);
			Controls.Add(labelObj);
			Controls.Add(patternUVFile);
			Controls.Add(animationFiles);
			Controls.Add(objectFile);
			Controls.Add(outputBox);
			Controls.Add(labelSaveTo);
			Controls.Add(createNewModel);
			Controls.Add(labelSelBaseModel);
			Controls.Add(baseModelFileBrowser);
			Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
			Name = "ImportPanel";
			Size = new System.Drawing.Size(715, 375);
			Load += ImportPanel_Load;
			ResumeLayout(false);
			PerformLayout();
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
		private System.Windows.Forms.CheckBox importTransformsBox;
	}
}
