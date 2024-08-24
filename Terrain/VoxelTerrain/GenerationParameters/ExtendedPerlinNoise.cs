using System;
using UnityEngine;

namespace UnityUtilities.Terrain
{
    [Serializable]
    public class ExtendedPerlinNoise
    {
        public bool enabled = true;

        [Space]
        public string seed = string.Empty;
        public float baseFrequency = 0.003f;
        public int octaves = 3;
        public float lacunarity = 2f;
        public float persistence = 0.5f;

        [Space]
        public bool applyAbsolute = false;

        [Space]
        [Tooltip("Clamp values to a given range and map back to [0, 1].")]
        public bool applyStep = false;
        public float stepLow = 0f;
        public float stepHigh = 1f;

        [Space]
        public bool applyExponent = false;
        public float exponent = 1f;

        [Space]
        public bool applySigmoid = false;
        public float sigmoidSlope = 10;

        [Space]
        public bool applyMagnitude = false;
        public float magnitude = 100f;

        [Space]
        [Tooltip("Increase result by a given constant when it is higher than a given threshold.")]
        public bool applyBias = false;
        public float bias = 0f;
        public float biasThreshold = 0.01f;

        public static float Step(float x, float low, float high)
        {
            x = (x - low) / (high - low);
            x = MathF.Max(x, 0f);
            x = MathF.Min(x, 1f);

            return x;
        }

        public float Get(float x, float z, string baseSeed)
        {
            if (!this.enabled)
            {
                return 0;
            }

            float noise = PerlinNoise.Get(
                x,
                z,
                baseSeed + this.seed,
                this.baseFrequency,
                this.octaves,
                this.lacunarity,
                this.persistence
            );

            // Applying absolute function creates a "lines" effect in the noise,
            // useful for things like rivers.
            if (this.applyAbsolute)
            {
                noise = 1f - (MathF.Abs(noise - 0.5f) * 2f);
            }

            if (this.applyStep)
            {
                noise = Step(noise, this.stepLow, this.stepHigh);
            }

            if (this.applyExponent)
            {
                noise = MathF.Pow(noise, this.exponent);
            }

            if (this.applySigmoid)
            {
                // Remap into [-1, 1] for sigmoid.
                // No need to remap output as it is already in [0, 1].
                noise = (noise * 2f) - 1f;
                noise = Sigmoid(noise, this.sigmoidSlope);
            }

            float result = noise;

            if (this.applyMagnitude)
            {
                result *= this.magnitude;
            }

            if (this.applyBias && result >= this.biasThreshold)
            {
                result += this.bias;
            }

            return result;
        }

        /// <summary>
        /// Get just the perlin noise value, in [0, 1].
        /// </summary>
        public float GetRaw(float x, float z, string baseSeed) =>
            PerlinNoise.Get(
                x,
                z,
                baseSeed + this.seed,
                this.baseFrequency,
                this.octaves,
                this.lacunarity,
                this.persistence
            );

        private static float Sigmoid(float x, float slope) => 1f / (1f + MathF.Exp(-slope * x));
    }
}
