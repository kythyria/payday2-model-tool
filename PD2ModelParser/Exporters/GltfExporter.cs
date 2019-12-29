using PD2ModelParser.Sections;
using SharpGLTF.Memory;
using System;
using System.Collections.Generic;
using System.Linq;
using GLTF = SharpGLTF.Schema2;
using System.Numerics;

namespace PD2ModelParser.Exporters
{
    class GltfExporter
    {
        public static string ExportFile(FullModelData data, string path)
        {
            var exporter = new GltfExporter();
            var gltfmodel = exporter.Convert(data);
            gltfmodel.SaveGLTF(path, Newtonsoft.Json.Formatting.Indented);

            return path;
        }

        FullModelData data;
        GLTF.ModelRoot root;
        GLTF.Scene scene;
        Dictionary<uint, List<(string, GLTF.Accessor)>> vertexAttributesByGeometryId;
        Dictionary<uint, GLTF.Material> materialsBySectionId;

        GLTF.ModelRoot Convert(FullModelData data)
        {
            vertexAttributesByGeometryId = new Dictionary<uint, List<(string, GLTF.Accessor)>>();
            materialsBySectionId = new Dictionary<uint, GLTF.Material>();
            this.data = data;
            root = GLTF.ModelRoot.CreateModel();
            scene = root.UseScene(0);

            foreach (var ms in data.parsed_sections.Where(i => i.Value is Material).Select(i => i.Value as Material))
            {
                materialsBySectionId[ms.SectionId] = root.CreateMaterial(ms.hashname.String);
            }

            foreach(var i in data.SectionsOfType<Object3D>().Where(i => i.parent == null))
            {
                CreateNodeFromObject3D(i, scene);
            }

            //root.MergeBuffers();

            return root;
        }

        void CreateNodeFromObject3D(Object3D thing, GLTF.IVisualNodeContainer parent)
        {
            var node = parent.CreateNode(thing.Name);
            if (thing != null)
            {
                node.LocalMatrix = thing.rotation.ToMatrix4x4();
            }
            if (thing is Model)
            {
                node.Mesh = GetMeshForModel(thing as Model);
            }
            foreach (var i in thing.children)
            {
                CreateNodeFromObject3D(i, node);
            }
        }

        GLTF.Mesh GetMeshForModel(Model model)
        {
            if(!data.parsed_sections.ContainsKey(model.passthroughGP_ID))
            {
                return null;
            }

            var mesh = root.CreateMesh(model.Name);

            var secPassthrough = (PassthroughGP)data.parsed_sections[model.passthroughGP_ID];
            var geometry = (Geometry)data.parsed_sections[secPassthrough.geometry_section];
            var topology = (Topology)data.parsed_sections[secPassthrough.topology_section];
            var materialGroup = (Material_Group)data.parsed_sections[model.material_group_section_id];

            var attribs = GetGeometryAttributes(geometry);

            foreach (var (indexAccessor,material) in CreatePrimitiveIndices(topology, model.renderAtoms, materialGroup))
            {
                var prim = mesh.CreatePrimitive();
                prim.DrawPrimitiveType = GLTF.PrimitiveType.TRIANGLES;
                foreach (var att in attribs)
                {
                    prim.SetVertexAccessor(att.Item1, att.Item2);
                }

                prim.SetIndexAccessor(indexAccessor);
                if (material.Name != "Material: Default Material")
                {
                    prim.Material = material;
                }
            }

            return mesh;
        }

        IEnumerable<(GLTF.Accessor, GLTF.Material)> CreatePrimitiveIndices(Topology topo, IEnumerable<RenderAtom> atoms, Material_Group materialGroup)
        {
            var buf = new ArraySegment<byte>(new byte[topo.facelist.Count * 3 * 2]);
            var mai = new MemoryAccessInfo($"indices_{topo.hashname}", 0, topo.facelist.Count * 3, 0, GLTF.DimensionType.SCALAR, GLTF.EncodingType.UNSIGNED_SHORT);
            var ma = new MemoryAccessor(buf, mai);
            var array = ma.AsIntegerArray();
            for (int i = 0; i < topo.facelist.Count; i++)
            {
                array[i * 3 + 0] = topo.facelist[i].a;
                array[i * 3 + 1] = topo.facelist[i].b;
                array[i * 3 + 2] = topo.facelist[i].c;
            }

            var atomcount = 0;

            foreach (var ra in atoms)
            {
                var atom_mai = new MemoryAccessInfo($"indices_{topo.hashname}_{atomcount++}", (int)ra.baseVertex*2, (int)ra.vertCount*3, 0, GLTF.DimensionType.SCALAR, GLTF.EncodingType.UNSIGNED_SHORT);
                var atom_ma = new MemoryAccessor(buf, atom_mai);
                var accessor = root.CreateAccessor();
                accessor.SetIndexData(atom_ma);
                var material = materialsBySectionId[materialGroup.items[(int)ra.material_id]];
                yield return (accessor, material);
            }
        }

