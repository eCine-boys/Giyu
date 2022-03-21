using System;

namespace Giyu.Core.Managers
{
    public class LogManager
    {
        public static void Log(string LogType, string args)
        {
            Console.ForegroundColor = ConsoleColor.Blue;

            Console.WriteLine($"[{DateTime.UtcNow}]\t({LogType})\t{args}");
        }

        public static void LogError(string message)
        {
            Console.ForegroundColor = ConsoleColor.Red;

            Console.WriteLine($"[{DateTime.UtcNow}]\t(ERROR)\t{message}", Console.ForegroundColor);
        }

        public static void LogDebug(string debug)
        {
            Console.ForegroundColor = ConsoleColor.Magenta;

            Console.WriteLine($"[{DateTime.UtcNow}]\t(DEBUG)\t{debug}", Console.ForegroundColor);

        }
    }
}
