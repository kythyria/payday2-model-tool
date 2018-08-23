using System;
using System.ComponentModel;
using System.IO;
using System.Windows.Forms;

namespace PD2ModelParser.UI
{
    [DefaultEvent("FileSelected")]
    public partial class FileBrowserControl : UserControl
    {
        public FileBrowserControl()
        {
            InitializeComponent();
        }

        public event EventHandler FileSelected;

        private bool _saveMode;

        public bool SaveMode
        {
            get
            {
                return _saveMode;
            }
            set
            {
                _saveMode = value;

                // If we're in save mode, the user can edit the output box
                inputFileBox.Enabled = value;
            }
        }

        public string Filter { get; set; }

        public string Selected
        {
            get
            {
                string text = inputFileBox.Text;
                if (text == "") return null;

                if (SaveMode)
                {
                    DirectoryInfo info = Directory.GetParent(text);
                    if (!info.Exists) return null;
                }
                else
                {
                    if (!File.Exists(text))
                    {
                        return null;
                    }
                }

                return inputFileBox.Text;
            }
        }

        private void browseBttn_Click(object sender, EventArgs e)
        {
            FileDialog fileDialog;
            if (SaveMode)
            {
                fileDialog = new SaveFileDialog();
            }
            else
            {
                fileDialog = new OpenFileDialog();
                fileDialog.CheckFileExists = true;
            }

            fileDialog.Filter = Filter; // "Diesel Model(*.model)|*.model";

            if (fileDialog.ShowDialog() != DialogResult.OK) return;

            inputFileBox.Text = fileDialog.FileName;

            FileSelected?.Invoke(this, e);
        }
    }
}
