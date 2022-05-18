using System;

namespace Client
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            Console.Title = string.Empty;
            LibVLCSharp.Shared.Core.Initialize();
            new Tui().Run();
        }
    }
}
