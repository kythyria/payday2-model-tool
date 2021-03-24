# Modelscript

Modelscript is an advanced customisation system for PAYDAY 2 models. It allows
you to create new Object3Ds, which allows you to add custom interaction points
to your models, as well as play effects in certain positions from your model script.

When writing modelscripts, I would *highly* recommend you use the **Objects** tab
in the model tool. Drag in your modelscript (and your base .model file if you have
one), and it will show you the heirachy of objects in the model that would result
if you imported the script into that model.

If you don't set a script and only set a model, it you can find the names of all
the objects in a model.

**Note:**

When the modelscript and/or guide refers to 'objects', these are **not** the same
as an object in blender, and will not visibly appear in-game. Those are called Models,
and are similar to objects in many ways - both objects and models have a position,
rotation and scale, and both can be parented to objects (however nothing can be parented
to a model).

# Introduction to writing modelscripts

Model scripts are plain text files with an extension of `.mscript`

At a very minimum, they contain an XML declaration followed by a `<modelscript>`
tag:


```xml
<?xml version="1.0" ?>
<modelscript>
    <!-- your stuff goes here -->
</modelscript>
```

That won't do anything - it would be the same as not setting a modelscript at
all. We can modify or add objects with an `<object3d>` tag, so let's try that:

```xml
<?xml version="1.0" ?>
<modelscript>
    <object3d name="rp_testmodel" mode="add">
        <parent root="true" />
    </object3d>
</modelscript>
```

This creates a new object called `rp_testmodel`. The `name` attribute sets the
new object's name - in this case I chose `rp_testmodel`, with `rp_` indicating
that this is a rootpoint - we want to attach models to it. The `mode` attribute
says we want to create a new object - we could use `mode="edit"` to edit an
existing object.

The `<parent root="true" />` bit defines what this object should be parented
to. Objects are arranged in a heirachy, each node either being attached onto a
parent object, or they are a root object and have no parent.

While I am not aware of any hard limitation that prevents you from doing otherwise,
I strongly recommend you only have one root object in your model - it's what
PAYDAY's default models do, and it's guaranteed to always work properly.

Here's what the heirachy of this model now looks like:

```
<root>
* rp_testmodel
```

(you can see this by importing the model into the objects tab by itself, or pressing
refresh if you have modified the file)

Next let's say we want to add a new object as a child of `rp_testmodel` - let's
say that the testmodel is a machine that will explode when the player interacts
with it. To trigger the explosion from a sequence manager, we have to have an
object to mark where the explosion should occur. Let's add that and call
it `explosion_point`:

```xml
<?xml version="1.0" ?>
<modelscript>
    <object3d name="rp_testmodel" mode="add">
        <parent root="true" />
    </object3d>
    <object3d name="explosion_point" mode="add">
        <parent name="rp_testmodel" />
    </object3d>
</modelscript>
```

The new `explosion_point` object is similar to the root object, only we set
it's parent as `rp_testmodel`.

If you open this script in the objects tab, it will now look like this:

```
<root>
* rp_testmodel
  * explosion_point
```

But this explosion point will be at 0,0,0 - that's where objects are by default. While
this is fine for the root object (which should usually be at 0,0,0), we want to move the
explosion point around. We can do this with the `<position />` element:

```xml
<?xml version="1.0" ?>
<modelscript>
    <object3d name="rp_testmodel" mode="add">
        <parent root="true" />
    </object3d>
    <object3d name="explosion_point" mode="add">
        <parent name="rp_testmodel" />
        <position x="250" y="500" z="100" />
    </object3d>
</modelscript>
```

This will position the object at 250,500,100 as you might guess.

There's also a `<rotation />` element which can be used to rotate an element. It works
using quaternion parameters (XYZW), and until I add an angle-axis mode, ask me how to
use it if you don't know how quaternions work.

As an example though, this would rotate an object around the Z axis:

```xml
<rotation w="0.707106769" x="0" y="0" z="0.707106769" />
```

# Importing models

In a modelscript, you can also import models and object3ds from external files. The `type`
must be one of `obj`, `fbx`, or `gltf`, if absent it will be guessed from the extension.

```xml
<?xml version="1.0" ?>
<modelscript>
    <object3d name="rp_testmodel" mode="add">
        <parent root="true" />
    </object3d>
    <import file="testmodel.obj" type="obj" create_objects="true">
        <rootpoint name="rp_testmodel">
            <default/>
        </rootpoint>
    </import>
</modelscript>
```

