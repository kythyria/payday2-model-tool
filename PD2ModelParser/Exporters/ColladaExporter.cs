using Collada141;
using Nexus;
using PD2ModelParser.Sections;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static PD2ModelParser.Tags;

namespace PD2ModelParser
{
    static class ColladaExporter
    {
        public static string ExportFile(FullModelData data, string path)
        {
            path = path.Replace(".model", ".dae");

            List<SectionHeader> sections = data.sections;
            Dictionary<UInt32, ISection> parsed_sections = data.parsed_sections;
            byte[] leftover_data = data.leftover_data;

            // Set up the XML structure
            library_geometries libgeoms = new library_geometries();

            library_visual_scenes libscenes = new library_visual_scenes();

            COLLADAScene scene = new COLLADAScene
            {
                instance_visual_scene = new InstanceWithExtra
                {
                    url = "#scene"
                }
            };

            COLLADA collada = new COLLADA
            {
                Items = new object[] {libgeoms, libscenes},
                scene = scene,
                asset = new asset
                {
                    created = DateTime.UtcNow,
                    modified = DateTime.UtcNow,

                    // Otherwise it defaults to Y-up (see asset's constructor), while we're
                    //  using Z-up. Without the asset tag Blender defaults to Z-up, so if you
                    //  remove this then the presence of the asset tag flips the model.
                    up_axis = UpAxisType.Z_UP,
                },
            };

            // Build the mesh

            List<geometry> geometries = new List<geometry>();
            List<node> nodes = new List<node>();

            int model_i = 0;
            foreach (SectionHeader sectionheader in sections)
            {
                if (sectionheader.type == model_data_tag)
                {
                    Model model_data = (Model) parsed_sections[sectionheader.id];
                    if (model_data.version == 6)
                        continue;

                    model_i++;

                    string model_id = model_i + "-" + model_data.HashName.String;

                    PassthroughGP passthrough_section = (PassthroughGP) parsed_sections[model_data.passthroughGP_ID];
                    Geometry geometry_section = passthrough_section.Geometry;
                    Topology topology_section = passthrough_section.Topology;
                    geometry geom = SerializeModel(geometry_section, topology_section, model_data, model_id);

                    geometries.Add(geom);

                    node root_node = new node
                    {
                        id = "model-" + model_id,
                        name = "Model " + model_id,
                        type = NodeType.NODE,

                        instance_geometry = new instance_geometry[]
                        {
                            new instance_geometry
                            {
                                url = "#model-geom-" + model_id,
                                name = "Model Geom" + model_id
                            }
                        }
                    };

                    nodes.Add(root_node);

                    if (model_data.SkinBones == null)
                        continue;

                    SkinBones sb = model_data.SkinBones;
                    //Console.WriteLine(sb.bones);
                    //Console.WriteLine(sb);

                    node bone_root_node = new node
                    {
                        id = "model-" + model_id + "-boneroot",
                        name = "Model " + model_id + " Bones",
                        type = NodeType.NODE,
                        /*Items = new object[]
                            {
                                new matrix
                                {
                                    sid = "transform", // Apparently Blender really wants this
                                    Values = MathUtil.Serialize(sb.unknown_matrix)
                                }
                            },
                        ItemsElementName = new ItemsChoiceType2[]
                            {
                                ItemsChoiceType2.matrix
                            }*/
                    };
                    root_node.node1 = new node[] {bone_root_node};

                    Dictionary<UInt32, node> bones = new Dictionary<UInt32, node>();

                    // In order to find locators, which aren't present in the SkinBones
                    // object, we check the child of each object we process.
                    //
                    // To then process those, we use a queue (to_parse) which lists the
                    // objects in our TODO list to search. We draw from this as we process them.
                    //
                    // We also keep the list of what we have processed in (parsed), so we don't
                    // process anything twice.
                    //
                    // TODO rewrite this code to be based around children, removing the need for a seperate
                    // loop after this that arranges the heirachy of nodes.
                    List<uint> parsed = new List<uint>(sb.objects);
                    Queue<uint> to_parse = new Queue<uint>();

                    foreach (UInt32 id in sb.objects)
                    {
                        to_parse.Enqueue(id);
                    }

                    int i = 0;
                    while (to_parse.Count != 0)
                    {
                        uint id = to_parse.Dequeue();

                        // This used to hard cast the section out - unfortunately I didn't
                        // make a comment when I wrote this and it's been like six months, so
                        // I can only guess that a model node found it's way in here?
                        Object3D obj = parsed_sections[id] as Object3D;
                        if (obj == null)
                            continue;

                        string bonename = obj.HashName.String;

                        // Find the locators and such, and add them to the TODO list
                        foreach (Object3D child in obj.children)
                        {
                            if (!parsed.Contains(child.SectionId)) // Don't process something twice
                            {
                                parsed.Add(child.SectionId);
                                to_parse.Enqueue(child.SectionId);
                            }
                        }

                        Vector3D translate;
                        Quaternion rotate;
                        Vector3D scale;
                        obj.Transform.Decompose(out scale, out rotate, out translate);

                        Matrix3D final_rot = Matrix3D.CreateFromQuaternion(rotate);
                        final_rot.Translation = translate;

                        if (obj.Parent == null || !sb.objects.Contains(obj.parentID))
                        {
                            Matrix3D fixed_obj_transform = obj.WorldTransform;
                            fixed_obj_transform.Decompose(out scale, out rotate, out translate);

                            // Fixes the head, but breaks the arms
                            // For now, leave it like this
                            //final_rot = final_rot.MultDiesel(fixed_obj_transform);
                        }

                        // If the object is not contained within the SkinBones
                        // object, it must be a locator.
                        bool locator = !sb.objects.Contains(id);

                        // Add the node
                        bones[id] = new node
                        {
                            id = "model-" + model_id + "-bone-" + bonename,
                            name = (locator ? "locator-" : "") + bonename,
                            type = NodeType.JOINT,
                            Items = new object[]
                            {
                                new matrix
                                {
                                    sid = "transform", // Apparently Blender really wants this
                                    Values = MathUtil.Serialize(final_rot)
                                },
                            },
                            ItemsElementName = new ItemsChoiceType2[]
                            {
                                ItemsChoiceType2.matrix
                            }
                        };

                        i++;
                    }

                    foreach (var nod in bones)
                    {
                        Object3D obj = (Object3D) parsed_sections[nod.Key];

                        node parent = bone_root_node;

                        if (bones.ContainsKey(obj.parentID))
                        {
                            parent = bones[obj.parentID];
                        }

                        if (parent.node1 == null)
                        {
                            parent.node1 = new node[1];
                        }
                        else
                        {
                            node[] children = parent.node1;
                            Array.Resize(ref children, children.Length + 1);
                            parent.node1 = children;
                        }

                        parent.node1[parent.node1.Length - 1] = nod.Value;
                    }
                }
            }

            libgeoms.geometry = geometries.ToArray();

            libscenes.visual_scene = new visual_scene[]
            {
                new visual_scene
                {
                    id = "scene",
                    name = "Scene",
                    node = nodes.ToArray()
                }
            };

            using (FileStream fs = new FileStream(path, FileMode.Create, FileAccess.Write))
            {
                collada.Save(fs);

                // Do we need this?
                // fs.Close();
            }

            return path;
        }

