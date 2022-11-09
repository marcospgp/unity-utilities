using UnityEngine;

namespace MarcosPereira.UnityUtilities {
    public static class LayerMaskExtensions {
        public static bool HasLayer(this LayerMask layerMask, int layerIndex) =>
            ((1 << layerIndex) & layerMask) != 0;
    }
}
