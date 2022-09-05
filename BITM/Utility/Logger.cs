using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BITM.Utility
{
    public static class Logger
    {
        public enum LogMode
        {
            Console,
            File,
            Both
        }
        public enum LogLevel
        {
            Debug,
            Info,
            Warn,
            Error,
            Fatal
        }
        public static LogMode mode { get; set; }
        private static ConsoleColor getConsoleColor(LogLevel logLevel) {
            switch (logLevel)
            {
                case LogLevel.Info:
                    return ConsoleColor.Gray;
                case LogLevel.Debug:
                    return ConsoleColor.DarkYellow;
                case LogLevel.Fatal:
                    return ConsoleColor.DarkRed;
                case LogLevel.Warn:
                    return ConsoleColor.Yellow;
                case LogLevel.Error:
                    return ConsoleColor.Red;
                default:
                    return ConsoleColor.White;
            }
        }

        public static void Log(string message, LogLevel level)
        {
            string logFormat = $"{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")} - [{level.ToString()}] {message}";
            Console.ForegroundColor = getConsoleColor(level);
            switch (mode)
            {
                case LogMode.Console:
                    Console.WriteLine(logFormat);
                    break;
                case LogMode.File:
                    AppendToFile(logFormat);
                    break;
                case LogMode.Both:
                    Console.WriteLine(logFormat);
                    AppendToFile(logFormat);
                    break;
            }
        }

        private static void AppendToFile(string message)
        {
            using (System.IO.StreamWriter file = new System.IO.StreamWriter(@"log.txt", true))
            {
                file.WriteLine(message);
            }
        }
    }
}
