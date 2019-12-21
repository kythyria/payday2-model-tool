# PAYDAY 2 Model Tool - Calcium Edition

This is a copy of IAmNotASpy and PoueT's model tool, with a bunch of new features:

* Greatly improved UI, with different functions cleanly separated
* The ability to use an XML-based script to modify the object/bone structure of models, and create entirely
new models without deriving them from an existing model, and set rootpoints for different objects
* Experimental Collada (DAE) and Filmbox (FBX) export support, for bones and (in the latter case) full
rigging, vertex colours, and eight UV channel support.
* Experimental glTF export support, with vertex colours, all eight UV channels, and material slots (multiUV).
* Very experimental glTF import support, also with vertex colours, all eight UV channels, and material slots.
* An importer for Filmbox (Coming Soon™)
* And a bunch of miscellaneous features and bugfixes

# glTF export/import

Both the importer and exporter treat the material name `Material: Default Material` specially: it becomes no
material on export, and a lack of material on import is replaced with that. Otherwise, the exporter creates
a dummy material for each material name in the Diesel model. The importer doesn't care about the precise
definition of materials, only their names.

Exporting preserves the object hierarchy, and converts bones into objects (more precisely, doesn't filter them
out; they're already like that in both Diesel models and glTF).

Importing is designed so you don't need a modelscript so much:
* If an object has the same name as one already in the .model, the latter's rotation and parentage are overwritten.
* Models with the same name as an existing object delete that object and adopt its child objects.
  (this may break animations).
* Models with the same name as an existing *model* have their model data replaced.
* Objects with a parent in the glTF file always keep that parent on import.
* Objects with no parent in the glTF file are parented according to the modelscript, *except* that if you don't
  specify a parent it'll become a root object.

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
