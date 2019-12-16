﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GLTF = SharpGLTF.Schema2;
using DM = PD2ModelParser.Sections;
using ET = System.Linq.Expressions;
using System.Numerics;

namespace PD2ModelParser.Importers
{

    class GltfImporter
    {
        /// <summary>
        /// Import all of the default scene of a GLTF file, parenting it as specified.
        /// </summary>
        /// <param name="fmd">Diesel model to add the contents to.</param>
        /// <param name="path">Path of the GLTF file to import</param>
        /// <param name="getParentByName">Callback to find the parent of a particular model.</param>
        public static void Import(FullModelData fmd, string path, bool createModels, Func<string, DM.Object3D> parentFinder)
        {
            var gltf = GLTF.ModelRoot.Load(path);
            var importer = new GltfImporter(fmd);
            importer.ImportTree(gltf, createModels, parentFinder);
        }

        GLTF.ModelRoot root;
        FullModelData data;
        Dictionary<GLTF.Mesh, MeshSections> meshSectionsByMesh = new Dictionary<GLTF.Mesh, MeshSections>();
        bool createModels;

        public GltfImporter(FullModelData data)
        {
            this.data = data;
        }

        public void ImportTree(GLTF.ModelRoot root, bool createModels, Func<string, DM.Object3D> parentFinder)
        {
            this.root = root;
            this.createModels = createModels;

            foreach(var node in root.DefaultScene.VisualChildren)
            {
                DM.Object3D parent = null;
                try
                {
                    parent = parentFinder(node.Name);
                }
                catch { } // TODO: Call with a better parentFinder that doesn't need this.
                ImportNode(node, parent);
            }
        }

        void ImportNode(GLTF.Node node, DM.Object3D parent)
        {
            var hashname = GetName(node.Name);

            var obj = data.parsed_sections
                .Select(i => i.Value as DM.Object3D)
                .Where(i => i != null)
                .FirstOrDefault(i => i.hashname.Hash == hashname.Hash);

            if (obj == null)
            {
                if (createModels)
                {
                    obj = new DM.Object3D(hashname.String, parent);
                }
                else
                {
                    throw new Exception($"Object {node.Name} does not already exist and object creation is disabled.");
                }
            }

            if(parent != null)
            {
                obj.SetParent(parent);
            }

            if (node.LocalMatrix != null)
            {
                obj.rotation = node.LocalMatrix.ToNexusMatrix();
            }
            else
            {
                obj.rotation = node.LocalTransform.Matrix.ToNexusMatrix();
            }

            if (node.Mesh != null)
            {
                AddOrOverwriteMesh(node.Mesh, obj);
            }

            foreach(var child in node.VisualChildren)
            {
                ImportNode(child, obj);
            }
        }

        void AddOrOverwriteMesh(GLTF.Mesh gmesh, DM.Object3D obj)
        {
            var model = data.SectionsOfType<DM.Model>().Where(i => i.object3D == obj).FirstOrDefault();
            var md = MeshData.FromGltfMesh(gmesh);

            var mats = md.materials.Select(i =>
            {
                var hn = GetName(i);
                var mat = data.SectionsOfType<DM.Material>().FirstOrDefault(j => j.hashname.Hash == hn.Hash);
                if (mat == null)
                {
                    mat = new DM.Material((uint)(i + ".mat").GetHashCode(), i);
                    data.AddSection(mat);
                }
                return mat;
            }).ToList();

            DM.Material_Group matgroup = null;
            if(model != null)
            {
                if(data.parsed_sections.ContainsKey(model.material_group_section_id))
                {
                    var existingMatgroup = data.parsed_sections[model.material_group_section_id] as DM.Material_Group;
                    var matches = existingMatgroup.items.Zip(mats.Select(i => i.id), (x, y) => x == y).All(i => i);
                    if(matches)
                    {
                        matgroup = existingMatgroup;
                    }
                }
            }

            if(matgroup == null)
            {
                var matGroup = new DM.Material_Group((uint)(gmesh.Name + ".matgroup").GetHashCode(), mats.Select(i => i.id));
                data.AddSection(matGroup);
            }

            if (model != null)
            {
            }
            else
            {


            }
        }

