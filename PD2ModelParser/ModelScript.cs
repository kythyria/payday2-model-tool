using System;
using System.Xml.Linq;
using Nexus;
using PD2ModelParser.Sections;

namespace PD2ModelParser
{
    public static class ModelScript
    {
        public static void Execute(FullModelData data, string filename)
        {
            XElement root = XElement.Parse(System.IO.File.ReadAllText(filename));

            if (root.Name != "modelscript")
                throw new Exception("Script root node is not named \"modelscript\"");

            foreach (XElement element in root.Elements())
            {
                switch (element.Name.ToString())
                {
                    case "object3d":
                        ExecObject3DElement(data, element);
                        break;
                    default:
                        throw new Exception($"Unknown node name \"{element.Name}\"");
                }
            }
        }

        private static Object3D FindObjectByHashname(FullModelData data, ulong hashname)
        {
            foreach (object section in data.parsed_sections.Values)
            {
                // Only look for Object3Ds
                if (!(section is Object3D obj)) continue;

                // Check if this one has the right name
                if (obj.hashname.Hash != hashname) continue;

                return obj;
            }

            return null;
        }

        private static string CheckAttr(XElement elem, string attr)
        {
            return elem.Attribute(attr)?.Value ??
                   throw new Exception($"Missing \"{attr}\" attribute for {elem.Name} element");
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

                    obj = new Object3D(name, 0) {rotation = Matrix3D.Identity};
                    data.AddSection(obj, Tags.object3D_tag);
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

            // Look through the element's children and run them
            foreach (XElement operation in element.Elements())
            {
                switch (operation.Name.ToString())
                {
                    case "position":
                    {
                        Vector3D vec;

                        vec.X = float.Parse(CheckAttr(operation, "x"));
                        vec.Y = float.Parse(CheckAttr(operation, "y"));
                        vec.Z = float.Parse(CheckAttr(operation, "z"));

                        // TODO update the object's world_transform property
                        obj.rotation.Translation = vec;

                        break;
                    }
                    case "rotation":
                    {
                        Quaternion quat;

                        quat.X = float.Parse(CheckAttr(operation, "x"));
                        quat.Y = float.Parse(CheckAttr(operation, "y"));
                        quat.Z = float.Parse(CheckAttr(operation, "z"));
                        quat.W = float.Parse(CheckAttr(operation, "w"));
                        quat.Normalize();

                        // Unfortunately, there's no clean way to replace the matrix's rotation
                        // So we break down the matrix into it's components, then rebuild it with the new rotation
                        obj.rotation.Decompose(
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

                        // TODO update the object's world_transform property
                        obj.rotation = mat;

                        break;
                    }
                    case "parent":
                    {
                        XAttribute root = operation.Attribute("root");

                        Object3D parent;

                        if (root != null)
                        {
                            if (root.Value != "true")
                                throw new Exception("parent element has root attribute set " +
                                                    $"to \"{root.Value}\" - the root attribute " +
                                                    "must be set to \"true\", if it exists");

                            parent = null;

                            if (operation.Attribute("name") != null)
                                throw new Exception("parent element cannot have a \"name\" " +
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
                        obj.parent?.children.Remove(obj);

                        // Set the object's parent attributes
                        obj.parent = parent;
                        obj.parentID = parent?.id ?? 0;

                        // And add it to the new parent's list of children
                        parent?.children.Add(obj);

                        break;
                    }
                    default:
                        throw new Exception($"Invalid Object3D child element \"{operation.Name}\"");
                }
            }
        }
    }
}
