using System.Collections.Generic;
using UnityEngine;

namespace MarcosPereira.UnityUtilities {
    // Inherit from ScriptableObject for proper serialization.
    public class FrequencyList : ScriptableObject {
        public string yo = "yo ma";
        public List<float> items;
    }
}