        HashName GetName(string input)
        {
            if (ulong.TryParse(input, out ulong hashnum))
            {
                return new HashName(hashnum);
            }
            else
            {
                return new HashName(input); 
            }
        }

        public class MeshSections
        {
            public DM.Geometry geom;
            public DM.Topology topo;
            public DM.TopologyIP topoip;
            public DM.PassthroughGP passgp;
            public List<DM.RenderAtom> atoms = new List<DM.RenderAtom>();
            public List<DM.Material> materials = new List<DM.Material>();
        }

        public class MeshData
        {
            public List<Vector3> verts = new List<Vector3>();
            public List<Vector3> normals = new List<Vector3>();
            public List<DM.GeometryColor> vertex_colors = new List<DM.GeometryColor>();
            public List<Vector3> binormals = new List<Vector3>();
            public List<Vector3> tangents = new List<Vector3>();
            public List<DM.Face> faces = new List<DM.Face>();
            public List<DM.RenderAtom> renderAtoms = new List<DM.RenderAtom>();
            public List<string> materials = new List<string>();
            public List<Vector2>[] uvs = new List<Vector2>[] {
                new List<Vector2>(),
                new List<Vector2>(),
                new List<Vector2>(),
                new List<Vector2>(),
                new List<Vector2>(),
                new List<Vector2>(),
                new List<Vector2>(),
                new List<Vector2>(),
            };

            public int AppendVertex(Vertex vtx)
            {
                var idx = this.verts.Count;
                this.verts.Add(vtx.pos);
                vtx.vtx_col.WithValue(v => this.vertex_colors.Add(v.ToGeometryColor()));
                vtx.normal.WithValue(v =>  this.normals.Add(v));
                vtx.tangent.WithValue(v => this.tangents.Add(v));
                vtx.binormal.WithValue(v =>this.binormals.Add(v));
                for (var i = 0; i < 8; i++)
                {
                    vtx.uv[i].WithValue(v => this.uvs[i].Add(v));
                }
                return idx;
            }

            public static MeshData FromGltfMesh(GLTF.Mesh mesh)
            {
                var attribsUsed = mesh.Primitives.First().VertexAccessors.Select(i => i.Key).OrderBy(i => i);
                var ok = mesh.Primitives.Select(i => i.VertexAccessors.Keys.OrderBy(j => j)).Aggregate(true, (acc, curr) => acc && curr.SequenceEqual(attribsUsed));
                if (!ok)
                {
                    throw new Exception("Vertex attributes not consistent between Primitives. Diesel cannot represent this.");
                }

                var ms = new MeshData();
                ms.materials = mesh.Primitives.Select(i => i.Material?.Name).Distinct().ToList();

                if (ms.materials.Contains(null))
                {
                    throw new Exception($"Missing material in mesh {mesh.Name}");
                }

                var indicesByVertex = new Dictionary<Vertex, int>();
                uint currentBaseVertex = 0;
                uint currentBaseFace = 0;
                foreach (var prim in mesh.Primitives)
                {
                    var vertices = GetVerticesFromPrimitive(prim).ToList();
                    foreach (var vtx in vertices)
                    {
                        if (indicesByVertex.ContainsKey(vtx)) { continue; }
                        indicesByVertex[vtx] = ms.AppendVertex(vtx);
                    }

                    foreach (var (A, B, C) in prim.GetTriangleIndices())
                    {
                        var face = new DM.Face
                        {
                            a = (ushort)indicesByVertex[vertices[A]],
                            b = (ushort)indicesByVertex[vertices[B]],
                            c = (ushort)indicesByVertex[vertices[C]],
                        };
                        ms.faces.Add(face);
                    }

                    var ra = new DM.RenderAtom
                    {
                        baseVertex = currentBaseVertex,
                        vertCount = (uint)prim.IndexAccessor.Count,
                        faceCount = (uint)ms.faces.Count,
                        unknown1 = currentBaseFace,
                        material_id = (uint)ms.materials.IndexOf(prim.Material.Name)
                    };
                    ms.renderAtoms.Add(ra);

                    currentBaseVertex += ra.vertCount;
                    currentBaseFace += ra.faceCount;
                }
                return ms;
            }

