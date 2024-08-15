using System;
using UnityEngine;

namespace UnityUtilities.Terrain
{
    [Serializable]
    public class GenerationParameters
    {
        [Tooltip("Ocean floor height.")]
        public float baseHeight = 64f;

        public ExtendedPerlinNoise baseTerrain;

        public ExtendedPerlinNoise mountains;
        public float mountainInlandExponent = 1f;

        public ExtendedPerlinNoise mountainFilter;

        /// <summary>
        /// Make mountain filter add height to terrain instead of scaling
        /// mountain height.
        /// </summary>
        public bool visualizeMountainFilter = false;

        public ExtendedPerlinNoise hills;
        public float hillInlandExponent = 1f;

        public ExtendedPerlinNoise rivers;
        public float riverMaxDepth;

        public ExtendedPerlinNoise riverFloor;
    }
}
