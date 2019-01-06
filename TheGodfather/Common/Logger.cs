#region USING_DIRECTIVES
using DSharpPlus;
using DSharpPlus.EventArgs;

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
#endregion

namespace TheGodfather.Common
{
    public sealed class Logger
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

        private bool filelog;
        private readonly List<string> ignoredApplications;
        private readonly string path;
        private readonly object writeLock;


        public Logger(BotConfig cfg)
        {
            this.writeLock = new object();
            this.BufferSize = 512;
            this.LogLevel = cfg.LogLevel;
            this.LogToFile = cfg.LogToFile;
            this.path = cfg.LogPath ?? "gf_log.txt";
            this.ignoredApplications = new List<string>();
            TaskScheduler.UnobservedTaskException += this.LogUnobservedTaskException;
        }


        public void IgnoreApplication(string app)
        {
            this.ignoredApplications.Add(app);
        }

        public bool Clear()
        {
            lock (this.writeLock) {
                Console.Clear();
                try {
                    File.Delete(this.path);
                } catch (Exception e) {
                    this.LogException(LogLevel.Error, e);
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
                    this.WriteToLogFile(level, message, timestamp);
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
                if (!(e.InnerException is null))
                    PrintLogMessage($"| Inner exception: {e.InnerException}");
                PrintLogMessage($"| Stack trace:\n{e.StackTrace}");
                if (filelog && this.filelog)
                    this.WriteToLogFile(level, e);
            }
        }

        public void LogMessage(int shard, DebugLogMessageEventArgs e, bool filelog = true)
        {
            if (e.Level > this.LogLevel)
                return;

            if (this.ignoredApplications.Contains(e.Application))
                return;

            lock (this.writeLock) {
                PrintTimestamp(e.Timestamp);
                PrintApplicationInfo(shard, e.Application);
                PrintLevel(e.Level);
                PrintLogMessage(e.Message);
                if (filelog && this.filelog)
                    this.WriteToLogFile(shard, e);
            }
        }

        public void LogMessage(LogLevel level, string message, int? shard = null, DateTime? timestamp = null, bool filelog = true)
        {
            if (level > this.LogLevel)
                return;

            this.ElevatedLog(level, message, shard, timestamp, filelog);
        }


        private void LogUnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs e)
        {
            this.LogException(LogLevel.Error, e.Exception);
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
                this.LogException(LogLevel.Error, e, filelog: false);
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
                this.LogException(LogLevel.Error, exc, filelog: false);
            }
        }

        private void WriteToLogFile(LogLevel level, Exception e, DateTime? timestamp = null)
        {
            try {
                using (var sw = new StreamWriter(this.path, true, Encoding.UTF8, this.BufferSize)) {
                    sw.WriteLine($"[{(timestamp ?? DateTime.Now):yyyy-MM-dd HH:mm:ss zzz}] [{level}]");
                    sw.WriteLine($"| Exception occured: {e.GetType()}");
                    sw.WriteLine($"| Details: {e.Message}");
                    if (!(e.InnerException is null))
                        sw.WriteLine($"| Inner exception: {e.InnerException}");
                    sw.WriteLine($"| Stack trace: {e.StackTrace}");
                    sw.WriteLine();
                    sw.Flush();
                }
            } catch (Exception exc) {
                this.LogException(LogLevel.Error, exc, filelog: false);
            }
        }
        #endregion

        #region CONSOLE_PRINT_HELPERS
        private static void PrintLevel(LogLevel level)
        {
            switch (level) {
                case LogLevel.Critical:
                    Console.ForegroundColor = ConsoleColor.Black;
                    Console.BackgroundColor = ConsoleColor.Red;
                    break;
                case LogLevel.Error:
                    Console.ForegroundColor = ConsoleColor.Red;
                    break;
                case LogLevel.Warning:
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    break;
                case LogLevel.Info:
                    break;
                case LogLevel.Debug:
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    break;
            }
            Console.WriteLine($"[{level}]");

            Console.ResetColor();
        }

        private static void PrintTimestamp(DateTime? timestamp = null)
        {
            Console.Write($"[{(timestamp ?? DateTime.Now):yyyy-MM-dd HH:mm:ss zzz}] ");
        }

        private static void PrintApplicationInfo(int? shard, string application)
        {
            Console.ForegroundColor = ConsoleColor.DarkGreen;
            Console.Write(shard.HasValue ? $"[#{shard.Value}] " : "[Main] ");
            Console.ResetColor();
            if (string.IsNullOrWhiteSpace(application))
                Console.Write($"[{TheGodfather.ApplicationName}] ");
            else
                Console.Write($"[{application}] ");
        }

        private static void PrintLogMessage(string message)
        {
            Console.WriteLine(message.Trim());
            Console.WriteLine();
        }
        #endregion
    }
}
