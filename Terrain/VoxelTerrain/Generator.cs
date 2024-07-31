using System;

namespace UnityUtilities.Terrain
{
    public static class Generator
    {
        /// <summary>
        /// Generating a chunk with a 1-block border is useful for building a
        /// chunk's mesh, as we check neighboring blocks when deciding whether
        /// to draw each face.
        /// </summary>
        ///
        public static Chunk GenerateChunkWithBorder(
            (int x, int z) chunkIndex,
            int chunkWidthInBlocks,
            float blockSize
        )
        {
            // Account for border.
            int w = chunkWidthInBlocks + 2;

            var chunk = new Chunk(w);

            for (int x = 0; x < w; x++)
            {
                for (int z = 0; z < w; z++)
                {
                    // -1 to account for border.
                    float globalX = ((chunkIndex.x * chunkWidthInBlocks) + x - 1) * blockSize;
                    float globalZ = ((chunkIndex.z * chunkWidthInBlocks) + z - 1) * blockSize;

                    float groundHeight = GetGroundHeight(globalX, globalZ);

                    int groundHeightInBlocks = (int)MathF.Round(groundHeight / blockSize);

                    var column = new Block[groundHeightInBlocks];

                    Array.Fill(column, value: Block.Dirt);

                    column[^1] = Block.Grass;

                    chunk.SetColumn(x, z, column);
                }
            }

            return chunk;
        }

        private static float GetGroundHeight(float x, float z)
        {
            // Ocean floor height.
            const float baseHeight = 128;

            // 0 = ocean, 1 = land
            float land = PerlinNoise.Get(x, z, baseFrequency: 0.01f) * 128;

            return baseHeight + land;
        }
    }
}
