using System.Collections.Generic;
using UnityEngine;

namespace MarcosPereira.UnityUtilities {
    // Inherit from ScriptableObject for proper serialization.
    public class FrequencyList : ScriptableObject {
        public List<Item> items;

        [System.Serializable]
        public class Item {
            public Object obj;
            public float frequency;
        }
    }
}
