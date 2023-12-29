using UnityEngine;

namespace UnityUtilities
{
    public static class SmootherStep
    {
        // Use the quintic smoothstep curve instead of the cubic one.
        // The quintic has 1st and 2nd degree derivatives of 0 at coordinates 0
        // and 1, while the cubic only has a 1st degree derivative of 0 at those
        // points.
        // https://en.wikipedia.org/wiki/Smoothstep#Variations
        //
        // "the 2nd order derivative is the normal's 1st order derivative"
        // https://stackoverflow.com/a/38441883/2037431
        public static float Get(float x)
        {
            if (x <= 0f)
            {
                return 0f;
            }
            else if (x >= 1f)
            {
                return 1f;
            }
            else
            {
                return (6 * Mathf.Pow(x, 5)) - (15 * Mathf.Pow(x, 4)) + (10 * Mathf.Pow(x, 3));
            }
        }
    }
}
