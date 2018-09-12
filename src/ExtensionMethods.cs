using System;

namespace DockerImageRemover
{
    public static class ExtensionMethods
    {
        public static void ToConsole(this string message, ConsoleColor color = ConsoleColor.White)
        {
            var oldColor = Console.ForegroundColor;

            Console.ForegroundColor = color;
            Console.Write(message);
            Console.ForegroundColor = oldColor;
        }
    }
}