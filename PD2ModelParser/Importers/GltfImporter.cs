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
        public static FullModelData ImportAsNew(string path)
        {
            var gltf = GLTF.ModelRoot.Load(path);
            var importer = new GltfImporter();
            var result = importer.Import(gltf);
            return result;
        }

        GLTF.ModelRoot root;
        FullModelData data;

        public class MeshSections {
            public DM.Geometry geom;
            public DM.Topology topo;
            public DM.TopologyIP topoip;
            public DM.PassthroughGP passgp;
            public List<DM.RenderAtom> atoms;
        }

        Dictionary<GLTF.Mesh, MeshSections> meshSectionsByMesh = new Dictionary<GLTF.Mesh, MeshSections>();

        public FullModelData Import(GLTF.ModelRoot root)
        {
            this.root = root;
            data = new FullModelData();

            foreach (var mesh in root.LogicalMeshes)
            {
                AddMeshSections(mesh);
            }

            return data;
        }

        public MeshSections AddMeshSections(GLTF.Mesh mesh)
        {
            var attribsUsed = mesh.Primitives.First().VertexAccessors.Select(i => i.Key).OrderBy(i => i);
            var ok = mesh.Primitives.Select(i => i.VertexAccessors.Keys.OrderBy(j => j)).Aggregate(true, (acc, curr) => acc && curr.SequenceEqual(attribsUsed));
            if(!ok)
            {
                throw new Exception("Vertex attributes not consistent between Primitives. Diesel cannot represent this.");
            }

            var geom = new DM.Geometry((uint)(mesh.Name + ".geom").GetHashCode());
            var topo = new DM.Topology((uint)(mesh.Name + ".topo").GetHashCode());
            var ras = new List<DM.RenderAtom>();

            var indicesByVertex = new Dictionary<Vertex, int>();
            uint currentBaseVertex = 0;
            uint currentBaseFace = 0;
            foreach(var prim in mesh.Primitives)
            {
                var vertices = GetVerticesFromPrimitive(prim).ToList();
                foreach(var vtx in vertices)
                {
                    if(indicesByVertex.ContainsKey(vtx)) { continue; }
                    indicesByVertex[vtx] = AppendVertex(geom, vtx);
                }
                
                foreach(var (A, B, C) in prim.GetTriangleIndices())
                {
                    var face = new DM.Face
                    {
                        a = (ushort)indicesByVertex[vertices[A]],
                        b = (ushort)indicesByVertex[vertices[B]],
                        c = (ushort)indicesByVertex[vertices[C]],
                    };
                    topo.facelist.Add(face);
                }

                var ra = new DM.RenderAtom
                {
                    baseVertex = currentBaseVertex,
                    vertCount = (uint)prim.IndexAccessor.Count,
                    faceCount = (uint)topo.facelist.Count,
                    unknown1 = currentBaseFace,
                    material_id = 0
                };
                ras.Add(ra);

                currentBaseVertex += ra.vertCount;
                currentBaseFace += ra.faceCount;
            }

            return new MeshSections
            {
                geom = geom,
                topo = topo,
                atoms = ras,
            };
        }

        int AppendVertex(DM.Geometry geom, Vertex vtx)
        {
            var idx = geom.verts.Count;
            geom.verts.Add(vtx.pos.ToNexusVector());
            vtx.vtx_col.WithValue(v => geom.vertex_colors.Add(v.ToGeometryColor()));
            vtx.normal.WithValue(v => geom.normals.Add(v.ToNexusVector()));
            vtx.tangent.WithValue(v => geom.tangents.Add(v.ToNexusVector()));
            vtx.binormal.WithValue(v => geom.binormals.Add(v.ToNexusVector()));
            for(var i = 0; i < 8; i++)
            {
                vtx.uv[i].WithValue(v => geom.UVs[i].Add(v.ToNexusVector()));
            }
            return idx;
        }

        IEnumerable<Vertex> GetVerticesFromPrimitive(GLTF.MeshPrimitive prim)
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
            if(tangent != null && tangent.Count > 0)
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
            if(vcols != null && vcols.Count > 0)
            {
                if(vcols.Dimensions == GLTF.DimensionType.VEC4)
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

            for(int i = 0; i < 8; i++)
            {
                var uvs = prim.GetVertexAccessor($"TEXCOORD_{i}");
                if(uvs != null && uvs.Count > 0)
                {
                    var uva = uvs.AsVector2Array();
                    result = result.Select((vtx, idx) => { vtx.uv[i] = uva[idx]; return vtx; });
                }
            }

            return result;
        }

        class Vertex : IEquatable<Vertex>
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
