using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows.Forms;
using PD2ModelParser.Modelscript;

namespace PD2ModelParser.UI
{
    public partial class ExportPanel : UserControl
    {
        private FullModelData model;

        public ExportPanel()
        {
            InitializeComponent();

            formatBox.BeginUpdate();

            // Fill in the actual list of exporters
            formatBox.Items.Clear();
            formatBox.Items.AddRange(FileTypeInfo.Types.Where(i => i.CanExport).ToArray());
            formatBox.DisplayMember = nameof(FileTypeInfo.FormatName);
            // Select the default item, since for whatever reason we can't
            // do that in the designer.
            formatBox.SelectedIndex = 0;

            formatBox.EndUpdate();

        }

        private void inputFileBox_FileSelected(object sender, EventArgs e)
        {
            if (inputFileBox.Selected == null)
            {
                exportBttn.Enabled = false;
                return;
            }
            exportBttn.Enabled = true;
        }

        private void exportBttn_Click(object sender, EventArgs e)
        {
            var script = new List<IScriptItem>();

            script.Add(new LoadModel() { File = inputFileBox.Selected });
            model = ModelReader.Open(inputFileBox.Selected);

            var exportCmd = new Export();

            var type = formatBox.SelectedItem as FileTypeInfo;
            if(type == null)
            {
                MessageBox.Show("Unknown format '{format}'");
                return;
            }
            var outName = System.IO.Path.ChangeExtension(inputFileBox.Selected, type.Extension);
            exportCmd.File = outName;
            script.Add(exportCmd);
            Script.ExecuteItems(script, System.IO.Directory.GetCurrentDirectory());

            MessageBox.Show($"Successfully exported model {inputFileBox.Selected.Split('\\').Last()} (placed in the input model folder)");
        }
    }
}
