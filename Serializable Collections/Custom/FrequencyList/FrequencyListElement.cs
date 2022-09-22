using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace MarcosPereira.UnityUtilities {
    public class FrequencyListElement : ListView {
        /// <param name="p">
        /// A SerializedProperty representing the FrequencyList.
        /// </param>
        public FrequencyListElement(SerializedProperty p) {
            this.fixedItemHeight = 16f;
            this.makeItem = () => new Label("test item");
            this.bindItem = (element, i) => {
            };

            this.headerTitle = p.displayName;
            this.horizontalScrollingEnabled = false;
            this.reorderable = true;
            this.reorderMode = ListViewReorderMode.Animated;
            this.showAddRemoveFooter = true;
            this.showAlternatingRowBackgrounds = AlternatingRowBackground.ContentOnly;
            this.showBorder = true;
            this.showBoundCollectionSize = true;
            this.showFoldoutHeader = true;

            // this.bindingPath = "items";
            // this.Bind(p.serializedObject);

            this.BindProperty(new SerializedObject(p.objectReferenceValue).FindProperty("items"));

            this.itemsSource = new List<float>() { 1f, 2f, 3f };

            UnityEngine.Debug.Log(this.itemsSource);
        }

        private class Item : VisualElement {
            public Slider slider;
            public ObjectField objectField;

            public Item() {
                this.style.flexDirection = FlexDirection.Row;
                this.style.justifyContent = Justify.SpaceAround;

                this.objectField = new ObjectField();
                this.slider = new Slider(0f, 1f);

                this.Add(this.objectField);
                this.Add(this.slider);

                this.objectField.labelElement.style.display = DisplayStyle.None;
                this.objectField.style.alignItems = Align.Center;
                this.objectField.style.width = new Length(48f, LengthUnit.Percent);
                this.slider.bindingPath = "Item1";

                this.slider.labelElement.style.display = DisplayStyle.None;
                this.slider.style.width = new Length(48f, LengthUnit.Percent);
                this.slider.style.alignSelf = Align.Center;
                this.slider.showInputField = true;
                this.slider.bindingPath = "Item2";
            }
        }
    }
}
