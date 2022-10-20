using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

// Usage is [ReadOnlyField]. Not [ReadOnly] due to conflict with
// `Unity.Collections.ReadOnlyAttribute`.

namespace MarcosPereira.UnityUtilities {
    public class ReadOnlyFieldAttribute : PropertyAttribute {
    }

#if UNITY_EDITOR
    [CustomPropertyDrawer(typeof(ReadOnlyFieldAttribute))]
    public class ReadOnlyFieldPropertyDrawer : PropertyDrawer {
        public override float GetPropertyHeight(
            SerializedProperty property,
            GUIContent label
        ) =>
            EditorGUI.GetPropertyHeight(property, label, true);

        public override void OnGUI(
            Rect position,
            SerializedProperty property,
            GUIContent label
        ) {
            bool guiEnabled = GUI.enabled;
            GUI.enabled = false;
            _ = EditorGUI.PropertyField(position, property, label, true);
            GUI.enabled = guiEnabled;
        }
    }
#endif

}
