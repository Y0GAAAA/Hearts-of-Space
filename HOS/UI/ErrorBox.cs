using System;
using Terminal.Gui;

namespace Client.UI
{
    public static class ErrorBox
    {
        public static void Show(String message) => MessageBox.ErrorQuery("error", message, "OK");
    }
}
