using UnityEngine;

namespace UnityUtilities
{
    public static class GameObjectExtensions
    {
        /// <summary>
        /// Strict version of GetComponentStrict that errors when the component
        /// is not found.
        /// </summary>
        public static T GetComponentStrict<T>(this GameObject m)
            where T : Component
        {
            if (!m.TryGetComponent(out T component))
            {
                throw new System.Exception($"Missing {typeof(T).Name} component.");
            }

            return component;
        }

        /// <summary>
        /// Strict version of GetComponentStrict that errors when the component
        /// is not found.
        /// </summary>
        public static T GetComponentStrict<T>(this Component m)
            where T : Component
        {
            if (!m.TryGetComponent(out T component))
            {
                throw new System.Exception($"Missing {typeof(T).Name} component.");
            }

            return component;
        }

        /// <summary>
        /// Strict version of GetComponentInChildrenStrict that errors when the
        /// component is not found.
        /// </summary>
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

        /// <summary>
        /// Strict version of GetComponentInChildrenStrict that errors when the
        /// component is not found.
        /// </summary>
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

        /// <summary>
        /// Strict version of GetComponentInParentStrict that errors when the
        /// component is not found.
        /// </summary>
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

        /// <summary>
        /// Strict version of GetComponentInParentStrict that errors when the
        /// component is not found.
        /// </summary>
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
