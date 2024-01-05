using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace UnityUtilities
{
    /// <summary>
    /// A replacement for `Task.Run()` that cancels tasks when entering or
    /// exiting play mode in the Unity editor (which doesn't happen by default).
    ///
    /// Also registers an UnobservedTaskException handler to prevent exceptions
    /// from being swallowed in all Tasks (including SafeTasks), which would
    /// happen when these are not awaited or are chained with `.ContinueWith()`.
    /// </summary>
    public static class SafeTask
    {
        private static CancellationTokenSource cancellationTokenSource = new();

        public static Task<TResult> Run<TResult>(Func<Task<TResult>> f) =>
            SafeTask.Run<TResult>((object)f);

        public static Task<TResult> Run<TResult>(Func<TResult> f) =>
            SafeTask.Run<TResult>((object)f);

        public static Task Run(Func<Task> f) => SafeTask.Run<object>((object)f);

        public static Task Run(Action f) => SafeTask.Run<object>((object)f);

        private static async Task<TResult> Run<TResult>(object f)
        {
            // We use tokens and not the cancellation source directly as it is
            // replaced with a new one upon exiting play or edit mode.
            CancellationToken token = CancellationToken.None;
            TResult result = default;

            // Pending tasks when entering/exiting play mode are only a problem
            // in the editor.
            if (Application.isEditor)
            {
                SafeTask.cancellationTokenSource ??= new();
                token = SafeTask.cancellationTokenSource.Token;
            }

            try
            {
                // Pass token to Task.Run() as well, otherwise upon cancelling
                // its status will change to faulted instead of cancelled.
                // https://stackoverflow.com/a/72145763/2037431

                if (f is Func<Task<TResult>> g)
                {
                    result = await Task.Run(() => g(), token);
                }
                else if (f is Func<TResult> h)
                {
                    result = await Task.Run(() => h(), token);
                }
                else if (f is Func<Task> i)
                {
                    await Task.Run(() => i(), token);
                }
                else if (f is Action j)
                {
                    await Task.Run(() => j(), token);
                }
            }
            catch (Exception e)
            {
                // We log unobserved exceptions with an UnobservedTaskException
                // handler, but those are only handled when garbage collection happens.
                // We thus force exceptions to be logged here - at least for SafeTasks.
                // If a failing SafeTask is awaited, the exception will be logged twice, but that's
                // ok.
                UnityEngine.Debug.LogException(e);
                throw;
            }

            if (token.IsCancellationRequested)
            {
                throw new OperationCanceledException(
                    "An asynchronous task has been canceled due to entering or exiting play mode.",
                    token
                );
            }

            return result;
        }

#if UNITY_EDITOR
        [UnityEditor.InitializeOnLoadMethod]
        private static void OnLoadCallback()
        {
            // Prevent unobserved task exceptions from being swallowed.
            // This happens when:
            //  * A Task that isn't awaited fails;
            //  * A Task chained with `.ContinueWith()` fails and exceptions are
            //    not explicitly handled in the function passed to it.
            //
            // This event handler works for both Tasks and SafeTasks.
            //
            // Note this only seems to run when garbage collection happens (such
            // as after script reloading in the Unity editor).
            // Calling `System.GC.Collect()` after the exception caused
            // exceptions to be logged right away.
            TaskScheduler.UnobservedTaskException += (_, e) =>
                UnityEngine.Debug.LogException(e.Exception);

            // Cancel pending `SafeTask.Run()` calls when exiting play or edit
            // mode.
            UnityEditor.EditorApplication.playModeStateChanged += (change) =>
            {
                if (
                    change == UnityEditor.PlayModeStateChange.ExitingPlayMode
                    || change == UnityEditor.PlayModeStateChange.ExitingEditMode
                )
                {
                    SafeTask.cancellationTokenSource.Cancel();
                    SafeTask.cancellationTokenSource.Dispose();
                    SafeTask.cancellationTokenSource = new CancellationTokenSource();
                }
            };
        }
#endif
    }
}
