using System;
using System.Threading;
using System.Threading.Tasks;

namespace TheGodfather.Services
{
    public sealed class AsyncExecutionService : ITheGodfatherService
    {
        public bool IsDisabled => false;


        public void Execute(Task? task)
        {
            if (task is null)
                return;

            var ts = new AsyncState<object>(new AutoResetEvent(false));
            task.ContinueWith(OnComplete, ts);
            ts.Lock.WaitOne();

            if (ts.Exception is { })
                throw ts.Exception;


            static void OnComplete(Task t, object? state)
            {
                AsyncState<object> stateRef = state as AsyncState<object> ?? throw new InvalidCastException(nameof(state));
                CheckForFaultsAndUpdateState(t, stateRef);
                stateRef.Lock.Set();
            }
        }

        public T Execute<T>(Task<T> task)
        {
            var ts = new AsyncState<T>(new AutoResetEvent(false));
            task.ContinueWith(TaskCompletionHandler, ts);
            ts.Lock.WaitOne();

            if (ts.Exception is { })
                throw ts.Exception;

            if (ts.HasResult)
                return ts.Result;

            throw new InvalidOperationException("Task returned no result.");


            static void TaskCompletionHandler(Task<T> t, object? state)
            {
                AsyncState<T> stateRef = state as AsyncState<T> ?? throw new InvalidCastException(nameof(state));
                CheckForFaultsAndUpdateState(t, stateRef);

                if (t.IsCompleted && !t.IsFaulted) {
                    stateRef.HasResult = true;
                    stateRef.Result = t.Result;
                }

                stateRef.Lock.Set();
            }
        }


        private static void CheckForFaultsAndUpdateState<T>(Task task, AsyncState<T> state)
        {
            if (task.IsFaulted) {
                state.Exception = task.Exception?.InnerExceptions.Count == 1 
                    ? task.Exception.InnerException 
                    : task.Exception ?? new Exception("No details provided");
            } else if (task.IsCanceled) {
                state.Exception = new TaskCanceledException(task);
            }
        }


        private sealed class AsyncState<T>
        {
            public AutoResetEvent Lock { get; }
            public Exception? Exception { get; set; }
            public T Result { get; set; }
            public bool HasResult { get; set; }


            public AsyncState(AutoResetEvent @lock)
            {
                this.Lock = @lock;
                this.HasResult = false;
                this.Result = default!;
            }
        }
    }
}
