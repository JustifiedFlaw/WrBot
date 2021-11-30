using System;
using Serilog;
using Serilog.Configuration;
using Serilog.Events;

public static class SerilogNHibernateSinkExtension
{
    public static LoggerConfiguration NHibernateSink(
                this LoggerSinkConfiguration loggerConfiguration,
                ILogService logService,
                LogEventLevel restrictedToMinimumLevel,
                IFormatProvider fmtProvider = null)
    {
        return loggerConfiguration.Sink(new SerilogNHibernateSink(logService, fmtProvider), restrictedToMinimumLevel);
    }
}