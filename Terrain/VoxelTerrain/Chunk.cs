using System;
using System.Collections.Generic;

namespace UnityUtilities.Terrain
{
    public readonly struct Chunk
    {
        private readonly Block[,][] blocks;

        public Chunk(int width)
        {
            this.blocks = new Block[width, width][];

            for (int x = 0; x < width; x++)
            {
                for (int z = 0; z < width; z++)
                {
                    this.blocks[x, z] = Array.Empty<Block>();
                }
            }
        }

        public Block this[int x, int y, int z]
        {
            get
            {
                // Never error when y is too large - simply return air.
                if (y >= this.blocks[x, z].Length)
                {
                    return Block.Air;
                }

                return this.blocks[x, z][y];
            }
            set => this.blocks[x, z][y] = value;
        }

        public void SetColumn(int x, int z, Block[] column)
        {
            this.blocks[x, z] = column;
        }

        /// <summary>
        /// Enumerate a chunk's blocks, skipping indices of border.
        /// This means X and Z will start at 1 and end 1 earlier.
        /// Useful for when building the chunk's mesh.
        /// </summary>
        public IEnumerable<(int x, int y, int z, Block block)> EnumerateWithoutBorder()
        {
            int width = this.blocks.GetLength(0);
            int depth = this.blocks.GetLength(1);

            for (int x = 1; x < width - 1; x++)
            {
                for (int z = 1; z < depth - 1; z++)
                {
                    Block[] column = this.blocks[x, z];

                    for (int y = 0; y < column.Length; y++)
                    {
                        yield return (x, y, z, column[y]);
                    }
                }
            }
        }

        /// <summary>
        /// Get global Y coordinate of top block at column (x, z).
        /// </summary>
        public float GetTopY(int x, int z)
        {
            Block[] column = this.blocks[x, z];

            return column.Length * VoxelTerrain.BLOCK_SIZE;
        }
    }
}