This will import all the models from `testmodel.obj` into the model file, attached to `rp_testmodel`.

Here's what the heirachy might look like at this point, depending on your model file:

```
<root>
* rp_testmodel
  * co_culling (model)
  * myobj_1 (model)
  * myobj_2 (model)
  * myobj_3 (model)
  * myobj_4 (model)
```

You can also attach different models to different objects:

```xml
<?xml version="1.0" ?>
<modelscript>
    <object3d name="rp_testmodel" mode="add">
        <parent root="true" />
    </object3d>
    <object3d name="my_other_object" mode="add">
        <parent name="rp_testmodel" />
    </object3d>
    <import file="testmodel.obj" type="obj" create_objects="true">
        <rootpoint name="rp_testmodel">
            <default/> <!-- unless otherwise specified, models will be attached to this -->
            <object name="co_culling" /> <!-- also attach the culling box here, this could be
                omitted since rp_testmodel is the default anyway, however it makes it clear
                where this needs to be -->
        </rootpoint>
        <rootpoint name="my_other_object">
            <object name="myobj_1" /> <!-- attach myobj_1, myobj_2, and myobj_3 to my_other_object -->
            <object name="myobj_2" />
            <object name="myobj_3" />
        </rootpoint>
    </import>
</modelscript>
```

And this will result in the following heirachy:

```
<root>
* rp_testmodel
  * my_other_object
    * myobj_1 (model)
    * myobj_2 (model)
    * myobj_3 (model)
  * co_culling (model)
  * myobj_4 (model)
```

You can also omit the `<default />` node if every model has it's rootpoint set. While this would be
rather tedious for a file with almost all it's models attached to a single object, it would be strongly
advisable on a model where models are split between many different objects - this is because if you
forgot to set a model's rootpoint, rather than setting it to the default one (which you probably don't
want in this case), it will throw an error to tell you to set it's parent.

# PatternUVs
This is the same functionality as the "Pattern UV" box in the GUI.

If you have a .obj file whose vertices precisely match the order and number of the vertices in the model
up to this point, you can set the second UV attribute of the model to the UV attribute of the .obj file.

```xml
<patternuv file="uvs.obj"/>
```

# Misc info

The contents of the modelscript is run top-down - if you try to define your root object at the
end of the file, all the previous references to it will cause errors.

This also means that if you want to move or reparent objects in an OBJ file, you must do that
after the import tag, as the objects don't exist before that.

# Element reference
Most of these exist to reflect command-line arguments or the GUI, but here they are.

## `<createnew create="bool"/>`
Control the default setting of `import/@create_objects`. Has one attribute `create` which must
be either `true` or `false`.

## `<rootpoint/>`
Set or clear the default rootpoint. `<rootpoint name="foo"/>` is equivalent to putting
```xml
<rootpoint name="foo">
    <default/>
</rootpoint>
```
in subsequent `<import>` elements. Omitting `name` cancels this behaviour.

## `<new/>`
Creates a blank model. 

## `<load file="whatever.model/>`
Load a Diesel model file, replacing what was already loaded.

## `<save file="whatever.model"/>`
Write the current model to a file.

## `<import>`
Import a model. `file` specifies the name, optionally `type` overrides the type (it will be
guessed from the filename otherwise). Can contain `<rootpoint>` and `<option/>`. `create_objects`
if set to false causes the import to fail if a new Object3D/Model/etc has to be created.

The latter gives the `name` of an option, and the value as the element content. The meaning of
these options is format-specific.

`<rootpoint name="str"/>` gives the name of an object that must exist in the model being imported
into, and its children indicate what should be parented to that object: `<object name="foo"/>`
parents the newly-imported object `foo`, and `<default/>` sucks up everything that isn't otherise
specified.

## `<object3d name="name" mode="add|edit">`
Create an Object3D (GLTF calls this a Node, Blender calls it an Empty), edit one, oredit a subclass
thereof (meshes, lamps, collision shapes). `name` specifies what to create/modify. Modifications are
given as one of a few elements:
```xml
<position x="0" y="0" z="0" />
<rotation x="0" y="0" z="0" w="1" />
<scale x="0" y="0" z="0" />
<parent root="true" />
<parent name="whatever" />
```
Which respectively set the position, rotation, or scale, clear the object's parent entirely, or set it
to the object named "whatever".

