using Serilog.Core;

namespace TheGodfather.Extensions;

public static class LogExt
{
    public static Logger CreateLogger(BotConfig cfg)
    {
        var logger = new LoggerConfiguration()
            .MinimumLevel.Is(cfg.LogLevel)
            .WriteTo.Console();

        if (cfg.LogToFile) {
            logger.WriteTo.File(
                cfg.LogPath,
                rollingInterval: cfg.RollingInterval,
                retainedFileCountLimit: cfg.MaxLogFiles,
                buffered: cfg.UseBufferedFileLogger
            );
        }

        return logger.CreateLogger();
    }
}
