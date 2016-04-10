using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Threading;

namespace AgilefantTimes
{
    internal enum LogLevel
    {
        Write,
        Info,
        Warn,
        Success,
        Error
    }

    internal static class Logger
    {
        public static bool Enabled { private get; set; }
        public static bool ShouldLog { private get; set; }
        private static readonly ConsoleColor DefaultColour;
        private static readonly DateTime StartTime;
        private static readonly BlockingCollection<LogItem> LogQueue;

        static Logger()
        {
            DefaultColour = Console.ForegroundColor;
            StartTime = Process.GetCurrentProcess().StartTime;
            LogQueue = new BlockingCollection<LogItem>();
            ShouldLog = true;

            var t = new Thread(LogBackground) {IsBackground = true};
            t.Start();
        }

        private struct LogItem
        {
            public string Data { get; }
            public LogLevel Level { get; }

            public LogItem(string data, LogLevel level)
            {
                Data = data;
                Level = level;
            }
        }

        private static void ResetColour()
        {
            Console.ForegroundColor = DefaultColour;
        }

        public static void Log(string data, LogLevel level)
        {
            LogQueue.Add(new LogItem(data, level));
        }

        public static void Log<T>(this T obj, LogLevel level)
        {
            Log(obj.ToString(), level);
        }

        private static void LogBackground()
        {
            while (ShouldLog)
            {
                var item = LogQueue.Take();
                Debug.WriteLine(item.Data);
                if (!Enabled && item.Level != LogLevel.Write) return;
                var time = (DateTime.Now - StartTime).TotalSeconds;
                Console.Write("[" + time.ToString("0.01").PadLeft(11, ' ') + "] ");
                switch (item.Level)
                {
                    case LogLevel.Write:
                        Console.ForegroundColor = DefaultColour;
                        break;
                    case LogLevel.Info:
                        Console.ForegroundColor = ConsoleColor.Cyan;
                        break;
                    case LogLevel.Warn:
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        break;
                    case LogLevel.Success:
                        Console.ForegroundColor = ConsoleColor.Green;
                        break;
                    case LogLevel.Error:
                        Console.ForegroundColor = ConsoleColor.Red;
                        break;
                    default:
                        goto case LogLevel.Write;
                }
                Console.WriteLine(item.Data);
                ResetColour();
            }
        }
    }
}
