using System;
using System.Threading;
using System.Threading.Tasks;

// Known Issues
//
// - Exiting play mode by interrupting the editor with a code change causes
//   MonoBehaviours to be destroyed while pending asynchronous tasks are still
//   executed. This means a task may return to its containing gameobject after
//   an await only to see it destroyed.
//   Interestingly, if the code change does not introduce an exception, any
//   exceptions resulting from accessing the destroyed object will be swallowed
//   (not logged to the console), and if doing something such as creating a
//   gameobject, it will be added to the scene in edit mode and will have to be
//   removed manually.
//   It is not clear whether this issue is related to `Task.Run()` only or any
//   task, even if running on the main thread.
//   To avoid any issues, make sure to check that your MonoBehaviour has not
//   been destroyed after awaiting a task, especially if that task goes into a
//   separate thread at some point (such as by starting a SafeTask).
//   See more at:
//   https://forum.unity.com/threads/stopping-play-mode-by-pressing-play-button-or-by-changing-a-script-have-different-outcomes.1337852/#post-8449817

namespace MarcosPereira.UnityUtilities {
    /// <summary>
    /// A replacement for `Task.Run()` that cancels tasks when exiting play
    /// mode, which Unity doesn't do by default.
    /// Also registers a UnobservedTaskException handler to prevent exceptions
    /// from being swallowed in both Tasks and SafeTasks, when these are
    /// unawaited or chained with `.ContinueWith()`.
    /// </summary>
    public static class SafeTask {
        private static CancellationTokenSource cancellationTokenSource =
            new CancellationTokenSource();

        public static Task<TResult> Run<TResult>(Func<Task<TResult>> f) =>
            SafeTask.Run<TResult>((object) f);

        public static Task<TResult> Run<TResult>(Func<TResult> f) =>
            SafeTask.Run<TResult>((object) f);

        public static Task Run(Func<Task> f) => SafeTask.Run<object>((object) f);

        public static Task Run(Action f) => SafeTask.Run<object>((object) f);

        private static async Task<TResult> Run<TResult>(object f) {
            // We have to store a token and cannot simply query the source
            // after awaiting, as the token source is replaced with a new one
            // upon exiting play mode.
            CancellationToken token = SafeTask.cancellationTokenSource.Token;
            TResult result = default;

            try {
                // Pass token to Task.Run() as well, otherwise upon cancelling
                // its status will change to faulted instead of cancelled.
                // https://stackoverflow.com/a/72145763/2037431

                if (f is Func<Task<TResult>> g) {
                    result = await Task.Run(() => g(), token);
                } else if (f is Func<TResult> h) {
                    result = await Task.Run(() => h(), token);
                } else if (f is Func<Task> i) {
                    await Task.Run(() => i(), token);
                } else if (f is Action j) {
                    await Task.Run(() => j(), token);
                }
            } catch (Exception e) {
                // We log unobserved exceptions with an UnobservedTaskException handler, but those
                // are only handled when garbage collection happens.
                // We thus force exceptions to be logged here - at least for SafeTasks.
                // If a failing SafeTask is awaited, the exception will be logged twice, but that's
                // ok.
                UnityEngine.Debug.LogException(e);
                throw;
            }

            SafeTask.ThrowIfCancelled(token);

            return result;
        }

        private static void ThrowIfCancelled(CancellationToken token) {
            if (token.IsCancellationRequested) {
                throw new OperationCanceledException(
                    "An asynchronous task has been cancelled due to exiting play mode.",
                    token
                );
            }
        }

#if UNITY_EDITOR
        [UnityEditor.InitializeOnLoadMethod]
        private static void OnLoad() {
            // Prevent unobserved task exceptions from being swallowed.
            // This happens when:
            //  * An unawaited Task fails;
            //  * A Task chained with `.ContinueWith()` fails and exceptions are
            //    not explicitly handled in the function passed to it.
            //
            // Note that this event handler works for both Tasks and SafeTasks.
            //
            // Also note that this handler may not fire right away. It seems to
            // only run when garbage collection happens (for example, in the
            // editor after script reloading).
            // Experimentally, calling `System.GC.Collect()` after the exception
            //  (using a small `Task.Delay()` to ensure it runs after the
            // exception is thrown) caused exceptions to be logged right away.
            TaskScheduler.UnobservedTaskException +=
                (_, e) => UnityEngine.Debug.LogException(e.Exception);

            // Cancel pending `Task.Run()` calls when exiting play mode, as
            // Unity won't do that for us.
            // See "Limitations of async and await tasks" (https://docs.unity3d.com/2022.2/Documentation/Manual/overview-of-dot-net-in-unity.html)
            // This only works in SafeTasks, so `Task.Run()` should never be
            // used directly.
            UnityEditor.EditorApplication.playModeStateChanged +=
                (change) => {
                    if (
                        change == UnityEditor.PlayModeStateChange.ExitingPlayMode ||
                        change == UnityEditor.PlayModeStateChange.ExitingEditMode
                    ) {
                        SafeTask.cancellationTokenSource.Cancel();
                        SafeTask.cancellationTokenSource.Dispose();
                        SafeTask.cancellationTokenSource = new CancellationTokenSource();
                    }
                };
        }
#endif
    }
}
