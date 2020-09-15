using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using DSharpPlus.CommandsNext;
using Serilog;
using Serilog.Context;
using Serilog.Core;
using Serilog.Events;
using TheGodfather.Services.Common;

namespace TheGodfather.Extensions
{
    internal static class LogExt
    {
        public static Logger CreateLogger(BotConfig cfg)
        {
            string template = "[{Timestamp:yyyy-MM-dd HH:mm:ss zzz}] [{Application}] [{Level:u3}] [T{ThreadId:d2}] ({ShardId}) {Message:l}{NewLine}{Exception}";

            LoggerConfiguration lcfg = new LoggerConfiguration()
                .Enrich.FromLogContext()
                .Enrich.With<Enrichers.ThreadIdEnricher>()
                .Enrich.With<Enrichers.ShardIdEnricher>()
                .Enrich.With<Enrichers.ApplicationNameEnricher>()
                .MinimumLevel.Is(cfg.LogLevel)
                .WriteTo.Console(outputTemplate: template)
                ;

            if (cfg.LogToFile) {
                lcfg = lcfg.WriteTo.File(
                    cfg.LogPath,
                    cfg.LogLevel,
                    outputTemplate: template,
                    rollingInterval: RollingInterval.Day
                );
            }

            foreach (BotConfig.SpecialLoggingRule rule in cfg.SpecialLoggerRules) {
                lcfg.Filter.ByExcluding(e => {
                    string app = (e.Properties.GetValueOrDefault("Application") as ScalarValue)?.Value as string ?? "UnknownApplication";
                    return app == rule.Application && e.Level < rule.MinLevel;
                });
            }

            return lcfg.CreateLogger();
        }

        public static void Verbose(int shardId, string template, params object[] propertyValues)
            => InternalLog(LogEventLevel.Verbose, shardId, template, null, propertyValues);

        public static void Verbose(int shardId, Exception ex, string template, params object[] propertyValues)
            => InternalLog(LogEventLevel.Verbose, shardId, template, ex, propertyValues);

        public static void Verbose(int shardId, Exception ex, string[] templates, params object[] propertyValues)
            => InternalLogMany(LogEventLevel.Verbose, shardId, templates, ex, propertyValues);

        public static void Verbose(int shardId, string[] templates, params object[] propertyValues)
            => InternalLogMany(LogEventLevel.Verbose, shardId, templates, propertyValues: propertyValues);

        public static void Verbose(CommandContext ctx, string template, params object[] propertyValues)
            => InternalLogContext(LogEventLevel.Verbose, ctx, null, template, propertyValues);

        public static void Verbose(CommandContext ctx, Exception ex, string template, params object[] propertyValues)
            => InternalLogContext(LogEventLevel.Verbose, ctx, ex, template, propertyValues);

        public static void Debug(int shardId, string template, params object[] propertyValues)
            => InternalLog(LogEventLevel.Debug, shardId, template, null, propertyValues);

        public static void Debug(int shardId, Exception ex, string template, params object[] propertyValues)
            => InternalLog(LogEventLevel.Debug, shardId, template, ex, propertyValues);

        public static void Debug(int shardId, Exception ex, string[] templates, params object[] propertyValues)
            => InternalLogMany(LogEventLevel.Debug, shardId, templates, ex, propertyValues);

        public static void Debug(int shardId, string[] templates, params object[] propertyValues)
            => InternalLogMany(LogEventLevel.Debug, shardId, templates, propertyValues: propertyValues);

        public static void Debug(CommandContext ctx, string template, params object[] propertyValues)
            => InternalLogContext(LogEventLevel.Debug, ctx, null, template, propertyValues);

        public static void Debug(CommandContext ctx, Exception ex, string template, params object[] propertyValues)
            => InternalLogContext(LogEventLevel.Debug, ctx, ex, template, propertyValues);

        public static void Information(int shardId, string template, params object[] propertyValues)
            => InternalLog(LogEventLevel.Information, shardId, template, null, propertyValues);

        public static void Information(int shardId, Exception ex, string template, params object[] propertyValues)
            => InternalLog(LogEventLevel.Information, shardId, template, ex, propertyValues);

        public static void Information(int shardId, string[] templates, params object[] propertyValues)
            => InternalLogMany(LogEventLevel.Information, shardId, templates, propertyValues: propertyValues);

        public static void Information(int shardId, Exception ex, string[] templates, params object[] propertyValues)
            => InternalLogMany(LogEventLevel.Information, shardId, templates, ex, propertyValues);

        public static void Information(CommandContext ctx, string template, params object[] propertyValues)
            => InternalLogContext(LogEventLevel.Information, ctx, null, template, propertyValues);

        public static void Information(CommandContext ctx, Exception ex, string templates, params object[] propertyValues)
            => InternalLogContext(LogEventLevel.Information, ctx, ex, templates, propertyValues);

        public static void Warning(int shardId, string template, params object[] propertyValues)
            => InternalLog(LogEventLevel.Warning, shardId, template, null, propertyValues);

