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

                    int waterLevelInBlocks = (int)(MathF.Ceiling(WATER_LEVEL) / blockSize) + 1;

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
            float groundHeight = BASE_HEIGHT;

            //
            // Base terrain (land vs ocean)
            //

            float landNoise = PerlinNoise.Get(
                x,
                z,
                seed: "base",
                baseFrequency: genParams.baseFrequency,
                numberOfOctaves: genParams.baseOctaves,
                lacunarity: genParams.baseLacunarity,
                persistence: genParams.basePersistence
            );

            // baseExponent controls land to ocean ratio.
            landNoise = MathF.Pow(landNoise, genParams.baseExponent);

            // Remap into [-1, 1] for sigmoid.
            landNoise = (landNoise * 2f) - 1f;
            landNoise = Sigmoid(landNoise, genParams.baseSigmoidSlope);

            groundHeight += landNoise * LAND_HEIGHT;

            //
            // Mountain terrain
            //

            float mountainHeight = 0f;

            if (genParams.mountainsEnabled)
            {
                float mountainNoise = PerlinNoise.Get(
                    x,
                    z,
                    seed: "mountain",
                    baseFrequency: genParams.mountainFrequency,
                    numberOfOctaves: genParams.mountainOctaves,
                    lacunarity: genParams.mountainLacunarity,
                    persistence: genParams.mountainPersistence
                );

                mountainNoise =
                    (mountainNoise - genParams.mountainNoiseFloor)
                    / (genParams.mountainNoiseCeiling - genParams.mountainNoiseFloor);
                mountainNoise = MathF.Max(mountainNoise, 0f);
                mountainNoise = MathF.Min(mountainNoise, 1f);

                mountainNoise = MathF.Pow(mountainNoise, genParams.mountainExponent);

                if (genParams.mountainSigmoid)
                {
                    // Remap into [-1, 1] for sigmoid.
                    mountainNoise = (mountainNoise * 2f) - 1f;
                    mountainNoise = Sigmoid(mountainNoise, genParams.mountainSigmoidSlope);
                }

                // Multiply by landNoise again to avoid underwater mountains.
                mountainHeight =
                    mountainNoise
                    * genParams.mountainHeight
                    * MathF.Pow(landNoise, genParams.mountainInlandExponent);
            }

            //
            // Mountain filter
            //

            if (genParams.mountainFilterEnabled)
            {
                float mountainFilterNoise = PerlinNoise.Get(
                    x,
                    z,
                    seed: "mountain filter",
                    baseFrequency: genParams.mountainFilterFrequency,
                    numberOfOctaves: genParams.mountainFilterOctaves,
                    lacunarity: genParams.mountainFilterLacunarity,
                    persistence: genParams.mountainFilterPersistence
                );

                mountainFilterNoise =
                    (mountainFilterNoise - genParams.mountainFilterNoiseFloor)
                    / (genParams.mountainFilterNoiseCeiling - genParams.mountainFilterNoiseFloor);
                mountainFilterNoise = MathF.Max(mountainFilterNoise, 0f);
                mountainFilterNoise = MathF.Min(mountainFilterNoise, 1f);

                mountainFilterNoise = MathF.Pow(
                    mountainFilterNoise,
                    genParams.mountainFilterExponent
                );

                if (genParams.mountainFilterSigmoid)
                {
                    // Remap into [-1, 1] for sigmoid.
                    mountainFilterNoise = (mountainFilterNoise * 2f) - 1f;
                    mountainFilterNoise = Sigmoid(
                        mountainFilterNoise,
                        genParams.mountainFilterSigmoidSlope
                    );
                }

                // Multiply by landNoise again to avoid underwater mountains.
                mountainHeight *=
                    mountainFilterNoise
                    * MathF.Pow(landNoise, genParams.mountainFilterInlandExponent);
            }

            groundHeight += mountainHeight;

            //
            // Hills
            //

            if (genParams.hillsEnabled)
            {
                float hillNoise = PerlinNoise.Get(
                    x,
                    z,
                    seed: "hill",
                    baseFrequency: genParams.hillFrequency,
                    numberOfOctaves: genParams.hillOctaves,
                    lacunarity: genParams.hillLacunarity,
                    persistence: genParams.hillPersistence
                );

                hillNoise =
                    (hillNoise - genParams.hillNoiseFloor)
                    / (genParams.hillNoiseCeiling - genParams.hillNoiseFloor);
                hillNoise = MathF.Max(hillNoise, 0f);
                hillNoise = MathF.Min(hillNoise, 1f);

                hillNoise = MathF.Pow(hillNoise, genParams.hillExponent);

                if (genParams.hillSigmoid)
                {
                    // Remap into [-1, 1] for sigmoid.
                    hillNoise = (hillNoise * 2f) - 1f;
                    hillNoise = Sigmoid(hillNoise, genParams.hillSigmoidSlope);
                }

                groundHeight +=
                    hillNoise
                    * MathF.Pow(landNoise, genParams.hillInlandExponent)
                    * genParams.hillHeight;
            }

            //
            // Rivers
            //

            if (genParams.riversEnabled)
            {
                float riverNoise = PerlinNoise.Get(
                    x,
                    z,
                    seed: "river",
                    baseFrequency: genParams.riverFrequency,
                    numberOfOctaves: 1
                );

                // Use absolute function to create river "lines".
                riverNoise = 1f - (MathF.Abs(riverNoise - 0.5f) * 2f);

                riverNoise =
                    (riverNoise - genParams.riverNoiseFloor)
                    / (genParams.riverNoiseCeiling - genParams.riverNoiseFloor);
                riverNoise = MathF.Max(riverNoise, 0f);
                riverNoise = MathF.Min(riverNoise, 1f);

                riverNoise = MathF.Pow(riverNoise, genParams.riverExponent);

                if (genParams.riverSigmoid)
                {
                    // Remap into [-1, 1] for sigmoid.
                    riverNoise = (riverNoise * 2f) - 1f;
                    riverNoise = Sigmoid(riverNoise, genParams.riverSigmoidSlope);
                }

                float heightAtMaxDepth = WATER_LEVEL - genParams.riverMaxDepth;

                float delta = (groundHeight - heightAtMaxDepth) * riverNoise;

                if (delta > 0)
                {
                    groundHeight -= delta;
                }
            }

            return groundHeight;
        }

        private static float Sigmoid(float x, float slope) => 1f / (1f + MathF.Exp(-slope * x));
    }
}
