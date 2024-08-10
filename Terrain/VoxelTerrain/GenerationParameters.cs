using System;
using System.Reflection;

namespace UnityUtilities.Terrain
{
    [Serializable]
    public class GenerationParameters
    {
        public float baseFrequency = 0.008f;
        public int baseOctaves = 3;
        public float baseLacunarity = 2f;
        public float basePersistence = 0.5f;
        public float baseExponent = 0.8f;
        public float baseSigmoidSlope = 10f;

        public float mountainFrequency = 0.008f;
        public int mountainOctaves = 3;
        public float mountainLacunarity = 2f;
        public float mountainPersistence = 0.5f;
        public float mountainNoiseFloor = 0.5f;
        public float mountainExponent = 1f;
        public bool mountainSigmoid = false;
        public float mountainSigmoidSlope = 10f;
        public float mountainHeight = 64f;
        public float mountainStep = 1f;

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
