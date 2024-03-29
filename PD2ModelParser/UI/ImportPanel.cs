using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.Windows.Forms.Layout;
using PD2ModelParser.Sections;

using Size = System.Drawing.Size;

namespace PD2ModelParser.UI
{
    public partial class ImportPanel : UserControl
    {
        class ImportPanelLayoutEngine : LayoutEngine
        {
            struct TableRow {
                public Label label;
                public Control field;
                public int labelWidth;
                public int minFieldWidth;
                public int height;

                public TableRow(Label label, Control field) {
                    this.label = label;
                    this.field = field;

                    var labelSize = label?.GetPreferredSize(new Size(1,1)) ?? new Size(0,0);
                    var fieldSize = this.field.GetPreferredSize(new Size(1,1));

                    this.height = Math.Max(labelSize.Height + (label?.Margin.Vertical ?? 0), fieldSize.Height + this.field.Margin.Vertical);
                    this.minFieldWidth = fieldSize.Width;
                    this.labelWidth = labelSize.Width + (label?.Margin.Horizontal ?? 0);
                }
            }

            Label[] labels;
            Control[] fields;
            TableRow[] rows;

            int maxLabelWidth;
            int minFieldWidth;

            public ImportPanelLayoutEngine(ImportPanel panel)
            {
                
            }

            private void InitialAnalysis(ImportPanel panel)
            {   
                if(this.rows != null) { return; }

                this.rows = new TableRow[] {
                    new TableRow(panel.labelSelBaseModel, panel.baseModelFileBrowser),
                    new TableRow(null, panel.createNewModel),
                    new TableRow(panel.lblScript, panel.scriptFile),
                    new TableRow(panel.labelObj, panel.objectFile),
                    new TableRow(panel.labelPatternUV, panel.patternUVFile),
                    new TableRow(panel.labelAnimations, panel.animationFiles),
                    new TableRow(null, panel.createNewObjectsBox),
                    new TableRow(null, panel.importTransformsBox),
                    new TableRow(panel.labelRootPoint, panel.rootPoints),
                    new TableRow(null, panel.labelRootPointHint),
                    new TableRow(panel.labelSaveTo, panel.outputBox)
                };
                
                this.maxLabelWidth = this.rows.Select(i => i.labelWidth).Max();

                var currY = 0;
                for (var i = 0; i < rows.Length; i++)
                {
                    var label = rows[i].label;
                    var labelSize = label?.GetPreferredSize(new Size(1,1)) ?? new Size(0,0);
                    var rowHeight = this.rows[i].height;
                    if (label != null)
                    {
                        var labelOffsY = (rowHeight - labelSize.Height) / 2;
                        label.SetBounds(this.maxLabelWidth - (labelSize.Width + label.Margin.Right), currY + labelOffsY, labelSize.Width, labelSize.Height);
                    }
                    currY += rowHeight;
                }
            }

            public override bool Layout(object container, LayoutEventArgs layoutEventArgs)
            {
                //return base.Layout(container, layoutEventArgs);

                //Console.WriteLine("Layout");

                var panel = (ImportPanel)container;

                this.InitialAnalysis(panel);

                var currY = 0;
                for (var i = 0; i < rows.Length; i++)
                {
                    var field = rows[i].field;
                    var fieldSize = field.GetPreferredSize(new Size(1,1));

                    var rowHeight = this.rows[i].height;

                    var fieldX = maxLabelWidth + field.Margin.Left;
                    var fieldOffsY = currY + (rowHeight - fieldSize.Height) / 2;
                    var fieldWidth = panel.Width - (maxLabelWidth + field.Margin.Horizontal);
                    field.SetBounds(fieldX, fieldOffsY, fieldWidth, fieldSize.Height);
                    currY += rowHeight;
                }

                var buttonSize = panel.convert.GetPreferredSize(new Size(1, 1));
                panel.convert.SetBounds(panel.convert.Margin.Left, currY + panel.convert.Margin.Top, panel.Width - panel.convert.Margin.Horizontal, buttonSize.Height);
                currY = panel.convert.Bounds.Bottom + panel.convert.Margin.Bottom;

                //Console.WriteLine("End Layout");

                panel.MinimumSize = new Size(panel.MinimumSize.Width, currY);

                return true;
            }

            //public Size GetMinimumSize()
            //{
            //    panel
            //}
        }

        private List<RootPointItem> root_point_items = new List<RootPointItem>();
        private ImportPanelLayoutEngine layout;
        public override LayoutEngine LayoutEngine => layout;

        public ImportPanel()
        {
            layout = new ImportPanelLayoutEngine(this);
            InitializeComponent();
        }

        private void ImportPanel_Load(object sender, EventArgs e)
        {
            UpdateRootPointBox();
        }

        private void createNewModel_CheckedChanged(object sender, EventArgs e)
        {
            baseModelFileBrowser.Enabled = !createNewModel.Checked;
            createNewObjectsBox.Enabled = !createNewModel.Checked;
            importTransformsBox.Enabled = !createNewModel.Checked;
        }

