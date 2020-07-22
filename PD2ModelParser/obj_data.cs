using System;
using System.Collections.Generic;

using Nexus;

using PD2ModelParser.Sections;

namespace PD2ModelParser
{
    class obj_data
    {
        public List<Vector3D> verts { get; set; }

        public List<Vector2D> uv { get; set; }

        public List<Vector3D> normals { get; set; }

        public string object_name { get; set; }

        public List<Face> faces { get; set; }

        public string material_name { get; set; }

        public Dictionary<String, List<Int32>> shading_groups { get; set; }
        
        public obj_data()
        {
            this.verts = new List<Vector3D>();
            this.uv = new List<Vector2D>();
            this.normals = new List<Vector3D>();
            this.object_name = "";
            this.faces = new List<Face>();
            this.material_name = "";
            this.shading_groups = new Dictionary<string, List<int>>();
        }
    }
}
