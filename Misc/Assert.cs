using System;

namespace UnityUtilities
{
    /// <summary>
    /// Assertions that are not removed in production code.
    /// </summary>
    public static class Assert
    {
        public static void That(bool condition)
        {
            if (!condition)
            {
                throw new Exception("Assertion failed.");
            }
        }
    }
}
