using UnityEngine;

namespace UnityUtilities
{
    public readonly struct Vector3Double
    {
        public readonly double x;
        public readonly double y;
        public readonly double z;

        public Vector3Double(double x, double y, double z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }

        public static Vector3Double Zero => new Vector3Double(0, 0, 0);

        public static explicit operator Vector3Double(Vector3Int a) =>
            new Vector3Double(a.x, a.y, a.z);

        public static explicit operator Vector3(Vector3Double a) =>
            new Vector3((float)a.x, (float)a.y, (float)a.z);

        public static Vector3Double operator -(Vector3Double a, Vector3Double b) =>
            new Vector3Double(a.x - b.x, a.y - b.y, a.z - b.z);

        public static Vector3Double operator +(Vector3Double a, Vector3Double b) =>
            new Vector3Double(a.x + b.x, a.y + b.y, a.z + b.z);

        public static Vector3Double operator /(Vector3Double a, int b) =>
            new Vector3Double(a.x / b, a.y / b, a.z / b);

        public static Vector3Double operator *(Vector3Double a, int b) =>
            new Vector3Double(a.x * b, a.y * b, a.z * b);

        public static Vector3Double operator *(Vector3Double a, float b) =>
            new Vector3Double(a.x * b, a.y * b, a.z * b);

        public static Vector3Double Lerp(Vector3Double a, Vector3Double b, float t) =>
            a + ((b - a) * t);
    }
}
