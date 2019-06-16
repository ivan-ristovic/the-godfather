#region USING_DIRECTIVES
using DSharpPlus;
using DSharpPlus.EventArgs;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
#endregion

namespace TheGodfather.Common
{
    public sealed class Logger
    {
        private static readonly string _separator = "|> ";

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
        private readonly List<SpecialLoggingRule> specialRules;
        private readonly string path;
        private readonly object writeLock;


        public Logger(BotConfig cfg)
        {
            this.writeLock = new object();
            this.BufferSize = 512;
            this.LogLevel = cfg.LogLevel;
            this.LogToFile = cfg.LogToFile;
            this.path = cfg.LogPath ?? "gf_log.txt";
            this.specialRules = new List<SpecialLoggingRule>();
            TaskScheduler.UnobservedTaskException += this.Log;
        }


        public void ApplySpecialLoggingRule(SpecialLoggingRule rule)
        {
            this.specialRules.Add(rule);
        }

        public bool ClearLog()
        {
            lock (this.writeLock) {
                Console.Clear();
                try {
                    File.Delete(this.path);
                } catch (Exception e) {
                    this.Log(LogLevel.Error, e);
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
                PrintLogMessage();
                if (filelog && this.filelog)
                    this.WriteToLogFile(level, message, timestamp);
            }
        }

        public void Log(LogLevel level, Exception e, DateTime? timestamp = null, bool filelog = true)
        {
            if (level > this.LogLevel)
                return;

            lock (this.writeLock) {
                PrintTimestamp(timestamp);
                PrintLevel(level);
                PrintLogMessage($"Exception occured: {e.GetType()}");
                PrintLogMessage($"Details: {e.Message}");
                if (!(e.InnerException is null))
                    PrintLogMessage($"Inner exception: {e.InnerException}");
                PrintLogMessage($"Stack trace:\n{e.StackTrace}");
                PrintLogMessage();
                if (filelog && this.filelog)
                    this.WriteToLogFile(level, e);
            }
        }

        public void Log(int shard, DebugLogMessageEventArgs e, bool filelog = true)
        {
            if (e.Level > this.LogLevel)
                return;

            if (this.specialRules.Any(r => r.Name == e.Application && r.MinLevel < e.Level))
                return;

            lock (this.writeLock) {
                PrintTimestamp(e.Timestamp);
                PrintApplicationInfo(shard, e.Application);
                PrintLevel(e.Level);
                PrintLogMessage(e.Message);
                PrintLogMessage();
                if (filelog && this.filelog)
                    this.WriteToLogFile(shard, e);
            }
        }

        public void Log(LogLevel level, string message, int? shard = null, DateTime? timestamp = null, bool filelog = true)
        {
            if (level > this.LogLevel)
                return;

            this.ElevatedLog(level, message, shard, timestamp, filelog);
        }

        public void LogMany(LogLevel level, int shard, DateTime timestamp, bool filelog, params string[] messages)
        {
            if (level > this.LogLevel)
                return;

            lock (this.writeLock) {
                PrintTimestamp(timestamp);
                PrintApplicationInfo(shard, null);
                PrintLevel(level);
                foreach (string message in messages.Where(m => !string.IsNullOrWhiteSpace(m)))
                    PrintLogMessage(message);
                PrintLogMessage();
                if (filelog && this.filelog)
                    this.WriteManyToLogFile(level, timestamp, messages);
            }
        }


        private void Log(object sender, UnobservedTaskExceptionEventArgs e)
        {
            this.Log(LogLevel.Error, e.Exception);
        }


        #region FILE_LOGGING
        private void WriteToLogFile(LogLevel level, string message, DateTime? timestamp = null)
        {
            try {
                using (var sw = new StreamWriter(this.path, true, Encoding.UTF8, this.BufferSize)) {
                    sw.WriteLine($"[{(timestamp ?? DateTime.Now):yyyy-MM-dd HH:mm:ss zzz}] [{level}]");
                    sw.WriteLine($"{_separator}{message.Trim()}");
                    sw.WriteLine();
                    sw.Flush();
                }
            } catch (Exception e) {
                this.Log(LogLevel.Error, e, filelog: false);
            }
        }

        private void WriteToLogFile(int shard, DebugLogMessageEventArgs e)
        {
            try {
                using (var sw = new StreamWriter(this.path, true, Encoding.UTF8, this.BufferSize)) {
                    sw.WriteLine($"[{e.Timestamp:yyyy-MM-dd HH:mm:ss zzz}] [#{shard}] [{e.Application}] [{e.Level}]");
                    sw.WriteLine($"{_separator}{e.Message.Trim()}");
                    sw.WriteLine();
                    sw.Flush();
                }
            } catch (Exception exc) {
                this.Log(LogLevel.Error, exc, filelog: false);
            }
        }

        private void WriteToLogFile(LogLevel level, Exception e, DateTime? timestamp = null)
        {
            try {
                using (var sw = new StreamWriter(this.path, true, Encoding.UTF8, this.BufferSize)) {
                    sw.WriteLine($"[{(timestamp ?? DateTime.Now):yyyy-MM-dd HH:mm:ss zzz}] [{level}]");
                    sw.WriteLine($"{_separator}Exception occured: {e.GetType()}");
                    sw.WriteLine($"{_separator}Details: {e.Message}");
                    if (!(e.InnerException is null))
                        sw.WriteLine($"{_separator}Inner exception: {e.InnerException}");
                    sw.WriteLine($"{_separator}Stack trace: {e.StackTrace}");
                    sw.WriteLine();
                    sw.Flush();
                }
            } catch (Exception exc) {
                this.Log(LogLevel.Error, exc, filelog: false);
            }
        }

        private void WriteManyToLogFile(LogLevel level, DateTime? timestamp, params string[] messages)
        {
            try {
                using (var sw = new StreamWriter(this.path, true, Encoding.UTF8, this.BufferSize)) {
                    sw.WriteLine($"[{(timestamp ?? DateTime.Now):yyyy-MM-dd HH:mm:ss zzz}] [{level}]");
                    foreach (string message in messages.Where(m => !string.IsNullOrWhiteSpace(m)))
                        sw.WriteLine($"{_separator}{message.Trim()}");
                    sw.WriteLine();
                    sw.Flush();
                }
            } catch (Exception e) {
                this.Log(LogLevel.Error, e, filelog: false);
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

        private static void PrintLogMessage(string message = "")
        {
            if (!(message is null))
                Console.WriteLine(string.IsNullOrWhiteSpace(message) ? "" : $"{_separator}{message.Trim()}");
        }
        #endregion


        public sealed class SpecialLoggingRule
        {
            [JsonProperty("app")]
            public string Name { get; set; }

            [JsonProperty("level")]
            public LogLevel MinLevel { get; set; }
        }
    }
}
