using System.Diagnostics.CodeAnalysis;
using UnityEditor;
using UnityEngine;

// Allow selecting a layer through the inspector using a [LayerSelect]
// attribute on  an int field.
// Based on http://answers.unity.com/comments/1777780/view.html

namespace UnityUtilities
{
    // This class must be included in game builds or the compiler will throw an
    // error.
    public class LayerSelectAttribute : PropertyAttribute { }

#if UNITY_EDITOR

    // This class must only be included in the editor, or the compiler will throw an
    // error.
    [CustomPropertyDrawer(typeof(LayerSelectAttribute))]
    [SuppressMessage("", "SA1402:FileMayOnlyContainASingleType", Justification = "Conciseness.")]
    public class LayerAttributeEditor : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            _ = EditorGUI.BeginProperty(position, label, property);
            property.intValue = EditorGUI.LayerField(position, label, property.intValue);
            EditorGUI.EndProperty();
        }
    }

#endif
}
