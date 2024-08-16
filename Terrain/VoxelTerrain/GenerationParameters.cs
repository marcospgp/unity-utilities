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

        [Tooltip("Make mountain filter add height to terrain instead of scaling mountain height.")]
        public bool visualizeMountainFilter = false;

        public ExtendedPerlinNoise overhangFilter;

        [Tooltip("Make overhang filter add height to terrain instead of scaling mountain height.")]
        public bool visualizeOverhangFilter = false;

        public ExtendedPerlinNoise hills;
        public float hillInlandExponent = 1f;

        public ExtendedPerlinNoise rivers;
        public float riverMaxDepth;

        public ExtendedPerlinNoise riverFloor;

        [Tooltip("Determines where surface can turn into sand.")]
        public ExtendedPerlinNoise beaches;

        [Tooltip("Maximum height at which surface can be sand.")]
        public float maxSandHeight;
    }
}
