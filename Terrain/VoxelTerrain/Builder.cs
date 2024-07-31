using System;
using System.Threading.Tasks;
using UnityEngine;

namespace UnityUtilities.Terrain
{
    public static class Builder
    {
        private enum Face : byte
        {
            XPlus,
            XMinus,
            YPlus,
            YMinus,
            ZPlus,
            ZMinus,
        }

        public static async Task<(Chunk, GameObject)> BuildChunk(
            (int x, int z) chunkIndex,
            int chunkWidthInBlocks,
            float blockSize,
            Material[] materials,
            int groundLayer
        )
        {
            string name = FormattableString.Invariant($"Chunk_x{chunkIndex.x}_z{chunkIndex.z}");

            (Chunk chunk, Mesh mesh) = await SafeTask.Run(() =>
            {
                Chunk c = Generator.GenerateChunkWithBorder(
                    chunkIndex,
                    chunkWidthInBlocks,
                    blockSize
                );
                Mesh m = BuildChunkMesh(c, blockSize);

                return (c, m);
            });

            UnityEngine.Mesh unityMesh = mesh.ToUnityMesh(name);

            unityMesh.RecalculateNormals();

            // Do not optimize as meshes are created at run time.
            // unityMesh.Optimize();

            var chunkGO = new GameObject { name = name, layer = groundLayer };

            chunkGO.transform.position = new Vector3(
                chunkIndex.x * chunkWidthInBlocks * blockSize,
                0,
                chunkIndex.z * chunkWidthInBlocks * blockSize
            );

            MeshFilter meshFilter = chunkGO.AddComponent<MeshFilter>();
            meshFilter.sharedMesh = unityMesh;

            MeshRenderer meshRenderer = chunkGO.AddComponent<MeshRenderer>();
            meshRenderer.sharedMaterials = materials;

            _ = chunkGO.AddComponent<MeshCollider>();

            return (chunk, chunkGO);
        }

        private static Mesh BuildChunkMesh(Chunk chunk, float blockSize)
        {
            var mesh = new Mesh();

            foreach ((int x, int y, int z, Block block) in chunk.EnumerateWithoutBorder())
            {
                if (block == Block.Air)
                {
                    continue;
                }

                // Because Block.Air is 0, we decrement submesh index by 1 to
                // avoid the first submesh always being empty.
                int submesh = (byte)block - 1;

                // Offset to compensate 1-unit border.
                var pos = new Vector3(x - 1, y, z - 1);

                if (chunk[x + 1, y, z] == Block.Air)
                {
                    AddFace(mesh, Face.XPlus, pos, blockSize, submesh);
                }

                if (chunk[x - 1, y, z] == Block.Air)
                {
                    AddFace(mesh, Face.XMinus, pos, blockSize, submesh);
                }

                if (chunk[x, y + 1, z] == Block.Air)
                {
                    AddFace(mesh, Face.YPlus, pos, blockSize, submesh);
                }

                if (y > 0 && chunk[x, y - 1, z] == Block.Air)
                {
                    AddFace(mesh, Face.YMinus, pos, blockSize, submesh);
                }

                if (chunk[x, y, z + 1] == Block.Air)
                {
                    AddFace(mesh, Face.ZPlus, pos, blockSize, submesh);
                }

                if (chunk[x, y, z - 1] == Block.Air)
                {
                    AddFace(mesh, Face.ZMinus, pos, blockSize, submesh);
                }
            }

            return mesh;
        }

        private static void AddFace(
            Mesh mesh,
            Face face,
            Vector3 offset,
            float blockSize,
            int submesh
        )
        {
            Vector3 v0 = offset;
            Vector3 v1 = offset;
            Vector3 v2 = offset;
            Vector3 v3 = offset;

            if (face == Face.XPlus)
            {
                v0 += new Vector3(1, 0, 0);
                v1 += new Vector3(1, 1, 0);
                v2 += new Vector3(1, 1, 1);
                v3 += new Vector3(1, 0, 1);
            }
            else if (face == Face.XMinus)
            {
                v0 += new Vector3(0, 0, 1);
                v1 += new Vector3(0, 1, 1);
                v2 += new Vector3(0, 1, 0);
                v3 += new Vector3(0, 0, 0);
            }
            else if (face == Face.YPlus)
            {
                v0 += new Vector3(0, 1, 0);
                v1 += new Vector3(0, 1, 1);
                v2 += new Vector3(1, 1, 1);
                v3 += new Vector3(1, 1, 0);
            }
            else if (face == Face.YMinus)
            {
                v0 += new Vector3(0, 0, 1);
                v1 += new Vector3(0, 0, 0);
                v2 += new Vector3(1, 0, 0);
                v3 += new Vector3(1, 0, 1);
            }
            else if (face == Face.ZPlus)
            {
                v0 += new Vector3(1, 0, 1);
                v1 += new Vector3(1, 1, 1);
                v2 += new Vector3(0, 1, 1);
                v3 += new Vector3(0, 0, 1);
            }
            else if (face == Face.ZMinus)
            {
                v0 += new Vector3(0, 0, 0);
                v1 += new Vector3(0, 1, 0);
                v2 += new Vector3(1, 1, 0);
                v3 += new Vector3(1, 0, 0);
            }
            else
            {
                throw new Exception("Unexpected block face.");
            }

            v0 *= blockSize;
            v1 *= blockSize;
            v2 *= blockSize;
            v3 *= blockSize;

            mesh.AddSquare(v0, v1, v2, v3, submesh);

            mesh.uvs.Add(new Vector2(0, 0));
            mesh.uvs.Add(new Vector2(0, 1));
            mesh.uvs.Add(new Vector2(1, 1));
            mesh.uvs.Add(new Vector2(1, 0));
        }
    }
}
