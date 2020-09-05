using System;
using System.Collections.Generic;
using SN = System.Numerics;
using FbxNet;
using Nexus;
using PD2ModelParser.Misc;
using PD2ModelParser.Sections;

namespace PD2ModelParser.Exporters
{
    static class FbxExporter
    {
        private static FbxManager fm;

        public static string ExportFile(FullModelData data, string path)
        {
            path = path.Replace(".model", ".fbx");

            if (fm == null)
            {
                fm = FbxManager.Create();

                FbxIOSettings io = FbxIOSettings.Create(fm, "fbx_root");
                fm.SetIOSettings(io);
            }

            FbxScene scene = FbxScene.Create(fm, "Scene");

            FbxDocumentInfo scene_info = FbxDocumentInfo.Create(fm, "SceneInfo");
            scene.SetSceneInfo(scene_info);

            // Add the scene contents
            AddModelContents(scene, data);

            // Find the ID of the FBX-ASCII filetype
            int file_format = -1;
            FbxIOPluginRegistry io_pr = fm.GetIOPluginRegistry();
            for (int i = 0; i < io_pr.GetWriterFormatCount(); i++)
            {
                if (!io_pr.WriterIsFBX(i))
                    continue;
                string desc = io_pr.GetWriterFormatDescription(i);
                // if (!desc.Contains("ascii")) continue;
                if (!desc.Contains("binary")) continue;
                // Console.WriteLine(desc);
                file_format = i;
                break;
            }

            // Save the scene
            FbxNet.FbxExporter ex = FbxNet.FbxExporter.Create(fm, "Exporter");
            ex.Initialize(path, file_format, fm.GetIOSettings());
            ex.Export(scene);

            return path;
        }

        private static void AddModelContents(FbxScene scene, FullModelData data)
        {
            // Find all the Object3Ds that are actually part of an object
            var model_objects = new HashSet<Object3D>(data.SectionsOfType<Model>());

            Dictionary<Object3D, SkeletonInfo> skeletonsObj = new Dictionary<Object3D, SkeletonInfo>();

            foreach (SectionHeader section_header in data.sections)
            {
                if (section_header.type != Tags.model_data_tag)
                    continue;

                Model model = (Model) data.parsed_sections[section_header.id];
                if (model.version == 6)
                    continue;

                ModelInfo mesh = AddModel(data, model);

                if (model.SkinBones == null)
                {
                    // If there's no corresponding skeleton, remove the 'Object' suffix
                    mesh.Node.SetName(model.Name);

                    scene.GetRootNode().AddChild(mesh.Node);
                    continue;
                }

                SkinBones sb = model.SkinBones;

                SkeletonInfo bones;
                if (skeletonsObj.ContainsKey(sb.ProbablyRootBone))
                {
                    bones = skeletonsObj[sb.ProbablyRootBone];
                }
                else
                {
                    bones = AddSkeleton(data, sb, model_objects);
                    skeletonsObj[sb.ProbablyRootBone] = bones;

                    // Make one root node to contain the skeleton, and all the models
                    FbxNode root = FbxNode.Create(fm, bones.Root.Game.Name + "_RigRoot");
                    root.AddChild(bones.Root.Node);
                    scene.GetRootNode().AddChild(root);
                    bones.RigRoot = root;

                    // Add a root skeleton node. THis must be in the model's parent, otherwise Blender won't
                    // set up the armatures correctly (or at all, actually).
                    FbxSkeleton skeleton = FbxSkeleton.Create(fm, "");
                    skeleton.SetSkeletonType(FbxSkeleton.EType.eRoot);
                    root.SetNodeAttribute(skeleton);
                }

                bones.RigRoot.AddChild(mesh.Node);

                // Add the skin weights, which bind the model onto the bones
                AddWeights(data, model, sb, mesh.Mesh, bones.Nodes);
            }
        }

