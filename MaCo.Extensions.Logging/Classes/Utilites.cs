using System.Text.RegularExpressions;

namespace Aghili.Logging.Classes;

public class Utilites
{
    public static string RemoveIligalPathChars(string v) => new Regex(string.Format("[" + Regex.Escape(new string(Path.GetInvalidFileNameChars()) + new string(Path.GetInvalidPathChars())) + "]")).Replace(v, "");
}