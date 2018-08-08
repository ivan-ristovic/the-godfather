#region USING_DIRECTIVES
using DSharpPlus;
using DSharpPlus.EventArgs;

using System;
using System.IO;
using System.Text;
#endregion

namespace TheGodfather.Common
{
    public class Logger
    {
        public int BufferSize { get; set; }
        public LogLevel LogLevel { get; set; }
        public bool LogToFile {
            get => this.filelog;
            set {
                lock (this.writeLock)
                    this.filelog = value;
            }
        }

        public readonly string path;
        private bool filelog;
        private readonly object writeLock;


        public Logger(BotConfig cfg)
        {
            this.writeLock = new object();
            this.BufferSize = 512;
            this.LogLevel = cfg.LogLevel;
            this.LogToFile = cfg.LogToFile;
            this.path = cfg.LogPath ?? "gf_log.txt";
        }


        public bool Clear()
        {
            lock (this.writeLock) {
                Console.Clear();
                try {
                    File.Delete(this.path);
                } catch (Exception e) {
                    LogException(LogLevel.Error, e);
                    return false;
                }
            }
            return true;
        }

        public void ElevatedLog(LogLevel level, string message, int? shard = null, DateTime? timestamp = null, bool filelog = true)
        {
            lock (this.writeLock) {
                PrintTimestamp(timestamp);
                PrintApplicationInfo(shard, null);
                PrintLevel(level);
                PrintLogMessage(message);
                if (filelog && this.filelog)
                    WriteToLogFile(level, message, timestamp);
            }
        }

        public void LogException(LogLevel level, Exception e, DateTime? timestamp = null, bool filelog = true)
        {
            if (level > this.LogLevel)
                return;

            lock (this.writeLock) {
                PrintTimestamp(timestamp);
                PrintLevel(level);
                PrintLogMessage($"| Exception occured: {e.GetType()}\n| Details: {e.Message}");
                if (e.InnerException != null)
                    PrintLogMessage($"| Inner exception: {e.InnerException}");
                PrintLogMessage($"| Stack trace:\n{e.StackTrace}");
                if (filelog && this.filelog)
                    WriteToLogFile(level, e);
            }
        }

        public void LogMessage(int shard, DebugLogMessageEventArgs e, bool filelog = true)
        {
            if (e.Level > this.LogLevel)
                return;

            lock (this.writeLock) {
                PrintTimestamp(e.Timestamp);
                PrintApplicationInfo(shard, e.Application);
                PrintLevel(e.Level);
                PrintLogMessage(e.Message);
                if (filelog && this.filelog)
                    WriteToLogFile(shard, e);
            }
        }

        public void LogMessage(LogLevel level, string message, int? shard = null, DateTime? timestamp = null, bool filelog = true)
        {
            if (level > this.LogLevel)
                return;

            ElevatedLog(level, message, shard, timestamp, filelog);
        }


        #region FILE_LOGGING
        private void WriteToLogFile(LogLevel level, string message, DateTime? timestamp = null)
        {
            try {
                using (var sw = new StreamWriter(this.path, true, Encoding.UTF8, this.BufferSize)) {
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
                using (var sw = new StreamWriter(this.path, true, Encoding.UTF8, this.BufferSize)) {
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
                using (var sw = new StreamWriter(this.path, true, Encoding.UTF8, this.BufferSize)) {
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
        #endregion

        #region CONSOLE_PRINT_HELPERS
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
            => Console.WriteLine(message.Trim());
        #endregion
    }
}
