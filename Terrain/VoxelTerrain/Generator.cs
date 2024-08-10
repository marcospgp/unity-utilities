using System;

namespace UnityUtilities.Terrain
{
    public static class Generator
    {
        private const float BASE_HEIGHT = 64f; // Ocean floor height.
        private const float LAND_HEIGHT = 64f;
        private const float WATER_LEVEL = BASE_HEIGHT + LAND_HEIGHT - 3.2f;

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
            GenerationParameters generationParameters
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

                    float groundHeight = GetGroundHeight(globalX, globalZ, generationParameters);

                    int groundHeightInBlocks = (int)(groundHeight / blockSize);

                    var column = new Block[groundHeightInBlocks];

                    Array.Fill(column, value: Block.Dirt);

                    column[^1] = Block.Grass;

                    // int waterLevelInBlocks = (int)(MathF.Ceiling(WATER_LEVEL) / blockSize) + 1;

                    // if (groundHeightInBlocks > waterLevelInBlocks)
                    // {
                    //     column[^1] = Block.Grass;
                    // }
                    // else if (groundHeightInBlocks == waterLevelInBlocks)
                    // {
                    //     column[^1] = Block.Sand;
                    // }

                    chunk.SetColumn(x, z, column);
                }
            }

            return chunk;
        }

        private static float GetGroundHeight(
            float x,
            float z,
            GenerationParameters generationParameters
        )
        {
            float landNoise = PerlinNoise.Get(
                x,
                z,
                seed: "base",
                baseFrequency: generationParameters.baseFrequency,
                numberOfOctaves: generationParameters.baseOctaves,
                lacunarity: generationParameters.baseLacunarity,
                persistence: generationParameters.basePersistence
            );

            // Controls land to ocean ratio.
            // 1 = small islands.
            landNoise = MathF.Pow(landNoise, generationParameters.baseExponent);

            // Remap into [-0.5, 0.5] for sigmoid.
            landNoise = (landNoise * 2f) - 1f;
            landNoise = Sigmoid(landNoise, generationParameters.baseSigmoidSlope);

            float landHeight = landNoise * LAND_HEIGHT;

            // // Mountains
            // float mountainHeight = 0;

            // bool isLand = landHeight > WATER_LEVEL;

            // if (isLand)
            // {
            //     float mountainNoise = PerlinNoise.Get(
            //         x,
            //         z,
            //         seed: "mountains",
            //         baseFrequency: 0.03f,
            //         numberOfOctaves: 1,
            //         lacunarity: 4f,
            //         persistence: 0.8f
            //     );

            //     mountainNoise = MathF.Pow(mountainNoise, 4f);

            //     // mountainHeight = 32f * mountainNoise;
            // }

            return BASE_HEIGHT + landHeight; // + mountainHeight;
        }

        private static float Sigmoid(float x, float k) => 1f / (1f + MathF.Exp(-k * x));
    }
}