## `<runscript file="path"/>`
Runs another mscript file with the same state. Entirely the same: `<createnew/>` and such will carry over
between them.

## `<dumpanims file="path">`
Create a modelscript file containing `<animate/>` commands that will restore all
of the animations in the current model file.

## `<animate object="obj_name">`
Replace the `Animations` property of `obj_name`. This lets you create animations
that live in the .model, as is used for doors and such. Each child element becomes
a controller, `<null/>` represents a null in the list of controllers.

All the others understand a base-16 `flags` attribute, which is always 0 or 2 in the
vanilla files. The contents of the element is a whitespace/newline separated list of
floats. This list is split into groups to make keyframes as follows:

| Element        | Items               |
|----------------|---------------------|
| `<float>`      | `timestamp value`   |
| `<vector3>`    | `timestamp X Y Z`   |
| `<quaternion>` | `timestamp X Y Z W` |

The combinations observed in vanilla files include the following

| Object type    | Types                             | Meanings                       |
|----------------|-----------------------------------|--------------------------------|
| Light          | `float      null    null null`    | `intensity -      - -`         |
| Light          | `vector3    null    null`         | `colour    -      - -`         |
| Light          | `float      vector3 null vector3` | `intensity colour - position`  |
| Object3d/Model | `quaternion null    null`         | `rotation  -      -`           |
| Object3d/Model | `vector3`                         | Probably position              |
| Object3d/Model | `quaternion vector3`              | Probably rotation and position |

## `<duplicate source="obj_name" destination="obj_name" instance="bool?" newmaterials="mat1,mat2,mat3...">
Make a copy of `source` with the name `destination`. If `instance` is true, reuse the vertex and index
buffers, otherwise copy them.

If `newmaterials` is a comma-separated list of material names, set the list of materials to this, else
reuse the source's material list. It is unknown what happens if the wrong number of materials is given.
Note that the parser will trim leading and trailing whitespace from each name, so don't use names where
that's a problem.

## `<merge property-merge="..." model-merge="..." vertex-attributes="...">`
**NOT YET IMPLEMENTED. This is just a spec.**

Combine another model with the current one. Must have one child element, a `<modelscript>`
which obtains the model to be merged (eg, by `<import>`ing it).

The `property-merge` attribute specifies a lot of what's carried over, as a comma-separated
list of:
| Flag         | Effect                                             |
|--------------|----------------------------------------------------|
| `newobjects` | Objects in the incoming model which have no counterpart in the current model are copied wholesale. |
| `parents`    | Objects which have parents in the incoming model are reparented to the corresponding (by name) object in the current model |
| `position`   | Copy object positions from the incoming model.     |
| `rotation`   | Copy object rotations from the incoming model.     |
| `scale`      | Copy object scale factors from the incoming model. |
| `transform`  | Equivalent to `position,rotation,scale`.           |

The `model-merge` attribute says what to do with mesh data:

| Value        | Effect                                                     |
|--------------|------------------------------------------------------------|
| `none`       | Don't do anything.                                         |
| `recreate`   | Delete Geometry and Topology sections and create new ones. |
| `vertexedit` | Copy data over existing vertices.                          |
| `overwrite`  | Wipe and recreate vertices, triangles, and material slots, without deleting any sections. |

The `vertex-attributes` attribute specifies which data is copied across (comma-separated list):
| Flag           | Effect                                                                   |
|----------------|--------------------------------------------------------------------------|
| `none`         | Nothing                                                                  |
| `positions`    | Vertex positions                                                         |
| `normals`      | Normals, binormals, and tangents (if present)                            |
| `colors`       | Vertex colours                                                           |
| `colours`      | Vertex colors                                                            |
| `weights`      | Joint IDs and weights                                                    |
| `uv0` to `uv7` | UV layers 0 through 7                                                    |
| `uvs`          | All the UV layers                                                        |
| `vertices`     | Everything                                                               |

If you want to copy a UV layer from one to another, `remap-uvs` takes a comma-separated list of
integers, indicating which UV layer in the incoming model corresponds to which in the current
model, for instance `remap-uvs="0,0" vertex-attributes="uv1"` copies UV0 in the incoming model
to UV1 in the current model.