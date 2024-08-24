using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace UnityUtilities.Terrain
{
    public class Mesh
    {
        public readonly List<Vector3> vs = new();
        public readonly List<Vector2> uvs = new();

        // Triangles are organized per submesh.
        public readonly Dictionary<int, List<int>> submeshes = new();

        public readonly List<Material> materials = new();

        public void AddSquare(Vector3 v0, Vector3 v1, Vector3 v2, Vector3 v3, int submesh)
        {
            int v = this.vs.Count;

            if (!this.submeshes.TryGetValue(submesh, out List<int> ts))
            {
                ts = new List<int>();
                this.submeshes.Add(submesh, ts);
            }

            this.vs.Add(v0);
            this.vs.Add(v1);
            this.vs.Add(v2);
            this.vs.Add(v3);

            ts.Add(v);
            ts.Add(v + 1);
            ts.Add(v + 2);

            ts.Add(v);
            ts.Add(v + 2);
            ts.Add(v + 3);
        }

        public UnityEngine.Mesh ToUnityMesh(string name)
        {
            int submeshCount = this.submeshes.Keys.Max() + 1;

            var mesh = new UnityEngine.Mesh()
            {
                name = name,
                vertices = this.vs.ToArray(),
                subMeshCount = submeshCount,
                uv = this.uvs.ToArray(),
            };

            for (int i = 0; i < submeshCount; i++)
            {
                if (this.submeshes.TryGetValue((byte)i, out List<int> triangles))
                {
                    mesh.SetTriangles(triangles, i);
                }
            }

            return mesh;
        }
    }
}
