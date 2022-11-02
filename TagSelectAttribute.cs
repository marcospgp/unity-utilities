using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using UnityEditor;
using UnityEngine;

// Allow selecting a tag through the inspector by declaring a string field with
// [SerializeField] and [TagSelect] attributes.
// Based on http://www.brechtos.com/tagselectorattribute/

namespace MarcosPereira.UnityUtilities {
    // This class must be included in game builds or the compiler will throw an
    // error.
    public class TagSelectAttribute : PropertyAttribute {
        public bool useDefaultTagFieldDrawer = false;
    }

#if UNITY_EDITOR

    // This class must only be included in the editor, or the compiler will throw an
    // error.
    [CustomPropertyDrawer(typeof(TagSelectAttribute))]
    [SuppressMessage("", "SA1402:FileMayOnlyContainASingleType")]
    public class TagSelectPropertyDrawer : PropertyDrawer {
        public override void OnGUI(
            Rect position,
            SerializedProperty property,
            GUIContent label
        ) {
            if (property.propertyType == SerializedPropertyType.String) {
                _ = EditorGUI.BeginProperty(position, label, property);

                var attrib = this.attribute as TagSelectAttribute;

                if (attrib.useDefaultTagFieldDrawer) {
                    property.stringValue =
                        EditorGUI.TagField(position, label, property.stringValue);
                } else {
                    //generate the taglist + custom tags
                    var tagList = new List<string>() { "Untagged" };
                    tagList.AddRange(
                        UnityEditorInternal.InternalEditorUtility.tags
                    );
                    string propertyString = property.stringValue;
                    int index = -1;
                    if (propertyString.Length == 0) {
                        // The tag is empty
                        index = 0; // First index is Untagged
                    } else {
                        // Check if there is an entry that matches the entry and get
                        // the index.
                        // Skip index 0 as that is a special custom case.
                        for (int i = 1; i < tagList.Count; i++) {
                            if (tagList[i] == propertyString) {
                                index = i;
                                break;
                            }
                        }
                    }

                    // Draw the popup box with the current selected index
                    index = EditorGUI
                        .Popup(position, label.text, index, tagList.ToArray());

                    // Adjust the actual string value of the property based on the selection
                    if (index == 0) {
                        property.stringValue = string.Empty;
                    } else if (index >= 1) {
                        property.stringValue = tagList[index];
                    } else {
                        property.stringValue = string.Empty;
                    }
                }

                EditorGUI.EndProperty();
            } else {
                _ = EditorGUI.PropertyField(position, property, label);
            }
        }
    }

#endif
}
