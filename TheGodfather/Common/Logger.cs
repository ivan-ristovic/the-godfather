#region USING_DIRECTIVES
using System;
using System.IO;
using System.Text;

using DSharpPlus;
using DSharpPlus.EventArgs;
#endregion

namespace TheGodfather.Common
{
    public class Logger
    {
        public bool LogToFile {
            get => _filelog;
            set {
                lock (_lock)
                    _filelog = value;
            }
        }
        public LogLevel LogLevel { get; set; } = LogLevel.Debug;
        public string Path
        {
            get => _path;
            set {
                if (!string.IsNullOrWhiteSpace(value))
                    _path = value;
            }
        }
        public int BufferSize { get; set; } = 512;
        
        private readonly object _lock = new object();
        private bool _filelog = true;
        private string _path = "gf_log.txt";


        public Logger(BotConfig cfg)
        {
            LogLevel = cfg.LogLevel;
            LogToFile = cfg.LogToFile;
            Path = cfg.LogPath;
        }


        public bool Clear()
        {
            lock (_lock) {
                Console.Clear();
                try {
                    File.Delete(_path);
                } catch (Exception e) {
                    LogException(LogLevel.Error, e);
                    return false;
                }
            }
            return true;
        }

        public void ElevatedLog(LogLevel level, string message, int? shard = null, DateTime? timestamp = null, bool filelog = true)
        {
            lock (_lock) {
                PrintTimestamp(timestamp);
                PrintApplicationInfo(shard, null);
                PrintLevel(level);
                PrintLogMessage(message);
                if (filelog && _filelog)
                    WriteToLogFile(level, message, timestamp);
            }
        }

        public void LogMessage(LogLevel level, string message, int? shard = null, DateTime? timestamp = null, bool filelog = true)
        {
            if (level > LogLevel)
                return;

            ElevatedLog(level, message, shard, timestamp, filelog);
        }

        public void LogMessage(int shard, DebugLogMessageEventArgs e, bool filelog = true)
        {
            if (e.Level > LogLevel)
                return;

            lock (_lock) {
                PrintTimestamp(e.Timestamp);
                PrintApplicationInfo(shard, e.Application);
                PrintLevel(e.Level);
                PrintLogMessage(e.Message);
                if (filelog && _filelog)
                    WriteToLogFile(shard, e);
            }
        }

        public void LogException(LogLevel level, Exception e, DateTime? timestamp = null, bool filelog = true)
        {
            if (level > LogLevel)
                return;

            lock (_lock) {
                PrintTimestamp(timestamp);
                PrintLevel(level);
                PrintLogMessage($"| Exception occured: {e.GetType()}\n| Details: {e.Message}\n");
                if (e.InnerException != null)
                    PrintLogMessage($"| Inner exception: {e.InnerException}\n");
                PrintLogMessage($"| Stack trace:\n{e.StackTrace}");
                if (filelog && _filelog)
                    WriteToLogFile(level, e);
            }
        }


        private void WriteToLogFile(LogLevel level, string message, DateTime? timestamp = null)
        {
            try {
                using (StreamWriter sw = new StreamWriter(_path, true, Encoding.UTF8, BufferSize)) {
                    sw.WriteLine($"[{(timestamp ?? DateTime.Now):yyyy-MM-dd HH:mm:ss zzz}] [{level}]");
                    sw.WriteLine(message.Trim().Replace("\n", Environment.NewLine));
                    sw.WriteLine();
                    sw.Flush();
                }
            } catch (Exception e) {
                LogException(LogLevel.Error, e, filelog: false);
            }
        }

        private void WriteToLogFile(int shard, DebugLogMessageEventArgs e)
        {
            try {
                using (var sw = new StreamWriter(_path, true, Encoding.UTF8, BufferSize)) {
                    sw.WriteLine($"[{e.Timestamp:yyyy-MM-dd HH:mm:ss zzz}] [#{shard}] [{e.Application}] [{e.Level}]");
                    sw.WriteLine(e.Message.Trim().Replace("\n", Environment.NewLine));
                    sw.WriteLine();
                    sw.Flush();
                }
            } catch (Exception exc) {
                LogException(LogLevel.Error, exc, filelog: false);
            }
        }

        private void WriteToLogFile(LogLevel level, Exception e, DateTime? timestamp = null)
        {
            try {
                using (var sw = new StreamWriter(_path, true, Encoding.UTF8, BufferSize)) {
                    sw.WriteLine($"[{(timestamp ?? DateTime.Now):yyyy-MM-dd HH:mm:ss zzz}] [{level}]");
                    sw.WriteLine($"| Exception occured: {e.GetType()}");
                    sw.WriteLine($"| Details: {e.Message}");
                    if (e.InnerException != null)
                        sw.WriteLine($"| Inner exception: {e.InnerException}");
                    sw.WriteLine($"| Stack trace: {e.StackTrace}");
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
            Console.WriteLine($"[{level}]");

            Console.ForegroundColor = ConsoleColor.Gray;
            Console.BackgroundColor = ConsoleColor.Black;
        }

        private static void PrintTimestamp(DateTime? timestamp = null)
        {
            Console.BackgroundColor = ConsoleColor.Black;
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write($"[{(timestamp ?? DateTime.Now):yyyy-MM-dd HH:mm:ss zzz}] ");
        }

        private static void PrintApplicationInfo(int? shard, string application)
        {
            Console.ForegroundColor = ConsoleColor.DarkGreen;
            Console.Write(shard.HasValue ? $"[#{shard.Value}] " : "[Main] ");
            Console.ForegroundColor = ConsoleColor.White;
            if (string.IsNullOrWhiteSpace(application))
                Console.Write("[TheGodfather] ");
            else
                Console.Write($"[{application}] ");
        }

        private static void PrintLogMessage(string message)
            => Console.WriteLine(message.Trim() + Environment.NewLine);
    }
}
