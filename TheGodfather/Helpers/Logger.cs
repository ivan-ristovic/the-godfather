#region USING_DIRECTIVES
using System;
using System.IO;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using Newtonsoft.Json;

using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Converters;
using DSharpPlus.CommandsNext.Entities;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
#endregion

namespace TheGodfather.Helpers
{
    public class Logger
    {
        private bool _valid { get; set; }
        private readonly string LOG_TAG = "TheGodfather";
        private readonly object _lock = new object();
        private StreamWriter _logstream = null;
        private DebugLogger _log = null;


        public Logger(DebugLogger logger)
        {
            _log = logger;
            OpenLogFile();
        }


        public void Log(LogLevel level, string message)
        {
            _log.LogMessage(level, LOG_TAG, message, DateTime.Now);
        }
        
        public void OpenLogFile()
        {
            try {
                _logstream = new StreamWriter("log.txt", append: true);
            } catch (Exception e) {
                _log.LogMessage(LogLevel.Error, LOG_TAG, "Cannot open log file. Details: " + e.Message, DateTime.Now);
                _valid = false;
            }

            try {
                lock (_lock) {
                    _logstream.WriteLine($"{Environment.NewLine}*** NEW INSTANCE STARTED AT {DateTime.Now.ToLongDateString()} : {DateTime.Now.ToLongTimeString()} ***{Environment.NewLine}");
                    _logstream.Flush();
                }
            } catch (Exception e) {
                _log?.LogMessage(LogLevel.Error, LOG_TAG, "Cannot write to log file. Details: " + e.Message, DateTime.Now);
                _valid = false;
            }

            _valid = true;
        }

        public void CloseLogFile()
        {
            if (_logstream != null) {
                _logstream.Close();
                _logstream = null;
            }
            _valid = false;
        }

        public void ClearLogFile()
        {
            CloseLogFile();
            File.Delete("log.txt");
            OpenLogFile();
        }

        public void WriteToFile(DebugLogMessageEventArgs e)
        {
            if (_valid)
                return;

            try {
                lock (_lock) {
                    _logstream.WriteLine($"[{e.Timestamp}] [{e.Level}]{Environment.NewLine}{e.Message}");
                    _logstream.Flush();
                }
            } catch (Exception ex) {
                Console.WriteLine("Cannot write to log file. Details: " + ex.GetType() + " : " + ex.Message);
            }
        }
    }
}
