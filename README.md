# PAYDAY 2 Model Tool - Calcium Edition

This is a copy of IAmNotASpy and PoueT's model tool, with a bunch of new features:

* Greatly improved UI, with different functions cleanly separated
* The ability to use an XML-based script to modify the object/bone structure of models, and create entirely
new models without deriving them from an existing model, and set rootpoints for different objects
* Experimental Collada (DAE) and Filmbox (FBX) export support, for bones and (in the latter case) full
rigging, vertex colours, and eight UV channel support.
* glTF export support, with vertex colours, all eight UV channels, and material slots (multiUV).
  There's also preliminary support for exporting rigged models.
* glTF import support, also with vertex colours, all eight UV channels, and material slots.
* An importer for Filmbox (Coming Soon™)
* And a bunch of miscellaneous features and bugfixes

# glTF export/import

Both the importer and exporter treat the material name `Material: Default Material` specially: it becomes no
material on export, and a lack of material on import is replaced with that. Otherwise, the exporter creates
a dummy material for each material name in the Diesel model. The importer doesn't care about the precise
definition of materials, only their names.

Exporting preserves the object hierarchy, and includes partial rigging support: bones and weights should be
exported, but validating glTF parsers may complain about non-normalised weights, and whether or not meshes
stay attached to their skeletons is a bit iffy.

Importing is designed so you don't need a modelscript so much:
* If an object has the same name as one already in the .model, the latter's rotation and parentage are overwritten.
* Models with the same name as an existing object delete that object and adopt its child objects.
  (this may break animations).
* Models with the same name as an existing *model* have their model data replaced.
* Objects with a parent in the glTF file always keep that parent on import.
* Objects with no parent in the glTF file are parented according to the modelscript, except that not specifying a
  rootpoint at all isn't an error.

Because GLTF dictates a 1m scale, and Payday 2 uses a 1cm scale, the exporter accounts for this (this does have
the downside that if you're importing into Blender bones and empties are drawn much too big).

# Feature Matrix

| Format | Import | Export |
|--------|--------|--------|
| OBJ    | ✓      | ✓      |
| DAE    |        | ✓      | 
| FBX    | ✓      | ✓      |
| GLTF   | ✓      | ✓      |

| Data             | FBX Out | FBX In | DAE | GLTF In | GLTF Out |
|------------------|---------|--------|-----|---------|----------|
| Triangles        | ✓       | ✓      | ✓   | ✓       | ✓        |
| UV channels      | ✓       | ✓      | One | ✓       | ✓        |
| Vertex colours   | ✓       | ?     | ✗   | ✓       | ✓        |
| Vertex weights   | ✓       | ✓     | ✗   | ✓       | ✓        |
| Material slots   | ✓       | ✗     | ✗   | ✓       | ✓        |
| Object hierarchy | ✓       | ✓     | ✓   | ✓       | ✓        |
| Bones            | ✓       | ✓     | As objects | As objects | Partial |
| Skinning         | ✓       | ✓     | ✗   | Ignored | Partial    |

Partial bone/skinning support refers to the result not being read sensibly in all implementations.

The GLTF importer completely ignores skinning data, so the results will be odd as well as effectively unrigged.

# Hashlists
Diesel very rarely stores actual names of things if it can store a hash of the name instead, so a list of
names is needed in order to present something readable names instead of just large numbers. On export anything
not in the list will be written as a number, while the GLTF importer will assume any name that's a valid
`unsigned long` is the result of that process.

A copy of [Luffyyy's version of the hashlist](https://github.com/Luffyyy/PAYDAY-2-Hashlist) is included; the
tool looks for files called `hashlist`, `hashes`, with or without a numeric suffix like `-3`, with or without
a `.txt` extension, in the current directory and next to the executable. Any it finds are interpreted as lists
of unhashed names, one per line. If you change hashlists you will need to restart the tool in order to pick
up the changes.

# Licence:

This program is Free Software under the terms of the GNU General Public Licence, version 3. A copy of
this licence is distributed with the program's source files.

As an exception to the GPLv3, you may use Autodesk's FBX SDK as part of this program, and you are not
required to provide that under the GPL (since it is impossible to do so, and would prevent binary redistribution).

If you are using this exception, you must ship the FBX SDK dynamically linked (not statically linked), and
you must provide the entire rest of the program under the GPLv3 with this exception.

If you wish, you may also delete this exception from modified versions of the software and use the plain
GPLv3.

# Error Codes

Error codes, descriptions and solutions in the format of 'EFBX123' can be found on
the [FBX Importer page](Docs/FBXImporter.md) page.
