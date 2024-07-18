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

            ContextMenuStrip cm = new ContextMenuStrip();
            ContextMenuStrip = cm;
            ToolStripItem clearItem = cm.Items.Add("Clear");
            clearItem.Click += ClearFileSelected;
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

        public bool MultiFile { get; set; }

        public string Filter { get; set; }

        public string Selected
        {
            get
            {
                if (AllSelected.Count > 0)
                {
                    return AllSelected[0];
                }
                return null;
            }
            set
            {
                AllSelected.Clear();
                AllSelected.Add(value);
            }
        }

        private List<string> _allSelected = new List<string>();
        public List<string> AllSelected
        {
            get
            {
                string text = inputFileBox.Text;
                if (text == "")
                {
                    _allSelected.Clear();
                    return _allSelected;
                }

                if (SaveMode)
                {
                    DirectoryInfo info = Directory.GetParent(text);
                    if (!info.Exists)
                    {
                        _allSelected.Clear();
                        return _allSelected;
                    }
                }
                else
                {
                    if (!MultiFile && !File.Exists(text))
                    {
                        _allSelected.Clear();
                        return _allSelected;
                    }

                    if (MultiFile)
                    {
                        _allSelected = inputFileBox.Text.Split(';').ToList();
                        foreach (string filepath in _allSelected)
                        {
                            if (!File.Exists(filepath))
                            {
                                _allSelected.Clear();
                                return _allSelected;
                            }
                        }
                    }
                }

                return _allSelected;
            }
            private set
            {
                if (value == null)
                {
                    value = new List<string>();
                }

                inputFileBox.Text = string.Join(";", value) ?? "";
                _allSelected = value;
                FileSelected?.Invoke(this, new EventArgs());
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

                if (MultiFile)
                {
                    ((OpenFileDialog)fileDialog).Multiselect = true;
                }
            }

            fileDialog.Filter = Filter; // "Diesel Model(*.model)|*.model";

            if (fileDialog.ShowDialog() != DialogResult.OK) return;

            AllSelected = fileDialog.FileNames.ToList();
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
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);

            if (!MultiFile && files.Length != 1)
                return;

            if (!files.All(File.Exists)) { return; }

            // Check the file against the set filters.
            //
            // For each allowable filter:

            MatchCollection matches = filterRegex.Matches(Filter);
            var extensions = matches
                .SelectMany(match => match.Groups["Extension"].Value.Split(';'))
                .Select(ext => ext.Split('.'))
                .Select(ext => ext.Length == 2 ? ext[1] : "")
                .Where(ext => ext != "")
                .ToList();

            var success = extensions.Contains("*");
            success |= files.Select(f => f.Split('.'))
                .All(p => p.Length >= 2 && extensions.Contains(p.Last()));

            if (success)
            {
                // We are effectively copying the file in
                e.Effect = DragDropEffects.Copy;
            }
        }

        private void HandleDragDrop(object sender, DragEventArgs e)
        {
            // This is only called if we set the effect in HandleDragEnter, so
            // this won't be called if the user is dragging in a directory or anything.
            String[] files = (String[])e.Data.GetData(DataFormats.FileDrop);

            if (!MultiFile && files.Length != 1)
                return;

            AllSelected = files.ToList();
        }

        private void ClearFileSelected(object sender, EventArgs e)
        {
            AllSelected.Clear();
            this.inputFileBox.Clear();
        }

        protected override void SetBoundsCore(int x, int y, int width, int height, BoundsSpecified specified)
        {
            base.SetBoundsCore(x, y, width, Math.Max(inputFileBox.Height, browseBttn.Height), specified);
        }

        class FileBrowserDesigner : ControlDesigner
        {
            public FileBrowserDesigner()
            {
                base.AutoResizeHandles = true;
            }

            public override SelectionRules SelectionRules => SelectionRules.LeftSizeable | SelectionRules.RightSizeable | SelectionRules.Moveable;

            public override IList SnapLines
            {
                get
                {
                    // https://stackoverflow.com/questions/93541/baseline-snaplines-in-custom-winforms-controls
                    // This isn't pretty, but it works.

                    var snaplines = base.SnapLines;
                    var fbc = Control as FileBrowserControl;
                    if (fbc == null) { return snaplines; }

                    var designer = TypeDescriptor.CreateDesigner(fbc.inputFileBox, typeof(IDesigner));
                    designer.Initialize(fbc.inputFileBox);

                    using (designer)
                    {
                        var boxDesigner = designer as ControlDesigner;
                        if (boxDesigner == null) { return snaplines; }

                        foreach (SnapLine i in boxDesigner.SnapLines)
                        {
                            if (i.SnapLineType == SnapLineType.Baseline)
                            {
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
