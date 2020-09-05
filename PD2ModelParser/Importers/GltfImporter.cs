using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

using GLTF = SharpGLTF.Schema2;
using DM = PD2ModelParser.Sections;
//using PD2ModelParser.Sections;

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
        public static void Import(FullModelData fmd, string path, bool createModels, Func<string, DM.Object3D> parentFinder, IOptionReceiver _)
        {
            var gltf = GLTF.ModelRoot.Load(path);
            var importer = new GltfImporter(fmd);
            importer.ImportTree(gltf, createModels, parentFinder);
        }

        FullModelData data;
        Dictionary<GLTF.Node, DM.Object3D> objectsByNode = new Dictionary<GLTF.Node, DM.Object3D>();
        Dictionary<GLTF.Skin, DM.SkinBones> skinBonesBySkin = new Dictionary<GLTF.Skin, DM.SkinBones>();
        bool createModels;
        List<(GLTF.Node node, DM.Model model)> toSkin = new List<(GLTF.Node node, DM.Model model)>();

        /// <summary>
        /// How much to embiggen incoming GLTF data.
        /// </summary>
        /// <remarks>
        /// GLTF specifies a 1m scale and Diesel uses 1cm, so the default of 100 should normally
        /// be useful.
        /// 
        /// The proof re how is longwinded, but if you work out how "apply scale" has to work
        /// when all transformations are translate-rotate-scale (as in GLTF), you can discover that
        /// because uniform scale commutes with rotate and with any scale, and it commutes with
        /// translation by multiplying or dividing the translation's vector by the scale factor,
        /// all we need to do is 1) scale every mesh on import, and 2) twiddle the translation
        /// component of every node on import.
        /// </remarks>
        float scaleFactor = 100;

        public GltfImporter(FullModelData data)
        {
            this.data = data;
        }

        public void ImportTree(GLTF.ModelRoot root, bool createModels, Func<string, DM.Object3D> parentFinder)
        {
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

            foreach (var i in toSkin)
            {
                ImportSkin(i.node, i.model);
            }
        }

        void ImportNode(GLTF.Node node, DM.Object3D parent)
        {
            var hashname = HashName.FromNumberOrString(node.Name);

            var obj = data.parsed_sections
                .Select(i => i.Value as DM.Object3D)
                .Where(i => i != null)
                .FirstOrDefault(i => i.HashName.Hash == hashname.Hash);

            if (obj == null)
            {
                var extras = node.TryUseExtrasAsDictionary(false);
                if(createModels && extras != null && extras.ContainsKey("diesel.modelv6.unk7"))
                {
                    obj = CreateNewModelv6(node, parent);
                }
                else if (createModels && node.Mesh == null)
                {
                    obj = new DM.Object3D(hashname.String, parent);
                }
                else if (createModels && node.Mesh != null)
                {
                    obj = CreateNewModel(node.Mesh, node.Name);

                    if (node.Skin != null)
                    {
                        toSkin.Add((node, obj as DM.Model));
                    }
                }
                else if (createModels && node.PunctualLight != null)
                {
                    obj = CreateNewLamp(node.PunctualLight, node.Name);
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
                else if (node.Mesh != null && obj is DM.Model mod)
                {
                    OverwriteModel(node.Mesh, mod);
                }
                else if (node.PunctualLight != null)
                {
                    throw new Exception("Can't overwrite lights yet.");
                }
            }

            objectsByNode.Add(node, obj);

            if(parent != null)
            {
                obj.SetParent(parent);
            }

            var lt = node.LocalTransform;
            lt.Translation *= scaleFactor;
            obj.Transform = lt.Matrix.ToNexusMatrix();

            (obj as DM.Model)?.UpdateBounds();

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
                var hn = HashName.FromNumberOrString(i);
                var mat = data.SectionsOfType<DM.Material>().FirstOrDefault(j => j.HashName.Hash == hn.Hash);
                if (mat == null)
                {
                    mat = new DM.Material(i);
                    data.AddSection(mat);
                }
                return mat;
            }).ToList();

            var matGroup = new DM.MaterialGroup(mats);
            data.AddSection(matGroup);

            var ms = new MeshSections();
            ms.matg = matGroup;
            ms.topoip = model.TopologyIP;
            ms.passgp = model.PassthroughGP;
            ms.geom = ms.passgp.Geometry;
            ms.topo = ms.passgp.Topology;
            ms.atoms = md.renderAtoms;

            ms.PopulateFromMeshData(md);
            ms.Scale(this.scaleFactor);

            model.RenderAtoms = md.renderAtoms;
        }

        DM.Light CreateNewLamp(GLTF.PunctualLight gl, string name)
        {
            var extras = gl.TryUseExtrasAsDictionary(false) ?? new SharpGLTF.IO.JsonDictionary();
            float? maybeNearRange = extras["diesel.nearRange"] as float?;
            float nearRange = 0.0f;
            if(!maybeNearRange.HasValue)
            {
                Log.Default.Warn($"Light {name} has no near range specified. Defaulting to 0");
            }
            else
            {
                nearRange = maybeNearRange.Value;
            }

            var dl = new DM.Light(name, null)
            {
                unknown_1 = 1,
                Colour = new DM.LightColour() { A = 1.0f, R = gl.Color.X, G = gl.Color.Y, B = gl.Color.Z },
                FarRange = gl.Range * scaleFactor,
                unknown_8 = BitConverter.ToSingle(new byte[4] { 4, 0, 0, 0 }, 0)
            };
            switch(gl.LightType)
            {
                case GLTF.PunctualLightType.Point:
                    dl.LightType = 1;
                    break;
                case GLTF.PunctualLightType.Spot:
                    dl.LightType = 2;
                    break;
                default:
                    throw new Exception($"Light {name} is neither point nor spot.");
            }
            if(extras.TryGetValue("diesel.unknown_6", out object ov))
            {
                if(ov != null && ov is float) { dl.unknown_6 = (float)ov; }
            }
            if (extras.TryGetValue("diesel.unknown_7", out object ou7))
            {
                if (ou7 != null && ou7 is float) { dl.unknown_6 = (float)ou7; }
            }
            return dl;
        }

        DM.Model CreateNewModel(GLTF.Mesh gmesh, string name)
        {
            var md = MeshData.FromGltfMesh(gmesh);

            var mats = md.materials.Select(i =>
            {
                var hn = HashName.FromNumberOrString(i);
                var mat = data.SectionsOfType<DM.Material>().FirstOrDefault(j => j.HashName.Hash == hn.Hash);
                if (mat == null)
                {
                    mat = new DM.Material(i);
                    data.AddSection(mat);
                }
                return mat;
            }).ToList();

            var matGroup = new DM.MaterialGroup(mats);
            data.AddSection(matGroup);

            var ms = new MeshSections();
            ms.matg = matGroup;

            ms.geom = new DM.Geometry();
            data.AddSection(ms.geom);
            ms.geom.HashName = new HashName(gmesh.Name + ".Geometry");

            ms.topo = new DM.Topology(gmesh.Name);
            data.AddSection(ms.topo);

            ms.topoip = new DM.TopologyIP(ms.topo);
            data.AddSection(ms.topoip);

            ms.passgp = new DM.PassthroughGP(ms.geom, ms.topo);
            data.AddSection(ms.passgp);

            ms.atoms = md.renderAtoms;

            ms.PopulateFromMeshData(md);
            ms.Scale(this.scaleFactor);

            var model = new DM.Model(name, (uint)ms.geom.verts.Count, (uint)ms.topo.facelist.Count, ms.passgp, ms.topoip, ms.matg, null);
            model.RenderAtoms = md.renderAtoms;

            return model;
        }

        DM.Model CreateNewModelv6(GLTF.Node node, DM.Object3D parent)
        {
            var extras = node.TryUseExtrasAsDictionary(true);
            object o_unk7, o_bmin, o_bmax;
            Vector3 v_bmin, v_bmax;

            if(!extras.TryGetValue("diesel.modelv6.unk7", out o_unk7) || !(o_unk7 is float || o_unk7 is double))
            {
                throw new Exception($"Node {node.Name} doesn't contain extra \"diesel.modelv6.unk7\" or it's not a float");
            }
            if(!extras.TryGetValue("diesel.modelv6.bound_min", out o_bmin))
            {
                throw new Exception($"Node {node.Name} is a v6 model and doesn't contain \"diesel.modelv6.bound_min\"");
            }
            if (!extras.TryGetValue("diesel.modelv6.bound_max", out o_bmax))
            {
                throw new Exception($"Node {node.Name} is a v6 model and doesn't contain \"diesel.modelv6.bound_max\"");
            }

            if(!(o_bmin is SharpGLTF.IO.JsonList jl_bmin) || (jl_bmin.Aggregate(true, (m, v) => m && (v is float || v is double)) == false)) {
                throw new Exception($"Node {node.Name} is a v6 model and \"diesel.modelv6.bound_min\" is not float[]");
            }
            else
            {
                v_bmin = new Vector3(Convert.ToSingle(jl_bmin[0]), Convert.ToSingle(jl_bmin[1]), Convert.ToSingle(jl_bmin[2]));
            }

            if (!(o_bmax is SharpGLTF.IO.JsonList jl_bmax) || (jl_bmax.Aggregate(true, (m, v) => m && (v is float || v is double)) == false)) {
                throw new Exception($"Node {node.Name} is a v6 model and \"diesel.modelv6.bound_max\" is not float[]");
            }
            else
            {
                v_bmax = new Vector3(Convert.ToSingle(jl_bmax[0]), Convert.ToSingle(jl_bmax[1]), Convert.ToSingle(jl_bmax[2]));
            }

            return new DM.Model(node.Name, Convert.ToSingle(o_unk7), v_bmin, v_bmax, parent);
        }

        void ImportSkin(GLTF.Node node, DM.Model model)
        {
            DM.SkinBones skinBones;
            if(skinBonesBySkin.TryGetValue(node.Skin, out skinBones))
            {
                model.SkinBones = skinBones;
                model.SetParent(skinBones.ProbablyRootBone);
                return;
            }

            var random = new Random();
            var randomBytes = new byte[4];
            random.NextBytes(randomBytes);
            skinBones = new DM.SkinBones();

            skinBones.global_skin_transform = Matrix4x4.Identity.ToNexusMatrix();
            var parent = data.SectionsOfType<DM.Object3D>()
                .FirstOrDefault(i => i.Name == node.Skin.Skeleton.Name);
            skinBones.ProbablyRootBone = parent;
            // I have no idea if this is universal. It looks like it might be.
            // TODO: For skinned meshes, does mesh.Parent == mesh.SkinBones.probably_root_bone?
            model.SetParent(parent);

            var bmi = new DM.BoneMappingItem();
            skinBones.bone_mappings.Add(bmi);
            for(var i = 0; i < node.Skin.JointsCount; i++)
            {
                var (jointNode, ibm) = node.Skin.GetJoint(i);
                ibm.Translation *= scaleFactor;
                skinBones.rotations.Add(ibm.ToNexusMatrix());
                skinBones.Objects.Add(objectsByNode[jointNode]);
                bmi.bones.Add((uint) i);
            }

            // It makes no sense that diesel wants this, but diesel wants this.
            // And it's consistent with the FBX importer, which worked.
            foreach(var ra in model.RenderAtoms)
            {
                skinBones.bone_mappings.Add(bmi);
            }

            skinBonesBySkin.Add(node.Skin, skinBones);
            data.AddSection(skinBones);
            model.SkinBones = skinBones;
        }

        public class MeshSections
        {
            public DM.Geometry geom;
            public DM.Topology topo;
            public DM.TopologyIP topoip;
            public DM.PassthroughGP passgp;
            public DM.MaterialGroup matg;
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
                AddToGeom(ref geom.weights, 3, DM.GeometryChannelTypes.BLENDWEIGHT0, md.weights, MathUtil.ToNexusVector);
                AddToGeom(ref geom.weight_groups, 7, DM.GeometryChannelTypes.BLENDINDICES0, md.weightGroups, x => x);

                geom.vert_count = (uint)geom.verts.Count;

                topo.facelist = md.faces;
            }

            public void Scale(float fac)
            {
                for (var i = 0; i < geom.verts.Count; i++)
                {
                    geom.verts[i] = geom.verts[i] * fac;
                }
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
            public List<Vector3> weights = new List<Vector3>();
            public List<DM.GeometryWeightGroups> weightGroups = new List<DM.GeometryWeightGroups>();

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
                vtx.weight.WithValue(v => this.weights.Add(v));
                if(vtx.weightGroups != null) { this.weightGroups.Add(vtx.weightGroups); }
                return idx;
            }

            public void AppendVertices(IEnumerable<Vertex> vertices)
            {
                foreach(var i in vertices)
                {
                    AppendVertex(i);
                }
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

                uint currentBaseVertex = 0;
                uint currentBaseIndex = 0;

                foreach (var prim in mesh.Primitives)
                {
                    var vertices = GetVerticesFromPrimitive(prim).ToList();
                    var primFaces = prim.GetTriangleIndices().ToList();
                    var matname = prim.Material?.Name ?? "Material: Default Material";

                    var ra = new DM.RenderAtom
                    {
                        BaseIndex = currentBaseIndex,
                        BaseVertex = currentBaseVertex,
                        MaterialId = (uint)ms.materials.IndexOf(matname),
                        TriangleCount = (uint)primFaces.Count
                    };

                    var vertexIds = new Dictionary<Vertex, int>();
                    foreach(var (A,B,C) in primFaces)
                    {
                        var vtxA = vertices[A];
                        var vtxB = vertices[B];
                        var vtxC = vertices[C];

                        if(!vertexIds.ContainsKey(vtxA))
                        {
                            vertexIds[vtxA] = ms.AppendVertex(vtxA);
                        }
                        if (!vertexIds.ContainsKey(vtxB))
                        {
                            vertexIds[vtxB] = ms.AppendVertex(vtxB);
                        }
                        if (!vertexIds.ContainsKey(vtxC))
                        {
                            vertexIds[vtxC] = ms.AppendVertex(vtxC);
                        }

                        var df = new DM.Face()
                        {
                            a = (ushort)vertexIds[vtxA],
                            b = (ushort)vertexIds[vtxB],
                            c = (ushort)vertexIds[vtxC]
                        };

                        ms.faces.Add(df);
                    }

                    ra.GeometrySliceLength = (uint)vertexIds.Count;
                    ms.renderAtoms.Add(ra);

                    currentBaseIndex += ra.TriangleCount * 3;
                    currentBaseVertex += ra.GeometrySliceLength;
                }
                return ms;
            }

            public static MeshData FromGltfMesh0(GLTF.Mesh mesh)
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

                var atomoids = new List<(List<Vertex> verts, List<DM.Face> faces, string matName)>();
                foreach (var prim in mesh.Primitives)
                {
                    var vertices = GetVerticesFromPrimitive(prim).ToList();
                    var faces = prim.GetTriangleIndices().Select(i => new DM.Face { a = (ushort)i.A, b = (ushort)i.B, c = (ushort)i.C }).ToList();
                    var matname = prim.Material?.Name ?? "Material: Default Material";
                    atomoids.Add((vertices, faces, matname));
                }

                var md = new MeshData();

                uint currentBaseVertex = 0;
                uint currentBaseIndex = 0;
                foreach (var i in atomoids)
                {
                    var ra = new DM.RenderAtom()
                    {
                        BaseIndex = currentBaseIndex,
                        BaseVertex = currentBaseVertex,
                        MaterialId = (uint)ms.materials.IndexOf(i.matName)
                    };

                    ms.AppendVertices(i.verts);
                    ra.GeometrySliceLength = (uint)i.verts.Count;
                    currentBaseVertex += (uint)i.verts.Count;

                    ms.faces.AddRange(i.faces);
                    ra.TriangleCount = (uint)i.faces.Count;
                    currentBaseIndex += (uint)i.faces.Count * 3;

                    ms.renderAtoms.Add(ra);
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

                prim.VertexAccessors.TryGetValue("JOINTS_0", out var joints);
                prim.VertexAccessors.TryGetValue("WEIGHTS_0", out var weights);
                if(joints != null && joints.Count > 0)
                {
                    var ja = joints.AsVector4Array();
                    var wa = joints.AsVector4Array();
                    result = result.Select((vtx, idx) =>
                    {
                        var gltfWeight = ja[idx];
                        vtx.weight = new Vector3(gltfWeight.X, gltfWeight.Y, gltfWeight.Z);
                        if(gltfWeight.W > 0)
                        {
                            Log.Default.Warn($"{prim.LogicalParent.Name} has a vertex with >3 weights at {vtx.pos}!");
                        }
                        vtx.weightGroups = new DM.GeometryWeightGroups()
                        {
                            Bones1 = (ushort)wa[idx].X,
                            Bones2 = (ushort)wa[idx].Y,
                            Bones3 = (ushort)wa[idx].Z,
                            Bones4 = (ushort)wa[idx].W
                        };

                        return vtx;
                    });
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
            public Vector3? weight;
            public DM.GeometryWeightGroups weightGroups;

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
                       EqualityComparer<Vector2?[]>.Default.Equals(uv, other.uv) &&
                       EqualityComparer<Vector3?>.Default.Equals(weight, other.weight) &&
                       EqualityComparer<DM.GeometryWeightGroups>.Default.Equals(weightGroups, other.weightGroups);
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
                hashCode = hashCode * -1521134295 + EqualityComparer<Vector3?>.Default.GetHashCode(weight);
                hashCode = hashCode * -1521134295 + EqualityComparer<DM.GeometryWeightGroups>.Default.GetHashCode(weightGroups);
                return hashCode;
            }
        }
    }
}
