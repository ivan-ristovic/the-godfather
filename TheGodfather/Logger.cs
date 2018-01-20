using DSharpPlus;
using DSharpPlus.EventArgs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheGodfather
{
    public static class Logger
    {
        private static object _lock = new object();

        public static void LogMessage(int shardid, DebugLogMessageEventArgs e)
        {
            lock (_lock) {
                Console.BackgroundColor = ConsoleColor.Black;

                Console.ForegroundColor = ConsoleColor.White;
                Console.Write("[{0:yyyy-MM-dd HH:mm:ss zzz}] ", e.Timestamp);

                Console.ForegroundColor = ConsoleColor.DarkGreen;
                Console.Write("[#{0}] ", shardid.ToString());

                Console.ForegroundColor = ConsoleColor.White;
                Console.Write("[{0}] ", e.Application);

                var ccfg = ConsoleColor.Gray;
                var ccbg = ConsoleColor.Black;
                switch (e.Level) {
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
                Console.WriteLine("[{0}]", e.Level.ToString());

                Console.ForegroundColor = ConsoleColor.Gray;
                Console.BackgroundColor = ConsoleColor.Black;
                Console.Write(" ");
                Console.WriteLine(e.Message.Replace("<br>", Environment.NewLine + " ").Trim());
            }
        }
    }
}
