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

                    Block[] column = GetColumn(globalX, globalZ, blockSize, genParams);

                    chunk.SetColumn(x, z, column);
                }
            }

            return chunk;
        }

        private static Block[] GetColumn(
            float x,
            float z,
            float blockSize,
            GenerationParameters genParams
        )
        {
            // -1 because we floor ground height when converting to # of blocks.
            float waterLevel = genParams.baseHeight + genParams.baseTerrain.magnitude - 1f;

            float inlandFactor = genParams.baseTerrain.GetRaw(x, z, genParams.seed);

            float groundHeight = genParams.baseHeight;

            float baseTerrainHeight = genParams.baseTerrain.Get(x, z, genParams.seed);
            groundHeight += baseTerrainHeight;

            float hillHeight =
                genParams.hills.Get(x, z, genParams.seed)
                * MathF.Pow(inlandFactor, genParams.hillInlandExponent);
            groundHeight += hillHeight;

            (float mountainStart, float mountainEnd) = GetMountainHeight(
                x,
                z,
                genParams,
                inlandFactor
            );
            int mountainBaseHeightInBlocks = (int)(groundHeight / blockSize);
            int mountainStartHeightInBlocks = (int)((groundHeight + mountainStart) / blockSize);
            groundHeight += mountainEnd;

            // Rivers
            float riverDepth = GetRiverDepth(x, z, genParams, waterLevel, groundHeight);
            groundHeight -= riverDepth;

            int groundHeightInBlocks = (int)(groundHeight / blockSize);

            // Allocate array now that we know final ground height.
            var column = new Block[groundHeightInBlocks];

            Array.Fill(column, value: Block.Dirt);

            for (int i = mountainBaseHeightInBlocks; i < mountainStartHeightInBlocks; i++)
            {
                if (i < column.Length)
                {
                    column[i] = Block.Air;
                }
            }

            int waterLevelInBlocks = (int)(waterLevel / blockSize);

            if (
                groundHeightInBlocks <= waterLevelInBlocks
                && inlandFactor < genParams.beachInlandThreshold
            )
            {
                column[^1] = Block.Sand;
            }
            else if (groundHeightInBlocks >= waterLevelInBlocks)
            {
                column[^1] = Block.Grass;
            }

            return column;
        }

        /// <returns>Start and end height, which allows for overhangs.</returns>
        private static (float start, float end) GetMountainHeight(
            float x,
            float z,
            GenerationParameters genParams,
            float inlandFactor
        )
        {
            string seed = genParams.seed;

            float mountainStart = genParams.overhangFilter.Get(x, z, seed);
            float mountainEnd = 0f;
            float mountainFilter = genParams.mountainFilter.enabled
                ? genParams.mountainFilter.Get(x, z, seed)
                : 1f;

            if (genParams.visualizeMountainFilter)
            {
                mountainEnd += mountainFilter * genParams.mountains.magnitude;
            }

            if (genParams.visualizeOverhangFilter)
            {
                mountainEnd += mountainStart;
                mountainStart = 0f;
            }

            if (genParams.mountains.enabled)
            {
                mountainEnd +=
                    genParams.mountains.Get(x, z, seed)
                    * MathF.Pow(inlandFactor, genParams.mountainInlandExponent)
                    * mountainFilter;
            }

            if (mountainStart >= mountainEnd)
            {
                return (0f, 0f);
            }

            return (mountainStart, mountainEnd);
        }

        private static float GetRiverDepth(
            float x,
            float z,
            GenerationParameters genParams,
            float waterLevel,
            float groundHeight
        )
        {
            float riverNoise = genParams.rivers.Get(x, z, genParams.seed);
            float heightAtMaxRiverDepth = waterLevel - genParams.riverMaxDepth;
            float riverDepth = MathF.Max(0f, (groundHeight - heightAtMaxRiverDepth) * riverNoise);
            riverDepth = MathF.Min(riverDepth, groundHeight - 1f);

            // Prevent river floor from:
            //   - being added where a river wasn't carved;
            //   - reaching river surface.
            float maxRiverFloorHeight = MathF.Max(0f, riverDepth - 1f);

            float riverFloorHeight = MathF.Min(
                genParams.riverFloor.Get(x, z, genParams.seed) * riverNoise,
                maxRiverFloorHeight
            );

            return riverDepth - riverFloorHeight;
        }
    }
}
