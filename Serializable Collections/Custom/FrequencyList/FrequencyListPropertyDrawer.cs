using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace MarcosPereira.UnityUtilities {
    [CustomPropertyDrawer(typeof(FrequencyList))]
    public class FrequencyListPropertyDrawer : PropertyDrawer {
        public override VisualElement CreatePropertyGUI(SerializedProperty property) {
            UnityEngine.Debug.Log("I'm alive!");
            return new FrequencyListElement(property);
        }
    }
}
