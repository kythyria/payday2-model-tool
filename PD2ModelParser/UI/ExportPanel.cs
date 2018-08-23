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

        private void browseBttn_Click(object sender, EventArgs e)
        {
            StaticStorage.objects_list = new List<string>();
            StaticStorage.rp_id = 0u;
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Diesel Model(*.model)|*.model";
            openFileDialog.CheckFileExists = true;
            if (openFileDialog.ShowDialog() != DialogResult.OK) return;

            inputFileBox.Text = openFileDialog.FileName;
            model = ModelReader.Open(openFileDialog.FileName, rootPoint_combobox.Text);

            this.rootPoint_combobox.Items.Clear();
            this.rootPoint_combobox.Items.AddRange(StaticStorage.objects_list.ToArray());

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

            model = ModelReader.Open(inputFileBox.Text, rootPoint_combobox.Text);
        }

        private void exportBttn_Click(object sender, EventArgs e)
        {
            string format = (string)formatBox.SelectedItem;

            string result;
            if (format.Contains(".obj"))
            {
                result = ObjWriter.ExportFile(model, inputFileBox.Text);
            }
            else if (format.Contains(".dae"))
            {
                result = ColladaExporter.ExportFile(model, inputFileBox.Text);
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
