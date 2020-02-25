using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using System.Xml.Linq;
using System.Linq;
using Nexus;
using PD2ModelParser.Importers;
using PD2ModelParser.Sections;

namespace PD2ModelParser
{
    public static class ModelScript
    {
        public static void Execute(FullModelData data, string filename)
        {
            XElement root = XElement.Parse(System.IO.File.ReadAllText(filename));
            string workdir = Directory.GetParent(filename).FullName;
            Execute(data, root, workdir);
        }

        public static void Execute(FullModelData data, XElement root, string workdir)
        {
            if (root.Name != "modelscript")
                throw new Exception("Script root node is not named \"modelscript\"");

            foreach (XElement element in root.Elements())
            {
                switch (element.Name.ToString())
                {
                    case "object3d":
                        ExecObject3DElement(data, element);
                        break;
                    case "import":
                        ExecImport(data, element, workdir);
                        break;
                    case "patternuv":
                        ExecPatternUV(data, element, workdir);
                        break;
                    default:
                        throw new Exception($"Unknown node name \"{element.Name}\"");
                }
            }
        }

        public static bool ExecuteHandled(FullModelData data, XElement script, string workdir)
        {
            try
            {
                ModelScript.Execute(data, script, workdir);
                return true;
            }
            catch (Exception exc)
            {
                Log.Default.Warn("Exception in script file: {0}", exc);
                MessageBox.Show("There was an error in the script file:\n" +
                                "(check the readme on GitLab for more information)\n" + exc.Message);
                return false;
            }
        }

        public static bool ExecuteHandled(FullModelData data, string filename)
        {
            XElement root = XElement.Parse(System.IO.File.ReadAllText(filename));
            string workdir = Directory.GetParent(filename).FullName;
            return ExecuteHandled(data, root, workdir);
        }

        private static Object3D FindObjectByHashname(FullModelData data, ulong hashname)
        {
            return data.SectionsOfType<Object3D>().Where(i => i.HashName.Hash == hashname).FirstOrDefault();
        }

        private static string CheckAttr(XElement elem, string attr)
        {
            return elem.Attribute(attr)?.Value ??
                   throw new Exception($"Missing \"{attr}\" attribute for {elem.Name} element");
        }

        private static Vector3D CheckAttrVec(XElement elem)
        {
            return new Vector3D
            {
                X = CheckAttr(elem, "x").ParseFloat(),
                Y = CheckAttr(elem, "y").ParseFloat(),
                Z = CheckAttr(elem, "z").ParseFloat(),
            };
        }

        private static void ExecObject3DElement(FullModelData data, XElement element)
        {
            string mode = CheckAttr(element, "mode");
            string name = CheckAttr(element, "name");

            ulong hashname = Hash64.HashString(name);

            Object3D obj = FindObjectByHashname(data, hashname);

            switch (mode)
            {
                case "add":
                    {
                        if (obj != null)
                            throw new Exception(
                                $"Cannot create new Object3D named \"{name}\", as such "
                                + "an object already exists!");

                        obj = new Object3D(name, null) { Transform = Matrix3D.Identity };
                        data.AddSection(obj);
                        break;
                    }
                case "edit":
                    {
                        if (obj == null)
                            throw new Exception($"Cannot find Object3D named \"{name}\"");

                        break;
                    }
                default:
                    throw new Exception($"Invalid Object3D mode \"{mode}\"");
            }

            // Keep track of whether the object's parent was set
            // This is mandatory for new elements
            bool set_parent = false;

            // Look through the element's children and run them
            foreach (XElement operation in element.Elements())
            {
                switch (operation.Name.ToString())
                {
                    case "position":
                        {
                            Vector3D vec = CheckAttrVec(operation);

                            // TODO update the object's world_transform property
                            Matrix3D transform = obj.Transform;
                            transform.Translation = vec;
                            obj.Transform = transform;

                            break;
                        }
                    case "rotation":
                        {
                            Quaternion quat;

                            quat.X = CheckAttr(operation, "x").ParseFloat();
                            quat.Y = CheckAttr(operation, "y").ParseFloat();
                            quat.Z = CheckAttr(operation, "z").ParseFloat();
                            quat.W = CheckAttr(operation, "w").ParseFloat();
                            quat.Normalize();

                            // Unfortunately, there's no clean way to replace the matrix's rotation
                            // So we break down the matrix into it's components, then rebuild it with the new rotation
                            obj.Transform.Decompose(
                                out Vector3D scale,
                                out Quaternion _,
                                out Vector3D translation
                            );

                            // Nexus's matrix multiplications work backwards, which is why this looks like it's
                            // in the wrong order.
                            // If this was the wrong way around, the scaling would be fixed, rather than relative
                            // to the object.
                            Matrix3D mat = Matrix3D.CreateScale(scale) * Matrix3D.CreateFromQuaternion(quat);
                            mat.Translation = translation;

                            obj.Transform = mat;

                            break;
                        }
                    case "scale":
                        {
                            // WARNING:
                            // DieselX probably doesn't expect you to scale stuff, so be careful with this!
                            // If something blows up in your face when scaled, check that first.

                            Vector3D scale = CheckAttrVec(operation);

                            // Same story as rotation - split and rebuild the matrix.
                            obj.Transform.Decompose(
                                out Vector3D _,
                                out Quaternion quat,
                                out Vector3D translation
                            );

                            // Nexus's matrix multiplications work backwards, which is why this looks like it's
                            // in the wrong order.
                            // If this was the wrong way around, the scaling would be fixed, rather than relative
                            // to the object.
                            Matrix3D mat = Matrix3D.CreateScale(scale) * Matrix3D.CreateFromQuaternion(quat);
                            mat.Translation = translation;

                            obj.Transform = mat;

                            break;
                        }
                    case "parent":
                        {
                            XAttribute root = operation.Attribute("root");

                            Object3D parent;

                            if (root != null)
                            {
                                if (root.Value != "true")
                                    throw new Exception("Parent element has root attribute set " +
                                                        $"to \"{root.Value}\" - the root attribute " +
                                                        "must be set to \"true\", if it exists");

                                parent = null;

                                if (operation.Attribute("name") != null)
                                    throw new Exception("Parent element cannot have a \"name\" " +
                                                        "attribute if set to root");
                            }
                            else
                            {
                                string parent_name = CheckAttr(operation, "name");

                                parent = FindObjectByHashname(data, Hash64.HashString(parent_name));

                                if (parent == null)
                                    throw new Exception($"Could not find parent object named \"{parent_name}\"");
                            }

                            // Remove the object from it's previous parent, if it had one
                            obj.Parent?.children.Remove(obj);

                            // Set the object's parent attributes
                            obj.Parent = parent;

                            // And add it to the new parent's list of children
                            parent?.children.Add(obj);

                            set_parent = true;

                            break;
                        }
                    default:
                        throw new Exception($"Invalid Object3D child element \"{operation.Name}\"");
                }
            }

            if (mode == "add" && !set_parent)
                throw new Exception($"New object \"{name}\" did not set it's parent - all new elements must do so!");
        }

