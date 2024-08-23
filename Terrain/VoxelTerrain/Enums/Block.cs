using System;
using System.Collections.Generic;

namespace UnityUtilities.Terrain
{
    /// <summary>
    /// Using byte to save space as we don't expect > 256 blocks for now.
    /// </summary>
    public enum Block : byte
    {
        Air,
        Dirt,
        Grass,
        Sand,
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage(
        null,
        "SA1649:File name should match first type name",
        Justification = "Enum extensions."
    )]
    public static class BlockExtensions
    {
        /// <summary>
        /// Block to texture map in one of these formats:
        ///
        /// 1. [all sides] (same texture for all faces)
        /// 2. [top, bottom, sides]
        /// </summary>
        private static readonly Dictionary<Block, BlockMaterial[]> materialMap =
            new()
            {
                { Block.Dirt, new[] { BlockMaterial.Dirt } },
                {
                    Block.Grass,
                    new[] { BlockMaterial.GrassTop, BlockMaterial.Dirt, BlockMaterial.GrassSide }
                },
                { Block.Sand, new[] { BlockMaterial.Sand } },
            };

        public static BlockMaterial GetMaterial(this Block block, Face face)
        {
            BlockMaterial[] map = materialMap[block];

            if (map.Length == 1)
            {
                return map[0];
            }

            if (map.Length == 3)
            {
                if (face == Face.YPlus)
                {
                    return map[0];
                }

                if (face == Face.YMinus)
                {
                    return map[1];
                }

                return map[2];
            }

            throw new Exception($"Unexpected texture map length of {map.Length}.");
        }
    }
}
