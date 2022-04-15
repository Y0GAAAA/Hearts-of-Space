using System;

namespace Client
{
    internal class Program
    {
        /*
         * TODO
         * "upper menu" with keybinds
         */
        private static void Main(String[] args)
        {
            Console.Title = string.Empty;
            new Tui().Run();
        }
    }
}
