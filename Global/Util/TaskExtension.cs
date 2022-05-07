using System.Threading;
using System.Threading.Tasks;

namespace Global.Util
{
    public static class TaskExtension
    {
        public static void BlockOn(this Task t) => t.GetAwaiter().GetResult();
        public static T BlockOn<T>(this Task<T> t) => t.GetAwaiter().GetResult();

        public static bool Finished(this Task t) => t.IsCompleted || t.IsCompletedSuccessfully || t.IsCanceled || t.IsFaulted;

        // The std's Wait method on Task seems to only wait for a task to complete successfully
        // we want to wait for a task to finish, whatever the result may be
        public static async Task WaitAsync(this Task t, CancellationToken token = default)
        {
            while (!t.Finished())
                await Task.Delay(50, token);
        }
    }
}
