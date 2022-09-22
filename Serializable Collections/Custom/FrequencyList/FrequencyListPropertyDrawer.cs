using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace MarcosPereira.UnityUtilities {
    [CustomPropertyDrawer(typeof(FrequencyList))]
    public class FrequencyListPropertyDrawer : PropertyDrawer {
        public override VisualElement CreatePropertyGUI(SerializedProperty property) {
            // var list = new ListView(
            //     new System.Collections.Generic.List<string>() { "one", "two", "three"},
            //     20,
            //     () => new Label("test item"),
            //     (el, i) => {}
            // );

            // list.headerTitle = "Test yo";
            // list.horizontalScrollingEnabled = false;
            // list.reorderable = true;
            // list.reorderMode = ListViewReorderMode.Animated;
            // list.showAddRemoveFooter = true;
            // list.showAlternatingRowBackgrounds = AlternatingRowBackground.ContentOnly;
            // list.showBorder = true;
            // list.showBoundCollectionSize = true;
            // list.showFoldoutHeader = true;

            // return list;

            var x = ScriptableObject.CreateInstance<FrequencyList>();

            var y = new SerializedObject(x);

            return new FrequencyListElement(property);
        }
    }
}
