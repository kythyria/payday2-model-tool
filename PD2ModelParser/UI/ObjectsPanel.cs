using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using PD2ModelParser.Sections;

namespace PD2ModelParser.UI
{
    public partial class ObjectsPanel : UserControl
    {
        private readonly Dictionary<uint, TreeNode> nodes = new Dictionary<uint, TreeNode>();
        private readonly ContextMenu nodeRightclickMenu;
        private TreeNode menuTarget;

        public ObjectsPanel()
        {
            InitializeComponent();

            treeView.Nodes.Clear();

            nodeRightclickMenu = new ContextMenu();

            MenuItem properties = new MenuItem("Properties");
            properties.Click += optProperties_Click;
            nodeRightclickMenu.MenuItems.Add(properties);
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
            // Load in the model file, if selected
            FullModelData data = modelFile.Selected != null
                ? ModelReader.Open(modelFile.Selected)
                : new FullModelData();

            // Apply the script, if selected and enabled
            if (scriptFile.Selected != null && showScriptChanges.Checked)
            {
                // TODO display the errors in a less intrusive way
                bool success = Modelscript.Script.ExecuteFileWithMsgBox(ref data, scriptFile.Selected);
                if (!success)
                    return;
            }

            var sb = data.SectionsOfType<SkinBones>().ToList();

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

            // TODO actually show some useful information
            //MessageBox.Show(obj.Name);
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
    }
}
