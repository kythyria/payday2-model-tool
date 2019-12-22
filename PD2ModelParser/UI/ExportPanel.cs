using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PD2ModelParser.UI
{
    public partial class ExportPanel : UserControl
    {
        private FullModelData model;

        public ExportPanel()
        {
            InitializeComponent();

            // Select the default item, since for whatever reason we can't
            // do that in the designer.
            formatBox.SelectedIndex = 0;
        }

        private void inputFileBox_FileSelected(object sender, EventArgs e)
        {
            if (inputFileBox.Selected == null)
            {
                exportBttn.Enabled = false;
                return;
            }

            model = ModelReader.Open(inputFileBox.Selected);

            exportBttn.Enabled = true;
        }

        private void exportBttn_Click(object sender, EventArgs e)
        {
            string format = (string)formatBox.SelectedItem;

            string result;
            if (format.Contains(".obj"))
            {
                result = ObjWriter.ExportFile(model, inputFileBox.Selected);
            }
            else if (format.Contains(".dae"))
            {
                result = ColladaExporter.ExportFile(model, inputFileBox.Selected);
            }
            else if (format.Contains(".fbx"))
            {
#if NO_FBX
                MessageBox.Show("This copy of the model tool was compiled without the FBX SDK", "FBX Export Unavailable");
                return;
#else
                result = Exporters.FbxExporter.ExportFile(model, inputFileBox.Selected);
#endif
            }
            else if (format.Contains(".glb"))
            {
                result = Exporters.GltfExporter.ExportFile(model, inputFileBox.Selected);
            }
            else
            {
                MessageBox.Show("Unknown format '" + format + "'");
                return;
            }

            MessageBox.Show("Successfully exported model " + result.Split('\\').Last() + " (placed in the input model folder)");

            //DieselExporter.ExportFile(model, inputFileBox.Text);
        }
    }
}
