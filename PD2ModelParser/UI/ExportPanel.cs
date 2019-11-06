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
            model = ModelReader.Open(inputFileBox.Selected);

            exportBttn.Enabled = true;
        }

        private void rootPoint_combobox_SelectedIndexChanged(object sender, EventArgs e)
        {
            //MessageBox.Show("root_point configuration temporarally unavailable");
            //if (Form1.fm.updateRP(this.rootPoint_combobox.Text))
            //{
            //    MessageBox.Show("Set model root_point successfully");
            //    return;
            //}
            //MessageBox.Show("Failed setting model root_point!");

            model = ModelReader.Open(inputFileBox.Selected);
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
