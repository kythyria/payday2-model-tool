using System;
using System.Collections.Generic;
using System.Linq;
using FbxNet;
using Nexus;
using PD2ModelParser.Misc;
using PD2ModelParser.Sections;

namespace PD2ModelParser.Importers
{
    public class FilmboxImporter
    {
        private static FbxManager _fm;

        public static void Import(FullModelData data, string filepath, bool addNew,
            Func<string, Object3D> rootPointResolver, IOptionReceiver options)
        {
            Import(data, filepath, addNew, rootPointResolver, (FbxImportOptions)options);
        }

        public static void Import(FullModelData data, string filepath, bool addNew,
            Func<string, Object3D> rootPointResolver, FbxImportOptions options)
        {
            if (_fm == null)
                _fm = FbxManager.Create();

            FbxImporter importer = FbxImporter.Create(_fm, "");
            bool result = importer.Initialize(filepath);
            if (!result)
                throw new Exception("EFBX001 Cannot load FBX file");

            FbxScene scene = FbxScene.Create(_fm, "");
            result = importer.Import(scene);
            // TODO add FbxIOBase to FbxNet so we can access the error code
            if (!result)
                throw new Exception("EFBX002 Cannot import FBX file");

            FilmboxImporter imp = new FilmboxImporter(data, options);

            List<FbxNode> meshes = new List<FbxNode>();
            imp.RecurseMeshes(scene.GetRootNode(), meshes);

            foreach (FbxNode node in meshes)
            {
                FbxMesh mesh = node.GetMesh();

                Object3D parent = rootPointResolver.Invoke(node.GetName());

                imp.AddMesh(parent, node, mesh);
            }
        }

        private readonly FullModelData data;
        private readonly FbxImportOptions _options;
        private readonly Dictionary<ulong, Object3D> _objects = new Dictionary<ulong, Object3D>();

        private FilmboxImporter(FullModelData data, FbxImportOptions options)
        {
            this.data = data;
            _options = options;

            foreach (var item in data.SectionsOfType<Object3D>())
            {
                _objects[item.HashName.Hash] = item;
            }
        }

        private Model CreateEmptyMesh(Object3D parent, string name)
        {
            // The basic geometry information - vertices, normals, UVs, but notably no faces
            Geometry geom = CreateGeometry();

            // Faces
            Topology topo = new Topology(0, name);
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
            MaterialGroup mat_g = new MaterialGroup(0, mat);
            data.AddSection(mat_g);

            // Used for some internal model stuff
            obj_data fake_obj = new obj_data
            {
                object_name = name,
                verts = geom.verts,
                faces = topo.facelist,
            };

            // Build the model itself
            Model model = new Model(fake_obj, pgp, tip, mat_g, parent);
            data.AddSection(model);

            return model;
        }

        private Model AddMesh(Object3D parent, FbxNode node, FbxMesh mesh)
        {
            string name = node.GetName();
            FbxNode skeleton_root = null;
            if (mesh.GetDeformerCount(FbxDeformer.EDeformerType.eSkin) != 0)
            {
                // We can't rely on consistent parents and take the name from that, see below
                name = node.GetName();

                if (!name.EndsWith("Object"))
                    throw new Exception("EFBX003 Mesh objects must currently end with the 'Object' suffix");

                name = node.GetName().Substring(0, name.Length - "Object".Length);

                FbxSkin skin = mesh.GetDeformer(0, FbxDeformer.EDeformerType.eSkin).CastToSkin();

                // Blender seems to put the root node at index 0, but go through and
                // find the root one just to be sure, and to allow imports from other
                // software.
                // Blender also omits the root skeleton node and makes the mesh no longer
                // a child of that, which of course means we can't just look up it's
                // parent's skeleton like we used to.
                skeleton_root = skin.GetCluster(0).GetLink();
                while (skeleton_root.GetParent()?.GetSkeleton() != null)
                {
                    skeleton_root = skeleton_root.GetParent();
                }
            }

            Model model;
            if (_objects.TryGetValue(Hash64.HashString(name), out Object3D existing_object))
            {
                model = (Model)existing_object;
            }
            else
            {
                model = CreateEmptyMesh(parent, name);
            }

            Dictionary<uint, ISection> parsed = data.parsed_sections;
            PassthroughGP pgp = model.PassthroughGP;
            Geometry geom = pgp.Geometry;
            Topology topo = pgp.Topology;

            BuildGeometry(mesh, geom);
            BuildTopology(topo, mesh);

            BuildUVs(mesh, geom);

            // Add the bones - note this *only* adds the skeleton, and not any weights
            Dictionary<ulong, Object3D> skel = AddSkeleton(skeleton_root, model, parent, out Object3D root_bone);
            if (skel == null)
                return model;

            // Parent the model to the root bone, as per the cop model where the meshes are parented to
            // the hip bone.
            // Note that the model only had one skeleton, shared between all models - this will probably break
            // it quite a bit if we try and export them all back in.
            model.Parent = root_bone;

            AddWeights(mesh, skel, model, geom);

            return model;
        }

