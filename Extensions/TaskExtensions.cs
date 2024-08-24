using System.Collections;
using System.Threading.Tasks;
using UnityEngine;

// Allow awaiting a task inside a coroutine, as if it were just another
// coroutine.
// Usage: yield return someTask.AsCoroutine();
// Based on https://forum.unity.com/threads/use-async-method-in-coroutine.868825/#post-5719195

namespace UnityUtilities
{
    public static class TaskExtensions
    {
        public static IEnumerator AsCoroutine(this Task task)
        {
            yield return new WaitUntil(() => task.IsCompleted);

            if (task.Exception != null)
            {
                throw task.Exception;
            }
        }
    }
}
