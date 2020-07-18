using PD2ModelParser.Sections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.Xml.Linq;

namespace PD2ModelParser.UI
{
    public partial class ImportPanel : UserControl
    {
        private List<RootPointItem> root_point_items = new List<RootPointItem>();

        public ImportPanel()
        {
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

            FullModelData model = new FullModelData();

            if (!createNewModel.Checked)
            {
                model = ModelReader.Open(baseModelFileBrowser.Selected);
            }

            if (scriptFile.Selected != null)
            {
                bool success = Modelscript.Script.ExecuteFileWithMsgBox(ref model, scriptFile.Selected);
                if (!success)
                    return;
            }

            var script = new List<Modelscript.IScriptItem>();
            if (objectFile.Selected != null)
            {
                script.Add(new Modelscript.CreateNewObjects() { Create = createNewObjects });
                var importDirective = new Modelscript.Import() { File = objectFile.Selected };

                if(rootPoints.SelectedIndex > 0)
                {
                    RootPointItem item = root_point_items[rootPoints.SelectedIndex];
                    importDirective.DefaultRootPoint = item.Name;
                }

                script.Add(importDirective);
            }

            if (patternUVFile.Selected != null)
            {
                script.Add(new Modelscript.PatternUV() { File = patternUVFile.Selected });
            }

            script.Add(new Modelscript.SaveModel() { File = outputBox.Selected });

            try
            {
                Modelscript.Script.ExecuteItems(script, System.IO.Directory.GetCurrentDirectory(), model);
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