        public static void Warning(int shardId, Exception ex, string template, params object[] propertyValues)
            => InternalLog(LogEventLevel.Warning, shardId, template, ex, propertyValues);

        public static void Warning(int shardId, string[] templates, params object[] propertyValues)
            => InternalLogMany(LogEventLevel.Warning, shardId, templates, propertyValues: propertyValues);

        public static void Warning(int shardId, Exception ex, string[] templates, params object[] propertyValues)
            => InternalLogMany(LogEventLevel.Warning, shardId, templates, ex, propertyValues);

        public static void Warning(CommandContext ctx, string template, params object[] propertyValues)
            => InternalLogContext(LogEventLevel.Warning, ctx, null, template, propertyValues);

        public static void Warning(CommandContext ctx, Exception ex, string template, params object[] propertyValues)
            => InternalLogContext(LogEventLevel.Warning, ctx, ex, template, propertyValues);

        public static void Error(int shardId, string template, params object[] propertyValues)
            => InternalLog(LogEventLevel.Error, shardId, template, null, propertyValues);

        public static void Error(int shardId, Exception ex, string template, params object[] propertyValues)
            => InternalLog(LogEventLevel.Error, shardId, template, ex, propertyValues);

        public static void Error(int shardId, string[] templates, params object[] propertyValues)
            => InternalLogMany(LogEventLevel.Error, shardId, templates, propertyValues: propertyValues);

        public static void Error(int shardId, Exception ex, string[] templates, params object[] propertyValues)
            => InternalLogMany(LogEventLevel.Error, shardId, templates, ex, propertyValues);

        public static void Error(CommandContext ctx, string template, params object[] propertyValues)
            => InternalLogContext(LogEventLevel.Error, ctx, null, template, propertyValues);

        public static void Error(CommandContext ctx, Exception ex, string template, params object[] propertyValues)
            => InternalLogContext(LogEventLevel.Error, ctx, ex, template, propertyValues);

        public static void Fatal(int shardId, string template, params object[] propertyValues)
            => InternalLog(LogEventLevel.Fatal, shardId, template, null, propertyValues);

        public static void Fatal(int shardId, Exception ex, string template, params object[] propertyValues)
            => InternalLog(LogEventLevel.Fatal, shardId, template, ex, propertyValues);

        public static void Fatal(int shardId, string[] templates, params object[] propertyValues)
            => InternalLogMany(LogEventLevel.Fatal, shardId, templates, propertyValues: propertyValues);

        public static void Fatal(int shardId, Exception ex, string[] templates, params object[] propertyValues)
            => InternalLogMany(LogEventLevel.Fatal, shardId, templates, ex, propertyValues);

        public static void Fatal(CommandContext ctx, string template, params object[] propertyValues)
            => InternalLogContext(LogEventLevel.Fatal, ctx, null, template, propertyValues);

        public static void Fatal(CommandContext ctx, Exception ex, string template, params object[] propertyValues)
            => InternalLogContext(LogEventLevel.Fatal, ctx, ex, template, propertyValues);


        private static void InternalLogContext(LogEventLevel level, CommandContext ctx, Exception? ex, string template, params object[] propertyValues)
        {
            object[] allPropertyValues = ctx.Guild is { }
                ? new object[] { ctx.User, ctx.Guild, ctx.Channel }
                : new object[] { ctx.User, ctx.Channel };
            if (propertyValues?.Any() ?? false)
                allPropertyValues = propertyValues.Concat(allPropertyValues).ToArray();
            InternalLogMany(level, ctx.Client.ShardId, new[] { template, "{User}", "{Guild}", "{Channel}" }, ex, allPropertyValues);
        }

        private static void InternalLog(LogEventLevel level, int shardId, string template, Exception? ex = null, object[]? propertyValues = null)
        {
#pragma warning disable Serilog004 // Constant MessageTemplate verifier
            using (LogContext.PushProperty("ShardId", shardId))
                Log.Write(level, ex, template, propertyValues);
#pragma warning restore Serilog004 // Constant MessageTemplate verifier
        }

        private static void InternalLogMany(LogEventLevel level, int shardId, string[] templates, Exception? ex = null, object[]? propertyValues = null)
        {
#pragma warning disable Serilog004 // Constant MessageTemplate verifier
            using (LogContext.PushProperty("ShardId", shardId))
                Log.Write(level, ex, string.Join(Environment.NewLine, templates), propertyValues);
#pragma warning restore Serilog004 // Constant MessageTemplate verifier
        }
    }

    internal sealed class Enrichers
    {
        public sealed class ThreadIdEnricher : ILogEventEnricher
        {
            public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
                => logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("ThreadId", Thread.CurrentThread.ManagedThreadId));
        }

        public sealed class ShardIdEnricher : ILogEventEnricher
        {
            public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
                => logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("ShardId", "M"));
        }

        public sealed class ApplicationNameEnricher : ILogEventEnricher
        {
            public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
                => logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("Application", TheGodfather.ApplicationName));
        }
    }
}
