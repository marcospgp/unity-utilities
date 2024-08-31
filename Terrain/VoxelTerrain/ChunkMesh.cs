using System.Collections.Generic;
using UnityEngine;

namespace UnityUtilities.Terrain
{
    public class ChunkMesh
    {
        private readonly List<Vector3> vs = new();
        private readonly List<Vector2> uvs = new();

        // Each submesh is a list of triangles.
        private readonly Dictionary<Material, List<int>> submeshes = new();

        public ChunkMesh(
            Chunk chunk,
            float blockSize,
            Dictionary<Block, BlockMaterials> blockMaterials
        )
        {
            foreach ((int x, int y, int z, Block block) in chunk.EnumerateWithoutBorder())
            {
                if (block == Block.Air)
                {
                    continue;
                }

                this.AddBlock((x, y, z), chunk, blockSize, blockMaterials[block]);
            }
        }

        public GameObject ToGameObject(
            string name,
            (int x, int z) chunkIndex,
            int chunkWidthInBlocks,
            float blockSize,
            int groundLayer
        )
        {
            var obj = new GameObject { name = name, layer = groundLayer };

            obj.transform.position = new Vector3(
                chunkIndex.x * chunkWidthInBlocks * blockSize,
                0,
                chunkIndex.z * chunkWidthInBlocks * blockSize
            );

            var mesh = new Mesh()
            {
                name = name,
                vertices = this.vs.ToArray(),
                subMeshCount = this.submeshes.Count,
                uv = this.uvs.ToArray(),
            };

            MeshFilter meshFilter = obj.AddComponent<MeshFilter>();
            meshFilter.sharedMesh = mesh;

            MeshRenderer meshRenderer = obj.AddComponent<MeshRenderer>();

            var materials = new Material[this.submeshes.Count];
            int i = 0;

            foreach ((Material key, List<int> submesh) in this.submeshes)
            {
                materials[i] = key;
                mesh.SetTriangles(submesh.ToArray(), i);
                i += 1;
            }

            mesh.RecalculateNormals();

            // Do not optimize as meshes are created at run time.
            // unityMesh.Optimize();

            meshRenderer.sharedMaterials = materials;

            _ = obj.AddComponent<MeshCollider>();

            return obj;
        }

        private static (Vector3, Vector3, Vector3, Vector3) FaceToQuad(Face face)
        {
            if (face == Face.XPlus)
            {
                return (
                    new Vector3(1, 0, 0),
                    new Vector3(1, 1, 0),
                    new Vector3(1, 1, 1),
                    new Vector3(1, 0, 1)
                );
            }

            if (face == Face.XMinus)
            {
                return (
                    new Vector3(0, 0, 1),
                    new Vector3(0, 1, 1),
                    new Vector3(0, 1, 0),
                    new Vector3(0, 0, 0)
                );
            }

            if (face == Face.YPlus)
            {
                return (
                    new Vector3(0, 1, 0),
                    new Vector3(0, 1, 1),
                    new Vector3(1, 1, 1),
                    new Vector3(1, 1, 0)
                );
            }

            if (face == Face.YMinus)
            {
                return (
                    new Vector3(0, 0, 1),
                    new Vector3(0, 0, 0),
                    new Vector3(1, 0, 0),
                    new Vector3(1, 0, 1)
                );
            }

            if (face == Face.ZPlus)
            {
                return (
                    new Vector3(1, 0, 1),
                    new Vector3(1, 1, 1),
                    new Vector3(0, 1, 1),
                    new Vector3(0, 0, 1)
                );
            }

            if (face == Face.ZMinus)
            {
                return (
                    new Vector3(0, 0, 0),
                    new Vector3(0, 1, 0),
                    new Vector3(1, 1, 0),
                    new Vector3(1, 0, 0)
                );
            }

            throw new System.Exception("Unreachable code.");
        }

        private void AddBlock(
            (int x, int y, int z) pos,
            Chunk chunk,
            float blockSize,
            BlockMaterials blockMaterials
        )
        {
            int x = pos.x;
            int y = pos.y;
            int z = pos.z;

            if (chunk[x + 1, y, z] == Block.Air)
            {
                this.AddFace(Face.XPlus, pos, blockSize, blockMaterials);
            }

            if (chunk[x - 1, y, z] == Block.Air)
            {
                this.AddFace(Face.XMinus, pos, blockSize, blockMaterials);
            }

            if (chunk[x, y + 1, z] == Block.Air)
            {
                this.AddFace(Face.YPlus, pos, blockSize, blockMaterials);
            }

            if (y > 0 && chunk[x, y - 1, z] == Block.Air)
            {
                this.AddFace(Face.YMinus, pos, blockSize, blockMaterials);
            }

            if (chunk[x, y, z + 1] == Block.Air)
            {
                this.AddFace(Face.ZPlus, pos, blockSize, blockMaterials);
            }

            if (chunk[x, y, z - 1] == Block.Air)
            {
                this.AddFace(Face.ZMinus, pos, blockSize, blockMaterials);
            }
        }

        private void AddFace(
            Face face,
            (int x, int y, int z) pos,
            float blockSize,
            BlockMaterials blockMaterials
        )
        {
            Material mat = blockMaterials.GetMaterial(face);

            if (!this.submeshes.TryGetValue(mat, out List<int> ts))
            {
                ts = new List<int>();
                this.submeshes.Add(mat, ts);
            }

            (Vector3 a, Vector3 b, Vector3 c, Vector3 d) = FaceToQuad(face);

            // Subtract 1 to compensate 1-unit border.
            var offset = new Vector3Int(pos.x - 1, pos.y, pos.z - 1);

            int v = this.vs.Count;

            this.vs.Add((a + offset) * blockSize);
            this.vs.Add((b + offset) * blockSize);
            this.vs.Add((c + offset) * blockSize);
            this.vs.Add((d + offset) * blockSize);

            ts.Add(v);
            ts.Add(v + 1);
            ts.Add(v + 2);

            ts.Add(v);
            ts.Add(v + 2);
            ts.Add(v + 3);

            this.uvs.Add(new Vector2(0, 0));
            this.uvs.Add(new Vector2(0, 1));
            this.uvs.Add(new Vector2(1, 1));
            this.uvs.Add(new Vector2(1, 0));
        }
    }
}
