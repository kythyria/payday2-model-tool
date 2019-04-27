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
                // TODO display the errors in a less intrusive way
                bool success = ModelScript.ExecuteHandled(data, scriptFile.Selected);
                if (!success)
                    return;
            }

            // Make a list of all the Object3Ds and Models to include in the tree
            List<ObjectOrModel> objs = new List<ObjectOrModel>();

            objs.AddRange(data.parsed_sections.Values
                .Select(a => a as Object3D)
                .Where(a => a != null)
                .Select(a => new ObjectOrModel(a)));

            objs.AddRange(data.parsed_sections.Values
                .Select(a => a as Model)
                .Where(a => a != null)
                .Select(a => new ObjectOrModel(a)));

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
            foreach (ObjectOrModel obj in objs)
            {
                if (!nodes.ContainsKey(obj.id))
                    nodes[obj.id] = new TreeNode();
                unused.Remove(obj.id);
            }

            // Set each object's label, and move it to the correct parent if needed.
            foreach (ObjectOrModel obj in objs)
            {
                TreeNode node = nodes[obj.id];
                TreeNode parent = obj.parent == null ? root : nodes[obj.parent.id];

                if (obj.model == null)
                    node.Text = obj.obj.Name;
                else
                    node.Text = obj.obj.Name + " (model)";

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

        private class ObjectOrModel
        {
            public readonly Object3D obj;
            public readonly Model model; // may be null

            public uint id => obj.id;

            public Object3D parent => obj.parent;

            public ObjectOrModel(Object3D obj)
            {
                this.obj = obj;
            }

            public ObjectOrModel(Model model)
            {
                this.model = model;
                this.obj = model.object3D;
            }
        }
    }
}
