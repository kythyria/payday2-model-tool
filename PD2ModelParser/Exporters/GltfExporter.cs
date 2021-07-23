using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using SharpGLTF.Memory;

using GLTF = SharpGLTF.Schema2;

using PD2ModelParser.Sections;

namespace PD2ModelParser.Exporters
{
    class GltfExporter
    {
        public static string ExportFile(FullModelData data, string path, bool binary)
        {
            var exporter = new GltfExporter();
            var gltfmodel = exporter.Convert(data);
            if(binary)
            {
                gltfmodel.SaveGLB(path);
            }
            else
            {
                gltfmodel.SaveGLTF(path, new GLTF.WriteSettings { 
                    JsonIndented = true
                });
            }
            return path;
        }

        FullModelData data;
        GLTF.ModelRoot root;
        GLTF.Scene scene;
        Dictionary<ISection, GLTF.Material> materialsBySection;
        Dictionary<ISection, GLTF.Node> nodesBySection;
        List<(Model, GLTF.Node)> toSkin;

        /// <summary>
        /// How much to embiggen data as it's converted to GLTF.
        /// </summary>
        /// <remarks>
        /// See the remarks of <see cref="PD2ModelParser.Importers.GltfImporter.scaleFactor"/> 
        /// for why the implementation here works.
        /// </remarks>
        float scaleFactor = 0.01f;

        GLTF.ModelRoot Convert(FullModelData data)
        {
            materialsBySection = new Dictionary<ISection, GLTF.Material>();
            nodesBySection = new Dictionary<ISection, GLTF.Node>();
            toSkin = new List<(Model, GLTF.Node)>();

            this.data = data;
            root = GLTF.ModelRoot.CreateModel();
            scene = root.UseScene(0);

            // Blender's GLTF importer cares about some extras.
            /*var extras = scene.TryUseExtrasAsDictionary(true);
            extras.Add("glTF2ExportSettings", new SharpGLTF.IO.JsonDictionary
            {
                { "export_extras", 1 },
                { "export_lights", 1 }
            });*/

            foreach (var ms in data.parsed_sections.Where(i => i.Value is Material).Select(i => i.Value as Material))
            {
                materialsBySection[ms] = root.CreateMaterial(ms.HashName.String);
            }

            foreach(var i in data.SectionsOfType<Object3D>().Where(i => i.Parent == null))
            {
                CreateNodeFromObject3D(i, scene);
            }

            foreach(var (thing, node) in toSkin)
            {
                SkinModel(thing, node);
            }

            //root.MergeBuffers();

            return root;
        }

        void CreateNodeFromObject3D(Object3D thing, GLTF.IVisualNodeContainer parent)
        {
            var isSkinned = thing is Model m && m.SkinBones != null;
            if (isSkinned)
            {
                parent = scene;
            }

            var node = parent.CreateNode(thing.Name);

            nodesBySection[thing] = node;
            if (thing != null)
            {
                var istrs = Matrix4x4.Decompose(thing.Transform, out var scale, out var rotation, out var translation);
                if(!istrs)
                {
                    throw new Exception($"In object \"{thing.Name}\" ({thing.SectionId}), non-TRS matrix");
                }

                // We only did that to be sure it was a TRS matrix. Knowing it is, and knowing we only
                // want to affect the translation, less stability problems exist by directly changing
                // just the cells that are the translation part.

                var mat = thing.Transform;
                mat.Translation = mat.Translation * scaleFactor;

                node.LocalMatrix = isSkinned ? Matrix4x4.Identity : mat;
                
            }
            if (thing is Model mod)
            {
                if (mod.version == 3)
                {
                    node.Mesh = GetMeshForModel(mod);
                    if (mod.SkinBones != null)
                    {
                        toSkin.Add((mod, node));
                    }
                }
                else if (mod.version == 6)
                {
                    /*var extras = node.TryUseExtrasAsDictionary(true);
                    extras.Add("diesel.modelv6.unk7", mod.v6_unknown7);
                    extras.Add("diesel.modelv6.bound_min", new float[] { mod.BoundsMin.X, mod.BoundsMin.Y, mod.BoundsMin.Z });
                    extras.Add("diesel.modelv6.bound_max", new float[] { mod.BoundsMax.X, mod.BoundsMax.Y, mod.BoundsMax.Z });*/
                }
                else
                {
                    throw new Exception($"Model {mod.Name} is of unknown version {mod.version}");
                }
            }
            else if (thing is Light dl)
            {
                GLTF.PunctualLight lamp;
                switch(dl.LightType)
                {
                    case 1:
                        lamp = root.CreatePunctualLight(GLTF.PunctualLightType.Point);
                        break;
                    case 2:
                        lamp = root.CreatePunctualLight(GLTF.PunctualLightType.Spot);
                        break;
                    default:
                        throw new Exception($"{thing.Name} is an unknown light type {dl.LightType}");
                }

                lamp.Color = new Vector3(dl.Colour.R, dl.Colour.G, dl.Colour.B);
                lamp.Range = dl.FarRange * scaleFactor;
                /*var extras = lamp.TryUseExtrasAsDictionary(false);
                extras = lamp.TryUseExtrasAsDictionary(true);
                extras.Add("diesel.nearRange", dl.NearRange);
                extras.Add("diesel.unknown_6", dl.unknown_6);
                extras.Add("diesel.unknown_7", dl.unknown_7);*/
                node.PunctualLight = lamp;
            }

            foreach (var i in thing.children)
            {
                CreateNodeFromObject3D(i, node);
            }
        }

