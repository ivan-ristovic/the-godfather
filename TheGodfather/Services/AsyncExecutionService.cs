using System;
using System.Threading;
using System.Threading.Tasks;

namespace TheGodfather.Services
{
    public sealed class AsyncExecutionService : ITheGodfatherService
    {
        public bool IsDisabled => false;

        private readonly SemaphoreSlim sem;


        public AsyncExecutionService()
        {
            this.sem = new SemaphoreSlim(1, 1);
        }


        public void Execute(Task task)
        {
            Exception tex = null;

            this.sem.Wait();
            try {
                using (var are = new AutoResetEvent(false)) {
                    _ = Task.Run(async () => {
                        try {
                            await task;
                        } catch (Exception e) {
                            tex = e;
                        } finally {
                            are.Set();
                        }
                    });
                    are.WaitOne();
                }
            } finally {
                this.sem.Release();
            }

            if (!(tex is null))
                throw tex;
        }

        public T Execute<T>(Task<T> task)
        {
            T result = default;
            Exception tex = null;

            this.sem.Wait();
            try {
                using (var are = new AutoResetEvent(false)) {
                    _ = Task.Run(async () => {
                        try {
                            result = await task;
                        } catch (Exception e) {
                            tex = e;
                        } finally {
                            are.Set();
                        }
                    });
                    are.WaitOne();
                }
            } finally {
                this.sem.Release();
            }

            if (!(tex is null))
                throw tex;

            return result;
        }
    }
}
