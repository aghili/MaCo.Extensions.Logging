using System.Text.Json.Serialization;

namespace MaCo.Extensions.Logging;

public partial class Log
{
    public class LogSettings
    {
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public LogMessageType MessageTypes { set; get; } = LogMessageType.Exception | LogMessageType.Warning;
        public string Info { set; get; } = "Informations :\nEnable = 'Type = Boolean| set to true for logging program information and exceptions.'\nMessageTypes = 'Type = Enum(Flags) | Exception=1,Warning = 2,Information = 4,DataLog = 8'";

        public bool Enabled { get; set; } = true;

        public LogType LogType { set; get; } = LogType.File;

        public int LogKeepDataOnLimitRichedPercent { get; set; } = 80;

        public int LogRowLimitPerContainer { get; set; } = 10000;

    public OnlineLoggerSettings Online { get; set; } = new OnlineLoggerSettings();
}

public class OnlineLoggerSettings
{
    public bool Enabled { get; set; } = false;

    public string ApiEndpoint { get; set; } = "";

    public string ApiKey { get; set; } = "";

    public int BatchSize { get; set; } = 50;

    public int UploadIntervalSeconds { get; set; } = 15;
}

}
