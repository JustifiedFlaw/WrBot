using System;
using Serilog.Core;
using Serilog.Events;
 

public class SerilogNHibernateSink : ILogEventSink
{
    ILogService _logService;
    IFormatProvider _formatProvider;

    public SerilogNHibernateSink(ILogService logService, IFormatProvider formatProvider)
    {
        _logService = logService;
        _formatProvider = formatProvider;
    }

    public void Emit(LogEvent logEvent)
    {
        _logService.Add(logEvent.Timestamp, logEvent.RenderMessage(_formatProvider));
    }
}