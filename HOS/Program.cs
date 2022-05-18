using System;
using Kurukuru;

namespace Client
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            Console.Title = string.Empty;
            Spinner.Start("Loading LibVLC...", () => LibVLCSharp.Shared.Core.Initialize());
            Spinner.Start("Loading console driver...", () => Terminal.Gui.Application.Init());
            new Tui().Run();
        }
    }
}
