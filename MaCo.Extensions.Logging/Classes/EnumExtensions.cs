namespace Aghili.Logging.Classes;

public static class EnumExtensions
{
    public static bool HasFlag(this Enum variable, Enum value)
    {
        var num = (variable.GetType() == value.GetType()) ? Convert.ToUInt64(value) : throw new ArgumentException("The checked flag is not from the same type as the checked variable.");
        return (Convert.ToUInt64(variable) & num) == num;
    }
}