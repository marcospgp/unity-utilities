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

        [Header("Base terrain")]
        public ExtendedPerlinNoise baseTerrain;

        [Header("Mountain")]
        public ExtendedPerlinNoise mountains;
        public float mountainInlandExponent = 1f;

        [Header("Mountain filter")]
        public ExtendedPerlinNoise mountainFilter;

        [Header("Hills")]
        public ExtendedPerlinNoise hills;
        public float hillInlandExponent = 1f;

        [Header("Rivers")]
        public ExtendedPerlinNoise rivers;

        [Header("River noise")]
        public ExtendedPerlinNoise riverFloor;
    }
}
