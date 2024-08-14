using System;
using System.Reflection;
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

        public ExtendedPerlinNoise hills;
        public float hillInlandExponent = 1f;

        public ExtendedPerlinNoise rivers;
        public float riverMaxDepth;

        public ExtendedPerlinNoise riverFloor;
    }
}
