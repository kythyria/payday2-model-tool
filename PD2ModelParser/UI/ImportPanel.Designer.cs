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
            System.Windows.Forms.Label lblScript;
            this.labelSelBaseModel = new System.Windows.Forms.Label();
            this.createNewModel = new System.Windows.Forms.CheckBox();
            this.labelSaveTo = new System.Windows.Forms.Label();
            this.labelObj = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.convert = new System.Windows.Forms.Button();
            this.createNewObjectsBox = new System.Windows.Forms.CheckBox();
            this.rootPoints = new System.Windows.Forms.ComboBox();
            this.labelRootPoint = new System.Windows.Forms.Label();
            this.patternUVFile = new PD2ModelParser.UI.FileBrowserControl();
            this.objectFile = new PD2ModelParser.UI.FileBrowserControl();
            this.outputBox = new PD2ModelParser.UI.FileBrowserControl();
            this.baseModelFileBrowser = new PD2ModelParser.UI.FileBrowserControl();
            this.label2 = new System.Windows.Forms.Label();
            this.scriptFile = new PD2ModelParser.UI.FileBrowserControl();
            lblScript = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // labelSelBaseModel
            // 
            this.labelSelBaseModel.AutoSize = true;
            this.labelSelBaseModel.Location = new System.Drawing.Point(3, 9);
            this.labelSelBaseModel.Name = "labelSelBaseModel";
            this.labelSelBaseModel.Size = new System.Drawing.Size(99, 13);
            this.labelSelBaseModel.TabIndex = 1;
            this.labelSelBaseModel.Text = "Select Base Model:";
            // 
            // createNewModel
            // 
            this.createNewModel.AutoSize = true;
            this.createNewModel.Location = new System.Drawing.Point(108, 36);
            this.createNewModel.Name = "createNewModel";
            this.createNewModel.Size = new System.Drawing.Size(133, 17);
            this.createNewModel.TabIndex = 2;
            this.createNewModel.Text = "Or create a new model";
            this.createNewModel.UseVisualStyleBackColor = true;
            this.createNewModel.CheckedChanged += new System.EventHandler(this.createNewModel_CheckedChanged);
            // 
            // labelSaveTo
            // 
            this.labelSaveTo.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.labelSaveTo.AutoSize = true;
            this.labelSaveTo.Location = new System.Drawing.Point(3, 251);
            this.labelSaveTo.Name = "labelSaveTo";
            this.labelSaveTo.Size = new System.Drawing.Size(51, 13);
            this.labelSaveTo.TabIndex = 3;
            this.labelSaveTo.Text = "Save To:";
            // 
            // labelObj
            // 
            this.labelObj.AutoSize = true;
            this.labelObj.Location = new System.Drawing.Point(61, 99);
            this.labelObj.Name = "labelObj";
            this.labelObj.Size = new System.Drawing.Size(41, 13);
            this.labelObj.TabIndex = 7;
            this.labelObj.Text = "Object:";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(40, 133);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(62, 13);
            this.label1.TabIndex = 8;
            this.label1.Text = "Pattern UV:";
            // 
            // convert
            // 
            this.convert.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.convert.Location = new System.Drawing.Point(6, 277);
            this.convert.Name = "convert";
            this.convert.Size = new System.Drawing.Size(479, 23);
            this.convert.TabIndex = 9;
            this.convert.Text = "Convert";
            this.convert.UseVisualStyleBackColor = true;
            this.convert.Click += new System.EventHandler(this.convert_Click);
            // 
            // createNewObjectsBox
            // 
            this.createNewObjectsBox.AutoSize = true;
            this.createNewObjectsBox.Location = new System.Drawing.Point(108, 159);
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
            this.rootPoints.Location = new System.Drawing.Point(108, 183);
            this.rootPoints.Name = "rootPoints";
            this.rootPoints.Size = new System.Drawing.Size(377, 21);
            this.rootPoints.TabIndex = 11;
            // 
            // labelRootPoint
            // 
            this.labelRootPoint.AutoSize = true;
            this.labelRootPoint.Location = new System.Drawing.Point(42, 186);
            this.labelRootPoint.Name = "labelRootPoint";
            this.labelRootPoint.Size = new System.Drawing.Size(60, 13);
            this.labelRootPoint.TabIndex = 12;
            this.labelRootPoint.Text = "Root Point:";
            // 
            // patternUVFile
            // 
            this.patternUVFile.AllowDrop = true;
            this.patternUVFile.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.patternUVFile.Filter = "Object File (*.obj)|*.obj";
            this.patternUVFile.Location = new System.Drawing.Point(108, 126);
            this.patternUVFile.Name = "patternUVFile";
            this.patternUVFile.SaveMode = false;
            this.patternUVFile.Size = new System.Drawing.Size(377, 27);
            this.patternUVFile.TabIndex = 6;
            // 
            // objectFile
            // 
            this.objectFile.AllowDrop = true;
            this.objectFile.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.objectFile.Filter = "Object File (*.obj)|*.obj";
            this.objectFile.Location = new System.Drawing.Point(108, 92);
            this.objectFile.Name = "objectFile";
            this.objectFile.SaveMode = false;
            this.objectFile.Size = new System.Drawing.Size(377, 27);
            this.objectFile.TabIndex = 5;
            // 
            // outputBox
            // 
            this.outputBox.AllowDrop = true;
            this.outputBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.outputBox.Filter = "Diesel Model (*.model)|*.model";
            this.outputBox.Location = new System.Drawing.Point(108, 244);
            this.outputBox.Name = "outputBox";
            this.outputBox.SaveMode = true;
            this.outputBox.Size = new System.Drawing.Size(377, 27);
            this.outputBox.TabIndex = 4;
            // 
            // baseModelFileBrowser
            // 
            this.baseModelFileBrowser.AllowDrop = true;
            this.baseModelFileBrowser.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.baseModelFileBrowser.Filter = "Diesel Model (*.model)|*.model";
            this.baseModelFileBrowser.Location = new System.Drawing.Point(108, 3);
            this.baseModelFileBrowser.Name = "baseModelFileBrowser";
            this.baseModelFileBrowser.SaveMode = false;
            this.baseModelFileBrowser.Size = new System.Drawing.Size(377, 27);
            this.baseModelFileBrowser.TabIndex = 0;
            this.baseModelFileBrowser.FileSelected += new System.EventHandler(this.baseModelFileBrowser_FileSelected);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(42, 207);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(234, 13);
            this.label2.TabIndex = 13;
            this.label2.Text = "(which bone new objects should be attached to)";
            // 
            // lblScript
            // 
            lblScript.AutoSize = true;
            lblScript.Location = new System.Drawing.Point(65, 66);
            lblScript.Name = "lblScript";
            lblScript.Size = new System.Drawing.Size(37, 13);
            lblScript.TabIndex = 15;
            lblScript.Text = "Script:";
            // 
            // scriptFile
            // 
            this.scriptFile.AllowDrop = true;
            this.scriptFile.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.scriptFile.Filter = "Model Script (*.mscript)|*.mscript";
            this.scriptFile.Location = new System.Drawing.Point(108, 59);
            this.scriptFile.Name = "scriptFile";
            this.scriptFile.SaveMode = false;
            this.scriptFile.Size = new System.Drawing.Size(377, 27);
            this.scriptFile.TabIndex = 14;
            // 
            // ImportPanel
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(lblScript);
            this.Controls.Add(this.scriptFile);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.labelRootPoint);
            this.Controls.Add(this.rootPoints);
            this.Controls.Add(this.createNewObjectsBox);
            this.Controls.Add(this.convert);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.labelObj);
            this.Controls.Add(this.patternUVFile);
            this.Controls.Add(this.objectFile);
            this.Controls.Add(this.outputBox);
            this.Controls.Add(this.labelSaveTo);
            this.Controls.Add(this.createNewModel);
            this.Controls.Add(this.labelSelBaseModel);
            this.Controls.Add(this.baseModelFileBrowser);
            this.Name = "ImportPanel";
            this.Size = new System.Drawing.Size(488, 303);
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
        private System.Windows.Forms.Label labelObj;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button convert;
        private System.Windows.Forms.CheckBox createNewObjectsBox;
        private System.Windows.Forms.ComboBox rootPoints;
        private System.Windows.Forms.Label labelRootPoint;
        private System.Windows.Forms.Label label2;
        private FileBrowserControl scriptFile;
    }
}
