using System;
using System.Threading;
using System.Threading.Tasks;

namespace MarcosPereira.Utility {
    public static class SafeTask {
        private static CancellationTokenSource cancellationTokenSource =
            new CancellationTokenSource();

        public static async Task<TResult> Run<TResult>(Func<Task<TResult>> f) {
            // We have to store a token and cannot simply query the source
            // after awaiting, as the token source is replaced with a new one
            // upon exiting play mode.
            CancellationToken token = SafeTask.cancellationTokenSource.Token;

            // Pass token to Task.Run() as well, otherwise upon cancelling its
            // status will change to faulted instead of cancelled.
            // https://stackoverflow.com/a/72145763/2037431
            TResult result = await Task.Run(f, token);

            SafeTask.ThrowIfCancelled(token);

            return result;
        }

        public static async Task Run(Func<Task> f) {
            CancellationToken token = SafeTask.cancellationTokenSource.Token;
            await Task.Run(f, token);
            SafeTask.ThrowIfCancelled(token);
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
            // Cancel pending tasks when exiting play mode, as Unity won't do
            // that for us.
            // See "Limitations of async and await tasks" (https://docs.unity3d.com/2022.2/Documentation/Manual/overview-of-dot-net-in-unity.html)
            UnityEditor.EditorApplication.playModeStateChanged +=
                (change) => {
                    if (change == UnityEditor.PlayModeStateChange.ExitingPlayMode) {
                        SafeTask.cancellationTokenSource.Cancel();
                        SafeTask.cancellationTokenSource.Dispose();
                        SafeTask.cancellationTokenSource = new CancellationTokenSource();
                    }
                };
        }
#endif
    }
}
