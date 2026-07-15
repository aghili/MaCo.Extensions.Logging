namespace MaCo.Extensions.Logging.Classes;

public class LogPayload
{
    public string Type { get; set; } = "event";

    public string TelemetryCategory { get; set; } = "event";

    public string Message { get; set; } = "";

    public string Source { get; set; } = "";

    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}
