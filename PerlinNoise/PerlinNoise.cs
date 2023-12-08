using UnityEngine;

namespace UnityUtilities
{
    public static class PerlinNoise
    {
        private const string DEFAULT_SEED = "green bananas";
        private const float DEFAULT_FREQUENCY = 0.01f;
        private const int DEFAULT_OCTAVE_COUNT = 1;
        private const float DEFAULT_LACUNARITY = 2f;
        private const float DEFAULT_PERSISTENCE = 0.5f;

        /// <summary> Get Perlin Noise value at given position.</summary>
        /// <param name="x">X coordinate.</param>
        /// <param name="z">Z coordinate.</param>
        /// <param name="seed">
        ///     Seed used for the random numbers underlying the noise.
        /// </param>
        /// <param name="baseFrequency">
        ///     The frequency of the first octave.
        /// </param>
        /// <param name="numberOfOctaves">The number of octaves.</param>
        /// <param name="lacunarity">
        ///     Each subsequent octave n will have frequency
        ///     Mathf.Pow(lacunarity, n). A common value for this parameter is
        ///     2.
        /// </param>
        /// <param name="persistence">
        ///     Each subsequent octave will have amplitude
        ///     Mathf.Pow(persistence, n). A common value for this parameter is
        ///     0.5.
        /// </param>
        /// <returns>Perlin noise value in [0, 1].</returns>
        public static float Get(
            float x,
            float z,
            string seed = null,
            float baseFrequency = PerlinNoise.DEFAULT_FREQUENCY,
            int numberOfOctaves = PerlinNoise.DEFAULT_OCTAVE_COUNT,
            float lacunarity = PerlinNoise.DEFAULT_LACUNARITY,
            float persistence = PerlinNoise.DEFAULT_PERSISTENCE
        )
        {
            if (string.IsNullOrEmpty(seed))
            {
                seed = PerlinNoise.DEFAULT_SEED;
            }

            float noise = 0f;
            float frequency = baseFrequency;
            float amplitude = 1f;
            float totalAmplitude = 0f;

            for (int i = 0; i < numberOfOctaves; i++)
            {
                // Make the seed of each octave different to avoid overlap artifacts.
                string octaveSeed = $"{i}{seed}";

                noise += PerlinNoise.Raw(x * frequency, z * frequency, octaveSeed) * amplitude;

                totalAmplitude += amplitude;

                frequency *= lacunarity;
                amplitude *= persistence;
            }

            return noise / totalAmplitude;
        }

        public static Texture2D GetPreview(
            int width,
            string seed = null,
            float baseFrequency = PerlinNoise.DEFAULT_FREQUENCY,
            int numberOfOctaves = PerlinNoise.DEFAULT_OCTAVE_COUNT,
            float lacunarity = PerlinNoise.DEFAULT_LACUNARITY,
            float persistence = PerlinNoise.DEFAULT_PERSISTENCE
        )
        {
            if (string.IsNullOrEmpty(seed))
            {
                seed = PerlinNoise.DEFAULT_SEED;
            }

            var texture = new Texture2D(width, width, TextureFormat.RGBA32, false);

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < width; y++)
                {
                    float noise01 = PerlinNoise.Get(
                        (float)x,
                        (float)y,
                        seed,
                        baseFrequency,
                        numberOfOctaves,
                        lacunarity,
                        persistence
                    );

                    texture.SetPixel(x, y, Color.Lerp(Color.black, Color.white, noise01));
                }
            }

            texture.Apply();

            return texture;
        }

        // Returns a value in range [0, 1].
        private static float Raw(float x, float z, string seed)
        {
            int cornerX = Mathf.FloorToInt(x);
            int cornerZ = Mathf.FloorToInt(z);

            var corners = new Vector2Int[]
            {
                new Vector2Int(cornerX, cornerZ),
                new Vector2Int(cornerX, cornerZ + 1),
                new Vector2Int(cornerX + 1, cornerZ + 1),
                new Vector2Int(cornerX + 1, cornerZ)
            };

            var gradients = new Vector2[4];

            for (int i = 0; i < gradients.Length; i++)
            {
                gradients[i] = Hash.GetDirection(corners[i], seed);
            }

            float u = x - cornerX;
            float v = z - cornerZ;

            var point = new Vector2(u, v);

            var offsets = new Vector2[]
            {
                point,
                point - new Vector2(0f, 1f),
                point - new Vector2(1f, 1f),
                point - new Vector2(1f, 0f)
            };

            float[] influences = new float[4];

            for (int i = 0; i < influences.Length; i++)
            {
                if (gradients[i] == Vector2.zero)
                {
                    // Density killed this gradient, influence is minimum
                    // possible (-sqrt(N/4), which happens at center of grid
                    // square when gradient is pointing opposite to offset).
                    // influences[i] = -1f * Mathf.Sqrt(0.5f);
                    gradients[i] = (-1f * offsets[i]).normalized;
                    // influences[i] = -1f * offsets[i].sqrMagnitude;
                }

                influences[i] = Vector2.Dot(gradients[i], offsets[i]);
            }

            // Interpolate the sample coordinates instead of the final values,
            // which avoids some problem I don't remember because I'm writing
            // this months later.
            float u2 = SmootherStep.Get(u);
            float v2 = SmootherStep.Get(v);

            float avg1 = Mathf.Lerp(influences[0], influences[3], u2);
            float avg2 = Mathf.Lerp(influences[1], influences[2], u2);
            float avg = Mathf.Lerp(avg1, avg2, v2);

            // Normalize output range to [0, 1].
            // Perlin noise output range is [-sqrt(N/4), sqrt(N/4)], where N is
            // the number of dimensions. These maximum values are reached in the
            // center of a grid square, when all corner vectors are pointing
            // away from or towards it (minimum and maximum values,
            // respectively).
            return (avg + Mathf.Sqrt(0.5f)) / Mathf.Sqrt(2f);
        }
    }
}
