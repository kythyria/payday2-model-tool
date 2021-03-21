using System;
using System.Collections;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.Windows.Forms.Design;
using System.Linq;
using System.Windows.Forms.Design.Behavior;
using NUnit.Framework;
using System.Collections.Generic;

namespace PD2ModelParser.UI {
    [DefaultEvent("FileSelected")]
    [Designer(typeof(FileBrowserDesigner))]
    public partial class FileBrowserControl : UserControl {
        // See https://stackoverflow.com/a/8531166
        private static Regex filterRegex = new Regex(@"(?<Name>[^|]*)\|(?<Extension>[^|]*)\|?");

        public FileBrowserControl() {
            InitializeComponent();

            AllowDrop = true;
            DragEnter += HandleDragEnter;
            DragDrop += HandleDragDrop;

            ContextMenu cm = new ContextMenu();
            ContextMenu = cm;
            MenuItem clearItem = cm.MenuItems.Add("Clear");
            clearItem.Click += ClearFileSelected;
        }

        public event EventHandler FileSelected;

        private bool _saveMode;

        public bool SaveMode {
            get {
                return _saveMode;
            }
            set {
                _saveMode = value;

                // If we're in save mode, the user can edit the output box
                inputFileBox.Enabled = value;
            }
        }

        public bool MultiFile { get; set; }

        public string Filter { get; set; }

        public string Selected {
            get {
                if (AllSelected.Count > 0) {
                    return AllSelected[0];
                }
                return null;
            }
            set {
                AllSelected.Clear();
                AllSelected.Add(value);
            }
        }

        private List<string> _allSelected = new List<string>();
        public List<string> AllSelected {
            get {
                string text = inputFileBox.Text;
                if (text == "") {
                    _allSelected.Clear();
                    return _allSelected;
                }

                if (SaveMode) {
                    DirectoryInfo info = Directory.GetParent(text);
                    if (!info.Exists) {
                        _allSelected.Clear();
                        return _allSelected;
                    }
                } else {
                    if (!MultiFile && !File.Exists(text)) {
                        _allSelected.Clear();
                        return _allSelected;
                    }

                    if (MultiFile) {
                        _allSelected = inputFileBox.Text.Split(';').ToList();
                        foreach (string filepath in _allSelected) {
                            if (!File.Exists(filepath)) {
                                _allSelected.Clear();
                                return _allSelected;
                            }
                        }
                    }
                }

                return _allSelected;
            }
            private set {
                if (value == null) {
                    value = new List<string>();
                }

                inputFileBox.Text = string.Join(";", value) ?? "";
                _allSelected = value;
                FileSelected?.Invoke(this, new EventArgs());
            }
        }

        private void browseBttn_Click(object sender, EventArgs e) {
            FileDialog fileDialog;
            if (SaveMode) {
                fileDialog = new SaveFileDialog();
            } else {
                fileDialog = new OpenFileDialog();
                fileDialog.CheckFileExists = true;

                if (MultiFile) {
                    ((OpenFileDialog)fileDialog).Multiselect = true;
                }
            }

            fileDialog.Filter = Filter; // "Diesel Model(*.model)|*.model";

            if (fileDialog.ShowDialog() != DialogResult.OK) return;

            AllSelected = fileDialog.FileNames.ToList();
        }

        private void HandleDragEnter(object sender, DragEventArgs e) {
            // Can't drag a file into a save box, since I can't be bothered to
            // write the 'are you sure you want to overwrite this file?' code.
            if (SaveMode)
                return;

            // Check it's a file being dragged in
            if (!e.Data.GetDataPresent(DataFormats.FileDrop))
                return;

            // Ensure there is only one file, and it is indeed a file and not a directory or anything
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);

            if (!MultiFile && files.Length != 1)
                return;

            string file = files[0];
            if (!File.Exists(file))
                return;

            if (MultiFile) {
                foreach (string leFile in files) {
                    if (!File.Exists(leFile))
                        return;
                }
            }

            // Check the file against the set filters.
            //
            // For each allowable filter:
            bool failed = false;

            MatchCollection matches = filterRegex.Matches(Filter);
            foreach (Match match in matches) {
                // The name of the filter we're testing against - not used.
                string name = match.Groups["Name"].Value;

                // For each of the allowable extensions
                foreach (string ext in match.Groups["Extension"].Value.Split(';')) {
                    // If it allows anything, it matches
                    if (ext != "*.*")
                        failed = true;

                    // Ensure the extension has two parts, and find the actual
                    // extension we're looking for (trueExt) - eg '*.abc' goes into 'abc'
                    string[] parts = ext.Split('.');
                    if (parts.Length != 2)
                        failed = true;
                    string trueExt = parts[1];

                    // Ensure the file doese have an extension
                    parts = file.Split('.');
                    if (parts.Length < 2)
                        failed = true;

                    // Check the extension matches
                    // Note we use length-1 - this is so a file like 'a.b.c' has an extension of 'c'
                    // (ie, for files with dots in their names)
                    if (parts[parts.Length - 1] != trueExt)
                        failed = true;

                    if (MultiFile) {
                        foreach (string leFile in files) {
                            // Ensure the file doese have an extension
                            parts = leFile.Split('.');
                            if (parts.Length < 2)
                                failed = true;

                            // Check the extension matches
                            // Note we use length-1 - this is so a file like 'a.b.c' has an extension of 'c'
                            // (ie, for files with dots in their names)
                            if (parts[parts.Length - 1] != trueExt)
                                failed = true;
                        }
                    }
                }
            }

            if (failed) {
                return;
            }

            // We are effectively copying the file in
            e.Effect = DragDropEffects.Copy;
        }

        private void HandleDragDrop(object sender, DragEventArgs e) {
            // This is only called if we set the effect in HandleDragEnter, so
            // this won't be called if the user is dragging in a directory or anything.
            List<string> files = (List<string>)e.Data.GetData(DataFormats.FileDrop);

            if (!MultiFile && files.Count != 1)
                return;

            AllSelected = files;
        }

        private void ClearFileSelected(object sender, EventArgs e) {
            AllSelected = null;
        }

        protected override void SetBoundsCore(int x, int y, int width, int height, BoundsSpecified specified) {
            base.SetBoundsCore(x, y, width, Math.Max(inputFileBox.Height, browseBttn.Height), specified);
        }

        class FileBrowserDesigner : ControlDesigner {
            public FileBrowserDesigner() {
                base.AutoResizeHandles = true;
            }

            public override SelectionRules SelectionRules => SelectionRules.LeftSizeable | SelectionRules.RightSizeable | SelectionRules.Moveable;

            public override IList SnapLines {
                get {
                    // https://stackoverflow.com/questions/93541/baseline-snaplines-in-custom-winforms-controls
                    // This isn't pretty, but it works.

                    var snaplines = base.SnapLines;
                    var fbc = Control as FileBrowserControl;
                    if (fbc == null) { return snaplines; }

                    var designer = TypeDescriptor.CreateDesigner(fbc.inputFileBox, typeof(IDesigner));
                    designer.Initialize(fbc.inputFileBox);

                    using (designer) {
                        var boxDesigner = designer as ControlDesigner;
                        if (boxDesigner == null) { return snaplines; }

                        foreach (SnapLine i in boxDesigner.SnapLines) {
                            if (i.SnapLineType == SnapLineType.Baseline) {
                                snaplines.Add(new SnapLine(SnapLineType.Baseline, i.Offset + fbc.inputFileBox.Top, i.Filter, i.Priority));
                            }
                        }
                    }
                    return snaplines;
                }
            }
        }
    }
}
