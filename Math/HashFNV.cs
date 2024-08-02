using System;
using System.Text;
using UnityEngine;

/// <summary>
/// An implementation of the Fowler–Noll–Vo 1a
///
/// Note that this hash has weak avalanche properties, and is thus not ideal for
/// hashing consecutive values such as coordinates.
///
/// Consider using xxHash from System.IO.Hashing for that purpose instead.
///
/// https://en.wikipedia.org/wiki/Fowler%E2%80%93Noll%E2%80%93Vo_hash_function#FNV-1a_hash
/// http://www.isthe.com/chongo/tech/comp/fnv/index.html
/// </summary>
namespace UnityUtilities
{
    public static class HashFNV
    {
        /// <summary>
        /// 32-bit prime number according to spec.
        /// </summary>
        private const uint PRIME = 16777619;

        /// <summary>
        /// 32-bit starting hash value according to spec.
        /// </summary>
        private const uint OFFSET_BASIS = 2166136261;

        public static Texture2D DebugTextureInt(int width = 512, string seed = "green bananas")
        {
            var texture = new Texture2D(width, width, TextureFormat.RGBA32, mipChain: false);

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < width; y++)
                {
                    float random01 = Get01(x - (width / 2), y - (width / 2), seed);

                    texture.SetPixel(x, y, Color.Lerp(Color.black, Color.white, random01));
                }
            }

            texture.Apply();

            return texture;
        }

        public static Texture2D DebugTextureFloat(int width = 512, string seed = "green bananas")
        {
            var texture = new Texture2D(width, width, TextureFormat.RGBA32, mipChain: false);

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < width; y++)
                {
                    float random01 = Get01(
                        (float)(x - (width / 2)),
                        (float)(y - (width / 2)),
                        seed
                    );

                    texture.SetPixel(x, y, Color.Lerp(Color.black, Color.white, random01));
                }
            }

            texture.Apply();

            return texture;
        }

        /// <summary>
        /// Get a random value in [0, 1] (both 0 and 1 are included).
        /// </summary>
        public static float Get01(int x, int y, string seed)
        {
            uint value = Coordinates(x, y, seed);

            return (float)value / (float)uint.MaxValue;
        }

        /// <summary>
        /// Get a random value in [0, 1] (both 0 and 1 are included).
        /// </summary>
        public static float Get01(float x, float y, string seed)
        {
            uint value = Coordinates(x, y, seed);

            return (float)value / (float)uint.MaxValue;
        }

        public static Vector2 GetDirection(Vector2 location, string seed)
        {
            uint hash = Coordinates(location.x, location.y, seed);

            // 0 corresponds to 0 degrees, max value corresponds to 2π - ϵ
            float hash01 = (float)hash / (float)uint.MaxValue;
            float angle = hash01 * ((2f * Mathf.PI) - Mathf.Epsilon);

            // This vector is already normalized
            return new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
        }

        public static uint Coordinates(int x, int y, string seed)
        {
            byte[] xb = BitConverter.GetBytes(x);
            byte[] yb = BitConverter.GetBytes(y);

            return Coordinates(xb, yb, seed);
        }

        public static uint Coordinates(float x, float y, string seed)
        {
            byte[] xb = BitConverter.GetBytes(x);
            byte[] yb = BitConverter.GetBytes(y);

            return Coordinates(xb, yb, seed);
        }

        public static uint Get(int x, string seed) => Get(BitConverter.GetBytes(x), seed);

        public static uint Get(float x, string seed) => Get(BitConverter.GetBytes(x), seed);

        private static uint Coordinates(byte[] x, byte[] y, string seed)
        {
            byte[] bytes = new byte[8];

            Array.Copy(x, bytes, x.Length);
            Array.Copy(y, 0, bytes, 4, y.Length);

            uint offsetBasis = GetOffsetBasis(seed);

            // Hashing twice avoids patterns when hashing coordinates (as seen
            // in debug texture).
            uint hash1 = FNV1a(bytes, offsetBasis);
            return FNV1a(BitConverter.GetBytes(hash1), offsetBasis);
        }

        private static uint FNV1a(byte[] bytes, uint offsetBasis)
        {
            uint hash = offsetBasis;

            for (int i = 0; i < bytes.Length; i++)
            {
                // Default context should be unchecked for overflow, but we make
                // sure.
                unchecked
                {
                    hash ^= (uint)bytes[i];
                    hash *= PRIME;
                }
            }

            return hash;
        }

        private static uint Get(byte[] bytes, string seed) => FNV1a(bytes, GetOffsetBasis(seed));

        private static uint GetOffsetBasis(string seed)
        {
            // Reasoning for this seeding method:
            // "The switch from FNV-0 to FNV-1 was purely to change the
            // offset_basis to a non-zero value. The selection of that non-zero
            // value is arbitrary."
            // Source: http://www.isthe.com/chongo/tech/comp/fnv/index.html#FNV-param

            byte[] bytes = Encoding.Unicode.GetBytes(seed);

            // Use nonzero value as seed to avoid issues with empty seed
            // strings.
            return FNV1a(bytes, OFFSET_BASIS);
        }
    }
}
