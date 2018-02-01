using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PD2ModelParser
{
    class Tags
    {
        public static uint animation_data_tag = 0x5DC011B8; // Animation data
        public static uint author_tag = 0x7623C465; // Author tag
        public static uint material_group_tag = 0x29276B1D; // Material Group
        public static uint material_tag = 0x3C54609C; // Material
        public static uint object3D_tag = 0x0FFCD100; // Object3D
        public static uint model_data_tag = 0x62212D88; // Model data
        public static uint geometry_tag = 0x7AB072D3; // Geometry
        public static uint topology_tag = 0x4C507A13; // Topology
        public static uint passthroughGP_tag = 0xE3A3B1CA; // PassthroughGP
        public static uint topologyIP_tag = 0x03B634BD;  // TopologyIP
        public static uint quatLinearRotationController_tag = 0x648A206C; // QuatLinearRotationController
        public static uint quatBezRotationController_tag = 0x197345A5; // QuatBezRotationController
        public static uint skinbones_tag = 0x65CC1825; // SkinBones
        public static uint bones_tag = 0xEB43C77; // Bones
        public static uint light_tag = 0xFFA13B80; //Light
        public static uint lightSet_tag = 0x33552583; //LightSet
        public static uint linearVector3Controller_tag = 0x26A5128C; //LinearVector3Controller
        public static uint linearFloatController_tag = 0x76BF5B66; //LinearFloatController
        public static uint lookAtConstrRotationController = 0x679D695B; //LookAtConstrRotationController
        public static uint camera_tag = 0x46BF31A7; //Camera
    }
}