        private void convert_Click(object sender, EventArgs e)
        {

            if (baseModelFileBrowser.Selected == null && !createNewModel.Checked)
            {
                MessageBox.Show("Either select a valid base model or select the create new model box");
                return;
            }

            if (outputBox.Selected == null)
            {
                MessageBox.Show("Please choose an output destination");
                return;
            }

            bool createNewObjects = createNewModel.Checked || createNewObjectsBox.Checked;

            var script = new List<Modelscript.IScriptItem>();
            if (!createNewModel.Checked)
            {
                script.Add(new Modelscript.LoadModel() { File = baseModelFileBrowser.Selected });
            }
            else
            {
                script.Add(new Modelscript.NewModel());
            }

            if (scriptFile.Selected != null)
            {
                script.Add(new Modelscript.RunScript() { File = scriptFile.Selected });
            }

            if (objectFile.Selected != null)
            {
                script.Add(new Modelscript.CreateNewObjects() { Create = createNewObjects });
                var importDirective = new Modelscript.Import() { File = objectFile.Selected };

                if(rootPoints.SelectedIndex > 0)
                {
                    RootPointItem item = root_point_items[rootPoints.SelectedIndex];
                    importDirective.DefaultRootPoint = item.Name;
                }

                importDirective.ImporterOptions.Add("import-transforms", importTransformsBox.Checked.ToString());

                script.Add(importDirective);
            }

            if (patternUVFile.Selected != null)
            {
                script.Add(new Modelscript.PatternUV() { File = patternUVFile.Selected });
            }

            if (animationFiles.AllSelected.Count > 0) {
                foreach (string filepath in animationFiles.AllSelected) {
                    script.Add(new Modelscript.LoadAnimation() { File = filepath });
                }
            }


            script.Add(new Modelscript.SaveModel() { File = outputBox.Selected });

            try
            {
                Modelscript.Script.ExecuteItems(script, System.IO.Directory.GetCurrentDirectory(), null);
            }
            catch(Exception exc)
            {
                Log.Default.Warn("Exception generating Diesel file: {0}", exc);
                MessageBox.Show("There was an error importing the data - see console");
                return;
            }

            MessageBox.Show("Model generated successfully");
        }

        private void baseModelFileBrowser_FileSelected(object sender, EventArgs e)
        {
            UpdateRootPointBox();
        }

        private void UpdateRootPointBox()
        {
            string old_selected_name;
            if(rootPoints.SelectedIndex > 0)
            {
                old_selected_name = root_point_items[rootPoints.SelectedIndex].Name;
            }
            else
            {
                // Use the root point, if it exists. Otherwise, this won't match and
                // we'll use the default new_index of 0.
                old_selected_name = "root_point";
            }
            int new_index = 0;

            root_point_items.Clear();
            root_point_items.Add(new RootPointItem("None", 0));

            if (scriptFile.Selected != null)
            {
                // If we're using a script, we unfortunately have to fully load the file to evaluate the script

                string model_file = baseModelFileBrowser.Enabled ? baseModelFileBrowser.Selected : null;
                FullModelData data = model_file != null ? ModelReader.Open(model_file) : new FullModelData();
                // TODO display the errors in a less intrusive way
                bool success = Modelscript.Script.ExecuteFileWithMsgBox(ref data, scriptFile.Selected);
                if (!success)
                    return;

                foreach (Object3D obj in data.SectionsOfType<Object3D>())
                {
                    root_point_items.Add(new RootPointItem(obj.Name, obj.SectionId));

                    if (old_selected_name == obj.Name)
                    {
                        new_index = root_point_items.Count - 1;
                    }
                }
            }
            else if (baseModelFileBrowser.Enabled && baseModelFileBrowser.Selected != null)
            {
                // If there is no script file, just skim the model and collect the object IDs like that.
                // This isn't a major improvement, but it does increase performance.
                StaticStorage.hashindex.Load();
                ModelReader.VisitModel(baseModelFileBrowser.Selected, (reader, header) => {
                    if (header.type == Tags.object3D_tag)
                    {
                        // First field of Object3D
                        ulong hashname = reader.ReadUInt64();
                        string name = StaticStorage.hashindex.GetString(hashname);

                        RootPointItem item = new RootPointItem(name, header.id);
                        root_point_items.Add(item);

                        Log.Default.Debug("Scanning for rootpoint: {0}", name);

                        if(old_selected_name == name)
                        {
                            new_index = root_point_items.Count - 1;
                        }
                    }
                });
            }

            rootPoints.Items.Clear();
            rootPoints.Items.AddRange(root_point_items.ToArray());
            rootPoints.SelectedIndex = new_index;
        }

        private class RootPointItem
        {
            public readonly string Name;
            public readonly uint Id;

            public RootPointItem(string name, uint id)
            {
                Name = name;
                Id = id;
            }

            public override string ToString()
            {
                return Name;
            }
        }

        private void scriptFile_FileSelected(object sender, EventArgs e)
        {
            UpdateRootPointBox();
        }
    }
}
