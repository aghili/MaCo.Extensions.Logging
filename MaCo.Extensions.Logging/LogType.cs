namespace Aghili.Logging;

[Flags]
public enum LogType
{
    File = 1,
    WindowsLogEvent = 2,
    Online = 4
}