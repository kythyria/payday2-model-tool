using PD2ModelParser.Sections;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows.Forms;

using Directory = System.IO.Directory;

namespace PD2ModelParser.UI
{
    public partial class ObjectsPanel : UserControl
    {
        private readonly Dictionary<uint, TreeNode> nodes = new Dictionary<uint, TreeNode>();
        private readonly ContextMenuStrip nodeRightclickMenu;
        private TreeNode menuTarget;
        private FullModelData data;

        public ObjectsPanel()
        {
            InitializeComponent();

            treeView.Nodes.Clear();

            nodeRightclickMenu = new ContextMenuStrip();

            ToolStripButton properties = new ToolStripButton("Properties");
            properties.Click += optProperties_Click;
            nodeRightclickMenu.Items.Add(properties);
        }

        /// <summary>
        /// Reload the tree view to reflect any new settings
        /// </summary>
        /// <remarks>
        /// This method reloads the model file each time it is
        /// called. While this may sound slow, particularly if you've
        /// seen how long it takes the tool to load a model when you
        /// first drag it in, it is <b>much</b> quicker on subsequent
        /// runs.
        /// </remarks>
        private void Reload()
        {
            data = null;
            var script = new List<Modelscript.IScriptItem>();
            if(modelFile.Selected != null)
            {
                script.Add(new Modelscript.LoadModel() { File = modelFile.Selected });
            }
            else
            {
                script.Add(new Modelscript.NewModel());
            }
            btnSave.Enabled = !showScriptChanges.Checked;

            if(scriptFile.Selected != null && showScriptChanges.Checked)
            {
                script.Add(new Modelscript.RunScript() { File = scriptFile.Selected });
            }

            // TODO: There must be a better way to deal with errors.
            bool success = Modelscript.Script.ExecuteWithMsgBox(script, Directory.GetCurrentDirectory(), ref data);
            if (!success)
                return;

            var rootinspector = new Inspector.ModelRootNode(data);
            ReconcileChildNodes(rootinspector, treeView.Nodes);
        }

        private void showScriptChanges_CheckedChanged(object sender, EventArgs e)
        {
            Reload();
        }

        private void btnReload_Click(object sender, EventArgs e)
        {
            Reload();
        }

        private void fileBrowserControl2_FileSelected(object sender, EventArgs e)
        {
            Reload();
        }

        private void fileBrowserControl1_FileSelected(object sender, EventArgs e)
        {
            Reload();
        }

        private void treeView_NodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            propertyGrid1.SelectedObject = e.Node.Tag;
            // Only process right clicks
            if (e.Button != MouseButtons.Right)
                return;

            // No properties for the root node
            if (e.Node.Tag == null)
                return;

            menuTarget = e.Node;
            nodeRightclickMenu.Show(treeView, e.Location);
        }

        private void optProperties_Click(object sender, EventArgs e)
        {
            var obj = menuTarget.Tag;

            propertyGrid1.SelectedObject = obj;
        }

        /// <summary>
        /// Merges the treeview nodes implied by an IInspectorNode into an existing tree.
        /// </summary>
        /// <remarks>
        /// <para>
        /// We use a TreeNodeCollection here to avoid the roots of the tree being special.
        /// The root of the inspector node tree corresponds to the treeview as a whole and
        /// is never rendered. But that's really a consideration for the caller.
        /// </para><para>
        /// Anyway, this method preserves the existing nodes if they match according to the
        /// Key member of the inspector node and the name of the treeview node. Keys only
        /// actually NEED to be 
        /// </para>
        /// </remarks>
        private void ReconcileChildNodes(Inspector.IInspectorNode modelNode, TreeNodeCollection viewNodes)
        {
            var newModels = modelNode.GetChildren().ToList();

            var existingKeys = new HashSet<string>(viewNodes.OfType<TreeNode>().Select(i=>i.Name));
            var newKeys = new HashSet<string>(newModels.Select(i => i.Key));

            var toRemove = new HashSet<string>(existingKeys);
            toRemove.ExceptWith(newKeys);
            foreach(var i in toRemove)
            {
                viewNodes.RemoveByKey(i);
            }

            List<TreeNode> toAdd = new List<TreeNode>(newKeys.Count);
            foreach(var i in newModels)
            {
                TreeNode[] mn = viewNodes.Find(i.Key, false);
                TreeNode n;
                if(mn.Length == 0) { n = new TreeNode(); toAdd.Add(n); }
                else { n = mn[0]; }
                n.Name = i.Key;
                n.Tag =  i.PropertyItem;
                n.Text = i.Label;
                ReconcileChildNodes(i, n.Nodes);
            }
            viewNodes.AddRange(toAdd.ToArray());
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            var script = new List<Modelscript.IScriptItem>()
            {
                new Modelscript.SaveModel() { File = modelFile.Selected }
            };
            // TODO: There must be a better way to deal with errors.
            bool success = Modelscript.Script.ExecuteWithMsgBox(script, Directory.GetCurrentDirectory(), ref data);
            if (!success)
                return;
        }
    }
}
