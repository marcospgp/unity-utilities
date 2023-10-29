using UnityEngine;

namespace UnityUtilities
{
    public static class MathUtils
    {
        /// <summary>
        /// Frame rate independent lerp.
        /// For more info, see: https://marcospereira.me/2022/08/24/lerp-how-to-frame-rate-independent/#fnref:1
        /// </summary>
        public static Vector3 Lerp(Vector3 a, Vector3 b, float d)
        {
            return Vector3.Lerp(b, a, Mathf.Pow(1f - d, Time.deltaTime));
        }

        public static float Lerp(float a, float b, float d)
        {
            return Mathf.Lerp(b, a, Mathf.Pow(1f - d, Time.deltaTime));
        }
    }
}
