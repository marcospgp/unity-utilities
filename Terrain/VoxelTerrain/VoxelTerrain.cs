using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace UnityUtilities.Terrain
{
    public sealed class VoxelTerrain : MonoBehaviour, IDisposable
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

        private readonly GenerationParameters lastGenerationParameters = new();

        [SerializeField]
        [Tooltip("Terrain textures, in the same order as they appear in BlockTexture.")]
        private Texture2D[] textures;

        [SerializeField]
        private Material baseMaterial;

        [SerializeField]
        private float viewDistance = 1000f;

        [SerializeField, LayerSelect]
        private int groundLayer;

        [Header("Terrain generation parameters")]
        [SerializeField]
        private GenerationParameters generationParameters = new();

        private Material[] materials;

        private CancellationTokenSource cancelTokenSource = new();
        private Task task;

        public static event EventHandler<Chunk> OnFirstChunk;

        public void Dispose()
        {
            this.cancelTokenSource.Dispose();
            GC.SuppressFinalize(this);
        }

        // Unity event.
        public async void Start()
        {
            // Cache generation parameters.
            this.lastGenerationParameters.CopyFrom(this.generationParameters);

            // Turn terrain textures into materials.
            this.materials = new Material[this.textures.Length];

            for (int i = 0; i < this.textures.Length; i++)
            {
                this.materials[i] = new Material(this.baseMaterial)
                {
                    mainTexture = this.textures[i],
                };
            }

            this.task = this.BuildTerrain();

            try
            {
                await this.task;
            }
            catch (OperationCanceledException)
            {
                // Suppress.
            }
        }

#if UNITY_EDITOR
        // Unity event.
        public async void Update()
        {
            if (!this.generationParameters.IsEqualTo(this.lastGenerationParameters))
            {
                this.lastGenerationParameters.CopyFrom(this.generationParameters);

                this.cancelTokenSource.Cancel();

                // Wait for terrain to stop generating.
                try
                {
                    await this.task;
                }
                catch (OperationCanceledException)
                {
                    // Suppress.
                }

                this.cancelTokenSource.Dispose();
                this.cancelTokenSource = new CancellationTokenSource();

                foreach ((_, GameObject chunk) in chunks.Values)
                {
                    Destroy(chunk);
                }

                chunks.Clear();

                this.task = this.BuildTerrain();

                try
                {
                    await this.task;
                }
                catch (OperationCanceledException)
                {
                    // Suppress.
                }
            }
        }
#endif

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

        private async Task BuildTerrain()
        {
            int viewDistanceInChunks = (int)(
                this.viewDistance / (CHUNK_WIDTH_IN_BLOCKS * BLOCK_SIZE)
            );

            foreach ((int x, int z) in Spiral(viewDistanceInChunks))
            {
                this.cancelTokenSource.Token.ThrowIfCancellationRequested();

                VoxelTerrain.chunks.Add(
                    (x, z),
                    await Builder.BuildChunk(
                        (x, z),
                        CHUNK_WIDTH_IN_BLOCKS,
                        BLOCK_SIZE,
                        this.materials,
                        this.groundLayer,
                        this.generationParameters
                    )
                );

                if (x == 0 && z == 0)
                {
                    VoxelTerrain.OnFirstChunk(sender: null, chunks[(0, 0)].chunk);
                }
            }
        }
    }
}
