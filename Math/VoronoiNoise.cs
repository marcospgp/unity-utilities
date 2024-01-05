using UnityEngine;
using System.IO.Hashing;
using System;

namespace UnityUtilities
{
    public static class VoronoiNoise
    {
        public static Texture2D GetTexture(int width)
        {
            var tex = new Texture2D(width, width, TextureFormat.RGBA32, mipChain: false);

            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < width; j++)
                {
                    tex.SetPixel(
                        i,
                        j,
                        Color.Lerp(Color.black, Color.white, VoronoiNoise.Get(i, j, 50f, 1234L))
                    );
                }
            }

            tex.Apply();

            return tex;
        }

        public static float Get(float x, float y, float scale, long seed)
        {
            float cellX = Mathf.Floor(x / scale) * scale;
            float cellY = Mathf.Floor(y / scale) * scale;

            // We compare square distances to save on expensive sqrt operation.
            float minSqrDistance = float.MaxValue;

            // Check neighboring cells
            for (int i = -1; i <= 1; i++)
            {
                for (int j = -1; j <= 1; j++)
                {
                    float subCellX = cellX + (scale * i);
                    float subCellY = cellY + (scale * j);

                    Vector2 point = GetCellPoint(subCellX, subCellY, scale, seed);

                    float sqrDistance = Vector2.SqrMagnitude(new Vector2(x, y) - point);

                    if (sqrDistance < minSqrDistance)
                    {
                        minSqrDistance = sqrDistance;
                    }
                }
            }

            float maxSqrDistance = scale * scale * 2;

            return MathF.Sqrt(minSqrDistance / maxSqrDistance);
        }

        private static Vector2 GetCellPoint(float cellX, float cellY, float scale, long seed)
        {
            byte[] bytes = new byte[8];

            Array.Copy(BitConverter.GetBytes(cellX), bytes, 4);
            Array.Copy(BitConverter.GetBytes(cellY), 0, bytes, 4, 4);

            byte[] hash = XxHash3.Hash(bytes, seed);

            byte[] a = new byte[4];
            byte[] b = new byte[4];

            Array.Copy(hash, 0, a, 0, 4);
            Array.Copy(hash, 4, b, 0, 4);

            float xOffset01 = (float)BitConverter.ToUInt32(a) / (float)uint.MaxValue;
            float yOffset01 = (float)BitConverter.ToUInt32(b) / (float)uint.MaxValue;

            return new Vector2(cellX + (xOffset01 * scale), cellY + (yOffset01 * scale));
        }
    }
}