        void SkinModel(Model model, GLTF.Node node)
        {
            if (model.SkinBones == null)
            {
                return;
            }

            var skinbones = model.SkinBones;
            var skin = root.CreateSkin(model.Name + "_Skin");
            skin.Skeleton = nodesBySection[skinbones.ProbablyRootBone];

            var wt = node.WorldMatrix;
            node.LocalTransform = Matrix4x4.Identity;

            var joints2 = skinbones.bone_mappings[0].bones.Select(b => {
                var jointNode = nodesBySection[skinbones.Objects[(int)b]];
                var ibm = skinbones.rotations[(int)b];
                ibm.Translation *= scaleFactor;
                return (jointNode, ibm);
            }).ToArray();

            skin.BindJoints(joints2);
            node.Skin = skin;
        }

        GLTF.Mesh GetMeshForModel(Model model)
        {
            if(model.PassthroughGP == null)
            {
                return null;
            }

            var mesh = root.CreateMesh(model.Name);

            var secPassthrough = model.PassthroughGP;
            var geometry = secPassthrough.Geometry;
            var topology = secPassthrough.Topology;
            var materialGroup = model.MaterialGroup;

            var attribs = GetGeometryAttributes(geometry);

            foreach (var (indexAccessor,material) in CreatePrimitiveIndices(topology, model.RenderAtoms, materialGroup))
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

        IEnumerable<(GLTF.Accessor, GLTF.Material)> CreatePrimitiveIndices(Topology topo, IEnumerable<RenderAtom> atoms, MaterialGroup materialGroup)
        {
            var buf = new ArraySegment<byte>(new byte[topo.facelist.Count * 3 * 2]);
            var mai = new MemoryAccessInfo($"indices_{topo.HashName.String}", 0, topo.facelist.Count * 3, 0, GLTF.DimensionType.SCALAR, GLTF.EncodingType.UNSIGNED_SHORT);
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
                var atom_mai = new MemoryAccessInfo($"indices_{topo.HashName}_{atomcount++}", (int)ra.BaseIndex*2, (int)ra.TriangleCount*3, 0, GLTF.DimensionType.SCALAR, GLTF.EncodingType.UNSIGNED_SHORT);
                var atom_ma = new MemoryAccessor(buf, atom_mai);
                var accessor = root.CreateAccessor();
                accessor.SetIndexData(atom_ma);
                var material = materialsBySection[materialGroup.Items[(int)ra.MaterialId]];
                yield return (accessor, material);
            }
        }