        private static geometry SerializeModel(Geometry geometry_section, Topology topology_section, Model model_data,
            string id)
        {
            string VERT_ID = "vertices-" + id;
            string NORM_ID = "norms-" + id;
            string UV_ID = "uv-" + id;
            string RAW_VERT_ID = "vert_raw-" + id;

            int vertlen = geometry_section.verts.Count;
            int normlen = geometry_section.normals.Count;
            int uvlen = geometry_section.uvs.Count;

            triangles triangles = new triangles();

            List<InputLocalOffset> inputs = new List<InputLocalOffset>();

            inputs.Add(new InputLocalOffset
            {
                semantic = "VERTEX",
                source = "#" + VERT_ID,
                offset = 0,
            });

            if (normlen > 0)
            {
                inputs.Add(new InputLocalOffset
                {
                    semantic = "NORMAL",
                    source = "#" + NORM_ID,
                    offset = 0,
                });
            }

            if (uvlen > 0)
            {
                inputs.Add(new InputLocalOffset
                {
                    semantic = "TEXCOORD",
                    source = "#" + UV_ID,
                    offset = 0,
                    set = 1, // IDK what this does or why we need it
                });
            }

            triangles.input = inputs.ToArray();

            mesh mesh = new mesh
            {
                vertices = new vertices
                {
                    id = VERT_ID,
                    input = new InputLocal[]
                    {
                        new InputLocal
                        {
                            semantic = "POSITION",
                            source = "#" + RAW_VERT_ID
                        }
                    }
                },
                Items = new object[]
                {
                    triangles
                }
            };

            StringBuilder facesData = new StringBuilder("\n"); // Start on a newline

            foreach (Face face in topology_section.facelist)
            {
                if (!face.BoundsCheck(vertlen))
                {
                    throw new Exception("Vert Out Of Bounds!");
                }

                if (normlen > 0 && !face.BoundsCheck(normlen))
                {
                    throw new Exception("Norm Out Of Bounds!");
                }

                if (uvlen > 0 && !face.BoundsCheck(uvlen))
                {
                    throw new Exception("UV Out Of Bounds!");
                }

                // This set is used for the Vertices, Normals and UVs, as they all have the same indexes
                // A bit inefficent in terms of storage space, but easier to implement as that's how it's
                // handled inside the Diesel model files and making a index remapper thing would be a pain.
                facesData.AppendFormat("{0} {1} {2}\n", face.a, face.b, face.c);

                triangles.count++;
            }

            triangles.p = facesData.ToString();

            List<source> sources = new List<source>();

            sources.Add(GenerateSource(RAW_VERT_ID, geometry_section.verts));

            if (normlen > 0)
                sources.Add(GenerateSource(NORM_ID, geometry_section.normals));

            if (uvlen > 0)
                sources.Add(GenerateSourceTex(UV_ID, geometry_section.uvs));

            mesh.source = sources.ToArray();

            return new geometry
            {
                name = "Diesel Converted Model " + id,
                id = "model-geom-" + id,
                Item = mesh
            };
        }

