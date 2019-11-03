using System;
using System.Collections.Generic;
using FbxNet;
using Nexus;
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
            foreach (SectionHeader section_header in data.sections)
            {
                if (section_header.type != Tags.model_data_tag)
                    continue;

                Model model = (Model) data.parsed_sections[section_header.id];
                if (model.version == 6)
                    continue;

                ModelInfo mesh = AddModel(data, model);
                scene.GetRootNode().AddChild(mesh.Node);

                SkinBones sb = (SkinBones) data.parsed_sections[model.skinbones_ID];
                if (sb == null)
                    continue;

                Dictionary<Object3D, FbxNode> bones = AddSkeleton(scene, data, sb);
            }
        }

        private static ModelInfo AddModel(FullModelData data, Model model)
        {
            Dictionary<uint, object> parsed = data.parsed_sections;
            PassthroughGP pgp = (PassthroughGP) parsed[model.passthroughGP_ID];
            Geometry geom = (Geometry) parsed[pgp.geometry_section];
            Topology topo = (Topology) parsed[pgp.topology_section];

            string name = model.object3D.Name;

            FbxNode mesh_node = FbxNode.Create(fm, name + "Object");
            FbxMesh mesh = FbxMesh.Create(fm, name + "Mesh");
            mesh_node.SetNodeAttributeGeom(mesh);

            CopyTransform(model.object3D.world_transform, mesh_node);

            mesh.InitControlPoints(geom.verts.Count);
            FbxVector4 temp = new FbxVector4();
            for (int i = 0; i < geom.verts.Count; i++)
            {
                Vector3D vert = geom.verts[i];
                temp.Set(vert.X, vert.Y, vert.Z);
                mesh.SetControlPointAt(temp, i);
            }

            foreach (Face face in topo.facelist)
            {
                mesh.BeginPolygon();
                mesh.AddPolygon(face.a);
                mesh.AddPolygon(face.b);
                mesh.AddPolygon(face.c);
                mesh.EndPolygon();
            }

            return new ModelInfo
            {
                Model = model,
                Mesh = mesh,
                Node = mesh_node,
            };
        }

        private static Dictionary<Object3D, FbxNode> AddSkeleton(FbxScene scene, FullModelData data, SkinBones bones)
        {
            Dictionary<uint, object> parsed = data.parsed_sections;
            Dictionary<Object3D, FbxNode> bone_maps = new Dictionary<Object3D, FbxNode>();
            Object3D root = (Object3D) parsed[bones.probably_root_bone];
            FbxNode fbx_root = AddBone(root, bone_maps);
            scene.GetRootNode().AddChild(fbx_root);
            return bone_maps;
        }

        private static FbxNode AddBone(Object3D obj, Dictionary<Object3D, FbxNode> bones)
        {
            FbxNode node = FbxNode.Create(fm, obj.Name + "Bone");
            bones[obj] = node;

            CopyTransform(obj.rotation, node);

            foreach (Object3D child in obj.children)
            {
                FbxNode n = AddBone(child, bones);
                node.AddChild(n);
            }

            return node;
        }

        private static void CopyTransform(Matrix3D transform, FbxNode node)
        {
            Vector3D translate;
            Quaternion rotate;
            Vector3D scale;
            transform.Decompose(out scale, out rotate, out translate);

            node.LclTranslation.Set(new FbxDouble3(translate.X, translate.Y, translate.Z));

            // FbxQuaternion fq = new FbxQuaternion(rotate.X, rotate.Y, rotate.Z, rotate.W);
            // node.LclRotation.Set(fq.DecomposeSphericalXYZ().D3());

            node.RotationOrder.Set(FbxEuler.EOrder.eOrderZYX);
            Vector3D euler = rotate.ToEulerZYX() * (180 / (float) Math.PI);
            node.LclRotation.Set(euler.ToFbxD3());
        }

        private static FbxDouble3 D3(this FbxDouble4 vec)
        {
            SWIGTYPE_p_double data = vec.mData;
            double x = FbxNet.FbxNet.doubleArray_getitem(data, 0);
            double y = FbxNet.FbxNet.doubleArray_getitem(data, 1);
            double z = FbxNet.FbxNet.doubleArray_getitem(data, 2);
            return new FbxDouble3(x, y, z);
        }

        private static Vector3D V3(this FbxDouble4 vec)
        {
            SWIGTYPE_p_double data = vec.mData;
            double x = FbxNet.FbxNet.doubleArray_getitem(data, 0);
            double y = FbxNet.FbxNet.doubleArray_getitem(data, 1);
            double z = FbxNet.FbxNet.doubleArray_getitem(data, 2);
            return new Vector3D((float) x, (float) y, (float) z);
        }

        // ReSharper disable once InconsistentNaming
        private static Vector3D ToEulerZYX(this Quaternion q)
        {
            Vector3D angles;

            // roll (x-axis rotation)
            double sinr_cosp = +2.0 * (q.W * q.X + q.Y * q.Z);
            double cosr_cosp = +1.0 - 2.0 * (q.X * q.X + q.Y * q.Y);
            angles.X = (float) Math.Atan2(sinr_cosp, cosr_cosp);

            // pitch (y-axis rotation)
            double sinp = +2.0 * (q.W * q.Y - q.Z * q.X);
            if (Math.Abs(sinp) >= 1)
                angles.Y = (float) Math.PI / 2 * (sinp > 0 ? 1 : -1); // use 90 degrees if out of range
            else
                angles.Y = (float) Math.Asin(sinp);

            // yaw (z-axis rotation)
            double siny_cosp = +2.0 * (q.W * q.Z + q.X * q.Y);
            double cosy_cosp = +1.0 - 2.0 * (q.Y * q.Y + q.Z * q.Z);
            angles.Z = (float) Math.Atan2(siny_cosp, cosy_cosp);

            return angles;
        }

        private static FbxDouble3 ToFbxD3(this Vector3D v) => new FbxDouble3(v.X, v.Y, v.Z);

        private struct ModelInfo
        {
            public Model Model;
            public FbxMesh Mesh;
            public FbxNode Node;
        }
    }
}
