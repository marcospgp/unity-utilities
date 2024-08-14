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

        [SerializeField]
        [Tooltip("Terrain textures, in the same order as they appear in BlockTexture.")]
        private Texture2D[] textures;

        [SerializeField]
        private Material baseMaterial;

        [SerializeField]
        private float viewDistance = 500f;

        [SerializeField, LayerSelect]
        private int groundLayer;

        [Header("Terrain generation parameters")]
        [SerializeField]
        private GenerationParameters generationParameters = new();

        private Material[] materials;

        private Task buildTask;
        private CancellationTokenSource cancelTokenSource;

        public static event EventHandler<Chunk> OnFirstChunk;

        public void Dispose()
        {
            this.cancelTokenSource.Dispose();
            GC.SuppressFinalize(this);
        }

        // Unity event.
        public async void Start()
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

            await this.RegenerateTerrain();
        }

#if UNITY_EDITOR
        public async void OnValidate()
        {
            // Only regenerate terrain when in play mode and when terrain has
            // already started being generated (this method is also called on
            // play start when nothing has changed in inspector).
            if (Application.isPlaying && this.buildTask != null)
            {
                await this.RegenerateTerrain();
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

        private async Task BuildTerrain(CancellationToken token)
        {
            int viewDistanceInChunks = (int)(
                this.viewDistance / (CHUNK_WIDTH_IN_BLOCKS * BLOCK_SIZE)
            );

            foreach ((int x, int z) in Spiral(viewDistanceInChunks))
            {
                token.ThrowIfCancellationRequested();

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

        // This method cannot await anything because it can be called in quick
        // succession. It must be atomic.
        private Task RegenerateTerrain()
        {
            // On first terrain generation object will be null.
            this.cancelTokenSource?.Cancel();
            this.cancelTokenSource?.Dispose();
            this.cancelTokenSource = new CancellationTokenSource();

            this.buildTask = this.RegenerateTerrainTask(
                this.buildTask,
                this.cancelTokenSource.Token
            );

            return this.buildTask;
        }

        private async Task RegenerateTerrainTask(Task previousTask, CancellationToken token)
        {
            if (previousTask != null)
            {
                try
                {
                    await previousTask;
                }
                catch (OperationCanceledException)
                {
                    // Suppress.
                }
            }

            foreach ((_, GameObject chunk) in chunks.Values)
            {
                Destroy(chunk);
            }

            chunks.Clear();

            try
            {
                await this.BuildTerrain(token);
            }
            catch (OperationCanceledException)
            {
                // Suppress.
            }
        }
    }
}