        private static source GenerateSource(string name, List<Vector3D> vecs)
        {
            return GenerateSource(name, new string[] {"X", "Y", "Z"}, vecs, VecToFloats);
        }

        private static source GenerateSource(string name, List<Vector2D> vecs)
        {
            return GenerateSource(name, new string[] {"X", "Y"}, vecs, VecToFloats);
        }

        private static source GenerateSourceTex(string name, List<Vector2D> vecs)
        {
            return GenerateSource(name, new string[] {"S", "T"}, vecs, VecToFloats);
        }

        private static double[] VecToFloats(Vector3D vec)
        {
            return new double[] {vec.X, vec.Y, vec.Z};
        }

        private static double[] VecToFloats(Vector2D vec)
        {
            return new double[] {vec.X, vec.Y};
        }

        private static source GenerateSource<T>(string id, string[] paramnames, List<T> list,
            Func<T, double[]> converter)
        {
            float_array verts = new float_array();
            source source = new source
            {
                id = id,
                name = id,
                Item = verts
            };

            List<double> values = new List<double>();

            int length = -1;

            foreach (T item in list)
            {
                double[] vals = converter(item);

                if (length == -1)
                {
                    length = vals.Length;
                }
                else if (vals.Length != length)
                {
                    throw new Exception("Incompatable lengths!");
                }

                values.AddRange(vals);
            }

            verts.Values = values.ToArray();
            verts.count = (ulong) verts.Values.LongLength;
            verts.id = id + "-data";
            verts.name = verts.id;

            param[] indexes = new param[paramnames.Length];

            for (int i = 0; i < paramnames.Length; i++)
            {
                indexes[i] = new param
                {
                    name = paramnames[i],
                    type = "float"
                };
            }

            source.technique_common = new sourceTechnique_common
            {
                accessor = new accessor
                {
                    source = "#" + verts.id,
                    count = (ulong) list.Count,
                    stride = (ulong) length,
                    param = indexes
                }
            };

            return source;
        }
    }
}
