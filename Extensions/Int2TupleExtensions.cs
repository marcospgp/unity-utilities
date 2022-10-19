using UnityEngine;

namespace MarcosPereira.UnityUtilities {
    public static class Int2TupleExtensions {
        public static float Norm(this (int x, int z) t) =>
            Mathf.Sqrt(Mathf.Pow(t.x, 2f) + Mathf.Pow(t.z, 2f));
    }
}
