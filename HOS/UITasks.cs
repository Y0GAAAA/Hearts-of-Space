using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

#nullable enable

namespace Client
{
    public static class UITasks
    {

        public static readonly List<TaskData> tasks = new List<TaskData>();
        private static readonly object TASK_MANIPULATE_LOCK = new object();

        private const int MAX_TASK_COUNT = byte.MaxValue;
        private const string UNKNOWN_TASK_TITLE = "<phantom task>";

        #region MUST be used in sync-lock
        private static int GetAvailableTaskId()
        {
            var taskIds = tasks.Select(t => t.Id);
            return Enumerable.Range(1, MAX_TASK_COUNT)
                             .First(id => !taskIds.Contains(id));
        }

        private static void RemoveTaskById(int id) =>
            // the int gets implicitly converted to a TaskData with null title and the right id so Equals() will detect it as same
            tasks.Remove(id);
        #endregion

        public static async Task WithUITask(this Task t, string title)
        {
            int id;
            lock (TASK_MANIPULATE_LOCK)
            {
                id = GetAvailableTaskId();
                tasks.Add(
                    new TaskData(title ?? UNKNOWN_TASK_TITLE, id)
                );
            }

            await t;

            lock (TASK_MANIPULATE_LOCK) { RemoveTaskById(id); }
        }
        public static async Task<T> WithUITask<T>(this Task<T> t, string? title)
        {
            int id;
            lock (TASK_MANIPULATE_LOCK)
            {
                id = GetAvailableTaskId();
                tasks.Add(
                    new TaskData(title ?? UNKNOWN_TASK_TITLE, id)
                );
            }

            var value = await t;

            lock (TASK_MANIPULATE_LOCK) { RemoveTaskById(id); }

            return value;
        }
    }
}
