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
    public class SerializableDictionary<TKey, TValue> :
    ISerializationCallbackReceiver, IEnumerable<KeyValuePair<TKey, TValue>> {
        private readonly Dictionary<TKey, TValue> dictionary = new Dictionary<TKey, TValue>();

        [SerializeField]
        private List<TKey> serializableKeys;

        [SerializeField]
        private List<TValue> serializableValues;

        public int Count => this.dictionary.Count;

        public Dictionary<TKey, TValue>.KeyCollection Keys => this.dictionary.Keys;

        public Dictionary<TKey, TValue>.ValueCollection Values =>
            this.dictionary.Values;

        public TValue this[TKey key] {
            get => this.dictionary[key];
            set => this.dictionary[key] = value;
        }

        void ISerializationCallbackReceiver.OnBeforeSerialize() {
            this.serializableKeys = new List<TKey>(this.dictionary.Keys);
            this.serializableValues = new List<TValue>(this.dictionary.Values);
        }

        void ISerializationCallbackReceiver.OnAfterDeserialize() {
            this.dictionary.Clear();

            for (int i = 0; i < this.serializableKeys.Count; i++) {
                this.dictionary.Add(
                    this.serializableKeys[i],
                    this.serializableValues[i]
                );
            }
        }

        // Allow iterating over this class
        IEnumerator<KeyValuePair<TKey, TValue>> IEnumerable<KeyValuePair<TKey, TValue>>
            .GetEnumerator() => this.dictionary.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() =>
            this.dictionary.GetEnumerator();

        public void Add(TKey key, TValue value) => this.dictionary.Add(key, value);

        public bool Remove(TKey key) => this.dictionary.Remove(key);

        public bool TryGetValue(TKey key, out TValue value) =>
            this.dictionary.TryGetValue(key, out value);
    }
}
