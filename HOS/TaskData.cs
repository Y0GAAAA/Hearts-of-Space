using System;

namespace Client
{
    public class TaskData : IEquatable<TaskData>
    {
        public string Title { get; private set; }
        public int Id { get; private set; }

        public TaskData(string title, int id)
        {
            (Title, Id) = (title, id);
        }

        public override string ToString() => $"T{Id} {Title}";
        public bool Equals(TaskData other) => Id == other.Id;

        // create TaskData with only id as int, implicitly
        public static implicit operator TaskData(int i)
        {
            return new TaskData(null, i);
        }
    }
}
