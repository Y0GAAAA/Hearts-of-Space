using System.Threading.Tasks;

namespace Global.Util
{
    public static class TaskExtension
    {
        public static void BlockOn(this Task t) => t.GetAwaiter().GetResult();
        public static T BlockOn<T>(this Task<T> t) => t.GetAwaiter().GetResult();
    }
}