        private Dictionary<ulong, Object3D> AddSkeleton(FbxNode rootNode, Model model,
            Object3D rootPoint, out Object3D rootBone)
        {
            FbxSkeleton root_skeleton = rootNode?.GetSkeleton();
            if (root_skeleton == null)
            {
                rootBone = null;
                return null;
            }

            Dictionary<ulong, Object3D> objs = new Dictionary<ulong, Object3D>();

            Object3D root_bone = null;

            // Temporary hack:
            // With the unified skeleton exports, we're getting bones like 'neck' in the
            // body skeleton which breaks everything. For this reason, ignore any bones
            // not in the previous SkinBones.
            // Ideally we'd fix this properly via mscript, but this will do for now.
            SkinBones previous = model.SkinBones;

            SkinBones sb = new SkinBones(0);
            data.AddSection(sb);
            model.SkinBones = sb;

            Recurse(rootNode, rootPoint, (node, parent) =>
            {
                FbxSkeleton skel = node.GetSkeleton();
                if (skel == null || skel.GetSkeletonType() == FbxSkeleton.EType.eRoot)
                    return parent;

                string name = node.GetName();
                bool isBone = true;
                if (name.EndsWith(FbxUtils.LocatorSuffix))
                {
                    name = name.Substring(0, name.Length - FbxUtils.LocatorSuffix.Length);
                    isBone = false;
                }

                // Look up if there's an existing object matching this object
                _objects.TryGetValue(Hash64.HashString(name), out Object3D obj);

                if (obj == null)
                {
                    obj = new Object3D(name, parent);
                    parent?.children?.Add(obj);
                    data.AddSection(obj);
                }

                if (root_bone == null)
                {
                    root_bone = obj;
                    sb.global_skin_transform = obj.Transform;
                }
                else if (parent == rootPoint)
                {
                    throw new Exception("EFBX004 Each rigged model must have only one root bone");
                }

                objs[node.PtrHashCode()] = obj;

                // Another little hack
                // Currently whether skeletons have the _locator suffix or not depends on
                // which model was first exported and created the skeleton. As such later
                // models might use these 'locators' as actual bones. However just fixing
                // the exporter won't be enough - all those bones will then get imported
                // back in and we'll exceed the bone limit.
                // As a temporary hack until we can get this done in XML, use the bones we're
                // replacing to determine if this is actually a bone or not, and completely
                // ignore the locator suffix.
                // if (!isBone)
                //     return obj;

                // See above, wrt the hack to get around extra bones
                if (!previous.objects.Contains(obj.SectionId))
                    return obj;

                // Bone transforms seem to get messed up really easily. Since changing bone
                // transforms will likely break animations anyway, make the user opt-in if
                // they want to move bones.
                if (_options.ImportBoneTransforms)
                {
                    // Note the field is named badly - it's a transform, not just a rotation
                    obj.Transform = node.GetNexusTransform();
                }

                sb.objects.Add(obj.SectionId);

                // TODO implement
                // ZNix's 10/11/19 notes on how the rotation and global_skin_transform seem
                // to work (on ene_security_3): Archived in Git history, check out an older version
                // to see the old and incorrect nodes.
                // As it turns out, this just has to be the inverse of the object's world transform matrix
                Matrix3D rotation = obj.WorldTransform;
                rotation.Invert();
                sb.rotations.Add(rotation);

                return obj;
            });

            // No bones :(
            // We could continue here, but it's almost certainly not what the
            // user would expect and a loud error is almost always better than
            // a silent failure.
            if (root_bone == null)
                throw new Exception("EFBX005 Rigged model " + rootNode.GetName() + " has no bones");

            sb.ProbablyRootBone = root_bone;
            rootBone = root_bone;

            // TODO more research into how this works
            // This seems to be an index mapping through to bones from RenderAtoms
            BoneMappingItem bmi = new BoneMappingItem();
            for (uint i = 0; i < sb.count; i++)
                bmi.bones.Add(i);

            for (int i = 0; i < model.RenderAtoms.Count; i++)
                sb.bone_mappings.Add(bmi);

            // TODO setup the other SkinBones fields - probably very important for Diesel
            return objs;
        }

