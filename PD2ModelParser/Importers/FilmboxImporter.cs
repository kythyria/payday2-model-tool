using System;
using System.Collections.Generic;
using System.Linq;
using FbxNet;
using Nexus;
using PD2ModelParser.Misc;
using PD2ModelParser.Sections;

namespace PD2ModelParser.Importers
{
    public static class FilmboxImporter
    {
        private static FbxManager _fm;

        public static void Import(FullModelData data, string filepath, bool addNew,
            Func<object, Object3D> rootPointResolver)
        {
            if (_fm == null)
                _fm = FbxManager.Create();

            FbxImporter importer = FbxImporter.Create(_fm, "");
            bool result = importer.Initialize(filepath);
            if (!result)
                throw new Exception("Cannot load FBX file");

            FbxScene scene = FbxScene.Create(_fm, "");
            result = importer.Import(scene);
            // TODO add FbxIOBase to FbxNet so we can access the error code
            if (!result)
                throw new Exception("Cannot import FBX file");

            List<FbxNode> meshes = new List<FbxNode>();
            RecurseMeshes(scene.GetRootNode(), meshes);

            foreach (FbxNode node in meshes)
            {
                Console.WriteLine(node.GetName());
                FbxMesh mesh = node.GetMesh();

                // FbxDeformer deformer = mesh.GetDeformer(0); // TODO multiple deformers?

                Object3D parent = rootPointResolver.Invoke(node);

                AddMesh(data, parent, node, mesh);
            }
        }

        private static Model AddMesh(FullModelData data, Object3D parent, FbxNode node, FbxMesh mesh)
        {
            // The basic geometry information - vertices, normals, UVs, but notably no faces
            Geometry geom = BuildGeometry(mesh);
            data.AddSection(geom);

            // Faces
            Topology topo = BuildTopology(mesh, node.GetName());
            data.AddSection(topo);

            // Weird wrappers
            TopologyIP tip = new TopologyIP(0, topo);
            data.AddSection(tip);
            PassthroughGP pgp = new PassthroughGP(0, geom, topo);
            data.AddSection(pgp);

            // Material information
            // TODO material setup
            Material mat = new Material(0, "");
            data.AddSection(mat);
            Material_Group mat_g = new Material_Group(0, mat.id);
            data.AddSection(mat_g);

            // Used for some internal model stuff
            obj_data fake_obj = new obj_data
            {
                object_name = node.GetName(),
                verts = geom.verts,
                faces = topo.facelist,
            };

            // Build the model itself
            Model model = new Model(fake_obj, pgp, tip, mat_g, parent);
            data.AddSection(model);

            // Add the bones - note this *only* adds the skeleton, and not any weights
            SkinBones sb = AddSkeleton(node, data);
            if (sb == null)
                return model;

            model.skinbones_ID = sb.id;

            return model;
        }

        private static SkinBones AddSkeleton(FbxNode meshNode, FullModelData data)
        {
            FbxSkeleton root_skeleton = meshNode.GetParent()?.GetSkeleton();
            if (root_skeleton == null)
                return null;

            Object3D root = new Object3D(meshNode.GetName() + "Skel", null);
            data.AddSection(root);

            SkinBones sb = new SkinBones(0)
            {
                probably_root_bone = root.id,
            };
            data.AddSection(sb);

            Recurse(meshNode.GetParent(), root, (node, parent) =>
            {
                if (node.GetSkeleton() == null)
                    return parent;

                Object3D obj = new Object3D(node.GetName(), parent);
                parent.children.Add(obj);
                data.AddSection(obj);

                // Note the field is named badly - it's a transform, not just a rotation
                obj.rotation = node.GetNexusTransform();

                sb.objects.Add(obj.id);
                // TODO sb.rotations
                // TODO sb.bones.bones

                return obj;
            });

            // TODO setup the other SkinBones fields - probably very important for Diesel
            return sb;
        }

