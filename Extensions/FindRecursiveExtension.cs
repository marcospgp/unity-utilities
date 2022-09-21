using UnityEngine;

namespace MarcosPereira.UnityUtilities {
    public static class FindRecursiveExtension {
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
