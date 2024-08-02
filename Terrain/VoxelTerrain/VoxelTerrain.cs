using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace UnityUtilities.Terrain
{
    public class VoxelTerrain : MonoBehaviour
    {
        public const float BLOCK_SIZE = 1f;
        private const int CHUNK_WIDTH_IN_BLOCKS = 16;

        /// <summary>
        /// Chunks.
        /// Note X and Z are indices, not real coordinates.
        /// </summary>
        private static readonly Dictionary<
            (int x, int z),
            (Chunk chunk, GameObject chunkGameObject)
        > chunks = new();

        [SerializeField]
        [Tooltip("Terrain textures, in the same order as they appear in BlockTexture.")]
        private Texture2D[] textures;

        [SerializeField]
        private Material baseMaterial;

        [SerializeField]
        private float viewDistance = 1000f;

        [SerializeField, LayerSelect]
        private int groundLayer;

        private Material[] materials;

        public static event EventHandler<Chunk> OnFirstChunk;

        // Unity event.
        public async Task Start()
        {
            // Turn terrain textures into materials.
            this.materials = new Material[this.textures.Length];

            for (int i = 0; i < this.textures.Length; i++)
            {
                this.materials[i] = new Material(this.baseMaterial)
                {
                    mainTexture = this.textures[i],
                };
            }

            int viewDistanceInChunks = (int)(
                this.viewDistance / (CHUNK_WIDTH_IN_BLOCKS * BLOCK_SIZE)
            );

            foreach ((int x, int z) in Spiral(viewDistanceInChunks))
            {
                VoxelTerrain.chunks.Add(
                    (x, z),
                    await Builder.BuildChunk(
                        (x, z),
                        CHUNK_WIDTH_IN_BLOCKS,
                        BLOCK_SIZE,
                        this.materials,
                        this.groundLayer
                    )
                );

                if (x == 0 && z == 0)
                {
                    VoxelTerrain.OnFirstChunk(sender: null, chunks[(0, 0)].chunk);
                    // Flag for garbage collection.
                    VoxelTerrain.OnFirstChunk = null;
                }
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
    }
}
