using Kurukuru;
using System;

namespace Client
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            Console.Title = string.Empty;
            var tui = new Tui();
            Spinner.Start("Loading LibVLC...", () => LibVLCSharp.Shared.Core.Initialize());
            Spinner.Start("Loading console driver...", () => Terminal.Gui.Application.Init());
            Spinner.Start("Initializing media player...", () => tui.audioPlayer.Initialize());
            tui.Run();
        }
    }
}
