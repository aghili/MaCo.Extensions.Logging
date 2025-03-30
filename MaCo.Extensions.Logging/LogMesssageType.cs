namespace Aghili.Logging;

[Flags]
public enum LogMesssageType
{
    Exception = 1,
    Warrning = 2,
    Information = 4,
    DataLog = 8,
}