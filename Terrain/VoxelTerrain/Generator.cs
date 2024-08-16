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
            float waterLevel = genParams.baseHeight + genParams.baseTerrain.magnitude;
            float inlandFactor = genParams.baseTerrain.GetRaw(x, z);

            float groundHeight = genParams.baseHeight;

            float baseTerrainHeight = genParams.baseTerrain.Get(x, z);
            groundHeight += baseTerrainHeight;

            float mountainHeight = GetMountainHeight(x, z, genParams, inlandFactor);
            groundHeight += mountainHeight;

            float hillHeight =
                genParams.hills.Get(x, z) * MathF.Pow(inlandFactor, genParams.hillInlandExponent);
            groundHeight += hillHeight;

            // Rivers
            float riverDepth = GetRiverDepth(x, z, genParams, waterLevel, groundHeight);
            groundHeight -= riverDepth;

            int groundHeightInBlocks = (int)(groundHeight / blockSize);

            // Allocate array now that we know final ground height.
            var column = new Block[groundHeightInBlocks];

            Array.Fill(column, value: Block.Dirt);

            column[^1] = Block.Grass;

            int waterLevelInBlocks = (int)(MathF.Ceiling(waterLevel) / blockSize);

            if (groundHeightInBlocks >= waterLevelInBlocks)
            {
                column[^1] = Block.Grass;
            }
            else if (groundHeightInBlocks < waterLevelInBlocks)
            {
                column[^1] = Block.Sand;
            }

            return column;
        }

        private static float GetMountainHeight(
            float x,
            float z,
            GenerationParameters genParams,
            float inlandFactor
        )
        {
            float mountainHeight = 0f;
            float mountainFilter = genParams.mountainFilter.enabled
                ? genParams.mountainFilter.Get(x, z)
                : 1f;

            if (genParams.visualizeMountainFilter)
            {
                mountainHeight += mountainFilter * genParams.mountains.magnitude;
            }

            if (genParams.mountains.enabled)
            {
                mountainHeight =
                    genParams.mountains.Get(x, z)
                    * MathF.Pow(inlandFactor, genParams.mountainInlandExponent)
                    * mountainFilter;
            }

            return mountainHeight;
        }

        private static float GetRiverDepth(
            float x,
            float z,
            GenerationParameters genParams,
            float waterLevel,
            float groundHeight
        )
        {
            float riverNoise = genParams.rivers.Get(x, z);
            float heightAtMaxRiverDepth = waterLevel - genParams.riverMaxDepth;
            float riverDepth = MathF.Max(0f, (groundHeight - heightAtMaxRiverDepth) * riverNoise);
            riverDepth = MathF.Min(riverDepth, groundHeight - 1f);

            // Prevent river floor from:
            //   - being added where a river wasn't carved;
            //   - reaching river surface.
            float maxRiverFloorHeight = MathF.Max(0f, riverDepth - 1f);

            float riverFloorHeight = MathF.Min(
                genParams.riverFloor.Get(x, z) * riverNoise,
                maxRiverFloorHeight
            );

            return riverDepth - riverFloorHeight;
        }
    }
}
