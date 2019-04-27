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

# Importing OBJs

In a modelscript, you can also import OBJ files:

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
        <rootpoint name="rp_testmodel">
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

# Misc info

The contents of the modelscript is run top-down - if you try to define your root object at the
end of the file, all the previous references to it will cause errors.

This also means that if you want to move or reparent objects in an OBJ file, you must do that
after the import tag, as the objects don't exist before that.