            static IEnumerable<Vertex> GetVerticesFromPrimitive(GLTF.MeshPrimitive prim)
            {
                var pos = prim.GetVertexAccessor("POSITON");
                var result = pos.AsVector3Array().Select((p, idx) => new Vertex { pos = p });

                var normal = prim.GetVertexAccessor("NORMAL");
                if (normal != null && normal.Count > 0)
                {
                    var na = normal.AsVector3Array();
                    result = result.Select((vtx, idx) => { vtx.normal = na[idx]; return vtx; });
                }

                var tangent = prim.GetVertexAccessor("TANGENT");
                if (tangent != null && tangent.Count > 0)
                {
                    var ta = tangent.AsVector4Array();
                    result = result.Select((vtx, idx) =>
                    {
                        var et = ta[idx];
                        var tangent_vector = new Vector3(et.X, et.Y, et.Z);
                        var binormal = Vector3.Cross(tangent_vector, vtx.normal.Value) * et.W;
                        vtx.tangent = tangent_vector;
                        vtx.binormal = binormal;

                        return vtx;
                    });
                }

                var vcols = prim.GetVertexAccessor("COLOR_0");
                if (vcols != null && vcols.Count > 0)
                {
                    if (vcols.Dimensions == GLTF.DimensionType.VEC4)
                    {
                        var vca = vcols.AsVector4Array();
                        result = result.Select((vtx, idx) => { vtx.vtx_col = vca[idx]; return vtx; });
                    }
                    else
                    {
                        var vca = vcols.AsVector3Array();
                        // TODO: Does Diesel have opaque be 1, 0, or 255?
                        result = result.Select((vtx, idx) => { vtx.vtx_col = new Vector4(vca[idx], 1.0f); return vtx; });
                    }
                }

                for (int i = 0; i < 8; i++)
                {
                    var uvs = prim.GetVertexAccessor($"TEXCOORD_{i}");
                    if (uvs != null && uvs.Count > 0)
                    {
                        var uva = uvs.AsVector2Array();
                        result = result.Select((vtx, idx) => { vtx.uv[i] = uva[idx]; return vtx; });
                    }
                }

                return result;
            }
        }

        public class Vertex : IEquatable<Vertex>
        {
            public Vector3 pos;
            public Vector3? normal, tangent, binormal;
            public Vector4? vtx_col;
            public Vector2?[] uv = new Vector2?[8];

            public override bool Equals(object obj)
            {
                return Equals(obj as Vertex);
            }

            public bool Equals(Vertex other)
            {
                return other != null &&
                       pos.Equals(other.pos) &&
                       EqualityComparer<Vector3?>.Default.Equals(normal, other.normal) &&
                       EqualityComparer<Vector3?>.Default.Equals(tangent, other.tangent) &&
                       EqualityComparer<Vector3?>.Default.Equals(binormal, other.binormal) &&
                       EqualityComparer<Vector4?>.Default.Equals(vtx_col, other.vtx_col) &&
                       EqualityComparer<Vector2?[]>.Default.Equals(uv, other.uv);
            }

            public override int GetHashCode()
            {
                var hashCode = -1990297534;
                hashCode = hashCode * -1521134295 + EqualityComparer<Vector3>.Default.GetHashCode(pos);
                hashCode = hashCode * -1521134295 + EqualityComparer<Vector3?>.Default.GetHashCode(normal);
                hashCode = hashCode * -1521134295 + EqualityComparer<Vector3?>.Default.GetHashCode(tangent);
                hashCode = hashCode * -1521134295 + EqualityComparer<Vector3?>.Default.GetHashCode(binormal);
                hashCode = hashCode * -1521134295 + EqualityComparer<Vector4?>.Default.GetHashCode(vtx_col);
                for (int i = 0; i < uv.Length; i++)
                {
                    uv[i].WithValue(v => hashCode = hashCode * -1521134295 + EqualityComparer<Vector2>.Default.GetHashCode(v));
                }
                return hashCode;
            }
        }
    }
}