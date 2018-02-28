#region USING_DIRECTIVES
using System;
using System.IO;
using System.Text;

using DSharpPlus;
using DSharpPlus.EventArgs;
#endregion

namespace TheGodfather.Entities
{
    public static class Logger
    {
        private static object _lock = new object();
        private static bool LogToFile = true;
        private static string _path = "log.txt";
        private static readonly int BUFFER_SIZE = 512;


        public static void Clear()
        {
            lock (_lock) {
                Console.Clear();
            }
        }

        public static void LogMessage(LogLevel level, string message, DateTime? timestamp = null, bool filelog = true)
        {
            lock (_lock) {
                PrintTimestamp(timestamp);
                PrintLevel(level);
                PrintLogMessage(message.Replace('\n', ' '));
                if (filelog && LogToFile)
                    WriteToLogFile(level, message, timestamp);
            }
        }

        public static void LogMessage(int shardid, DebugLogMessageEventArgs e, bool filelog = true)
        {
            lock (_lock) {
                PrintTimestamp(e.Timestamp);

                Console.ForegroundColor = ConsoleColor.DarkGreen;
                Console.Write("[#{0}] ", shardid.ToString());

                Console.ForegroundColor = ConsoleColor.White;
                Console.Write("[{0}] ", e.Application);

                PrintLevel(e.Level);
                PrintLogMessage(e.Message.Replace('\n', ' '));
                if (filelog && LogToFile)
                    WriteToLogFile(shardid, e);
            }
        }

        public static void LogException(LogLevel level, Exception e, DateTime? timestamp = null, bool filelog = true)
        {
            lock (_lock) {
                PrintTimestamp(timestamp);
                PrintLevel(level);
                PrintLogMessage($"Exception occured: {e.GetType()}<br>Details: {e.Message.Replace('\n', ' ')}<br>");
                if (e.InnerException != null)
                    PrintLogMessage($"Inner exception: {e.InnerException}<br>");
                PrintLogMessage($"Stack trace: {e.StackTrace}");
                if (filelog && LogToFile)
                    WriteToLogFile(level, e);
            }
        }


        private static void WriteToLogFile(LogLevel level, string message, DateTime? timestamp = null)
        {
            try {
                using (StreamWriter sw = new StreamWriter(_path, true, Encoding.UTF8, BUFFER_SIZE)) {
                    sw.Write("[{0:yyyy-MM-dd HH:mm:ss zzz}] ", timestamp ?? DateTime.Now);
                    sw.WriteLine("[{0}]", level.ToString());
                    sw.WriteLine(message.Replace("<br>", Environment.NewLine).Trim());
                    sw.WriteLine();
                    sw.Flush();
                }
            } catch (Exception e) {
                LogException(LogLevel.Error, e, filelog: false);
            }
        }

        private static void WriteToLogFile(int shardid, DebugLogMessageEventArgs e)
        {
            try {
                using (StreamWriter sw = new StreamWriter(_path, true, Encoding.UTF8, BUFFER_SIZE)) {
                    sw.Write("[{0:yyyy-MM-dd HH:mm:ss zzz}] ", e.Timestamp);
                    sw.Write("[#{0}] ", shardid.ToString());
                    sw.Write("[{0}] ", e.Application);
                    sw.WriteLine("[{0}]", e.Level.ToString());
                    sw.WriteLine(e.Message.Replace("<br>", Environment.NewLine).Trim());
                    sw.WriteLine();
                    sw.Flush();
                }
            } catch (Exception exc) {
                LogException(LogLevel.Error, exc, filelog: false);
            }
        }

        private static void WriteToLogFile(LogLevel level, Exception e, DateTime? timestamp = null)
        {
            try {
                using (StreamWriter sw = new StreamWriter(_path, true, Encoding.UTF8, BUFFER_SIZE)) {
                    sw.Write("[{0:yyyy-MM-dd HH:mm:ss zzz}] ", timestamp ?? DateTime.Now);
                    sw.WriteLine("[{0}]", level.ToString());
                    sw.WriteLine($"Exception occured: {e.GetType()}");
                    sw.WriteLine($"Details: {e.Message}");
                    if (e.InnerException != null)
                        sw.WriteLine($"Inner exception: {e.InnerException}");
                    sw.WriteLine($"Stack trace: {e.StackTrace}");
                    sw.WriteLine();
                    sw.Flush();
                }
            } catch (Exception exc) {
                LogException(LogLevel.Error, exc, filelog: false);
            }
        }

        private static void PrintLevel(LogLevel level)
        {
            var ccfg = ConsoleColor.Gray;
            var ccbg = ConsoleColor.Black;
            switch (level) {
                case LogLevel.Critical:
                    ccfg = ConsoleColor.Black;
                    ccbg = ConsoleColor.Red;
                    break;
                case LogLevel.Error:
                    ccfg = ConsoleColor.Red;
                    break;
                case LogLevel.Warning:
                    ccfg = ConsoleColor.Yellow;
                    break;
                case LogLevel.Info:
                    ccfg = ConsoleColor.White;
                    break;
                case LogLevel.Debug:
                    ccfg = ConsoleColor.Magenta;
                    break;
            }
            Console.ForegroundColor = ccfg;
            Console.BackgroundColor = ccbg;
            Console.WriteLine("[{0}]", level.ToString());

            Console.ForegroundColor = ConsoleColor.Gray;
            Console.BackgroundColor = ConsoleColor.Black;
        }

        private static void PrintTimestamp(DateTime? timestamp = null)
        {
            Console.BackgroundColor = ConsoleColor.Black;
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write("[{0:yyyy-MM-dd HH:mm:ss zzz}] ", timestamp ?? DateTime.Now);
        }

        private static void PrintLogMessage(string message)
            => Console.WriteLine(message.Replace("<br>", Environment.NewLine).Trim() + Environment.NewLine);
    }
}
