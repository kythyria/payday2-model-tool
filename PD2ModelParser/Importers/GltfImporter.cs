using System;
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
                .FirstOrDefault(i => i.HashName.Hash == hashname.Hash);

            if (obj == null)
            {
                if (createModels && node.Mesh == null)
                {
                    obj = new DM.Object3D(hashname.String, parent);
                }
                else if (createModels && node.Mesh != null)
                {
                    obj = CreateNewModel(node.Mesh, node.Name);
                }
                else
                {
                    throw new Exception($"Object {node.Name} does not already exist and object creation is disabled.");
                }
                data.AddSection(obj);
            }
            else
            {
                if(node.Mesh != null && !(obj is DM.Model))
                {
                    if(!createModels)
                    {
                        throw new Exception($"Object {node.Name} already exists, isn't a model, and object creation is disabled.");
                    }
                    var newObj = CreateNewModel(node.Mesh, node.Name);
                    foreach(var i in obj.children)
                    {
                        i.SetParent(newObj);
                    }
                }
                else if (node.Mesh != null && obj is DM.Model)
                {
                    OverwriteModel(node.Mesh, obj as DM.Model);
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
            (obj as DM.Model)?.UpdateBounds(data);

            foreach(var child in node.VisualChildren)
            {
                ImportNode(child, obj);
            }
        }

        void OverwriteModel(GLTF.Mesh gmesh, DM.Model model)
        {
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

            var matGroup = new DM.Material_Group((uint)(gmesh.Name + ".matgroup").GetHashCode(), mats.Select(i => i.SectionId));
            data.AddSection(matGroup);

            var ms = new MeshSections();
            ms.matg = matGroup;
            ms.topoip = data.parsed_sections[model.topologyIP_ID] as DM.TopologyIP;
            ms.passgp = data.parsed_sections[model.passthroughGP_ID] as DM.PassthroughGP;
            ms.geom = data.parsed_sections[ms.passgp.SectionId] as DM.Geometry;
            ms.topo = data.parsed_sections[ms.topoip.SectionId] as DM.Topology;
            ms.atoms = md.renderAtoms;

            ms.PopulateFromMeshData(md);

            model.RenderAtoms = md.renderAtoms;
        }

        DM.Model CreateNewModel(GLTF.Mesh gmesh, string name)
        {
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

            var matGroup = new DM.Material_Group((uint)(gmesh.Name + ".matgroup").GetHashCode(), mats.Select(i => i.SectionId));
            data.AddSection(matGroup);

            var ms = new MeshSections();
            ms.matg = matGroup;

            ms.geom = new DM.Geometry((uint)(gmesh.Name + ".geom").GetHashCode());
            ms.geom.hashname = Hash64.HashString(gmesh.Name + ".Geometry");
            data.AddSection(ms.geom);

            ms.topo = new DM.Topology((uint)(gmesh.Name + ".Topology").GetHashCode(), gmesh.Name);
            data.AddSection(ms.topo);

            ms.topoip = new DM.TopologyIP((uint)(gmesh.Name + ".topoIP").GetHashCode(), ms.topo);
            data.AddSection(ms.topoip);

            ms.passgp = new DM.PassthroughGP((uint)(gmesh.Name + ".passGP").GetHashCode(), ms.geom, ms.topo);
            data.AddSection(ms.passgp);

            ms.atoms = md.renderAtoms;

            ms.PopulateFromMeshData(md);

            var model = new DM.Model(name, (uint)ms.geom.verts.Count, (uint)ms.topo.facelist.Count, ms.passgp, ms.topoip, ms.matg, null);
            model.RenderAtoms = md.renderAtoms;

            return model;
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
            public DM.Material_Group matg;
            public List<DM.RenderAtom> atoms = new List<DM.RenderAtom>();

            public void PopulateFromMeshData(MeshData md)
            {
                geom.headers.Clear();

                // Have to specify conv, as the type inference isn't smart enough to figure that out.
                void AddToGeom<TS,TD>(ref List<TD> dest, uint size, DM.GeometryChannelTypes ct, IList<TS> src, Func<TS,TD> conv)
                {
                    if(src.Count > 0)
                    {
                        geom.headers.Add(new DM.GeometryHeader(size, ct));
                        dest = src.Select(conv).ToList();
                    }
                }

                AddToGeom(ref geom.verts, 3, DM.GeometryChannelTypes.POSITION, md.verts, MathUtil.ToNexusVector);
                AddToGeom(ref geom.normals, 3, DM.GeometryChannelTypes.NORMAL0, md.normals, MathUtil.ToNexusVector);
                AddToGeom(ref geom.binormals, 3, DM.GeometryChannelTypes.BINORMAL0, md.binormals, MathUtil.ToNexusVector);
                AddToGeom(ref geom.tangents, 3, DM.GeometryChannelTypes.TANGENT0, md.tangents, MathUtil.ToNexusVector);
                AddToGeom(ref geom.vertex_colors, 5, DM.GeometryChannelTypes.COLOR0, md.vertex_colors, i => i);
                for(var i = 0; i < md.uvs.Length; i++)
                {
                    var ct = (DM.GeometryChannelTypes)((int)DM.GeometryChannelTypes.TEXCOORD0 + i);
                    AddToGeom(ref geom.UVs[i], 2, ct, md.uvs[i], MathUtil.ToNexusVector);
                }

                geom.vert_count = (uint)geom.verts.Count;

                topo.facelist = md.faces;
            }

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
                ms.materials = mesh.Primitives.Select(i => i.Material?.Name ?? "Material: Default Material").Distinct().ToList();

                if (ms.materials.Contains(null))
                {
                    throw new Exception($"Missing material in mesh {mesh.Name}");
                }

                uint currentBaseVertex = 0;
                uint currentBaseFace = 0;
                foreach (var prim in mesh.Primitives)
                {
                    //var indicesByVertex = new Dictionary<Vertex, int>();
                    var vertices = GetVerticesFromPrimitive(prim).ToList();
                    foreach (var vtx in vertices)
                    {
                        //if (indicesByVertex.ContainsKey(vtx)) { continue; }
                        /*indicesByVertex[vtx] =*/ ms.AppendVertex(vtx);
                    }

                    foreach (var (A, B, C) in prim.GetTriangleIndices())
                    {
                        var face = new DM.Face
                        {
                            a = (ushort)(currentBaseVertex + A),// indicesByVertex[vertices[A]],
                            b = (ushort)(currentBaseVertex + B),// indicesByVertex[vertices[B]],
                            c = (ushort)(currentBaseVertex + C),// indicesByVertex[vertices[C]],
                        };
                        ms.faces.Add(face);
                    }

                    var ra = new DM.RenderAtom
                    {
                        BaseIndex = currentBaseVertex,
                        TriangleCount = (uint)ms.faces.Count,
                        GeometrySliceLength = (uint)ms.verts.Count,
                        BaseVertex = currentBaseFace,
                        MaterialId = (uint)ms.materials.IndexOf(prim.Material?.Name ?? "Material: Default Material")
                    };
                    ms.renderAtoms.Add(ra);

                    currentBaseVertex += ra.TriangleCount;
                    currentBaseFace += ra.GeometrySliceLength;
                }
                return ms;
            }

            static IEnumerable<Vertex> GetVerticesFromPrimitive(GLTF.MeshPrimitive prim)
            {
                var pos = prim.VertexAccessors["POSITION"];
                //pos = prim.GetVertexAccessor("POSITON");
                var result = pos.AsVector3Array().Select((p, idx) => {
                    return new Vertex { pos = p };
                });

                prim.VertexAccessors.TryGetValue("NORMAL", out var normal);
                if (normal != null && normal.Count > 0)
                {
                    var na = normal.AsVector3Array();
                    result = result.Select((vtx, idx) => { vtx.normal = na[idx]; return vtx; });
                }

                prim.VertexAccessors.TryGetValue("TANGENT", out var tangent);
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

                prim.VertexAccessors.TryGetValue("COLOR_0", out var vcols);
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
                    var ii = i; // Closures capture by reference, and i is declared outside the loop body in for.
                    prim.VertexAccessors.TryGetValue($"TEXCOORD_{ii}", out var uvs);
                    if (uvs != null && uvs.Count > 0)
                    {
                        var uva = uvs.AsVector2Array();
                        result = result.Select((vtx, idx) => { vtx.uv[ii] = new Vector2(uva[idx].X, 1-uva[idx].Y); return vtx; });
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