        List<(string, GLTF.Accessor)> GetGeometryAttributes(Geometry geometry)
        {
            List<(string, GLTF.Accessor)> result;
            result = new List<(string, GLTF.Accessor)>();

            var a_pos = MakeVertexAttributeAccessor("vpos", geometry.verts.Select(i=>i*scaleFactor).ToList(), 12, GLTF.DimensionType.VEC3, i=>i, ma => ma.AsVector3Array());
            result.Add(("POSITION", a_pos));

            if (geometry.normals.Count > 0)
            {
                Vector3 MakeNormal(Vector3 norm, int idx)
                {
                    var normalized = Vector3.Normalize(norm);
                    if(!normalized.IsFinite())
                    {
                        Log.Default.Warn("Vertex {0} of geometry {1}|{2} is bogus ({3})", idx, geometry.SectionId, geometry.HashName.String, norm);
                        return new Vector3(1, 0, 0);
                    }
                    if(!normalized.IsUnitLength())
                    {
                        Log.Default.Warn("Vertex {0} of geometry {1}|{2} is bogus length {4} ({3})", idx, geometry.SectionId, geometry.HashName.String, norm, norm.Length());
                        return new Vector3(1, 0, 0);
                    }
                    return normalized;
                }
                var a_norm = MakeVertexAttributeAccessor("vnorm", geometry.normals, 12, GLTF.DimensionType.VEC3, MakeNormal, ma => ma.AsVector3Array());
                result.Add(("NORMAL", a_norm));
            }

            if (geometry.tangents.Count > 0)
            {
                Func<Vector3, int, Vector4> makeTangent = (input, index) =>
                {
                    var tangent = Vector3.Normalize(input);

                    if(!tangent.IsFinite())
                    {
                        Log.Default.Warn("Vertex {0} of geometry {1}|{2} has bogus tangent ({3})", index, geometry.SectionId, geometry.HashName.String, tangent);
                        tangent = new Vector3(0, 1, 0);
                    }
                    if(!tangent.IsUnitLength())
                    {
                        Log.Default.Warn("Vertex {0} of geometry {1}|{2} has bogus tangent length {4} ({3})", index, geometry.SectionId, geometry.HashName.String, tangent, tangent.Length());
                        tangent = new Vector3(0, 1, 0);
                    }

                    var binorm = geometry.binormals[index];
                    var normal = geometry.normals[index];

                    var txn = Vector3.Cross(tangent, normal);
                    var dot = Vector3.Dot(txn, binorm);
                    if(float.IsNaN(dot))
                    {
                        Log.Default.Warn("Weird normals in vtx {3} of geom {4}|{5}, N={0}, T={1}, B={2}, (T cross N) dot B is NaN", normal, tangent, binorm, index, geometry.SectionId, geometry.HashName.String);
                        return new Vector4(tangent, 1);
                    }
                    var sgn = float.IsNaN(dot) ? 1 : Math.Sign(dot);

                    // A few models have vertices where tangent==binorm, which is silly
                    // also breaks because SharpGLTF tries to do validation. So we return
                    // 1 in that case, which is probably also unhelpful. I'm not 100% sure
                    // how important having sane binormals is anyway.
                    return new Vector4(tangent, sgn != 0 ? sgn : 1);
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

            if (geometry.weights.Count > 0)
            {
                Vector4 ConvertWeight(Vector3 weight)
                {
                    // gltf spec requires weights sum to 1.0
                    var total = weight.X + weight.Y + weight.Z;
                    if (Math.Abs(1.0 - total) > 2e-7)
                    {
                        var largest = Math.Max(weight.X, Math.Max(weight.Y, weight.Z));
                        var fac = 1 / total;
                        weight *= fac;
                    }
                    return new Vector4(weight, 0);
                }

                var a_wght = MakeVertexAttributeAccessor("vweight", geometry.weights, 16, GLTF.DimensionType.VEC4, ConvertWeight, ma => ma.AsVector4Array());
                result.Add(("WEIGHTS_0", a_wght));
            }
            
            if(geometry.weight_groups.Count > 0)
            {
                // TODO: Is there a way that doesn't require round-tripping through float? It's unnecessary,
                // even if it doesn't actually hurt as such.
                Func<GeometryWeightGroups, Vector4> ConvertWeightGroup = (i)
                    => new Vector4(i.Bones1, i.Bones2, i.Bones3, i.Bones4);
                var a_joint = MakeVertexAttributeAccessor("vjoint", geometry.weight_groups, 8, GLTF.DimensionType.VEC4, ConvertWeightGroup, ma => ma.AsVector4Array(), GLTF.EncodingType.UNSIGNED_SHORT);
                result.Add(("JOINTS_0", a_joint));
            }

            return result;
        }

        Vector2 FixupUV(Vector2 input) => new Vector2(input.X, 1-input.Y);

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
