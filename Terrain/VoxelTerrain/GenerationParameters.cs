using System;
using System.Reflection;
using UnityEngine;

namespace UnityUtilities.Terrain
{
    [Serializable]
    public class GenerationParameters
    {
        [Header("Base terrain")]
        public float baseFrequency = 0.008f;
        public int baseOctaves = 3;
        public float baseLacunarity = 2f;
        public float basePersistence = 0.5f;
        public float baseExponent = 0.8f;
        public float baseSigmoidSlope = 10f;

        [Header("Mountain")]
        public float mountainFrequency = 0.008f;
        public int mountainOctaves = 3;
        public float mountainLacunarity = 2f;
        public float mountainPersistence = 0.5f;
        public float mountainNoiseFloor = 0f;
        public float mountainNoiseCeiling = 1f;
        public float mountainExponent = 1f;
        public bool mountainSigmoid = false;
        public float mountainSigmoidSlope = 10f;
        public float mountainHeight = 64f;

        [Header("Mountain filter")]
        public bool mountainFilterEnabled = false;
        public float mountainFilterFrequency = 0.008f;
        public int mountainFilterOctaves = 3;
        public float mountainFilterLacunarity = 2f;
        public float mountainFilterPersistence = 0.5f;
        public float mountainFilterNoiseFloor = 0f;
        public float mountainFilterNoiseCeiling = 1f;
        public float mountainFilterExponent = 1f;
        public bool mountainFilterSigmoid = false;
        public float mountainFilterSigmoidSlope = 10f;

        [Header("Hills")]
        public bool hillsEnabled = false;
        public float hillFrequency = 0.008f;
        public int hillOctaves = 3;
        public float hillLacunarity = 2f;
        public float hillPersistence = 0.5f;
        public float hillNoiseFloor = 0f;
        public float hillNoiseCeiling = 1f;
        public float hillExponent = 1f;
        public bool hillSigmoid = false;
        public float hillSigmoidSlope = 10f;
        public float hillHeight = 32f;

        /// <summary>
        /// Allows reusing the same instance, to avoid generating garbage by
        /// allocating new instances repeatedly.
        /// </summary>
        public void CopyFrom(GenerationParameters other)
        {
            foreach (
                FieldInfo field in typeof(GenerationParameters).GetFields(
                    BindingFlags.Public | BindingFlags.Instance
                )
            )
            {
                field.SetValue(this, field.GetValue(other));
            }
        }

        public bool IsEqualTo(GenerationParameters other)
        {
            if (other == null)
            {
                return false;
            }

            foreach (
                FieldInfo field in typeof(GenerationParameters).GetFields(
                    BindingFlags.Public | BindingFlags.Instance
                )
            )
            {
                object value1 = field.GetValue(this);
                object value2 = field.GetValue(other);

                if (!Equals(value1, value2))
                {
                    return false;
                }
            }

            return true;
        }
    }
}
