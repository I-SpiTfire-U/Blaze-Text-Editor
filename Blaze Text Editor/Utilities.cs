namespace Blaze_Text_Editor
{
    internal static class Utilities
    {
        internal static string? PromptUser(string prompt)
        {
            Console.Write(prompt);
            return Console.ReadLine();
        }

        internal static void WriteAtPosition(int x, int y, string text)
        {
            Console.SetCursorPosition(x, y);
            Console.Write(text);
        }
    }
}