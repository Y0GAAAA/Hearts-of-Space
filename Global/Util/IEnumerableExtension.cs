using System;
using System.Collections.Generic;

namespace Global.Util
{
    public static class IEnumerableExtension
    {

        // ty <3 https://stackoverflow.com/a/24181715/10974882
        public static IEnumerable<T> Cycle<T>(this IEnumerable<T> enumerable)
        {
            while (true)
            {
                foreach (var item in enumerable)
                    yield return item;
            }
        }

        public static void AddWhile<R>(this ICollection<R> collection, Func<bool> predicate, Func<R> f)
        {
            while (predicate())
                collection.Add(f());
        }
    }
}
