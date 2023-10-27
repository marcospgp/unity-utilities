using UnityEngine;

namespace UnityUtilities
{
    public static class Int2TupleExtensions
    {
        public static float Norm(this (int x, int z) t) =>
            Mathf.Sqrt(Mathf.Pow(t.x, 2f) + Mathf.Pow(t.z, 2f));

        public static (int, int) Add(this (int a, int b) i, (int a, int b) j) =>
            (i.a + j.a, i.b + j.b);
    }
}
