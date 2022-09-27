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
    public class SerializableHashSet<T> : ISerializationCallbackReceiver, IEnumerable<T> {
        private readonly HashSet<T> hashSet = new HashSet<T>();

        [SerializeField]
        private List<T> serializableItems;

        public int Count => this.hashSet.Count;

        void ISerializationCallbackReceiver.OnBeforeSerialize() {
            this.serializableItems = new List<T>(this.hashSet);
        }

        void ISerializationCallbackReceiver.OnAfterDeserialize() {
            this.hashSet.Clear();

            foreach (T item in this.serializableItems) {
                _ = this.hashSet.Add(item);
            }
        }

        // Allow iterating over this class
        IEnumerator<T> IEnumerable<T>.GetEnumerator() =>
            this.hashSet.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() =>
            this.hashSet.GetEnumerator();

        public bool Add(T item) => this.hashSet.Add(item);

        public bool Remove(T item) => this.hashSet.Remove(item);

        public bool Contains(T item) => this.hashSet.Contains(item);
    }
}
