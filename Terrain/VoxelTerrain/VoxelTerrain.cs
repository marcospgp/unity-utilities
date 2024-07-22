using System;
using System.Collections.Generic;
using UnityEngine;

namespace UnityUtilities.Terrain
{
    public class VoxelTerrain : MonoBehaviour
    {
        private const int CHUNK_WIDTH = 16;
        private const int CHUNK_HEIGHT = 256;

        private readonly Dictionary<(int, int), GameObject> chunks = new();

        [SerializeField]
        private int viewDistanceInChunks = 16;

        [SerializeField, LayerSelect]
        private int groundLayer;

        [SerializeField]
        private Material dirtMaterial;

        public void Start()
        {
            foreach ((int x, int z) in Spiral(this.viewDistanceInChunks))
            {
                this.chunks.Add((x, z), this.BuildChunk(x, z));
            }
        }

        // Enumerates coordinates for a spiral.
        private static IEnumerable<(int, int)> Spiral(int radius)
        {
            int radiusSquared = radius * radius;
            int x = 0;
            int z = 0;

            yield return (x, z);

            bool IsWithinRadius(int x, int z) => ((x * x) + (z * z)) <= radiusSquared;

            for (int i = 1; i < radius; i++)
            {
                z -= 1;

                for (; x < +i; x++)
                {
                    if (IsWithinRadius(x, z))
                    {
                        yield return (x, z);
                    }
                }

                for (; z < +i; z++)
                {
                    if (IsWithinRadius(x, z))
                    {
                        yield return (x, z);
                    }
                }

                for (; x > -i; x--)
                {
                    if (IsWithinRadius(x, z))
                    {
                        yield return (x, z);
                    }
                }

                for (; z > -i; z--)
                {
                    if (IsWithinRadius(x, z))
                    {
                        yield return (x, z);
                    }
                }

                if (IsWithinRadius(x, z))
                {
                    yield return (x, z);
                }
            }
        }

        private GameObject BuildChunk(int x, int z)
        {
            string name = FormattableString.Invariant($"Chunk_x{x}_z{z}");

            Mesh chunkMesh = MeshBuilder.BuildChunkMesh((x, z), (CHUNK_WIDTH, CHUNK_HEIGHT));

            var chunk = new GameObject { name = name, layer = this.groundLayer };

            chunk.transform.position = new Vector3(x * CHUNK_WIDTH, 0, z * CHUNK_WIDTH);

            MeshRenderer meshRenderer = chunk.AddComponent<MeshRenderer>();

            meshRenderer.material = this.dirtMaterial;

            MeshFilter meshFilter = chunk.AddComponent<MeshFilter>();

            // Use sharedMesh and not mesh to avoid making a copy
            // unnecessarily.
            meshFilter.sharedMesh = chunkMesh;

            _ = chunk.AddComponent<MeshCollider>();

            return chunk;
        }
    }
}
