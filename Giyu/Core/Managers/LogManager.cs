using System;

namespace Giyu.Core.Managers
{
    public class LogManager
    {
        public static void Log(string LogType, string args)
        {
            Console.WriteLine($"[{DateTime.UtcNow}]\t({LogType})\t{args}");
        }
    }
}
