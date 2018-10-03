#region USING_DIRECTIVES
using System;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace TheGodfather.Common
{
    public class AsyncExecutor
    {
        private readonly SemaphoreSlim sem;


        public AsyncExecutor()
        {
            this.sem = new SemaphoreSlim(1, 1);
        }


        public void Execute(Task task)
        {
            this.sem.Wait();

            Exception tex = null;

            var are = new AutoResetEvent(false);
            _ = Task.Run(async () => {
                try {
                    await task;
                } catch (Exception ex) {
                    tex = ex;
                } finally {
                    are.Set();
                }
            });
            are.WaitOne();

            this.sem.Release();

            if (!(tex is null))
                throw tex;
        }

        public T Execute<T>(Task<T> task)
        {
            this.sem.Wait();

            Exception tex = null;
            T result = default;

            var are = new AutoResetEvent(false);
            _ = Task.Run(async () => {
                try {
                    result = await task;
                } catch (Exception ex) {
                    tex = ex;
                } finally {
                    are.Set();
                }
            });
            are.WaitOne();

            this.sem.Release();

            if (!(tex is null))
                throw tex;

            return result;
        }
    }
}