        private void AddWeights(FbxMesh mesh,
            Dictionary<ulong, Object3D> skel, Model model, Geometry geom)
        {
            int deformer_count = mesh.GetDeformerCount(FbxDeformer.EDeformerType.eSkin);
            if (deformer_count == 0) return;
            if (deformer_count != 1)
                throw new Exception("EFBX006 Only one skin per mesh is supported");

            FbxSkin skin = mesh.GetDeformer(0, FbxDeformer.EDeformerType.eSkin).CastToSkin();
            if (skin == null)
                throw new Exception("EFBX007 Could not get skin deformer ID=0");

            SkinBones sb = model.SkinBones;

            // Either 2 for low-LOD models or 3 for high-LOD models - afaik this
            // has something to do with which render template is used.
            // TODO confirm if this is true, and if so allow selection of the render template somehow
            if (!geom.HasHeader(GeometryChannelTypes.BLENDWEIGHT))
            {
                geom.headers.Add(new GeometryHeader(3, GeometryChannelTypes.BLENDWEIGHT));
            }

            // Odd size, but consistent when taken from default models
            if (!geom.HasHeader(GeometryChannelTypes.BLENDINDICES))
            {
                geom.headers.Add(new GeometryHeader(7, GeometryChannelTypes.BLENDINDICES));
            }

            // Clear the channels out, required if we're overwriting a model
            geom.weights.Clear();
            geom.weight_groups.Clear();

            // Build a lookup table to find the index of a given bone
            Dictionary<Object3D, int> bone_indices = new Dictionary<Object3D, int>();
            for (int i = 0; i < sb.count; i++)
            {
                Object3D obj = (Object3D) data.parsed_sections[sb.objects[i]];
                bone_indices[obj] = i;
            }

            // FBX (roughly, via clusters) stores the vertices/weights for each bone, while Diesel
            // stores the bones/weights for each vertex. This list corresponds to each
            // vertex in the model so we can flip this around.
            List<WeightPart>[] parts = new List<WeightPart>[geom.vert_count];
            for (int i = 0; i < geom.vert_count; i++)
                parts[i] = new List<WeightPart>();

            for (int i = 0; i < skin.GetClusterCount(); i++)
            {
                FbxCluster cluster = skin.GetCluster(i);

                FbxNode bone_node = cluster.GetLink();
                Object3D bone = skel[bone_node.PtrHashCode()];

                // Blender seems to add clusters for all it's bones, even ones which
                // don't attach to anything.
                if (!bone_indices.ContainsKey(bone) && cluster.GetControlPointIndicesCount() == 0)
                    continue;

                if (!bone_indices.ContainsKey(bone))
                {
                    throw new Exception($"EFBX008 Model {model.Name} uses bone {bone.Name} which is "
                                        + "unavailable in this model");
                }

                int idx = bone_indices[bone];

                SWIGTYPE_p_int indices = cluster.GetControlPointIndices();
                SWIGTYPE_p_double weights = cluster.GetControlPointWeights();
                for (int j = 0; j < cluster.GetControlPointIndicesCount(); j++)
                {
                    int vert_idx = FbxNet.FbxNet.intArray_getitem(indices, j);
                    double weight = FbxNet.FbxNet.doubleArray_getitem(weights, j);

                    List<WeightPart> vert = parts[vert_idx];

                    if (vert.Any(p => p.boneID == idx))
                        throw new Exception("EFBX009 Two clusters for the same bone and vertex " +
                                            "are currently unsupported");

                    vert.Add(new WeightPart
                    {
                        Bone = bone,
                        boneID = idx,
                        weight = (float) weight,
                    });
                }
            }

            for (int i = 0; i < geom.vert_count; i++)
            {
                AddWeightsForVertex(parts[i], geom, model, i);
            }
        }

