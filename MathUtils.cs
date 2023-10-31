using UnityEngine;

namespace UnityUtilities
{
    public static class MathV2
    {
        /// <summary>
        /// Frame rate independent lerp.
        /// For more info, see: https://marcospereira.me/2022/08/24/lerp-how-to-frame-rate-independent
        /// </summary>
        /// <param name="d">How much a approaches b per second.</param>
        public static Vector3 Lerp(Vector3 a, Vector3 b, float d)
        {
            return Vector3.Lerp(b, a, Mathf.Pow(1f - d, Time.deltaTime));
        }

        /// <summary>
        /// Frame rate independent lerp.
        /// For more info, see: https://marcospereira.me/2022/08/24/lerp-how-to-frame-rate-independent
        /// </summary>
        /// <param name="d">How much a approaches b per second.</param>
        public static Vector2 Lerp(Vector2 a, Vector2 b, float d)
        {
            return Vector2.Lerp(b, a, Mathf.Pow(1f - d, Time.deltaTime));
        }

        /// <summary>
        /// Frame rate independent lerp.
        /// For more info, see: https://marcospereira.me/2022/08/24/lerp-how-to-frame-rate-independent
        /// </summary>
        /// <param name="d">How much a approaches b per second.</param>
        public static float Lerp(float a, float b, float d)
        {
            return Mathf.Lerp(b, a, Mathf.Pow(1f - d, Time.deltaTime));
        }
    }
}
