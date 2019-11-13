# FBX Importer Documentation

Documentation for the FBX importer

TODO: write the introduction

# Errors

The FBX importer priorities correctness over functionality, with a fail-fast
approach: if something isn't quite right, it immediately fails the import
rather than trying to guess what you wanted to do.

This can save a **LOT** of time wondering why your model isn't working, but
also adds a lot of errors. Some (and eventually all) of these are documented
below.

To avoid breakage between versions, each error is assigned a code that will
remain static, even if the link changes.

## EFBX001

**Cannot load FBX file**

This means that the FBX SDK was unable to load the selected FBX file. This
may be caused by selecting an incorrect file name.

## EFBX002

**Cannot import FBX file**

This means the FBX file could be opened but not loaded into the scene. Check
that your model is not corrupt.

## EFBX003

**Mesh objects must currently end with the 'Object' suffix**

The nodes for each mesh must be named *object3d name*Object - that is, have an 'Object' suffix.

In Blender, a valid sample heirachy is as follows:

```
Hips_RigRoot
├ Hips_RigRoot (bones)
├ Pose
└ g_bodyObject <<< ***this is the node in question*** - it is called g_body in game
  ├ Mesh (might be called something else, shouldn't matter)
  ├ Modifiers
  └ Vertex Groups
```

## EFBX004

**Each rigged model must have only one root bone**

This should be impossible to get now. It means that your model has more than one root
bone, but new versions detect the root bone automatically and can only find one anyway.

## EFBX005

**Rigged model** *modelname* **has no bones**

Fairly self-explanatory.

This occurs when you import a model that has a skin deformer attached but doesn't have
any bones. If you don't want a rigged model then delete the deformer, if you want a
rigged model then you'll need some bones.

## EFBX006

**Only one skin per mesh is supported**

This occurs when you have more than one skin deformer on a given mesh - the model tool
only supports a single mesh deformer.

## EFBX007

**Could not get skin deformer ID=0**

This occurs when the first deformer on the mesh is not a skin deformer. If you have
any other deformers, either delete them or move them down the list.

## EFBX008

**Model** *model* **uses bone** *bone* **which is unavailable in this model**

This is a somewhat more complicated issue, but an easy trap to fall into.

Each PD2 '.model' file contains zero or more meshes, named 'models' confusingly
enough. In characters there are generally two main categories of these: heads and
bodies.

Some bones are only present on the body, and some bones are only present on the
head. One bone in particular (the 'head' bone) has already been used on several
body models casuing this error.

Ensure that the referenced model does not use the mentioned bone, either by
unpainting it (setting it's weights to zero for any bones using it) or
removing it from the model (only practical if you're adjusting the body but
not the head or vice-versa).

Again, if this refuses to go away, contact me.

## EFBX009

**Two clusters for the same bone and vertex are currently unsupported**

This could possibly be caused by some weirdness on the part of your modeling
software. If you can create this issue, I'd be very interested to hear about it.

## EFBX010

**Vertices cannot be affected by more than three bones**

As an engine limition, no vertex can ever be affected by more than three
bones. This means some vertex in your model is affected by more than three,
though currently the error doesn't give you anywhere to look (a major TODO since
this would be a very obnoxious error to fix).

You could maybe try (after making a backup of course) chopping up your
model until you find the problem vertex, or tell me to hurry up and add some
way to find the vertex.

## EFBX011

**Too many bones!**

This occurs if you have more than around 16.000 bones in a single model - you'll
hit all kinds of other issues long before then, good luck triggering this.

The solution is, of course, to not use thousands of bones.

## EFBX012

**Short normal!**

This occurs when the normals on one of your models are unset. This is often
triggered by the hitbox meshes - delete them in your modeling software (as
per the yet-to-be written introduction, this won't remove them in-game).

## EFBX013

**The model tool does not support more than one vertex colour layer**

This occurs when a given model has more than one vertex colour element, as
PD2 does not support multiple.

## EFBX014

**Triangles are the only supported type of polygon**

This occurs when the FBX contains a polygon with more than three points.

Solution: triangulate your meshes in your modeling tool first. In Blender, this
can (as of 2.8) be done by pressing CTRL-T in edit mode.

## EFBX015

**Unknown UV** *UV channel name*

One of your UV channels has a name that the model tool can't interpret. Currently
the following UV names are supported:

* PrimaryUV: alias for UV0, used for models exported by previous versions of
the model tool.
* UV0, UV1, UV2 etc through UV7 - each of the eight UV layers supported by the
model format.

## EFBX016

**Unsupported mapping mode** *modename*

One of the channels in your model uses an unsupported mapping mode. Please contact
me (ZNix) to get this fixed.

