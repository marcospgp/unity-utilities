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
            inlandFactor = ExtendedPerlinNoise.Step(
                inlandFactor,
                genParams.inlandStepLow,
                genParams.inlandStepHigh
            );

            float groundHeight = genParams.baseHeight;

            groundHeight += genParams.baseTerrain.Get(x, z, genParams.seed);

            groundHeight += GetHillHeight(x, z, genParams, inlandFactor);

            (float riverStart, float riverEnd) = GetRiverHeight(
                x,
                z,
                genParams,
                waterLevel,
                groundHeight
            );

            (float mountainStart, float mountainEnd) = GetMountainHeight(
                x,
                z,
                genParams,
                inlandFactor,
                groundHeight
            );

            groundHeight = MathF.Max(waterLevel, mountainEnd);

            int groundHeightInBlocks = (int)(groundHeight / blockSize);

            // Allocate array now that we know final ground height.
            var column = new Block[groundHeightInBlocks];

            int riverStartInBlocks = (int)(riverStart / blockSize);
            int riverEndInBlocks = (int)(riverEnd / blockSize);
            int waterLevelInBlocks = (int)(waterLevel / blockSize);

            // Place base terrain.
            for (int i = 0; i < riverStartInBlocks; i++)
            {
                column[i] = Block.Dirt;
            }

            // Place river water.
            for (int i = riverStartInBlocks; i < waterLevelInBlocks; i++)
            {
                // TODO: rivers should be water blocks.
                //
                // if (i <= waterLevelInBlocks)
                // {
                //     column[i] = Block.Water;
                // }

                column[i] = Block.Air;
            }

            int mountainStartInBlocks = (int)(mountainStart / blockSize);
            int mountainEndInBlocks = (int)(mountainEnd / blockSize);

            // Place mountain.
            for (int i = riverEndInBlocks; i < mountainEndInBlocks; i++)
            {
                if (i < mountainStartInBlocks)
                {
                    // Place overhang (empty space between ground and mountain
                    // start).
                    column[i] = Block.Air;
                }
                else
                {
                    column[i] = Block.Dirt;
                }
            }

            // TODO: uncomment this when Block.Water introduced?
            // Assert.That(column[^1] != Block.Air);

            if (column[^1] == Block.Dirt)
            {
                if (
                    groundHeightInBlocks <= waterLevelInBlocks
                    && inlandFactor < genParams.beachInlandThreshold
                )
                {
                    // Turn beaches to sand.
                    column[^1] = Block.Sand;
                }
                else if (groundHeightInBlocks >= waterLevelInBlocks)
                {
                    column[^1] = Block.Grass;
                }
            }

            return column;
        }

        private static float GetHillHeight(
            float x,
            float z,
            GenerationParameters genParams,
            float inlandFactor
        ) =>
            genParams.hills.Get(x, z, genParams.seed)
            * MathF.Pow(inlandFactor, genParams.hillInlandExponent);

        /// <returns>
        /// Start and end height, which allows for overhangs.
        /// </returns>
        private static (float start, float end) GetMountainHeight(
            float x,
            float z,
            GenerationParameters genParams,
            float inlandFactor,
            float groundHeight
        )
        {
            string seed = genParams.seed;

            float start = genParams.overhangFilter.Get(x, z, seed);
            float end = 0f;
            float filter = genParams.mountainFilter.enabled
                ? genParams.mountainFilter.Get(x, z, seed)
                : 1f;

            if (genParams.visualizeMountainFilter)
            {
                end += filter * genParams.mountains.magnitude;
            }

            if (genParams.visualizeOverhangFilter)
            {
                end += start;
                start = 0f;
            }

            if (genParams.mountains.enabled)
            {
                end +=
                    genParams.mountains.Get(x, z, seed)
                    * MathF.Pow(inlandFactor, genParams.mountainInlandExponent)
                    * filter;
            }

            if (start >= end)
            {
                start = 0f;
                end = 0f;
            }

            float span = end - start;

            bool isOverhang = start > 0.01f;

            // Enforce minimum overhang span.
            if (isOverhang && span < 3f)
            {
                // Remove overhang entirely.
                start = 0f;
                end = 0f;
            }

            // Enforce minimum overhang height.
            if (start < 2f)
            {
                // Fill overhang to ground level.
                start = 0f;
            }

            return (groundHeight + start, groundHeight + end);
        }

        /// <returns>
        /// Start and end height, allowing for things like rivers flowing into
        /// mountains and under overhangs.
        /// </returns>
        private static (float start, float end) GetRiverHeight(
            float x,
            float z,
            GenerationParameters genParams,
            float waterLevel,
            float surfaceHeight
        )
        {
            float riverNoise = genParams.rivers.Get(x, z, genParams.seed);

            float heightAtMaxRiverDepth = waterLevel - genParams.riverMaxDepth;

            // Prevent negative delta when terrain is already below max river
            // depth.
            float maxDepth = MathF.Max(0f, surfaceHeight - heightAtMaxRiverDepth);

            float depth = MathF.Max(0f, maxDepth * riverNoise);

            // Prevent river from reaching bottom of chunk.
            // Should only happen by mistake while designing terrain, but better
            // to prevent errors.
            depth = MathF.Min(depth, surfaceHeight - 1f);

            // Prevent river floor from:
            //   - being added where a river wasn't carved;
            //   - reaching river surface.
            float maxRiverFloorHeight = MathF.Max(0f, depth - 1f);

            float riverFloorHeight = MathF.Min(
                genParams.riverFloor.Get(x, z, genParams.seed) * riverNoise,
                maxRiverFloorHeight
            );

            depth -= riverFloorHeight;

            float riverStart = surfaceHeight - depth;

            // TODO: improve.
            float riverEnd = surfaceHeight + depth;

            return (riverStart, riverEnd);
        }
    }
}