        private void AddWeightsForVertex(List<WeightPart> parts, Geometry geom, Model model, int vertIdx)
        {
            List<WeightPart> processed = ProcessWeights(parts);

            if (_options.NormaliseWeights)
            {
                float total_weights = processed.Sum(v => v.weight);
                if (total_weights < 0.1 && total_weights > 10)
                {
                    throw new Exception($"Weights sum {total_weights} out of range!");
                }

                float factor = 1 / total_weights;
                foreach (WeightPart w in processed)
                {
                    w.weight *= factor;
                }
            }

            // AFAIK this is affected by the header thing - see above
            if (processed.Count > 3)
            {
                Vector3D vert = geom.verts[vertIdx];
                string dbg = $"{model.Name}: vert at {vert.X},{vert.Y},{vert.Z}";
                dbg = processed.Aggregate(dbg, (current, part) => current + $"\n{part.Bone.Name} weight {part.weight}");
                throw new Exception("EFBX010 Vertices cannot be affected by more than three bones:\n" + dbg);
            }

            Vector3D weights = Vector3D.Zero;
            GeometryWeightGroups groups = new GeometryWeightGroups();

            int wi = 0;
            foreach (WeightPart part in processed)
            {
                if (part.boneID > ushort.MaxValue)
                    throw new Exception("EFBX011 Too many bones!");

                weights[wi] = part.weight;

                ushort bid = (ushort) part.boneID;
                switch (wi)
                {
                    case 0:
                        groups.Bones1 = bid;
                        break;
                    case 1:
                        groups.Bones2 = bid;
                        break;
                    case 2:
                        groups.Bones3 = bid;
                        break;
                    default:
                        throw new Exception(); // Should already be stopped above
                }

                wi++;
            }

            geom.weights.Add(weights);
            geom.weight_groups.Add(groups);
        }

        private List<WeightPart> ProcessWeights(List<WeightPart> orig)
        {
            List<WeightPart> sorted = orig.OrderByDescending(v => v.weight).ToList();

            // TODO use the header length to find out the max value, so we don't try
            // and stuff three weights into a two-weight LOD model
            const int max_weights = 3;

            if (sorted.Count <= max_weights || _options.WeightRoundingThreshold == null)
                return sorted;

            // Check that we're about to round weights within the allowable tolerance
            float to_round = 0;
            for (int i = max_weights; i < sorted.Count; i++)
            {
                to_round += sorted[i].weight;
            }

            // If we would be rounding off more than the user has allowed, abort
            if (to_round > _options.WeightRoundingThreshold)
                return sorted;

            // Remove the weights in question
            sorted.RemoveRange(max_weights, sorted.Count - max_weights);

            return sorted;
        }

        private Geometry CreateGeometry()
        {
            Geometry geom = new Geometry(0);
            data.AddSection(geom);

            // TODO cleanup
            geom.headers.Add(new GeometryHeader(3, GeometryChannelTypes.POSITION)); // vert
            geom.headers.Add(new GeometryHeader(2, GeometryChannelTypes.TEXCOORD0)); // uv
            geom.headers.Add(new GeometryHeader(3, GeometryChannelTypes.NORMAL0)); // norm
            geom.headers.Add(new GeometryHeader(3, GeometryChannelTypes.BINORMAL0)); // unk20
            geom.headers.Add(new GeometryHeader(3, GeometryChannelTypes.TANGENT0)); // unk21

            return geom;
        }

