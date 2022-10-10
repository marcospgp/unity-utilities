using System.Collections;
using System.Threading.Tasks;
using UnityEngine;

// Allow awaiting a task inside a coroutine, as if it were just another
// coroutine.
// Based on https://forum.unity.com/threads/use-async-method-in-coroutine.868825/#post-5719195

namespace MarcosPereira.UnityUtilities {
    public static class TaskAsCoroutineExtension {
        public static IEnumerator AsCoroutine(this Task task) {
            yield return new WaitUntil(() => task.IsCompleted);

            if (task.Exception != null) {
                throw task.Exception;
            }
        }
    }
}
