using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cascade.Core.Logging
{
    public class LogManager
    {
        public void Log(string line, LogType logType)
        {
            var existingColor = Console.ForegroundColor;
            ApplyColorScheme(GetColorByLogType(logType));
            Console.WriteLine(GetLogDateString() + line);
            ApplyColorScheme(existingColor);
        }

        private static string GetLogDateString()
        {
            return " [" + DateTime.Now.ToLongTimeString() + "] ";
        }

        private static ConsoleColor GetColorByLogType(LogType logType)
        {
            switch (logType)
            {
                case LogType.Error:
                    return ConsoleColor.Red;
                case LogType.Warning:
                    return ConsoleColor.Yellow;
                default:
                    return ConsoleColor.Gray;
            }
        }

        private static void ApplyColorScheme(ConsoleColor foregroundColor)
        {
            Console.ForegroundColor = foregroundColor;
        }
    }
}