        private void BuildGeometry(FbxMesh mesh, Geometry geom)
        {
            geom.vert_count = (uint) mesh.GetControlPointsCount();

            geom.verts.Clear();
            geom.normals.Clear();
            geom.vertex_colors.Clear();
            foreach (List<Vector2D> uvs in geom.UVs) uvs.Clear();

            FbxLayerElementNormal normals = mesh.GetElementNormal();
            List<int>[] cp_to_entries = FindPerVertEntries(mesh, normals, normals.GetIndexArray());

            for (int i = 0; i < mesh.GetControlPointsCount(); i++)
            {
                FbxVector4 v = mesh.GetControlPointAt(i);
                geom.verts.Add(v.V3());

                // Average out all the normals, since if we're in per-poly-vert mode there
                // will be more than one normal per control point.
                // TODO will this break them, and is there a better way to do it?
                Vector3D norm = Vector3D.Zero;
                foreach (int cp in cp_to_entries[i])
                {
                    FbxVector4 n = normals.GetDirectArray().GetAt(cp);
                    if (n.V3().Length() < 0.1)
                        throw new Exception("EFBX012 Short normal!");

                    norm += n.V3();
                    n.Dispose();
                }

                norm.Normalize();

                geom.normals.Add(norm);

                // Normally I don't care about leaving stuff around as it'll be cleaned
                // up when the C# GC eats it, but in this case it might be a bit too much.
                v.Dispose();
            }

            AddVertexColours(mesh, geom);
        }

        private void AddVertexColours(FbxMesh mesh, Geometry geom)
        {
            int vertex_colour_count = mesh.GetElementVertexColorCount();
            if (vertex_colour_count > 1)
                throw new Exception("EFBX013 The model tool does not support more than one vertex colour layer");

            if (vertex_colour_count == 0) return;

            // TODO confirm the size is indeed 3
            if (!geom.HasHeader(GeometryChannelTypes.COLOR))
                geom.headers.Add(new GeometryHeader(3, GeometryChannelTypes.COLOR));
            FbxLayerElementVertexColor layer = mesh.GetElementVertexColor();

            List<int>[] cp_to_entries = FindPerVertEntries(mesh, layer, layer.GetIndexArray());

            FbxLayerElementArrayTemplateColour array = layer.GetDirectArray();

            for (int i = 0; i < geom.vert_count; i++)
            {
                int r = 0, g = 0, b = 0, a = 0;
                int count = 0;

                foreach (int idx in cp_to_entries[i])
                {
                    count++;
                    GeometryColor point_colour = array.GetAt(idx).ToGeomColour();
                    r += point_colour.red;
                    g += point_colour.green;
                    b += point_colour.blue;
                    a += point_colour.alpha;
                }

                GeometryColor colour = new GeometryColor
                {
                    red = (byte) (r / count),
                    green = (byte) (g / count),
                    blue = (byte) (b / count),
                    alpha = (byte) (a / count),
                };

                geom.vertex_colors.Add(colour);
            }
        }

        private void BuildTopology(Topology topo, FbxMesh mesh)
        {
            topo.facelist.Clear();

            for (int i = 0; i < mesh.GetPolygonCount(); i++)
            {
                int size = mesh.GetPolygonSize(i);
                if (size != 3)
                    throw new Exception("EFBX014 Triangles are the only supported type of polygon");

                int a = mesh.GetPolygonVertex(i, 0);
                int b = mesh.GetPolygonVertex(i, 1);
                int c = mesh.GetPolygonVertex(i, 2);
                // TODO verify the indices are within bounds
                Face f = new Face {a = (ushort) a, b = (ushort) b, c = (ushort) c};
                topo.facelist.Add(f);
            }
        }

