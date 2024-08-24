using System;
using System.Collections.Generic;
using UnityEngine;

namespace UnityUtilities.Terrain
{
    public static class Builder
    {
        public static GameObject BuildChunkGameObject(
            (int x, int z) chunkIndex,
            Chunk chunk,
            int chunkWidthInBlocks,
            float blockSize,
            Dictionary<Block, BlockMaterials> materialsByBlockType,
            int groundLayer
        )
        {
            string name = FormattableString.Invariant($"Chunk_x{chunkIndex.x}_z{chunkIndex.z}");

            Mesh mesh = BuildChunkMesh(chunk, blockSize, materialsByBlockType);

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

            meshRenderer.sharedMaterials = mesh.materials.ToArray();

            _ = chunkGO.AddComponent<MeshCollider>();

            return chunkGO;
        }

        private static Mesh BuildChunkMesh(
            Chunk chunk,
            float blockSize,
            Dictionary<Block, BlockMaterials> materialsByBlockType
        )
        {
            Mesh mesh = new();

            Dictionary<Block, byte> submeshByBlock = new();

            foreach ((int x, int y, int z, Block block) in chunk.EnumerateWithoutBorder())
            {
                if (block == Block.Air)
                {
                    continue;
                }

                if (!submeshByBlock.TryGetValue(block, out byte blockSubmeshIndex))
                {
                    int count = mesh.submeshes.Count;
                    submeshByBlock.Add(block, (byte)count);
                    blockSubmeshIndex = (byte)count;
                }

                BlockMaterials blockMaterials = materialsByBlockType[block];

                // Subtract 1 to compensate 1-unit border.
                var offset = new Vector3Int(x - 1, y, z - 1);

                BuildBlock(chunk, mesh, offset, blockSize, blockSubmeshIndex, blockMaterials);
            }

            return mesh;
        }

        private static void BuildBlock(
            Chunk chunk,
            Mesh mesh,
            Vector3Int offset,
            float blockSize,
            byte blockSubmeshIndex,
            BlockMaterials blockMaterials
        )
        {
            void AddFace(Vector3 a, Vector3 b, Vector3 c, Vector3 d, byte submesh)
            {
                mesh.AddSquare(
                    (a + offset) * blockSize,
                    (b + offset) * blockSize,
                    (c + offset) * blockSize,
                    (d + offset) * blockSize,
                    submesh
                );

                mesh.uvs.Add(new Vector2(0, 0));
                mesh.uvs.Add(new Vector2(0, 1));
                mesh.uvs.Add(new Vector2(1, 1));
                mesh.uvs.Add(new Vector2(1, 0));
            }

            byte GetSubmesh(Face face) =>
                (byte)(blockSubmeshIndex + blockMaterials.GetMaterialIndex(face));

            int x = offset.x;
            int y = offset.y;
            int z = offset.z;

            if (chunk[x + 1, y, z] == Block.Air)
            {
                AddFace(
                    new Vector3(1, 0, 0),
                    new Vector3(1, 1, 0),
                    new Vector3(1, 1, 1),
                    new Vector3(1, 0, 1),
                    GetSubmesh(Face.XPlus)
                );
            }

            if (chunk[x - 1, y, z] == Block.Air)
            {
                AddFace(
                    new Vector3(0, 0, 1),
                    new Vector3(0, 1, 1),
                    new Vector3(0, 1, 0),
                    new Vector3(0, 0, 0),
                    GetSubmesh(Face.XMinus)
                );
            }

            if (chunk[x, y + 1, z] == Block.Air)
            {
                AddFace(
                    new Vector3(0, 1, 0),
                    new Vector3(0, 1, 1),
                    new Vector3(1, 1, 1),
                    new Vector3(1, 1, 0),
                    GetSubmesh(Face.YPlus)
                );
            }

            if (y > 0 && chunk[x, y - 1, z] == Block.Air)
            {
                AddFace(
                    new Vector3(0, 0, 1),
                    new Vector3(0, 0, 0),
                    new Vector3(1, 0, 0),
                    new Vector3(1, 0, 1),
                    GetSubmesh(Face.YMinus)
                );
            }

            if (chunk[x, y, z + 1] == Block.Air)
            {
                AddFace(
                    new Vector3(1, 0, 1),
                    new Vector3(1, 1, 1),
                    new Vector3(0, 1, 1),
                    new Vector3(0, 0, 1),
                    GetSubmesh(Face.ZPlus)
                );
            }

            if (chunk[x, y, z - 1] == Block.Air)
            {
                AddFace(
                    new Vector3(0, 0, 0),
                    new Vector3(0, 1, 0),
                    new Vector3(1, 1, 0),
                    new Vector3(1, 0, 0),
                    GetSubmesh(Face.ZMinus)
                );
            }
        }
    }
}
