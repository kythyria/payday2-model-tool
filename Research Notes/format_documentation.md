Model format notes.
0x0 DW If -1 then additional header, else, number of sections.
Additional headers:
0x4 DW appears to be total file size.
0x8 DW number of sections

Sections:
uint32 section_type // Uses one of the below tags. Tags are assigned to serializable objects within the Diesel engine.
uint32 section_id // Appears to be a random, but unique value assigned to the section. Unknown if these have any requirements or meanings.
uint32 size
char[size] data

Tag:
TopologyIP:
uint32 topology_section_id

PassthroughGP:
uint32 geometry_section_id
uint32 topology_section_id

Animation Data:
uint64 unique_id
uint32 unknown
uint32 unknown
uint32 count
item[count]:
	uint32 unknown

Topology:
uint32 unknown
int32 indice_count
short[count1] indices
int32 count2
char[count2]
uint64 unknown

Geometry:
int32 item_count
int32 type_count
types[type_count]:
	uint32 type_size
	uint32 type
char[count1*calculated_size] vertex_buffer
uint64 unknown
calculated_size = sum(size_index[type])
size_index = {0, 4, 8, 12, 16, 4, 4, 8, 12}
types: 1 = Vertex, 2 = Normal, 7 = UV, 8 = Unknown, 15 = Unknown, 20 = Tangent/Binormal, 21 = Tangent/Binormal

cur_data_offset = 0
for type in types:
	type_size = size_index[type.type_size]
	data_for_type = data[cur_data_offset:cur_data_offset+type_size*item_count]
	for x in xrange(item_count):
		item = data_for_type[x*type_size:x*type_size+type_size]


Material:
uint64 material_id
uint32 zero //48 bytes of skipped data when reading.
uint32 zero
char[16] zero
char[16] zero
uint32 zero
uint32 zero //end 48 bytes of skipped data.
uint32 count
item[count]:
	uint32 unknown
	uint32 unknown

Material Group:
uint32 material_count
uint32[count] material_section_ids

Author:
uint64 unknown
cstring email
cstring source_path
uint32 unknown

Object3D:
uint64 unique_id
uint32 count
item[count]:
	uint32 unknown
	uint32 unknown
	uint32 unknown
float[4][4] rotation_matrix // Custom orientation matrix for submeshes
float[3] position // Used to position submeshes within object space.
unit32 parent/child_object_section_id

Model Data:
Object3D 3d_object
uint32 version
if version == 6:
	float[3] bounds_min
	float[3] bounds_max
	uint32 unknown
	uint32 unknown
else:
	uint32 passthroughgp_section_id
	uint32 topologyip_section_id
	uint32 count
	item[count]:
		uint32 unknown
		uint32 unknown
		uint32 unknown
		uint32 unknown
	uint32 unknown
	uint32 material_group_section_id
	uint32 unknown
	float[3] bounds_min
	float[3] bounds_max
	uint32 unknown
	uint32 unknown
	uint32 unknown

Light:
Object3D object3d
byte unknown
int32 unknown
float[4] unknown //color?
float unknown //intensity?
float unknown //falloff
float unknown //cone inner?
float unknown //cone outer?
float unknown

LinearFloatController:
uint64 unique_id //hash of animation name?
uint32 unknown //flags? Appears to use bytes 1 and 2 as flags.
uint32 unknown
uint32 unknown //Appears linked to above value
uint32 keyframe_count
keyframe[keyframe_count]:
    float unknown //Timestamp?
    float value

LookAtConstrRotationController:
uint64 unique_id //hash of animation name?
uint32 unknown
uint32 unknown //Reference to another section
uint32 unknown //Reference to another section
uint32 unknown //Reference to another section

LinearVector3Controller:
uint64 unique_id //hash of animation name?
uint32 unknown //flags? Appears to use bytes 1 and 2 as flags.
uint32 unknown
uint32 unknown //Appears linked to above value
uint32 keyframe_count
keyframe[keyframe_count]:
	float unknown //Timestamp?
	float[3] position

QuatLinearRotationController:
uint64 unique_id //hash of animation name?
uint32 unknown //flags? Appears to use bytes 1 and 2 as flags.
uint32 unknown
uint32 unknown //Appears linked to above value
uint32 keyframe_count
keyframe[keyframe_count]:
	float unknown //Timestamp?
	float[4] rotation //Quaternion
	
QuatBezRotationcontroller:
uint64 unique_id //hash of animation name?
uint32 unknown //flags? Appears to use bytes 1 and 2 as flags.
uint32 unknown
uint32 unknown //Appears linked to above value
uint32 keyframe_count
keyframe[keyframe_count]:
	float unknown //Timestamp?
	float[4] unknown
	float[4] unknown
	float[4] unknown
	
LightSet:
uint64 unique_id //hash of light set name?
uint32 light_count
light[light_count]
	uint32 light_section_id
	
Bones:
uint32 count
bone[count]:
	uint32 vertex_count?
	bone_vertex[count]:
		uint32 vertex_id?


Skin Bones:
Bones bones
uint32 object3d_section_id
uint32 count
objects[count]:
	uint32 object3d_section_id
rotations[count]:
	float[4][4] orientation_matrix
float[4][4] unknown_matrix

Camera:
Object3D object
float unknown
float unknown
float unknown
float unknown
float unknown
float unknown

Tags:
0x5DC011B8 == Load routine at 0x0073E930 //Animation data
0x7623C465 == Load routine at 0x006FA100 //Author tag
0x29276B1D == Load routine at 0x0073E340 //Material Group
0x3C54609C == Load routine at 0x0073E270 //Material
0x0FFCD100 == Load routine at 0x00742F80 //Object3D
0x62212D88 == Load routine at 0x00749750 //Model data
0x7AB072D3 == Load routine at 0x0071FEA0 //Geometry
0x4C507A13 == Load routine at 0x0071FFF0 //Topology
0xE3A3B1CA == Load routine at 0x0073DD10 //PassthroughGP
0x03B634BD == Load routine at 0x0073DDC0 //TopologyIP
0x648A206C == Load routine at 0x0071F680 //QuatLinearRotationController
0x197345A5 == Load routine at 0x0071F6B0 //QuatBezRotationController
0x65CC1825 == Load routine at 0x007440D0 //SkinBones
0x2EB43C77 == Load routine at 0x00743FD0 //Bones
0xFFA13B80 == Load routine at 0x00745A40 //Light
0x33552583 == Load routine at 0x0073DDF0 //LightSet
0x26A5128C == Load routine at 0x0071F620 //LinearVector3Controller
0x76BF5B66 == Load routine at 0x0071F570 //LinearFloatController
0x679D695B == Load routine at 0x0073DA00 //LookAtConstrRotationController
0x46BF31A7 == Load routine at 0x00745970 //Camera

