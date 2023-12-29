using UnityEngine;

namespace UnityUtilities
{
    /// <summary>
    /// Frame rate independent lerp, equivalent to an exponential curve.
    /// For more info, see: https://marcospereira.me/2023/11/15/frame-rate-independent-lerp/#example-2-iterative-lerp
    /// </summary>
    public static class FILerp
    {
        /// <summary>
        /// Frame rate independent lerp, equivalent to an exponential curve.
        /// </summary>
        /// <param name="a">Starting value.</param>
        /// <param name="b">Value to move towards.</param>
        /// <param name="deltaTime">Usually Time.deltaTime.</param>
        /// <param name="d">Time until half of original distance remains.</param>
        /// <returns>A value between a and b.</returns>
        public static float Get(float a, float b, float deltaTime, float d)
        {
            return Mathf.Lerp(a, b, 1f - Mathf.Pow(0.5f, deltaTime / d));
        }

        /// <summary>
        /// Frame rate independent lerp, equivalent to an exponential curve.
        /// </summary>
        /// <param name="a">Starting value.</param>
        /// <param name="b">Value to move towards.</param>
        /// <param name="deltaTime">Usually Time.deltaTime.</param>
        /// <param name="d">Time until half of original distance remains.</param>
        /// <returns>A value between a and b.</returns>
        public static Vector2 Get(Vector2 a, Vector2 b, float deltaTime, float d)
        {
            return Vector2.Lerp(a, b, 1f - Mathf.Pow(0.5f, deltaTime / d));
        }

        /// <summary>
        /// Frame rate independent lerp, equivalent to an exponential curve.
        /// </summary>
        /// <param name="a">Starting value.</param>
        /// <param name="b">Value to move towards.</param>
        /// <param name="deltaTime">Usually Time.deltaTime.</param>
        /// <param name="d">Time until half of original distance remains.</param>
        /// <returns>A value between a and b.</returns>
        public static Vector3 Get(Vector3 a, Vector3 b, float deltaTime, float d)
        {
            return Vector3.Lerp(a, b, 1f - Mathf.Pow(0.5f, deltaTime / d));
        }
    }
}
