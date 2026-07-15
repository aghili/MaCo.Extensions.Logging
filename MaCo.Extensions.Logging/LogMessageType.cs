namespace MaCo.Extensions.Logging;

[Flags]
public enum LogMessageType
{
    Exception = 1,
    Warning = 2,
    Information = 4,
    DataLog = 8,
}