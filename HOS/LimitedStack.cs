using System;
using System.Collections;
using System.Collections.Generic;

#nullable enable

namespace Client
{
    public class LimitedStack<T> : ICollection<T>, ICollection, IEnumerable<T>, IReadOnlyCollection<T>
    {
        private readonly int size;
        private readonly List<T> stack;

        public LimitedStack(int size)
        {
            stack = new List<T>(size);
            this.size = size;
        }

        public int Count => stack.Count;
        public bool IsReadOnly => false;
        public bool IsSynchronized => false;
        public object? SyncRoot => null;

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
        public T? Remove(int index)
        {
            if (index >= size || index >= Count || index < 0)
                return default(T);

            var v = stack[index];
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
        public bool Contains(T item) => stack.Contains(item);
        public void CopyTo(T[] array, int arrayIndex) => stack.CopyTo(array, arrayIndex);
        public void CopyTo(Array array, int index) => throw new NotImplementedException();
        public IEnumerator<T> GetEnumerator() => stack.GetEnumerator();
        public bool Remove(T item) => stack.Remove(item);
        IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable) stack).GetEnumerator();
    }
}