        List<(string, GLTF.Accessor)> GetGeometryAttributes(Geometry geometry)
        {
            List<(string, GLTF.Accessor)> result;
            if(vertexAttributesByGeometryId.TryGetValue(geometry.SectionId, out result))
            {
                return result;
            }
            result = new List<(string, GLTF.Accessor)>();

            var a_pos = MakeVertexAttributeAccessor("vpos", geometry.verts, 12, GLTF.DimensionType.VEC3, MathUtil.ToVector3, ma => ma.AsVector3Array());
            result.Add(("POSITION", a_pos));

            if (geometry.normals.Count > 0)
            {
                var a_norm = MakeVertexAttributeAccessor("vnorm", geometry.normals, 12, GLTF.DimensionType.VEC3, MathUtil.ToVector3, ma => ma.AsVector3Array());
                result.Add(("NORMAL", a_norm));
            }

            if (geometry.tangents.Count > 0)
            {
                Func<Nexus.Vector3D, int, Vector4> makeTangent = (input, index) =>
                {
                    var tangent = input.ToVector3();
                    var binorm = geometry.binormals[index].ToVector3();
                    var normal = geometry.normals[index].ToVector3();

                    var txn = Vector3.Cross(tangent, normal);
                    return new Vector4(tangent, Math.Sign(Vector3.Dot(txn, binorm)));
                };

                var a_binorm = MakeVertexAttributeAccessor("vtan", geometry.tangents, 16, GLTF.DimensionType.VEC4, makeTangent, ma => ma.AsVector4Array());
                result.Add(("TANGENT", a_binorm));
            }

            if (geometry.vertex_colors.Count > 0)
            {
                var a_col = MakeVertexAttributeAccessor("vcol", geometry.vertex_colors, 16, GLTF.DimensionType.VEC4, MathUtil.ToVector4, ma => ma.AsVector4Array());
                result.Add(("COLOR_0", a_col));
            }

            for (var i = 0; i < geometry.UVs.Length; i++)
            {
                var uvs = geometry.UVs[i];
                if(uvs.Count > 0)
                {
                    var a_uv = MakeVertexAttributeAccessor($"vuv_{i}", uvs, 12, GLTF.DimensionType.VEC2, FixupUV, ma => ma.AsVector2Array());
                    result.Add(($"TEXCOORD_{i}", a_uv));
                }
            }

            return result;
        }

        Vector2 FixupUV(Nexus.Vector2D input) => new Vector2(input.X, 1-input.Y);

        GLTF.Accessor MakeIndexAccessor(Topology topo)
        {
            var mai = new MemoryAccessInfo($"indices_{topo.hashname}", 0, topo.facelist.Count * 3, 0, GLTF.DimensionType.SCALAR, GLTF.EncodingType.UNSIGNED_SHORT);
            var ma = new MemoryAccessor(new ArraySegment<byte>(new byte[topo.facelist.Count * 3 * 2]), mai);
            var array = ma.AsIntegerArray();
            for(int i = 0; i < topo.facelist.Count; i++)
            {
                array[i * 3 + 0] = topo.facelist[i].a;
                array[i * 3 + 1] = topo.facelist[i].b;
                array[i * 3 + 2] = topo.facelist[i].c;
            }
            var accessor = root.CreateAccessor();
            accessor.SetIndexData(ma);
            return accessor;
        }

        GLTF.Accessor MakeVertexAttributeAccessor<TSource, TResult>(string maiName, IList<TSource> source, int stride, GLTF.DimensionType dimtype, Func<TSource, TResult> conv, Func<MemoryAccessor, IList<TResult>> getcontainer, GLTF.EncodingType enc = GLTF.EncodingType.FLOAT, bool normalized = false)
        {
            return MakeVertexAttributeAccessor(maiName, source, stride, dimtype, (s, i) => conv(s), getcontainer, enc, normalized);
        }

        GLTF.Accessor MakeVertexAttributeAccessor<TSource, TResult>(string maiName, IList<TSource> source, int stride, GLTF.DimensionType dimtype, Func<TSource, int, TResult> conv, Func<MemoryAccessor, IList<TResult>> getcontainer, GLTF.EncodingType enc = GLTF.EncodingType.FLOAT, bool normalized = false)
        {
            var mai = new MemoryAccessInfo(maiName, 0, source.Count, stride, dimtype, enc, normalized);
            var ma = new MemoryAccessor(new ArraySegment<byte>(new byte[source.Count * stride]), mai);
            var array = getcontainer(ma);
            for(int i = 0; i < source.Count; i++)
            {
                array[i] = conv(source[i], i);
            }
            var accessor = root.CreateAccessor();
            accessor.SetVertexData(ma);
            return accessor;
        }
    }
}
