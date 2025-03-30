namespace Aghili.Logging.Classes;

public class WriteOption : IWriterOption
{
    public int LogKeepDataOnLimitRichedPercent { get; set; } = 80;

    public int LogRowLimitPerContainer { get; set; } = 20;
}