        private void BuildUVs(FbxMesh mesh, Geometry geom)
        {
            for (int i = 0; i < mesh.GetElementUVCount(); i++)
            {
                FbxLayerElementUV layer = mesh.GetElementUV(i);

                int gi;
                switch (layer.GetName())
                {
                    case "PrimaryUV":
                        gi = 0;
                        break;
                    case "UV0":
                    case "UV1":
                    case "UV2":
                    case "UV3":
                    case "UV4":
                    case "UV5":
                    case "UV6":
                    case "UV7":
                        gi = layer.GetName()[2] - '0';
                        break;
                    default:
                        throw new Exception("EFBX015 Unknown UV " + layer.GetName());
                }

                List<int>[] cp_to_entries = FindPerVertEntries(mesh, layer, layer.GetIndexArray());

                FbxLayerElementArrayTemplateVector2 direct = layer.GetDirectArray();
                List<Vector2D> uv = geom.UVs[gi];

                for (int j = 0; j < geom.vert_count; j++)
                {
                    List<int> entries = cp_to_entries[j];

                    Vector2D v = Vector2D.Zero;

                    foreach (int idx in entries)
                    {
                        Vector2D vv = direct.GetAt(idx).V2();
                        v.X += vv.X;
                        v.Y += vv.Y;
                    }

                    v.X /= entries.Count;
                    v.Y /= entries.Count;

                    uv.Add(v);
                }
            }
        }

        private void RecurseMeshes(FbxNode root, List<FbxNode> meshes)
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

        private void Recurse<T>(FbxNode node, T ud, Func<FbxNode, T, T> callback)
        {
            T sub = callback(node, ud);
            for (int i = 0; i < node.GetChildCount(); i++)
            {
                FbxNode child = node.GetChild(i);
                Recurse(child, sub, callback);
            }
        }

        /// <summary>
        /// For a given layer element, produce a mapping of control points
        /// to all the values in the direct array they correspond to.
        /// </summary>
        private static List<int>[] FindPerVertEntries(FbxMesh mesh, FbxLayerElement elem,
            FbxLayerElementArrayTemplateInt indexArray)
        {
            List<int>[] points = new List<int>[mesh.GetControlPointsCount()];
            for (int i = 0; i < points.Length; i++)
            {
                points[i] = new List<int>();
            }

            switch (elem.GetMappingMode())
            {
                case FbxLayerElement.EMappingMode.eByControlPoint:
                    // 1:1 mapping of control points to the positions in the index array
                    for (int i = 0; i < points.Length; i++)
                    {
                        points[i].Add(i);
                    }

                    break;
                case FbxLayerElement.EMappingMode.eByPolygonVertex:
                    // Map each control point to each of the polygon verts that reference it
                    SWIGTYPE_p_int vertices = mesh.GetPolygonVertices();
                    int poly_count = mesh.GetPolygonCount();

                    int vert_id = 0;
                    for (int poly = 0; poly < poly_count; poly++)
                    {
                        int vert_count = mesh.GetPolygonSize(poly);
                        for (int vert = 0; vert < vert_count; vert++)
                        {
                            int control_point = FbxNet.FbxNet.intArray_getitem(vertices, vert_id);
                            points[control_point].Add(vert_id);
                            vert_id++;
                        }
                    }

                    break;
                default:
                    throw new Exception("EFBX016 Unsupported mapping mode " + elem.GetMappingMode());
            }

            switch (elem.GetReferenceMode())
            {
                case FbxLayerElement.EReferenceMode.eDirect:
                    // Already correct, CP maps straight to the direct array
                    break;
                case FbxLayerElement.EReferenceMode.eIndexToDirect:
                    // Look up each point in the index array
                    foreach (List<int> elems in points)
                    {
                        for (int i = 0; i < elems.Count; i++)
                        {
                            elems[i] = indexArray.GetAt(elems[i]);
                        }
                    }

                    break;
                default:
                    throw new Exception("Unknown reference mode " + elem.GetReferenceMode());
            }

            return points;
        }

        private class WeightPart
        {
            public Object3D Bone;
            public int boneID;
            public float weight;
        }

        public class FbxImportOptions : IOptionReceiver
        {
            public float? WeightRoundingThreshold = null;
            public bool NormaliseWeights = true;
            public bool ImportBoneTransforms = false;

            public void AddOption(string name, string value)
            {
                switch (name)
                {
                    case "weight-rounding-threshold":
                        WeightRoundingThreshold = value.ParseFloat();
                        break;
                    case "normalise-weights":
                        NormaliseWeights = bool.Parse(value);
                        break;
                    case "import-bone-transforms":
                        ImportBoneTransforms = bool.Parse(value);
                        break;
                    default:
                        throw new Exception($"Unsupported option type '{name}' for FBX import");
                }
            }
        }
    }
}
