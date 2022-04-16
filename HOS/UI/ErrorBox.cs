using Terminal.Gui;

namespace Client.UI
{
    public static class ErrorBox
    {
        public static void Show(string message) => MessageBox.ErrorQuery("error", message, "OK");
    }
}
