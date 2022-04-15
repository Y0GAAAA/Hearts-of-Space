using System;
using System.Collections;
using System.Collections.Generic;

#nullable enable

namespace Client
{
    public class LimitedStack<T> : ICollection<T>, ICollection, IEnumerable<T>, IReadOnlyCollection<T>
    {
        private readonly Int32 size;
        private readonly List<T> stack;

        public LimitedStack(Int32 size)
        {
            stack = new List<T>(size);
            this.size = size;
        }

        public Int32 Count => stack.Count;
        public Boolean IsReadOnly => false;
        public Boolean IsSynchronized => false;
        public Object? SyncRoot => null;

        public T? Pop() => Remove(Count - 1);
        public T? PopBack() => Remove(0);
        public T? Peek()
        {
            if (Count == 0)
                return default(T);
            return stack[Count - 1];
        }
        public T? PeekBack()
        {
            if (Count == 0)
                return default(T);
            return stack[0];
        }
        public T? Remove(Int32 index)
        {
            if (index >= size || index >= Count || index < 0)
                return default(T);

            T v = stack[index];
            stack.RemoveAt(index);
            return v;
        }
        public void Add(T item)
        {
            while (Count >= size)
                Remove(0);
            stack.Add(item);
        }
        public void Clear() => stack.Clear();
        public Boolean Contains(T item) => stack.Contains(item);
        public void CopyTo(T[] array, Int32 arrayIndex) => stack.CopyTo(array, arrayIndex);
        public void CopyTo(Array array, Int32 index) => throw new NotImplementedException();
        public IEnumerator<T> GetEnumerator() => stack.GetEnumerator();
        public Boolean Remove(T item) => stack.Remove(item);
        IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)stack).GetEnumerator();
    }
}
