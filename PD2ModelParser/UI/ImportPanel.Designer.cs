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
            this.labelSelBaseModel = new System.Windows.Forms.Label();
            this.createNewModel = new System.Windows.Forms.CheckBox();
            this.baseModelFileBrowser = new PD2ModelParser.UI.FileBrowserControl();
            this.labelSaveTo = new System.Windows.Forms.Label();
            this.outputBox = new PD2ModelParser.UI.FileBrowserControl();
            this.objectFile = new PD2ModelParser.UI.FileBrowserControl();
            this.patternUVFile = new PD2ModelParser.UI.FileBrowserControl();
            this.labelObj = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.convert = new System.Windows.Forms.Button();
            this.createNewObjectsBox = new System.Windows.Forms.CheckBox();
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
            // baseModelFileBrowser
            // 
            this.baseModelFileBrowser.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.baseModelFileBrowser.Filter = "Diesel Model (*.model)|*.model";
            this.baseModelFileBrowser.Location = new System.Drawing.Point(108, 3);
            this.baseModelFileBrowser.Name = "baseModelFileBrowser";
            this.baseModelFileBrowser.SaveMode = false;
            this.baseModelFileBrowser.Size = new System.Drawing.Size(377, 27);
            this.baseModelFileBrowser.TabIndex = 0;
            // 
            // labelSaveTo
            // 
            this.labelSaveTo.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.labelSaveTo.AutoSize = true;
            this.labelSaveTo.Location = new System.Drawing.Point(3, 192);
            this.labelSaveTo.Name = "labelSaveTo";
            this.labelSaveTo.Size = new System.Drawing.Size(51, 13);
            this.labelSaveTo.TabIndex = 3;
            this.labelSaveTo.Text = "Save To:";
            // 
            // outputBox
            // 
            this.outputBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.outputBox.Filter = "Diesel Model (*.model)|*.model";
            this.outputBox.Location = new System.Drawing.Point(108, 184);
            this.outputBox.Name = "outputBox";
            this.outputBox.SaveMode = true;
            this.outputBox.Size = new System.Drawing.Size(377, 27);
            this.outputBox.TabIndex = 4;
            // 
            // objectFile
            // 
            this.objectFile.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.objectFile.Filter = "Object File (*.obj)|*.obj";
            this.objectFile.Location = new System.Drawing.Point(108, 60);
            this.objectFile.Name = "objectFile";
            this.objectFile.SaveMode = false;
            this.objectFile.Size = new System.Drawing.Size(377, 27);
            this.objectFile.TabIndex = 5;
            // 
            // patternUVFile
            // 
            this.patternUVFile.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.patternUVFile.Filter = "Object File (*.obj)|*.obj";
            this.patternUVFile.Location = new System.Drawing.Point(108, 94);
            this.patternUVFile.Name = "patternUVFile";
            this.patternUVFile.SaveMode = false;
            this.patternUVFile.Size = new System.Drawing.Size(377, 27);
            this.patternUVFile.TabIndex = 6;
            // 
            // labelObj
            // 
            this.labelObj.AutoSize = true;
            this.labelObj.Location = new System.Drawing.Point(61, 74);
            this.labelObj.Name = "labelObj";
            this.labelObj.Size = new System.Drawing.Size(41, 13);
            this.labelObj.TabIndex = 7;
            this.labelObj.Text = "Object:";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(40, 108);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(62, 13);
            this.label1.TabIndex = 8;
            this.label1.Text = "Pattern UV:";
            // 
            // convert
            // 
            this.convert.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.convert.Location = new System.Drawing.Point(6, 217);
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
            this.createNewObjectsBox.Location = new System.Drawing.Point(108, 127);
            this.createNewObjectsBox.Name = "createNewObjectsBox";
            this.createNewObjectsBox.Size = new System.Drawing.Size(163, 17);
            this.createNewObjectsBox.TabIndex = 10;
            this.createNewObjectsBox.Text = "Import objects not in base file";
            this.createNewObjectsBox.UseVisualStyleBackColor = true;
            // 
            // ImportPanel
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
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
            this.Size = new System.Drawing.Size(488, 243);
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
    }
}
