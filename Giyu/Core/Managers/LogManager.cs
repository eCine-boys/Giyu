using System;

namespace Giyu.Core.Managers
{

    public interface ILogger
    {
        void Log(string? logType, string message);

        void Error(string? typeDef, string message, string moreInfo);

        void Fatal(string? typeDef, string message, Exception ex);

        void Debug(string? typeDef, string message);

    }

    public class Logger : ILogger
    {
        public void Debug(string typeDef, string message)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;

            Console.WriteLine($"({DateTime.UtcNow})\t({typeDef})\t{message}");
        }

        public void Error(string typeDef, string message, string? moreInfo)
        {
            Console.ForegroundColor = ConsoleColor.DarkRed;

            Console.WriteLine($"({DateTime.UtcNow})\t({typeDef})\t{message}\n{moreInfo}");
        }

        public void Fatal(string typeDef, string message, Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;

            Console.WriteLine($"({DateTime.UtcNow})\t({typeDef})\t{message}\n{ex.Message}");
        }

        public void Log(string logType, string message)
        {
            Console.ForegroundColor = ConsoleColor.Magenta;

            Console.WriteLine($"({DateTime.UtcNow})\t({logType})\t{message}");
        }
    }

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
