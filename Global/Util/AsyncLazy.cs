using System;
using System.Threading;
using System.Threading.Tasks;

namespace Global.Util
{
    public class AsyncLazy<T> where T : class
    {
        private T value;
        private readonly Func<Task<T>> factory;

        private readonly SemaphoreSlim semaphore = new SemaphoreSlim(1);

        public bool ValueCreated { get; private set; } = false;

        public AsyncLazy(Func<Task<T>> f)
        {
            factory = f;
        }

        public async Task<T> GetAsync()
        {
            // fast path
            if (ValueCreated)
                return value;

            await semaphore.WaitAsync();

            // second check to avoid multiple initializations
            if (!ValueCreated)
            {
                value = await factory();
                ValueCreated = true;
            }
            semaphore.Release();
            return value;
        }
    }
}