using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace MarcosPereira.UnityUtilities {
    public class FrequencyListElement : ListView {
        /// <param name="p">
        /// A SerializedProperty representing the FrequencyList.
        /// </param>
        public FrequencyListElement(SerializedProperty p) {
            this.fixedItemHeight = 16f;
            this.makeItem = () => new Item();
            // this.bindItem = (element, i) => {
            //     this.itemsSource
            // };

            this.headerTitle = p.displayName;
            this.horizontalScrollingEnabled = false;
            this.reorderable = true;
            this.reorderMode = ListViewReorderMode.Animated;
            this.showAddRemoveFooter = true;
            this.showAlternatingRowBackgrounds = AlternatingRowBackground.ContentOnly;
            this.showBorder = true;
            this.showBoundCollectionSize = true;
            this.showFoldoutHeader = true;

            this.BindProperty(p);
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
                this.slider.bindingPath = nameof(FrequencyList.Item.obj);

                this.slider.labelElement.style.display = DisplayStyle.None;
                this.slider.style.width = new Length(48f, LengthUnit.Percent);
                this.slider.style.alignSelf = Align.Center;
                this.slider.showInputField = true;
                this.slider.bindingPath = nameof(FrequencyList.Item.frequency);
            }
        }
    }
}
