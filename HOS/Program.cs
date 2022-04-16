using System;

namespace Client
{
    internal class Program
    {
        /*
         * TODO
         * "upper menu" with keybinds
         */
        private static void Main(string[] args)
        {
            Console.Title = string.Empty;
            new Tui().Run();
        }
    }
}