        private static Geometry BuildGeometry(FbxMesh mesh)
        {
            Geometry geom = new Geometry(0)
            {
                vert_count = (uint) mesh.GetControlPointsCount(),
            };

            // TODO cleanup
            geom.headers.Add(new GeometryHeader(3, GeometryChannelTypes.POSITION)); // vert
            geom.headers.Add(new GeometryHeader(2, GeometryChannelTypes.TEXCOORD0)); // uv
            geom.headers.Add(new GeometryHeader(3, GeometryChannelTypes.NORMAL0)); // norm
            geom.headers.Add(new GeometryHeader(3, GeometryChannelTypes.BINORMAL0)); // unk20
            geom.headers.Add(new GeometryHeader(3, GeometryChannelTypes.TANGENT0)); // unk21

            for (int i = 0; i < mesh.GetControlPointsCount(); i++)
            {
                FbxVector4 v = mesh.GetControlPointAt(i);
                geom.verts.Add(v.V3());

                // TODO Normals
                geom.normals.Add(Vector3D.Zero);

                // TODO UVs
                geom.uvs.Add(Vector2D.Zero);

                // Normally I don't care about leaving stuff around as it'll be cleaned
                // up when the C# GC eats it, but in this case it might be a bit too much.
                v.Dispose();
            }

            AddVertexColours(mesh, geom);

            return geom;
        }

        private static void AddVertexColours(FbxMesh mesh, Geometry geom)
        {
            int vertex_colour_count = mesh.GetElementVertexColorCount();
            if (vertex_colour_count > 1)
                throw new Exception("The model tool does not support more than one vertex colour layer");

            if (vertex_colour_count == 0) return;

            // TODO confirm the size is indeed 3
            geom.headers.Add(new GeometryHeader(3, GeometryChannelTypes.COLOR));
            FbxLayerElementVertexColor layer = mesh.GetElementVertexColor();

            if (layer.GetMappingMode() != FbxLayerElement.EMappingMode.eByControlPoint)
                throw new Exception("Vertex colour: only per-vertex colouring is supported");

            if (layer.GetReferenceMode() != FbxLayerElement.EReferenceMode.eDirect)
                throw new Exception("Vertex colour: only per-vertex colouring is supported");

            FbxLayerElementArrayTemplateColour array = layer.GetDirectArray();

            if (array.GetCount() != geom.vert_count)
                throw new Exception("Vertex colour: mismatched vertex count");

            for (int i = 0; i < array.GetCount(); i++)
            {
                FbxColor colour = array.GetAt(i);
                geom.vertex_colors.Add(colour.ToGeomColour());
            }
        }

        private static Topology BuildTopology(FbxMesh mesh, string name)
        {
            Topology topo = new Topology(0, name);

            for (int i = 0; i < mesh.GetPolygonCount(); i++)
            {
                int size = mesh.GetPolygonSize(i);
                if (size != 3)
                    throw new Exception("Triangles are the only supported type of polygon");

                int a = mesh.GetPolygonVertex(i, 0);
                int b = mesh.GetPolygonVertex(i, 1);
                int c = mesh.GetPolygonVertex(i, 2);
                // TODO verify the indices are within bounds
                Face f = new Face {a = (ushort) a, b = (ushort) b, c = (ushort) c};
                topo.facelist.Add(f);
            }

            return topo;
        }

        private static void RecurseMeshes(FbxNode root, List<FbxNode> meshes)
        {
            Recurse<object>(root, null, (node, ud) =>
            {
                FbxMesh mesh = node.GetMesh();
                if (mesh == null)
                    return null;

                meshes.Add(node);
                return null;
            });
        }

        private static void Recurse<T>(FbxNode node, T ud, Func<FbxNode, T, T> callback)
        {
            T sub = callback(node, ud);
            for (int i = 0; i < node.GetChildCount(); i++)
            {
                FbxNode child = node.GetChild(i);
                Recurse(child, sub, callback);
            }
        }
    }
}
