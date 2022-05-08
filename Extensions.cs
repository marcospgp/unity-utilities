using UnityEngine;

namespace MarcosPereira.UnityUtilities {
    public static class Extensions {
        // Strict versions of GetComponent*() methods that error when the component
        // is not found.

        public static T GetComponentStrict<T>(
            this GameObject m
        ) where T : Component {
            if (!m.TryGetComponent(out T component)) {
                throw new System.Exception($"Missing {typeof(T).Name} component.");
            }

            return component;
        }

        public static T GetComponentStrict<T>(
            this Component m
        ) where T : Component {
            if (!m.TryGetComponent(out T component)) {
                throw new System.Exception($"Missing {typeof(T).Name} component.");
            }

            return component;
        }

        public static T GetComponentInChildrenStrict<T>(
            this GameObject m
        ) where T : Component {
            T c = m.GetComponentInChildren<T>();

            if (c == null) {
                throw new System.Exception($"Missing {typeof(T).Name} component.");
            }

            return c;
        }

        public static T GetComponentInChildrenStrict<T>(
            this Component m
        ) where T : Component {
            T c = m.GetComponentInChildren<T>();

            if (c == null) {
                throw new System.Exception($"Missing {typeof(T).Name} component.");
            }

            return c;
        }

        public static T GetComponentInParentStrict<T>(
            this GameObject m
        ) where T : Component {
            T c = m.GetComponentInParent<T>();

            if (c == null) {
                throw new System.Exception($"Missing {typeof(T).Name} component.");
            }

            return c;
        }

        public static T GetComponentInParentStrict<T>(
            this Component m
        ) where T : Component {
            T c = m.GetComponentInParent<T>();

            if (c == null) {
                throw new System.Exception($"Missing {typeof(T).Name} component.");
            }

            return c;
        }

        /// <summary>
        /// Recursive Transform.Find(). Like Transform.Find(), but searches
        /// recursively over the transform's entire hierarchy instead of only direct
        /// children.
        /// </summary>
        /// <param name="t">The transform whose children will be checked.</param>
        /// <param name="name">The transform name to look for.</param>
        /// <returns>The transform if found, null otherwise.</returns>
        public static Transform FindRecursive(this Transform t, string name) {
            Transform[] children =
                t.GetComponentsInChildren<Transform>(includeInactive: true);

            foreach (Transform childTransform in children) {
                if (childTransform.name == name) {
                    return childTransform;
                }
            }

            return null;
        }
    }
}
