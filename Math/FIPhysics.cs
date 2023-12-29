using UnityEngine;

namespace UnityUtilities
{
    /// <summary>
    /// Frame rate independent physics.
    /// For more info, see: https://marcospereira.me/2023/11/15/frame-rate-independent-lerp
    /// </summary>
    public static class FIPhysics
    {
        /// <summary>
        /// Frame rate independent movement with constant acceleration.
        ///
        /// We use the solution to the differential equations `dv/dt = a` and
        /// `dx/dt = v`, as it is a flow (equivalent to frame rate independent).
        /// This solution is formulated as:
        ///
        /// `x(t) = (1/2 * a * (t^2)) + (v0 * t) + x0`
        /// </summary>
        public static Vector3 Move(
            Vector3 position,
            Vector3 velocity,
            Vector3 acceleration,
            float deltaTime
        ) => (0.5f * Mathf.Pow(deltaTime, 2) * acceleration) + (deltaTime * velocity) + position;
    }
}
