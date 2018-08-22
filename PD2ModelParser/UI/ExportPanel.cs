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
            ColladaExporter.ExportFile(model, inputFileBox.Text);
        }
    }
}
