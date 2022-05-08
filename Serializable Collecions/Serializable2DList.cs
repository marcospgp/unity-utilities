using UnityEngine;
using System.Collections.Generic;

namespace MarcosPereira.Utility {
    public class Serializable2DList<T> : ScriptableObject {
        [SerializeField]
        private List<SubList<T>> items = new List<SubList<T>>();

        // Have to wrap sublists in a custom class for serialization of nested
        // containers to work.
        private class SubList<U> : ScriptableObject {
            public List<U> items = new List<U>();
        }

        public static Serializable2DList<T> Create() {
            Serializable2DList<T> instance =
                ScriptableObject.CreateInstance<Serializable2DList<T>>();

            return instance;
        }

        public int count => this.items.Count;

        // Implement multidimensional indexer
        public T this[int i, int j] {
            get => this.items[i].items[j];
            set => this.items[i].items[j] = value;
        }
    }
}
