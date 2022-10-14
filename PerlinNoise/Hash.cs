using System;
using System.Text;
using UnityEngine;

/// <summary>
/// An implementation of the Fowler–Noll–Vo 1a hash
/// https://en.wikipedia.org/wiki/Fowler%E2%80%93Noll%E2%80%93Vo_hash_function#FNV-1a_hash
/// http://www.isthe.com/chongo/tech/comp/fnv/index.html
/// </summary>
namespace MarcosPereira.UnityUtilities {
    public static class Hash {
        /// <summary>
        /// 32-bit prime number according to spec.
        /// </summary>
        private const uint PRIME = 16777619;

        /// <summary>
        /// 32-bit starting hash value according to spec.
        /// </summary>
        private const uint OFFSET_BASIS = 2166136261;

        public static Texture2D DebugTexture(string seed = "green bananas") {
            const int width = 256;

            var texture = new Texture2D(
                width,
                width,
                TextureFormat.RGBA32,
                false
            );

            for (int x = 0; x < width; x++) {
                for (int y = 0; y < width; y++) {
                    float random01 = Hash.Get01(x, y, seed);

                    texture.SetPixel(
                        x,
                        y,
                        Color.Lerp(
                            Color.black,
                            Color.white,
                            random01
                        )
                    );
                }
            }

            texture.Apply();

            return texture;
        }

        public static Vector2 GetDirection(Vector2 location, string seed) {
            uint hash = Hash.Get(location.x, location.y, seed);

            // 0 corresponds to 0 degrees, max value corresponds to 2π - ϵ
            float angle = (float) hash / (float) uint.MaxValue;
            angle *= (2f * Mathf.PI) - Mathf.Epsilon;

            // This vector is already normalized
            return new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
        }

        public static float Get01(int x, int y, string seed) {
            uint value = Hash.Get(x, y, seed);

            return Mathf.Clamp01((float) value / (float) uint.MaxValue);
        }

        /// <summary>Get a random value in [0, 1] (both 0 and 1 are included).</summary>
        public static float Get01(float x, float y, string seed) {
            uint value = Hash.Get(x, y, seed);

            return Mathf.Clamp01((float) value / (float) uint.MaxValue);
        }

        /// <summary>Get a random value in (0, 1] (0 is excluded).</summary>
        // 0f + epsilon == epsilon, but 1f + epsilon == 1f.
        public static float Get01Exclusive(float x, float y, string seed) =>
            Hash.Get01(x, y, seed) + Mathf.Epsilon;

        public static uint Get(int x, int y, string seed) {
            unchecked {
                byte[] xb = BitConverter.GetBytes(x * Hash.PRIME);
                byte[] yb = BitConverter.GetBytes(y * Hash.PRIME);

                return Hash.Get(xb, yb, seed);
            }
        }

        public static uint Get(float x, float y, string seed) {
            unchecked {
                byte[] xb = BitConverter.GetBytes(x * (float) Hash.PRIME);
                byte[] yb = BitConverter.GetBytes(y * (float) Hash.PRIME);

                return Hash.Get(xb, yb, seed);
            }
        }

        private static uint Get(byte[] x, byte[] y, string seed) {
            // Shuffle the bytes around to improve randomization.
            byte[] bytes = new byte[] {
                y[3],
                x[0],
                y[2],
                x[1],
                y[1],
                x[2],
                y[0],
                x[3]
            };

            return Hash.Get(bytes, seed);
        }

        private static uint Get(byte[] bytes, uint offsetBasis) {
            uint hash = offsetBasis;

            for (int i = 0; i < bytes.Length; i++) {
                // Default context should be unchecked for overflow,
                // but we make sure.
                unchecked {
                    hash ^= (uint) bytes[i];
                    hash *= Hash.PRIME;
                }
            }

            return hash;
        }

        private static uint Get(byte[] bytes, string seed) =>
            Hash.Get(bytes, Hash.GetOffsetBasis(seed));

        private static uint GetOffsetBasis(string seed) {
            // Reasoning for this seeding method:
            // "The switch from FNV-0 to FNV-1 was purely to change the
            // offset_basis to a non-zero value. The selection of that non-zero
            // value is arbitrary."
            // Source: http://www.isthe.com/chongo/tech/comp/fnv/index.html#FNV-param

            byte[] bytes = Encoding.Unicode.GetBytes(seed);

            return Hash.Get(bytes, Hash.OFFSET_BASIS);
        }
    }
}
