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
        private readonly TreeNode root = new TreeNode("<root>");

        public ObjectsPanel()
        {
            InitializeComponent();

            treeView.Nodes.Clear();
            treeView.Nodes.Add(root);
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
        ///
        /// Secondly, this method preserves TreeNodes whenever possible. Rather
        /// than clearing the tree and rebuilding it (which would be the
        /// simplest implementation), the nodes are rearranged and only
        /// deleted if they no longer appear in the model. The reason for this
        /// is to maintain their state - having the entire tree collapse whenever
        /// it is reloaded would be very annoying.
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
                try
                {
                    ModelScript.Execute(data, scriptFile.Selected);
                }
                catch (Exception exc)
                {
                    // TODO display the errors in a less intrusive way
                    Log.Default.Warn("Exception in script file: {0}", exc);
                    MessageBox.Show("There was an error in the script file - see console");
                    return;
                }
            }

            // Make a list of all the Object3Ds to include in the tree
            // TODO also include Model-s, which in DieselX are a subclass of Object3D
            List<Object3D> objs = data.parsed_sections.Values
                .Select(a => a as Object3D)
                .Where(a => a != null)
                .ToList();

            // Make a set with all the IDs of the nodes currently in the tree. As we
            // walk through the objects in the file, we remove them from this set. After
            // we've done everything else, we remove the nodes still in this set.
            //
            // This will ensure nodes that used to be in the model but are not anymore will
            // be deleted.
            HashSet<uint> unused = new HashSet<uint>(nodes.Keys);

            // Walk through and create a node (if it does not already exist) for each object.
            // Do this before attaching them to their parents, as otherwise the parent might
            // not exist when we build one if it's children.
            foreach (Object3D obj in objs)
            {
                if (!nodes.ContainsKey(obj.id))
                    nodes[obj.id] = new TreeNode();
                unused.Remove(obj.id);
            }

            // Set each object's label, and move it to the correct parent if needed.
            foreach (Object3D obj in objs)
            {
                TreeNode node = nodes[obj.id];
                TreeNode parent = obj.parent == null ? root : nodes[obj.parent.id];

                node.Text = obj.Name;

                if (node.Parent == parent) continue;

                node.Remove();
                parent.Nodes.Add(node);
            }

            // Remove any unused nodes. That is, nodes that existed on the last-viewed model
            // that do not exist on the current model.
            foreach (uint id in unused)
            {
                nodes[id].Remove();
                nodes.Remove(id);
            }
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
    }
}
