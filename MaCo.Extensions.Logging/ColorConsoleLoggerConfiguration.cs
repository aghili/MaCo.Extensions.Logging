using Aghili.Logging;
using Microsoft.Extensions.Logging;
using System.Text.Json.Serialization;

public sealed class MaCoLoggerConfiguration
{
    public int EventId { get; set; }
    public List<LogLevel> LogLevels { get; set; } = [LogLevel.Information];

    public LogType LogType { set; get; } = LogType.File;

    public int LogKeepDataOnLimitRichedPercent { get; set; } = 80;

    public int LogRowLimitPerContainer { get; set; } = 10000;

}