using UnityEngine;

namespace UnityUtilities
{
    public static class VoronoiNoise
    {
        public static float Get(float x, float y, float scale)
        {
            float gridX = Mathf.Floor(x / scale) * scale;
            float gridY = Mathf.Floor(y / scale) * scale;
        }
    }
}
