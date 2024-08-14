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
            float blockSize,
            GenerationParameters genParams
        )
        {
            float waterLevel = genParams.baseHeight + genParams.baseTerrain.magnitude;

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

                    float groundHeight = GetGroundHeight(globalX, globalZ, genParams);

                    int groundHeightInBlocks = (int)(groundHeight / blockSize);

                    var column = new Block[groundHeightInBlocks];

                    Array.Fill(column, value: Block.Dirt);

                    column[^1] = Block.Grass;

                    int waterLevelInBlocks = (int)(MathF.Ceiling(waterLevel) / blockSize) + 1;

                    if (groundHeightInBlocks >= waterLevelInBlocks)
                    {
                        column[^1] = Block.Grass;
                    }
                    else if (groundHeightInBlocks < waterLevelInBlocks)
                    {
                        column[^1] = Block.Sand;
                    }

                    chunk.SetColumn(x, z, column);
                }
            }

            return chunk;
        }

        private static float GetGroundHeight(float x, float z, GenerationParameters genParams)
        {
            float waterLevel = genParams.baseHeight + genParams.baseTerrain.magnitude;

            float groundHeight = genParams.baseHeight;

            // Base terrain (land vs ocean)
            groundHeight += genParams.baseTerrain.Get(x, z);

            float inlandFactor =
                groundHeight / (genParams.baseHeight + genParams.baseTerrain.magnitude);

            // Mountains
            groundHeight +=
                genParams.mountains.Get(x, z)
                * MathF.Pow(inlandFactor, genParams.mountainInlandExponent)
                * genParams.mountainFilter.Get(x, z);

            // Hills
            groundHeight +=
                genParams.hills.Get(x, z) * MathF.Pow(inlandFactor, genParams.hillInlandExponent);

            // Rivers
            float riverNoise = genParams.rivers.Get(x, z);
            float maxDepth = genParams.rivers.magnitude;
            float heightAtMaxDepth = waterLevel - maxDepth;
            float delta = (groundHeight - heightAtMaxDepth) * riverNoise;

            if (delta > 0)
            {
                groundHeight -= delta;
            }

            // River floor
            groundHeight += genParams.riverFloor.Get(x, z) * riverNoise;

            return groundHeight;
        }
    }
}
