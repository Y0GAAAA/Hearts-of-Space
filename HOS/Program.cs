using System;

namespace Client
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            Console.Title = string.Empty;
            new Tui().Run();
        }
    }
}