        private static ModelInfo AddModel(FullModelData data, Model model)
        {
            Dictionary<uint, ISection> parsed = data.parsed_sections;
            PassthroughGP pgp = model.PassthroughGP;
            Geometry geom = pgp.Geometry;
            Topology topo = pgp.Topology;

            string name = model.Name;

            FbxNode mesh_node = FbxNode.Create(fm, name + "Object");
            FbxMesh mesh = FbxMesh.Create(fm, name + "Mesh");
            mesh_node.SetNodeAttributeGeom(mesh);

            CopyTransform(model.WorldTransform, mesh_node);

            FbxLayerElementNormal normals = mesh.CreateElementNormal();
            normals.SetReferenceMode(FbxLayerElement.EReferenceMode.eIndexToDirect);
            normals.SetMappingMode(FbxLayerElement.EMappingMode.eByControlPoint);

            mesh.InitControlPoints(geom.verts.Count);
            FbxVector4 temp = new FbxVector4();
            for (int i = 0; i < geom.verts.Count; i++)
            {
                temp.Set(geom.verts[i]);
                mesh.SetControlPointAt(temp, i);

                if (geom.normals.Count <= 0) continue;
                temp.Set(geom.normals[i]);
                mesh.SetControlPointNormalAt(temp, i);
            }

            foreach (Face face in topo.facelist)
            {
                mesh.BeginPolygon();
                mesh.AddPolygon(face.a);
                mesh.AddPolygon(face.b);
                mesh.AddPolygon(face.c);
                mesh.EndPolygon();
            }

            // Export the UVs
            for (int i = 0; i < geom.UVs.Length; i++)
            {
                AddUVs(mesh, "UV" + i, geom.UVs[i]);
            }

            if (geom.vertex_colors.Count > 0)
            {
                FbxLayerElementVertexColor colours = mesh.CreateElementVertexColor();
                colours.SetMappingMode(FbxLayerElement.EMappingMode.eByControlPoint);
                colours.SetReferenceMode(FbxLayerElement.EReferenceMode.eDirect);
                for (int i = 0; i < geom.vert_count; i++)
                {
                    GeometryColor c = geom.vertex_colors[i];
                    colours.mDirectArray.Add(new FbxColor(
                        c.red / 255.0,
                        c.green / 255.0,
                        c.blue / 255.0,
                        c.alpha / 255.0
                    ));
                }
            }

            return new ModelInfo
            {
                Model = model,
                Mesh = mesh,
                Node = mesh_node,
            };
        }

        private static void AddUVs(FbxMesh mesh, string name, List<Vector2D> uvs)
        {
            if (uvs.Count == 0)
                return;

            FbxLayerElementUV uv = mesh.CreateElementUV(name);
            uv.SetMappingMode(FbxLayerElement.EMappingMode.eByControlPoint);
            uv.SetReferenceMode(FbxLayerElement.EReferenceMode.eDirect);
            for (int i = 0; i < uvs.Count; i++)
            {
                uv.GetDirectArray().Add(uvs[i].ToFbxV2());
            }
        }

        private static SkeletonInfo AddSkeleton(FullModelData data, SkinBones bones, HashSet<Object3D> exclude)
        {
            Dictionary<uint, ISection> parsed = data.parsed_sections;
            Dictionary<Object3D, BoneInfo> bone_maps = new Dictionary<Object3D, BoneInfo>();
            Object3D root = bones.ProbablyRootBone;
            BoneInfo root_bone = AddBone(root, bone_maps, exclude, bones);
            return new SkeletonInfo
            {
                Nodes = bone_maps,
                Root = root_bone,
                SkinBones = bones,
            };
        }

        private static BoneInfo AddBone(Object3D obj, Dictionary<Object3D, BoneInfo> bones, HashSet<Object3D> exclude,
            SkinBones sb)
        {
            string name = obj.Name;

            // If it's not part of the SkinBones object list, then it's a locator that vertices can't bind to
            // This will be read later when importing
            if (!sb.Objects.Contains(obj))
            {
                name += FbxUtils.LocatorSuffix;
            }

            FbxNode node = FbxNode.Create(fm, name);

            CopyTransform(obj.Transform, node);

            FbxSkeleton skel = FbxSkeleton.Create(fm, obj.Name + "Skel");
            skel.Size.Set(1);
            skel.SetSkeletonType(FbxSkeleton.EType.eLimbNode);
            node.SetNodeAttribute(skel);

            foreach (Object3D child in obj.children)
            {
                if (exclude.Contains(child))
                    continue;

                BoneInfo n = AddBone(child, bones, exclude, sb);
                node.AddChild(n.Node);
            }

            BoneInfo info = new BoneInfo
            {
                Game = obj,
                Node = node,
                Skeleton = skel,
            };

            bones[obj] = info;

            return info;
        }

