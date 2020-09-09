using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using System.Windows.Forms.Layout;
using PD2ModelParser.Modelscript;

namespace PD2ModelParser.UI
{
    public partial class ExportPanel : UserControl
    {
        private FullModelData model;
        private ExportLayoutEngine layout = new ExportLayoutEngine();
        public override LayoutEngine LayoutEngine => layout;

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

        class ExportLayoutEngine : LayoutEngine
        {

            public override bool Layout(object sender, LayoutEventArgs e)
            {
                var panel = (ExportPanel)sender;

                var labels = new Label[] {
                    panel.label1,
                    panel.label2
                };

                var fields = new Control[]
                {
                panel.inputFileBox,
                panel.formatBox
                };

                var maxLabelWidth = labels
                    .Where(i => i != null)
                    .Select(i => i.PreferredSize.Width + i.Margin.Horizontal).Max();
                var currY = 0;

                for (var i = 0; i < fields.Length; i++)
                {
                    var label = labels[i];
                    var labelSize = label?.GetPreferredSize(new Size(1, 1)) ?? new Size(0, 0);
                    var field = fields[i];
                    var fieldSize = field.GetPreferredSize(new Size(1, 1));

                    var rowHeight = Math.Max(labelSize.Height + (label?.Margin.Vertical ?? 0), fieldSize.Height + field.Margin.Vertical);

                    if (label != null)
                    {
                        var labelOffsY = (rowHeight - labelSize.Height) / 2;
                        label.SetBounds(maxLabelWidth - (labelSize.Width + label.Margin.Right), currY + labelOffsY, labelSize.Width, labelSize.Height);
                    }

                    var fieldX = maxLabelWidth + field.Margin.Left;
                    var fieldOffsY = currY + (rowHeight - fieldSize.Height) / 2;
                    var fieldWidth = panel.Width - (maxLabelWidth + field.Margin.Horizontal);
                    field.SetBounds(fieldX, fieldOffsY, fieldWidth, fieldSize.Height);
                    currY += rowHeight;
                }

                var buttonSize = panel.exportBttn.GetPreferredSize(new Size(1, 1));
                panel.exportBttn.SetBounds(panel.exportBttn.Margin.Left, currY + panel.exportBttn.Margin.Top, panel.Width - panel.exportBttn.Margin.Horizontal, buttonSize.Height);
                currY += panel.exportBttn.Bounds.Bottom + panel.exportBttn.Margin.Bottom;

                //Console.WriteLine("End Layout");

                panel.MinimumSize = new Size(panel.MinimumSize.Width, currY);
                return true;
            }
        }
    }
}
