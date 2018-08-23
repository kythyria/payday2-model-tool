using System;
using System.ComponentModel;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace PD2ModelParser.UI
{
    [DefaultEvent("FileSelected")]
    public partial class FileBrowserControl : UserControl
    {
        // See https://stackoverflow.com/a/8531166
        private static Regex filterRegex = new Regex(@"(?<Name>[^|]*)\|(?<Extension>[^|]*)\|?");

        public FileBrowserControl()
        {
            InitializeComponent();

            AllowDrop = true;
            DragEnter += HandleDragEnter;
            DragDrop += HandleDragDrop;
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

        private void HandleDragEnter(object sender, DragEventArgs e)
        {
            // Can't drag a file into a save box, since I can't be bothered to
            // write the 'are you sure you want to overwrite this file?' code.
            if (SaveMode)
                return;

            // Check it's a file being dragged in
            if (!e.Data.GetDataPresent(DataFormats.FileDrop))
                return;

            // Ensure there is only one file, and it is indeed a file and not a directory or anything
            string[] files = (string[]) e.Data.GetData(DataFormats.FileDrop);

            if (files.Length != 1)
                return;

            string file = files[0];
            if (!File.Exists(file))
                return;

            // Check the file against the set filters.
            //
            // For each allowable filter:
            MatchCollection matches = filterRegex.Matches(Filter);
            foreach (Match match in matches)
            {
                // The name of the filter we're testing against - not used.
                string name = match.Groups["Name"].Value;

                // For each of the allowable extensions
                foreach (string ext in match.Groups["Extension"].Value.Split(';'))
                {
                    // If it allows anything, it matches
                    if (ext == "*.*")
                        goto FilterMatch;

                    // Ensure the extension has two parts, and find the actual
                    // extension we're looking for (trueExt) - eg '*.abc' goes into 'abc'
                    string[] parts = ext.Split('.');
                    if (parts.Length != 2)
                        continue;
                    string trueExt = parts[1];

                    // Ensure the file doese have an extension
                    parts = file.Split('.');
                    if (parts.Length < 2)
                        continue;

                    // Check the extension matches
                    // Note we use length-1 - this is so a file like 'a.b.c' has an extension of 'c'
                    // (ie, for files with dots in their names)
                    if (parts[parts.Length - 1] == trueExt)
                        goto FilterMatch;
                }
            }

            return;

            // We are effectively copying the file in
            FilterMatch:
            e.Effect = DragDropEffects.Copy;
        }

        private void HandleDragDrop(object sender, DragEventArgs e)
        {
            // This is only called if we set the effect in HandleDragEnter, so
            // this won't be called if the user is dragging in a directory or anything.
            string file = ((string[]) e.Data.GetData(DataFormats.FileDrop))[0];

            inputFileBox.Text = file;
        }
    }
}
