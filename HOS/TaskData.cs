using System;

namespace Client
{
    public class TaskData : IEquatable<TaskData>
    {
        public String Title { get; private set; }
        public int Id { get; private set; }

        public TaskData(String title, int id)
        {
            (Title, Id) = (title, id);
        }

        public override String ToString() => $"T{Id} {Title}";
        public Boolean Equals(TaskData other) => Id == other.Id;

        // create TaskData with only id as int, implicitly
        public static implicit operator TaskData(int i)
        {
            return new TaskData(null, i);
        }
    }
}
