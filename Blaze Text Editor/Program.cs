namespace Blaze_Text_Editor
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            Instance instance = new(args.Length > 0 ? args[0] : null);
            instance.Start();
        }
    }
}