        private static void ExecImport(FullModelData data, XElement element, string directory)
        {
            string file = Path.Combine(directory, CheckAttr(element, "file"));
            string type = CheckAttr(element, "type");

            string create_objects_str = CheckAttr(element, "create_objects");
            bool create_objects;

            switch (create_objects_str)
            {
                case "true":
                    create_objects = true;
                    break;
                case "false":
                    create_objects = false;
                    break;
                default:
                    throw new Exception($"Invalid value '{create_objects_str}' for create_objects: "
                                        + "must either be true or false");
            }

            IOptionReceiver options;
            switch (type)
            {
#if !NO_FBX
                case "fbx":
                    options = new FilmboxImporter.FbxImportOptions();
                    break;
#endif
                default:
                    options = null;
                    break;
            }

            Dictionary<string, Object3D> object_rootpoints = new Dictionary<string, Object3D>();
            Object3D default_rootpoint = null;

            foreach (XElement child in element.Elements())
            {
                switch (child.Name.ToString())
                {
                    case "rootpoint":
                        {
                            string id_str = CheckAttr(child, "name");
                            Object3D obj = FindObjectByHashname(data, Hash64.HashString(id_str)) ??
                                           throw new Exception($"Cannot find rootpoint element {id_str}");

                            foreach (XElement elem in child.Elements())
                            {
                                switch (elem.Name.ToString())
                                {
                                    case "object":
                                        string obj_name = CheckAttr(elem, "name");
                                        if (object_rootpoints.ContainsKey(obj_name))
                                            throw new Exception($"Cannot redefine rootpoint for object {obj_name}");
                                        object_rootpoints[obj_name] = obj;
                                        break;
                                    case "default":
                                        if (default_rootpoint == null)
                                            default_rootpoint = obj;
                                        else
                                            throw new Exception($"Cannot redefine default rootpoint - '{id_str}'");
                                        break;
                                    default:
                                        throw new Exception($"Invalid rootpoint child element \"{elem.Name}\"");
                                }
                            }

                            break;
                        }
                    case "option":
                        {
                            if (options == null)
                                throw new Exception($"Options are not supported on {type} imports");

                            string name = CheckAttr(child, "name");
                            string value = child.Value.Trim();

                            try
                            {
                                options.AddOption(name, value);
                            }
                            catch (FormatException ex)
                            {
                                throw new Exception($"Error parsing value for option {name} value '{value}'", ex);
                            }

                            break;
                        }
                    default:
                        throw new Exception($"Invalid Import child element \"{child.Name}\"");
                }
            }

            Object3D ParentFinder(string name)
            {
                if (object_rootpoints.ContainsKey(name)) return object_rootpoints[name];

                return default_rootpoint ??
                       throw new Exception($"No default- nor object-rootpoint set for {name}");
            }

            switch (type)
            {
                case "obj":
                    bool result = NewObjImporter.ImportNewObj(data, file, create_objects,
                        objData => ParentFinder(objData.object_name));

                    if (!result)
                        throw new Exception($"Could not import OBJ file {file} - see console");
                    break;
#if !NO_FBX
                case "fbx":
                    if (create_objects)
                        throw new Exception("Creating objects is not yet supported for FBX");
                    FilmboxImporter.Import(data, file, create_objects, fn => null,
                        (FilmboxImporter.FbxImportOptions) options);
                    break;
#endif
                case "gltf":
                    GltfImporter.Import(data, file, create_objects, name => object_rootpoints.ContainsKey(name) ? object_rootpoints[name] : default_rootpoint);
                    break;
                default:
                    throw new NotImplementedException($"Cannot import file with type '{type}', "
                                                      + "only OBJ and FBX are currently supported");
            }
        }

        private static void ExecPatternUV(FullModelData data, XElement element, string directory)
        {
            string file = Path.Combine(directory, CheckAttr(element, "file"));
            bool result = NewObjImporter.ImportNewObjPatternUV(data, file);
            if (!result)
            {
                throw new Exception("There was an error importing Pattern UV OBJ - see console");
            }
        }
    }
}