        private static void AddWeights(FullModelData data, Model model, SkinBones sb,
            FbxMesh mesh, IReadOnlyDictionary<Object3D, BoneInfo> bones)
        {
            Dictionary<uint, ISection> parsed = data.parsed_sections;
            PassthroughGP pgp = model.PassthroughGP;
            Geometry geom = pgp.Geometry;

            // Mainly for testing stuff with bone exports, keep things working if
            // the model has a skeleton but no weights.
            if (geom.weights.Count == 0)
                return;

            FbxSkin skin = FbxSkin.Create(fm, model.Name + "Skin");
            mesh.AddDeformer(skin);

            for (int bone_idx = 0; bone_idx < sb.count; bone_idx++)
            {
                Object3D obj = sb.Objects[bone_idx];

                FbxCluster cluster = FbxCluster.Create(fm, "");
                cluster.SetLink(bones[obj].Node);
                cluster.SetLinkMode(FbxCluster.ELinkMode.eNormalize);

                // This is all AFAIK, but here's what I'm pretty sure this is doing
                // SetTransformMatrix registers the transform of the mesh
                // While SetTransformLinkMatrix binds it to the transform of the bone
                FbxAMatrix ident = new FbxAMatrix();
                ident.SetIdentity();
                cluster.SetTransformMatrix(ident);

                // Break down the bone's transform and convert it to an FBX affine matrix
                // Skip the scale for now though, we don't need it
                SN.Matrix4x4.Decompose(obj.WorldTransform, out SN.Vector3 _, out SN.Quaternion rotate, out SN.Vector3 translate);
                FbxAMatrix mat = new FbxAMatrix();
                mat.SetIdentity();
                mat.SetT(new FbxVector4(translate.X, translate.Y, translate.Z));
                mat.SetQ(new FbxQuaternion(rotate.X, rotate.Y, rotate.Z, rotate.W));

                // And lode that in as the bone (what it's linked to) transform matrix
                cluster.SetTransformLinkMatrix(mat);

                for (int i = 0; i < geom.verts.Count; i++)
                {
                    GeometryWeightGroups groups = geom.weight_groups[i];
                    Vector3D weights = geom.weights[i];
                    float weight;

                    int bi = bone_idx;

                    if (groups.Bones1 == bi)
                        weight = weights.X;
                    else if (groups.Bones2 == bi)
                        weight = weights.Y;
                    else if (groups.Bones3 == bi)
                        weight = weights.Z;
                    else if (groups.Bones4 == bi && bi != 0)
                        throw new Exception("Unsupported Bone4 weight - not in weights");
                    else
                        continue;

                    if (weight < 0.00001f)
                        continue;

                    cluster.AddControlPointIndex(i, weight);
                }

                if (!skin.AddCluster(cluster))
                    throw new Exception();
            }
        }

        private static void CopyTransform(Matrix4x4 transform, FbxNode node)
        {
            SN.Vector3 translate;
            SN.Quaternion rotate;
            SN.Vector3 scale;
            SN.Matrix4x4.Decompose(transform, out scale, out rotate, out translate);

            node.LclTranslation.Set(new FbxDouble3(translate.X, translate.Y, translate.Z));

            // FbxQuaternion fq = new FbxQuaternion(rotate.X, rotate.Y, rotate.Z, rotate.W);
            // node.LclRotation.Set(fq.DecomposeSphericalXYZ().D3());

            node.RotationOrder.Set(FbxEuler.EOrder.eOrderZYX);
            SN.Vector3 euler = rotate.ToEulerZYX() * (180 / (float) Math.PI);
            node.LclRotation.Set(euler.ToFbxD3());
        }

        private struct ModelInfo
        {
            public Model Model;
            public FbxMesh Mesh;
            public FbxNode Node;
        }

        private struct BoneInfo
        {
            public Object3D Game;
            public FbxNode Node;
            public FbxSkeleton Skeleton;
        }

        private class SkeletonInfo
        {
            public SkinBones SkinBones;
            public Dictionary<Object3D, BoneInfo> Nodes;
            public BoneInfo Root;
            public FbxNode RigRoot;
        }
    }
}
