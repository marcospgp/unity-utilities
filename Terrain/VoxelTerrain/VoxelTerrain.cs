using System;
using System.Collections.Generic;
using System.Linq;
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

        // Ensure singleton.
        private static VoxelTerrain instance;

        [SerializeField]
        private List<BlockMaterials> blockMaterials;

        [SerializeField]
        private float viewDistance = 500f;

        [SerializeField, LayerSelect]
        private int groundLayer;

        [Header("Terrain generation parameters")]
        [SerializeField]
        private GenerationParameters generationParameters = new();

        private Task buildTask;
        private CancellationTokenSource cancelTokenSource;

        private Dictionary<Block, BlockMaterials> materialsByBlockType;

        public static event EventHandler<Chunk> OnFirstChunk;

        public void Dispose()
        {
            this.cancelTokenSource.Dispose();
            GC.SuppressFinalize(this);
        }

        // Unity event.
        public async void Start()
        {
            // Ensure singleton.
            if (instance != null && this != VoxelTerrain.instance)
            {
                throw new Exception("Duplicate instance of singleton component detected.");
            }

            VoxelTerrain.instance = this;

            // Build dictionary of materials by block type.
            this.materialsByBlockType = BuildBlockMaterialsDictionary(this.blockMaterials);

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

        private static Dictionary<Block, BlockMaterials> BuildBlockMaterialsDictionary(
            List<BlockMaterials> blockMaterials
        )
        {
            var blockEnumValues = (Block[])Enum.GetValues(typeof(Block));

            // Get block types excluding air.
            Block[] blockTypes = blockEnumValues.Skip(1).ToArray();

            Dictionary<Block, BlockMaterials> materialsByBlockType = new();

            if (blockMaterials.Count != blockTypes.Length)
            {
                throw new Exception(
                    "Number of blockMaterials entries must match number of "
                        + " block types in Block enum (and be in the same "
                        + "order)."
                );
            }

            for (int i = 0; i < blockTypes.Length; i++)
            {
                materialsByBlockType.Add(blockTypes[i], blockMaterials[i]);
            }

            return materialsByBlockType;
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

        private async Task BuildTerrain(CancellationToken token)
        {
            int viewDistanceInChunks = (int)(
                this.viewDistance / (CHUNK_WIDTH_IN_BLOCKS * BLOCK_SIZE)
            );

            foreach ((int x, int z) in Spiral(viewDistanceInChunks))
            {
                token.ThrowIfCancellationRequested();

                Chunk chunk = await Generator.GenerateChunkWithBorder(
                    (x, z),
                    CHUNK_WIDTH_IN_BLOCKS,
                    BLOCK_SIZE,
                    this.generationParameters
                );

                ChunkMesh mesh = await SafeTask.Run(
                    () => new ChunkMesh(chunk, BLOCK_SIZE, this.materialsByBlockType)
                );

                string name = FormattableString.Invariant($"Chunk_x{x}_z{z}");

                GameObject chunkGO = mesh.ToGameObject(
                    name,
                    (x, z),
                    CHUNK_WIDTH_IN_BLOCKS,
                    BLOCK_SIZE,
                    this.groundLayer
                );

                VoxelTerrain.chunks.Add((x, z), (chunk, chunkGO));

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
