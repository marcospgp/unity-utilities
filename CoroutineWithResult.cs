using System.Collections;

// Transforms a coroutine into an object that stores its last yield as the
// result.
// Usage (from inside another coroutine):
//
// var c = new Coroutine<GameObject>(SomeCoroutine());
// yield return c;
// Gameobject g = c.result;

namespace MarcosPereira.UnityUtilities {
    public class CoroutineWithResult<T> : IEnumerator {
        public T result;
        private readonly IEnumerator coroutine;

        public CoroutineWithResult(IEnumerator coroutine) {
            this.coroutine = coroutine;
        }

        object IEnumerator.Current => this.coroutine.Current;

        bool IEnumerator.MoveNext() {
            bool x = this.coroutine.MoveNext();

            if (this.coroutine.Current is T current) {
                this.result = current;
            }

            return x;
        }

        void IEnumerator.Reset() => this.coroutine.Reset();
    }
}
