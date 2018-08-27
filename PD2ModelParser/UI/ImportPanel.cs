using System;
using System.Windows.Forms;

namespace PD2ModelParser.UI
{
    public partial class ImportPanel : UserControl
    {
        public ImportPanel()
        {
            InitializeComponent();
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

            FileManager fm = new FileManager();

            if (!createNewModel.Checked)
            {
                FullModelData model = ModelReader.Open(baseModelFileBrowser.Selected);
                fm.parsed_sections = model.parsed_sections;
                fm.sections = model.sections;
                fm.leftover_data = model.leftover_data;
            }

            if (objectFile.Selected != null)
            {
                bool result = NewObjImporter.ImportNewObj(fm, objectFile.Selected, createNewObjects);
                if (!result)
                {
                    MessageBox.Show("There was an error importing OBJ - see console");
                    return;
                }
            }

            if (patternUVFile.Selected != null)
            {
                bool result = NewObjImporter.ImportNewObjPatternUV(fm, objectFile.Selected);
                if (!result)
                {
                    MessageBox.Show("There was an error importing Pattern UV OBJ - see console");
                    return;
                }
            }

            bool outputSuccess = fm.GenerateNewModel(outputBox.Selected);
            if (!outputSuccess)
            {
                MessageBox.Show("There was an error generating the output file - see console");
                return;
            }

            MessageBox.Show("Model generated successfully");
        }
    }
}