using System;
using UnityEngine;

namespace UnityUtilities.Terrain
{
    [Serializable]
    public class GenerationParameters
    {
        public string seed = string.Empty;

        [Tooltip("Ocean floor height.")]
        public float baseHeight = 64f;

        public float beachInlandThreshold;

        public ExtendedPerlinNoise baseTerrain;

        public ExtendedPerlinNoise hills;
        public float hillInlandExponent = 1f;

        public ExtendedPerlinNoise mountains;
        public float mountainInlandExponent = 1f;

        public ExtendedPerlinNoise mountainFilter;

        [Tooltip("Make mountain filter add height to terrain instead of scaling mountain height.")]
        public bool visualizeMountainFilter = false;

        public ExtendedPerlinNoise overhangFilter;

        [Tooltip("Make overhang filter add height to terrain instead of scaling mountain height.")]
        public bool visualizeOverhangFilter = false;

        public ExtendedPerlinNoise rivers;
        public float riverMaxDepth;

        public ExtendedPerlinNoise riverFloor;
    }
}
