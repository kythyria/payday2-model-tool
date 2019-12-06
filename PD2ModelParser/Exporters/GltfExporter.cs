using PD2ModelParser.Sections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GLTF = SharpGLTF.Schema2;

namespace PD2ModelParser.Exporters
{
    class GltfExporter
    {
        public static string ExportFile(FullModelData data, string path)
        {
            var exporter = new GltfExporter();
            var gltfmodel = exporter.Convert(data);
            gltfmodel.SaveGLB(path);

            return path;
        }

        GLTF.ModelRoot root;
        GLTF.Scene scene;
        Dictionary<uint, Nodeoid> nodeoidsByObjectId;

        GLTF.ModelRoot Convert(FullModelData data)
        {
            nodeoidsByObjectId = new Dictionary<uint, Nodeoid>();

            root = SharpGLTF.Schema2.ModelRoot.CreateModel();
            scene = root.UseScene(0);

            var rootNodeoids = GenerateNodeoids(data);
            AddModelNodes(data);
            foreach(var i in rootNodeoids)
            {
                i.HoistMeshChild();
                CreateNodeFromNodeoid(i, scene);
            }

            return root;
        }

        void CreateNodeFromNodeoid(Nodeoid thing, GLTF.IVisualNodeContainer parent)
        {
            var node = parent.CreateNode(thing.TryGetName());
            if(thing.ObjectItem != null)
            {
                node.LocalMatrix = thing.ObjectItem.rotation.ToMatrix4x4();
            }
            if(thing.Model != null)
            {
                
            }
            foreach (var i in thing.Children)
            {
                CreateNodeFromNodeoid(i, node);
            }
        }

        List<Nodeoid> GenerateNodeoids(FullModelData data)
        {
            return data.parsed_sections
                .Where(i => i.Value is Object3D)
                .Select(i => (Object3D)i.Value)
                .Where(i => i.parent == null)
                .Select(GenerateNodeoid)
                .ToList();
        }

        Nodeoid GenerateNodeoid(Object3D whatfor)
        {
            var n = new Nodeoid { ObjectItem = whatfor };
            n.Children.AddRange(whatfor.children.Select(GenerateNodeoid));
            nodeoidsByObjectId[whatfor.id] = n;
            return n;
        }

        void AddModelNodes(FullModelData data)
        {
            var models = data.parsed_sections
                .Where(i => i.Value is Model)
                .Select(i => (Model)i.Value)
                .Where(i => i.object3D != null);
            foreach (var i in models)
            {
                var parentNode = nodeoidsByObjectId[i.object3D.id];
                var modelNode = new Nodeoid { Model = i };
                parentNode.Children.Add(modelNode);
            }
        }

        class Nodeoid
        {
            public Nodeoid()
            {
                Children = new List<Nodeoid>();
            }

            public List<Nodeoid> Children { get; set; }
            public Object3D ObjectItem { get; set; }
            public Model Model { get; set; }

            public void HoistMeshChild()
            {
                var meshChildren = Children.Where(i => i.Model != null && i.ObjectItem == null).ToList();
                if(meshChildren.Count == 1)
                {
                    Model = meshChildren[0].Model;
                    Children.Remove(meshChildren[0]);
                }
                foreach (var i in Children)
                {
                    i.HoistMeshChild();
                }
            }

            public string TryGetName()
            {
                if(ObjectItem != null)
                {
                    return ObjectItem.Name;
                }
                else if(Model?.object3D != null)
                {
                    return Model.object3D.Name;
                }
                else
                {
                    return null;
                }
            }
        }
    }
}
