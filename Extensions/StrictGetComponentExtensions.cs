using UnityEngine;

namespace UnityUtilities
{
    public static class StrictGetComponentExtensions
    {
        // Strict versions of GetComponent*() methods that error when the component
        // is not found.

        public static T GetComponentStrict<T>(this GameObject m)
            where T : Component
        {
            if (!m.TryGetComponent(out T component))
            {
                throw new System.Exception($"Missing {typeof(T).Name} component.");
            }

            return component;
        }

        public static T GetComponentStrict<T>(this Component m)
            where T : Component
        {
            if (!m.TryGetComponent(out T component))
            {
                throw new System.Exception($"Missing {typeof(T).Name} component.");
            }

            return component;
        }

        public static T GetComponentInChildrenStrict<T>(this GameObject m)
            where T : Component
        {
            T c = m.GetComponentInChildren<T>();

            if (c == null)
            {
                throw new System.Exception($"Missing {typeof(T).Name} component.");
            }

            return c;
        }

        public static T GetComponentInChildrenStrict<T>(this Component m)
            where T : Component
        {
            T c = m.GetComponentInChildren<T>();

            if (c == null)
            {
                throw new System.Exception($"Missing {typeof(T).Name} component.");
            }

            return c;
        }

        public static T GetComponentInParentStrict<T>(this GameObject m)
            where T : Component
        {
            T c = m.GetComponentInParent<T>();

            if (c == null)
            {
                throw new System.Exception($"Missing {typeof(T).Name} component.");
            }

            return c;
        }

        public static T GetComponentInParentStrict<T>(this Component m)
            where T : Component
        {
            T c = m.GetComponentInParent<T>();

            if (c == null)
            {
                throw new System.Exception($"Missing {typeof(T).Name} component.");
            }

            return c;
        }
    }
}
