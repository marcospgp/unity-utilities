using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MarcosPereira.UnityUtilities {
    // This class is serializable but not a ScriptableObject, as Unity does not
    // support generic ScriptableObjects.
    // Keep in mind that custom classes have poorer serialization support.
    // For example, shared references may become independent copies.
    [Serializable]
    public class SerializableSortedSet<T> : ISerializationCallbackReceiver, IEnumerable<T> {
        private readonly SortedSet<T> sortedSet = new SortedSet<T>();

        [SerializeField]
        private List<T> serializableItems;

        public SerializableSortedSet() {
            this.sortedSet = new SortedSet<T>();
        }

        public SerializableSortedSet(IComparer<T> comparer) {
            this.sortedSet = new SortedSet<T>(comparer);
        }

        public int Count => this.sortedSet.Count;
        public T Min => this.sortedSet.Min;
        public T Max => this.sortedSet.Max;

        void ISerializationCallbackReceiver.OnBeforeSerialize() {
            this.serializableItems = new List<T>(this.sortedSet);
        }

        void ISerializationCallbackReceiver.OnAfterDeserialize() {
            this.sortedSet.Clear();

            foreach (T item in this.serializableItems) {
                _ = this.sortedSet.Add(item);
            }
        }

        // Allow iterating over this class
        IEnumerator<T> IEnumerable<T>.GetEnumerator() =>
            this.sortedSet.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() =>
            this.sortedSet.GetEnumerator();

        public bool Add(T item) => this.sortedSet.Add(item);

        public bool Remove(T item) => this.sortedSet.Remove(item);

        public bool Contains(T item) => this.sortedSet.Contains(item);
    }
}
