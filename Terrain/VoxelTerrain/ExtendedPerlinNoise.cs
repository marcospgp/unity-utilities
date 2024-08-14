using System;

namespace UnityUtilities.Terrain
{
    [Serializable]
    public class ExtendedPerlinNoise
    {
        public bool enabled = true;

        public string seed = string.Empty;
        public float baseFrequency = 0.003f;
        public int octaves = 3;
        public float lacunarity = 2f;
        public float persistence = 0.5f;

        public bool applyStep = false;
        public float stepLow = 0f;
        public float stepHigh = 1f;

        public bool applyExponent = false;
        public float exponent = 1f;

        public bool applySigmoid = false;
        public float sigmoidSlope = 10;
        public float magnitude = 100f;

        public float Get(float x, float z)
        {
            if (!this.enabled)
            {
                return 0;
            }

            float noise = PerlinNoise.Get(
                x,
                z,
                this.seed,
                this.baseFrequency,
                this.octaves,
                this.lacunarity,
                this.persistence
            );

            if (this.applyStep)
            {
                noise = (noise - this.stepLow) / (this.stepHigh - this.stepLow);
                noise = MathF.Max(noise, 0f);
                noise = MathF.Min(noise, 1f);
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

            return noise * this.magnitude;
        }

        private static float Sigmoid(float x, float slope) => 1f / (1f + MathF.Exp(-slope * x));
    }
}
