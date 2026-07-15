using System.Text.RegularExpressions;

namespace MaCo.Extensions.Logging.Classes;

public class Utilites
{
    private static readonly Regex InvalidPathCharsRegex = new(
        "[" + Regex.Escape(new string(Path.GetInvalidFileNameChars()) + new string(Path.GetInvalidPathChars())) + "]",
        RegexOptions.Compiled);

    public static string RemoveIligalPathChars(string v) => InvalidPathCharsRegex.Replace(v ?? string.Empty, "");
}